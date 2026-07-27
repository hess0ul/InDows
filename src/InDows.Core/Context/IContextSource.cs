namespace InDows.Core.Context;

/// <summary>
/// Reads a read-only snapshot of the current machine for the Home screen. Implemented per platform
/// (a fake in tests, the real Windows reader in the providers project), so the view-model stays testable.
/// </summary>
public interface IContextSource
{
    MachineContext Load();
}

/// <summary>The machine as InDows sees it: identity, OS, and its drives.</summary>
public sealed record MachineContext(string MachineName, string OsDescription, IReadOnlyList<DriveContext> Drives);

/// <summary>One drive: mount name, filesystem, capacity. Sizes are null when the drive is not ready.</summary>
public sealed record DriveContext(string Name, string Format, long? TotalBytes, long? FreeBytes, bool Ready);
