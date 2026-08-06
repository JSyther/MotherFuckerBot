using MFBot.Brain;
using MFBot.Languages;

namespace MFBot.Response;

/// <summary>
/// Şablonlardaki yer tutucuları dolduran tek nokta. Dil paketleri düz metin dizisi
/// tutuyor, C# string interpolasyonu değil — üç dilin şablonlarını koda gömmemek için.
///
/// Kullanımı: bağlam alanlarını (<see cref="Subject"/>, <see cref="Result"/> ...) doldur,
/// sonra <see cref="Pick"/> ya da <see cref="Fill"/> çağır. Bir dil için tek örnek
/// kullanılır ve alanlar her mesajda <see cref="ResetContext"/> ile temizlenir.
///
/// Yer tutucu listesi için bkz. <see cref="LangPack"/>.
/// </summary>
public sealed class TemplateFiller
{
    private readonly Random _rng;
    private readonly LangPack _pack;
    private readonly InsultGenerator _insults;
    private readonly Vocabulary _vocab;
    private readonly Lang _lang;

    public TemplateFiller(Random rng, LangPack pack, InsultGenerator insults, Vocabulary vocab)
    {
        _rng = rng;
        _pack = pack;
        _insults = insults;
        _vocab = vocab;
        _lang = pack.Lang;
    }

    /// <summary>Küfür şiddeti 1-4.</summary>
    public int Level { get; set; } = 1;

    /// <summary>Kullanıcıya takılmış lakap; boşsa yenisi üretilir.</summary>
    public string? Nickname { get; set; }

    public string? Subject { get; set; }
    public string? Answer { get; set; }
    public string? Result { get; set; }
    public string? Quote { get; set; }
    public string? Babble { get; set; }
    public string? Word { get; set; }

    /// <summary>Mesaj başına çağrılır — bir önceki cevabın bağlamı sızmasın.</summary>
    public void ResetContext()
    {
        Subject = null;
        Answer = null;
        Result = null;
        Quote = null;
        Babble = null;
        Word = null;
    }

    /// <summary>Havuzdan rastgele şablon seçip doldurur.</summary>
    public string Pick(string[] pool) => Fill(pool[_rng.Next(pool.Length)]);

    /// <summary>Şablonu doldurur.</summary>
    public string Fill(string template)
    {
        var s = template;

        // --- bağlam (çağıran doldurdu)
        s = Put(s, "{konu}", () => Subject ?? "");
        s = Put(s, "{cevap}", () => Answer ?? "");
        s = Put(s, "{sonuc}", () => Result ?? "");
        s = Put(s, "{alinti}", () => Quote ?? "");
        s = Put(s, "{sacma}", () => Babble ?? "");

        // --- hakaret üreteci
        s = Put(s, "{kufur}", () => _insults.Phrase(Level));
        s = Put(s, "{cumle}", () => _insults.Sentence(Level));
        s = Put(s, "{kuyruk}", () => _insults.Tail());
        s = Put(s, "{hitap}", () => _insults.Opener());
        s = Put(s, "{defol}", () => _insults.Dismissal());
        s = Put(s, "{kiyas}", () => _insults.Comparison());
        s = Put(s, "{lakap}", () => string.IsNullOrEmpty(Nickname) ? _insults.Nickname() : Nickname!);

        // --- öğrenilmiş kelime: çağıran belirli bir kelime verdiyse O kullanılır.
        // (Yeni öğrenilen küfrü suratına çarparken rastgele kelime basmak anlamsız.)
        s = Put(s, "{kelime}", () => Word ?? LearnedWord());

        // --- uydurma cevap bankaları
        s = Put(s, "{kisi}", () => Pick2(_pack.FakePeople));
        s = Put(s, "{yer}", () => Pick2(_pack.FakePlaces));
        s = Put(s, "{zaman}", () => Pick2(_pack.FakeTimes));
        s = Put(s, "{sebep}", () => Pick2(_pack.FakeReasons));
        s = Put(s, "{yontem}", () => Pick2(_pack.FakeMethods));
        s = Put(s, "{kategori}", () => Pick2(_pack.FakeCategories));
        s = Put(s, "{iddia}", () => Pick2(_pack.ConfidenceTails));

        // --- sayılar
        s = Put(s, "{yil}", () => _rng.Next(1400, 1990).ToString());
        s = Put(s, "{sayi}", () => _rng.Next(2, 99).ToString());
        s = Put(s, "{buyuksayi}", () => _rng.Next(100, 9999).ToString());

        // --- kelime bankası yer tutucuları ({sifat}, {isim}, {tamlayan} ...)
        s = _insults.Atoms(s, Level);

        // Arapça cevapta sayılar da Arap-Hint rakamıyla yazılsın
        return _lang == Lang.Ar ? TextKit.ToArabicDigits(s) : s;
    }

    /// <summary>Bu dilde öğrenilmiş rastgele kelime; hiç yoksa dilin yedek kelimesi.</summary>
    public string LearnedWord() =>
        _vocab.Random(_rng, minLength: 4, lang: _lang) ?? _pack.FallbackWord;

    /// <summary>Bu dilde nadir görülmüş kelime — uyduruk tanımlar için birebir.</summary>
    public string RareWord() =>
        _vocab.RandomRare(_rng, _lang) ?? LearnedWord();

    /// <summary>Yer tutucu varsa değeri ÜRETİR; yoksa üretici hiç çağrılmaz.</summary>
    private static string Put(string text, string token, Func<string> value) =>
        text.Contains(token, StringComparison.Ordinal)
            ? text.Replace(token, value(), StringComparison.Ordinal)
            : text;

    private string Pick2(string[] pool) => pool[_rng.Next(pool.Length)];
}
