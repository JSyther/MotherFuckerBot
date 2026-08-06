using MFBot.Languages;

namespace MFBot.Brain;

public enum GuardVerdict
{
    /// <summary>Sorun yok, öğren ve söyle.</summary>
    Clean,

    /// <summary>Öğrenilmesin ama sohbet devam etsin.</summary>
    DoNotLearn,

    /// <summary>Bot bu konuya hiç girmesin.</summary>
    Block
}

/// <summary>
/// Botun ağzı bozuk olsun diye yazıldı, ama <b>kendi kendine öğrenen</b> bir bot
/// kullanıcının yazdığı her boku ezberler. Bu sınıf o boku süzer:
/// ırk / etnik köken / din / cinsel yönelim / engellilik hedefleyen nefret dili
/// ve çocuk içeren cinsel içerik ne öğrenilir ne de botun ağzından çıkar.
///
/// Amaç ahlak dersi vermek değil: bot küfürbaz kalsın ama seni sunucudan
/// ban yedirmesin, başına iş açmasın.
///
/// Genel küfür (amk, orospu çocuğu, fuck off, يا حمار) BURADA ENGELLENMEZ.
/// Botun tüm olayı o zaten. Süzgeç ÜÇ DİLDE BİRDEN çalışır — kullanıcı Türkçe
/// yazarken araya İngilizce nefret söylemi sıkıştıramasın.
///
/// Ek terim eklemek için: Data/blocklist.txt (satır başına bir terim, # yorum).
/// </summary>
public sealed class ContentGuard
{
    // ---------------------------------------------------------------- Türkçe listeler

    private static readonly string[] SlurTr =
    {
        // cinsel yönelim / cinsel kimlik
        "ibne", "ibno", "nonos", "gotveren", "gotoglan",
        // engellilik
        "mongol", "ozurlu", "otistik", "sakat kafali",
        // etnik / milli
        "kiro", "cingene", "cingen", "gavur",
        // din
        "dinsiz kopek", "allahsiz kopek", "kafir kopek"
    };

    private static readonly string[] IdentityTr =
    {
        "ermeni", "ermeniler", "rum", "rumlar", "arap", "araplar", "suriyeli", "suriyeliler",
        "afgan", "afganlar", "roman", "romanlar", "cerkez", "kurtler", "kurtlere", "kurdistan",
        "alevi", "aleviler", "sunni", "sunniler", "musluman", "muslumanlar", "hristiyan",
        "hristiyanlar", "yahudi", "yahudiler", "ateist", "ateistler", "kafir", "kafirler",
        "gay", "gayler", "lezbiyen", "lezbiyenler", "escinsel", "escinseller",
        "trans", "translar", "biseksuel", "lgbt", "lgbti",
        "engelli", "engelliler", "otizmli", "otizm", "down sendromlu", "felcli",
        "zenci", "zenciler", "siyahi", "siyahiler", "multeci", "multeciler", "gocmen", "gocmenler"
    };

    private static readonly string[] HateTr =
    {
        "dolu", "dollari", "pisligi", "gebersin", "gebersinler", "olmeli", "olsunlar",
        "defol", "defolun", "nefret", "temizlemek", "yakalim", "asalim", "kovalim",
        "istemiyoruz", "hepsi", "irki", "soyu"
    };

    private static readonly string[] MinorTr =
    {
        "kucuk kiz", "kucuk oglan", "kucuk cocuk", "yasinda kiz", "yasinda oglan",
        "yasindaki kiz", "yasindaki cocuk", "resit olmayan", "resit degil",
        "ilkokul ogrenci", "ortaokul ogrenci", "loli", "cocuk pornos"
    };

    private static readonly string[] SexualTr =
    {
        "sikis", "sikme", "seks", "porno", "cinsel", "ciplak", "azgin",
        "tecavuz", "mastur", "orgazm", "sapik"
    };

    // ---------------------------------------------------------------- İngilizce listeler

    private static readonly string[] SlurEn =
    {
        // cinsel yönelim / cinsel kimlik
        "faggot", "fagot", "tranny", "trannie", "shemale",
        // ırk / etnik
        "nigger", "nigga", "kike", "chink", "wetback", "gook", "raghead", "towelhead",
        "sandnigger", "beaner", "gyppo", "pikey",
        // engellilik
        "retard", "spastic", "mongoloid", "autist"
    };

