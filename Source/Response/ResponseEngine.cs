using MFBot.Bot;
using MFBot.Brain;
using MFBot.Languages;

namespace MFBot.Response;

/// <summary>
/// Cevabın nasıl üretileceğine karar veren katman.
///
/// Sıra şu: engellenmiş içerik mi -> öğretilmiş kalıp var mı -> niyete göre
/// yanlış cevap üret -> öğrendiklerinden bir şeyler karıştır -> üsluba sok ->
/// son güvenlik süzgecinden geçir.
///
/// Her dilin kendi takımı var (paket + hakaret üreteci + şablon doldurucu +
/// yanlış cevap motoru). Cevap, mesajın diliyle aynı takımdan çıkar — botun
/// "yazdığın dilde cevap verme" garantisi tam olarak burada duruyor.
/// </summary>
public sealed class ResponseEngine
{
    private readonly Random _rng;
    private readonly BotBrain _brain;
    private readonly BotConfig _config;
    private readonly ToxicStyler _styler;
    private readonly MathSaboteur _math;
    private readonly Dictionary<Lang, LangRig> _rigs = new();

    /// <summary>Bir dilin cevap üretme takımı.</summary>
    private sealed class LangRig
    {
        public required LangPack Pack { get; init; }
        public required InsultGenerator Insults { get; init; }
        public required TemplateFiller Filler { get; init; }
        public required WrongAnswerEngine Wrong { get; init; }
    }

    public ResponseEngine(Random rng, BotBrain brain, BotConfig config)
    {
        _rng = rng;
        _brain = brain;
        _config = config;

        _math = new MathSaboteur(rng);
        _styler = new ToxicStyler(rng);

        foreach (var lang in LangInfo.All)
        {
            var pack = LangPacks.For(lang);
            var insults = new InsultGenerator(rng, brain.Vocab, pack);
            var filler = new TemplateFiller(rng, pack, insults, brain.Vocab);

            _rigs[lang] = new LangRig
            {
                Pack = pack,
                Insults = insults,
                Filler = filler,
                Wrong = new WrongAnswerEngine(rng, pack, filler, _math)
            };
        }
    }

    public string Compose(string user, string message, IntentResult intent, LearnReport report)
    {
        var lang = intent.Lang;
        var rig = _rigs[lang];

        var profile = _brain.GetProfile(user);
        var level = InsultGenerator.LevelFor(profile.Grudge, _brain.State.Mood);

        // Ayar dosyasındaki toxicity seviyeyi yukarı iter
        if (_config.Toxicity >= 8) level = Math.Min(4, level + 1);
        if (_config.Toxicity <= 3) level = Math.Max(1, level - 1);

        var intensity = Math.Clamp(profile.Grudge + (_config.Toxicity * 4) - (_brain.State.Mood / 3), 0, 100);

        // Lakap dil başına bir kere takılır — İngilizce cevapta Türkçe lakap komik değil, bozuk
        if (profile.NicknameFor(lang).Length == 0 && profile.MessageCount >= 2)
            profile.SetNickname(lang, rig.Insults.Nickname());

        rig.Filler.ResetContext();
        rig.Filler.Level = level;
        rig.Filler.Nickname = profile.NicknameFor(lang);

        var body = report.Verdict == GuardVerdict.Block
            ? rig.Filler.Pick(rig.Pack.Deflect)
            : Build(message, intent, profile, rig, level, lang);

        body = MaybeAddLearnedSpice(body, rig, report, level);

        var styled = _styler.Style(body, intensity, lang);
        styled = _styler.AppendTail(styled, rig.Insults.Tail(), intensity);

        // Son süzgeç: kendi ürettiğimiz cümle sınırı aştıysa sade hakarete düş
        if (!_brain.Guard.IsSafeToSay(styled))
            styled = rig.Insults.Sentence(level);

        if (string.IsNullOrWhiteSpace(styled)) styled = rig.Insults.Sentence(level);

        // Skor tablosu: her cevap değil, sadece gerçekten küfürlü olanlar sayılır
        if (IntentDetector.ContainsProfanity(styled)) profile.InsultsTaken++;

        return styled;
    }

    // ------------------------------------------------------------- strateji seçimi

    private string Build(string message, IntentResult intent, UserProfile profile, LangRig rig, int level, Lang lang)
    {
        // 1) Öğretilmiş kalıp varsa büyük ihtimalle onu kullan (sadece aynı dildekiler)
        var pattern = _brain.Patterns.Match(message, lang);
        if (pattern is not null && _rng.Next(100) < 85)
            return rig.Filler.Fill(pattern.Pick(_rng));

        // 2) Niyete göre
        return intent.Intent switch
        {
            Intent.MathQuestion => AnswerMath(message, rig, level, lang),
            Intent.Definition => rig.Wrong.Definition(intent.Subject ?? message, level),
            Intent.YesNoQuestion => rig.Wrong.YesNo(message, level),
            Intent.WhQuestion => rig.Wrong.Wh(message, IntentDetector.QuestionKind(message, lang), level),
            Intent.Greeting => rig.Filler.Pick(rig.Pack.Greeting),
            Intent.Farewell => rig.Filler.Pick(rig.Pack.Farewell),
            Intent.Insult => AnswerInsult(rig, level),
            Intent.Compliment => rig.Filler.Pick(rig.Pack.ComplimentRejection),
            Intent.AboutBot => rig.Filler.Pick(rig.Pack.AboutBot),
            _ => AnswerStatement(message, profile, rig, level, lang)
        };
    }

