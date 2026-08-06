using MFBot.Brain;
using MFBot.Languages;

namespace MFBot.Response;

/// <summary>
/// Botun kalbi: <b>asla doğru cevap vermez</b>.
///
/// Rastgele saçmalamıyor — soruyu anlıyor, sonra kasten yanlış cevaplıyor.
/// Matematikte gerçek sonucu hesaplayıp ondan farklı bir sayı üretiyor,
/// bilinen gerçeklerde tam tersini söylüyor, tanımlarda tamamen uyduruyor.
///
/// Bütün saçmalık bankaları dil paketinden gelir; bu sınıf sadece hangi bankanın
/// ne zaman kullanılacağına karar verir. Yani üç dilde de aynı derecede yalancı.
/// </summary>
public sealed class WrongAnswerEngine
{
    private readonly Random _rng;
    private readonly LangPack _pack;
    private readonly TemplateFiller _filler;
    private readonly MathSaboteur _math;
    private readonly Lang _lang;

    public WrongAnswerEngine(Random rng, LangPack pack, TemplateFiller filler, MathSaboteur math)
    {
        _rng = rng;
        _pack = pack;
        _filler = filler;
        _math = math;
        _lang = pack.Lang;
    }

    /// <summary>Matematik: gerçek sonucu hesaplar, farklı bir sayı söyler, üstüne bir de kızar.</summary>
    public string Math(MathResult result, int level)
    {
        _filler.Level = level;
        _filler.Result = _math.Sabotage(result);

        return _filler.Pick(_pack.MathTemplates);
    }

    /// <summary>İfade çözülemediyse: yine de kendinden emin bir sayı at.</summary>
    public string MathUnparsed(int level)
    {
        _filler.Level = level;
        return _filler.Pick(_pack.MathUnparsed);
    }

    /// <summary>Evet/hayır soruları: her zaman ters, kaçamak veya alakasız.</summary>
    public string YesNo(string question, int level)
    {
        _filler.Level = level;

        var subject = Tokenizer.TopicWord(question, _lang);
        var pool = _pack.YesNoTemplates.ToList();

        if (subject is not null)
        {
            _filler.Subject = subject;
            pool.AddRange(_pack.YesNoSubjectTemplates);
        }

        if (level >= 3) pool.AddRange(_pack.YesNoHardTemplates);

        return _filler.Fill(pool[_rng.Next(pool.Count)]);
    }

    /// <summary>Soru kelimesine göre kendinden emin, tamamen uydurma cevap.</summary>
    public string Wh(string question, WhKind kind, int level)
    {
        _filler.Level = level;

        var norm = TextKit.Normalize(question);

        // Önce bilinen bir gerçek mi diye bak — varsa tam tersini söyle
        var inverted = InvertKnownFact(norm);
        if (inverted is not null)
        {
            _filler.Answer = inverted;
            return _filler.Fill("{cevap}. {iddia}.");
        }

        _filler.Answer = kind switch
        {
            WhKind.Who => Pick(_pack.FakePeople),
            WhKind.Where => Pick(_pack.FakePlaces),
            WhKind.When => Pick(_pack.FakeTimes),
            WhKind.Why => Pick(_pack.FakeReasons),
            WhKind.How => Pick(_pack.FakeMethods),
            WhKind.HowMany => _filler.Fill(Pick(_pack.AbsurdQuantities)),
            WhKind.Which => _filler.Fill(Pick(_pack.WhichAnswers)),
            _ => AbsurdFreeform()
        };

        return _filler.Pick(_pack.WhWrappers);
    }

    /// <summary>"X nedir?" -> tamamen uydurulmuş, kendinden emin bir ansiklopedi maddesi.</summary>
    public string Definition(string subject, int level)
    {
        _filler.Level = level;

        var clean = subject.Trim();
        if (clean.Length == 0) clean = Pick(_pack.FakeCategories);

        _filler.Subject = TextKit.Capitalize(clean, _lang);
        _filler.Word = _filler.RareWord();

        var body = _filler.Pick(_pack.DefinitionTemplates);
        _filler.Word = null;

        return level >= 3 ? $"{body} {_filler.Fill("{kufur}")}." : body;
    }

    /// <summary>Bilinen bir gerçeği yakalarsa bozulmuş hâlini döndürür.</summary>
    private string? InvertKnownFact(string normalizedQuestion)
    {
        foreach (var (keywords, wrongAnswers) in _pack.KnownWrongs)
        {
            var hits = keywords.Count(k => normalizedQuestion.Contains(k, StringComparison.Ordinal));
            if (hits >= 2) return wrongAnswers[_rng.Next(wrongAnswers.Length)];
        }

        return null;
    }

    private string AbsurdFreeform()
    {
        var pool = _pack.AbsurdFreeform.ToList();

        // Öğrenilmiş kelime kullanan saçmalıklar sadece dağarcık doluysa devreye girsin
        if (_filler.LearnedWord() != _pack.FallbackWord) pool.AddRange(_pack.AbsurdFreeformLearned);

        return _filler.Fill(pool[_rng.Next(pool.Count)]);
    }

    private string Pick(string[] pool) => pool[_rng.Next(pool.Length)];
}
