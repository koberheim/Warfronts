using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using GFileAccess = Godot.FileAccess;

namespace FrontsOfWar.Map.Authoring;

public sealed class MapSerializationException : Exception
{
    public MapSerializationException(string message) : base(message) { }
    public MapSerializationException(string message, Exception innerException) : base(message, innerException) { }
}

// Canonical map persistence. Arrays are normalized before ResourceSaver sees
// them, and writes go through a sibling temp resource before replacement.
public static class MapSerializer
{
    private static readonly Regex ExternalResourcePattern = new(
        "^\\[ext_resource[^\\r\\n]*\\spath=\"(?<path>[^\"]+)\"[^\\r\\n]*\\sid=\"(?<id>[^\"]+)\"\\]$",
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);

    public static MapDefinition Load(string path)
    {
        ValidatePath(path);
        if (!GFileAccess.FileExists(path) && !ResourceLoader.Exists(path))
            throw new FileNotFoundException($"Map resource was not found at '{path}'.", path);

        MapDefinition map;
        try
        {
            map = ResourceLoader.Load<MapDefinition>(path, "", ResourceLoader.CacheMode.Ignore);
        }
        catch (Exception exception)
        {
            throw new MapSerializationException($"Map resource '{path}' is corrupt or unreadable.", exception);
        }

        if (map == null)
            throw new MapSerializationException($"Map resource '{path}' is corrupt, unreadable, or not a MapDefinition.");

        MapSchemaMigrator.ValidateAndMigrate(map);
        EnsureArrays(map);
        return map;
    }

    public static void SaveAs(MapDefinition map, string path)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        ValidatePath(path);
        MapSchemaMigrator.ValidateAndMigrate(map);
        EnsureArrays(map);

        var validation = MapDefinitionValidator.Validate(map);
        if (!validation.IsValid)
            throw new MapSerializationException($"Map cannot be saved: {string.Join("; ", validation.Errors)}");

        NormalizeForSave(map);
        string directory = path.GetBaseDir();
        string absoluteDirectory = ProjectSettings.GlobalizePath(directory);
        if (!DirAccess.DirExistsAbsolute(absoluteDirectory))
        {
            Error directoryError = DirAccess.MakeDirRecursiveAbsolute(absoluteDirectory);
            if (directoryError != Error.Ok)
                throw new MapSerializationException($"Could not create map directory '{directory}' ({directoryError}).");
        }

        string stem = path.TrimSuffix(".tres");
        string tempPath = $"{stem}.tmp.tres";
        string backupPath = $"{stem}.backup.tres";
        RemoveIfPresent(tempPath);
        RemoveIfPresent(backupPath);

        Error saveError = ResourceSaver.Save(map, tempPath);
        if (saveError != Error.Ok)
        {
            RemoveIfPresent(tempPath);
            throw new MapSerializationException($"Could not serialize map to '{tempPath}' ({saveError}).");
        }

