#if DEBUG
using System;
using System.Linq;
using Godot;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Editing;

public static class MapObjectOperations
{
    public static MapSnapshotCommand Delete(MapDefinition map, string[] ids)
        => new("Delete selection", target =>
        {
            target.Terrain = target.Terrain.Where(item => item != null && !ids.Contains(item.Id)).ToArray();
            target.Assets = target.Assets.Where(item => item != null && !ids.Contains(item.Id)).ToArray();
            target.Clusters = target.Clusters.Where(item => item != null && !ids.Contains(item.Id)).ToArray();
            target.Paths = target.Paths.Where(item => item != null && !ids.Contains(item.Id)).ToArray();
            target.AirCorridors = target.AirCorridors.Where(item => item != null && !ids.Contains(item.Id)).ToArray();
            target.TowerNodes = target.TowerNodes.Where(item => item != null && !ids.Contains(item.Id)).ToArray();
            target.Markers = target.Markers.Where(item => item != null && !ids.Contains(item.Id)).ToArray();
            target.Zones = target.Zones.Where(item => item != null && !ids.Contains(item.Id)).ToArray();
            target.Gimmicks = target.Gimmicks.Where(item => item != null && !ids.Contains(item.Id)).ToArray();
            foreach (var cluster in target.Clusters)
                cluster.Children = (cluster.Children ?? Array.Empty<MapAssetInstance>()).Where(item => item != null && !ids.Contains(item.Id)).ToArray();
        });

    public static MapSnapshotCommand Duplicate(MapDefinition map, string[] ids, Vector2 offsetTiles = default)
        => new("Duplicate selection", target =>
        {
            foreach (string id in ids)
            {
                var handle = MapObjectLocator.Find(target, id);
                if (handle == null) continue;
                var copy = handle.Resource.Duplicate(true) as Resource;
                string newId = MapObjectId.New(id);
                if (copy is MapAssetInstance child && handle.Kind == MapObjectKind.ClusterChild)
                {
                    child.Id = newId; child.PositionTiles += offsetTiles;
                    handle.ParentCluster.Children = handle.ParentCluster.Children.Append(child).ToArray(); continue;
                }
                switch (copy)
                {
                    case TerrainInstance terrain: terrain.Id = newId; terrain.Cell += new Vector2I(Mathf.RoundToInt(offsetTiles.X), Mathf.RoundToInt(offsetTiles.Y)); target.Terrain = target.Terrain.Append(terrain).ToArray(); break;
                    case MapAssetInstance asset: asset.Id = newId; asset.PositionTiles += offsetTiles; target.Assets = target.Assets.Append(asset).ToArray(); break;
                    case ClusterInstance cluster:
                        cluster.Id = newId; cluster.PositionTiles += offsetTiles;
                        foreach (var childAsset in cluster.Children ?? Array.Empty<MapAssetInstance>()) if (childAsset != null) childAsset.Id = MapObjectId.New(childAsset.Id);
                        target.Clusters = target.Clusters.Append(cluster).ToArray(); break;
                    case PathDefinition path: path.Id = newId; target.Paths = target.Paths.Append(path).ToArray(); break;
                    case MapAirCorridorDefinition corridor: corridor.Id = newId; target.AirCorridors = target.AirCorridors.Append(corridor).ToArray(); break;
                    case TowerPlacementNode node: node.Id = newId; node.PositionTiles += offsetTiles; target.TowerNodes = target.TowerNodes.Append(node).ToArray(); break;
                    case GameplayMarker marker: marker.Id = newId; marker.PositionTiles += offsetTiles; target.Markers = target.Markers.Append(marker).ToArray(); break;
                    case MapZone zone: zone.Id = newId; zone.CenterTiles += offsetTiles; target.Zones = target.Zones.Append(zone).ToArray(); break;
                    case MapGimmick gimmick: gimmick.Id = newId; target.Gimmicks = target.Gimmicks.Append(gimmick).ToArray(); break;
                    default: throw new InvalidOperationException($"Object '{id}' cannot be duplicated in this phase.");
                }
            }
        });
}

public static class MapClipboard
{
    private static MapDefinition _contents;
    public static bool HasContent => _contents != null;

