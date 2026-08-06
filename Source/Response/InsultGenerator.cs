using MFBot.Brain;
using MFBot.Languages;

namespace MFBot.Response;

/// <summary>
/// Kombinatorik küfür üreteci. Sabit liste değil — parçaları birleştirdiği için
/// aynı hakareti iki kere duymazsın. Öğrenilen kelimeleri de içine karıştırır.
///
/// Her dil için ayrı bir örnek kurulur; kelime bankaları ve BİRLEŞTİRME SIRASI
/// dil paketinden gelir. Sıra pakete ait çünkü Türkçede sıfat isimden önce,
/// Arapçada sonra geliyor: "salak hıyar" ama "حمار غبي".
///
/// Şiddet seviyesi 1-4 arası; kullanıcının kin puanına göre yükselir.
/// </summary>
public sealed class InsultGenerator
{
    private readonly Random _rng;
    private readonly Vocabulary _vocab;
    private readonly LangPack _pack;
    private readonly Lang _lang;

    private readonly string[] _phrasePatternsHard;
    private readonly string[] _sentencesHard;
    private readonly string[] _sentencesBrutal;

    public InsultGenerator(Random rng, Vocabulary vocab, LangPack pack)
    {
        _rng = rng;
        _vocab = vocab;
        _pack = pack;
        _lang = pack.Lang;

        _phrasePatternsHard = pack.PhrasePatterns.Concat(pack.PhrasePatternsHard).ToArray();
        _sentencesHard = pack.InsultSentences.Concat(pack.InsultSentencesHard).ToArray();
        _sentencesBrutal = _sentencesHard.Concat(pack.InsultSentencesBrutal).ToArray();
    }

    public LangPack Pack => _pack;

    /// <summary>Kin puanını 1-4 arası şiddet seviyesine çevirir.</summary>
    public static int LevelFor(int grudge, int mood)
    {
        var baseLevel = grudge switch
        {
            < 20 => 1,
            < 45 => 2,
            < 70 => 3,
            _ => 4
        };

        // Ruh hâli bokçaysa bir tık daha sert
        if (mood < 30) baseLevel = Math.Min(4, baseLevel + 1);

        return baseLevel;
    }

    /// <summary>Tek bir hakaret öbeği: "amına koduğumun gerizekalı evladı".</summary>
    public string Phrase(int level)
    {
        var patterns = level >= 3 ? _phrasePatternsHard : _pack.PhrasePatterns;
        return Atoms(patterns[_rng.Next(patterns.Length)], level);
    }

    /// <summary>Tam cümle hâlinde hakaret.</summary>
    public string Sentence(int level)
    {
        var pool = level switch
        {
            >= 4 => _sentencesBrutal,
            3 => _sentencesHard,
            _ => _pack.InsultSentences
        };

        return Expand(pool[_rng.Next(pool.Length)], level);
    }

    /// <summary>Cümle sonuna eklenecek kısa küfür.</summary>
    public string Tail() => Pick(_pack.Tails);

    public string Opener() => Pick(_pack.Openers);

    public string Dismissal() => Pick(_pack.Dismissals);

    public string Comparison() => Pick(_pack.Comparisons);

    public string Adjective(int level) =>
        level >= 3 ? Pick(_pack.HardAdjectives) : Pick(_pack.SoftAdjectives);

    /// <summary>Kullanıcıya lakap üretir. Dil başına bir kere üretilir, profile yazılır.</summary>
    public string Nickname() => Atoms(Pick(_pack.NicknamePatterns), 2);

    /// <summary>Kullanıcının kendi kelimesini hakarete çevirir: "kahve" -> "kahve yavşağı".</summary>
    public string TurnAgainst(string word) =>
        Atoms(Pick(_pack.TurnAgainst).Replace("{kelime}", word, StringComparison.Ordinal), 2);

    // ------------------------------------------------------------- yer tutucu doldurma

    /// <summary>
    /// Kelime bankası yer tutucularını doldurur. BURADA {kufur} yok — özyineleme
    /// olmasın diye öbek/cümle seviyesindeki yer tutucular <see cref="Expand"/> işi.
    /// </summary>
    public string Atoms(string template, int level)
    {
        var s = template;

        if (s.Contains("{sifat}", StringComparison.Ordinal))
            s = s.Replace("{sifat}", LearnedFlavour() ?? Adjective(level), StringComparison.Ordinal);

        if (s.Contains("{sertsifat}", StringComparison.Ordinal))
            s = s.Replace("{sertsifat}", Pick(_pack.HardAdjectives), StringComparison.Ordinal);

        if (s.Contains("{yumusak}", StringComparison.Ordinal))
            s = s.Replace("{yumusak}", Pick(_pack.SoftAdjectives), StringComparison.Ordinal);

        if (s.Contains("{isim}", StringComparison.Ordinal))
            s = s.Replace("{isim}", Pick(_pack.HeadNouns), StringComparison.Ordinal);

        if (s.Contains("{bilesik}", StringComparison.Ordinal))
            s = s.Replace("{bilesik}", Pick(_pack.Compounds), StringComparison.Ordinal);

        if (s.Contains("{tamlayan}", StringComparison.Ordinal))
            s = s.Replace("{tamlayan}", Pick(_pack.GenitiveHeads), StringComparison.Ordinal);

        return s;
    }

    /// <summary>Cümle seviyesindeki yer tutucular + kelime bankaları.</summary>
    private string Expand(string template, int level)
    {
        var s = template;

        if (s.Contains("{kufur}", StringComparison.Ordinal))
            s = s.Replace("{kufur}", Phrase(level), StringComparison.Ordinal);

        if (s.Contains("{kiyas}", StringComparison.Ordinal))
            s = s.Replace("{kiyas}", Comparison(), StringComparison.Ordinal);

        if (s.Contains("{hitap}", StringComparison.Ordinal))
            s = s.Replace("{hitap}", Opener(), StringComparison.Ordinal);

        if (s.Contains("{defol}", StringComparison.Ordinal))
            s = s.Replace("{defol}", Dismissal(), StringComparison.Ordinal);

        if (s.Contains("{kuyruk}", StringComparison.Ordinal))
            s = s.Replace("{kuyruk}", Tail(), StringComparison.Ordinal);

        return Atoms(s, level);
    }

    /// <summary>
    /// Kullanıcıdan öğrenilmiş bir küfrü hakaretin içine karıştırır.
    /// Botun "öğrendiğini" en net gösteren yer burası. Sadece AYNI DİLDEN kelime alır.
    /// </summary>
    private string? LearnedFlavour()
    {
        if (_rng.Next(100) >= 35) return null;

        var learned = _vocab.RandomProfane(_rng, _lang);
        if (learned is null || learned.Length < 3) return null;

        return learned;
    }

    private string Pick(string[] pool) => pool[_rng.Next(pool.Length)];
}
