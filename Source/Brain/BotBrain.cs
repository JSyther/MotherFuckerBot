using MFBot.Languages;

namespace MFBot.Brain;

/// <summary>
/// Botun beyni. Öğrenmeyle ilgili her şey buradan geçer:
/// dil başına Markov zinciri, kelime dağarcığı, öğretilen kalıplar, kullanıcı profilleri.
///
/// Üç dil ayrı zincirlerde tutulur. Kin, ruh hâli ve kullanıcı profili ise ORTAK —
/// bot sana Türkçe küfrettikten sonra İngilizceye geçince kinini unutmaz.
/// </summary>
public sealed class BotBrain
{
    private readonly ContentGuard _guard;
    private readonly Random _rng;

    public BrainSnapshot State { get; private set; }

    public Vocabulary Vocab => State.Vocabulary;
    public PatternMemory Patterns => State.Patterns;
    public ContentGuard Guard => _guard;

    /// <summary>O dilin Markov zinciri.</summary>
    public MarkovChain Markov(Lang lang) => State.ChainFor(lang);

    public BotBrain(BrainSnapshot state, ContentGuard guard, Random rng)
    {
        State = state;
        State.Migrate();
        _guard = guard;
        _rng = rng;
    }

    // ---------------------------------------------------------------- öğrenme

    /// <summary>
    /// Botun öğrenme kapısı. Kullanıcının her mesajı buradan geçer.
    /// Öğrenilen her şey mesajın diline yazılır.
    /// </summary>
    public LearnReport Observe(string user, string text, Lang lang)
    {
        var report = new LearnReport { Lang = lang };

        if (string.IsNullOrWhiteSpace(text)) return report;

        report.Verdict = _guard.Inspect(text);

        var profile = GetProfile(user);
        profile.Touch();
        profile.NoteLang(lang);
        State.TotalMessages++;

        // Kin / ruh hâli, engellenmiş mesajlarda bile güncellenir
        report.GrudgeDelta = UpdateMood(profile, text);

        if (report.Verdict != GuardVerdict.Clean) return report;

        profile.NoteQuote(text);

        var chain = State.ChainFor(lang);

        // 1) Cümleleri o dilin Markov zincirine bas
        foreach (var sentence in Tokenizer.Sentences(text))
        {
            var tokens = Tokenizer.Tokenize(sentence, lang)
                .Where(_guard.IsSafeWord)
                .ToList();

            if (tokens.Count < 2) continue;

            chain.Learn(tokens);
            report.SentencesLearned++;
        }

        // 2) Kelimeleri dağarcığa al
        foreach (var word in Tokenizer.ContentWords(text, lang))
        {
            if (!_guard.IsSafeWord(word)) continue;

            var profane = LooksProfane(word);
            var isNew = Vocab.Learn(word, user, profane, lang);

            profile.NoteWord(word);

            if (!isNew) continue;

            report.NewWords.Add(word);
            if (profane) report.NewProfanity.Add(word);
        }

        return report;
    }

    /// <summary>
    /// Kelime küfür kokuyor mu — botun küfür cephanesi böyle büyüyor.
    /// Üç dilin köklerine birden bakılır, dil tespiti şaşarsa küfür kaçmasın.
    /// </summary>
    public static bool LooksProfane(string word)
    {
        var flat = TextKit.Deobfuscate(word);
        if (flat.Length < 2) return false;

        return LangPacks.AllProfanityStems.Any(stem => TextKit.TokenMatches(flat, stem, 4));
    }

    /// <summary>Kin ve ruh hâlini günceller, kin değişimini döndürür.</summary>
    private int UpdateMood(UserProfile profile, string text)
    {
        var before = profile.Grudge;

        if (IntentDetector.ContainsProfanity(text))
        {
            profile.InsultsThrown++;
            profile.RaiseGrudge(_rng.Next(4, 10));
            State.Mood = Math.Clamp(State.Mood - _rng.Next(2, 6), 0, 100);
        }
        else if (profile.MessageCount % 7 == 0)
        {
            // Uzun süre efendi efendi konuşursan bot azıcık sakinleşir. Azıcık.
            profile.CoolDown();
            State.Mood = Math.Clamp(State.Mood + _rng.Next(0, 3), 0, 100);
        }

        // Her mesajda küçük bir rastgele sapma — bot dengesiz olsun
        State.Mood = Math.Clamp(State.Mood + _rng.Next(-2, 3), 0, 100);

        return profile.Grudge - before;
    }

    // ---------------------------------------------------------------- profil

    public UserProfile GetProfile(string user)
    {
        if (State.Users.TryGetValue(user, out var existing)) return existing;

        var profile = new UserProfile
        {
            Name = user,
            FirstSeen = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow
        };

        State.Users[user] = profile;
        return profile;
    }

    // ---------------------------------------------------------------- üretim

    /// <summary>
    /// O dilin Markov zincirinden cümle üretir; zincir zayıfsa boş döner.
    ///
    /// <paramref name="avoid"/> verilirse üretilen cümle ona fazla benziyorsa reddedilir.
    /// Sebep: az veriyle Markov, beslediğin cümleyi kelimesi kelimesine geri kusuyor —
    /// o zaman bot papağan oluyor ve kazara DÜZGÜN cevap vermiş oluyor. Olmaz.
    /// </summary>
    public string Babble(Lang lang, int maxWords = 12, string? seed = null, string? avoid = null)
    {
        var chain = State.ChainFor(lang);

        for (var attempt = 0; attempt < 6; attempt++)
        {
            var sentence = chain.Generate(_rng, maxWords, seed);
            var wordCount = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            var tooSimilar = avoid is not null && TextKit.Similarity(sentence, avoid) > 0.7;

            if (wordCount >= 3 && !tooSimilar && _guard.IsSafeToSay(sentence))
                return sentence;

            seed = null; // sonraki denemede tohumu bırak, başka yerden başlasın
        }

        return string.Empty;
    }

