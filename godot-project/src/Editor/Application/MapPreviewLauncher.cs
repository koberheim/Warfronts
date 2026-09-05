#if DEBUG
using System;
using Godot;

namespace FrontsOfWar.Editor.Application;

public static class MapPreviewLauncher
{
    public static string[] BuildArguments(string projectPath, string mapId)
        => new[] { "--path", projectPath ?? "", "--mission", "--map-id", mapId ?? "", "--skip-tutorial" };

    public static Error Launch(string projectPath, string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId)) throw new InvalidOperationException("Save the map with a valid ID before previewing it.");
        int processId = OS.CreateProcess(OS.GetExecutablePath(), BuildArguments(projectPath, mapId));
        return processId > 0 ? Error.Ok : Error.CantFork;
    }
}
#endif
