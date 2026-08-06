using System.Text.RegularExpressions;
using MFBot.Languages;

namespace MFBot.Brain;

public enum Intent
{
    Greeting,
    Farewell,
    MathQuestion,
    YesNoQuestion,
    WhQuestion,
    Definition,
    Insult,
    Compliment,
    AboutBot,
    Command,
    Statement
}

public sealed record IntentResult(Intent Intent, string? Subject, double Confidence, Lang Lang)
{
    public static IntentResult Of(Intent intent, Lang lang, string? subject = null, double confidence = 1.0) =>
        new(intent, subject, confidence, lang);
}

/// <summary>
/// Kullanıcının ne sorduğunu anlar — ki bot tam olarak onun tersini söyleyebilsin.
///
/// Kelime listeleri artık dil paketlerinde (<see cref="LangPack"/>) ve hepsi
/// <see cref="TextKit.Normalize"/> geçirilmiş hâlde tutuluyor; karşılaştırma da
/// normalize edilmiş metin üzerinde TOKEN bazlı yapılıyor. Alt-dize eşleştirmesi
/// "sagol" kelimesini "sa" selamına takıyordu, o yüzden token bazlı.
/// </summary>
public static partial class IntentDetector
{
    /// <summary>
    /// "2+2" gibi boşluksuz ifadeler. Sayının önüne [-+]? KOYMA — "+2" tek sayı
    /// sanılıp operatör yutuluyor ve ifade çözülemiyor. Tekli eksiyi parser hallediyor.
    /// Arap-Hint rakamları da (٠-٩) kapsanır.
    /// </summary>
    [GeneratedRegex(@"[\d٠-٩۰-۹]+(?:[.,][\d٠-٩۰-۹]+)?\s*[+\-*/x×÷%^]\s*[\d٠-٩۰-۹]+")]
    private static partial Regex MathExpression();

    /// <summary>Önce dili tespit eder, sonra niyeti çıkarır.</summary>
    public static IntentResult Detect(string raw) =>
        Detect(raw, LanguageDetector.Detect(raw));

    /// <summary>Dil zaten belliyse niyeti çıkarır.</summary>
    public static IntentResult Detect(string raw, Lang lang)
    {
        var text = raw.Trim();
        if (text.Length == 0) return IntentResult.Of(Intent.Statement, lang);

        if (text.StartsWith('/') || text.StartsWith('!'))
            return IntentResult.Of(Intent.Command, lang);

        var pack = LangPacks.For(lang);

        var clean = TextKit.Clean(text, lang);   // noktalama duruyor, regex'ler bunu ister
        var norm = TextKit.Normalize(text);      // aksansız, noktalamasız, token eşleştirme için
        var tokens = norm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0) return IntentResult.Of(Intent.Statement, lang);

        // 1) Matematik — botun en komik olduğu yer, önceliği en yüksek
        if (MathExpression().IsMatch(text) || (pack.MathKeyword.IsMatch(norm) && HasDigit(text)))
            return IntentResult.Of(Intent.MathQuestion, lang, text, 0.95);

        // 2) Tanım sorusu: "X nedir?" — konu ADI aksanlı hâliyle alınmalı
        var definitionMatch = pack.DefinitionPattern.Match(clean);
        if (definitionMatch.Success)
        {
            var subject = definitionMatch.Groups[1].Value.Trim(' ', '"', '\'', '،');
            if (subject.Length > 0)
                return IntentResult.Of(Intent.Definition, lang, subject, 0.9);
        }

        // 3) Selam — sadece ilk kelimeye bakılır ve mesaj kısa olmalı.
        // Sınır 5: "hey mate how are you" tam 5 kelime ve düpedüz selam,
        // ama uzun mesajlar "selam" ile başlasa bile selam sayılmamalı.
        if (tokens.Length <= 5 && IsGreetingOpener(tokens[0], norm, pack))
            return IntentResult.Of(Intent.Greeting, lang, null, 0.85);

        if (HasToken(norm, tokens, pack.Farewells))
            return IntentResult.Of(Intent.Farewell, lang, null, 0.8);

