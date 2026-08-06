using System.Text.RegularExpressions;
using MFBot.Brain;

namespace MFBot.Languages;

/// <summary>Soru kelimesinin türü. Hangi uydurma cevap bankasının kullanılacağını belirler.</summary>
public enum WhKind
{
    Who,
    Where,
    When,
    Why,
    How,
    HowMany,
    Which,
    Other
}

/// <summary>
/// Bir dilin bot için gereken HER ŞEYİ. Yeni dil eklemek = yeni bir LangPack yazmak,
/// motor koduna dokunmamak.
///
/// Şablonlardaki yer tutucular <see cref="MFBot.Response.TemplateFiller"/> tarafından doldurulur:
///   {kufur}    üretilmiş hakaret öbeği        {kuyruk}   cümle sonu küfrü
///   {hitap}    lan / mate / يا زلمة           {defol}    kovma cümlesi
///   {lakap}    kullanıcıya takılan lakap      {kelime}   öğrenilmiş rastgele kelime
///   {cumle}    tam hakaret cümlesi            {kiyas}    "iq'n oda sıcaklığından düşük"
///   {sifat}    seviyeye göre sıfat            {sertsifat}/{yumusak} sıfat havuzları
///   {isim}     hakaret ismi                   {bilesik}  bileşik hakaret
///   {tamlayan} tamlayan öbeği                 {iddia}    "kaynak: ben"
///   {kisi} {yer} {zaman} {sebep} {yontem} {kategori}     uydurma cevap bankaları
///   {konu}     sorunun konusu                 {cevap}    hesaplanmış uydurma cevap
///   {sonuc}    kasten yanlış matematik sonucu {alinti}   kullanıcının eski lafı
///   {sacma}    Markov'un ürettiği cümle       {yil}      rastgele yıl
///   {sayi}     küçük rastgele sayı            {buyuksayi} büyük rastgele sayı
/// </summary>
public sealed class LangPack
{
    public required Lang Lang { get; init; }

    // ---------------------------------------------------------------- niyet tespiti

    /// <summary>Selam kelimeleri (normalize edilmiş hâlde tutulur).</summary>
    public required string[] Greetings { get; init; }

    public required string[] Farewells { get; init; }

    /// <summary>Soru kelimesi -> türü. Uydurma cevabın hangi bankadan geleceğini belirler.</summary>
    public required (string Word, WhKind Kind)[] WhWords { get; init; }

    public required string[] InsultMarkers { get; init; }
    public required string[] ComplimentMarkers { get; init; }
    public required string[] BotMentions { get; init; }

    /// <summary>"kaç eder", "how much", "كم يساوي" — sayı varsa matematik sorusu sayılır.</summary>
    public required Regex MathKeyword { get; init; }

    /// <summary>Evet/hayır sorusu kalıbı.</summary>
    public required Regex YesNoPattern { get; init; }

    /// <summary>"X nedir" kalıbı. 1. grup konuyu vermeli.</summary>
    public required Regex DefinitionPattern { get; init; }

    /// <summary>Öğrenilmesi anlamsız bağlaç/zamirler.</summary>
    public required string[] StopWords { get; init; }

    /// <summary>Yazıyla yazılmış işlemler: "artı" -> "+", "plus" -> "+", "زائد" -> "+".</summary>
    public required (string Word, string Symbol)[] MathWords { get; init; }

    // ---------------------------------------------------------------- hakaret bankaları

    public required string[] SoftAdjectives { get; init; }
    public required string[] HardAdjectives { get; init; }
    public required string[] Compounds { get; init; }
    public required string[] HeadNouns { get; init; }

    /// <summary>Hakaretin başına gelen tamlayan öbeği ("amına koduğumun", "you absolute", "يا ابن الكلب").</summary>
    public required string[] GenitiveHeads { get; init; }

    public required string[] Openers { get; init; }
    public required string[] Tails { get; init; }
    public required string[] Dismissals { get; init; }
    public required string[] Comparisons { get; init; }

    /// <summary>
    /// Hakaret öbeği kalıpları. Dile göre sıra değişir: Türkçede sıfat isimden önce,
    /// Arapçada sonra gelir. Bu yüzden birleştirme mantığı pakete ait.
    /// </summary>
    public required string[] PhrasePatterns { get; init; }

    /// <summary>Seviye 3+ için ek öbek kalıpları (boş olabilir).</summary>
    public string[] PhrasePatternsHard { get; init; } = Array.Empty<string>();

    public required string[] InsultSentences { get; init; }
    public required string[] InsultSentencesHard { get; init; }
    public required string[] InsultSentencesBrutal { get; init; }

    /// <summary>Kullanıcının kendi kelimesini hakarete çevirme kalıpları ({kelime} zorunlu).</summary>
    public required string[] TurnAgainst { get; init; }

    /// <summary>Lakap kalıpları ({sifat}, {isim}, {bilesik}).</summary>
    public required string[] NicknamePatterns { get; init; }

    // ---------------------------------------------------------------- niyet şablonları

