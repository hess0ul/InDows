using InDows.Core.Restore;
using InDows.Gui.ViewModels;
using Xunit;

namespace InDows.Gui.Tests;

public class RestoreViewModelTests
{
    [Fact]
    public void Cannot_run_until_a_backup_folder_is_set()
    {
        var vm = new RestoreViewModel(new FakeRestoreRunner(), new FakeFolderBrowser(null));

        Assert.False(vm.RunCommand.CanExecute(null));

        vm.BackupFolder = @"C:\backup";

        Assert.True(vm.RunCommand.CanExecute(null));
    }

    [Fact]
    public void Browsing_fills_the_backup_path()
    {
        var vm = new RestoreViewModel(new FakeRestoreRunner(), new FakeFolderBrowser(@"D:\redows-backup"));

        vm.BrowseBackupCommand.Execute(null);

        Assert.Equal(@"D:\redows-backup", vm.BackupFolder);
    }

    [Fact]
    public void Browsing_a_target_switches_off_restore_to_original_locations()
    {
        var vm = new RestoreViewModel(new FakeRestoreRunner(), new FakeFolderBrowser(@"E:\rebuild"));

        vm.BrowseTargetCommand.Execute(null);

        Assert.Equal(@"E:\rebuild", vm.TargetFolder);
        Assert.False(vm.ToOriginalLocations);
    }

    [Fact]
    public async Task Running_passes_the_request_and_shows_the_result()
    {
        var runner = new FakeRestoreRunner(new RestoreResultView("42", "3", "0", "5 restored", []));
        var vm = new RestoreViewModel(runner, new FakeFolderBrowser(null))
        {
            BackupFolder = @"C:\backup",
            VaultPassword = "pw",
        };

        vm.RunCommand.Execute(null);
        await WaitUntil(() => !vm.IsRunning);

        Assert.NotNull(runner.LastRequest);
        Assert.Equal(@"C:\backup", runner.LastRequest!.BackupFolder);
        Assert.Equal("pw", runner.LastRequest.VaultPassword);
        Assert.True(runner.LastRequest.ToOriginalLocations);
        Assert.NotNull(vm.Result);
        Assert.Equal("42", vm.Result!.RestoredText);
        Assert.Null(vm.Error);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var i = 0; i < 100 && !condition(); i++)
        {
            await Task.Delay(10);
        }
    }
}
