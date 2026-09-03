using System.Collections.Generic;
using Godot;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Nations;

public sealed class NationBalanceReport
{
    public readonly List<string> Errors = new();
    public readonly Dictionary<string, float> DpsPerSupply = new();
    public bool IsValid => Errors.Count == 0;
}

public static class NationBalanceValidator
{
    public const float DefaultEnvelope = 0.15f;
    public const float DefaultParityTolerance = 0.03f;

    public static NationBalanceReport Validate(IReadOnlyList<NationProfile> profiles,
                                                IReadOnlyList<TowerDefinition> roster,
                                                float envelope = DefaultEnvelope,
                                                float parityTolerance = DefaultParityTolerance)
    {
        var report = new NationBalanceReport();
        if (profiles == null || profiles.Count != 6)
            report.Errors.Add($"Expected six nation profiles; found {profiles?.Count ?? 0}.");

        if (profiles == null) return report;
        foreach (var profile in profiles)
        {
            if (profile == null)
            {
                report.Errors.Add("Nation profile collection contains null.");
                continue;
            }
            ValidateProfile(profile, envelope, report.Errors);
            report.DpsPerSupply[profile.Id] = CalculateDpsPerSupply(profile, roster);
        }

        if (report.DpsPerSupply.Count > 0)
        {
            float mean = 0f;
            foreach (var value in report.DpsPerSupply.Values) mean += value;
            mean /= report.DpsPerSupply.Count;
            foreach (var pair in report.DpsPerSupply)
            {
                float deviation = mean > 0f ? Mathf.Abs(pair.Value / mean - 1f) : 0f;
                if (deviation > parityTolerance)
                    report.Errors.Add($"{pair.Key} DPS-per-Supply deviates {deviation:P1} from roster mean.");
            }
        }
        return report;
    }

    public static void ValidateProfile(NationProfile profile, float envelope, List<string> errors)
    {
        if (profile == null) { errors.Add("Nation profile is null."); return; }
        foreach (var lean in profile.StatLeans)
        {
            if (lean == null) { errors.Add($"{profile.Id} contains a null stat lean."); continue; }
            if (lean.Multiplier < 1f - envelope || lean.Multiplier > 1f + envelope)
                errors.Add($"{profile.Id}/{lean.Archetype}/{lean.StatId} is outside the ±{envelope:P0} envelope.");
            if (lean.Multiplier <= 0f)
                errors.Add($"{profile.Id}/{lean.StatId} must be positive.");
        }
    }

    public static float CalculateDpsPerSupply(NationProfile profile, IReadOnlyList<TowerDefinition> roster)
    {
        float total = 0f;
        if (roster == null) return total;
        foreach (var definition in roster)
        {
            if (definition == null) continue;
            var variant = NationStatApplicator.Apply(definition, profile);
            foreach (var stats in variant.Levels)
            {
                if (stats != null && stats.Cost > 0f)
                    total += stats.DamagePerShot * stats.RateOfFirePerSec / stats.Cost;
            }
        }
        return total;
    }
}
