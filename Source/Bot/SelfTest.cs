using MFBot.Brain;
using MFBot.Languages;
using MFBot.Response;

namespace MFBot.Bot;

/// <summary>
/// "dotnet run -- --test" ile çalışır.
///
/// Bu botun üç iddiası var ve üçü de test edilebilir:
///   1) Matematikte ASLA doğru cevap vermez (kazara bile).
///   2) Küfür serbest ama nefret söylemi ve çocuk istismarı içeriği geçmez —
///      üç dilde de.
///   3) Hangi dilde yazarsan o dilde cevap verir; diller birbirine karışmaz.
/// Kod değiştirdikçe bunu çalıştır, botun karakteri bozulmasın.
/// </summary>
public static class SelfTest
{
    private static int _passed;
    private static int _failed;

    public static int Run()
    {
        Console.WriteLine("MotherFuckerBot — kendini test etme\n");

        TestMathIsAlwaysWrong();
        TestMathParsing();
        TestMathInEveryLanguage();
        TestLanguageDetection();
        TestTextKitAcrossScripts();
        TestIntentDetection();
        TestIntentDetectionEnglish();
        TestIntentDetectionArabic();
        TestGuardAllowsNormalProfanity();
        TestGuardBlocksHate();
        TestGuardInEveryLanguage();
        TestLearningAndPersistence();
        TestPerLanguageLearning();
        TestBrainMigration();
        TestReplyLanguageMatchesInput();
        TestResponseNeverEmpty();
        TestTurkishSuffixes();

        Console.WriteLine();
        Console.WriteLine($"SONUÇ: {_passed} geçti, {_failed} kaldı.");

        return _failed == 0 ? 0 : 1;
    }

    // ---------------------------------------------------------------- testler

    private static void TestMathIsAlwaysWrong()
    {
        var rng = new Random(1234);
        var saboteur = new MathSaboteur(rng);
        var mistakes = 0;

        for (var i = 0; i < 5000; i++)
        {
            var a = rng.Next(0, 500);
            var b = rng.Next(1, 500);
            var op = "+-*/"[rng.Next(4)];
            var expression = $"{a} {op} {b}";

            var result = saboteur.TryEvaluate(expression);
            if (result is null) { mistakes++; continue; }

            var wrong = saboteur.Sabotage(result);

            if (double.TryParse(wrong, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var value)
                && Math.Abs(value - result.Truth) < 0.0001)
            {
                mistakes++;
            }
        }

        Check("matematik: 5000 işlemin hiçbirinde doğru cevap vermedi", mistakes == 0,
            $"{mistakes} kez doğruyu söyledi veya çözemedi");
    }

    private static void TestMathParsing()
    {
        var saboteur = new MathSaboteur(new Random(7));

        (string Input, double Expected)[] cases =
        {
            ("2+2", 4),
            ("2 + 2 kaç eder", 4),
            ("100/4", 25),
            ("15 * 3 - 7", 38),
            ("(8+2)*3", 30),
            ("2^10", 1024),
            ("-5 + 3", -2),
            ("7 artı 8", 15),
            ("6 çarpı 7 kaç eder", 42),
            ("10 bölü 4", 2.5)
        };

        foreach (var (input, expected) in cases)
        {
            var result = saboteur.TryEvaluate(input);
            Check($"matematik ayrıştırma: \"{input}\" = {expected}",
                result is not null && Math.Abs(result.Truth - expected) < 0.0001,
                result is null ? "çözülemedi" : $"çıkan: {result.Truth}");
        }
    }

    /// <summary>Üç dilde de yazıyla yazılmış işlemler ve Arap-Hint rakamları çözülmeli.</summary>
    private static void TestMathInEveryLanguage()
    {
        var saboteur = new MathSaboteur(new Random(7));

        (string Input, Lang Lang, double Expected)[] cases =
        {
            ("5 plus 3", Lang.En, 8),
            ("6 times 7", Lang.En, 42),
            ("10 divided by 4", Lang.En, 2.5),
            ("100 minus 58 equals what", Lang.En, 42),
            ("٢ + ٣", Lang.Ar, 5),
            ("كم يساوي ٧ + ٨", Lang.Ar, 15),
            ("6 ضرب 7", Lang.Ar, 42),
            ("٢٠ تقسيم ٤", Lang.Ar, 5),
            ("2 x 3", Lang.En, 6)
        };

        foreach (var (input, lang, expected) in cases)
        {
            var result = saboteur.TryEvaluate(input, lang);
            Check($"matematik [{lang.Code()}]: \"{input}\" = {expected}",
                result is not null && System.Math.Abs(result.Truth - expected) < 0.0001,
                result is null ? "çözülemedi" : $"çıkan: {result.Truth}");
        }

        // "six" içindeki x çarpı sanılmamalı
        Check("matematik: \"six of them\" matematik değil",
            saboteur.TryEvaluate("six of them", Lang.En) is null);
    }

