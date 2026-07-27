using System.IO;
using InDows.Gui.Settings;
using Xunit;

namespace InDows.Gui.Tests;

public sealed class FileSettingsStoreTests : IDisposable
{
    private readonly string _path;

    public FileSettingsStoreTests()
    {
        _path = Path.Combine(Path.GetTempPath(), "indows-settings-test-" + Guid.NewGuid().ToString("N") + ".json");
    }

    public void Dispose()
    {
        try
        {
            File.Delete(_path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    [Fact]
    public void Saves_and_reloads_settings()
    {
        var store = new FileSettingsStore(_path);

        store.Save(new AppSettings { OutputFolder = @"C:\answer-files" });

        Assert.Equal(@"C:\answer-files", store.Load().OutputFolder);
    }

    [Fact]
    public void A_missing_file_yields_defaults()
    {
        var store = new FileSettingsStore(_path);   // nothing saved yet

        Assert.Equal("", store.Load().OutputFolder);
    }
}
