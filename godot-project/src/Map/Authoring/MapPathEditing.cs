using System;
using System.Linq;
using Godot;

namespace FrontsOfWar.Map.Authoring;

public static class MapPathEditing
{
    public static PathDefinition AddPoint(PathDefinition path, Vector2 pointTiles)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        path.Points = (path.Points ?? Array.Empty<Vector2>()).Append(pointTiles).ToArray();
        path.BakedRuntimePoints = path.Points.ToArray();
        return path;
    }

    public static float LengthTiles(PathDefinition path)
    {
        float length = 0f;
        var points = path?.Points ?? Array.Empty<Vector2>();
        for (int i = 1; i < points.Length; i++) length += points[i - 1].DistanceTo(points[i]);
        return length;
    }

    public static bool IsTraversable(PathDefinition path)
        => path?.Points?.Length >= 2 && LengthTiles(path) > 0.01f;
}
