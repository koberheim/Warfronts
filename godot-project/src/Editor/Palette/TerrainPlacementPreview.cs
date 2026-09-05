#if DEBUG
using Godot;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Palette;

public static class TerrainPlacementPreview
{
    public static Vector2I Snap(Vector2 positionTiles)
        => new(Mathf.RoundToInt(positionTiles.X), Mathf.RoundToInt(positionTiles.Y));

    public static bool CanPlace(MapDefinition map, Vector2 positionTiles)
    {
        var cell = Snap(positionTiles);
        if (map?.Metadata == null || cell.X < 0 || cell.Y < 0 || cell.X >= map.Metadata.WidthTiles || cell.Y >= map.Metadata.HeightTiles) return false;
        foreach (var terrain in map.Terrain ?? System.Array.Empty<TerrainInstance>())
            if (terrain != null && terrain.Cell == cell) return false;
        return true;
    }
}
#endif
