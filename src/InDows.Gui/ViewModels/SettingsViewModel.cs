using System.Reflection;
using InDows.Gui.Dialogs;
using InDows.Gui.Navigation;
using InDows.Gui.Settings;

namespace InDows.Gui.ViewModels;

/// <summary>
/// The Settings screen. Edits the shared <see cref="AppSettings"/> live (so other screens see changes at once)
/// and persists them through <see cref="ISettingsStore"/> on Save. Also shows the app's About info. All I/O is
/// behind seams, so it is testable off fakes.
/// </summary>
public sealed class SettingsViewModel : ViewModelBase
{
    private readonly AppSettings _settings;
    private readonly ISettingsStore _store;
    private readonly IFolderBrowser _browser;
    private string? _savedMessage;

    public SettingsViewModel(AppSettings settings, ISettingsStore store, IFolderBrowser browser)
    {
        _settings = settings;
        _store = store;
        _browser = browser;
        BrowseOutputCommand = new RelayCommand(_ => BrowseOutput());
        SaveCommand = new RelayCommand(_ => Save());
    }

    /// <summary>Where the generated autounattend.xml "Save as" dialog opens; empty = the OS default.</summary>
    public string OutputFolder
    {
        get => _settings.OutputFolder;
        set
        {
            if (_settings.OutputFolder != value)
            {
                _settings.OutputFolder = value;
                Raise(nameof(OutputFolder));
                SavedMessage = null;
            }
        }
    }

    /// <summary>Set to a confirmation once the settings have been written to disk.</summary>
    public string? SavedMessage
    {
        get => _savedMessage;
        private set => Set(ref _savedMessage, value);
    }

    public RelayCommand BrowseOutputCommand { get; }

    public RelayCommand SaveCommand { get; }

    public string AppName => "InDows";

    public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";

    public string RepoUrl => "https://github.com/hess0ul/InDows";

    private void BrowseOutput()
    {
        var picked = _browser.PickFolder("Pick the default output folder");
        if (picked is not null)
        {
            OutputFolder = picked;
        }
    }

    private void Save()
    {
        _store.Save(_settings);
        SavedMessage = "Settings saved.";
    }
}
