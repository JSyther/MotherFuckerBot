using System.Globalization;
using System.Text;
using MFBot.Languages;

namespace MFBot.Brain;

/// <summary>
/// Çok dilli metin çekirdeği: küçültme, normalize etme, kaçamak yazımı sadeleştirme.
///
/// Üç yazı sistemi var ve üçünün de tuzağı farklı:
///   - Türkçe: I/İ ayrımı tr-TR kültürü ister.
///   - İngilizce: tr-TR ile küçültülürse "I" -> "ı" olur, bot "ı am" der. Rezalet.
///   - Arapça: hareke (tashkeel), tatweel ve elif/ye/te-marbuta varyantları
///     temizlenmezse "مرحباً" ile "مرحبا" farklı kelime sanılır.
///
/// <see cref="Normalize"/> çıktısı SADECE eşleştirme içindir, kullanıcıya gösterilmez.
/// </summary>
public static class TextKit
{
    public static readonly CultureInfo TrCulture = CultureInfo.GetCultureInfo("tr-TR");

    // ---------------------------------------------------------------- küçük/büyük harf

    /// <summary>Dile göre küçük harf. Türkçe dışında tr-TR kullanmak yasak (I -> ı olur).</summary>
    public static string Lower(string s, Lang lang) =>
        lang == Lang.Tr ? s.ToLower(TrCulture) : s.ToLowerInvariant();

    /// <summary>Dile göre büyük harf. Arapçada büyük harf yok, metin aynen döner.</summary>
    public static string Upper(string s, Lang lang) => lang switch
    {
        Lang.Tr => s.ToUpper(TrCulture),
        Lang.Ar => s,
        _ => s.ToUpperInvariant()
    };

    /// <summary>İlk harfi büyütür. Arapçada büyük harf yok, metin aynen döner.</summary>
    public static string Capitalize(string s, Lang lang)
    {
        if (string.IsNullOrEmpty(s) || lang == Lang.Ar) return s;
        return Upper(s[..1], lang) + s[1..];
    }

    // ---------------------------------------------------------------- Arapça yardımcıları

    /// <summary>Karakter Arap alfabesi bloklarından birinde mi.</summary>
    public static bool IsArabic(char ch) =>
        (ch >= '؀' && ch <= 'ۿ') ||   // Arabic
        (ch >= 'ݐ' && ch <= 'ݿ') ||   // Arabic Supplement
        (ch >= 'ࢠ' && ch <= 'ࣿ') ||   // Arabic Extended-A
        (ch >= 'ﭐ' && ch <= '﷿') ||   // Presentation Forms-A
        (ch >= 'ﹰ' && ch <= '﻿');     // Presentation Forms-B

    /// <summary>Hareke, tatweel ve benzeri — normalize ederken tamamen atılır.</summary>
    public static bool IsArabicMark(char ch) =>
        (ch >= 'ً' && ch <= 'ٟ') ||   // tashkeel (hareke)
        ch == 'ـ' ||                       // tatweel — uzatma çizgisi
        ch == 'ٰ' ||                       // hançer elif
        (ch >= 'ۖ' && ch <= 'ۭ');     // Kur'an işaretleri

    public static bool HasArabic(string s)
    {
        foreach (var ch in s)
            if (IsArabic(ch) && !IsArabicMark(ch))
                return true;

        return false;
    }

    /// <summary>
    /// Arapçada ekler kelimenin BAŞINA da gelir (ال، و، ب، ك، ل، ف). Ön ek eşleştirmesi
    /// bunları görmezse "الكلب" kelimesi "كلب" köküne takılmaz.
    /// </summary>
    public static string StripArabicClitics(string token)
    {
        var t = token;

        // Önce bağlaç/edat + belirlilik takısı ("وال", "بال", "كال", "فال", "لل")
        string[] compound = { "وال", "بال", "كال", "فال", "لل" };
        foreach (var prefix in compound)
        {
            if (t.Length > prefix.Length + 1 && t.StartsWith(prefix, StringComparison.Ordinal))
                return t[prefix.Length..];
        }

        if (t.Length > 3 && t.StartsWith("ال", StringComparison.Ordinal))
            return t[2..];

        if (t.Length > 3 && (t[0] is 'و' or 'ف' or 'ب' or 'ك' or 'ل'))
            return t[1..];

        return t;
    }

