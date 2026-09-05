using System;
using System.Collections.Generic;
using Godot;

namespace FrontsOfWar.Map.Authoring;

public sealed class MapDefinitionValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public bool IsValid => Errors.Count == 0;
}

// Phase 2 structural validation protects serialization. Deeper strategic,
// adjacency, coverage, and publish checks remain Phase 11 responsibilities.
public static class MapDefinitionValidator
{
    public static MapDefinitionValidationResult Validate(MapDefinition map)
    {
        var result = new MapDefinitionValidationResult();
        if (map == null)
        {
            result.Errors.Add("MapDefinition is null.");
            return result;
        }

        try { MapSchemaMigrator.ValidateAndMigrate(map); }
        catch (MapSchemaException exception) { result.Errors.Add(exception.Message); }

        if (map.Metadata == null)
        {
            result.Errors.Add("Metadata is required.");
            return result;
        }

        if (!MapObjectId.IsValid(map.Metadata.Id)) result.Errors.Add("Metadata.Id is invalid.");
        if (string.IsNullOrWhiteSpace(map.Metadata.DisplayName)) result.Errors.Add("Metadata.DisplayName is required.");
        if (map.Metadata.WidthTiles <= 0 || map.Metadata.HeightTiles <= 0)
            result.Errors.Add("Map dimensions must be positive tile counts.");

        var ids = new HashSet<string>(StringComparer.Ordinal);
        ValidateTerrain(map, ids, result);
        ValidateAssets(map, ids, result);
        ValidateClusters(map, ids, result);
        ValidateMarkers(map, ids, result);
        ValidatePaths(map, ids, result);
        ValidateAirCorridors(map, ids, result);
        ValidateTowerNodes(map, ids, result);
        ValidateZones(map, ids, result);
        ValidateGimmicks(map, ids, result);
        return result;
    }

    private static void ValidateTerrain(MapDefinition map, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        foreach (var item in map.Terrain ?? Array.Empty<TerrainInstance>())
        {
            if (!ValidateItem(item, item?.Id, "terrain", ids, result)) continue;
            if (string.IsNullOrWhiteSpace(item.AssetId)) result.Errors.Add($"Terrain '{item.Id}' has no AssetId.");
            if (item.Cell.X < 0 || item.Cell.Y < 0 || item.Cell.X >= map.Metadata.WidthTiles || item.Cell.Y >= map.Metadata.HeightTiles)
                result.Errors.Add($"Terrain '{item.Id}' is outside map bounds.");
            if (item.RotationQuarterTurns is < 0 or > 3)
                result.Errors.Add($"Terrain '{item.Id}' rotation must be 0–3 quarter turns.");
        }
    }

    private static void ValidateAssets(MapDefinition map, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        foreach (var item in map.Assets ?? Array.Empty<MapAssetInstance>())
            ValidateAsset(item, "asset", ids, result);
    }

    private static void ValidateAsset(MapAssetInstance item, string category, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        if (!ValidateItem(item, item?.Id, category, ids, result)) return;
        if (string.IsNullOrWhiteSpace(item.AssetId)) result.Errors.Add($"Asset '{item.Id}' has no AssetId.");
        if (!MapCoordinateSystem.IsFinite(item.PositionTiles)) result.Errors.Add($"Asset '{item.Id}' position is not finite.");
        if (!float.IsFinite(item.RotationRadians)) result.Errors.Add($"Asset '{item.Id}' rotation is not finite.");
        if (!MapCoordinateSystem.IsUniformScale(item.Scale)) result.Errors.Add($"Asset '{item.Id}' scale must be positive and uniform.");
        if (!MapCoordinateSystem.IsUniformScale(item.DefaultScale)) result.Errors.Add($"Asset '{item.Id}' default scale must be positive and uniform.");
    }

    private static void ValidateClusters(MapDefinition map, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        foreach (var cluster in map.Clusters ?? Array.Empty<ClusterInstance>())
        {
            if (!ValidateItem(cluster, cluster?.Id, "cluster", ids, result)) continue;
            if (!MapCoordinateSystem.IsFinite(cluster.PositionTiles)) result.Errors.Add($"Cluster '{cluster.Id}' position is not finite.");
            if (!float.IsFinite(cluster.RotationRadians)) result.Errors.Add($"Cluster '{cluster.Id}' rotation is not finite.");
            if (!float.IsFinite(cluster.UniformScale) || cluster.UniformScale <= 0f)
                result.Errors.Add($"Cluster '{cluster.Id}' scale must be positive and finite.");
            foreach (var child in cluster.Children ?? Array.Empty<MapAssetInstance>())
                ValidateAsset(child, $"child asset in cluster '{cluster.Id}'", ids, result);
        }
    }

    private static void ValidateMarkers(MapDefinition map, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        foreach (var marker in map.Markers ?? Array.Empty<GameplayMarker>())
        {
            if (!ValidateItem(marker, marker?.Id, "marker", ids, result)) continue;
            if (!MapCoordinateSystem.IsFinite(marker.PositionTiles)) result.Errors.Add($"Marker '{marker.Id}' position is not finite.");
            ValidateProperties(marker.Metadata, $"marker '{marker.Id}'", result);
        }
    }

