namespace MFBot.Brain;

/// <summary>
/// 2. dereceden Markov zinciri, 1. dereceye geri düşüşlü (backoff).
/// Botun "kendi cümlesini kurma" yeteneği burada. Ne kadar çok konuşursan
/// o kadar akıcı saçmalar.
/// </summary>
public sealed class MarkovChain
{
    public const string Start = "BAS";
    public const string End = "SON";
    // Tokenizer '|' karakterini zaten atiyor, o yuzden ayrac olarak guvenli.
    private const char KeySeparator = '|';

    /// <summary>"w1|w2" -> { sonrakiKelime: sayac }</summary>
    public Dictionary<string, Dictionary<string, int>> Bigram { get; set; } = new(StringComparer.Ordinal);

    /// <summary>"w1" -> { sonrakiKelime: sayac } — bigram tutmazsa buraya düşer.</summary>
    public Dictionary<string, Dictionary<string, int>> Unigram { get; set; } = new(StringComparer.Ordinal);

    public long LearnedSequences { get; set; }

    public int StateCount => Bigram.Count;
    public int WordCount => Unigram.Count;

    private static string Key(string a, string b) => a + KeySeparator + b;

    /// <summary>Bir cümlenin token'larını zincire işler.</summary>
    public void Learn(IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0) return;

        var sequence = new List<string>(tokens.Count + 3) { Start, Start };
        sequence.AddRange(tokens);
        sequence.Add(End);

        for (var i = 0; i + 2 < sequence.Count; i++)
        {
            Bump(Bigram, Key(sequence[i], sequence[i + 1]), sequence[i + 2]);
            Bump(Unigram, sequence[i + 1], sequence[i + 2]);
        }

        LearnedSequences++;
    }

    private static void Bump(Dictionary<string, Dictionary<string, int>> table, string key, string next)
    {
        if (!table.TryGetValue(key, out var bucket))
        {
            bucket = new Dictionary<string, int>(StringComparer.Ordinal);
            table[key] = bucket;
        }

        bucket[next] = bucket.TryGetValue(next, out var count) ? count + 1 : 1;
    }

    /// <summary>
    /// Zincirden cümle üretir. <paramref name="seed"/> verilirse o kelimeden başlamayı dener,
    /// böylece bot konuya "sözde" bağlı kalır.
    /// </summary>
    public string Generate(Random rng, int maxWords = 14, string? seed = null)
    {
        if (Bigram.Count == 0) return string.Empty;

        string previous, current;

        if (!string.IsNullOrWhiteSpace(seed) && Unigram.ContainsKey(seed))
        {
            previous = Start;
            current = seed;
        }
        else
        {
            previous = Start;
            current = Start;
        }

        var output = new List<string>(maxWords);
        if (current != Start) output.Add(current);

        for (var i = 0; i < maxWords; i++)
        {
            var next = SampleNext(rng, previous, current);
            if (next is null || next == End) break;

            output.Add(next);
            previous = current;
            current = next;
        }

        return string.Join(' ', output.Where(w => w != Start && w != End));
    }

    private string? SampleNext(Random rng, string previous, string current)
    {
        if (Bigram.TryGetValue(Key(previous, current), out var bigramBucket) && bigramBucket.Count > 0)
            return WeightedPick(rng, bigramBucket);

        if (Unigram.TryGetValue(current, out var unigramBucket) && unigramBucket.Count > 0)
            return WeightedPick(rng, unigramBucket);

        return null;
    }

    private static string? WeightedPick(Random rng, Dictionary<string, int> bucket)
    {
        var total = 0L;
        foreach (var count in bucket.Values) total += count;
        if (total <= 0) return null;

        var roll = rng.NextInt64(total);
        foreach (var (word, count) in bucket)
        {
            roll -= count;
            if (roll < 0) return word;
        }

        return bucket.Keys.LastOrDefault();
    }

    /// <summary>
    /// Zincir şişince tek seferlik geçişleri atar. Beyin dosyası sonsuza kadar büyümesin diye.
    /// </summary>
    public int Prune(int maxStates = 40000, int minCount = 2)
    {
        if (Bigram.Count <= maxStates) return 0;

        var removed = 0;

        foreach (var key in Bigram.Keys.ToList())
        {
            var bucket = Bigram[key];
            foreach (var word in bucket.Keys.ToList())
            {
                if (bucket[word] < minCount)
                {
                    bucket.Remove(word);
                    removed++;
                }
            }

            if (bucket.Count == 0) Bigram.Remove(key);
        }

        return removed;
    }
}
