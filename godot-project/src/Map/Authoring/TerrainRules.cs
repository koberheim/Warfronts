using System;
using System.Collections.Generic;
using Godot;

namespace FrontsOfWar.Map.Authoring;

public sealed class TerrainRuleSet
{
    public string Id { get; init; } = "default";
    public HashSet<string> AllowedAdjacencyVariants { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> RequiredSockets { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public TerrainCollisionClass DefaultCollision { get; init; } = TerrainCollisionClass.Decorative;
    public bool AllowsRotation { get; init; } = true;

    public static TerrainRuleSet Default { get; } = new();
}

public static class TerrainAuthoringRules
{
    public static bool IsRotationLegal(TerrainInstance terrain, TerrainRuleSet rules)
        => terrain != null && rules != null && rules.AllowsRotation && terrain.RotationQuarterTurns is >= 0 and <= 3;

    public static MapDefinitionValidationResult Validate(MapDefinition map, TerrainRuleSet rules = null)
    {
        rules ??= TerrainRuleSet.Default;
        var result = new MapDefinitionValidationResult();
        if (map?.Metadata == null) { result.Errors.Add("Terrain validation requires map metadata."); return result; }
        var occupied = new Dictionary<Vector2I, TerrainInstance>();
        foreach (var terrain in map.Terrain ?? Array.Empty<TerrainInstance>())
        {
            if (terrain == null) continue;
            if (!occupied.TryAdd(terrain.Cell, terrain)) result.Errors.Add($"Terrain cell {terrain.Cell} is occupied more than once.");
            if (!IsRotationLegal(terrain, rules)) result.Errors.Add($"Terrain '{terrain.Id}' has an illegal rotation.");
            if (!string.IsNullOrWhiteSpace(terrain.AdjacencyVariant) && rules.AllowedAdjacencyVariants.Count > 0 &&
                !rules.AllowedAdjacencyVariants.Contains(terrain.AdjacencyVariant))
                result.Errors.Add($"Terrain '{terrain.Id}' uses unknown adjacency variant '{terrain.AdjacencyVariant}'.");
            if (terrain.Cell.X < 0 || terrain.Cell.Y < 0 || terrain.Cell.X >= map.Metadata.WidthTiles || terrain.Cell.Y >= map.Metadata.HeightTiles)
                result.Errors.Add($"Terrain '{terrain.Id}' is outside map bounds.");
        }
        foreach (var terrain in occupied.Values)
        {
            if (!string.IsNullOrWhiteSpace(terrain.SocketMetadata) && rules.RequiredSockets.Count > 0 &&
                !rules.RequiredSockets.Contains(terrain.SocketMetadata))
                result.Errors.Add($"Terrain '{terrain.Id}' has an unsupported route socket '{terrain.SocketMetadata}'.");
        }
        return result;
    }
}