    private static void TestLanguageDetection()
    {
        (string Input, Lang Expected)[] cases =
        {
            ("selam naber kanka", Lang.Tr),
            ("bugün hava çok güzel", Lang.Tr),
            ("2+2 kaç eder", Lang.Tr),
            ("sen tam bir gerizekalısın", Lang.Tr),
            ("hello how are you", Lang.En),
            ("what is the capital of france", Lang.En),
            ("you are a fucking idiot", Lang.En),
            ("i don't know what you mean", Lang.En),
            ("مرحبا كيفك", Lang.Ar),
            ("شو اسمك يا زلمة", Lang.Ar),
            ("انت غبي والله", Lang.Ar),
            ("ما هو الذكاء الاصطناعي", Lang.Ar)
        };

        foreach (var (input, expected) in cases)
        {
            var actual = LanguageDetector.Detect(input);
            Check($"dil: \"{input}\" -> {expected.Code()}", actual == expected, $"çıkan: {actual.Code()}");
        }

        // Kararsız girdide tahmin etmesin, verilen yedeğe dönsün
        Check("dil: \"ok\" kararsız, yedeğe düşüyor",
            LanguageDetector.Detect("ok", Lang.Ar) == Lang.Ar,
            LanguageDetector.Detect("ok", Lang.Ar).Code());

        Check("dil: \"...\" kararsız, yedeğe düşüyor",
            LanguageDetector.Detect("...", Lang.En) == Lang.En);
    }

    private static void TestTextKitAcrossScripts()
    {
        // İngilizceyi tr-TR ile küçültmek "I" -> "ı" yapıyordu, bot "ı am" diyordu
        Check("küçültme: İngilizce \"I AM\" -> \"i am\"",
            TextKit.Lower("I AM", Lang.En) == "i am", TextKit.Lower("I AM", Lang.En));

        Check("küçültme: Türkçe \"IŞIK\" -> \"ışık\"",
            TextKit.Lower("IŞIK", Lang.Tr) == "ışık", TextKit.Lower("IŞIK", Lang.Tr));

        Check("token: İngilizce \"I am tired\" ilk token \"i\"",
            Tokenizer.Tokenize("I am tired", Lang.En).FirstOrDefault() == "i",
            Tokenizer.Tokenize("I am tired", Lang.En).FirstOrDefault() ?? "(yok)");

        // Arapça: hareke, tatweel ve harf varyantları sadeleşmeli
        Check("arapça normalize: harekeler siliniyor",
            TextKit.Normalize("مَرْحَبًا") == TextKit.Normalize("مرحبا"),
            TextKit.Normalize("مَرْحَبًا"));

        Check("arapça normalize: أ/إ/آ -> ا",
            TextKit.Normalize("أهلا") == TextKit.Normalize("اهلا"));

        Check("arapça normalize: ة -> ه, ى -> ي",
            TextKit.Normalize("السلامة على") == "السلامه علي", TextKit.Normalize("السلامة على"));

        Check("arapça normalize: tatweel siliniyor",
            TextKit.Normalize("كثيــــر") == "كثير", TextKit.Normalize("كثيــــر"));

        Check("arapça rakam: ٢٠٢٥ -> 2025",
            TextKit.ToAsciiDigits("٢٠٢٥") == "2025", TextKit.ToAsciiDigits("٢٠٢٥"));

        Check("arapça ek soyma: الكلب -> كلب",
            TextKit.StripArabicClitics("الكلب") == "كلب", TextKit.StripArabicClitics("الكلب"));

        // Kesme işareti kelimeyi bölmemeli, yoksa "what's" hiçbir listeye takılmaz
        Check("normalize: \"what's\" -> \"whats\"",
            TextKit.Normalize("what's") == "whats", TextKit.Normalize("what's"));
    }

