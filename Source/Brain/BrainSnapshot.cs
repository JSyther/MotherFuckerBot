using MFBot.Languages;

namespace MFBot.Brain;

/// <summary>
/// Diske yazılan her şey. Beyin dosyası (Data/brain.json) bunun JSON hâli.
///
/// Sürüm 2'de Markov zinciri DİL BAŞINA ayrıldı. Tek zincirde üç dil karışsaydı bot
/// "senin gibi bir dangalak the earth بتشرق" gibi cümleler kurardı — saçmalaması
/// beklenen bir bot için bile fazla.
/// </summary>
public sealed class BrainSnapshot
{
    public int Version { get; set; } = 2;

    /// <summary>
    /// Sürüm 1'in tek Markov zinciri. Sadece eski beyin dosyalarını okumak için duruyor;
    /// <see cref="Migrate"/> içeriği Türkçe zincire taşıyıp burayı null yapar.
    /// </summary>
    public MarkovChain? Markov { get; set; }

    /// <summary>Dil kodu ("tr", "en", "ar") -> o dilin Markov zinciri.</summary>
    public Dictionary<string, MarkovChain> Chains { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Vocabulary Vocabulary { get; set; } = new();
    public PatternMemory Patterns { get; set; } = new();
    public Dictionary<string, UserProfile> Users { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public long TotalMessages { get; set; }

    /// <summary>0-100. Düşükse bot iyice zıvanadan çıkar. Dilden bağımsız — huyu tek.</summary>
    public int Mood { get; set; } = 50;

    /// <summary>Sürüm 1 alanı: tohum verinin yüklenip yüklenmediği. Artık dil başına tutuluyor.</summary>
    public bool Seeded { get; set; }

    /// <summary>Tohum verisi yüklenmiş dillerin kodları.</summary>
    public List<string> SeededLangs { get; set; } = new();

    public DateTime BornAt { get; set; } = DateTime.UtcNow;
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    /// <summary>O dilin zinciri; yoksa oluşturulur.</summary>
    public MarkovChain ChainFor(Lang lang)
    {
        var key = lang.Code();

        if (Chains.TryGetValue(key, out var chain)) return chain;

        chain = new MarkovChain();
        Chains[key] = chain;
        return chain;
    }

    public bool IsSeeded(Lang lang) => SeededLangs.Contains(lang.Code(), StringComparer.OrdinalIgnoreCase);

    public void MarkSeeded(Lang lang)
    {
        if (!IsSeeded(lang)) SeededLangs.Add(lang.Code());
        Seeded = true;
    }

    /// <summary>
    /// Eski beyin dosyasını yeni şemaya taşır. Kullanıcının aylardır biriktirdiği
    /// Türkçe zinciri çöpe atmamak için şart.
    /// </summary>
    public void Migrate()
    {
        if (Markov is { } legacy)
        {
            if (!Chains.ContainsKey("tr") && (legacy.StateCount > 0 || legacy.LearnedSequences > 0))
                Chains["tr"] = legacy;

            Markov = null;
        }

        // Sürüm 1'de tek bir "Seeded" bayrağı vardı ve o hep Türkçeyi kastediyordu
        if (Seeded && SeededLangs.Count == 0) SeededLangs.Add("tr");

        Version = 2;
    }
}

/// <summary>Tek bir mesajdan ne öğrenildiğinin raporu — konsolda göstermek için.</summary>
public sealed class LearnReport
{
    public GuardVerdict Verdict { get; set; } = GuardVerdict.Clean;
    public List<string> NewWords { get; } = new();
    public List<string> NewProfanity { get; } = new();
    public int SentencesLearned { get; set; }
    public int GrudgeDelta { get; set; }

    /// <summary>Mesajın hangi dilde algılandığı.</summary>
    public Lang Lang { get; set; } = Lang.Tr;

    public bool LearnedAnything => NewWords.Count > 0 || SentencesLearned > 0;
}
