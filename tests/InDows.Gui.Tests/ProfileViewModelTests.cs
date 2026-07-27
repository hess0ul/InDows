using InDows.Core.Profile;
using InDows.Gui.ViewModels;
using Xunit;

namespace InDows.Gui.Tests;

public class ProfileViewModelTests
{
    [Fact]
    public void Browsing_reads_and_shows_the_profile()
    {
        var summary = new ProfileSummary(
            new[] { "Brave.Brave" },
            2,
            new[] { new SettingsBucket("Redo by hand", new[] { "misc" }, 3) },
            []);
        var vm = new ProfileViewModel(new FakeProfileReader(summary), new FakeFolderBrowser(@"C:\profile"));

        vm.BrowseCommand.Execute(null);

        Assert.Equal(@"C:\profile", vm.ProfileFolder);
        Assert.NotNull(vm.Summary);
        Assert.Single(vm.Summary!.ActiveApps);
        Assert.Null(vm.Error);
    }

    [Fact]
    public void A_read_failure_becomes_an_error_not_a_crash()
    {
        var vm = new ProfileViewModel(new FakeProfileReader(failure: new InvalidOperationException("nope")), new FakeFolderBrowser(null));

        vm.Load(@"C:\bad");

        Assert.Null(vm.Summary);
        Assert.NotNull(vm.Error);
        Assert.Contains("nope", vm.Error);
    }

    [Fact]
    public void Cancelling_the_browse_leaves_the_state_untouched()
    {
        var vm = new ProfileViewModel(new FakeProfileReader(), new FakeFolderBrowser(null));

        vm.BrowseCommand.Execute(null);

        Assert.Equal("", vm.ProfileFolder);
        Assert.Null(vm.Summary);
    }
}