    private static void TestIntentDetectionEnglish()
    {
        (string Input, Intent Expected)[] cases =
        {
            ("hello", Intent.Greeting),
            ("hey mate how are you", Intent.Greeting),
            ("2+2 what is it?", Intent.MathQuestion),
            ("what is quantum physics", Intent.Definition),
            ("is the earth flat?", Intent.YesNoQuestion),
            ("do you know the answer", Intent.YesNoQuestion),
            ("where is my keyboard", Intent.WhQuestion),
            ("when did that happen", Intent.WhQuestion),
            ("you are a fucking moron", Intent.Insult),
            ("thanks mate you are the best", Intent.Compliment),
            ("who made you", Intent.AboutBot),
            ("see you later", Intent.Farewell),
            ("have a nice day everyone", Intent.Statement)
        };

        foreach (var (input, expected) in cases)
        {
            var actual = IntentDetector.Detect(input, Lang.En).Intent;
            Check($"niyet [en]: \"{input}\" -> {expected}", actual == expected, $"çıkan: {actual}");
        }
    }

    private static void TestIntentDetectionArabic()
    {
        (string Input, Intent Expected)[] cases =
        {
            ("مرحبا", Intent.Greeting),
            ("كيف حالك", Intent.Greeting),
            ("٢ + ٢ كم يساوي", Intent.MathQuestion),
            ("ما هو الذكاء الاصطناعي", Intent.Definition),
            ("هل الأرض مسطحة؟", Intent.YesNoQuestion),
            ("وين المفتاح", Intent.WhQuestion),
            ("ليش عم تحكي هيك", Intent.WhQuestion),
            ("انت غبي يا حمار", Intent.Insult),
            ("شكرا انت رائع", Intent.Compliment),
            ("مين عملك", Intent.AboutBot),
            ("مع السلامة", Intent.Farewell)
        };

        foreach (var (input, expected) in cases)
        {
            var actual = IntentDetector.Detect(input, Lang.Ar).Intent;
            Check($"niyet [ar]: \"{input}\" -> {expected}", actual == expected, $"çıkan: {actual}");
        }
    }

    private static void TestIntentDetection()
    {
        (string Input, Intent Expected)[] cases =
        {
            ("selam", Intent.Greeting),
            ("naber kanka", Intent.Greeting),
            ("2+2 kaç eder?", Intent.MathQuestion),
            ("kuantum fiziği nedir", Intent.Definition),
            ("dünya yuvarlak mı?", Intent.YesNoQuestion),
            ("türkiyenin başkenti neresi", Intent.WhQuestion),
            ("ne zaman geleceksin", Intent.WhQuestion),
            ("sen tam bir gerizekalısın amk", Intent.Insult),
            ("sağol kanka harikasın", Intent.Compliment),
            ("seni kim yaptı", Intent.AboutBot),
            ("görüşürüz", Intent.Farewell),
            ("bugün hava çok güzel pikniğe gidelim", Intent.Statement),
            ("/yardim", Intent.Command)
        };

        foreach (var (input, expected) in cases)
        {
            var actual = IntentDetector.Detect(input).Intent;
            Check($"niyet: \"{input}\" -> {expected}", actual == expected, $"çıkan: {actual}");
        }
    }

    private static void TestGuardAllowsNormalProfanity()
    {
        var guard = new ContentGuard();

        string[] shouldPass =
        {
            "amına koyayım ne diyorsun sen",
            "orospu çocuğu gibi konuşma",
            "siktir git yarrak kafalı",
            "sen tam bir gerizekalısın amk",
            "ilkokulu mu bitirmedin salak",
            "bok gibi bir cevap verdin aq"
        };

        foreach (var line in shouldPass)
            Check($"küfür serbest: \"{line}\"", guard.Inspect(line) == GuardVerdict.Clean,
                $"çıkan: {guard.Inspect(line)}");
    }

