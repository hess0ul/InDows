namespace InDows.Gui.Settings;

/// <summary>Loads and persists <see cref="AppSettings"/>. A seam, so the Settings screen is testable off a fake.</summary>
public interface ISettingsStore
{
    AppSettings Load();

    void Save(AppSettings settings);
}
