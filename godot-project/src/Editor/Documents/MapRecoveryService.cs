#if DEBUG
using System;
using Godot;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Documents;

public static class MapRecoveryService
{
    public static string RecoveryPath(string mapId)
        => $"user://map-editor-recovery/{MapObjectId.NormalizePrefix(mapId)}.recovery.tres";

    public static bool Write(MapDocument document)
    {
        if (document?.Current == null || !document.IsDirty) return false;
        MapSerializer.SaveAs(document.Current, RecoveryPath(document.Current.Metadata?.Id ?? "untitled"));
        return true;
    }

    public static MapDefinition TryLoad(string mapId)
    {
        string path = RecoveryPath(mapId);
        return FileAccess.FileExists(path) ? MapSerializer.Load(path) : null;
    }

    public static void Clear(string mapId)
    {
        string path = RecoveryPath(mapId);
        if (FileAccess.FileExists(path)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(path));
    }
}
#endif
