using InDows.Core.Build;
using InDows.Core.Context;
using InDows.Core.Profile;
using InDows.Core.Restore;
using InDows.Gui.Dialogs;
using InDows.Gui.Navigation;
using InDows.Gui.Settings;

namespace InDows.Gui.ViewModels;

/// <summary>
/// The window's brain. It owns every screen's view-model and the current one, and exposes a ShowXxx
/// command per nav entry. It takes its dependencies as interface seams (today just the context source),
/// so a test drives it with fakes, no window or disk involved.
/// </summary>
public sealed class ShellViewModel : ViewModelBase
{
    private object _currentViewModel;
    private string _currentScreen = "home";

    public ShellViewModel(IContextSource context, IRestoreRunner restoreRunner, IProfileReader profileReader, IModuleCatalog moduleCatalog, IBaseTemplate baseTemplate, IFileSaver fileSaver, IFolderBrowser folderBrowser, AppSettings appSettings, ISettingsStore settingsStore, IAppSearch appSearch)
    {
        Home = new HomeViewModel(context);
        Build = new BuildViewModel(moduleCatalog, baseTemplate, fileSaver, profileReader, folderBrowser, appSearch);
        Profile = new ProfileViewModel(profileReader, folderBrowser);
        Restore = new RestoreViewModel(restoreRunner, folderBrowser);
        Settings = new SettingsViewModel(appSettings, settingsStore, folderBrowser);

        _currentViewModel = Home;

        ShowHomeCommand = new RelayCommand(_ => Show(Home, "home"));
        ShowBuildCommand = new RelayCommand(_ => Show(Build, "build"));
        ShowProfileCommand = new RelayCommand(_ => Show(Profile, "profile"));
        ShowRestoreCommand = new RelayCommand(_ => Show(Restore, "restore"));
        ShowSettingsCommand = new RelayCommand(_ => Show(Settings, "settings"));
    }

    public HomeViewModel Home { get; }

    public BuildViewModel Build { get; }

    public ProfileViewModel Profile { get; }

    public RestoreViewModel Restore { get; }

    public SettingsViewModel Settings { get; }

    public RelayCommand ShowHomeCommand { get; }

    public RelayCommand ShowBuildCommand { get; }

    public RelayCommand ShowProfileCommand { get; }

    public RelayCommand ShowRestoreCommand { get; }

    public RelayCommand ShowSettingsCommand { get; }

    public object CurrentViewModel
    {
        get => _currentViewModel;
        private set => Set(ref _currentViewModel, value);
    }

    /// <summary>
    /// The key of the current screen. The nav radio buttons bind to it two-way to stay lit; only the
    /// ShowXxx commands actually change it (the converter's ConvertBack is a no-op).
    /// </summary>
    public string CurrentScreen
    {
        get => _currentScreen;
        set => Set(ref _currentScreen, value);
    }

    /// <summary>Load the Home screen's context once at startup. A provider failure surfaces on Home, never a crash.</summary>
    public void Initialize() => Home.Load();

    private void Show(object viewModel, string screen)
    {
        CurrentViewModel = viewModel;
        CurrentScreen = screen;
    }
}
