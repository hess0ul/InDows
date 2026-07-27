using InDows.Core.Profile;
using InDows.Gui.Dialogs;
using InDows.Gui.Navigation;

namespace InDows.Gui.ViewModels;

/// <summary>
/// The Profile screen's brain: point at a ReDows profile folder and show what it will bring back, the apps
/// to reinstall and the settings grouped by what happens to them. Reads through <see cref="IProfileReader"/>
/// only, so it is testable off a fake. A read failure becomes <see cref="Error"/>, never a crash.
/// </summary>
public sealed class ProfileViewModel : ViewModelBase
{
    private readonly IProfileReader _reader;
    private readonly IFolderBrowser _browser;

    private string _profileFolder = "";
    private ProfileSummary? _summary;
    private string? _error;

    public ProfileViewModel(IProfileReader reader, IFolderBrowser browser)
    {
        _reader = reader;
        _browser = browser;
        BrowseCommand = new RelayCommand(_ => Browse());
    }

    public string ProfileFolder
    {
        get => _profileFolder;
        private set => Set(ref _profileFolder, value);
    }

    public ProfileSummary? Summary
    {
        get => _summary;
        private set => Set(ref _summary, value);
    }

    public string? Error
    {
        get => _error;
        private set => Set(ref _error, value);
    }

    public RelayCommand BrowseCommand { get; }

    private void Browse()
    {
        var picked = _browser.PickFolder("Pick the ReDows profile folder");
        if (picked is not null)
        {
            Load(picked);
        }
    }

    public void Load(string folder)
    {
        ProfileFolder = folder;
        try
        {
            Summary = _reader.Read(folder);
            Error = null;
        }
        catch (Exception ex)
        {
            Summary = null;
            Error = $"Could not read that profile: {ex.GetType().Name}: {ex.Message}";
        }
    }
}
