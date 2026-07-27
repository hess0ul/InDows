namespace InDows.Core.Restore;

/// <summary>
/// Maps a file in the backup tree to the target path(s) to restore it to (pure, so it is unit-tested). A
/// normal file goes back to its one original location; a de-duplicated file (found in the restore map) goes
/// back to EVERY place its content belonged. In "original locations" mode the targets are the real Windows
/// paths; otherwise they are rebuilt under a chosen folder, keeping the drive-as-folder layout so nothing
/// collides.
/// </summary>
public sealed class RestorePlan
{
    private readonly Dictionary<string, IReadOnlyList<string>> _dedupByStored;
    private readonly bool _toOriginal;
    private readonly string? _targetFolder;

    public RestorePlan(IReadOnlyList<RestoreMapEntry> restoreMap, bool toOriginalLocations, string? targetFolder)
    {
        _dedupByStored = restoreMap.ToDictionary(e => e.StoredAt, e => e.BelongsAt, StringComparer.OrdinalIgnoreCase);
        _toOriginal = toOriginalLocations;
        _targetFolder = targetFolder;
    }

    /// <summary>Every target (Windows path) a given backup-relative file must be written to.</summary>
    public IReadOnlyList<string> TargetsFor(string backupRelativePath)
    {
        var rel = backupRelativePath.Replace('\\', '/');
        var originals = _dedupByStored.TryGetValue(rel, out var belongsAt)
            ? belongsAt
            : [BackupFormat.OriginalPath(rel)];
        return originals.Select(ToTarget).ToList();
    }

    private string ToTarget(string originalPath) => _toOriginal
        ? originalPath.Replace('/', '\\')
        : Path.Combine(_targetFolder!, BackupFormat.RelativePath(originalPath).Replace('/', '\\'));
}
