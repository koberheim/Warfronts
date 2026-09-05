#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using FrontsOfWar.Art;

namespace FrontsOfWar.Editor.Palette;

public sealed class ArtPaletteQuery
{
    public string Search { get; init; } = "";
    public string Category { get; init; } = "";
    public string Theater { get; init; } = "";
    public string Biome { get; init; } = "";
    public string Status { get; init; } = "";
    public string Layer { get; init; } = "";
    public bool ApprovedOnly { get; init; }
    public bool CompatibleOnly { get; init; }
}

public static class ArtPaletteService
{
    public static IReadOnlyList<ArtAssetEntry> Query(ArtAssetCatalog catalog, ArtPaletteQuery query)
    {
        query ??= new ArtPaletteQuery();
        IEnumerable<ArtAssetEntry> entries = catalog?.Entries ?? new List<ArtAssetEntry>();
        if (!string.IsNullOrWhiteSpace(query.Search))
            entries = entries.Where(entry => Contains(entry.Id, query.Search) || Contains(entry.Name, query.Search) ||
            (entry.Items ?? new List<string>()).Any(item => Contains(item, query.Search)) || (entry.Tags ?? new List<string>()).Any(tag => Contains(tag, query.Search)));
        if (!string.IsNullOrWhiteSpace(query.Category)) entries = entries.Where(entry => Equal(entry.Category, query.Category));
        if (!string.IsNullOrWhiteSpace(query.Theater)) entries = entries.Where(entry => Equal(entry.Theater, query.Theater));
        if (!string.IsNullOrWhiteSpace(query.Biome)) entries = entries.Where(entry => Equal(entry.Biome, query.Biome));
        if (!string.IsNullOrWhiteSpace(query.Status)) entries = entries.Where(entry => Equal(entry.Status, query.Status));
        if (query.ApprovedOnly) entries = entries.Where(entry => entry.IsApproved);
        if (query.CompatibleOnly) entries = entries.Where(entry => entry.SupportsLayer(query.Layer));
        return entries.OrderBy(entry => entry.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ThenBy(entry => entry.Id, StringComparer.Ordinal).ToArray();
    }

    public static string PreviewPath(ArtAssetEntry entry)
    {
        if (entry == null) return "";
        if (!string.IsNullOrWhiteSpace(entry.ThumbnailPath) && Godot.ResourceLoader.Exists(entry.ThumbnailPath)) return entry.ThumbnailPath;
        return entry.PlaceholderPath;
    }

    public static bool CanReplace(ArtAssetEntry current, ArtAssetEntry replacement)
        => current != null && replacement != null && (string.IsNullOrWhiteSpace(current.ReplacementGroup) ||
            string.Equals(current.ReplacementGroup, replacement.ReplacementGroup, StringComparison.OrdinalIgnoreCase));

    private static bool Contains(string value, string search) => value?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
    private static bool Equal(string left, string right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
#endif