    private string AnswerMath(string message, LangRig rig, int level, Lang lang)
    {
        var result = _math.TryEvaluate(message, lang);

        // İfadeyi çözemediyse yine de matematikten anlamıyormuş gibi yapmasın, saçmalasın
        return result is null ? rig.Wrong.MathUnparsed(level) : rig.Wrong.Math(result, level);
    }

    private string AnswerInsult(LangRig rig, int level)
    {
        // Küfür yiyince kin artıyor, cevap da sertleşiyor
        var comeback = rig.Filler.Pick(rig.Pack.InsultComeback);

        if (level >= 3 && _rng.Next(100) < 40)
            return $"{comeback}. {rig.Insults.Sentence(level)}";

        return comeback;
    }

    private string AnswerStatement(string message, UserProfile profile, LangRig rig, int level, Lang lang)
    {
        var roll = _rng.Next(100);
        var maturity = _brain.Maturity(lang);

        // Bot o dilde olgunlaştıkça kendi ürettiği cümleleri daha çok kullanır
        var babbleThreshold = (int)(_config.BabbleChance * maturity);

        if (roll < babbleThreshold)
        {
            var seed = Tokenizer.TopicWord(message, lang);
            var babble = _brain.FrankenBabble(lang, 12, seed, avoid: message);

            // Ham Markov çıktısı bazen kazara normal bir cümle oluyor.
            // Karakteri bozulmasın diye her seferinde küfürle sarmalanır.
            if (babble.Length > 0)
            {
                rig.Filler.Babble = babble;
                return rig.Filler.Pick(rig.Pack.Garnish);
            }
        }

        // Kullanıcının kendi lafını suratına çarp — ama aynı dilde söylediği bir laf olsun
        if (roll is >= 60 and < 72)
        {
            var quote = PickQuote(profile, message, lang);

            if (quote is not null)
            {
                rig.Filler.Quote = quote;
                return rig.Filler.Pick(rig.Pack.QuoteComebacks);
            }
        }

        // Kullanıcının favori kelimesini hakarete çevir
        if (roll is >= 72 and < 80)
        {
            var favorite = FavoriteWordIn(profile, lang);
            if (favorite is not null) return rig.Insults.TurnAgainst(favorite);
        }

        return rig.Filler.Pick(rig.Pack.StatementReply);
    }

    // ------------------------------------------------------------- öğrenme vitrini

    /// <summary>
    /// Botun öğrendiğini görünür kılar: yeni kelimeyle övünür ya da
    /// öğrendiği bir küfrü suratına çarpar.
    /// </summary>
    private string MaybeAddLearnedSpice(string body, LangRig rig, LearnReport report, int level)
    {
        // Yeni küfür öğrendiyse bunu kesinlikle yüzüne vursun
        if (report.NewProfanity.Count > 0 && _rng.Next(100) < 55)
        {
            rig.Filler.Word = report.NewProfanity[_rng.Next(report.NewProfanity.Count)];
            var note = rig.Filler.Pick(rig.Pack.NewProfanityNote);
            rig.Filler.Word = null;
            return $"{body} {note}";
        }

        if (report.NewWords.Count > 0 && _rng.Next(100) < _config.BragChance)
        {
            rig.Filler.Word = report.NewWords[_rng.Next(report.NewWords.Count)];
            var brag = rig.Filler.Pick(rig.Pack.LearningBrag);
            rig.Filler.Word = null;
            return $"{body} ({brag})";
        }

        return body;
    }

    // ------------------------------------------------------------- yardımcılar

    /// <summary>
    /// Kullanıcının eski laflarından bu dilde yazılmış birini seçer.
    /// Alıntılar dil etiketiyle saklanmıyor, o yüzden kullanım anında tespit ediliyor —
    /// İngilizce cevabın ortasına Türkçe alıntı sıkıştırmak dil tutmayı bozar.
    /// </summary>
    private string? PickQuote(UserProfile profile, string message, Lang lang)
    {
        if (profile.Quotes.Count == 0) return null;

        var candidates = profile.Quotes
            .Where(q => !string.Equals(q, message, StringComparison.OrdinalIgnoreCase))
            .Where(q => LanguageDetector.Analyse(q, lang) is { Confidence: > 0 } analysis && analysis.Lang == lang)
            .ToList();

        return candidates.Count == 0 ? null : candidates[_rng.Next(candidates.Count)];
    }

    /// <summary>Kullanıcının en sevdiği kelimelerden bu dile ait olanı.</summary>
    private string? FavoriteWordIn(UserProfile profile, Lang lang)
    {
        var pool = profile.FavoriteWords
            .Where(kv => _brain.Vocab.Words.TryGetValue(kv.Key, out var stat) && Vocabulary.LangOf(stat) == lang)
            .ToList();

        if (pool.Count == 0) return null;

        var repeated = pool.Where(kv => kv.Value >= 2).ToList();
        if (repeated.Count > 0) pool = repeated;

        return pool[_rng.Next(pool.Count)].Key;
    }
}