    /// <summary>
    /// Regex'lerin çalıştığı ara biçim: dile göre küçük harf + Arapça hareke/varyant
    /// temizliği, ama noktalama ve Türkçe aksanlar KORUNUR.
    ///
    /// <see cref="Normalize"/> noktalamayı da attığı için soru kalıpları ("... mı?",
    /// "is it ...?") orada çalışmaz. Bu yüzden ayrı bir biçim lazım.
    /// </summary>
    public static string Clean(string s, Lang lang)
    {
        var lowered = Lower(s, lang);
        if (lang != Lang.Ar && !HasArabic(lowered)) return lowered;

        var sb = new StringBuilder(lowered.Length);
        foreach (var ch in lowered)
        {
            if (IsArabicMark(ch)) continue;
            sb.Append(IsArabic(ch) ? Fold(ch) : ch);
        }

        return sb.ToString();
    }

    // ---------------------------------------------------------------- normalize

    /// <summary>
    /// Eşleştirme için sadeleştirir: küçük harf, aksansız, harekesiz, noktalamasız.
    /// Türkçe küçültme kullanılır çünkü hem "I" hem "İ" sonunda "i" olur — bu üç dilde de doğru.
    /// </summary>
    public static string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;

        var lowered = s.Trim().ToLower(TrCulture);
        var sb = new StringBuilder(lowered.Length);

