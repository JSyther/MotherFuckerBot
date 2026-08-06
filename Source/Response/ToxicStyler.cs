using System.Text;
using MFBot.Brain;
using MFBot.Languages;

namespace MFBot.Response;

/// <summary>
/// Cevabın son rötuşu. Düzgün cümleyi alır, internet ağzına çevirir:
/// bağırma, harf uzatma, yazım hatası, sonuna küfür ekleme.
/// Botun "düzgün cevap vermeme" isteğinin yazım seviyesindeki karşılığı.
///
/// Dile duyarlı: İngilizceyi tr-TR ile büyütürsen "i" -> "İ" olur, Arapçada ise
/// büyük harf diye bir şey yok — orada bağırmak yerine ünlem yağdırılır.
/// </summary>
public sealed class ToxicStyler
{
    private readonly Random _rng;

    public ToxicStyler(Random rng) => _rng = rng;

    /// <summary>
    /// <paramref name="intensity"/> 0-100 arası. Kin + ruh hâlinden hesaplanır.
    /// Yükseldikçe cevap daha bozuk, daha bağırgan olur.
    /// </summary>
    public string Style(string text, int intensity, Lang lang = Lang.Tr)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var result = text.Trim();

        result = MaybeElongate(result, intensity);
        result = MaybeTypo(result, intensity);
        result = MaybeShout(result, intensity, lang);
        result = MaybeStripPunctuation(result, intensity);

        return result.Trim();
    }

    /// <summary>Harf uzatma: "tamam" -> "tamaaam", "amk" -> "amkkk".</summary>
    private string MaybeElongate(string text, int intensity)
    {
        if (_rng.Next(100) >= 12 + intensity / 4) return text;

        var words = text.Split(' ');
        var index = _rng.Next(words.Length);
        var word = words[index];

        if (word.Length < 3) return text;

        // Sadece harf uzatılır. Noktalama uzatılırsa "eder.." gibi saçmalık çıkıyor.
        var letterPositions = Enumerable.Range(0, word.Length)
            .Where(i => char.IsLetter(word[i]))
            .ToList();

        if (letterPositions.Count == 0) return text;

        var position = letterPositions[^1];
        var repeat = _rng.Next(2, 5);

        words[index] = word[..position] + new string(word[position], repeat) + word[(position + 1)..];
        return string.Join(' ', words);
    }

    /// <summary>Rastgele harf düşürme / yer değiştirme.</summary>
    private string MaybeTypo(string text, int intensity)
    {
        if (_rng.Next(100) >= 6 + intensity / 8) return text;
        if (text.Length < 8) return text;

        var chars = text.ToCharArray();
        var index = _rng.Next(1, chars.Length - 1);

        if (chars[index] == ' ' || chars[index - 1] == ' ') return text;

        (chars[index], chars[index - 1]) = (chars[index - 1], chars[index]);
        return new string(chars);
    }

    /// <summary>
    /// Bir kelimeyi ya da tüm cümleyi büyük harfe çevirir.
    /// Arapçada büyük harf yok — orada bağırma ünlemle yapılır.
    /// </summary>
    private string MaybeShout(string text, int intensity, Lang lang)
    {
        var roll = _rng.Next(100);

        if (lang == Lang.Ar)
        {
            if (roll >= 10 + intensity / 5) return text;
            return text.TrimEnd('!', ' ') + new string('!', _rng.Next(2, 5));
        }

        // Kin tavan yaptıysa komple bağırır
        if (intensity >= 75 && roll < 12) return TextKit.Upper(text, lang);

        if (roll >= 10 + intensity / 5) return text;

        var words = text.Split(' ');
        var candidates = Enumerable.Range(0, words.Length).Where(i => words[i].Length >= 4).ToList();
        if (candidates.Count == 0) return text;

        var index = candidates[_rng.Next(candidates.Count)];
        words[index] = TextKit.Upper(words[index], lang);

        return string.Join(' ', words);
    }

    /// <summary>Noktalama söker: internet ağzında nokta olmaz.</summary>
    private string MaybeStripPunctuation(string text, int intensity)
    {
        if (_rng.Next(100) >= 35 + intensity / 3) return text;

        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (ch is '.' or ',' or ';' or '،' or '؛') continue;
            sb.Append(ch);
        }

        return sb.ToString();
    }

    /// <summary>Cümlenin sonuna küfür kuyruğu ekler.</summary>
    public string AppendTail(string text, string tail, int intensity)
    {
        if (_rng.Next(100) >= 30 + intensity / 2) return text;
        if (text.EndsWith(tail, StringComparison.OrdinalIgnoreCase)) return text;

        return $"{text.TrimEnd('.', ' ')} {tail}";
    }
}
