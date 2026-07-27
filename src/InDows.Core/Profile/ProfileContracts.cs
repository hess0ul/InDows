namespace InDows.Core.Profile;

/// <summary>
/// Reads a ReDows profile folder (the apps + settings export) into a summary InDows can show. A seam, so
/// the Profile view-model is testable off a fake. The real reader parses the documented profile files.
/// </summary>
public interface IProfileReader
{
    ProfileSummary Read(string profileFolder);
}

/// <summary>What a ReDows profile export contains, summarised: the apps to reinstall, the settings by bucket,
/// and <see cref="SettingModules"/> — the module names the captured settings map to (which InDows selects).</summary>
public sealed record ProfileSummary(
    IReadOnlyList<string> ActiveApps,
    int CommentedApps,
    IReadOnlyList<SettingsBucket> Buckets,
    IReadOnlyList<string> SettingModules);

/// <summary>One group of settings from the profile: what happens to them, which modules, and how many.</summary>
public sealed record SettingsBucket(string Name, IReadOnlyList<string> Modules, int SettingCount);
