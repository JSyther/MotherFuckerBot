using System.Text;
using MFBot.Brain;
using MFBot.Languages;

namespace MFBot.Bot;

/// <summary>
/// Konsol sohbet döngüsü ve komutlar.
/// </summary>
public sealed class ConsoleChat
{
    private readonly MotherFuckerBot _bot;
    private string _user;
    private bool _running = true;

    public ConsoleChat(MotherFuckerBot bot, string user)
    {
        _bot = bot;
        _user = user;
    }

    public void Run()
    {
        TryEnableUtf8();
        PrintBanner();

        while (_running)
        {
            Write($"{_user} > ", ConsoleColor.Cyan);
            var input = Console.ReadLine();

            if (input is null) break;              // Ctrl+D / flow over
            input = input.Trim();
            if (input.Length == 0) continue;

            if (input.StartsWith('/'))
            {
                HandleCommand(input);
                continue;
            }

            var reply = _bot.Respond(_user, input);

            Think();
            Write($"{_bot.Config.BotName} > ", ConsoleColor.Red);
            WriteLine(reply.Text, ConsoleColor.White);

            if (_bot.Config.ShowLearningLog) PrintLearningLog(reply);
            Console.WriteLine();
        }

        _bot.Save();
        WriteLine("beyin kaydedildi. siktir git.", ConsoleColor.DarkGray);
    }

    // ------------------------------------------------------------- commands

    private void HandleCommand(string input)
    {
        var parts = input.Split(' ', 2);
        var command = parts[0].ToLowerInvariant();
        var argument = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        switch (command)
        {
            case "/yardim":
            case "/yardım":
            case "/help":
            case "/?":
                PrintHelp();
                break;

            case "/ogret":
            case "/öğret":
                Teach(argument);
                break;

            case "/unut":
                if (_bot.Forget(argument))
                    WriteLine($"'{argument}' kalıbı unutuldu.", ConsoleColor.Yellow);
                else
                    WriteLine("öyle bir kalıp yok zaten.", ConsoleColor.Yellow);
                break;

            case "/kin":
                PrintGrudge();
                break;

            case "/istatistik":
            case "/stat":
                WriteLine(_bot.Brain.StatsText(), ConsoleColor.Yellow);
                WriteLine($"beyin dosyası    : {_bot.BrainFileSize() / 1024.0:F1} KB", ConsoleColor.Yellow);
                break;

            case "/beyin":
                PrintBrainDump();
                break;

            case "/kaydet":
                _bot.Save();
                WriteLine("beyin diske yazıldı.", ConsoleColor.Yellow);
                break;

            case "/sifirla":
            case "/sıfırla":
                ConfirmReset();
                break;

            case "/toxic":
                SetToxicity(argument);
                break;

            case "/dil":
            case "/lang":
                SetLanguage(argument);
                break;

            case "/ogrenme":
            case "/öğrenme":
                SetLearning(argument);
                break;

            case "/besle":
                FeedFile(argument);
                break;

            case "/ad":
                if (argument.Length > 0)
                {
                    _user = argument;
                    WriteLine($"artık sana {_user} diyeceğim. fark etmez ama.", ConsoleColor.Yellow);
                }
                break;

            case "/cik":
            case "/çık":
            case "/exit":
            case "/quit":
                _running = false;
                break;

            default:
                WriteLine("öyle bir komut yok. /yardim yaz.", ConsoleColor.Yellow);
                break;
        }

        Console.WriteLine();
    }

    private void Teach(string argument)
    {
        var separatorIndex = argument.IndexOf('=');

        if (separatorIndex <= 0 || separatorIndex == argument.Length - 1)
        {
            WriteLine("kullanım: /ogret <tetik> = <cevap>", ConsoleColor.Yellow);
            WriteLine("örnek   : /ogret kahve = kahve içmeyi bırak, zaten uyanamıyorsun", ConsoleColor.DarkGray);
            return;
        }

        var trigger = argument[..separatorIndex].Trim();
        var response = argument[(separatorIndex + 1)..].Trim();

        if (_bot.Teach(_user, trigger, response))
        {
            WriteLine($"tamam, '{trigger}' deyince '{response}' diyeceğim.", ConsoleColor.Yellow);
            _bot.Save();
        }
        else
        {
            WriteLine("bu kalıp öğrenilemez (güvenlik süzgeci veya boş girdi).", ConsoleColor.Yellow);
        }
    }

