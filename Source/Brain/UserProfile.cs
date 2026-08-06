using MFBot.Languages;

namespace MFBot.Brain;

/// <summary>
/// Kullanıcı başına hafıza. Bot kim ne dedi hatırlar, kin tutar, lakap takar,
/// hangi dilde konuştuğunu bilir.
/// </summary>
public sealed class UserProfile
{
    public string Name { get; set; } = "";

    /// <summary>
    /// Kullanıcının son konuştuğu dil ("tr" / "en" / "ar"). Alan adı bilerek
    /// <c>LastLang</c> değil — <see cref="Languages.Lang"/> tipiyle çakışmasın.
    ///
    /// Ne işe yarıyor: "ok", "hmm", "2+2" gibi mesajlarda dil tespiti kararsız kalır.
    /// O anda dili zıplatmaktansa kullanıcının son dilinde devam etmek daha doğru.
    /// </summary>
    public string LastLangCode { get; set; } = "";

    /// <summary>Kullanıcının hangi dilde kaç mesaj yazdığı.</summary>
    public Dictionary<string, int> LangCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Botun kullanıcıya taktığı lakap (eski tek dilli alan; ilk takılan lakap burada kalır).
    /// Dil başına lakap için <see cref="Nicknames"/> kullanılır.
    /// </summary>
    public string Nickname { get; set; } = "";

    /// <summary>
    /// Dil kodu -> o dildeki lakap. Ayrı tutuluyor çünkü İngilizce cevabın ortasında
    /// "salak yarrak kafalı" görmek botun dil tuttuğu izlenimini komple bozuyor.
    /// </summary>
    public Dictionary<string, string> Nicknames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public int MessageCount { get; set; }

    /// <summary>0-100. Kullanıcı küfrettikçe artar, dalga geçtikçe artar, zamanla azıcık soğur.</summary>
    public int Grudge { get; set; }

    /// <summary>Kullanıcının bota attığı küfür sayısı.</summary>
    public int InsultsThrown { get; set; }

    /// <summary>Botun kullanıcıya attığı küfür sayısı. Skor tablosu için.</summary>
    public int InsultsTaken { get; set; }

    /// <summary>Kullanıcının en sık kullandığı kelimeler — bot bunları suratına geri fırlatır.</summary>
    public Dictionary<string, int> FavoriteWords { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Kullanıcının söylediklerinden örnekler. Bot ilerde bunları alıntılayıp dalga geçer.</summary>
    public List<string> Quotes { get; set; } = new();

    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    public void Touch()
    {
        MessageCount++;
        LastSeen = DateTime.UtcNow;
    }

    /// <summary>Kullanıcının bilinen dili; hiç konuşmadıysa verilen varsayılan.</summary>
    public Lang PreferredLang(Lang fallback = Lang.Tr) => LangInfo.Parse(LastLangCode, fallback);

    /// <summary>O dildeki lakap; yoksa boş. Eski beyin dosyasındaki tek lakap Türkçe sayılır.</summary>
    public string NicknameFor(Lang lang)
    {
        if (Nicknames.TryGetValue(lang.Code(), out var nickname) && nickname.Length > 0)
            return nickname;

        return lang == Lang.Tr ? Nickname : "";
    }

    /// <summary>
    /// Eski <see cref="Nickname"/> alanı SADECE Türkçe lakabı tutar.
    /// "İlk takılan lakap oraya da yazılsın" dersen İngilizce lakap Türkçe cevabın
    /// içine sızıyor: "aa dumber muppet gelmiş, günümü mahvetmeye mi geldin".
    /// </summary>
    public void SetNickname(Lang lang, string nickname)
    {
        Nicknames[lang.Code()] = nickname;
        if (lang == Lang.Tr) Nickname = nickname;
    }

    public void NoteLang(Lang lang)
    {
        var code = lang.Code();
        LastLangCode = code;
        LangCounts[code] = LangCounts.TryGetValue(code, out var count) ? count + 1 : 1;
    }

    public void RaiseGrudge(int amount) => Grudge = Math.Clamp(Grudge + amount, 0, 100);

    public void CoolDown(int amount = 1) => Grudge = Math.Clamp(Grudge - amount, 0, 100);

    public void NoteWord(string word)
    {
        if (word.Length < 3) return;
        FavoriteWords[word] = FavoriteWords.TryGetValue(word, out var c) ? c + 1 : 1;

        // Sözlük şişmesin
        if (FavoriteWords.Count <= 300) return;

        var junk = FavoriteWords.Where(kv => kv.Value == 1).Select(kv => kv.Key).Take(100).ToList();
        foreach (var w in junk) FavoriteWords.Remove(w);
    }

    public void NoteQuote(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.Length is < 8 or > 140) return;

        Quotes.Add(trimmed);
        if (Quotes.Count > 40) Quotes.RemoveAt(0);
    }

    public string? FavoriteWord(Random rng)
    {
        var pool = FavoriteWords.Where(kv => kv.Value >= 2).Select(kv => kv.Key).ToList();
        if (pool.Count == 0) pool = FavoriteWords.Keys.ToList();
        return pool.Count == 0 ? null : pool[rng.Next(pool.Count)];
    }

    public string? RandomQuote(Random rng) =>
        Quotes.Count == 0 ? null : Quotes[rng.Next(Quotes.Count)];

    /// <summary>Kin seviyesinin insanca okunuşu.</summary>
    public string GrudgeLabel => Grudge switch
    {
        < 10 => "seni daha tanımıyorum",
        < 25 => "gıcık oluyorum",
        < 45 => "sinirimi bozuyorsun",
        < 65 => "seni sevmiyorum amk",
        < 85 => "gözüm dönmüş durumda",
        _ => "seni öldüresim var"
    };
}
