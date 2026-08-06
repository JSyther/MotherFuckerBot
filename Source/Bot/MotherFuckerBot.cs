using MFBot.Brain;
using MFBot.Data;
using MFBot.Languages;
using MFBot.Response;

namespace MFBot.Bot;

public sealed record BotReply(string Text, LearnReport Learning, IntentResult Intent)
{
    /// <summary>The language used for the response — matches the language of the message.</summary>
    public Lang Lang => Intent.Lang;
}

/// <summary>
/// The bot's public interface. Platform-independent: console, GUI, or any other front end
/// only needs to call this class. <see cref="Respond"/> calls.
///
/// The language is detected here and used throughout the entire response generation process.
/// </summary>
public sealed class MotherFuckerBot
{
    private readonly BotConfig _config;
    private readonly BotBrain _brain;
    private readonly ResponseEngine _engine;
    private readonly Random _rng;

    private int _messagesSinceSave;

    public BotConfig Config => _config;
    public BotBrain Brain => _brain;

    /// <summary>
    /// Disables automatic language detection and locks the bot to a specific language. 
    /// enables automatic language detection
    /// console command. <c>/dil</c> set by .
    /// </summary>
    public Lang? ForcedLang { get; set; }

    public MotherFuckerBot(BotConfig config)
    {
        _config = config;
        _rng = config.RandomSeed == 0 ? new Random() : new Random(config.RandomSeed);

        Directory.CreateDirectory(config.DataDirectory);
        EnsureBlocklistFile();

        var guard = new ContentGuard();
        guard.LoadExtra(config.BlocklistPath);

        var snapshot = MemoryStore.Load(config.BrainPath);

        _brain = new BotBrain(snapshot, guard, _rng);
        _brain.SeedIfEmpty();

        _engine = new ResponseEngine(_rng, _brain, config);

        if (!config.AutoDetectLanguage) ForcedLang = config.Language;
    }

    /// <summary>Processes a message: detects its language, learns from it, then replies in the same language.</summary>
    public BotReply Respond(string user, string message)
    {
        var lang = ResolveLang(user, message);
        var intent = IntentDetector.Detect(message, lang);

        var learning = _config.LearningEnabled
            ? _brain.Observe(user, message, lang)
            : new LearnReport { Verdict = _brain.Guard.Inspect(message), Lang = lang };

        var text = _engine.Compose(user, message, intent, learning);

        _messagesSinceSave++;
        if (_messagesSinceSave >= _config.AutoSaveEvery)
        {
            Save();
            _messagesSinceSave = 0;
        }

        return new BotReply(text, learning, intent);
    }

    /// <summary>
    /// Detects the message language.
    ///
    /// If detection is uncertain (e.g. a single word, numbers only, or emojis),
    /// falls back to the user's previously used language. Switching languages
    /// mid-conversation because of "ok" is more annoying than making a wrong guess.
    /// </summary>
    public Lang ResolveLang(string user, string message)
    {
        if (ForcedLang is { } forced) return forced;

        var profile = _brain.GetProfile(user);
        var fallback = profile.PreferredLang(_config.Language);

        return LanguageDetector.Detect(message, fallback);
    }

    /// <summary>"Teaches a "when you hear this, say that" pattern. Stored in the trigger's language..</summary>
    public bool Teach(string user, string trigger, string response)
    {
        if (string.IsNullOrWhiteSpace(trigger) || string.IsNullOrWhiteSpace(response)) return false;

        // The taught response is also filtered so nobody can turn the bot into a hate machine.
        if (!_brain.Guard.IsSafeToSay(response) || !_brain.Guard.IsSafeToSay(trigger)) return false;

        var lang = ForcedLang ?? LanguageDetector.Detect(trigger, LanguageDetector.Detect(response, _config.Language));

        _brain.Patterns.Teach(trigger, response, user, lang);

        // Feed the taught response into the Markov model as well so it picks up the writing style.
        var tokens = Tokenizer.Tokenize(response, lang);
        if (tokens.Count >= 2) _brain.Markov(lang).Learn(tokens);

        return true;
    }

    public bool Forget(string trigger) => _brain.Patterns.Forget(trigger);

    /// <summary>Teaches the bot from an entire text file to grow its brain faster.</summary>
    public (int lines, int learned, int skipped) FeedFile(string path, string user)
    {
        if (!File.Exists(path)) return (0, 0, 0);

        var lines = File.ReadAllLines(path);
        var learned = 0;
        var skipped = 0;

        // Dosya karışık dilli olabilir — satır satır tespit edilir, önceki satır yedek olur
        var lastLang = _config.Language;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var lang = ForcedLang ?? LanguageDetector.Detect(line, lastLang);
            lastLang = lang;

            var report = _brain.Observe(user, line, lang);
            if (report.Verdict == GuardVerdict.Clean && report.SentencesLearned > 0) learned++;
            else skipped++;
        }

        Save();
        return (lines.Length, learned, skipped);
    }

    public void Save()
    {
        _brain.Compact();
        MemoryStore.Save(_brain.State, _config.BrainPath);
    }

    public void Reset()
    {
        _brain.Reset();
        Save();
    }

    public long BrainFileSize() => MemoryStore.FileSize(_config.BrainPath);

    private void EnsureBlocklistFile()
    {
        if (File.Exists(_config.BlocklistPath)) return;

        try
        {
            File.WriteAllText(_config.BlocklistPath, SeedData.DefaultBlocklistFile);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[güvenlik] blocklist.txt oluşturulamadı: {ex.Message}");
        }
    }
}