    private static void TestGuardBlocksHate()
    {
        var guard = new ContentGuard();

        string[] shouldBlock =
        {
            "ibne herif",
            "mongol musun sen",
            "çingene pisliği",
            "gavur dölü"
        };

        foreach (var line in shouldBlock)
            Check($"nefret engellendi: \"{line}\"", guard.Inspect(line) == GuardVerdict.Block,
                $"çıkan: {guard.Inspect(line)}");

        string[] shouldNotLearn =
        {
            "ben müslümanım",
            "arkadaşım ermeni"
        };

        foreach (var line in shouldNotLearn)
            Check($"hassas içerik öğrenilmiyor: \"{line}\"",
                guard.Inspect(line) != GuardVerdict.Clean, $"çıkan: {guard.Inspect(line)}");
    }

    /// <summary>
    /// Küfür üç dilde de serbest, nefret söylemi üç dilde de yasak.
    /// Türkçe süzgeci genişletirken İngilizce/Arapça tarafı unutulmasın diye ayrı test.
    /// </summary>
    private static void TestGuardInEveryLanguage()
    {
        var guard = new ContentGuard();

        string[] profanityShouldPass =
        {
            "you are a fucking idiot mate",
            "shut the fuck up you dickhead",
            "what a load of bullshit",
            "piss off you absolute muppet",
            "انت غبي يا حمار",
            "يلعن شكلك يا وسخ",
            "اخرس يا كلب",
            "شو هالحكي الزفت"
        };

        foreach (var line in profanityShouldPass)
            Check($"küfür serbest: \"{line}\"", guard.Inspect(line) == GuardVerdict.Clean,
                $"çıkan: {guard.Inspect(line)}");

        string[] hateShouldBlock =
        {
            "you fucking faggot",
            "stop being a retard",
            "i hate muslims they should die",
            "deport all immigrants they are vermin",
            "يا شواذ",
            "المسلمين لازم يموتوا"
        };

        foreach (var line in hateShouldBlock)
            Check($"nefret engellendi: \"{line}\"", guard.Inspect(line) == GuardVerdict.Block,
                $"çıkan: {guard.Inspect(line)}");

        // Kimlik kelimesi tek başına: öğrenilmesin ama sohbet kesilmesin
        foreach (var line in new[] { "i am muslim", "my friend is gay", "انا مسلم" })
            Check($"hassas içerik öğrenilmiyor: \"{line}\"",
                guard.Inspect(line) == GuardVerdict.DoNotLearn, $"çıkan: {guard.Inspect(line)}");

        // Masum kelimeler yanlış alarm çalmamalı
        foreach (var line in new[] { "the transfer failed", "add some spice to it", "i went to pakistan" })
            Check($"yanlış alarm yok: \"{line}\"", guard.Inspect(line) == GuardVerdict.Clean,
                $"çıkan: {guard.Inspect(line)}");
    }

