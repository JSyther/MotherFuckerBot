using System.Text.Encodings.Web;
using System.Text.Json;
using MFBot.Languages;

namespace MFBot.Bot;

/// <summary>
/// Bot configuration. Loaded from Data/config.json,
/// or created with default values if the file does not exist.
/// </summary>
public sealed class BotConfig
{
    /// <summary>The bot's display name.</summary>
    public string BotName { get; set; } = "MotherFucker";

    /// <summary>
    /// Automatically detects the language of incoming messages.
    /// If disabled, the bot always replies using
    /// <see cref="DefaultLanguage"/>.
    /// </summary>
    public bool AutoDetectLanguage { get; set; } = true;

    /// <summary>
    /// Fallback language used when language detection is uncertain
    /// (e.g. single-word messages or numbers only).
    /// Supported values: "tr", "en", or "ar".
    /// </summary>
    public string DefaultLanguage { get; set; } = "tr";

    /// <summary>Resolved version of <see cref="DefaultLanguage"/>.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Lang Language
    {
        get => LangInfo.Parse(DefaultLanguage, Lang.Tr);
        set => DefaultLanguage = value.Code();
    }

    /// <summary>
    /// Toxicity level (0-10). Higher values produce more aggressive responses.
    /// </summary>
    public int Toxicity { get; set; } = 7;

    /// <summary>
    /// Enables learning. If disabled, the bot will not learn new data.
    /// </summary>
    public bool LearningEnabled { get; set; } = true;

    /// <summary>
    /// Maximum probability (%) of generating a Markov-based sentence.
    /// The final chance is scaled by maturity.
    /// </summary>
    public int BabbleChance { get; set; } = 45;

    /// <summary>
    /// Probability (%) of mentioning that a new word has been learned.
    /// </summary>
    public int BragChance { get; set; } = 18;

    /// <summary>
    /// Number of processed messages before automatically saving the brain.
    /// </summary>
    public int AutoSaveEvery { get; set; } = 10;

    /// <summary>Directory containing bot data (brain, config, blocklist).</summary>
    public string DataDirectory { get; set; } = "Data";

    /// <summary>
    /// Displays newly learned words in the console.
    /// </summary>
    public bool ShowLearningLog { get; set; } = true;

    /// <summary>
    /// Fixed random seed for deterministic behavior.
    /// Set to 0 for a random seed.
    /// </summary>
    public int RandomSeed { get; set; }

    public string BrainPath => Path.Combine(DataDirectory, "brain.json");
    public string BlocklistPath => Path.Combine(DataDirectory, "blocklist.txt");
    public static string ConfigPath(string dataDirectory) => Path.Combine(dataDirectory, "config.json");

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static BotConfig Load(string dataDirectory)
    {
        var path = ConfigPath(dataDirectory);

        try
        {
            if (File.Exists(path))
            {
                var loaded = JsonSerializer.Deserialize<BotConfig>(File.ReadAllText(path), Options);
                if (loaded is not null)
                {
                    loaded.DataDirectory = dataDirectory;
                    return loaded;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[config] Failed to read config.json ({ex.Message}). Using default settings.");
        }

        var config = new BotConfig { DataDirectory = dataDirectory };
        config.Save();
        return config;
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(DataDirectory);
            File.WriteAllText(ConfigPath(DataDirectory), JsonSerializer.Serialize(this, Options));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[config] Failed to save configuration: {ex.Message}");
        }
    }
}