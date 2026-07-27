using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using InDows.Core.Build;
using InDows.Core.Profile;
using InDows.Gui.Dialogs;
using InDows.Gui.Navigation;

namespace InDows.Gui.ViewModels;

/// <summary>
/// The Build screen's brain. Pick the install mode, tick modules (and their individual settings) from a
/// bundled, categorised checklist, then Generate the autounattend.xml — grafting the selected modules onto the
/// base — into a preview, and Save it. All I/O is behind seams (catalog, base template, file saver), so it is
/// testable off fakes; a load or generation failure becomes <see cref="Error"/> rather than a crash.
/// </summary>
public sealed partial class BuildViewModel : ViewModelBase
{
    private readonly IModuleCatalog _catalog;
    private readonly IBaseTemplate _baseTemplate;
    private readonly IFileSaver _fileSaver;
    private readonly IProfileReader _profileReader;
    private readonly IFolderBrowser _browser;
    private readonly IAppSearch _appSearch;

    private BuildMode _mode = BuildMode.Clean;
    private string _summary = "";
    private string? _error;
    private string? _previewXml;
    private string? _placeholderWarning;
    private string? _savedPath;
    private string _profileFolder = "";
    private string? _profileInfo;
    private string? _profileError;
    private IReadOnlyList<string> _profileApps = [];
    private bool _isAddingApp;
    private string _appSearchQuery = "";
    private bool _searching;
    private string? _searchError;

    public BuildViewModel(IModuleCatalog catalog, IBaseTemplate baseTemplate, IFileSaver fileSaver,
        IProfileReader profileReader, IFolderBrowser browser, IAppSearch appSearch)
    {
        _catalog = catalog;
        _baseTemplate = baseTemplate;
        _fileSaver = fileSaver;
        _profileReader = profileReader;
        _browser = browser;
        _appSearch = appSearch;
        GenerateCommand = new RelayCommand(_ => Generate());
        SaveCommand = new RelayCommand(_ => Save());
        BrowseProfileCommand = new RelayCommand(_ => BrowseProfile());
        StartAddAppCommand = new RelayCommand(_ => IsAddingApp = true);
        CancelAddAppCommand = new RelayCommand(_ => IsAddingApp = false);
        SearchAppsCommand = new RelayCommand(async _ => await SearchAppsAsync());
        PickAppCommand = new RelayCommand(app => PickApp(app as AppSearchResult));
        Load();
    }

    /// <summary>Modules grouped by category, each group alphabetised, for the checklist sections.</summary>
    public ObservableCollection<ModuleCategory> Categories { get; } = [];

    public BuildMode Mode
    {
        get => _mode;
        set
        {
            Set(ref _mode, value);
            Raise(nameof(RequiresProfile));
            Recompute();
        }
    }

    /// <summary>M2 and M3 pull apps from a ReDows profile, so the profile picker shows for those modes.</summary>
    public bool RequiresProfile => _mode is BuildMode.CleanWithProfile or BuildMode.FullRestore;

    public string ProfileFolder
    {
        get => _profileFolder;
        private set => Set(ref _profileFolder, value);
    }

    /// <summary>How many apps the loaded profile will add, once one is picked.</summary>
    public string? ProfileInfo
    {
        get => _profileInfo;
        private set => Set(ref _profileInfo, value);
    }

    public string? ProfileError
    {
        get => _profileError;
        private set => Set(ref _profileError, value);
    }

    public RelayCommand BrowseProfileCommand { get; }

    private void BrowseProfile()
    {
        var picked = _browser.PickFolder("Pick the ReDows profile folder");
        if (picked is not null)
        {
            LoadProfile(picked);
        }
    }

    public void LoadProfile(string folder)
    {
        ProfileFolder = folder;
        try
        {
            var summary = _profileReader.Read(folder);
            _profileApps = summary.ActiveApps;

            // Tick the modules the profile's captured settings map to; note any InDows doesn't have.
            var byName = AllModules.ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
            var applied = 0;
            var unavailable = new List<string>();
            foreach (var name in summary.SettingModules)
            {
                if (byName.TryGetValue(name, out var module))
                {
                    module.IsSelected = true;
                    applied++;
                }
                else
                {
                    unavailable.Add(name);
                }
            }

            ProfileInfo = $"{_profileApps.Count} app(s) + {applied} setting module(s) selected from the profile"
                + (unavailable.Count > 0 ? $"; not available in InDows: {string.Join(", ", unavailable)}" : ".");
            ProfileError = null;
        }
        catch (Exception ex)
        {
            _profileApps = [];
            ProfileInfo = null;
            ProfileError = $"Could not read that profile: {ex.GetType().Name}: {ex.Message}";
        }
    }

    public string Summary
    {
        get => _summary;
        private set => Set(ref _summary, value);
    }

    public string? Error
    {
        get => _error;
        private set => Set(ref _error, value);
    }

    public RelayCommand GenerateCommand { get; }

    public RelayCommand SaveCommand { get; }

    /// <summary>The composed autounattend.xml after Generate, shown in the preview; null before generating.</summary>
    public string? PreviewXml
    {
        get => _previewXml;
        private set
        {
            Set(ref _previewXml, value);
            Raise(nameof(HasPreview));
        }
    }

    public bool HasPreview => _previewXml is not null;

