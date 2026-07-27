using System.IO;
using System.Linq;
using InDows.Core.Profile;
using Xunit;

namespace InDows.Gui.Tests;

public sealed class FileProfileReaderTests : IDisposable
{
    private readonly string _folder;

    public FileProfileReaderTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "indows-profile-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    [Fact]
    public void Reads_active_and_commented_apps_from_the_dsc_yaml()
    {
        File.WriteAllText(Path.Combine(_folder, "configuration.dsc.yaml"),
            "resources:\n" +
            "  - settings: { id: Brave.Brave, source: winget }\n" +
            "  - settings: { id: 7zip.7zip, source: winget }\n" +
            "#   - settings: { id: Some.Thing, source: winget }\n");

        var summary = new FileProfileReader().Read(_folder);

        Assert.Equal(new[] { "Brave.Brave", "7zip.7zip" }, summary.ActiveApps);
        Assert.Equal(1, summary.CommentedApps);
    }

    [Fact]
    public void Reads_settings_buckets_from_the_json()
    {
        File.WriteAllText(Path.Combine(_folder, "settings-profile.json"),
            "{ \"ExistingModules\": [ { \"Module\": \"misc\", \"Settings\": [ {}, {} ] } ], " +
            "  \"Manual\": [ { \"Module\": \"gaming\", \"Settings\": [ {} ] } ] }");

        var summary = new FileProfileReader().Read(_folder);

        var existing = summary.Buckets.Single(b => b.Name.Contains("already built"));
        Assert.Equal(new[] { "misc" }, existing.Modules);
        Assert.Equal(2, existing.SettingCount);
        Assert.Contains(summary.Buckets, b => b.Name == "Redo by hand");
    }

    [Fact]
    public void A_folder_that_is_not_a_profile_export_is_rejected()
    {
        Assert.Throws<InvalidOperationException>(() => new FileProfileReader().Read(_folder));
    }

    [Fact]
    public void A_present_but_malformed_settings_file_is_rejected()
    {
        File.WriteAllText(Path.Combine(_folder, "settings-profile.json"), "{ not valid json");

        Assert.Throws<InvalidOperationException>(() => new FileProfileReader().Read(_folder));
    }
}
