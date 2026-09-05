#if DEBUG
using System;
using System.Collections.Generic;
using Godot;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Rendering;

// Phase 3's scene factory produces a deterministic, asset-independent render
// snapshot. Catalog sprites can replace these authored placeholders later.
public static class MapSceneFactory
{
    public static MapRenderSnapshot Build(MapDefinition map)
    {
        if (map?.Metadata == null) throw new ArgumentException("A map with metadata is required.", nameof(map));
        var items = new List<MapRenderItem>();
        var paths = new List<MapRenderPath>();

        foreach (var terrain in map.Terrain ?? Array.Empty<TerrainInstance>())
            if (terrain != null) items.Add(Item(terrain.Id, MapRenderKind.Terrain, terrain.Cell + new Vector2(0.5f, 0.5f), Vector2.One, terrain.RotationQuarterTurns * Mathf.Pi / 2f, ColorFor(terrain.AssetId, new Color("#65766b"))));
        foreach (var asset in map.Assets ?? Array.Empty<MapAssetInstance>())
            if (asset != null && asset.Enabled) items.Add(Item(asset.Id, MapRenderKind.Asset, asset.PositionTiles, asset.Scale * 0.8f, asset.RotationRadians, LayerColor(asset.Layer, asset.AssetId)));
        foreach (var cluster in map.Clusters ?? Array.Empty<ClusterInstance>())
        {
            if (cluster == null || !cluster.Enabled) continue;
            items.Add(Item(cluster.Id, MapRenderKind.Cluster, cluster.PositionTiles, Vector2.One * (2f * cluster.UniformScale), cluster.RotationRadians, new Color("#b58a4b")));
            foreach (var child in cluster.Children ?? Array.Empty<MapAssetInstance>())
                if (child != null && child.Enabled) items.Add(Item(child.Id, MapRenderKind.Asset, cluster.PositionTiles + child.PositionTiles * cluster.UniformScale, child.Scale * 0.65f, cluster.RotationRadians + child.RotationRadians, LayerColor(child.Layer, child.AssetId)));
        }
        foreach (var path in map.Paths ?? Array.Empty<PathDefinition>())
            if (path != null) paths.Add(new MapRenderPath { Id = path.Id, PointsTiles = path.Points ?? Array.Empty<Vector2>(), Color = new Color("#c5a46d"), WidthTiles = 0.22f });
        foreach (var corridor in map.AirCorridors ?? Array.Empty<MapAirCorridorDefinition>())
            if (corridor != null) paths.Add(new MapRenderPath { Id = corridor.Id, PointsTiles = new[] { corridor.EntryPositionTiles, corridor.ObjectivePositionTiles }, Color = new Color("#9ab8cf"), WidthTiles = corridor.WidthTiles });
        foreach (var node in map.TowerNodes ?? Array.Empty<TowerPlacementNode>())
            if (node != null && node.Enabled) items.Add(Item(node.Id, MapRenderKind.TowerNode, node.PositionTiles, Vector2.One * 0.7f, node.RotationRadians, new Color("#d8b86d")));
        foreach (var marker in map.Markers ?? Array.Empty<GameplayMarker>())
            if (marker != null) items.Add(Item(marker.Id, MapRenderKind.Marker, marker.PositionTiles, marker.ShapeSizeTiles == Vector2.Zero ? Vector2.One * 0.7f : marker.ShapeSizeTiles, 0f, MarkerColor(marker.Kind)));
        foreach (var zone in map.Zones ?? Array.Empty<MapZone>())
            if (zone != null && zone.Enabled) items.Add(Item(zone.Id, MapRenderKind.Zone, zone.CenterTiles, zone.SizeTiles, 0f, ZoneColor(zone.Kind)));

        return new MapRenderSnapshot { WidthTiles = map.Metadata.WidthTiles, HeightTiles = map.Metadata.HeightTiles, Items = items.ToArray(), Paths = paths.ToArray() };
    }

    private static MapRenderItem Item(string id, MapRenderKind kind, Vector2 position, Vector2 size, float rotation, Color color)
        => new() { Id = id, Kind = kind, PositionTiles = position, SizeTiles = size, RotationRadians = rotation, Color = color };

    private static Color LayerColor(MapAssetLayer layer, string assetId) => layer switch
    {
        MapAssetLayer.Architecture => ColorFor(assetId, new Color("#a69a87")),
        MapAssetLayer.Vegetation => ColorFor(assetId, new Color("#618465")),
        MapAssetLayer.Flavor => ColorFor(assetId, new Color("#aa886a")),
        MapAssetLayer.Route => new Color("#c5a46d"),
        _ => ColorFor(assetId, new Color("#788797")),
    };

    private static Color MarkerColor(GameplayMarkerKind kind) => kind switch
    {
        GameplayMarkerKind.Objective or GameplayMarkerKind.AirObjective => new Color("#e4b85d"),
        GameplayMarkerKind.GroundEntry or GameplayMarkerKind.AirEntry => new Color("#82b39a"),
        _ => new Color("#bd8d85"),
    };

    private static Color ZoneColor(MapZoneKind kind) => kind switch
    {
        MapZoneKind.NoPlacement => new Color("#b56f6a", 0.28f),
        MapZoneKind.RestrictedTerrain => new Color("#d1a85c", 0.25f),
        _ => new Color("#6c8fa5", 0.22f),
    };

    private static Color ColorFor(string value, Color baseColor)
    {
        int hash = 17;
        foreach (char character in value ?? "") hash = unchecked(hash * 31 + character);
        float shift = (hash & 31) / 31f * 0.16f - 0.08f;
        return new Color(Mathf.Clamp(baseColor.R + shift, 0f, 1f), Mathf.Clamp(baseColor.G + shift, 0f, 1f), Mathf.Clamp(baseColor.B + shift, 0f, 1f), baseColor.A);
    }
}
#endif
