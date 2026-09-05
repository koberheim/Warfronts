using Godot;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Nations;

public static class NationStatApplicator
{
    public static TowerDefinition Apply(TowerDefinition source, NationProfile profile)
    {
        var result = new TowerDefinition
        {
            Id = source.Id,
            BehaviorId = source.BehaviorId,
            Archetype = source.Archetype,
            DisplayName = source.DisplayName,
            DamageType = source.DamageType,
            DefaultTargeting = source.DefaultTargeting,
            ProjectileScene = source.ProjectileScene,
            ControllerScene = source.ControllerScene,
            ProjectileSpeedTilesPerSec = source.ProjectileSpeedTilesPerSec,
            GroundOnly = source.GroundOnly,
            AirOnly = source.AirOnly,
            BranchA = CopyBranch(source.BranchA),
            BranchB = CopyBranch(source.BranchB),
        };

        result.Levels = CopyStats(source.Levels, source.Archetype, profile);
        if (result.BranchA != null)
            result.BranchA.Levels = CopyStats(result.BranchA.Levels, source.Archetype, profile);
        if (result.BranchB != null)
            result.BranchB.Levels = CopyStats(result.BranchB.Levels, source.Archetype, profile);
        return result;
    }

    private static TowerBranch CopyBranch(TowerBranch source)
    {
        if (source == null) return null;
        return new TowerBranch
        {
            Name = source.Name,
            BehaviorId = source.BehaviorId,
            Levels = source.Levels,
        };
    }

    private static TowerStatBlock[] CopyStats(TowerStatBlock[] source, TowerArchetype archetype, NationProfile profile)
    {
        var result = new TowerStatBlock[source?.Length ?? 0];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = CopyStatBlock(source[i]);
            foreach (var lean in profile?.StatLeans ?? System.Array.Empty<NationStatLean>())
            {
                if (lean != null && lean.Archetype == archetype)
                    ApplyLean(result[i], lean.StatId, lean.Multiplier);
            }
        }
        return result;
    }

    private static TowerStatBlock CopyStatBlock(TowerStatBlock source)
    {
        return new TowerStatBlock
        {
            Cost = source.Cost,
            DamagePerShot = source.DamagePerShot,
            RateOfFirePerSec = source.RateOfFirePerSec,
            RangeTiles = source.RangeTiles,
            MinRangeTiles = source.MinRangeTiles,
            BlastRadiusTiles = source.BlastRadiusTiles,
            TurnRateSeconds = source.TurnRateSeconds,
            TargetDomain = source.TargetDomain,
            SecondaryTargetDomain = source.SecondaryTargetDomain,
            DamageMultiplier = source.DamageMultiplier,
            SecondaryDamagePerShot = source.SecondaryDamagePerShot,
            SecondaryRateOfFirePerSec = source.SecondaryRateOfFirePerSec,
            SecondaryDamageType = source.SecondaryDamageType,
            SecondaryTargeting = source.SecondaryTargeting,
            SecondaryBlastRadiusTiles = source.SecondaryBlastRadiusTiles,
            SecondaryRangeTiles = source.SecondaryRangeTiles,
            SalvoCount = source.SalvoCount,
            FireDelaySeconds = source.FireDelaySeconds,
            MaxCharges = source.MaxCharges,
            ChargeRegenSeconds = source.ChargeRegenSeconds,
            TriggerRadiusTiles = source.TriggerRadiusTiles,
            TriggerArmingSeconds = source.TriggerArmingSeconds,
            StatusEffectId = source.StatusEffectId,
            StatusDurationSeconds = source.StatusDurationSeconds,
            UsesDamageTypeOverride = source.UsesDamageTypeOverride,
            DamageTypeOverride = source.DamageTypeOverride,
            AuraRadiusTiles = source.AuraRadiusTiles,
            AuraRangeBonusPercent = source.AuraRangeBonusPercent,
            AuraRateOfFireBonusPercent = source.AuraRateOfFireBonusPercent,
            CommandPointsPerWave = source.CommandPointsPerWave,
            SupplyPerWave = source.SupplyPerWave,
        };
    }

    public static bool ApplyLean(TowerStatBlock stats, string statId, float multiplier)
    {
        if (stats == null || string.IsNullOrWhiteSpace(statId)) return false;
        switch (statId.Trim().ToLowerInvariant())
        {
            case "cost": stats.Cost = Mathf.Max(1, Mathf.RoundToInt(stats.Cost * multiplier)); return true;
            case "damage":
            case "damagepershot": stats.DamagePerShot *= multiplier; return true;
            case "rate":
            case "rateoffirepersec": stats.RateOfFirePerSec *= multiplier; return true;
            case "range":
            case "rangetiles": stats.RangeTiles *= multiplier; return true;
            case "minrange":
            case "minrangetiles": stats.MinRangeTiles *= multiplier; return true;
            case "blastradius":
            case "blastradiustiles": stats.BlastRadiusTiles *= multiplier; return true;
            case "turnrate":
            case "turnrateseconds": stats.TurnRateSeconds *= multiplier; return true;
            case "firedelay":
            case "firedelayseconds": stats.FireDelaySeconds *= multiplier; return true;
            case "chargeregen":
            case "chargeregenseconds": stats.ChargeRegenSeconds *= multiplier; return true;
            case "triggeradius":
            case "triggerradiustiles": stats.TriggerRadiusTiles *= multiplier; return true;
            case "auraradius":
            case "auraradiustiles": stats.AuraRadiusTiles *= multiplier; return true;
            case "aurarangebonus": stats.AuraRangeBonusPercent *= multiplier; return true;
            case "auraratebonus": stats.AuraRateOfFireBonusPercent *= multiplier; return true;
            default: return false;
        }
    }
}
