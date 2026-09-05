#if DEBUG
using System;
using Godot;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Rendering;

public enum MapRenderKind { Terrain, Asset, Cluster, Path, AirCorridor, TowerNode, Marker, Zone }

public sealed class MapRenderItem
{
    public string Id { get; init; } = "";
    public MapRenderKind Kind { get; init; }
    public Vector2 PositionTiles { get; init; }
    public Vector2 SizeTiles { get; init; } = Vector2.One;
    public float RotationRadians { get; init; }
    public Color Color { get; init; } = Colors.White;
}

public sealed class MapRenderPath
{
    public string Id { get; init; } = "";
    public Vector2[] PointsTiles { get; init; } = Array.Empty<Vector2>();
    public Color Color { get; init; } = Colors.White;
    public float WidthTiles { get; init; } = 0.18f;
}

public sealed class MapRenderSnapshot
{
    public int WidthTiles { get; init; }
    public int HeightTiles { get; init; }
    public MapRenderItem[] Items { get; init; } = Array.Empty<MapRenderItem>();
    public MapRenderPath[] Paths { get; init; } = Array.Empty<MapRenderPath>();
}
#endif