    /// <summary>
    /// Ön ek eşleştirmesi yapılmayan, sadece TAM eşleşen küfürler.
    /// Sebep: "spic" -> "spice", "paki" -> "pakistan", "coon" -> "cocoon" gibi
    /// masum kelimeleri bloklamayalım.
    /// </summary>
    private static readonly string[] SlurExactEn =
    {
        "spic", "spics", "paki", "pakis", "coon", "coons", "tard", "tards",
        "dyke", "dykes", "cripple", "cripples", "abo", "abos"
    };

    private static readonly string[] IdentityEn =
    {
        "muslim", "muslims", "christian", "christians", "jew", "jews", "jewish",
        "hindu", "hindus", "buddhist", "buddhists", "atheist", "atheists",
        "arab", "arabs", "kurd", "kurds", "armenian", "armenians", "turk", "turks",
        "syrian", "syrians", "afghan", "afghans", "mexican", "mexicans",
        "immigrant", "immigrants", "refugee", "refugees", "migrant", "migrants",
        "blacks", "whites", "asians", "latino", "latinos", "hispanic", "hispanics",
        "gay", "gays", "lesbian", "lesbians", "queer", "queers", "bisexual",
        "trans", "transgender", "lgbt", "lgbtq",
        "disabled", "handicapped", "autistic", "autism", "deaf", "blind people",
        "down syndrome", "wheelchair user"
    };

    private static readonly string[] HateEn =
    {
        "should die", "must die", "deserve to die", "kill them", "kill all",
        "gas them", "deport", "deported", "vermin", "subhuman", "scum", "filth",
        "go home", "get out", "all of them", "inferior", "exterminate", "invaders",
        "infest", "parasites", "hate", "disgusting race", "breed like"
    };

    private static readonly string[] MinorEn =
    {
        "little girl", "little boy", "young girl", "young boy", "underage",
        "year old girl", "year old boy", "yr old girl", "yr old boy",
        "schoolgirl", "schoolboy", "preteen", "pre teen", "loli", "lolita",
        "child porn", "kid porn", "toddler", "not of age", "below the age"
    };

    private static readonly string[] SexualEn =
    {
        "sex", "sexual", "sexy", "porn", "porno", "nude", "naked", "horny",
        "rape", "raping", "masturbat", "orgasm", "fetish", "erotic", "aroused"
    };

    // ---------------------------------------------------------------- Arapça listeler

    private static readonly string[] SlurAr =
    {
        "شواذ", "لوطي", "مخنث", "منغولي", "متخلف عقليا", "زنجي", "زنوج"
    };

    private static readonly string[] IdentityAr =
    {
        "مسلم", "مسلمين", "مسلمون", "مسيحي", "مسيحيين", "يهودي", "يهود",
        "درزي", "دروز", "علوي", "علويين", "سني", "سنه", "شيعي", "شيعه",
        "كردي", "اكراد", "امازيغي", "قبطي", "اقباط", "كافر", "كفار",
        "لاجي", "لاجيين", "مهاجر", "مهاجرين", "سوري", "سوريين",
        "مصري", "مصريين", "هندي", "افريقي", "مثلي", "مثليين", "متحول جنسيا",
        "معاق", "معاقين", "ذوي الاحتياجات", "توحد", "متلازمه داون"
    };

    private static readonly string[] HateAr =
    {
        "اقتلوا", "يموتوا", "يستاهلوا الموت", "لازم يموتوا", "اطردوا", "برا من البلد",
        "نكرههم", "حثاله", "قذاره", "ارحلوا", "كلهم", "عرقهم", "نسلهم",
        "انقرضوا", "اعدموا", "ما بدنا ياهم", "نجس"
    };

    private static readonly string[] MinorAr =
    {
        "بنت صغيره", "ولد صغير", "طفل صغير", "قاصر", "قاصرات", "تحت السن",
        "دون السن", "عمرها سنه", "عمره سنه", "تلميذه صغيره", "اطفال"
    };

    private static readonly string[] SexualAr =
    {
        "جنس", "جنسي", "جنسيه", "اباحي", "اباحيه", "عاري", "عاريه",
        "اغتصاب", "شهوه", "سكس", "بورن", "نيك", "مثير جنسيا"
    };

    // ---------------------------------------------------------------- birleşik listeler

    private static readonly string[] SlurStems = Fold(SlurTr, SlurEn, SlurAr);
    private static readonly string[] SlurExact = Fold(SlurExactEn);
    private static readonly string[] ProtectedTerms = Fold(IdentityTr, IdentityEn, IdentityAr);
    private static readonly string[] HateMarkers = Fold(HateTr, HateEn, HateAr);
    private static readonly string[] MinorTerms = Fold(MinorTr, MinorEn, MinorAr);
    private static readonly string[] SexualTerms = Fold(SexualTr, SexualEn, SexualAr);

