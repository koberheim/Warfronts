using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.Meta;

namespace FrontsOfWar.Debug;

public static partial class DataValidator
{
    private static void ValidateMapsAndMissions(IReadOnlyList<(string Path, Resource Resource)> resources, DataValidationReport report)
    {
        var maps = resources.Where(item => item.Resource is MapDefinition)
            .Select(item => (item.Path, Map: (MapDefinition)item.Resource)).ToList();
        foreach (var (path, map) in maps)
        {
            var validation = MapProductionValidator.Validate(map,
                requireApprovedArt: map.Metadata?.Status == MapAuthoringStatus.Production);
            foreach (var error in validation.Errors) report.AddError(path, error.Message);
        }
        foreach (var (path, resource) in resources)
        {
            if (resource is not MissionDefinition mission) continue;
            var map = maps.FirstOrDefault(item => item.Map.Metadata?.Id == mission.MapId).Map;
            if (map == null)
            {
                report.AddError(path, $"Mission MapId '{mission.MapId}' does not resolve to an authored map.");
                continue;
            }
            try { MissionMapResolver.ValidateWavePaths(mission, map); }
            catch (Exception error) { report.AddError(path, error.Message); }
        }
    }
}
