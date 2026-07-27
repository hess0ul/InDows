namespace InDows.Core.Restore;

/// <summary>
/// The on-disk shape of a ReDows backup that InDows restores from: the file names at the backup root and
/// the drive-letter path mapping. This is the format contract between the two tools (ReDows writes it,
/// InDows reads it); see docs/backup-format.md. Kept in one place so nothing drifts.
/// </summary>
public static class BackupFormat
{
    /// <summary>The de-duplication map at the backup root (one stored copy, all its original locations).</summary>
    public const string RestoreMapFileName = "redows-restore-map.json";

    /// <summary>The per-file SHA-256 manifest at the backup root (end-to-end integrity proof).</summary>
    public const string HashesFileName = "redows-hashes.json";

    /// <summary>The encrypted secrets vault at the backup root (a standard WinZip AES-256 zip).</summary>
    public const string VaultFileName = "secrets-vault.zip";

    /// <summary>
    /// An original path as a backup-relative one: the drive's ':' becomes a folder, so nothing collides
    /// across volumes (<c>C:\Users\x</c> becomes <c>C/Users/x</c>).
    /// </summary>
    public static string RelativePath(string sourcePath) =>
        string.Join('/', sourcePath.Split('/', '\\', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Replace(":", "", StringComparison.Ordinal)));

    /// <summary>
    /// The inverse: a backup-relative path back to its original (<c>C/Users/x</c> becomes <c>C:/Users/x</c>).
    /// Only the drive segment ever lost a ':' (Windows file names can't contain one), so this is exact.
    /// </summary>
    public static string OriginalPath(string relativePath)
    {
        var slash = relativePath.IndexOf('/');
        return slash < 0 ? relativePath + ":" : relativePath[..slash] + ":" + relativePath[slash..];
    }
}
