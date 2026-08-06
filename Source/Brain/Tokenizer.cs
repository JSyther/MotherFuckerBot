using System.Text;
using MFBot.Languages;

namespace MFBot.Brain;

/// <summary>
/// Kaba ama işini gören çok dilli tokenizer. Markov zinciri ve kelime dağarcığı bunu kullanır.
///
/// Dil parametresi şart: İngilizce metni tr-TR ile küçültürsen "I" -> "ı" olur ve bot
/// "ı am" demeye başlar. Arapçada küçük harf yok ama harekeler temizlenmeli.
/// </summary>
public static class Tokenizer
{
    private static readonly char[] SentenceEnders = { '.', '!', '?', ';', '\n', '؟', '؛' };

    /// <summary>Metni cümlelere böler.</summary>
    public static List<string> Sentences(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();

        return text
            .Split(SentenceEnders, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();
    }

    /// <summary>
    /// Cümleyi öğrenilebilir token'lara böler. Noktalama atılır, harf/rakam korunur.
    /// Arapça harekeler atılır (yoksa "مرحباً" ile "مرحبا" ayrı kelime sanılır),
    /// ama harflerin kendisi bozulmaz — bu çıktı kullanıcıya geri gösteriliyor.
    /// </summary>
    public static List<string> Tokenize(string text, Lang lang = Lang.Tr)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return tokens;

        var lowered = TextKit.Lower(text, lang);
        var sb = new StringBuilder();

        foreach (var ch in lowered)
        {
            if (TextKit.IsArabicMark(ch)) continue;   // hareke/tatweel: kelimeyi bölmeden atla

            if (char.IsLetterOrDigit(ch) || ch == '\'' || ch == '-')
            {
                sb.Append(ch);
            }
            else
            {
                if (sb.Length > 0) { tokens.Add(sb.ToString()); sb.Clear(); }
            }
        }

        if (sb.Length > 0) tokens.Add(sb.ToString());

        return tokens
            .Select(t => t.Trim('\'', '-'))
            .Where(t => t.Length > 0 && t.Length <= 32)
            .ToList();
    }

    /// <summary>Anlamlı kelimeler: çok kısa olanlar, saf sayılar ve bağlaçlar elenir.</summary>
    public static List<string> ContentWords(string text, Lang lang = Lang.Tr)
    {
        var stopWords = StopWordsFor(lang);

        return Tokenize(text, lang)
            .Where(t => t.Length >= 3)
            .Where(t => !t.All(char.IsDigit))
            .Where(t => !stopWords.Contains(TextKit.Normalize(t)))
            .ToList();
    }

    /// <summary>Kullanıcının cümlesindeki "konu" gibi duran en uzun kelimeyi seçer.</summary>
    public static string? TopicWord(string text, Lang lang = Lang.Tr)
    {
        var candidates = ContentWords(text, lang);
        if (candidates.Count == 0) return null;

        return candidates
            .OrderByDescending(w => w.Length)
            .ThenBy(w => w, StringComparer.Ordinal)
            .First();
    }

    /// <summary>
    /// Dilin bağlaç/zamir listesi — normalize edilmiş hâlde tutulur, böylece
    /// "için" de "icin" de elenir.
    /// </summary>
    public static HashSet<string> StopWordsFor(Lang lang)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(lang, out var cached)) return cached;

            var set = LangPacks.For(lang).StopWords
                .Select(TextKit.Normalize)
                .Where(w => w.Length > 0)
                .ToHashSet(StringComparer.Ordinal);

            Cache[lang] = set;
            return set;
        }
    }

    private static readonly Dictionary<Lang, HashSet<string>> Cache = new();

    /// <summary>Geriye dönük uyumluluk: dil verilmemiş eski çağrılar için Türkçe liste.</summary>
    public static HashSet<string> StopWords => StopWordsFor(Lang.Tr);
}
