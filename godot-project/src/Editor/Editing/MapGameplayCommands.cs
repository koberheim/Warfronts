#if DEBUG
using System;
using System.Linq;
using Godot;
using FrontsOfWar.Map;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Editing;

public static class MapGameplayCommands
{
    public static MapSnapshotCommand AddTerrain(MapDefinition map, string assetId, Vector2 positionTiles, string terrainSetId = "default")
        => new("Place terrain tile", target =>
        {
            var cell = new Vector2I(Mathf.RoundToInt(positionTiles.X), Mathf.RoundToInt(positionTiles.Y));
            if ((target.Terrain ?? Array.Empty<TerrainInstance>()).Any(item => item?.Cell == cell))
                throw new InvalidOperationException($"Terrain cell {cell} is already occupied.");
            target.Terrain = (target.Terrain ?? Array.Empty<TerrainInstance>()).Append(new TerrainInstance
            {
                Id = MapObjectId.New("terrain"), AssetId = assetId ?? "", Cell = cell, TerrainSetId = terrainSetId ?? "default",
                CollisionClass = TerrainCollisionClass.Decorative,
            }).ToArray();
        });

    public static MapSnapshotCommand RotateTerrain(MapDefinition map, string[] ids, int quarterTurns)
        => new("Rotate terrain", target =>
        {
            foreach (string id in ids ?? Array.Empty<string>())
            {
                var terrain = (target.Terrain ?? Array.Empty<TerrainInstance>()).FirstOrDefault(item => item?.Id == id);
                if (terrain == null) continue;
                terrain.RotationQuarterTurns = MapCoordinateSystem.NormalizeQuarterTurns(terrain.RotationQuarterTurns + quarterTurns);
            }
        });

    public static MapSnapshotCommand AddTowerNode(MapDefinition map, Vector2 positionTiles, PadTag tag = PadTag.Standard)
        => new("Place tower node", target =>
        {
            target.TowerNodes = (target.TowerNodes ?? Array.Empty<TowerPlacementNode>()).Append(new TowerPlacementNode
            {
                Id = MapObjectId.New("pad"), PositionTiles = MapCoordinateSystem.SnapToTile(positionTiles), Tag = tag, Enabled = true,
            }).ToArray();
        });

    public static MapSnapshotCommand AddMarker(MapDefinition map, GameplayMarkerKind kind, Vector2 positionTiles, string pathId = "")
        => new("Place gameplay marker", target =>
        {
            target.Markers = (target.Markers ?? Array.Empty<GameplayMarker>()).Append(new GameplayMarker
            {
                Id = MapObjectId.New(kind.ToString()), Kind = kind, PositionTiles = MapCoordinateSystem.SnapToTile(positionTiles), PathId = pathId,
            }).ToArray();
        });

    public static MapSnapshotCommand AddPathPoint(MapDefinition map, string pathId, Vector2 pointTiles)
        => new("Add path point", target =>
        {
            var path = (target.Paths ?? Array.Empty<PathDefinition>()).FirstOrDefault(item => item?.Id == pathId)
                ?? throw new InvalidOperationException($"Path '{pathId}' was not found.");
            path.Points = (path.Points ?? Array.Empty<Vector2>()).Append(MapCoordinateSystem.SnapToTile(pointTiles)).ToArray();
            path.BakedRuntimePoints = path.Points.ToArray();
        });
}
#endif
