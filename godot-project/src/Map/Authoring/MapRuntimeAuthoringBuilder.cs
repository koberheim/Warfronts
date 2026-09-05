using System;
using System.Linq;
using Godot;
using FrontsOfWar.Core;

namespace FrontsOfWar.Map.Authoring;

// Validate before changing the live tree; coordinates are mission-local.
public static class MapRuntimeAuthoringBuilder
{
    public static NodePath InstallIntoMission(Node missionRoot, MapDefinition map, RuntimeMapData runtimeData,
        NodePath currentPath, Node towerPadParent)
    {
        if (missionRoot == null) throw new ArgumentNullException(nameof(missionRoot));
        if (runtimeData?.Paths.Count is not > 0) throw new InvalidOperationException("A playable map needs a ground path.");
        var validation = MapProductionValidator.Validate(map);
        if (!validation.CanPublish)
            throw new InvalidOperationException(string.Join("; ", validation.Errors.Select(error => error.Message)));

        // Detach first so old colliders cannot receive input this frame.
        foreach (var child in missionRoot.GetChildren())
            if (child is PathNetwork or BuildPad || child.Name == "ArtEnvironment" || child.Name == "AuthoredArt")
            {
                missionRoot.RemoveChild(child);
                child.QueueFree();
            }

        PathNetwork first = null;
        float tileSize = GameBalanceConfigAutoload.Config.TilePixelSize;
        foreach (var pathData in runtimeData.Paths)
        {
            var network = new PathNetwork
            {
                Name = $"AuthoredPathNetwork_{pathData.Id}", PathId = pathData.Id,
                ActiveFromWave = pathData.ActiveFromWave, ActiveUntilWave = pathData.ActiveUntilWave,
            };
            var route = new Path2D { Name = "Route", Curve = new Curve2D() };
            var points = pathData.Points.Select(point => MapCoordinateSystem.TileToPixel(point, tileSize)).ToArray();
            foreach (var point in points) route.Curve.AddPoint(point);
            network.AddChild(route);
            network.AddChild(new Line2D
            {
                Name = "Road", Points = points, Width = tileSize * 0.8f,
                DefaultColor = new Color("776b4d"), ZIndex = -4,
                BeginCapMode = Line2D.LineCapMode.Round, EndCapMode = Line2D.LineCapMode.Round,
                JointMode = Line2D.LineJointMode.Round,
            });
            missionRoot.AddChild(network);
            first ??= network;
        }
        InstallPads(towerPadParent ?? missionRoot, map, tileSize);
        MapRuntimeArtBuilder.Install(missionRoot, map, tileSize);
        return missionRoot.GetPathTo(first);
    }

    private static void InstallPads(Node parent, MapDefinition map, float tileSize)
    {
        var packedPad = ResourceLoader.Load<PackedScene>("res://scenes/map/build_pad.tscn")
            ?? throw new InvalidOperationException("The build pad scene is missing.");
        foreach (var node in map.TowerNodes ?? Array.Empty<TowerPlacementNode>())
        {
            if (node == null || !node.Enabled) continue;
            var pad = packedPad.Instantiate<BuildPad>();
            pad.Name = $"AuthoredPad_{node.Id}";
            pad.Tag = node.Tag;
            pad.AllowedArchetypeIds = (string[])(node.AllowedArchetypeIds ?? Array.Empty<string>()).Clone();
            pad.ArcFacingDegrees = node.ArcFacingDegrees;
            pad.ArcHalfAngleDegrees = node.ArcHalfAngleDegrees;
            pad.Position = MapCoordinateSystem.TileToPixel(node.PositionTiles, tileSize);
            pad.Rotation = node.RotationRadians;
            parent.AddChild(pad);
        }
    }

    public static AirCorridorDefinition BuildAirCorridor(RuntimeMapData data, float tileSize)
    {
        var air = data.AirCorridors.FirstOrDefault();
        return air == null ? null : new AirCorridorDefinition
        {
            EntryPosition = MapCoordinateSystem.TileToPixel(air.EntryPositionTiles, tileSize),
            ObjectivePosition = MapCoordinateSystem.TileToPixel(air.ObjectivePositionTiles, tileSize),
        };
    }
}
