using InDows.Core.Context;
using InDows.Gui.ViewModels;
using Xunit;

namespace InDows.Gui.Tests;

public class HomeViewModelTests
{
    [Fact]
    public void Load_shapes_drives_and_a_summary()
    {
        var context = new MachineContext("BOX", "Windows 11", new[]
        {
            new DriveContext("C:\\", "NTFS", 500_000_000_000, 200_000_000_000, true),
            new DriveContext("D:\\", "?", null, null, false),
        });
        var home = new HomeViewModel(new FakeContextSource(context));

        home.Load();

        Assert.Equal(2, home.Drives.Count);
        Assert.Contains("BOX", home.Summary);
        Assert.Contains("free", home.Drives[0].Detail);
        Assert.Equal("not ready", home.Drives[1].Detail);
        Assert.Null(home.Error);
    }

    [Fact]
    public void Load_surfaces_a_provider_failure_instead_of_crashing()
    {
        var home = new HomeViewModel(new FakeContextSource(failure: new InvalidOperationException("boom")));

        home.Load();

        Assert.NotNull(home.Error);
        Assert.Contains("boom", home.Error);
        Assert.Empty(home.Drives);
    }
}
