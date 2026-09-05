#if DEBUG
using System;
using System.Linq;
using Godot;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Editing;

public static class MapAssetCommands
{
    public static MapSnapshotCommand AddAsset(MapDefinition map, string assetId, string category, Vector2 positionTiles,
        MapAssetLayer layer = MapAssetLayer.Flavor, Vector2 scale = default)
        => new("Place catalog asset", target =>
        {
            if (string.IsNullOrWhiteSpace(assetId)) throw new InvalidOperationException("An asset catalog ID is required.");
            Vector2 actualScale = scale == default ? Vector2.One : scale;
            target.Assets = (target.Assets ?? Array.Empty<MapAssetInstance>()).Append(new MapAssetInstance
            {
                Id = MapObjectId.New("asset"), AssetId = assetId, Category = category ?? "", Layer = layer,
                PositionTiles = MapCoordinateSystem.SnapToTile(positionTiles), Scale = actualScale, DefaultScale = actualScale,
            }).ToArray();
        });
}
#endif