    private static void ValidatePaths(MapDefinition map, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        var markerIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var marker in map.Markers ?? Array.Empty<GameplayMarker>())
            if (marker != null && !string.IsNullOrEmpty(marker.Id)) markerIds.Add(marker.Id);

        foreach (var path in map.Paths ?? Array.Empty<PathDefinition>())
        {
            if (!ValidateItem(path, path?.Id, "path", ids, result)) continue;
            if ((path.Points?.Length ?? 0) < 2) result.Errors.Add($"Path '{path.Id}' needs at least two points.");
            foreach (var point in path.Points ?? Array.Empty<Vector2>())
                if (!MapCoordinateSystem.IsFinite(point)) result.Errors.Add($"Path '{path.Id}' contains a non-finite point.");
            if (!markerIds.Contains(path.EntryMarkerId)) result.Errors.Add($"Path '{path.Id}' references missing entry marker '{path.EntryMarkerId}'.");
            if (!markerIds.Contains(path.ObjectiveMarkerId)) result.Errors.Add($"Path '{path.Id}' references missing objective marker '{path.ObjectiveMarkerId}'.");
            if (path.ActiveFromWave < 1) result.Errors.Add($"Path '{path.Id}' ActiveFromWave must be at least 1.");
            if (path.ActiveUntilWave != -1 && path.ActiveUntilWave < path.ActiveFromWave)
                result.Errors.Add($"Path '{path.Id}' ActiveUntilWave precedes ActiveFromWave.");
        }
    }

    private static void ValidateAirCorridors(MapDefinition map, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        foreach (var corridor in map.AirCorridors ?? Array.Empty<MapAirCorridorDefinition>())
        {
            if (!ValidateItem(corridor, corridor?.Id, "air corridor", ids, result)) continue;
            if (!MapCoordinateSystem.IsFinite(corridor.EntryPositionTiles) || !MapCoordinateSystem.IsFinite(corridor.ObjectivePositionTiles))
                result.Errors.Add($"Air corridor '{corridor.Id}' has non-finite endpoints.");
            if (!float.IsFinite(corridor.WidthTiles) || corridor.WidthTiles <= 0f)
                result.Errors.Add($"Air corridor '{corridor.Id}' width must be positive and finite.");
        }
    }

    private static void ValidateTowerNodes(MapDefinition map, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        foreach (var node in map.TowerNodes ?? Array.Empty<TowerPlacementNode>())
        {
            if (!ValidateItem(node, node?.Id, "tower node", ids, result)) continue;
            if (!MapCoordinateSystem.IsFinite(node.PositionTiles)) result.Errors.Add($"Tower node '{node.Id}' position is not finite.");
            if (!float.IsFinite(node.RotationRadians)) result.Errors.Add($"Tower node '{node.Id}' rotation is not finite.");
            ValidateProperties(node.RuntimeMetadata, $"tower node '{node.Id}'", result);
        }
    }

    private static void ValidateZones(MapDefinition map, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        foreach (var zone in map.Zones ?? Array.Empty<MapZone>())
        {
            if (!ValidateItem(zone, zone?.Id, "zone", ids, result)) continue;
            if (!MapCoordinateSystem.IsFinite(zone.CenterTiles) || !MapCoordinateSystem.IsFinite(zone.SizeTiles))
                result.Errors.Add($"Zone '{zone.Id}' geometry is not finite.");
            else if (zone.SizeTiles.X <= 0f || zone.SizeTiles.Y <= 0f)
                result.Errors.Add($"Zone '{zone.Id}' size must be positive.");
        }
    }

    private static void ValidateGimmicks(MapDefinition map, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        foreach (var gimmick in map.Gimmicks ?? Array.Empty<MapGimmick>())
        {
            if (!ValidateItem(gimmick, gimmick?.Id, "gimmick", ids, result)) continue;
            if (string.IsNullOrWhiteSpace(gimmick.Type)) result.Errors.Add($"Gimmick '{gimmick.Id}' has no type.");
            ValidateProperties(gimmick.Parameters, $"gimmick '{gimmick.Id}'", result);
        }
    }

    private static bool ValidateItem(object item, string id, string category, HashSet<string> ids, MapDefinitionValidationResult result)
    {
        if (item == null)
        {
            result.Errors.Add($"Map contains a null {category}.");
            return false;
        }
        if (!MapObjectId.IsValid(id))
        {
            result.Errors.Add($"{category} has invalid ID '{id}'.");
            return false;
        }
        if (!ids.Add(id)) result.Errors.Add($"Duplicate map object ID '{id}'.");
        return true;
    }

    private static void ValidateProperties(MapProperty[] properties, string owner, MapDefinitionValidationResult result)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in properties ?? Array.Empty<MapProperty>())
        {
            if (property == null || string.IsNullOrWhiteSpace(property.Key))
                result.Errors.Add($"{owner} contains metadata without a key.");
            else if (!keys.Add(property.Key))
                result.Errors.Add($"{owner} contains duplicate metadata key '{property.Key}'.");
        }
    }
}