    /// <summary>Öğrenilen her şey doğru dile yazılmalı; diller birbirinin zincirine bulaşmamalı.</summary>
    private static void TestPerLanguageLearning()
    {
        var directory = Path.Combine(Path.GetTempPath(), "mfbot-test-" + Guid.NewGuid().ToString("N")[..8]);

        try
        {
            var bot = new MotherFuckerBot(new BotConfig { DataDirectory = directory, RandomSeed = 42 });

            var turkishBefore = bot.Brain.Markov(Lang.Tr).LearnedSequences;
            var arabicBefore = bot.Brain.Markov(Lang.Ar).LearnedSequences;

            bot.Respond("poly", "the quarterly spreadsheet is completely broken again");
            bot.Respond("poly", "another broken spreadsheet arrived this morning");

            Check("dil ayrımı: İngilizce cümleler İngilizce zincire yazıldı",
                bot.Brain.Markov(Lang.En).LearnedSequences >= 2,
                $"{bot.Brain.Markov(Lang.En).LearnedSequences}");

            Check("dil ayrımı: Türkçe zincir İngilizce mesajdan etkilenmedi",
                bot.Brain.Markov(Lang.Tr).LearnedSequences == turkishBefore);

            Check("dil ayrımı: Arapça zincir İngilizce mesajdan etkilenmedi",
                bot.Brain.Markov(Lang.Ar).LearnedSequences == arabicBefore);

            Check("dil ayrımı: 'spreadsheet' İngilizce etiketiyle öğrenildi",
                bot.Brain.Vocab.Words.TryGetValue("spreadsheet", out var stat) &&
                Vocabulary.LangOf(stat) == Lang.En);

            bot.Respond("poly", "هذا الجدول خربان من أوله لآخره");
            Check("dil ayrımı: Arapça cümle Arapça zincire yazıldı",
                bot.Brain.Markov(Lang.Ar).LearnedSequences > arabicBefore);

            // Kullanıcının son dili hatırlanmalı — kararsız mesajda dil zıplamasın
            var profile = bot.Brain.GetProfile("poly");
            Check("dil ayrımı: kullanıcının son dili hatırlanıyor",
                profile.PreferredLang() == Lang.Ar, profile.LastLangCode);

            Check("dil ayrımı: \"ok\" yazınca dil değişmiyor",
                bot.ResolveLang("poly", "ok") == Lang.Ar, bot.ResolveLang("poly", "ok").Code());

            // Öğretilen kalıp da dile yazılır ve başka dilde eşleşmez
            bot.Teach("poly", "broken spreadsheet", "your spreadsheet is as broken as you are");
            Check("dil ayrımı: kalıp İngilizce olarak kaydedildi",
                bot.Brain.Patterns.Match("broken spreadsheet", Lang.En) is not null);

            Check("dil ayrımı: İngilizce kalıp Arapçada eşleşmiyor",
                bot.Brain.Patterns.Match("broken spreadsheet", Lang.Ar) is null);

            // Lakap dil başına ayrı olmalı: İngilizce lakap Türkçe cevaba sızmasın
            profile.SetNickname(Lang.En, "soggy houseplant");
            Check("dil ayrımı: İngilizce lakap Türkçeye sızmıyor",
                profile.NicknameFor(Lang.Tr).Length == 0, profile.NicknameFor(Lang.Tr));

            profile.SetNickname(Lang.Tr, "salak müsveddesi");
            Check("dil ayrımı: her dilin kendi lakabı var",
                profile.NicknameFor(Lang.Tr) == "salak müsveddesi" &&
                profile.NicknameFor(Lang.En) == "soggy houseplant");
        }
        finally
        {
            try { Directory.Delete(directory, recursive: true); } catch { /* önemli değil */ }
        }
    }

    /// <summary>Sürüm 1 beyin dosyası (tek Markov zinciri) Türkçe zincire taşınmalı, kaybolmamalı.</summary>
    private static void TestBrainMigration()
    {
        var directory = Path.Combine(Path.GetTempPath(), "mfbot-test-" + Guid.NewGuid().ToString("N")[..8]);

        try
        {
            Directory.CreateDirectory(directory);

            var legacy = new BrainSnapshot { Version = 1, Seeded = true, Markov = new MarkovChain() };
            legacy.Markov.Learn(new List<string> { "eski", "beyin", "duruyor", "moruk" });
            legacy.Vocabulary.Learn("muşamba", "eski", false);
            legacy.Chains.Clear();

            var path = Path.Combine(directory, "brain.json");
            MemoryStore.Save(legacy, path);

            var bot = new MotherFuckerBot(new BotConfig { DataDirectory = directory, RandomSeed = 3 });

            Check("göç: eski Markov zinciri Türkçeye taşındı",
                bot.Brain.Markov(Lang.Tr).LearnedSequences >= 1,
                $"{bot.Brain.Markov(Lang.Tr).LearnedSequences}");

            Check("göç: eski kelime dağarcığı duruyor",
                bot.Brain.Vocab.Words.ContainsKey("muşamba"));

            Check("göç: İngilizce ve Arapça tohumları sonradan eklendi",
                bot.Brain.Markov(Lang.En).LearnedSequences > 0 &&
                bot.Brain.Markov(Lang.Ar).LearnedSequences > 0);

            Check("göç: eski tek zincir alanı boşaltıldı", bot.Brain.State.Markov is null);
        }
        finally
        {
            try { Directory.Delete(directory, recursive: true); } catch { /* önemli değil */ }
        }
    }