        foreach (var ch in lowered)
        {
            if (IsArabicMark(ch)) continue;                     // hareke ve tatweel komple atılır

            // Kesme işareti kelimeyi BÖLMEZ, silinir: "don't" -> "dont", "Türkiye'nin" -> "turkiyenin".
            // Boşluğa çevirseydik İngilizce "what's" iki token olur, hiçbir listeye takılmazdı.
            if (ch is '\'' or '’' or '`' or '´') continue;

            if (char.IsLetterOrDigit(ch) || ch == ' ')
                sb.Append(Fold(ch));
            else if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch))
                sb.Append(' ');
        }

        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>Harfi eşleştirilebilir sade biçimine indirger (Türkçe aksan + Arapça varyant + Arap rakamı).</summary>
    public static char Fold(char ch) => ch switch
    {
        // Türkçe
        'ı' => 'i',
        'ğ' => 'g',
        'ü' => 'u',
        'ş' => 's',
        'ö' => 'o',
        'ç' => 'c',
        'â' => 'a',
        'î' => 'i',
        'û' => 'u',

        // Arapça harf varyantları
        'أ' or 'إ' or 'آ' or 'ٱ' or 'ٲ' or 'ٳ' => 'ا',
        'ى' or 'ئ' or 'ی' => 'ي',
        'ة' => 'ه',
        'ؤ' => 'و',
        'ک' => 'ك',

        _ => AsciiDigit(ch)
    };

    /// <summary>Arap-Hint rakamlarını ASCII'ye çevirir (٥ -> 5), diğerlerini aynen bırakır.</summary>
    private static char AsciiDigit(char ch)
    {
        if (ch >= '٠' && ch <= '٩') return (char)('0' + (ch - '٠'));
        if (ch >= '۰' && ch <= '۹') return (char)('0' + (ch - '۰'));
        return ch;
    }

    /// <summary>Metindeki Arap-Hint rakamlarını ASCII rakama çevirir. Matematik ayrıştırıcı bunu kullanır.</summary>
    public static string ToAsciiDigits(string s)
    {
        if (s.All(ch => ch < '٠')) return s;

        var sb = new StringBuilder(s.Length);
        foreach (var ch in s) sb.Append(AsciiDigit(ch));
        return sb.ToString();
    }

    /// <summary>
    /// ASCII rakamları Arap-Hint rakamına çevirir (5 -> ٥). Arapça cevabın ortasındaki
    /// "42" doğru ama yabancı duruyor; bot o dilde konuşuyorsa sayıyı da o dilde yazsın.
    /// </summary>
    public static string ToArabicDigits(string s)
    {
        if (!s.Any(char.IsAsciiDigit)) return s;

        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
            sb.Append(char.IsAsciiDigit(ch) ? (char)('٠' + (ch - '0')) : ch);

        return sb.ToString();
    }

    /// <summary>Leet-speak / kaçamak yazımları sadeleştirir: "s1k1m" -> "sikim", "aaammk" -> "amk".</summary>
    public static string Deobfuscate(string s)
    {
        var normalized = Normalize(s);
        var sb = new StringBuilder(normalized.Length);
        var previous = '\0';

        foreach (var raw in normalized)
        {
            var ch = raw switch
            {
                '0' => 'o',
                '1' => 'i',
                '3' => 'e',
                '4' => 'a',
                '5' => 's',
                '7' => 't',
                '8' => 'b',
                '@' => 'a',
                '$' => 's',
                _ => raw
            };

            if (ch == previous) continue;   // aaaammmk -> amk
            sb.Append(ch);
            previous = ch;
        }

        return sb.ToString();
    }

    // ---------------------------------------------------------------- eşleştirme

    /// <summary>
    /// Token bir köke uyuyor mu. Tam eşleşme her zaman geçerli; ön ek eşleşmesi
    /// sadece kök yeterince uzunsa ("mal" -> "malzeme" yanlış alarmı olmasın).
    /// Arapça köklerde token'ın başındaki ال / و / ب gibi ekler soyulup tekrar denenir.
    /// </summary>
    public static bool TokenMatches(string token, string needle, int minPrefixLength)
    {
        if (token.Length == 0 || needle.Length == 0) return false;
        if (token == needle) return true;

        if (needle.Length >= minPrefixLength && token.StartsWith(needle, StringComparison.Ordinal))
            return true;

        if (!HasArabic(needle)) return false;

        var stripped = StripArabicClitics(token);
        if (stripped == token) return false;

        return stripped == needle ||
               (needle.Length >= minPrefixLength && stripped.StartsWith(needle, StringComparison.Ordinal));
    }

    /// <summary>
    /// Normalize edilmiş metinde köklerden biri geçiyor mu.
    /// Boşluklu kökler alt-dize, tek kelimelikler token bazlı aranır.
    /// </summary>
    public static bool ContainsAny(string normalized, string[] tokens, IEnumerable<string> needles, int minPrefixLength)
    {
        foreach (var needle in needles)
        {
            if (needle.Length == 0) continue;

            if (needle.Contains(' '))
            {
                if (normalized.Contains(needle, StringComparison.Ordinal)) return true;
                continue;
            }

            foreach (var token in tokens)
                if (TokenMatches(token, needle, minPrefixLength))
                    return true;
        }

        return false;
    }

    /// <summary>İki metin arasındaki token bazlı Jaccard benzerliği (0..1).</summary>
    public static double Similarity(string a, string b)
    {
        var setA = Normalize(a).Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
        var setB = Normalize(b).Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);

        if (setA.Count == 0 || setB.Count == 0) return 0;

        var intersection = setA.Intersect(setB).Count();
        var union = setA.Union(setB).Count();
        var jaccard = (double)intersection / union;

        // Biri diğerini tamamen kapsıyorsa bonus ver
        var containment = (double)intersection / Math.Min(setA.Count, setB.Count);

        return (jaccard * 0.6) + (containment * 0.4);
    }

    /// <summary>Bir liste kurulurken kökleri normalize eder — listeler doğal yazılabilsin diye.</summary>
    public static string[] Folded(params string[] words)
    {
        var result = new string[words.Length];
        for (var i = 0; i < words.Length; i++) result[i] = Normalize(words[i]);
        return result;
    }
}
