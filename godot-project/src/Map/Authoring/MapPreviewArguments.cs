using System;

namespace FrontsOfWar.Map.Authoring;

public static class MapPreviewArguments
{
    public static string ReadMapId(string[] args)
    {
        if (args == null) return "";
        for (int i = 0; i + 1 < args.Length; i++)
            if (args[i] == "--map-id") return args[i + 1] ?? "";
        return "";
    }

    public static string Build(string mapId)
        => string.IsNullOrWhiteSpace(mapId) ? "" : $"--map-id {mapId}";
}