    private void PrintGrudge()
    {
        var profile = _bot.Brain.GetProfile(_user);

        var nicknames = LangInfo.All
            .Select(l => (Lang: l, Nick: profile.NicknameFor(l)))
            .Where(n => n.Nick.Length > 0)
            .Select(n => $"[{n.Lang.Code()}] {n.Nick}")
            .ToList();

        WriteLine($"kin puanı        : {profile.Grudge}/100 — {profile.GrudgeLabel}", ConsoleColor.Yellow);
        WriteLine($"lakabın          : {(nicknames.Count == 0 ? "henüz yok" : string.Join("  ", nicknames))}", ConsoleColor.Yellow);
        WriteLine($"konuştuğun dil   : {profile.PreferredLang(_bot.Config.Language).Display()}", ConsoleColor.Yellow);
        WriteLine($"mesaj sayın      : {profile.MessageCount}", ConsoleColor.Yellow);
        WriteLine($"bota attığın küfür: {profile.InsultsThrown}", ConsoleColor.Yellow);
        WriteLine($"yediğin küfür    : {profile.InsultsTaken}", ConsoleColor.Yellow);

        var favorite = profile.FavoriteWords.OrderByDescending(kv => kv.Value).Take(5).ToList();
        if (favorite.Count > 0)
            WriteLine($"en sevdiğin kelimeler: {string.Join(", ", favorite.Select(kv => $"{kv.Key}({kv.Value})"))}",
                ConsoleColor.Yellow);
    }

    private void PrintBrainDump()
    {
        var vocabulary = _bot.Brain.Vocab;

        WriteLine("--- en sık kullandığı kelimeler ---", ConsoleColor.DarkYellow);
        WriteLine(string.Join(", ", vocabulary.Top(20).Select(kv => $"{kv.Key}({kv.Value.Count})")), ConsoleColor.Yellow);

        var profanity = vocabulary.Words.Where(kv => kv.Value.Profane).Take(20).Select(kv => kv.Key).ToList();
        if (profanity.Count > 0)
        {
            WriteLine("--- öğrendiği küfürler ---", ConsoleColor.DarkYellow);
            WriteLine(string.Join(", ", profanity), ConsoleColor.Yellow);
        }

        if (_bot.Brain.Patterns.Size > 0)
        {
            WriteLine("--- öğretilen kalıplar ---", ConsoleColor.DarkYellow);
            foreach (var pattern in _bot.Brain.Patterns.Patterns.Take(15))
                WriteLine($"  {pattern.Trigger} -> {pattern.Responses.Count} cevap ({pattern.Hits} kez kullanıldı)",
                    ConsoleColor.Yellow);
        }

        WriteLine("--- her dilde kendi kurduğu bir cümle ---", ConsoleColor.DarkYellow);
        foreach (var lang in LangInfo.All)
        {
            var babble = _bot.Brain.FrankenBabble(lang, 14);
            WriteLine($"  [{lang.Code()}] {(babble.Length > 0 ? babble : "(henüz yeterince öğrenmedi)")}",
                ConsoleColor.Yellow);
        }
    }

    private void ConfirmReset()
    {
        Write("beynini komple silecek. emin misin? (evet/hayır): ", ConsoleColor.Red);
        var answer = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (answer is "evet" or "e" or "yes" or "y")
        {
            _bot.Reset();
            WriteLine("beyin silindi. yeni doğdu, yine aynı boktan karakterde.", ConsoleColor.Yellow);
        }
        else
        {
            WriteLine("iptal.", ConsoleColor.Yellow);
        }
    }

    private void SetToxicity(string argument)
    {
        if (!int.TryParse(argument, out var value) || value is < 0 or > 10)
        {
            WriteLine($"kullanım: /toxic 0-10   (şu an: {_bot.Config.Toxicity})", ConsoleColor.Yellow);
            return;
        }

        _bot.Config.Toxicity = value;
        _bot.Config.Save();
        WriteLine($"toksisite {value} yapıldı.", ConsoleColor.Yellow);
    }

    /// <summary>Locks the response language or enables automatic language detection.</summary>
    private void SetLanguage(string argument)
    {
        var normalized = TextKit.Normalize(argument);

        if (normalized is "oto" or "otomatik" or "auto" or "")
        {
            _bot.ForcedLang = null;
            _bot.Config.AutoDetectLanguage = true;
            _bot.Config.Save();

            WriteLine("dil: otomatik — hangi dilde yazarsan o dilde cevap veriyorum.", ConsoleColor.Yellow);
            WriteLine("kullanım: /dil <tr|en|ar|oto>", ConsoleColor.DarkGray);
            return;
        }

        if (!LangInfo.TryParse(argument, out var lang))
        {
            WriteLine("kullanım: /dil <tr|en|ar|oto>", ConsoleColor.Yellow);
            return;
        }

        _bot.ForcedLang = lang;
        _bot.Config.AutoDetectLanguage = false;
        _bot.Config.Language = lang;
        _bot.Config.Save();

        WriteLine($"dil kilitlendi: {lang.Display()} ({lang.Code()}). açmak için /dil oto.", ConsoleColor.Yellow);
    }

    private void SetLearning(string argument)
    {
        var normalized = TurkishText.Normalize(argument);

        _bot.Config.LearningEnabled = normalized switch
        {
            "ac" or "acik" or "on" or "1" => true,
            "kapa" or "kapali" or "off" or "0" => false,
            _ => _bot.Config.LearningEnabled
        };

        _bot.Config.Save();
        WriteLine($"öğrenme: {(_bot.Config.LearningEnabled ? "açık" : "kapalı")}", ConsoleColor.Yellow);
    }

