using InDows.Core.Build;

namespace InDows.Gui.Tests;

/// <summary>A module catalog that returns a fixed list, or throws on demand to exercise the error path.</summary>
internal sealed class FakeModuleCatalog(IReadOnlyList<ModuleCatalogEntry>? modules = null, Exception? failure = null) : IModuleCatalog
{
    public IReadOnlyList<ModuleCatalogEntry> Load() =>
        failure is not null ? throw failure : modules ?? [];
}