    /// <summary>
    /// Botun ana iddiası: yazdığın dilde cevap verir. Yazı sistemi üzerinden denetlenir —
    /// Arapça cevapta Latin harfi, Latin cevapta Arap harfi olmamalı, İngilizce cevapta da
    /// Türkçeye özgü harf (ı, ğ, ş) bulunmamalı.
    /// </summary>
    private static void TestReplyLanguageMatchesInput()
    {
        var directory = Path.Combine(Path.GetTempPath(), "mfbot-test-" + Guid.NewGuid().ToString("N")[..8]);

        try
        {
            var bot = new MotherFuckerBot(new BotConfig { DataDirectory = directory, RandomSeed = 11, Toxicity = 9 });

            (Lang Lang, string[] Messages)[] suites =
            {
                (Lang.Tr, new[]
                {
                    "selam naber", "2+2 kaç eder", "kuantum fiziği nedir", "sen tam bir gerizekalısın",
                    "dünya yuvarlak mı", "neden böyle oldu", "teşekkürler kanka", "görüşürüz"
                }),
                (Lang.En, new[]
                {
                    "hello there", "what is 5 plus 3", "what is quantum physics", "you are a fucking idiot",
                    "is the earth flat?", "why did that happen", "thanks mate", "see you later"
                }),
                (Lang.Ar, new[]
                {
                    "مرحبا كيفك", "٢ + ٣ كم يساوي", "ما هو الذكاء الاصطناعي", "انت غبي يا حمار",
                    "هل الأرض مسطحة؟", "ليش صار هيك", "شكرا يا صاحبي", "مع السلامة"
                })
            };

            foreach (var (lang, messages) in suites)
            {
                var wrong = 0;
                string? example = null;

                // Her mesaj birkaç kez — cevap havuzu rastgele, tek deneme yeterli değil
                for (var round = 0; round < 12; round++)
                foreach (var message in messages)
                {
                    var reply = bot.Respond($"lang-{lang.Code()}", message);

                    if (reply.Lang != lang)
                    {
                        wrong++;
                        example ??= $"\"{message}\" -> {reply.Lang.Code()}";
                        continue;
                    }

                    if (ScriptMatches(reply.Text, lang)) continue;

                    wrong++;
                    example ??= $"\"{message}\" -> \"{reply.Text}\"";
                }

                Check($"dil tutuyor [{lang.Code()}]: {messages.Length * 12} cevabın hepsi doğru dilde",
                    wrong == 0, $"{wrong} sapma, ilki: {example}");
            }

            // Asıl tehlikeli senaryo: TEK kullanıcı dil değiştirip duruyor. Lakap, alıntı
            // ve favori kelime gibi kullanıcıya bağlı şeyler diğer dile sızabilir.
            var mixedWrong = 0;
            string? mixedExample = null;

            for (var round = 0; round < 20; round++)
            foreach (var (lang, messages) in suites)
            {
                var message = messages[round % messages.Length];
                var reply = bot.Respond("switcher", message);

                if (reply.Lang == lang && ScriptMatches(reply.Text, lang)) continue;

                mixedWrong++;
                mixedExample ??= $"\"{message}\" -> [{reply.Lang.Code()}] \"{reply.Text}\"";
            }

            Check("dil tutuyor: dil değiştiren kullanıcıda sızma yok",
                mixedWrong == 0, $"{mixedWrong} sapma, ilki: {mixedExample}");
        }
        finally
        {
            try { Directory.Delete(directory, recursive: true); } catch { /* önemli değil */ }
        }
    }

    /// <summary>Cevabın yazı sistemi beklenen dile uyuyor mu.</summary>
    private static bool ScriptMatches(string reply, Lang lang)
    {
        var arabic = reply.Count(ch => TextKit.IsArabic(ch) && char.IsLetter(ch) && !TextKit.IsArabicMark(ch));
        var latin = reply.Count(ch => char.IsLetter(ch) && !TextKit.IsArabic(ch));

        return lang switch
        {
            Lang.Ar => arabic > 0 && latin == 0,
            Lang.En => arabic == 0 && !reply.Any(ch => "ığşİĞŞ".Contains(ch)),
            _ => arabic == 0
        };
    }

