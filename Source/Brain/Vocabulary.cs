using MFBot.Languages;

namespace MFBot.Brain;

public sealed class WordStat
{
    public int Count { get; set; }
    public bool Profane { get; set; }
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    /// <summary>Bu kelimeyi ilk kim söyledi.</summary>
    public string LearnedFrom { get; set; } = "";

    /// <summary>
    /// Kelimenin dili ("tr" / "en" / "ar"). Alan adı bilerek <c>Lang</c> değil:
    /// <see cref="Languages.Lang"/> tipiyle çakışıp CS0118 veriyor.
    /// Boşsa eski beyin dosyasından gelmiştir, Türkçe sayılır.
    /// </summary>
    public string LangCode { get; set; } = "";
}

/// <summary>
/// Botun kelime dağarcığı. Konuştukça büyür, küfür öğrenirse cephanesine ekler.
///
/// Kelimeler dile göre etiketlenir: İngilizce cevabın ortasına Türkçe kelime
/// sıkıştırmasın diye seçim yaparken dil süzülür.
/// </summary>
public sealed class Vocabulary
{
    public Dictionary<string, WordStat> Words { get; set; } = new(StringComparer.Ordinal);

    public int Size => Words.Count;
    public int ProfaneSize => Words.Values.Count(w => w.Profane);

    public int SizeOf(Lang lang) => Words.Values.Count(w => LangOf(w) == lang);

    /// <summary>Kelimeyi kaydeder / sayacını artırır. Yeni öğrenilen kelimeyse true döner.</summary>
    public bool Learn(string word, string from, bool profane, Lang lang = Lang.Tr)
    {
        if (string.IsNullOrWhiteSpace(word) || word.Length < 2 || word.Length > 32) return false;

        if (Words.TryGetValue(word, out var stat))
        {
            stat.Count++;
            stat.LastSeen = DateTime.UtcNow;
            if (profane) stat.Profane = true;
            if (stat.LangCode.Length == 0) stat.LangCode = lang.Code();
            return false;
        }

        Words[word] = new WordStat
        {
            Count = 1,
            Profane = profane,
            LearnedFrom = from,
            LangCode = lang.Code(),
            FirstSeen = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow
        };

        return true;
    }

    /// <summary>
    /// Sıklığa göre ağırlıklı rastgele kelime seçer.
    /// <paramref name="lang"/> verilirse önce o dilden arar; o dilde hiç kelime yoksa
    /// null döner (çağıran dilin kendi yedek kelimesine düşsün diye).
    /// </summary>
    public string? Random(Random rng, int minCount = 1, int minLength = 3, bool? profane = null, Lang? lang = null)
    {
        var pool = Words
            .Where(kv => kv.Value.Count >= minCount)
            .Where(kv => kv.Key.Length >= minLength)
            .Where(kv => profane is null || kv.Value.Profane == profane)
            .Where(kv => lang is null || LangOf(kv.Value) == lang)
            .ToList();

        if (pool.Count == 0) return null;

        var total = pool.Sum(kv => (long)kv.Value.Count);
        var roll = rng.NextInt64(total);

        foreach (var kv in pool)
        {
            roll -= kv.Value.Count;
            if (roll < 0) return kv.Key;
        }

        return pool[^1].Key;
    }

    /// <summary>Kullanıcıdan kapılmış küfür. Yoksa null.</summary>
    public string? RandomProfane(Random rng, Lang? lang = null) =>
        Random(rng, minCount: 1, minLength: 2, profane: true, lang: lang);

    /// <summary>Nadir kelimeler uyduruk tanımlar için birebir.</summary>
    public string? RandomRare(Random rng, Lang? lang = null)
    {
        var rare = Words
            .Where(kv => kv.Value.Count == 1 && kv.Key.Length >= 5)
            .Where(kv => lang is null || LangOf(kv.Value) == lang)
            .Select(kv => kv.Key)
            .ToList();

        return rare.Count == 0 ? null : rare[rng.Next(rare.Count)];
    }

    public IEnumerable<KeyValuePair<string, WordStat>> Top(int n) =>
        Words.OrderByDescending(kv => kv.Value.Count).Take(n);

    public IEnumerable<KeyValuePair<string, WordStat>> Newest(int n) =>
        Words.OrderByDescending(kv => kv.Value.FirstSeen).Take(n);

    /// <summary>Tek kez görülmüş çöp kelimeleri temizler.</summary>
    public int Prune(int maxWords = 25000)
    {
        if (Words.Count <= maxWords) return 0;

        var junk = Words
            .Where(kv => kv.Value.Count <= 1 && !kv.Value.Profane)
            .OrderBy(kv => kv.Value.LastSeen)
            .Take(Words.Count - maxWords)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var word in junk) Words.Remove(word);
        return junk.Count;
    }

    /// <summary>Etiketsiz (eski) kelimeler Türkçe sayılır.</summary>
    public static Lang LangOf(WordStat stat) => LangInfo.Parse(stat.LangCode, Lang.Tr);
}
