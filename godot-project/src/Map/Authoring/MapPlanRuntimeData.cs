using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FrontsOfWar.Map.Authoring;

public sealed class RuntimeMapData
{
    public string MapId { get; init; } = "";
    public IReadOnlyList<RuntimePathData> Paths { get; init; } = Array.Empty<RuntimePathData>();
    public IReadOnlyList<RuntimePadData> Pads { get; init; } = Array.Empty<RuntimePadData>();
    public IReadOnlyList<RuntimeAirCorridorData> AirCorridors { get; init; } = Array.Empty<RuntimeAirCorridorData>();
    public IReadOnlyList<RuntimeGimmickData> Gimmicks { get; init; } = Array.Empty<RuntimeGimmickData>();
}

// GDD §11.2: "one boolean or one timer" per gimmick, authored per map. Type
// is a free-form string discriminator (see GimmickSystem) rather than an
// enum, matching MapGimmick's own free-form Type - MapDefinitionValidator
// only requires it non-empty, not a member of a fixed list.
public sealed class RuntimeGimmickData
{
    public string Id { get; init; } = "";
    public string Type { get; init; } = "";
    public bool Enabled { get; init; } = true;
    public IReadOnlyList<string> PathIds { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> Parameters { get; init; } = new Dictionary<string, string>();
}

public sealed class RuntimePathData
{
    public string Id { get; init; } = "";
    public string EntryMarkerId { get; init; } = "";
    public Vector2[] Points { get; init; } = Array.Empty<Vector2>();
    public string BranchGroupId { get; init; } = "";
    public int ActiveFromWave { get; init; } = 1;
    public int ActiveUntilWave { get; init; } = -1;
}

public sealed class RuntimePadData
{
    public string Id { get; init; } = "";
    public Vector2 PositionTiles { get; init; }
    public PadTag Tag { get; init; }
    public string[] AllowedArchetypeIds { get; init; } = Array.Empty<string>();
    public bool Enabled { get; init; }
    public float ArcFacingDegrees { get; init; }
    public float ArcHalfAngleDegrees { get; init; } = 180f;
}

public sealed class RuntimeAirCorridorData
{
    public string Id { get; init; } = "";
    public Vector2 EntryPositionTiles { get; init; }
    public Vector2 ObjectivePositionTiles { get; init; }
    public float WidthTiles { get; init; }
}

public static class MapRuntimeDataFactory
{
    public static RuntimeMapData Build(MapDefinition map)
    {
        if (map?.Metadata == null) throw new ArgumentException("A map with metadata is required.", nameof(map));
        return new RuntimeMapData
        {
            MapId = map.Metadata.Id,
            Paths = (map.Paths ?? Array.Empty<PathDefinition>()).Where(path => path != null).Select(path => new RuntimePathData
            {
                Id = path.Id, EntryMarkerId = path.EntryMarkerId, Points = (Godot.Vector2[])((path.BakedRuntimePoints?.Length ?? 0) >= 2 ? path.BakedRuntimePoints : path.Points).Clone(),
                BranchGroupId = path.BranchGroupId, ActiveFromWave = path.ActiveFromWave, ActiveUntilWave = path.ActiveUntilWave,
            }).ToArray(),
            Pads = (map.TowerNodes ?? Array.Empty<TowerPlacementNode>()).Where(node => node != null).Select(node => new RuntimePadData
            {
                Id = node.Id, PositionTiles = node.PositionTiles, Tag = node.Tag, AllowedArchetypeIds = node.AllowedArchetypeIds ?? Array.Empty<string>(), Enabled = node.Enabled,
                ArcFacingDegrees = node.ArcFacingDegrees, ArcHalfAngleDegrees = node.ArcHalfAngleDegrees,
            }).ToArray(),
            AirCorridors = (map.AirCorridors ?? Array.Empty<MapAirCorridorDefinition>()).Where(corridor => corridor != null).Select(corridor => new RuntimeAirCorridorData
            {
                Id = corridor.Id, EntryPositionTiles = corridor.EntryPositionTiles, ObjectivePositionTiles = corridor.ObjectivePositionTiles, WidthTiles = corridor.WidthTiles,
            }).ToArray(),
            Gimmicks = (map.Gimmicks ?? Array.Empty<MapGimmick>()).Where(gimmick => gimmick != null).Select(gimmick => new RuntimeGimmickData
            {
                Id = gimmick.Id, Type = gimmick.Type, Enabled = gimmick.Enabled,
                PathIds = (string[])(gimmick.PathIds ?? Array.Empty<string>()).Clone(),
                Parameters = (gimmick.Parameters ?? Array.Empty<MapProperty>()).Where(p => p != null)
                    .ToDictionary(p => p.Key, p => p.Value),
            }).ToArray(),
        };
    }
}
