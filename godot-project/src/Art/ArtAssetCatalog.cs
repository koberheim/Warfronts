using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace FrontsOfWar.Art;

public sealed class ArtAssetCatalog
{
    public const string ProjectPath = "res://assets/data/art/art_asset_catalog.json";

    [JsonPropertyName("schema_version")] public string SchemaVersion { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("intent")] public string Intent { get; set; } = "";
    [JsonPropertyName("placeholder_root")] public string PlaceholderRoot { get; set; } = "";
    [JsonPropertyName("entries")] public List<ArtAssetEntry> Entries { get; set; } = new();

    private static ArtAssetCatalog _cached;

    public static ArtAssetCatalog LoadFromProject()
    {
        if (_cached != null) return _cached;
        if (!FileAccess.FileExists(ProjectPath))
        {
            GD.PushError($"Art asset catalog is missing: {ProjectPath}");
            return _cached = new ArtAssetCatalog();
        }

        var json = FileAccess.GetFileAsString(ProjectPath);
        _cached = JsonSerializer.Deserialize<ArtAssetCatalog>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ArtAssetCatalog();
        return _cached;
    }

    public static void ClearCache() => _cached = null;

    public ArtAssetEntry Find(string id)
        => (Entries ?? new List<ArtAssetEntry>()).FirstOrDefault(entry => entry?.Id == id);
}

public sealed class ArtAssetEntry
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("category")] public string Category { get; set; } = "";
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("placeholder")] public string PlaceholderPath { get; set; } = "";
    [JsonPropertyName("production_directory")] public string ProductionDirectory { get; set; } = "";
    [JsonPropertyName("production_path")] public string ProductionPath { get; set; } = "";
    [JsonPropertyName("filename_pattern")] public string FilenamePattern { get; set; } = "";
    [JsonPropertyName("notes")] public string Notes { get; set; } = "";
    [JsonPropertyName("items")] public List<string> Items { get; set; } = new();
    [JsonPropertyName("states")] public List<string> States { get; set; } = new();
    [JsonPropertyName("target_count")] public string TargetCount { get; set; } = "";
    // Optional discovery metadata. The original catalog predates these fields;
    // nullable-friendly defaults keep all legacy entries loadable.
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = new();
    [JsonPropertyName("theater")] public string Theater { get; set; } = "";
    [JsonPropertyName("biome")] public string Biome { get; set; } = "";
    [JsonPropertyName("thumbnail")] public string ThumbnailPath { get; set; } = "";
    [JsonPropertyName("scalability")] public string Scalability { get; set; } = "fixed";
    [JsonPropertyName("compatible_layers")] public List<string> CompatibleLayers { get; set; } = new();
    [JsonPropertyName("replacement_group")] public string ReplacementGroup { get; set; } = "";

    public bool IsApproved => string.Equals(Status, "APPROVED", System.StringComparison.OrdinalIgnoreCase);
    public bool IsReview => string.Equals(Status, "REVIEW", System.StringComparison.OrdinalIgnoreCase);
    public bool SupportsLayer(string layer)
        => CompatibleLayers == null || CompatibleLayers.Count == 0 || CompatibleLayers.Exists(value =>
            string.Equals(value, layer, System.StringComparison.OrdinalIgnoreCase));

    public string ResolvePath(bool useApprovedAsset, bool allowReviewAsset = false)
    {
        var canUseProduction = Status == "APPROVED" || (allowReviewAsset && Status == "REVIEW");
        if (useApprovedAsset && canUseProduction && !string.IsNullOrWhiteSpace(ProductionPath) &&
            ResourceLoader.Exists(ProductionPath))
            return ProductionPath;
        return PlaceholderPath;
    }
}