        // 4) Hakaret / iltifat
        var insultScore = CountTokens(norm, tokens, pack.InsultMarkers);
        var complimentScore = CountTokens(norm, tokens, pack.ComplimentMarkers);

        if (insultScore > 0 && insultScore >= complimentScore)
            return IntentResult.Of(Intent.Insult, lang, null, Math.Min(1.0, 0.5 + insultScore * 0.2));

        if (complimentScore > 0)
            return IntentResult.Of(Intent.Compliment, lang, null, 0.75);

        if (HasToken(norm, tokens, pack.BotMentions))
            return IntentResult.Of(Intent.AboutBot, lang, null, 0.75);

        // 5) Soru tipi
        var looksLikeQuestion = text.Contains('?') || text.Contains('؟') || pack.YesNoPattern.IsMatch(clean);

        if (HasToken(norm, tokens, pack.WhWordList))
            return IntentResult.Of(Intent.WhQuestion, lang, Tokenizer.TopicWord(text, lang),
                looksLikeQuestion ? 0.85 : 0.6);

        if (looksLikeQuestion)
            return IntentResult.Of(Intent.YesNoQuestion, lang, Tokenizer.TopicWord(text, lang), 0.8);

        return IntentResult.Of(Intent.Statement, lang, Tokenizer.TopicWord(text, lang), 0.5);
    }

    /// <summary>Kullanıcının hangi soru kelimesini kullandığını verir (normalize edilmiş hâliyle).</summary>
    public static string? QuestionWord(string text, Lang lang)
    {
        var pack = LangPacks.For(lang);
        var norm = TextKit.Normalize(text);
        var tokens = norm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Uzun kalıplar önce denenmeli: "ne zaman" varken "ne" yakalanmasın
        foreach (var word in pack.WhWordList.OrderByDescending(w => w.Length))
        {
            if (word.Contains(' '))
            {
                if (norm.Contains(word, StringComparison.Ordinal)) return word;
                continue;
            }

            if (tokens.Any(t => TextKit.TokenMatches(t, word, 5))) return word;
        }

        return null;
    }

    /// <summary>Soru kelimesinin türü — hangi uydurma cevap bankası kullanılacak.</summary>
    public static WhKind QuestionKind(string text, Lang lang) =>
        LangPacks.For(lang).KindOf(QuestionWord(text, lang));

    /// <summary>
    /// Mesajda küfür var mı — kin sistemi ve küfür öğrenme bunu kullanır.
    /// Bütün dillere birden bakar: Türkçe yazarken araya "fuck" sıkıştıran da kin toplasın.
    /// </summary>
    public static bool ContainsProfanity(string text)
    {
        var flat = TextKit.Deobfuscate(text);
        var tokens = flat.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return HasToken(flat, tokens, LangPacks.AllInsultMarkers);
    }

    // ------------------------------------------------------------- yardımcılar

    private static bool HasDigit(string text)
    {
        foreach (var ch in text)
            if (char.IsDigit(ch))
                return true;

        return false;
    }

    private static bool IsGreetingOpener(string firstToken, string norm, LangPack pack) =>
        pack.Greetings.Any(g =>
            g == firstToken
            || (g.Length >= 4 && firstToken.StartsWith(g, StringComparison.Ordinal))
            || (g.Contains(' ') && norm.StartsWith(g, StringComparison.Ordinal)));

    /// <summary>
    /// Tam kelime eşleşmesi; 5 harften uzun köklerde ön ek eşleşmesi de kabul
    /// ("salak" -> "salaksin"). Kısa kökler ("mal", "got") sadece tam eşleşir,
    /// yoksa "malzeme" ve "goturmek" yanlış alarm çalar.
    /// </summary>
    private static bool HasToken(string norm, string[] tokens, IEnumerable<string> needles) =>
        TextKit.ContainsAny(norm, tokens, needles, minPrefixLength: 5);

    private static int CountTokens(string norm, string[] tokens, IEnumerable<string> needles) =>
        needles.Count(needle => HasToken(norm, tokens, new[] { needle }));
}
