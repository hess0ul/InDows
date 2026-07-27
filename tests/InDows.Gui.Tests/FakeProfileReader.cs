using InDows.Core.Profile;

namespace InDows.Gui.Tests;

/// <summary>A profile reader that returns a canned summary, or throws on demand to exercise the error path.</summary>
internal sealed class FakeProfileReader(ProfileSummary? summary = null, Exception? failure = null) : IProfileReader
{
    public ProfileSummary Read(string profileFolder) =>
        failure is not null ? throw failure : summary ?? new ProfileSummary([], 0, [], []);
}
