using System.Globalization;
using MFBot.Languages;

namespace MFBot.Brain;

/// <summary>
/// Türkçeye ÖZEL metin yardımcıları: ünlü uyumu ve ek çekimi.
/// Bu sınıf sayesinde bot "araba'a" değil "arabaya" diyor.
///
/// Dilden bağımsız işler (normalize, küçültme, benzerlik) <see cref="TextKit"/> içine
/// taşındı — İngilizce metni tr-TR ile küçültünce "I" -> "ı" oluyordu.
/// Buradaki Normalize / Deobfuscate / Similarity sadece eski çağrılar için ince kabuk.
/// </summary>
public static class TurkishText
{
    public static readonly CultureInfo Tr = TextKit.TrCulture;

    private const string Vowels = "aeıioöuü";
    private const string BackVowels = "aıou";      // kalın
    private const string RoundedVowels = "ouöü";   // yuvarlak
    private const string HardConsonants = "fstkçşhp";

    /// <summary>Türkçe kurallarına göre küçük harfe çevirir (I -> ı, İ -> i).</summary>
    public static string Lower(string s) => TextKit.Lower(s, Lang.Tr);

    public static string Upper(string s) => TextKit.Upper(s, Lang.Tr);

    /// <summary>Karşılaştırma için normalize eder. Bkz. <see cref="TextKit.Normalize"/>.</summary>
    public static string Normalize(string s) => TextKit.Normalize(s);

    /// <summary>Harfi eşleştirilebilir sade biçimine indirger.</summary>
    public static char Deaccent(char ch) => TextKit.Fold(ch);

    /// <summary>Leet-speak / kaçamak yazımları sadeleştirir.</summary>
    public static string Deobfuscate(string s) => TextKit.Deobfuscate(s);

    /// <summary>İki metin arasındaki token bazlı benzerlik (0..1).</summary>
    public static double Similarity(string a, string b) => TextKit.Similarity(a, b);

    // ---------------------------------------------------------------- ünlü uyumu

    /// <summary>Kelimenin son ünlüsünü döndürür, yoksa '\0'.</summary>
    public static char LastVowel(string word)
    {
        var w = Lower(word);
        for (var i = w.Length - 1; i >= 0; i--)
            if (Vowels.Contains(w[i]))
                return w[i];
        return '\0';
    }

    public static char LastLetter(string word)
    {
        var w = Lower(word).TrimEnd();
        return w.Length == 0 ? '\0' : w[^1];
    }

    public static bool EndsWithVowel(string word) => Vowels.Contains(LastLetter(word));

    public static bool EndsWithHardConsonant(string word) => HardConsonants.Contains(LastLetter(word));

    /// <summary>Büyük ünlü uyumu: a/e seçer.</summary>
    public static string TwoWay(string word)
    {
        var v = LastVowel(word);
        if (v == '\0') return "e";
        return BackVowels.Contains(v) ? "a" : "e";
    }

    /// <summary>Küçük ünlü uyumu: ı/i/u/ü seçer.</summary>
    public static string FourWay(string word)
    {
        var v = LastVowel(word);
        if (v == '\0') return "i";

        var back = BackVowels.Contains(v);
        var rounded = RoundedVowels.Contains(v);

        return (back, rounded) switch
        {
            (true, false) => "ı",
            (true, true) => "u",
            (false, false) => "i",
            (false, true) => "ü"
        };
    }

    // ---------------------------------------------------------------- ek çekimi

    /// <summary>Yönelme hâli: "araba" -> "arabaya", "ev" -> "eve".</summary>
    public static string Dative(string word)
    {
        var buffer = EndsWithVowel(word) ? "y" : "";
        return word + buffer + TwoWay(word);
    }

    /// <summary>Belirtme hâli: "araba" -> "arabayı", "ev" -> "evi".</summary>
    public static string Accusative(string word)
    {
        var buffer = EndsWithVowel(word) ? "y" : "";
        return word + buffer + FourWay(word);
    }

    /// <summary>Bulunma hâli: "araba" -> "arabada", "kitap" -> "kitapta".</summary>
    public static string Locative(string word)
    {
        var d = EndsWithHardConsonant(word) ? "t" : "d";
        return word + d + TwoWay(word);
    }

    /// <summary>Ayrılma hâli: "araba" -> "arabadan", "kitap" -> "kitaptan".</summary>
    public static string Ablative(string word)
    {
        var d = EndsWithHardConsonant(word) ? "t" : "d";
        return word + d + TwoWay(word) + "n";
    }

    /// <summary>İyelik (3. tekil): "araba" -> "arabası", "ev" -> "evi".</summary>
    public static string Possessive3(string word)
    {
        var buffer = EndsWithVowel(word) ? "s" : "";
        return word + buffer + FourWay(word);
    }

    /// <summary>İyelik (2. tekil): "araba" -> "araban", "ev" -> "evin".</summary>
    public static string Possessive2(string word)
    {
        if (EndsWithVowel(word)) return word + "n";
        return word + FourWay(word) + "n";
    }

    /// <summary>Tamlayan hâli: "araba" -> "arabanın", "ev" -> "evin".</summary>
    public static string Genitive(string word)
    {
        var buffer = EndsWithVowel(word) ? "n" : "";
        return word + buffer + FourWay(word) + "n";
    }

    /// <summary>Çoğul: "araba" -> "arabalar", "ev" -> "evler".</summary>
    public static string Plural(string word) => word + "l" + TwoWay(word) + "r";

    /// <summary>"-lı/-li/-lu/-lü" sıfat eki: "yarrak" -> "yarraklı".</summary>
    public static string WithSuffixLi(string word) => word + "l" + FourWay(word);

    /// <summary>"-sın/-sin/-sun/-sün": "salak" -> "salaksın".</summary>
    public static string YouAre(string word) => word + "s" + FourWay(word) + "n";

    /// <summary>İlk harfi büyütür (Türkçe kurallarıyla).</summary>
    public static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return Upper(s[..1]) + s[1..];
    }
}
