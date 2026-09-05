#if DEBUG
using System;
using System.Collections.Generic;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Editing;

public static class MapScatterService
{
    public static List<Vector2> GeneratePositions(int count, Vector2 min, Vector2 max, float spacing, ulong seed)
    {
        var random = new SeededRandom(seed);
        var result = new List<Vector2>();
        int attempts = Math.Max(100, count * 80);
        while (result.Count < count && attempts-- > 0)
        {
            var candidate = new Vector2(random.NextFloat(min.X, max.X), random.NextFloat(min.Y, max.Y));
            bool clear = true;
            foreach (var position in result) if (position.DistanceTo(candidate) < spacing) { clear = false; break; }
            if (clear) result.Add(candidate);
        }
        return result;
    }

    public static MapSnapshotCommand CreateAssetScatter(MapDefinition map, string assetId, string category,
        MapAssetLayer layer, int count, Vector2 min, Vector2 max, float spacing, ulong seed)
        => new("Scatter catalog assets", target =>
        {
            var assets = new List<MapAssetInstance>(target.Assets ?? Array.Empty<MapAssetInstance>());
            foreach (var position in GeneratePositions(count, min, max, spacing, seed))
                assets.Add(new MapAssetInstance { Id = MapObjectId.New("asset"), AssetId = assetId, Category = category ?? "", Layer = layer, PositionTiles = position, Scale = Vector2.One, DefaultScale = Vector2.One });
            target.Assets = assets.ToArray();
        });
}
#endif
