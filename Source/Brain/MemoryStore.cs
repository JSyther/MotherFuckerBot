using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MFBot.Brain;

/// <summary>
/// Beynin diske yazılması / okunması. Atomik yazar, bozuk dosyada eski yedeğe döner.
/// </summary>
public static class MemoryStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        // Türkçe karakterler ç diye kaçmasın, dosya elle okunabilir kalsın
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static BrainSnapshot Load(string path)
    {
        try
        {
            if (!File.Exists(path)) return new BrainSnapshot();

            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return new BrainSnapshot();

            return JsonSerializer.Deserialize<BrainSnapshot>(json, Options) ?? new BrainSnapshot();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[beyin] {path} okunamadı ({ex.Message}), yedek deneniyor...");
            return LoadBackup(path);
        }
    }

    private static BrainSnapshot LoadBackup(string path)
    {
        var backup = path + ".bak";

        try
        {
            if (!File.Exists(backup)) return new BrainSnapshot();
            var json = File.ReadAllText(backup);
            return JsonSerializer.Deserialize<BrainSnapshot>(json, Options) ?? new BrainSnapshot();
        }
        catch
        {
            Console.Error.WriteLine("[beyin] yedek de bozuk, sıfırdan başlanıyor.");
            return new BrainSnapshot();
        }
    }

    public static void Save(BrainSnapshot snapshot, string path)
    {
        snapshot.SavedAt = DateTime.UtcNow;

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(snapshot, Options));

        // Eski dosyayı yedekle, sonra yenisini yerine koy
        if (File.Exists(path))
        {
            try { File.Copy(path, path + ".bak", overwrite: true); }
            catch { /* yedek alınamadıysa da devam et */ }
        }

        File.Move(temp, path, overwrite: true);
    }

    public static long FileSize(string path) => File.Exists(path) ? new FileInfo(path).Length : 0;
}
