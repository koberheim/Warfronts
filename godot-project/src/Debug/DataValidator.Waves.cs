using System;
using FrontsOfWar.Waves;

namespace FrontsOfWar.Debug;

public static partial class DataValidator
{
    private static void ValidateSpawnGroup(string path, SpawnGroup group, DataValidationReport report)
    {
        if (group == null)
        {
            report.AddError(path, "SpawnGroup entry is null.");
            return;
        }

        if (group.Enemy == null)
            report.AddError(path, "SpawnGroup has a null Enemy reference.");

        if (group.Count < 1)
            report.AddError(path, $"SpawnGroup Count must be >= 1 (found {group.Count}).");

        if (group.IntervalSeconds < 0f)
            report.AddError(path, $"SpawnGroup IntervalSeconds must be >= 0 (found {group.IntervalSeconds}).");
    }

    private static void ValidateWaveGroups(string path, int waveNumber, SpawnGroup[] groups, DataValidationReport report)
    {
        if (groups == null) return;
        foreach (var group in groups)
            ValidateSpawnGroup(path, group, report);
    }

    private static void ValidateWaveSequence(string path, WaveSequence sequence, DataValidationReport report)
    {
        int previousWaveNumber = int.MinValue;
        foreach (var wave in sequence.Waves ?? Array.Empty<WaveDefinition>())
        {
            if (wave == null)
            {
                report.AddError(path, "WaveSequence contains a null WaveDefinition entry.");
                continue;
            }

            ValidateWaveGroups(path, wave.WaveNumber, wave.Groups, report);

            if (wave.WaveNumber <= previousWaveNumber)
                report.AddError(path,
                    $"WaveNumbers must strictly increase within a sequence; {wave.WaveNumber} does not follow {previousWaveNumber}.");
            previousWaveNumber = wave.WaveNumber;
        }
    }
}