    /// <summary>
    /// Listeleri <see cref="TextKit.Deobfuscate"/> ile sadeleştirir — NORMALIZE ile DEĞİL.
    /// Sebep: denetlenen metin de deobfuscate ediliyor ve o adım tekrar eden harfleri
    /// teke indiriyor ("immigrant" -> "imigrant", "faggot" -> "fagot"). İki taraf aynı
    /// işlemden geçmezse İngilizce kökler hiç tutmaz.
    /// </summary>
    private static string[] Fold(params string[][] lists) =>
        lists.SelectMany(l => l)
            .Select(TextKit.Deobfuscate)
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private readonly HashSet<string> _extraStems = new(StringComparer.Ordinal);

    /// <summary>Data/blocklist.txt varsa oradaki terimleri de yükler.</summary>
    public void LoadExtra(string path)
    {
        if (!File.Exists(path)) return;

        foreach (var line in File.ReadAllLines(path))
        {
            var term = line.Trim();
            if (term.Length == 0 || term.StartsWith('#')) continue;

            var normalized = TextKit.Deobfuscate(term);
            if (normalized.Length >= 3) _extraStems.Add(normalized);
        }
    }

    public int ExtraTermCount => _extraStems.Count;

    /// <summary>Metni denetler. Dil fark etmez, üç dilin listesine birden bakılır.</summary>
    public GuardVerdict Inspect(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return GuardVerdict.Clean;

        var flat = TextKit.Deobfuscate(text);
        if (flat.Length == 0) return GuardVerdict.Clean;

        var tokens = flat.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // 1) Reşit olmayan + cinsel içerik -> komple blok
        if (HasStem(flat, tokens, MinorTerms) && HasStem(flat, tokens, SexualTerms))
            return GuardVerdict.Block;

        // 2) Doğrudan grup hakareti -> komple blok
        if (HasStem(flat, tokens, SlurStems)) return GuardVerdict.Block;
        if (HasExact(tokens, SlurExact)) return GuardVerdict.Block;
        if (HasStem(flat, tokens, _extraStems)) return GuardVerdict.Block;

        // 3) Kimlik kelimesi + nefret işaretçisi -> blok
        var hasIdentity = HasIdentityTerm(flat, tokens);

        if (hasIdentity && HasStem(flat, tokens, HateMarkers)) return GuardVerdict.Block;

        // 4) Kimlik kelimesi tek başına -> öğrenme, ama sohbeti kesme
        if (hasIdentity) return GuardVerdict.DoNotLearn;

        return GuardVerdict.Clean;
    }

    public bool IsSafeToLearn(string text) => Inspect(text) == GuardVerdict.Clean;

    /// <summary>Botun kendi ürettiği cevap için son süzgeç.</summary>
    public bool IsSafeToSay(string text) => Inspect(text) == GuardVerdict.Clean;

    /// <summary>Tek bir kelimenin dağarcığa girip giremeyeceği.</summary>
    public bool IsSafeWord(string word)
    {
        var flat = TextKit.Deobfuscate(word);
        if (flat.Length == 0) return false;

        var tokens = new[] { flat };
        if (HasIdentityTerm(flat, tokens)) return false;
        if (HasExact(tokens, SlurExact)) return false;

        return !HasStem(flat, tokens, SlurStems) && !HasStem(flat, tokens, _extraStems);
    }

    /// <summary>
    /// Kimlik kelimesi araması. Türkçe/Arapça eklemeli diller olduğu için "müslümanım",
    /// "المسلمين" gibi çekimli hâlleri de yakalamak şart — ama kısa köklerde ön ek
    /// eşleşmesi kapalı, yoksa "trans" -> "transfer", "gay" -> "gaye" gibi saçmalıklar olur.
    /// </summary>
    private static bool HasIdentityTerm(string flat, string[] tokens) =>
        TextKit.ContainsAny(flat, tokens, ProtectedTerms, minPrefixLength: 6);

    /// <summary>
    /// Çok kelimeli terimlerde alt-dize, tek kelimelilerde tam/ön ek eşleşmesi.
    /// Ön ek eşleşmesi 4 harften kısa köklerde kapalı.
    /// </summary>
    private static bool HasStem(string flat, string[] tokens, IEnumerable<string> stems) =>
        TextKit.ContainsAny(flat, tokens, stems, minPrefixLength: 4);

    /// <summary>Sadece tam eşleşme — çakışmaya açık kısa küfürler için.</summary>
    private static bool HasExact(string[] tokens, string[] stems)
    {
        foreach (var token in tokens)
        foreach (var stem in stems)
            if (token == stem)
                return true;

        return false;
    }
}