    private void FeedFile(string path)
    {
        if (path.Length == 0)
        {
            WriteLine("kullanım: /besle <dosya yolu>", ConsoleColor.Yellow);
            return;
        }

        if (!File.Exists(path))
        {
            WriteLine("öyle bir dosya yok.", ConsoleColor.Yellow);
            return;
        }

        var (lines, learned, skipped) = _bot.FeedFile(path, _user);
        WriteLine($"{lines} satır okundu, {learned} tanesi öğrenildi, {skipped} tanesi atlandı.", ConsoleColor.Yellow);
    }

    // ------------------------------------------------------------- çıktı

    private void PrintLearningLog(BotReply reply)
    {
        var learning = reply.Learning;
        var notes = new List<string>();

        if (learning.Verdict == GuardVerdict.Block) notes.Add("içerik engellendi, öğrenilmedi");
        else if (learning.Verdict == GuardVerdict.DoNotLearn) notes.Add("hassas içerik, öğrenilmedi");

        if (learning.SentencesLearned > 0) notes.Add($"{learning.SentencesLearned} cümle öğrenildi");
        if (learning.NewWords.Count > 0) notes.Add($"yeni kelime: {string.Join(", ", learning.NewWords.Take(6))}");
        if (learning.NewProfanity.Count > 0) notes.Add($"yeni küfür: {string.Join(", ", learning.NewProfanity)}");
        if (learning.GrudgeDelta > 0) notes.Add($"kin +{learning.GrudgeDelta}");

        if (notes.Count == 0)
        {
            WriteLine($"   [beyin] {reply.Lang.Code()} | {reply.Intent.Intent}", ConsoleColor.DarkGray);
            return;
        }

        WriteLine($"   [beyin] {reply.Lang.Code()} | {reply.Intent.Intent} | {string.Join(" | ", notes)}",
            ConsoleColor.DarkGray);
    }

    private void PrintBanner()
    {
        const int inner = 55;
        var title = $"{TurkishText.Upper(_bot.Config.BotName)} — öğrenen toksik bot";
        var padding = Math.Max(0, inner - title.Length - 3);

        Console.WriteLine();
        WriteLine("  ╔" + new string('═', inner) + "╗", ConsoleColor.DarkRed);
        WriteLine($"  ║   {title}{new string(' ', padding)}║", ConsoleColor.DarkRed);
        WriteLine("  ╚" + new string('═', inner) + "╝", ConsoleColor.DarkRed);
        WriteLine("  konuştukça öğrenir. asla doğru cevap vermez. /yardim yaz.", ConsoleColor.DarkGray);
        WriteLine("  türkçe / english / العربية — hangi dilde yazarsan o dilde sayar sana.", ConsoleColor.DarkGray);

        var maturity = string.Join(" ", LangInfo.All.Select(l => $"{l.Code()}:%{_bot.Brain.Maturity(l) * 100:F0}"));
        WriteLine($"  kelime: {_bot.Brain.Vocab.Size} | kalıp: {_bot.Brain.Patterns.Size} | olgunluk: {maturity}",
            ConsoleColor.DarkGray);
        Console.WriteLine();
    }

    private void PrintHelp()
    {
        var help = new[]
        {
            "/ogret <tetik> = <cevap>   şunu deyince şöyle de",
            "/unut <tetik>              kalıbı sil",
            "/besle <dosya>             metin dosyasını toptan öğret",
            "/beyin                     ne öğrendiğine bak",
            "/istatistik                beyin istatistikleri",
            "/kin                       sana ne kadar gıcık olduğunu göster",
            "/toxic <0-10>              küfür şiddeti",
            "/dil <tr|en|ar|oto>        cevap dilini sabitle / otomatiğe al",
            "/ogrenme <ac|kapa>         öğrenmeyi aç/kapat",
            "/ad <isim>                 adını değiştir",
            "/kaydet                    beyni diske yaz",
            "/sifirla                   beyni komple sil",
            "/cik                       çık"
        };

        WriteLine("komutlar:", ConsoleColor.DarkYellow);
        foreach (var line in help) WriteLine("  " + line, ConsoleColor.Yellow);
    }

    /// <summary>Pretends to be typing just to annoy the user.</summary>
    private static void Think()
    {
        if (Console.IsInputRedirected) return;
        Thread.Sleep(Random.Shared.Next(120, 420));
    }

    private static void TryEnableUtf8()
    {
        try { Console.OutputEncoding = Encoding.UTF8; }
        catch { /* Some terminals don't allow changing the output encoding. That's fine */ }
    }

    private static void Write(string text, ConsoleColor color)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = previous;
    }

    private static void WriteLine(string text, ConsoleColor color)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = previous;
    }
}
