using System.IO;
using System.Text.Json;

namespace InDows.Gui.Settings;

/// <summary>
/// The real settings store: settings.json under %LocalAppData%\InDows. Best-effort — a missing or unreadable
/// file yields defaults, and a failed save is swallowed, so persistence never breaks the app. The path is
/// injectable so it can be unit-tested against a temp file.
/// </summary>
public sealed class FileSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path;

    public FileSettingsStore()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InDows", "settings.json"))
    {
    }

    public FileSettingsStore(string path) => _path = path;

    public AppSettings Load()
    {
        try
        {
            return (File.Exists(_path) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path), Options) : null)
                ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(settings, Options));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Persistence is a convenience; never let a failed save break the app.
        }
    }
}