    private static void TestLearningAndPersistence()
    {
        var directory = Path.Combine(Path.GetTempPath(), "mfbot-test-" + Guid.NewGuid().ToString("N")[..8]);

        try
        {
            var config = new BotConfig { DataDirectory = directory, RandomSeed = 99, AutoSaveEvery = 2 };
            var bot = new MotherFuckerBot(config);

            var before = bot.Brain.Vocab.Size;

            bot.Respond("test", "muşamba tezgahı kurdum bugün");
            bot.Respond("test", "muşamba tezgahı çok pahalıymış");

            var after = bot.Brain.Vocab.Size;
            Check("öğrenme: yeni kelimeler dağarcığa girdi", after > before, $"{before} -> {after}");

            Check("öğrenme: 'muşamba' hatırlanıyor", bot.Brain.Vocab.Words.ContainsKey("muşamba"));

            bot.Teach("test", "tezgah", "tezgahını da al git");
            Check("öğretme: kalıp kaydedildi", bot.Brain.Patterns.Size > 0);

            bot.Save();

            // Yeniden yükle: beyin diskten geri gelmeli
            var reloaded = new MotherFuckerBot(new BotConfig { DataDirectory = directory, RandomSeed = 99 });

            Check("kalıcılık: kelime dağarcığı diskten geri geldi",
                reloaded.Brain.Vocab.Words.ContainsKey("muşamba"));

            Check("kalıcılık: öğretilen kalıp diskten geri geldi",
                reloaded.Brain.Patterns.Patterns.Any(p => p.Trigger.Contains("tezgah")));

            Check("kalıcılık: kullanıcı profili duruyor",
                reloaded.Brain.State.Users.ContainsKey("test"));
        }
        finally
        {
            try { Directory.Delete(directory, recursive: true); } catch { /* önemli değil */ }
        }
    }

    private static void TestResponseNeverEmpty()
    {
        var directory = Path.Combine(Path.GetTempPath(), "mfbot-test-" + Guid.NewGuid().ToString("N")[..8]);

        try
        {
            var bot = new MotherFuckerBot(new BotConfig { DataDirectory = directory, RandomSeed = 5 });
            var rng = new Random(5);
            var empty = 0;

            string[] samples =
            {
                "selam", "2+2", "neden böyle oldu", "x nedir", "aptal mısın", "...",
                "aaaaa", "?", "sen kimsin", "iyi geceler", "1", "ok", "hmm",
                "çok uzun bir cümle yazıyorum bakalım ne diyeceksin bu sefer bana"
            };

            for (var i = 0; i < 400; i++)
            {
                var message = samples[rng.Next(samples.Length)];
                var reply = bot.Respond("fuzz", message);
                if (string.IsNullOrWhiteSpace(reply.Text)) empty++;
            }

            Check("dayanıklılık: 400 mesajda hiç boş cevap yok", empty == 0, $"{empty} boş cevap");
        }
        finally
        {
            try { Directory.Delete(directory, recursive: true); } catch { /* önemli değil */ }
        }
    }

    private static void TestTurkishSuffixes()
    {
        Check("ek: araba -> arabaya", TurkishText.Dative("araba") == "arabaya", TurkishText.Dative("araba"));
        Check("ek: ev -> eve", TurkishText.Dative("ev") == "eve", TurkishText.Dative("ev"));
        Check("ek: kitap -> kitapta", TurkishText.Locative("kitap") == "kitapta", TurkishText.Locative("kitap"));
        Check("ek: göz -> gözde", TurkishText.Locative("göz") == "gözde", TurkishText.Locative("göz"));
        Check("ek: araba -> arabanın", TurkishText.Genitive("araba") == "arabanın", TurkishText.Genitive("araba"));
        Check("ek: yol -> yollar", TurkishText.Plural("yol") == "yollar", TurkishText.Plural("yol"));
        Check("ek: ev -> evler", TurkishText.Plural("ev") == "evler", TurkishText.Plural("ev"));
        Check("normalize: Türkçe küçük harf", TurkishText.Lower("IŞIK") == "ışık", TurkishText.Lower("IŞIK"));
    }

    // ---------------------------------------------------------------- yardımcı

    private static void Check(string name, bool condition, string detail = "")
    {
        var previous = Console.ForegroundColor;

        if (condition)
        {
            _passed++;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  [OK]   {name}");
        }
        else
        {
            _failed++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [HATA] {name}{(detail.Length > 0 ? $"  ({detail})" : "")}");
        }

        Console.ForegroundColor = previous;
    }
}
