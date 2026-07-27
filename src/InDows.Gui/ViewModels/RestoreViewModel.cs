using InDows.Core.Restore;
using InDows.Gui.Dialogs;
using InDows.Gui.Navigation;

namespace InDows.Gui.ViewModels;

/// <summary>
/// The Restore screen's brain: pick a ReDows backup folder, choose whether files go back to their
/// original locations or into a chosen folder, optionally give the vault password, then run. Progress
/// and the result are display state. It talks to the engine only through <see cref="IRestoreRunner"/>,
/// so it is fully testable off a fake and never touches disk itself.
/// </summary>
public sealed class RestoreViewModel : ViewModelBase
{
    private readonly IRestoreRunner _runner;
    private readonly IFolderBrowser _browser;
    private CancellationTokenSource? _cancellation;

    private string _backupFolder = "";
    private bool _toOriginalLocations = true;
    private string _targetFolder = "";
    private string _vaultPassword = "";
    private bool _isRunning;
    private string _progressText = "";
    private RestoreResultView? _result;
    private string? _error;

    public RestoreViewModel(IRestoreRunner runner, IFolderBrowser browser)
    {
        _runner = runner;
        _browser = browser;

        BrowseBackupCommand = new RelayCommand(_ => BrowseBackup());
        BrowseTargetCommand = new RelayCommand(_ => BrowseTarget());
        RunCommand = new RelayCommand(async _ => await RunAsync(), _ => !IsRunning && BackupFolder.Length > 0);
        CancelCommand = new RelayCommand(_ => _cancellation?.Cancel(), _ => IsRunning);
    }

    public string BackupFolder
    {
        get => _backupFolder;
        set
        {
            Set(ref _backupFolder, value);
            RunCommand.RaiseCanExecuteChanged();
        }
    }

    public bool ToOriginalLocations
    {
        get => _toOriginalLocations;
        set => Set(ref _toOriginalLocations, value);
    }

    public string TargetFolder
    {
        get => _targetFolder;
        set => Set(ref _targetFolder, value);
    }

    /// <summary>The secrets-vault password. Set from the view's PasswordBox (a password is not bindable).</summary>
    public string VaultPassword
    {
        get => _vaultPassword;
        set => Set(ref _vaultPassword, value);
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            Set(ref _isRunning, value);
            RunCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }
    }

    public string ProgressText
    {
        get => _progressText;
        private set => Set(ref _progressText, value);
    }

    public RestoreResultView? Result
    {
        get => _result;
        private set => Set(ref _result, value);
    }

    public string? Error
    {
        get => _error;
        private set => Set(ref _error, value);
    }

    public RelayCommand BrowseBackupCommand { get; }

    public RelayCommand BrowseTargetCommand { get; }

    public RelayCommand RunCommand { get; }

    public RelayCommand CancelCommand { get; }

    private void BrowseBackup()
    {
        var picked = _browser.PickFolder("Pick the ReDows backup folder");
        if (picked is not null)
        {
            BackupFolder = picked;
        }
    }

    private void BrowseTarget()
    {
        var picked = _browser.PickFolder("Pick where to rebuild the files");
        if (picked is not null)
        {
            TargetFolder = picked;
            ToOriginalLocations = false;
        }
    }

    private async Task RunAsync()
    {
        if (IsRunning || BackupFolder.Length == 0)
        {
            return;
        }

        IsRunning = true;
        Result = null;
        Error = null;
        ProgressText = "Starting...";
        var cancellation = new CancellationTokenSource();
        _cancellation = cancellation;

        var request = new RestoreRequest(
            BackupFolder,
            ToOriginalLocations,
            ToOriginalLocations ? null : (TargetFolder.Length > 0 ? TargetFolder : null),
            VaultPassword.Length > 0 ? VaultPassword : null);
        var progress = new Progress<RestoreProgress>(p => ProgressText = $"{p.Items} item(s): {p.CurrentPath}");

        try
        {
            Result = await _runner.RunAsync(request, progress, cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            Error = "Restore cancelled.";
        }
        catch (Exception ex)
        {
            Error = $"Restore could not run: {ex.GetType().Name}: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
            _cancellation = null;
            cancellation.Dispose();
        }
    }
}
