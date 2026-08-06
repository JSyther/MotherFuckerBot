namespace MFBot.Languages;

/// <summary>
/// Botun konuşabildiği diller.
///
/// DİKKAT: namespace <c>MFBot.Languages</c>, tip <c>Lang</c>. Namespace'e "Lang"
/// dersek <c>MFBot.Lang.Lang</c> çakışması (CS0118) çıkar — aynı tuzağa
/// MotherFuckerBot sınıfında bir kere düşüldü, tekrarlama.
/// </summary>
public enum Lang
{
    /// <summary>Türkçe — botun ana dili.</summary>
    Tr,

    /// <summary>İngilizce.</summary>
    En,

    /// <summary>Arapça.</summary>
    Ar
}

public static class LangInfo
{
    public static readonly Lang[] All = { Lang.Tr, Lang.En, Lang.Ar };

    /// <summary>Diske ve ayar dosyasına yazılan iki harfli kod.</summary>
    public static string Code(this Lang lang) => lang switch
    {
        Lang.En => "en",
        Lang.Ar => "ar",
        _ => "tr"
    };

    /// <summary>Konsolda gösterilecek ad.</summary>
    public static string Display(this Lang lang) => lang switch
    {
        Lang.En => "english",
        Lang.Ar => "العربية",
        _ => "türkçe"
    };

    /// <summary>Sağdan sola yazılan dil mi (Arapça).</summary>
    public static bool IsRightToLeft(this Lang lang) => lang == Lang.Ar;

    public static bool TryParse(string? code, out Lang lang)
    {
        switch (code?.Trim().ToLowerInvariant())
        {
            case "tr" or "tur" or "turkce" or "türkçe" or "turkish":
                lang = Lang.Tr; return true;
            case "en" or "eng" or "ing" or "ingilizce" or "english":
                lang = Lang.En; return true;
            case "ar" or "ara" or "arapca" or "arapça" or "arabic" or "عربي" or "العربية":
                lang = Lang.Ar; return true;
            default:
                lang = Lang.Tr; return false;
        }
    }

    public static Lang Parse(string? code, Lang fallback = Lang.Tr) =>
        TryParse(code, out var lang) ? lang : fallback;
}
