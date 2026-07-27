using System.IO;
using System.Windows;
using System.Windows.Threading;
using InDows.Core.Build;
using InDows.Core.Profile;
using InDows.Gui.Dialogs;
using InDows.Gui.Settings;
using InDows.Gui.ViewModels;
using InDows.Providers.Windows.Apps;
using InDows.Providers.Windows.Context;
using InDows.Providers.Windows.Restore;

namespace InDows.Gui;

/// <summary>
/// Application entry point. The shell is built here with the real, read-only seams: the machine context
/// source, a folder picker, and the restore engine. These are the same seams a test replaces with fakes.
/// It is then handed to the window.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Diagnostic: append any unhandled UI-thread exception to a log and keep the app alive, so a crash
        // on a screen can be inspected instead of just closing the window.
        DispatcherUnhandledException += OnUnhandled;

        var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
        var catalog = new JsonModuleCatalog(Path.Combine(dataDir, "modules.catalog.json"));
        var baseTemplate = new FileBaseTemplate(Path.Combine(dataDir, "autounattend.base.xml"));
        var settingsStore = new FileSettingsStore();
        var appSettings = settingsStore.Load();
        var shell = new ShellViewModel(new WindowsContextSource(), new WindowsRestoreRunner(), new FileProfileReader(), catalog, baseTemplate, new WindowsFileSaver(appSettings), new WindowsFolderBrowser(), appSettings, settingsStore, new WingetAppSearch());
        var window = new MainWindow(shell);
        shell.Initialize();
        window.Show();
    }

    private static void OnUnhandled(object sender, DispatcherUnhandledExceptionEventArgs args)
    {
        try
        {
            File.AppendAllText(
                Path.Combine(Path.GetTempPath(), "indows-crash.log"),
                "=== " + args.Exception.GetType().Name + " ===" + Environment.NewLine + args.Exception + Environment.NewLine + Environment.NewLine);
        }
        catch (IOException)
        {
        }

        args.Handled = true;
    }
}
