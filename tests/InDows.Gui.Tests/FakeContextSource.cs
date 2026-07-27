using InDows.Core.Context;

namespace InDows.Gui.Tests;

/// <summary>A context source with no machine behind it: it returns a canned snapshot, or throws on demand
/// to exercise the error path. Lets the view-models be tested without touching the real disk.</summary>
internal sealed class FakeContextSource(MachineContext? context = null, Exception? failure = null) : IContextSource
{
    public MachineContext Load() =>
        failure is not null ? throw failure : context ?? new MachineContext("PC", "Windows", []);
}
