using System.Text.Json;

namespace InDows.Core.Restore;

/// <summary>One de-duplicated file: stored once in the backup, but it belonged at several original locations.</summary>
public sealed record RestoreMapEntry(string StoredAt, IReadOnlyList<string> BelongsAt);

/// <summary>
/// Reads <c>redows-restore-map.json</c> (the de-dup map: one stored copy, all the places its content
/// belonged). A missing or broken map just means no de-dup replication; the stored copies still restore.
/// </summary>
public static class RestoreMap
{
    public static IReadOnlyList<RestoreMapEntry> Read(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            var file = JsonSerializer.Deserialize<MapFile>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return file?.Duplicates?
                .Where(d => !string.IsNullOrEmpty(d.StoredAt))
                .Select(d => new RestoreMapEntry(d.StoredAt!, d.BelongsAt ?? []))
                .ToList() ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private sealed record MapFile(int Version, IReadOnlyList<MapRow>? Duplicates);

    private sealed record MapRow(string? StoredAt, IReadOnlyList<string>? BelongsAt);
}
