using InDows.Core.Build;

namespace InDows.Gui.Tests;

/// <summary>An app search that returns a fixed set of results, or throws on demand to exercise the error path.</summary>
internal sealed class FakeAppSearch(AppSearchResult[]? results = null, Exception? failure = null) : IAppSearch
{
    public IReadOnlyList<AppSearchResult> Search(string query) =>
        failure is not null ? throw failure : results ?? [];
}
