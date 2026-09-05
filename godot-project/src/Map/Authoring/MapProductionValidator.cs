using System;
using System.Collections.Generic;
using System.Linq;
using FrontsOfWar.Art;
using Godot;

namespace FrontsOfWar.Map.Authoring;

public enum MapDiagnosticSeverity { Error, Warning, Info }

public sealed class MapDiagnostic
{
    public MapDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
    public string ObjectId { get; init; } = "";
}

public sealed class MapProductionValidationResult
{
    public List<MapDiagnostic> Diagnostics { get; } = new();
    public IEnumerable<MapDiagnostic> Errors => Diagnostics.Where(item => item.Severity == MapDiagnosticSeverity.Error);
    public IEnumerable<MapDiagnostic> Warnings => Diagnostics.Where(item => item.Severity == MapDiagnosticSeverity.Warning);
    public bool CanPublish => !Errors.Any();
}

public static class MapProductionValidator
{
    public static MapProductionValidationResult Validate(MapDefinition map, ArtAssetCatalog catalog = null,
        bool requireApprovedArt = false, string registryRoot = "res://assets/data/maps")
    {
        var result = new MapProductionValidationResult();
        if (map == null) { Add(result, MapDiagnosticSeverity.Error, "MAP_NULL", "Map is null."); return result; }
        var structure = MapDefinitionValidator.Validate(map);
        foreach (string error in structure.Errors) Add(result, MapDiagnosticSeverity.Error, "STRUCTURE", error);
        foreach (string warning in structure.Warnings) Add(result, MapDiagnosticSeverity.Warning, "STRUCTURE_WARNING", warning);
        var terrain = TerrainAuthoringRules.Validate(map);
        foreach (string error in terrain.Errors) Add(result, MapDiagnosticSeverity.Error, "TERRAIN", error);
        ValidateRuntimeMarkers(map, result);
        ValidateAssets(map, catalog, requireApprovedArt, result);
        ValidateBounds(map, result);
        if (map.Metadata?.Status == MapAuthoringStatus.Production)
            Add(result, MapDiagnosticSeverity.Info, "PRODUCTION", "Map is already marked for production.");
        return result;
    }

    private static void ValidateRuntimeMarkers(MapDefinition map, MapProductionValidationResult result)
    {
        if ((map.Paths?.Length ?? 0) == 0)
            Add(result, MapDiagnosticSeverity.Error, "MISSING_PATH", "A playable map requires at least one ground path.");
        if (!(map.TowerNodes ?? Array.Empty<TowerPlacementNode>()).Any(node => node?.Enabled == true))
            Add(result, MapDiagnosticSeverity.Error, "MISSING_PAD", "A playable map requires an enabled tower pad.");
        bool entry = (map.Markers ?? Array.Empty<GameplayMarker>()).Any(marker => marker?.Kind == GameplayMarkerKind.GroundEntry);
        bool objective = (map.Markers ?? Array.Empty<GameplayMarker>()).Any(marker => marker?.Kind == GameplayMarkerKind.Objective);
        if ((map.Paths?.Length ?? 0) > 0 && !entry) Add(result, MapDiagnosticSeverity.Error, "MISSING_ENTRY", "At least one ground entry marker is required for authored paths.");
        if ((map.Paths?.Length ?? 0) > 0 && !objective) Add(result, MapDiagnosticSeverity.Error, "MISSING_OBJECTIVE", "An objective marker is required for authored paths.");
        foreach (var path in map.Paths ?? Array.Empty<PathDefinition>())
        {
            if (path?.Points?.Length is not >= 2) continue;
            if (path.Points.All(point => point == path.Points[0]))
                Add(result, MapDiagnosticSeverity.Error, "PATH_ZERO_LENGTH", $"Path '{path.Id}' has no traversal length.", path.Id);
            CheckEndpoint(map, result, path.Id, path.EntryMarkerId, path.Points[0], GameplayMarkerKind.GroundEntry);
            CheckEndpoint(map, result, path.Id, path.ObjectiveMarkerId, path.Points[^1], GameplayMarkerKind.Objective);
            foreach (var point in path.BakedRuntimePoints ?? Array.Empty<Vector2>())
                if (!MapCoordinateSystem.IsFinite(point))
                    Add(result, MapDiagnosticSeverity.Error, "PATH_BAKED_POINT", "Baked path points must be finite.", path.Id);
            if ((path.BakedRuntimePoints?.Length ?? 0) > 0 && (path.BakedRuntimePoints.Length < 2 ||
                !path.BakedRuntimePoints[0].IsEqualApprox(path.Points[0]) || !path.BakedRuntimePoints[^1].IsEqualApprox(path.Points[^1])))
                Add(result, MapDiagnosticSeverity.Error, "PATH_BAKED_ENDPOINT", "Baked path endpoints do not match authored endpoints.", path.Id);
        }
        foreach (var air in map.AirCorridors ?? Array.Empty<MapAirCorridorDefinition>())
        {
            if (air == null) continue;
            CheckEndpoint(map, result, air.Id, air.EntryMarkerId, air.EntryPositionTiles, GameplayMarkerKind.AirEntry);
            CheckEndpoint(map, result, air.Id, air.ObjectiveMarkerId, air.ObjectivePositionTiles, GameplayMarkerKind.AirObjective);
            if (air.EntryPositionTiles.IsEqualApprox(air.ObjectivePositionTiles))
                Add(result, MapDiagnosticSeverity.Error, "AIR_ZERO_LENGTH", "Air corridor endpoints must differ.", air.Id);
        }
    }

