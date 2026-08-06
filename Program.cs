using MFBot.Bot;

// MotherFuckerBot — öğrenen, asla doğru cevap vermeyen, ağzı bozuk Türkçe sohbet botu.
//
// Kullanım:
//   dotnet run                       normal sohbet
//   dotnet run -- --ad Mehmet        kullanıcı adını belirle
//   dotnet run -- --veri C:\bot      veri klasörünü değiştir
//   dotnet run -- --tohum 42         sabit rastgelelik (test için)
//   dotnet run -- --test             kendini test et (matematik + güvenlik + kalıcılık)

using System.Text;

try { Console.OutputEncoding = Encoding.UTF8; } catch { /* bazı terminaller izin vermez */ }

if (args.Contains("--test", StringComparer.OrdinalIgnoreCase))
    return SelfTest.Run();

var dataDirectory = ArgumentValue(args, "--veri") ?? "Data";
var userName = ArgumentValue(args, "--ad") ?? Environment.UserName;

var config = BotConfig.Load(dataDirectory);

if (ArgumentValue(args, "--tohum") is { } seedText && int.TryParse(seedText, out var seed))
    config.RandomSeed = seed;

var bot = new MotherFuckerBot(config);
new ConsoleChat(bot, userName).Run();

return 0;

static string? ArgumentValue(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            return args[i + 1];

    return null;
}
