using System;

namespace FrontsOfWar.Map.Authoring;

public static class MapLoader
{
    public static MapDefinition Load(string mapIdOrPath)
    {
        string path = IsResourcePath(mapIdOrPath)
            ? mapIdOrPath
            : MapRegistry.ResolvePath(mapIdOrPath);
        return MapSerializer.Load(path);
    }

    public static string ResolvePath(string mapIdOrPath)
        => IsResourcePath(mapIdOrPath) ? mapIdOrPath : MapRegistry.ResolvePath(mapIdOrPath);

    private static bool IsResourcePath(string value)
        => !string.IsNullOrWhiteSpace(value)
            && (value.StartsWith("res://", StringComparison.Ordinal)
                || value.StartsWith("user://", StringComparison.Ordinal));
}