        bool movedExisting = false;
        try
        {
            CanonicalizeExternalResourceIds(tempPath);
            if (GFileAccess.FileExists(path))
            {
                Error backupError = DirAccess.RenameAbsolute(ToAbsolute(path), ToAbsolute(backupPath));
                if (backupError != Error.Ok)
                    throw new MapSerializationException($"Could not stage existing map '{path}' for replacement ({backupError}).");
                movedExisting = true;
            }

            Error replaceError = DirAccess.RenameAbsolute(ToAbsolute(tempPath), ToAbsolute(path));
            if (replaceError != Error.Ok)
                throw new MapSerializationException($"Could not move completed map into '{path}' ({replaceError}).");

            if (movedExisting) RemoveIfPresent(backupPath);
        }
        catch
        {
            RemoveIfPresent(tempPath);
            if (movedExisting && !GFileAccess.FileExists(path) && GFileAccess.FileExists(backupPath))
                DirAccess.RenameAbsolute(ToAbsolute(backupPath), ToAbsolute(path));
            throw;
        }
    }

    public static void NormalizeForSave(MapDefinition map)
    {
        EnsureArrays(map);
        map.Metadata.CampaignUsageIds = SortStrings(map.Metadata.CampaignUsageIds);
        map.Terrain = SortResources(map.Terrain, item => item.Id);
        map.Assets = SortResources(map.Assets, item => item.Id);
        map.Clusters = SortResources(map.Clusters, item => item.Id);
        map.Paths = SortResources(map.Paths, item => item.Id);
        map.AirCorridors = SortResources(map.AirCorridors, item => item.Id);
        map.TowerNodes = SortResources(map.TowerNodes, item => item.Id);
        map.Markers = SortResources(map.Markers, item => item.Id);
        map.Zones = SortResources(map.Zones, item => item.Id);
        map.Gimmicks = SortResources(map.Gimmicks, item => item.Id);

        foreach (var terrain in map.Terrain) terrain.EditorTags = SortStrings(terrain.EditorTags);
        foreach (var asset in map.Assets) NormalizeAsset(asset);
        foreach (var cluster in map.Clusters)
        {
            cluster.Tags = SortStrings(cluster.Tags);
            cluster.Children = SortResources(cluster.Children, item => item.Id);
            foreach (var child in cluster.Children) NormalizeAsset(child);
        }
        foreach (var path in map.Paths) path.Tags = SortStrings(path.Tags);
        foreach (var corridor in map.AirCorridors) corridor.Tags = SortStrings(corridor.Tags);
        foreach (var node in map.TowerNodes)
        {
            node.AllowedArchetypeIds = SortStrings(node.AllowedArchetypeIds);
            node.RuntimeMetadata = SortProperties(node.RuntimeMetadata);
        }
        foreach (var marker in map.Markers) marker.Metadata = SortProperties(marker.Metadata);
        foreach (var zone in map.Zones) zone.Tags = SortStrings(zone.Tags);
        foreach (var gimmick in map.Gimmicks)
        {
            gimmick.PathIds = SortStrings(gimmick.PathIds);
            gimmick.Parameters = SortProperties(gimmick.Parameters);
        }
        if (map.Provenance != null) map.Provenance.InitialMetrics = SortProperties(map.Provenance.InitialMetrics);
        AssignDeterministicSubresourceIds(map);
    }

    private static void NormalizeAsset(MapAssetInstance asset)
    {
        asset.EditorTags = SortStrings(asset.EditorTags);
        asset.RuntimeTags = SortStrings(asset.RuntimeTags);
    }

    private static void AssignDeterministicSubresourceIds(MapDefinition map)
    {
        map.Metadata.ResourceSceneUniqueId = "metadata";
        foreach (var terrain in map.Terrain) terrain.ResourceSceneUniqueId = terrain.Id;
        foreach (var asset in map.Assets) asset.ResourceSceneUniqueId = asset.Id;
        foreach (var cluster in map.Clusters)
        {
            cluster.ResourceSceneUniqueId = cluster.Id;
            foreach (var child in cluster.Children) child.ResourceSceneUniqueId = child.Id;
        }
        foreach (var path in map.Paths) path.ResourceSceneUniqueId = path.Id;
        foreach (var corridor in map.AirCorridors) corridor.ResourceSceneUniqueId = corridor.Id;
        foreach (var node in map.TowerNodes)
        {
            node.ResourceSceneUniqueId = node.Id;
            AssignPropertyIds(node.RuntimeMetadata, $"{node.Id}_runtime");
        }
        foreach (var marker in map.Markers)
        {
            marker.ResourceSceneUniqueId = marker.Id;
            AssignPropertyIds(marker.Metadata, $"{marker.Id}_metadata");
        }
        foreach (var zone in map.Zones) zone.ResourceSceneUniqueId = zone.Id;
        foreach (var gimmick in map.Gimmicks)
        {
            gimmick.ResourceSceneUniqueId = gimmick.Id;
            AssignPropertyIds(gimmick.Parameters, $"{gimmick.Id}_parameter");
        }
        if (map.Provenance != null)
        {
            map.Provenance.ResourceSceneUniqueId = "provenance";
            AssignPropertyIds(map.Provenance.InitialMetrics, "provenance_metric");
        }
    }

    private static void AssignPropertyIds(MapProperty[] properties, string prefix)
    {
        for (int i = 0; i < properties.Length; i++)
            properties[i].ResourceSceneUniqueId = $"{prefix}_{i}";
    }

    private static void EnsureArrays(MapDefinition map)
    {
        map.Terrain ??= Array.Empty<TerrainInstance>();
        map.Assets ??= Array.Empty<MapAssetInstance>();
        map.Clusters ??= Array.Empty<ClusterInstance>();
        map.Paths ??= Array.Empty<PathDefinition>();
        map.AirCorridors ??= Array.Empty<MapAirCorridorDefinition>();
        map.TowerNodes ??= Array.Empty<TowerPlacementNode>();
        map.Markers ??= Array.Empty<GameplayMarker>();
        map.Zones ??= Array.Empty<MapZone>();
        map.Gimmicks ??= Array.Empty<MapGimmick>();
        if (map.Metadata != null) map.Metadata.CampaignUsageIds ??= Array.Empty<string>();
    }

    private static T[] SortResources<T>(T[] items, Func<T, string> id)
        => (items ?? Array.Empty<T>()).OrderBy(id, StringComparer.Ordinal).ToArray();

    private static string[] SortStrings(string[] items)
        => (items ?? Array.Empty<string>()).OrderBy(item => item, StringComparer.Ordinal).ToArray();

    private static MapProperty[] SortProperties(MapProperty[] items)
        => SortResources(items, item => item.Key);

    private static void ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Map path is required.", nameof(path));
        if (!path.StartsWith("res://", StringComparison.Ordinal) && !path.StartsWith("user://", StringComparison.Ordinal))
            throw new ArgumentException("Map paths must be project-relative res:// or user:// paths.", nameof(path));
        if (!path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Canonical maps must use the .tres extension.", nameof(path));
    }

    private static void RemoveIfPresent(string path)
    {
        if (!GFileAccess.FileExists(path)) return;
        Error error = DirAccess.RemoveAbsolute(ToAbsolute(path));
        if (error != Error.Ok) throw new MapSerializationException($"Could not remove temporary map file '{path}' ({error}).");
    }

    private static string ToAbsolute(string path) => ProjectSettings.GlobalizePath(path);

    // Godot appends a random suffix to external-resource IDs. Replacing only
    // those generated identifiers makes equivalent resource graphs diff cleanly.
    private static void CanonicalizeExternalResourceIds(string path)
    {
        string contents = GFileAccess.GetFileAsString(path);
        Match[] matches = ExternalResourcePattern.Matches(contents).Cast<Match>().ToArray();
        var canonicalByGenerated = new Dictionary<string, string>(StringComparer.Ordinal);
        int index = 1;
        foreach (Match match in matches.OrderBy(item => item.Groups["path"].Value, StringComparer.Ordinal))
        {
            string generated = match.Groups["id"].Value;
            if (!canonicalByGenerated.ContainsKey(generated))
                canonicalByGenerated.Add(generated, $"{index++}_canonical");
        }

        contents = ExternalResourcePattern.Replace(contents, match =>
        {
            Group id = match.Groups["id"];
            int relativeIndex = id.Index - match.Index;
            return match.Value.Remove(relativeIndex, id.Length)
                .Insert(relativeIndex, canonicalByGenerated[id.Value]);
        });
        foreach ((string generated, string canonical) in canonicalByGenerated)
            contents = contents.Replace($"ExtResource(\"{generated}\")", $"ExtResource(\"{canonical}\")", StringComparison.Ordinal);

        using GFileAccess file = GFileAccess.Open(path, GFileAccess.ModeFlags.Write);
        if (file == null)
            throw new MapSerializationException($"Could not canonicalize temporary map resource '{path}' ({GFileAccess.GetOpenError()}).");
        file.StoreString(contents);
        file.Flush();
    }
}
