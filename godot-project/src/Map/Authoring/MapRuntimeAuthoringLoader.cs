using System;
using System.Linq;
using Godot;
using FrontsOfWar.Core;

namespace FrontsOfWar.Map.Authoring;

// Runtime boundary used by developer preview and future mission scene
// construction. It exposes validated authored data without giving runtime
// systems a dependency on editor assemblies.
public static class MapRuntimeAuthoringLoader
{
    public static bool TryLoadFromCommandLine(out MapDefinition map, out RuntimeMapData runtimeData, out string error)
    {
        string mapId = MapPreviewArguments.ReadMapId(OS.GetCmdlineArgs());
        if (string.IsNullOrWhiteSpace(mapId))
        {
            map = null; runtimeData = null; error = "";
            return false;
        }
        try
        {
            map = MapLoader.Load(mapId);
            var validation = MapProductionValidator.Validate(map);
            if (!validation.CanPublish)
                throw new InvalidOperationException(string.Join("; ", validation.Errors.Select(item => item.Message)));
            runtimeData = MapRuntimeDataFactory.Build(map);
            error = "";
            return true;
        }
        catch (Exception exception)
        {
            map = null; runtimeData = null; error = exception.Message;
            GD.PushError($"Developer map preview could not load '{mapId}': {error}");
            return false;
        }
    }
}