    /// <summary>
    /// İki ayrı Markov üretimini ortadan kesip birleştirir, sonra içine rastgele
    /// öğrenilmiş bir kelime sokuşturur.
    ///
    /// Neden: düz Markov, az veriyle öğrendiği cümleyi kelimesi kelimesine geri
    /// söylüyor. O zaman bot hem papağan hem de kazara DÜZGÜN konuşmuş oluyor.
    /// Frankenstein cümle bunu imkânsız kılıyor.
    /// </summary>
    public string FrankenBabble(Lang lang, int maxWords = 12, string? seed = null, string? avoid = null)
    {
        var first = Babble(lang, maxWords, seed, avoid);
        if (first.Length == 0) return string.Empty;

        var second = Babble(lang, maxWords, null, avoid);
        if (second.Length == 0) return Distort(first, lang);

        var head = first.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tail = second.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var headCut = Math.Max(1, head.Length / 2 + _rng.Next(0, 2));
        var tailCut = Math.Max(0, tail.Length / 2 - _rng.Next(0, 2));

        var mixed = string.Join(' ', head.Take(headCut).Concat(tail.Skip(tailCut)));
        if (mixed.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 3) mixed = first;

        var distorted = Distort(mixed, lang);
        return _guard.IsSafeToSay(distorted) ? distorted : string.Empty;
    }

    /// <summary>Cümledeki bir kelimeyi AYNI DİLDEN öğrenilmiş başka bir kelimeyle değiştirir.</summary>
    private string Distort(string sentence, Lang lang)
    {
        if (_rng.Next(100) >= 45) return sentence;

        var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 3) return sentence;

        var replacement = Vocab.RandomProfane(_rng, lang) ?? Vocab.Random(_rng, minLength: 4, lang: lang);
        if (replacement is null) return sentence;

        words[_rng.Next(1, words.Length)] = replacement;
        return string.Join(' ', words);
    }

    /// <summary>O dildeki "gelişmişliğin" 0-1 arası ölçüsü. Cevap stratejisini etkiler.</summary>
    public double Maturity(Lang lang)
    {
        var chain = State.ChainFor(lang);

        var vocabScore = Math.Min(1.0, Vocab.SizeOf(lang) / 400.0);
        var chainScore = Math.Min(1.0, chain.StateCount / 1500.0);
        var patternScore = Math.Min(1.0, Patterns.SizeOf(lang) / 25.0);

        return (vocabScore * 0.4) + (chainScore * 0.4) + (patternScore * 0.2);
    }

    // ---------------------------------------------------------------- bakım

    /// <summary>
    /// Tohum veriyi yükler — dil başına, bir kere. Eski beyin dosyası sadece Türkçe
    /// tohumlanmış olarak gelir, İngilizce ve Arapça ilk açılışta eklenir.
    /// </summary>
    public void SeedIfEmpty()
    {
        foreach (var lang in LangInfo.All)
        {
            if (State.IsSeeded(lang)) continue;

            var pack = LangPacks.For(lang);
            var chain = State.ChainFor(lang);

            foreach (var line in pack.SeedCorpus)
            {
                var tokens = Tokenizer.Tokenize(line, lang);
                if (tokens.Count >= 2) chain.Learn(tokens);

                foreach (var word in Tokenizer.ContentWords(line, lang))
                    Vocab.Learn(word, "tohum", LooksProfane(word), lang);
            }

            foreach (var (trigger, response) in pack.SeedPatterns)
                Patterns.Teach(trigger, response, "tohum", lang);

            State.MarkSeeded(lang);
        }
    }

    /// <summary>Beyin şişince temizlik yapar.</summary>
    public (int chain, int words) Compact()
    {
        var chain = 0;
        foreach (var lang in LangInfo.All) chain += State.ChainFor(lang).Prune();

        var words = Vocab.Prune();
        return (chain, words);
    }

    public void Reset()
    {
        State = new BrainSnapshot();
        SeedIfEmpty();
    }

    public string StatsText()
    {
        var lines = new List<string>
        {
            $"toplam mesaj      : {State.TotalMessages}",
            $"kelime dağarcığı  : {Vocab.Size} kelime ({Vocab.ProfaneSize} tanesi küfür)",
            $"öğretilen kalıp   : {Patterns.Size} tetik / {Patterns.ResponseCount} cevap",
            $"tanıdığı insanlar : {State.Users.Count}",
            $"ruh hâli          : {State.Mood}/100",
            $"doğum tarihi      : {State.BornAt.ToLocalTime():dd.MM.yyyy HH:mm}"
        };

        foreach (var lang in LangInfo.All)
        {
            var chain = State.ChainFor(lang);
            lines.Add($"  [{lang.Code()}] {lang.Display(),-8} : {Vocab.SizeOf(lang)} kelime / " +
                      $"{chain.StateCount} geçiş / {chain.LearnedSequences} cümle / olgunluk %{Maturity(lang) * 100:F0}");
        }

        var newest = Vocab.Newest(8).Select(kv => kv.Key).ToList();
        if (newest.Count > 0)
            lines.Add($"son öğrendikleri  : {string.Join(", ", newest)}");

        return string.Join(Environment.NewLine, lines);
    }
}
