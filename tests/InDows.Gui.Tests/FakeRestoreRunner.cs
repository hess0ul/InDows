using InDows.Core.Restore;

namespace InDows.Gui.Tests;

/// <summary>A restore runner that records the request it was given and returns a canned result. Lets the
/// Restore view-model be tested without any file work.</summary>
internal sealed class FakeRestoreRunner(RestoreResultView? result = null) : IRestoreRunner
{
    public RestoreRequest? LastRequest { get; private set; }

    public Task<RestoreResultView> RunAsync(RestoreRequest request, IProgress<RestoreProgress> progress, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(result ?? new RestoreResultView("1", "0", "0", "1 restored", []));
    }
}
