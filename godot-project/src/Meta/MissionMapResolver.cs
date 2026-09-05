using System;
using System.Linq;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Meta;

public static class MissionMapResolver
{
    public static MapDefinition Load(MissionDefinition mission)
    {
        if (mission == null || string.IsNullOrWhiteSpace(mission.MapId))
            throw new InvalidOperationException("Mission has no MapId.");
        return MapLoader.Load(mission.MapId);
    }

    public static void ValidateWavePaths(MissionDefinition mission, MapDefinition map)
    {
        if (mission?.WaveSequence?.Waves is not { Length: > 0 })
            throw new InvalidOperationException("Mission requires a nonempty wave sequence.");
        var validation = MapProductionValidator.Validate(map);
        if (!validation.CanPublish)
            throw new InvalidOperationException(string.Join("; ", validation.Errors.Select(item => item.Message)));
        foreach (var wave in mission.WaveSequence.Waves)
        {
            if (wave?.Groups is not { Length: > 0 }) throw new InvalidOperationException("Wave has no spawn groups.");
            foreach (var group in wave.Groups)
            {
                if (group?.Enemy?.ControllerScene == null || group.Count <= 0)
                    throw new InvalidOperationException($"Wave {wave.WaveNumber} has an invalid spawn group.");
                var path = map.Paths.FirstOrDefault(item => item.Id == group.PathId);
                if (path == null || wave.WaveNumber < path.ActiveFromWave ||
                    (path.ActiveUntilWave >= 0 && wave.WaveNumber > path.ActiveUntilWave))
                    throw new InvalidOperationException($"Wave {wave.WaveNumber} requires unavailable path '{group.PathId}'.");
                if (group.Enemy.IsAir && map.AirCorridors.Length == 0)
                    throw new InvalidOperationException($"Air wave {wave.WaveNumber} requires an authored air corridor.");
            }
        }
    }
}
