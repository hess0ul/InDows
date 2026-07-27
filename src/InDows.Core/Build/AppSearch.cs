namespace InDows.Core.Build;

/// <summary>One hit from an app search: what the user sees, and the winget id it maps to.</summary>
public sealed record AppSearchResult(string Name, string Id);

/// <summary>Searches the app catalog (winget) by name. A seam, so the Build screen is testable off a fake.</summary>
public interface IAppSearch
{
    IReadOnlyList<AppSearchResult> Search(string query);
}
