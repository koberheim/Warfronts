using System;
using Godot;
using FrontsOfWar.Core;

namespace FrontsOfWar.Map.Authoring;

// One conversion surface for authored tile coordinates. Pixel scale always
// comes from GameBalanceConfig; authoring tools never duplicate the 64px
// baseline as a local constant.
public static class MapCoordinateSystem
{
    public static Vector2 TileToPixel(Vector2 tilePosition, GameBalanceConfig config)
        => TileToPixel(tilePosition, config.TilePixelSize);

    public static Vector2 TileToPixel(Vector2 tilePosition, float tilePixelSize)
    {
        RequirePositiveFinite(tilePixelSize, nameof(tilePixelSize));
        return tilePosition * tilePixelSize;
    }

    public static Vector2 PixelToTile(Vector2 pixelPosition, GameBalanceConfig config)
        => PixelToTile(pixelPosition, config.TilePixelSize);

    public static Vector2 PixelToTile(Vector2 pixelPosition, float tilePixelSize)
    {
        RequirePositiveFinite(tilePixelSize, nameof(tilePixelSize));
        return pixelPosition / tilePixelSize;
    }

    public static Vector2 SnapToTile(Vector2 tilePosition, float incrementTiles = 1f)
    {
        RequirePositiveFinite(incrementTiles, nameof(incrementTiles));
        return new Vector2(
            Mathf.Round(tilePosition.X / incrementTiles) * incrementTiles,
            Mathf.Round(tilePosition.Y / incrementTiles) * incrementTiles);
    }

    public static int NormalizeQuarterTurns(int quarterTurns)
        => ((quarterTurns % 4) + 4) % 4;

    public static float NormalizeRotation(float radians)
        => Mathf.PosMod(radians, Mathf.Tau);

    public static float ClampUniformScale(float requested, float minimum, float maximum)
    {
        RequirePositiveFinite(minimum, nameof(minimum));
        RequirePositiveFinite(maximum, nameof(maximum));
        if (minimum > maximum) throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum scale cannot exceed maximum scale.");
        if (!float.IsFinite(requested)) throw new ArgumentOutOfRangeException(nameof(requested), "Scale must be finite.");
        return Mathf.Clamp(requested, minimum, maximum);
    }

    public static bool IsUniformScale(Vector2 scale, float tolerance = 0.0001f)
        => IsFinite(scale) && scale.X > 0f && scale.Y > 0f && Mathf.Abs(scale.X - scale.Y) <= tolerance;

    public static bool IsFinite(Vector2 value)
        => float.IsFinite(value.X) && float.IsFinite(value.Y);

    private static void RequirePositiveFinite(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value <= 0f)
            throw new ArgumentOutOfRangeException(parameterName, "Value must be positive and finite.");
    }
}