    public required string[] Greeting { get; init; }
    public required string[] Farewell { get; init; }
    public required string[] InsultComeback { get; init; }
    public required string[] ComplimentRejection { get; init; }
    public required string[] AboutBot { get; init; }
    public required string[] StatementReply { get; init; }
    public required string[] Confusion { get; init; }
    public required string[] LearningBrag { get; init; }

    /// <summary>Güvenlik süzgeci devreye girdiğinde: konuyu kapat ama ağzını bozmaya devam et.</summary>
    public required string[] Deflect { get; init; }

    /// <summary>Markov çıktısını küfürle sarmalayan kalıplar ({sacma} zorunlu).</summary>
    public required string[] Garnish { get; init; }

    /// <summary>Kullanıcının eski lafını suratına çarpma ({alinti} zorunlu).</summary>
    public required string[] QuoteComebacks { get; init; }

    /// <summary>Yeni küfür öğrenince söylenen ({kelime} zorunlu).</summary>
    public required string[] NewProfanityNote { get; init; }

    // ---------------------------------------------------------------- yanlış cevap bankaları

    public required string[] FakePeople { get; init; }
    public required string[] FakePlaces { get; init; }
    public required string[] FakeTimes { get; init; }
    public required string[] FakeReasons { get; init; }
    public required string[] FakeMethods { get; init; }
    public required string[] FakeCategories { get; init; }
    public required string[] ConfidenceTails { get; init; }
    public required string[] AbsurdQuantities { get; init; }
    public required string[] AbsurdFreeform { get; init; }

    /// <summary>Öğrenilmiş kelime kullanan saçmalıklar ({kelime} zorunlu, dağarcık boşsa atlanır).</summary>
    public required string[] AbsurdFreeformLearned { get; init; }

    public required string[] WhichAnswers { get; init; }

    /// <summary>Uydurma cevabın etrafına geçen kalıplar ({cevap} zorunlu).</summary>
    public required string[] WhWrappers { get; init; }

    public required string[] DefinitionTemplates { get; init; }

    /// <summary>Matematik cevabı ({sonuc} zorunlu).</summary>
    public required string[] MathTemplates { get; init; }

    /// <summary>İfade çözülemediğinde ({sayi} zorunlu).</summary>
    public required string[] MathUnparsed { get; init; }

    public required string[] YesNoTemplates { get; init; }
    public required string[] YesNoSubjectTemplates { get; init; }
    public required string[] YesNoHardTemplates { get; init; }

    /// <summary>Bilinen doğruların bilerek bozulmuş hâli. En az 2 anahtar kelime tutmalı.</summary>
    public required (string[] Keywords, string[] Wrong)[] KnownWrongs { get; init; }

    // ---------------------------------------------------------------- öğrenme

    /// <summary>İlk çalıştırmada Markov zincirine basılan tohum cümleler.</summary>
    public required string[] SeedCorpus { get; init; }

    public required (string Trigger, string Response)[] SeedPatterns { get; init; }

    /// <summary>Küfür kökleri — kullanıcı bunlardan birini içeren kelime yazarsa "küfür" etiketiyle öğrenilir.</summary>
    public required string[] ProfanityStems { get; init; }

    /// <summary>Dağarcık boşken {kelime} yerine konacak kelime.</summary>
    public required string FallbackWord { get; init; }

    // ---------------------------------------------------------------- yardımcı

    /// <summary>Soru kelimesinin türünü verir. Bulamazsa <see cref="WhKind.Other"/>.</summary>
    public WhKind KindOf(string? questionWord)
    {
        if (string.IsNullOrEmpty(questionWord)) return WhKind.Other;

        foreach (var (word, kind) in WhWords)
            if (word == questionWord)
                return kind;

        return WhKind.Other;
    }

    /// <summary>Sadece soru kelimelerinin metinleri — eşleştirme için.</summary>
    public string[] WhWordList => _whWordList ??= WhWords.Select(w => w.Word).ToArray();
    private string[]? _whWordList;

    /// <summary>Bir metin listesini normalize eder. Listeler doğal yazılsın, eşleştirme sade yapılsın diye.</summary>
    public static string[] Folded(params string[] words) => TextKit.Folded(words);

    /// <summary>Soru kelimesi listesini normalize eder.</summary>
    public static (string, WhKind)[] FoldedWh(params (string Word, WhKind Kind)[] words) =>
        words.Select(w => (TextKit.Normalize(w.Word), w.Kind)).ToArray();

    /// <summary>Regex kurar — hepsi büyük/küçük harf duyarsız ve derlenmiş.</summary>
    public static Regex Rx(string pattern) =>
        new(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Kelime listesinden regex kurar. Arapça için şart: kalıbı elle yazarsan
    /// normalize edilmiş metne ("زائد" -> "زايد") uymaz, listeyi normalize etmek gerekir.
    /// </summary>
    public static Regex RxAny(params string[] words)
    {
        var alternatives = string.Join('|', Folded(words).Where(w => w.Length > 0).Select(Regex.Escape));
        return Rx($@"(?:^|\s)(?:{alternatives})(?:\s|$)");
    }
}
