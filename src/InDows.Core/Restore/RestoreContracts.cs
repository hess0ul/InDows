namespace InDows.Core.Restore;

/// <summary>
/// A restore request: which ReDows backup folder to read, whether to put files back at their original
/// locations or rebuild the tree under a chosen folder, and the vault password (null leaves the secrets
/// vault untouched). The shape mirrors the ReDows restore contract so its engine can back it directly.
/// </summary>
public sealed record RestoreRequest(string BackupFolder, bool ToOriginalLocations, string? TargetFolder, string? VaultPassword);

/// <summary>Progress while restoring: how many items so far, and the path currently being written.</summary>
public sealed record RestoreProgress(long Items, string CurrentPath);

/// <summary>One file that could not be restored, with the reason (a hash mismatch, a locked target).</summary>
public sealed record RestoreFailureRow(string Path, string Reason);

/// <summary>The outcome, already shaped for display: counts as text, the worst failures, and the hash-verify note.</summary>
public sealed record RestoreResultView(
    string RestoredText,
    string SkippedText,
    string FailedText,
    string SecretsText,
    IReadOnlyList<RestoreFailureRow> TopFailures,
    string? VerifiedText = null);

/// <summary>
/// Runs a restore. Implemented per platform (a fake in tests, later the real Windows engine), so the
/// Restore view-model stays testable and does not decide, on its own, how files land on disk.
/// </summary>
public interface IRestoreRunner
{
    Task<RestoreResultView> RunAsync(RestoreRequest request, IProgress<RestoreProgress> progress, CancellationToken cancellationToken);
}
