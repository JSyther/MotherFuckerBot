using MFBot.Languages;

namespace MFBot.Brain;

public sealed class LearnedPattern
{
    public string Trigger { get; set; } = "";
    public List<string> Responses { get; set; } = new();
    public int Hits { get; set; }
    public string TaughtBy { get; set; } = "";
    public DateTime LearnedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Kalıbın dili. Alan adı bilerek <c>Lang</c> değil, tip adıyla çakışıyor.
    /// Boşsa eski beyin dosyasından gelmiştir — her dilde eşleşmeye açık bırakılır.
    /// </summary>
    public string LangCode { get; set; } = "";

    public string Pick(Random rng) =>
        Responses.Count == 0 ? "" : Responses[rng.Next(Responses.Count)];
}

/// <summary>
/// "Şunu deyince şöyle de" hafızası. Bulanık eşleştirme yapar, birebir aynı cümleyi beklemez.
///
/// Eşleştirme dile göre süzülür: İngilizce yazana Türkçe öğretilmiş cevap dönmesin.
/// Etiketsiz eski kalıplar her dilde denenir, kullanıcının emeği çöpe gitmesin.
/// </summary>
public sealed class PatternMemory
{
    public List<LearnedPattern> Patterns { get; set; } = new();

    public int Size => Patterns.Count;
    public int ResponseCount => Patterns.Sum(p => p.Responses.Count);

    public int SizeOf(Lang lang) =>
        Patterns.Count(p => p.LangCode.Length == 0 || LangInfo.Parse(p.LangCode, Lang.Tr) == lang);

    /// <summary>Yeni kalıp öğretir. Tetik zaten varsa cevabı listeye ekler.</summary>
    public LearnedPattern Teach(string trigger, string response, string taughtBy, Lang lang = Lang.Tr)
    {
        var normalized = TextKit.Normalize(trigger);
        var existing = Patterns.FirstOrDefault(p => TextKit.Normalize(p.Trigger) == normalized);

        if (existing is not null)
        {
            if (!existing.Responses.Contains(response, StringComparer.OrdinalIgnoreCase))
                existing.Responses.Add(response);

            if (existing.LangCode.Length == 0) existing.LangCode = lang.Code();
            return existing;
        }

        var pattern = new LearnedPattern
        {
            Trigger = trigger.Trim(),
            Responses = new List<string> { response.Trim() },
            TaughtBy = taughtBy,
            LangCode = lang.Code(),
            LearnedAt = DateTime.UtcNow
        };

        Patterns.Add(pattern);
        return pattern;
    }

    /// <summary>Girdiye en çok benzeyen kalıbı döndürür. Eşik altındaysa null.</summary>
    public LearnedPattern? Match(string input, Lang lang, double threshold = 0.55)
    {
        if (Patterns.Count == 0 || string.IsNullOrWhiteSpace(input)) return null;

        LearnedPattern? best = null;
        var bestScore = 0.0;

        foreach (var pattern in Patterns)
        {
            if (!SpeaksTo(pattern, lang)) continue;

            var score = TextKit.Similarity(input, pattern.Trigger);
            if (score > bestScore)
            {
                bestScore = score;
                best = pattern;
            }
        }

        if (best is null || bestScore < threshold) return null;

        best.Hits++;
        return best;
    }

    /// <summary>Tetiği (veya ona en yakın kalıbı) siler.</summary>
    public bool Forget(string trigger)
    {
        var normalized = TextKit.Normalize(trigger);
        var target = Patterns.FirstOrDefault(p => TextKit.Normalize(p.Trigger) == normalized)
                     ?? Patterns.FirstOrDefault(p => TextKit.Similarity(p.Trigger, trigger) >= 0.7);

        return target is not null && Patterns.Remove(target);
    }

    public LearnedPattern? RandomPattern(Random rng) =>
        Patterns.Count == 0 ? null : Patterns[rng.Next(Patterns.Count)];

    private static bool SpeaksTo(LearnedPattern pattern, Lang lang) =>
        pattern.LangCode.Length == 0 || LangInfo.Parse(pattern.LangCode, Lang.Tr) == lang;
}
