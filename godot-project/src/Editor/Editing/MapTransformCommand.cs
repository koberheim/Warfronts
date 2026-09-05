#if DEBUG
using System;
using System.Collections.Generic;
using Godot;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Editor.Editing;

public sealed class MapTransformCommand : MapSnapshotCommand
{
    private MapTransformCommand(string description, Action<MapDefinition> operation) : base(description, operation) { }

    public static MapTransformCommand Move(MapDefinition map, IEnumerable<string> ids, Vector2 deltaTiles)
        => new("Move selection", target => ForEach(target, ids, handle =>
        {
            if (!MapObjectLocator.TryGetTransform(handle, out var transform)) throw Unsupported(handle);
            MapObjectLocator.ApplyTransform(handle, new MapObjectTransform(transform.PositionTiles + deltaTiles, transform.RotationRadians, transform.Scale, transform.CanRotate, transform.CanScale));
        }));

    public static MapTransformCommand Rotate(MapDefinition map, IEnumerable<string> ids, float radians)
        => new("Rotate selection", target => ForEach(target, ids, handle =>
        {
            if (!MapObjectLocator.TryGetTransform(handle, out var transform) || !transform.CanRotate) throw Unsupported(handle);
            MapObjectLocator.ApplyTransform(handle, new MapObjectTransform(transform.PositionTiles, transform.RotationRadians + radians, transform.Scale, transform.CanRotate, transform.CanScale));
        }));

    public static MapTransformCommand Scale(MapDefinition map, IEnumerable<string> ids, float factor)
        => new("Scale selection", target => ForEach(target, ids, handle =>
        {
            if (!MapObjectLocator.TryGetTransform(handle, out var transform) || !transform.CanScale || factor <= 0f) throw Unsupported(handle);
            float scalar = MapCoordinateSystem.ClampUniformScale(transform.Scale.X * factor, 0.1f, 8f);
            MapObjectLocator.ApplyTransform(handle, new MapObjectTransform(transform.PositionTiles, transform.RotationRadians, Vector2.One * scalar, transform.CanRotate, transform.CanScale));
        }));

    public static MapTransformCommand Set(MapDefinition map, string id, MapObjectTransform transform)
        => new("Edit inspector transform", target =>
        {
            var handle = MapObjectLocator.Find(target, id);
            if (handle == null) throw new InvalidOperationException($"Object '{id}' was not found.");
            if ((handle.Resource is MapAssetInstance or ClusterInstance) && !MapCoordinateSystem.IsUniformScale(transform.Scale))
                throw new InvalidOperationException("Scale must remain uniform.");
            MapObjectLocator.ApplyTransform(handle, transform);
        });

    private static void ForEach(MapDefinition map, IEnumerable<string> ids, Action<MapObjectHandle> action)
    {
        bool changed = false;
        foreach (string id in ids)
        {
            var handle = MapObjectLocator.Find(map, id);
            if (handle == null) throw new InvalidOperationException($"Object '{id}' was not found.");
            action(handle); changed = true;
        }
        if (!changed) throw new InvalidOperationException("Select at least one map object.");
    }

    private static Exception Unsupported(MapObjectHandle handle)
        => new InvalidOperationException($"Object '{handle?.Id}' does not support that transform.");
}
#endif
