using InDows.Gui.ViewModels;
using Xunit;

namespace InDows.Gui.Tests;

public class ShellViewModelTests
{
    private static ShellViewModel NewShell() =>
        new(new FakeContextSource(), new FakeRestoreRunner(), new FakeProfileReader(), new FakeModuleCatalog(),
            new FakeBaseTemplate("<unattend/>"), new FakeFileSaver(), new FakeFolderBrowser(null),
            new InDows.Gui.Settings.AppSettings(), new FakeSettingsStore(), new FakeAppSearch());

    [Fact]
    public void Starts_on_the_home_screen()
    {
        var shell = NewShell();

        Assert.Equal("home", shell.CurrentScreen);
        Assert.Same(shell.Home, shell.CurrentViewModel);
    }

    [Fact]
    public void Navigating_switches_both_the_screen_key_and_the_current_view_model()
    {
        var shell = NewShell();

        shell.ShowRestoreCommand.Execute(null);

        Assert.Equal("restore", shell.CurrentScreen);
        Assert.Same(shell.Restore, shell.CurrentViewModel);
    }

    [Fact]
    public void Every_nav_command_lands_on_its_own_screen()
    {
        var shell = NewShell();

        shell.ShowBuildCommand.Execute(null);
        Assert.Same(shell.Build, shell.CurrentViewModel);

        shell.ShowProfileCommand.Execute(null);
        Assert.Same(shell.Profile, shell.CurrentViewModel);

        shell.ShowSettingsCommand.Execute(null);
        Assert.Same(shell.Settings, shell.CurrentViewModel);

        shell.ShowHomeCommand.Execute(null);
        Assert.Same(shell.Home, shell.CurrentViewModel);
    }

    [Fact]
    public void Initialize_loads_the_home_context_without_error()
    {
        var shell = NewShell();

        shell.Initialize();

        Assert.Null(shell.Home.Error);
    }
}
