using System.Text.Json;
using System.Text.Json.Serialization;

namespace InDows.Core.Build;

/// <summary>
/// Reads the bundled catalog (<c>modules.catalog.json</c>) that ships next to the app. Each entry is a
/// curated module the Build checklist offers. A missing or malformed file is reported as an exception the
/// screen can show. Risk and kind are the lowercase strings the generator writes (<c>safe</c>, <c>snippet</c>…).
/// </summary>
public sealed class JsonModuleCatalog(string catalogPath) : IModuleCatalog
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public IReadOnlyList<ModuleCatalogEntry> Load()
    {
        if (!File.Exists(catalogPath))
        {
            throw new InvalidOperationException($"The module catalog is missing: '{catalogPath}'.");
        }

        CatalogFile? file;
        try
        {
            file = JsonSerializer.Deserialize<CatalogFile>(File.ReadAllText(catalogPath), Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"The module catalog is malformed: {ex.Message}", ex);
        }

        // Normalise missing "tweaks"/"params"/"options" arrays to empty, so callers never see a null list.
        return (file?.Modules ?? [])
            .Select(m => m with
            {
                Tweaks = m.Tweaks ?? [],
                Params = (m.Params ?? []).Select(p => p with { Options = p.Options ?? [] }).ToList(),
            })
            .ToList();
    }

    private sealed record CatalogFile(IReadOnlyList<ModuleCatalogEntry>? Modules);
}
