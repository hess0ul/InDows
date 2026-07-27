using InDows.Gui.Settings;
using InDows.Gui.ViewModels;
using Xunit;

namespace InDows.Gui.Tests;

public class SettingsViewModelTests
{
    [Fact]
    public void Editing_the_output_folder_updates_the_shared_settings()
    {
        var settings = new AppSettings();
        var vm = new SettingsViewModel(settings, new FakeSettingsStore(), new FakeFolderBrowser(null));

        vm.OutputFolder = @"D:\answer-files";

        Assert.Equal(@"D:\answer-files", settings.OutputFolder);   // the shared holder is updated live
    }

    [Fact]
    public void Browsing_sets_the_output_folder()
    {
        var vm = new SettingsViewModel(new AppSettings(), new FakeSettingsStore(), new FakeFolderBrowser(@"E:\dst"));

        vm.BrowseOutputCommand.Execute(null);

        Assert.Equal(@"E:\dst", vm.OutputFolder);
    }

    [Fact]
    public void Save_persists_through_the_store_and_confirms()
    {
        var settings = new AppSettings { OutputFolder = @"C:\x" };
        var store = new FakeSettingsStore();
        var vm = new SettingsViewModel(settings, store, new FakeFolderBrowser(null));

        vm.SaveCommand.Execute(null);

        Assert.Same(settings, store.Saved);
        Assert.Contains("saved", vm.SavedMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void It_exposes_about_info()
    {
        var vm = new SettingsViewModel(new AppSettings(), new FakeSettingsStore(), new FakeFolderBrowser(null));

        Assert.Equal("InDows", vm.AppName);
        Assert.False(string.IsNullOrEmpty(vm.Version));
        Assert.Contains("hess0ul/InDows", vm.RepoUrl);
    }
}
