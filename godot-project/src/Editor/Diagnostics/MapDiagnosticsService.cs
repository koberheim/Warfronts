#if DEBUG
using System.Linq;
using Godot;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Diagnostics;

public static class MapDiagnosticsService
{
    public static MapProductionValidationResult Validate(MapDefinition map)
        => MapProductionValidator.Validate(map, null, false);

    public static string Summary(MapProductionValidationResult result)
        => result == null ? "NO DIAGNOSTICS" : $"{result.Errors.Count()} ERRORS   {result.Warnings.Count()} WARNINGS";

    public static Vector2? FocusPoint(MapDefinition map, MapDiagnostic diagnostic)
    {
        if (map == null || diagnostic == null || string.IsNullOrWhiteSpace(diagnostic.ObjectId)) return null;
        var handle = Editor.Editing.MapObjectLocator.Find(map, diagnostic.ObjectId);
        return handle != null && Editor.Editing.MapObjectLocator.TryGetTransform(handle, out var transform) ? transform.PositionTiles : null;
    }
}
#endif
