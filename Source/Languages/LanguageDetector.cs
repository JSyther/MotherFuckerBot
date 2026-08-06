using MFBot.Brain;

namespace MFBot.Languages;

/// <summary>
/// Mesajın hangi dilde yazıldığını tahmin eder. Bot cevabı bu dilde verir.
///
/// Sıralama önemli: Arap alfabesi görünürse iş biter, tartışma yok. Latin
/// alfabesindeyse Türkçe ile İngilizce arasında puanlama yapılır.
///
/// Kararsız kalırsa (örn. "ok", "2+2", "hmm") tahmin ETMEZ, çağıranın verdiği
/// yedeğe döner — o da genelde kullanıcının son konuştuğu dildir. Sebebi basit:
/// tek kelimelik mesajda dil değiştirmek, sohbetin ortasında botun dilini
/// zıplatmaktan daha kötü bir hata.
/// </summary>
public static class LanguageDetector
{
    /// <summary>Sadece Türkçede olan harfler. Bir tanesi bile güçlü kanıt.</summary>
    private const string TurkishOnlyLetters = "ığşİĞŞ";

    /// <summary>Türkçede de var ama Türkçeye işaret eden harfler (Almanca/Fransızca da kullanır).</summary>
    private const string TurkishHintLetters = "çöüÇÖÜ";

    private static readonly HashSet<string> TurkishMarkers = new(StringComparer.Ordinal)
    {
        "bir", "bu", "su", "ben", "sen", "biz", "siz", "ne", "nasil", "neden", "niye",
        "kim", "nerede", "nerde", "hangi", "kac", "cok", "az", "ama", "fakat", "cunku",
        "gibi", "kadar", "icin", "ile", "degil", "var", "yok", "evet", "hayir", "tamam",
        "iyi", "kotu", "guzel", "simdi", "sonra", "once", "bugun", "yarin", "dun",
        "selam", "merhaba", "naber", "nabersin", "tesekkurler", "tesekkur", "sagol",
        "lan", "ulan", "amk", "abi", "kanka", "moruk", "hacı", "haci", "yani", "iste",
        "sadece", "belki", "bence", "seni", "sana", "beni", "bana", "onu", "ona",
        "olur", "oldu", "yap", "yapma", "soyle", "biliyorum", "bilmiyorum", "anladim",
        "misin", "misiniz", "musun", "mudur", "midir", "diye", "hep", "her", "biraz",
        "nedir", "kimdir", "nereye", "nereden", "hangi", "kacinci", "eder", "yapar",
        "olan", "olarak", "zaten", "bile", "hem", "galiba", "herhalde", "tabii",
        "sanki", "yine", "hala", "artik", "biraz", "cunki", "ise", "ki"
    };

    /// <summary>
    /// İngilizce işaretçileri. Türkçede aynı yazılan kelimeler BİLEREK yok:
    /// "is" (Türkçe "iş"), "on" (on), "at" (at), "an" (an), "can" (can), "her" (her),
    /// "it" (it), "am" — bunlar dili yanlış tarafa çeker.
    /// </summary>
    private static readonly HashSet<string> EnglishMarkers = new(StringComparer.Ordinal)
    {
        "the", "and", "you", "your", "youre", "are", "was", "were", "what", "why",
        "how", "who", "where", "when", "which", "this", "that", "these", "those",
        "with", "from", "have", "has", "had", "will", "would", "should", "could",
        "do", "does", "did", "dont", "doesnt", "not", "yes", "no", "please", "sorry",
        "thanks", "thank", "hello", "hey", "hi", "bye", "good", "bad", "know", "think",
        "tell", "say", "said", "make", "made", "give", "want", "need", "like",
        "because", "about", "there", "their", "they", "them", "we", "me", "my",
        "im", "ive", "its", "just", "really", "very", "much", "many", "some",
        "shit", "fuck", "fucking", "stupid", "dumb", "idiot", "bitch", "damn",
        // "got" BİLEREK yok: Türkçe "göt" normalize olunca "got" oluyor.
        "to", "of", "for", "in", "so", "up", "out", "get", "going", "gonna"
    };

    /// <summary>Türkçe çekim ekleri — kelime bazlı işaretçiler yetmezse morfoloji konuşur.</summary>
    private static readonly string[] TurkishSuffixes =
    {
        "iyor", "yorum", "yorsun", "mak", "mek", "lar", "ler", "dir", "dur",
        "sin", "sun", "acak", "ecek", "mis", "mus", "dan", "den", "tan", "ten",
        "nin", "nun", "lik", "luk", "ci", "cu", "siniz", "sunuz", "ligi"
    };

    /// <summary>İngilizce çekimleri.</summary>
    private static readonly string[] EnglishSuffixes =
    {
        "ing", "tion", "ness", "ment", "able", "ible", "ould", "ally"
    };

    /// <summary>Mesajın dilini tahmin eder; emin olamazsa <paramref name="fallback"/> döner.</summary>
    public static Lang Detect(string text, Lang fallback = Lang.Tr)
    {
        var result = Analyse(text, fallback);
        return result.Lang;
    }

    /// <summary>Tahmin + ne kadar emin olduğu. 0 = hiç emin değil, yedeğe düştü.</summary>
    public static (Lang Lang, int Confidence) Analyse(string text, Lang fallback = Lang.Tr)
    {
        if (string.IsNullOrWhiteSpace(text)) return (fallback, 0);

        // 1) Yazı sistemi — Arapça harf varsa tartışma bitti
        var arabicLetters = 0;
        var latinLetters = 0;

        foreach (var ch in text)
        {
            if (TextKit.IsArabicMark(ch)) continue;

            if (TextKit.IsArabic(ch) && char.IsLetter(ch)) arabicLetters++;
            else if (char.IsLetter(ch)) latinLetters++;
        }

        if (arabicLetters > 0 && arabicLetters >= latinLetters)
            return (Lang.Ar, Math.Min(100, 60 + arabicLetters * 4));

        if (latinLetters == 0) return (fallback, 0);

        // 2) Latin alfabesi: Türkçe mi İngilizce mi
        var turkish = 0;
        var english = 0;

        foreach (var ch in text)
        {
            if (TurkishOnlyLetters.Contains(ch)) turkish += 5;
            else if (TurkishHintLetters.Contains(ch)) turkish += 3;
        }

        var normalized = TextKit.Normalize(text);
        var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            if (TurkishMarkers.Contains(token)) turkish += 6;
            if (EnglishMarkers.Contains(token)) english += 6;

            // Türkçede neredeyse hiç kullanılmayan harfler. Tek harflik token'da
            // sayılmaz — "x nedir" matematik değişkeni yüzünden İngilizce sanılmasın.
            if (token.Length >= 2)
                foreach (var ch in token)
                    if (ch is 'w' or 'x' or 'q')
                        english += 2;

            if (token.Length < 5) continue;

            foreach (var suffix in TurkishSuffixes)
                if (token.EndsWith(suffix, StringComparison.Ordinal)) { turkish += 2; break; }

            foreach (var suffix in EnglishSuffixes)
                if (token.EndsWith(suffix, StringComparison.Ordinal)) { english += 2; break; }
        }

        // İngilizcede sık, Türkçede neredeyse hiç olmayan harf ikilileri
        if (normalized.Contains("th", StringComparison.Ordinal)) english += 3;
        if (normalized.Contains("wh", StringComparison.Ordinal)) english += 2;

        if (turkish == 0 && english == 0) return (fallback, 0);

        return turkish >= english
            ? (Lang.Tr, Math.Min(100, turkish - english + 20))
            : (Lang.En, Math.Min(100, english - turkish + 20));
    }
}
