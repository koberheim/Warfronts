using System.Collections.Generic;
using Godot;

namespace FrontsOfWar.Waves;

public sealed class WavePacingReport
{
    public readonly List<string> Warnings = new();
    public float ThreatValue;
    public float EstimatedSeconds;
    public bool IsValid => Warnings.Count == 0;
}

public static class WavePacingAnalyzer
{
    public static WavePacingReport Analyze(WaveDefinition wave, WaveDefinition previous = null,
                                           int sequenceIndex = 0)
    {
        var report = new WavePacingReport { ThreatValue = ThreatValueCalculator.Calculate(wave) };
        report.EstimatedSeconds = EstimateDuration(wave);
        if (report.EstimatedSeconds > 75f)
            report.Warnings.Add($"Estimated duration {report.EstimatedSeconds:0.0}s exceeds the 75s limit.");

        if (wave?.Groups != null && wave.Groups.Length > 0)
        {
            var last = wave.Groups[wave.Groups.Length - 1];
            if (last?.Enemy != null && last.Count <= 1 && last.Enemy.BaseHp >= 400f &&
                last.Enemy.MoveSpeedTilesPerSec <= 1f)
                report.Warnings.Add("Cleanup tail: the wave ends with one slow, high-HP enemy.");
        }

        if (previous != null)
        {
            float previousThreat = ThreatValueCalculator.Calculate(previous);
            if (previousThreat > 0f && report.ThreatValue > previousThreat * 1.5f)
                report.Warnings.Add($"Spike: Threat Value is {report.ThreatValue / previousThreat:0.00}× the previous wave.");
            if (wave.WaveNumber > 6 && wave.WaveNumber % 3 == 0 &&
                report.ThreatValue > previousThreat * 0.7f)
                report.Warnings.Add("Recovery wave should be at or below 70% of the previous wave.");
        }

        if (wave?.IsAirWave == true && sequenceIndex < 3)
            report.Warnings.Add("Air wave needs three waves of advance announcement.");
        if (wave?.IsBossWave == true && wave.BuildTimeSeconds < 40f)
            report.Warnings.Add("Boss wave needs at least a 40s build window.");
        return report;
    }

    public static float EstimateDuration(WaveDefinition wave)
    {
        float lastSpawn = 0f;
        if (wave?.Groups == null) return lastSpawn;
        foreach (var group in wave.Groups)
        {
            if (group?.Enemy == null || group.Count <= 0) continue;
            float finalSpawn = group.StartDelaySeconds + (group.Count - 1) * group.IntervalSeconds;
            float killEstimate = group.Enemy.BaseHp / 50f;
            lastSpawn = Mathf.Max(lastSpawn, finalSpawn + killEstimate);
        }
        return lastSpawn;
    }
}