    /// <summary>Set when the generated file still holds <c>__PLACEHOLDER__</c> values the user must fill in.</summary>
    public string? PlaceholderWarning
    {
        get => _placeholderWarning;
        private set => Set(ref _placeholderWarning, value);
    }

    /// <summary>Set to a confirmation once the preview has been saved.</summary>
    public string? SavedPath
    {
        get => _savedPath;
        private set => Set(ref _savedPath, value);
    }

    /// <summary>Extra apps the user picked to install at first login (added to the base essentials).</summary>
    public ObservableCollection<AppRow> Apps { get; } = [];

    public RelayCommand StartAddAppCommand { get; }

    public RelayCommand CancelAddAppCommand { get; }

    public RelayCommand SearchAppsCommand { get; }

    public RelayCommand PickAppCommand { get; }

    /// <summary>Results of the last app search, offered for the user to pick from.</summary>
    public ObservableCollection<AppSearchResult> AppSearchResults { get; } = [];

    /// <summary>Whether the add-an-app search panel is open.</summary>
    public bool IsAddingApp
    {
        get => _isAddingApp;
        set
        {
            Set(ref _isAddingApp, value);
            if (!value)
            {
                AppSearchResults.Clear();
                AppSearchQuery = "";
                SearchError = null;
            }
        }
    }

    public string AppSearchQuery
    {
        get => _appSearchQuery;
        set => Set(ref _appSearchQuery, value);
    }

    public bool Searching
    {
        get => _searching;
        private set => Set(ref _searching, value);
    }

    public string? SearchError
    {
        get => _searchError;
        private set => Set(ref _searchError, value);
    }

    /// <summary>Run a winget search for the typed name; results go to <see cref="AppSearchResults"/> to pick from.</summary>
    public async Task SearchAppsAsync()
    {
        if (string.IsNullOrWhiteSpace(_appSearchQuery))
        {
            return;
        }

        Searching = true;
        SearchError = null;
        AppSearchResults.Clear();
        try
        {
            var query = _appSearchQuery;
            var results = await Task.Run(() => _appSearch.Search(query));
            foreach (var result in results)
            {
                AppSearchResults.Add(result);
            }

            if (results.Count == 0)
            {
                SearchError = "No matches — try another name.";
            }
        }
        catch (Exception ex)
        {
            SearchError = $"Search failed: {ex.GetType().Name}: {ex.Message}";
        }
        finally
        {
            Searching = false;
        }
    }

    private void PickApp(AppSearchResult? app)
    {
        if (app is null)
        {
            return;
        }

        if (!Apps.Any(a => string.Equals(a.Id, app.Id, StringComparison.OrdinalIgnoreCase)))
        {
            Apps.Add(new AppRow(app, row => Apps.Remove(row)));
        }

        IsAddingApp = false;
    }

    private void Generate()
    {
        SavedPath = null;
        try
        {
            var grafts = AllModules.Select(m => m.ToGraft()).OfType<ModuleGraft>().ToList();
            var xml = AutounattendGenerator.Compose(_baseTemplate.Read(), grafts);

            // Apps to install at first login: the user's picks, plus the profile's apps in M2/M3.
            var apps = Apps.Select(a => a.Id).ToList();
            if (RequiresProfile)
            {
                apps.AddRange(_profileApps);
            }

            xml = AutounattendGenerator.AppendProfileApps(xml, apps);

            PreviewXml = xml;
            PlaceholderWarning = DescribePlaceholders(PreviewXml);
            Error = null;
        }
        catch (Exception ex)
        {
            PreviewXml = null;
            PlaceholderWarning = null;
            Error = $"Could not generate: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private void Save()
    {
        if (_previewXml is null)
        {
            return;
        }

        var path = _fileSaver.Save("autounattend.xml", _previewXml);
        if (path is not null)
        {
            SavedPath = $"Saved to {path}";
        }
    }

    private static string? DescribePlaceholders(string xml)
    {
        var found = PlaceholderPattern().Matches(xml).Select(m => m.Value).Distinct().ToList();
        return found.Count == 0
            ? null
            : $"Fill in before use ({found.Count}): {string.Join(", ", found)}";
    }

    [GeneratedRegex("__[A-Z][A-Z0-9_]*__")]
    private static partial Regex PlaceholderPattern();

    public void Load()
    {
        Categories.Clear();
        try
        {
            var entries = _catalog.Load();
            foreach (var group in entries
                         .GroupBy(e => e.Category)
                         .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
            {
                var rows = group
                    .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(e => new ModuleRow(e, Recompute))
                    .ToList();
                Categories.Add(new ModuleCategory(group.Key, rows));
            }

            Error = null;
        }
        catch (Exception ex)
        {
            Error = $"Could not load the module catalog: {ex.GetType().Name}: {ex.Message}";
        }

        Recompute();
    }

    private IEnumerable<ModuleRow> AllModules => Categories.SelectMany(c => c.Modules);

    private void Recompute()
    {
        var settings = AllModules.Sum(m => m.SelectedCount);
        var modules = AllModules.Count(m => m.SelectedCount > 0);
        Summary = $"{ModeLabel(Mode)}, {settings} setting(s) across {modules} module(s)";
    }

    private static string ModeLabel(BuildMode mode) => mode switch
    {
        BuildMode.Clean => "Clean install",
        BuildMode.CleanWithProfile => "Clean install + ReDows profile",
        BuildMode.FullRestore => "Full restore",
        _ => mode.ToString(),
    };
}
