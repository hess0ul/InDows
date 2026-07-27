using System.Text.Json;
using System.Text.RegularExpressions;

namespace InDows.Core.Profile;

/// <summary>
/// The real profile reader: parses a ReDows profile folder into a <see cref="ProfileSummary"/>. Apps come
/// from <c>configuration.dsc.yaml</c> (the winget ids, active vs commented out), settings from
/// <c>settings-profile.json</c> (grouped into the buckets ReDows sorts them into). Neither file present
/// means the folder is not a profile export, and that is reported as an exception the screen can show.
/// </summary>
public sealed partial class FileProfileReader : IProfileReader
{
    private const string AppsFileName = "configuration.dsc.yaml";
    private const string SettingsFileName = "settings-profile.json";

    public ProfileSummary Read(string profileFolder)
    {
        var appsPath = Path.Combine(profileFolder, AppsFileName);
        var settingsPath = Path.Combine(profileFolder, SettingsFileName);

        if (!File.Exists(appsPath) && !File.Exists(settingsPath))
        {
            throw new InvalidOperationException(
                $"That folder is not a ReDows profile export (no {AppsFileName} or {SettingsFileName}): '{profileFolder}'.");
        }

        var (active, commented) = ReadApps(appsPath);
        var (buckets, settingModules) = ReadSettings(settingsPath);
        return new ProfileSummary(active, commented, buckets, settingModules);
    }

    private static (IReadOnlyList<string> Active, int Commented) ReadApps(string path)
    {
        if (!File.Exists(path))
        {
            return ([], 0);
        }

        var active = new List<string>();
        var commented = 0;
        foreach (var raw in File.ReadLines(path))
        {
            var match = WinGetIdPattern().Match(raw);
            if (!match.Success)
            {
                continue;
            }

            if (raw.TrimStart().StartsWith('#'))
            {
                commented++;
            }
            else
            {
                active.Add(match.Groups[1].Value);
            }
        }

        return (active, commented);
    }

    private static (IReadOnlyList<SettingsBucket> Buckets, IReadOnlyList<string> SettingModules) ReadSettings(string path)
    {
        if (!File.Exists(path))
        {
            return ([], []);
        }

        ProfileDto? profile;
        try
        {
            profile = JsonSerializer.Deserialize<ProfileDto>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            // The file is present but unparseable — surface it, rather than silently dropping every setting.
            throw new InvalidOperationException($"The profile's {SettingsFileName} is malformed: {ex.Message}", ex);
        }

        if (profile is null)
        {
            return ([], []);
        }

        var buckets = new List<SettingsBucket>();
        Add(buckets, "Modules already built (re-applied automatically)", profile.ExistingModules);
        Add(buckets, "Modules still to build", profile.NewModules);
        Add(buckets, "Applied by the base install", profile.ByBase);
        Add(buckets, "Private config only (public leaves the default)", profile.PrivateOnly);
        Add(buckets, "Module exists but the setting is off", profile.NotApplied);
        Add(buckets, "Redo by hand", profile.Manual);
        Add(buckets, "Capture-only (no module)", profile.NotInLoop);
        Add(buckets, "Unreadable", profile.Unreadable);

        // The modules whose settings the user had on and wants applied: InDows ticks the matching ones.
        // (Not the "off" or base-handled buckets — only what maps to a module the user actually wants.)
        var settingModules = Names(profile.ExistingModules).Concat(Names(profile.NewModules))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return (buckets, settingModules);
    }

    private static IEnumerable<string> Names(IReadOnlyList<ModuleDto>? modules) =>
        (modules ?? []).Select(m => m.Module).Where(m => !string.IsNullOrEmpty(m)).Select(m => m!);

    private static void Add(List<SettingsBucket> buckets, string name, IReadOnlyList<ModuleDto>? modules)
    {
        if (modules is null || modules.Count == 0)
        {
            return;
        }

        var names = modules.Select(m => m.Module).Where(m => !string.IsNullOrEmpty(m)).Select(m => m!).ToList();
        var count = modules.Sum(m => m.Settings?.Count ?? 0);
        buckets.Add(new SettingsBucket(name, names, count));
    }

    [GeneratedRegex(@"id:\s*([\w.+-]+),\s*source:\s*winget", RegexOptions.IgnoreCase)]
    private static partial Regex WinGetIdPattern();

    private sealed record ProfileDto(
        IReadOnlyList<ModuleDto>? ExistingModules,
        IReadOnlyList<ModuleDto>? NewModules,
        IReadOnlyList<ModuleDto>? ByBase,
        IReadOnlyList<ModuleDto>? PrivateOnly,
        IReadOnlyList<ModuleDto>? NotApplied,
        IReadOnlyList<ModuleDto>? Manual,
        IReadOnlyList<ModuleDto>? NotInLoop,
        IReadOnlyList<ModuleDto>? Unreadable);

    private sealed record ModuleDto(string? Module, IReadOnlyList<JsonElement>? Settings);
}
