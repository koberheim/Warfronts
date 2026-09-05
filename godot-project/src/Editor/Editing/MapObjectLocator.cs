#if DEBUG
using System;
using System.Collections.Generic;
using Godot;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Editing;

public enum MapObjectKind { Terrain, Asset, Cluster, ClusterChild, Path, AirCorridor, TowerNode, Marker, Zone, Gimmick }

public readonly struct MapObjectTransform
{
    public MapObjectTransform(Vector2 positionTiles, float rotationRadians, Vector2 scale, bool canRotate, bool canScale)
    {
        PositionTiles = positionTiles; RotationRadians = rotationRadians; Scale = scale;
        CanRotate = canRotate; CanScale = canScale;
    }
    public Vector2 PositionTiles { get; }
    public float RotationRadians { get; }
    public Vector2 Scale { get; }
    public bool CanRotate { get; }
    public bool CanScale { get; }
}

public sealed class MapObjectHandle
{
    public string Id { get; init; } = "";
    public MapObjectKind Kind { get; init; }
    public Resource Resource { get; init; }
    public ClusterInstance ParentCluster { get; init; }
}

public static class MapObjectLocator
{
    public static IEnumerable<MapObjectHandle> Enumerate(MapDefinition map)
    {
        foreach (var item in map?.Terrain ?? Array.Empty<TerrainInstance>()) if (item != null) yield return Handle(item.Id, MapObjectKind.Terrain, item);
        foreach (var item in map?.Assets ?? Array.Empty<MapAssetInstance>()) if (item != null) yield return Handle(item.Id, MapObjectKind.Asset, item);
        foreach (var item in map?.Clusters ?? Array.Empty<ClusterInstance>())
        {
            if (item == null) continue;
            yield return Handle(item.Id, MapObjectKind.Cluster, item);
            foreach (var child in item.Children ?? Array.Empty<MapAssetInstance>())
                if (child != null) yield return new MapObjectHandle { Id = child.Id, Kind = MapObjectKind.ClusterChild, Resource = child, ParentCluster = item };
        }
        foreach (var item in map?.Paths ?? Array.Empty<PathDefinition>()) if (item != null) yield return Handle(item.Id, MapObjectKind.Path, item);
        foreach (var item in map?.AirCorridors ?? Array.Empty<MapAirCorridorDefinition>()) if (item != null) yield return Handle(item.Id, MapObjectKind.AirCorridor, item);
        foreach (var item in map?.TowerNodes ?? Array.Empty<TowerPlacementNode>()) if (item != null) yield return Handle(item.Id, MapObjectKind.TowerNode, item);
        foreach (var item in map?.Markers ?? Array.Empty<GameplayMarker>()) if (item != null) yield return Handle(item.Id, MapObjectKind.Marker, item);
        foreach (var item in map?.Zones ?? Array.Empty<MapZone>()) if (item != null) yield return Handle(item.Id, MapObjectKind.Zone, item);
        foreach (var item in map?.Gimmicks ?? Array.Empty<MapGimmick>()) if (item != null) yield return Handle(item.Id, MapObjectKind.Gimmick, item);
    }

    public static MapObjectHandle Find(MapDefinition map, string id)
    {
        foreach (var handle in Enumerate(map)) if (handle.Id == id) return handle;
        return null;
    }

    public static bool TryGetTransform(MapObjectHandle handle, out MapObjectTransform transform)
    {
        switch (handle?.Resource)
        {
            case TerrainInstance terrain:
                transform = new MapObjectTransform(new Vector2(terrain.Cell.X, terrain.Cell.Y), terrain.RotationQuarterTurns * Mathf.Pi / 2f, Vector2.One, true, false); return true;
            case MapAssetInstance asset:
                transform = new MapObjectTransform(asset.PositionTiles, asset.RotationRadians, asset.Scale, true, true); return true;
            case ClusterInstance cluster:
                transform = new MapObjectTransform(cluster.PositionTiles, cluster.RotationRadians, Vector2.One * cluster.UniformScale, true, true); return true;
            case PathDefinition path when path.Points?.Length > 0:
                transform = new MapObjectTransform(path.Points[0], 0f, Vector2.One, false, false); return true;
            case MapAirCorridorDefinition corridor:
                transform = new MapObjectTransform(corridor.EntryPositionTiles, corridor.EntryPositionTiles.AngleToPoint(corridor.ObjectivePositionTiles), Vector2.One * corridor.WidthTiles, false, false); return true;
            case TowerPlacementNode node:
                transform = new MapObjectTransform(node.PositionTiles, node.RotationRadians, Vector2.One, true, false); return true;
            case GameplayMarker marker:
                transform = new MapObjectTransform(marker.PositionTiles, 0f, marker.ShapeSizeTiles, false, true); return true;
            case MapZone zone:
                transform = new MapObjectTransform(zone.CenterTiles, 0f, zone.SizeTiles, false, true); return true;
            default:
                transform = default; return false;
        }
    }

    public static void ApplyTransform(MapObjectHandle handle, MapObjectTransform transform)
    {
        switch (handle?.Resource)
        {
            case TerrainInstance terrain:
                terrain.Cell = new Vector2I(Mathf.RoundToInt(transform.PositionTiles.X), Mathf.RoundToInt(transform.PositionTiles.Y));
                terrain.RotationQuarterTurns = MapCoordinateSystem.NormalizeQuarterTurns(Mathf.RoundToInt(transform.RotationRadians / (Mathf.Pi / 2f))); break;
            case MapAssetInstance asset:
                asset.PositionTiles = transform.PositionTiles; asset.RotationRadians = MapCoordinateSystem.NormalizeRotation(transform.RotationRadians); asset.Scale = transform.Scale; break;
            case ClusterInstance cluster:
                cluster.PositionTiles = transform.PositionTiles; cluster.RotationRadians = MapCoordinateSystem.NormalizeRotation(transform.RotationRadians); cluster.UniformScale = transform.Scale.X; break;
            case TowerPlacementNode node:
                node.PositionTiles = transform.PositionTiles; node.RotationRadians = MapCoordinateSystem.NormalizeRotation(transform.RotationRadians); break;
            case GameplayMarker marker:
                marker.PositionTiles = transform.PositionTiles; marker.ShapeSizeTiles = transform.Scale; break;
            case MapZone zone:
                zone.CenterTiles = transform.PositionTiles; zone.SizeTiles = transform.Scale; break;
            case PathDefinition path when path.Points?.Length > 0:
                Vector2 delta = transform.PositionTiles - path.Points[0]; for (int i = 0; i < path.Points.Length; i++) path.Points[i] += delta; break;
            case MapAirCorridorDefinition corridor:
                Vector2 corridorDelta = transform.PositionTiles - corridor.EntryPositionTiles; corridor.EntryPositionTiles += corridorDelta; corridor.ObjectivePositionTiles += corridorDelta; break;
            default: throw new InvalidOperationException($"Object '{handle?.Id}' does not support transforms.");
        }
    }

    private static MapObjectHandle Handle(string id, MapObjectKind kind, Resource resource)
        => new() { Id = id, Kind = kind, Resource = resource };
}
#endif