    private static void CheckEndpoint(MapDefinition map, MapProductionValidationResult result, string owner,
        string markerId, Vector2 position, GameplayMarkerKind kind)
    {
        var marker = (map.Markers ?? Array.Empty<GameplayMarker>()).FirstOrDefault(item => item?.Id == markerId);
        if (marker == null || marker.Kind != kind || !marker.PositionTiles.IsEqualApprox(position))
            Add(result, MapDiagnosticSeverity.Error, "PATH_ENDPOINT", $"'{owner}' endpoint must match its {kind} marker '{markerId}'.", owner);
    }

    private static void ValidateAssets(MapDefinition map, ArtAssetCatalog catalog, bool requireApprovedArt, MapProductionValidationResult result)
    {
        if (catalog == null) catalog = ArtAssetCatalog.LoadFromProject();
        foreach (var asset in map.Assets ?? Array.Empty<MapAssetInstance>())
        {
            if (asset == null) continue;
            var entry = catalog.Find(asset.AssetId);
            if (entry == null) Add(result, MapDiagnosticSeverity.Error, "MISSING_ASSET", $"Asset '{asset.AssetId}' is not in the catalog.", asset.Id);
            else if (requireApprovedArt && !entry.IsApproved)
                Add(result, MapDiagnosticSeverity.Error, "UNAPPROVED_ASSET", $"Asset '{asset.AssetId}' is not approved for production.", asset.Id);
            else if (!entry.IsApproved) Add(result, MapDiagnosticSeverity.Warning, "REVIEW_ASSET", $"Asset '{asset.AssetId}' is using a review/placeholder entry.", asset.Id);
        }
        foreach (var cluster in map.Clusters ?? Array.Empty<ClusterInstance>())
        {
            if (cluster == null || !cluster.Enabled) continue;
            foreach (var child in cluster?.Children ?? Array.Empty<MapAssetInstance>())
                if (child?.Enabled == true) CheckCatalogAsset(catalog, result, child.AssetId, child.Id, requireApprovedArt);
            if ((cluster.Children?.Length ?? 0) == 0 && !string.IsNullOrWhiteSpace(cluster.CatalogAssetId))
                CheckCatalogAsset(catalog, result, cluster.CatalogAssetId, cluster.Id, requireApprovedArt);
        }
        foreach (var terrain in map.Terrain ?? Array.Empty<TerrainInstance>())
            if (terrain != null) CheckCatalogAsset(catalog, result, terrain.AssetId, terrain.Id, requireApprovedArt);
    }

    private static void CheckCatalogAsset(ArtAssetCatalog catalog, MapProductionValidationResult result,
        string assetId, string objectId, bool requireApproved)
    {
        var entry = catalog.Find(assetId);
        if (entry == null) Add(result, MapDiagnosticSeverity.Error, "MISSING_ASSET", $"Asset '{assetId}' is not in the catalog.", objectId);
        else if (requireApproved && !entry.IsApproved)
            Add(result, MapDiagnosticSeverity.Error, "UNAPPROVED_ASSET", $"Asset '{assetId}' is not approved for production.", objectId);
    }

    private static void ValidateBounds(MapDefinition map, MapProductionValidationResult result)
    {
        if (map.Metadata == null) return;
        foreach (var node in map.TowerNodes ?? Array.Empty<TowerPlacementNode>())
            if (node != null && (node.PositionTiles.X < 0 || node.PositionTiles.Y < 0 || node.PositionTiles.X >= map.Metadata.WidthTiles || node.PositionTiles.Y >= map.Metadata.HeightTiles))
                Add(result, MapDiagnosticSeverity.Error, "PAD_BOUNDS", $"Tower node '{node.Id}' is outside map bounds.", node.Id);
    }

    private static void Add(MapProductionValidationResult result, MapDiagnosticSeverity severity, string code, string message, string objectId = "")
        => result.Diagnostics.Add(new MapDiagnostic { Severity = severity, Code = code, Message = message, ObjectId = objectId });
}
