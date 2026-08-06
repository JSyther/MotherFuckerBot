namespace MFBot.Languages;

/// <summary>
/// Dil paketi kayıt defteri. Yeni dil eklemek isteyen tek yere bakar: buraya.
/// </summary>
public static class LangPacks
{
    public static LangPack For(Lang lang) => lang switch
    {
        Lang.En => EnglishPack.Pack,
        Lang.Ar => ArabicPack.Pack,
        _ => TurkishPack.Pack
    };

    public static IEnumerable<LangPack> All => LangInfo.All.Select(For);

    /// <summary>
    /// Bütün dillerin küfür kökleri. Kullanıcı Türkçe yazarken araya İngilizce küfür
    /// sıkıştırırsa da yakalansın diye birleşik bakılır.
    ///
    /// DİKKAT: bu listeler <see cref="Brain.TextKit.Deobfuscate"/> geçirilmiş, çünkü
    /// karşılaştırılan metin de öyle geçiriliyor. O adım tekrar eden harfleri teke
    /// indirdiği için ("asshole" -> "ashole") sadece normalize edilmiş kök tutmaz.
    /// </summary>
    public static readonly string[] AllProfanityStems =
        All.SelectMany(p => p.ProfanityStems)
            .Select(Brain.TextKit.Deobfuscate)
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    /// <summary>Bütün dillerin hakaret işaretçileri — kin sistemi bunu kullanır.</summary>
    public static readonly string[] AllInsultMarkers =
        All.SelectMany(p => p.InsultMarkers)
            .Select(Brain.TextKit.Deobfuscate)
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
}