    public static void Copy(MapDefinition map, string[] ids)
    {
        _contents = new MapDefinition { Metadata = map.Metadata.Duplicate(true) as MapMetadata };
        foreach (string id in ids)
        {
            var handle = MapObjectLocator.Find(map, id);
            if (handle == null) continue;
            switch (handle.Resource.Duplicate(true))
            {
                case TerrainInstance terrain: _contents.Terrain = _contents.Terrain.Append(terrain).ToArray(); break;
                case MapAssetInstance asset: _contents.Assets = _contents.Assets.Append(asset).ToArray(); break;
                case ClusterInstance cluster: _contents.Clusters = _contents.Clusters.Append(cluster).ToArray(); break;
                case TowerPlacementNode node: _contents.TowerNodes = _contents.TowerNodes.Append(node).ToArray(); break;
                case GameplayMarker marker: _contents.Markers = _contents.Markers.Append(marker).ToArray(); break;
                case MapZone zone: _contents.Zones = _contents.Zones.Append(zone).ToArray(); break;
                case PathDefinition path: _contents.Paths = _contents.Paths.Append(path).ToArray(); break;
                case MapAirCorridorDefinition corridor: _contents.AirCorridors = _contents.AirCorridors.Append(corridor).ToArray(); break;
                case MapGimmick gimmick: _contents.Gimmicks = _contents.Gimmicks.Append(gimmick).ToArray(); break;
            }
        }
    }

    public static MapSnapshotCommand Paste(MapDefinition map, Vector2 offsetTiles = default)
        => new("Paste selection", target =>
        {
            if (_contents == null) throw new InvalidOperationException("Clipboard is empty.");
            foreach (var item in _contents.Terrain ?? Array.Empty<TerrainInstance>())
            {
                var copy = item.Duplicate(true) as TerrainInstance; copy.Id = MapObjectId.New(item.Id); copy.Cell += new Vector2I(Mathf.RoundToInt(offsetTiles.X), Mathf.RoundToInt(offsetTiles.Y)); target.Terrain = target.Terrain.Append(copy).ToArray();
            }
            foreach (var item in _contents.Assets ?? Array.Empty<MapAssetInstance>())
            {
                var copy = item.Duplicate(true) as MapAssetInstance; copy.Id = MapObjectId.New(item.Id); copy.PositionTiles += offsetTiles; target.Assets = target.Assets.Append(copy).ToArray();
            }
            foreach (var item in _contents.Clusters ?? Array.Empty<ClusterInstance>())
            {
                var copy = item.Duplicate(true) as ClusterInstance; copy.Id = MapObjectId.New(item.Id); copy.PositionTiles += offsetTiles;
                foreach (var child in copy.Children ?? Array.Empty<MapAssetInstance>()) if (child != null) child.Id = MapObjectId.New(child.Id);
                target.Clusters = target.Clusters.Append(copy).ToArray();
            }
            foreach (var item in _contents.Paths ?? Array.Empty<PathDefinition>()) { var copy = item.Duplicate(true) as PathDefinition; copy.Id = MapObjectId.New(item.Id); target.Paths = target.Paths.Append(copy).ToArray(); }
            foreach (var item in _contents.AirCorridors ?? Array.Empty<MapAirCorridorDefinition>()) { var copy = item.Duplicate(true) as MapAirCorridorDefinition; copy.Id = MapObjectId.New(item.Id); target.AirCorridors = target.AirCorridors.Append(copy).ToArray(); }
            foreach (var item in _contents.TowerNodes ?? Array.Empty<TowerPlacementNode>()) { var copy = item.Duplicate(true) as TowerPlacementNode; copy.Id = MapObjectId.New(item.Id); copy.PositionTiles += offsetTiles; target.TowerNodes = target.TowerNodes.Append(copy).ToArray(); }
            foreach (var item in _contents.Markers ?? Array.Empty<GameplayMarker>()) { var copy = item.Duplicate(true) as GameplayMarker; copy.Id = MapObjectId.New(item.Id); copy.PositionTiles += offsetTiles; target.Markers = target.Markers.Append(copy).ToArray(); }
            foreach (var item in _contents.Zones ?? Array.Empty<MapZone>()) { var copy = item.Duplicate(true) as MapZone; copy.Id = MapObjectId.New(item.Id); copy.CenterTiles += offsetTiles; target.Zones = target.Zones.Append(copy).ToArray(); }
            foreach (var item in _contents.Gimmicks ?? Array.Empty<MapGimmick>()) { var copy = item.Duplicate(true) as MapGimmick; copy.Id = MapObjectId.New(item.Id); target.Gimmicks = target.Gimmicks.Append(copy).ToArray(); }
        });
}
#endif
