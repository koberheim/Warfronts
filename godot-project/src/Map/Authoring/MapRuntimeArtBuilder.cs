using System;
using Godot;
using FrontsOfWar.Art;

namespace FrontsOfWar.Map.Authoring;

public static class MapRuntimeArtBuilder
{
    public static void Install(Node parent, MapDefinition map, float tileSize)
    {
        var scene = ResourceLoader.Load<PackedScene>("res://scenes/art/art_asset_slot.tscn")
            ?? throw new InvalidOperationException("The catalog art scene is missing.");
        var root = new Node2D { Name = "AuthoredArt" };
        parent.AddChild(root);
        foreach (var terrain in map.Terrain ?? Array.Empty<TerrainInstance>())
            AddSlot(root, scene, terrain.Id, terrain.AssetId,
                ((Vector2)terrain.Cell + Vector2.One * 0.5f) * tileSize,
                terrain.RotationQuarterTurns * Mathf.Pi / 2f, Vector2.One, -8);
        foreach (var asset in map.Assets ?? Array.Empty<MapAssetInstance>())
            if (asset.Enabled) AddAsset(root, scene, asset, tileSize);
        foreach (var cluster in map.Clusters ?? Array.Empty<ClusterInstance>())
        {
            if (!cluster.Enabled) continue;
            var group = new Node2D
            {
                Name = $"Cluster_{cluster.Id}", Position = cluster.PositionTiles * tileSize,
                Rotation = cluster.RotationRadians, Scale = Vector2.One * cluster.UniformScale,
            };
            root.AddChild(group);
            if ((cluster.Children?.Length ?? 0) == 0 && !string.IsNullOrWhiteSpace(cluster.CatalogAssetId))
                AddSlot(group, scene, cluster.Id, cluster.CatalogAssetId, Vector2.Zero, 0, Vector2.One, -2);
            foreach (var child in cluster.Children ?? Array.Empty<MapAssetInstance>())
                if (child.Enabled) AddAsset(group, scene, child, tileSize);
        }
    }

    private static void AddAsset(Node parent, PackedScene scene, MapAssetInstance asset, float tileSize)
        => AddSlot(parent, scene, asset.Id, asset.AssetId, asset.PositionTiles * tileSize,
            asset.RotationRadians, asset.Scale, (int)asset.Layer - 8);

    private static void AddSlot(Node parent, PackedScene scene, string id, string assetId,
        Vector2 position, float rotation, Vector2 scale, int zIndex)
    {
        var slot = scene.Instantiate<ArtAssetSprite>();
        slot.Name = $"Asset_{id}";
        slot.AssetId = assetId;
        slot.UseApprovedAsset = true;
#if DEBUG
        slot.AllowReviewAsset = true;
#endif
        slot.Position = position;
        slot.Rotation = rotation;
        slot.Scale = scale;
        slot.ZIndex = zIndex;
        parent.AddChild(slot);
    }
}
