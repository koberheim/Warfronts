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
        => Entries.FirstOrDefault(entry => entry.Id == id);
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

    public string ResolvePath(bool useApprovedAsset)
    {
        if (useApprovedAsset && Status == "APPROVED" && !string.IsNullOrWhiteSpace(ProductionPath) &&
            ResourceLoader.Exists(ProductionPath))
            return ProductionPath;
        return PlaceholderPath;
    }
}
