using System.Text.Json;

namespace InDows.Core.Restore;

/// <summary>
/// Reads <c>redows-hashes.json</c> into a backup-relative-path to SHA-256 map (forward slashes), so a
/// restore can prove each restored file is byte-identical to its original. A missing or broken manifest
/// just means "no verification", never a crash.
/// </summary>
public static class BackupHashes
{
    public static IReadOnlyDictionary<string, string> Read(string path)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            var file = JsonSerializer.Deserialize<HashesFile>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in file?.Files ?? [])
            {
                if (!string.IsNullOrEmpty(entry.Path) && !string.IsNullOrEmpty(entry.Sha256))
                {
                    map[entry.Path.Replace('\\', '/')] = entry.Sha256;
                }
            }

            return map;
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>();
        }
    }

    private sealed record HashesFile(int Version, string? Algorithm, IReadOnlyList<HashRow>? Files);

    private sealed record HashRow(string? Path, string? Sha256);
}
