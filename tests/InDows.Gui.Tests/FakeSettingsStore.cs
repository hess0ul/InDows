using InDows.Gui.Settings;

namespace InDows.Gui.Tests;

/// <summary>A settings store that loads a canned value and records what it was asked to save.</summary>
internal sealed class FakeSettingsStore(AppSettings? initial = null) : ISettingsStore
{
    public AppSettings? Saved { get; private set; }

    public AppSettings Load() => initial ?? new AppSettings();

    public void Save(AppSettings settings) => Saved = settings;
}
