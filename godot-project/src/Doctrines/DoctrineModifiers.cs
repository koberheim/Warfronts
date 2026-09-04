using FrontsOfWar.Core;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;
using System.Collections.Generic;

namespace FrontsOfWar.Doctrines;

// Resolves one doctrine's passive into the multipliers the rest of the game
// consults (GDD §19 prompt 39). Called once per SimTick from DoctrineSystem,
// the same "recompute every tick, like Command Post auras" pattern
// CommandPostManager already uses — cheap at the tower/post/field counts
// this game runs (PerformanceBudgetTowers = 40).
public static class DoctrineModifiers
{
    // Gates the shared per-tower multiplier bundle (cost, damage, range,
    // rate of fire, status duration). TerrainTagFilter never matches today —
    // maps carry no terrain tag data — so a doctrine that sets it (Desert
    // Rats) is deliberately inert until that data exists.
    public static bool Matches(DoctrinePassive passive, TowerArchetype archetype, PadTag padTag)
    {
        if (passive == null) return false;
        if (!string.IsNullOrEmpty(passive.TerrainTagFilter)) return false;
        if (passive.ArchetypeFilter >= 0 && passive.ArchetypeFilter != (int)archetype) return false;
        if (passive.PadTagFilter >= 0 && passive.PadTagFilter != (int)padTag) return false;
        return true;
    }

    public static float PlacementCostMultiplier(DoctrinePassive passive, TowerArchetype archetype, PadTag padTag)
        => Matches(passive, archetype, padTag) ? passive.TowerCostMultiplier : 1f;

    public static void ApplyToTowers(DoctrinePassive passive, DoctrineAbility ability, bool abilityBuffActive,
        TowerController abilityBuffSingleTarget, TowerManager towers, GameBalanceConfig config)
    {
        if (passive == null) return;
        var list = towers.Towers;
        foreach (var tower in list)
        {
            float damage = 1f, range = 1f, rof = 1f, upgradeCost = 1f, statusDuration = 1f;
            float rangeBonusTiles = 0f;
            bool suppressionImmune = false;

            if (Matches(passive, tower.Definition.Archetype, tower.PadTag))
            {
                damage *= passive.DamageMultiplier;
                range *= passive.RangeMultiplier;
                rangeBonusTiles += passive.RangeBonusTiles;
                rof *= passive.RateOfFireMultiplier;
                upgradeCost *= passive.UpgradeCostMultiplier;
                statusDuration *= passive.StatusDurationMultiplier;
            }

            if (passive.NearDifferentArchetypeTiles > 0f &&
                HasDifferentArchetypeNearby(tower, list, passive.NearDifferentArchetypeTiles, config))
                damage *= passive.NearDifferentArchetypeDamageMultiplier;

            if (abilityBuffActive && ability != null && ability.Kind == DoctrineAbilityKind.AuraBuff &&
                MatchesAuraTarget(ability, tower, abilityBuffSingleTarget))
            {
                damage *= ability.DamageMultiplier;
                range *= ability.RangeMultiplier;
                rof *= ability.RateOfFireMultiplier;
                if (ability.SuppressionImmunity) suppressionImmune = true;
            }

            tower.DoctrineDamageMultiplier = damage;
            tower.DoctrineRangeMultiplier = range;
            tower.DoctrineRangeBonusTiles = rangeBonusTiles;
            tower.DoctrineRateOfFireMultiplier = rof;
            tower.DoctrineStatusDurationMultiplier = statusDuration;
            tower.DoctrineSuppressionImmune = suppressionImmune;
            tower.Upgrade.DoctrineCostMultiplier = upgradeCost;
        }
    }

    public static void ApplyToCommandPosts(DoctrinePassive passive, CommandPostManager commandPosts)
    {
        float multiplier = passive?.CommandPostAuraRadiusMultiplier ?? 1f;
        foreach (var post in commandPosts.Posts) post.DoctrineAuraRadiusMultiplier = multiplier;
    }

    public static void ApplyToMinefields(DoctrinePassive passive, MinefieldManager minefields)
    {
        int capBonus = (passive?.MinefieldCapBonus ?? 0) + (passive?.MinefieldExtraCharges ?? 0);
        float damageMultiplier = passive?.MinefieldDamageMultiplier ?? 1f;
        foreach (var field in minefields.Fields)
        {
            field.DoctrineCapBonus = capBonus;
            field.DoctrineDamageMultiplier = damageMultiplier;
        }
    }

    public static void ApplyToSignatures(DoctrinePassive passive, SignatureManager signatures)
    {
        float multiplier = passive?.SignatureRegenMultiplier ?? 1f;
        foreach (var signature in signatures.Signatures) signature.DoctrineRegenMultiplier = multiplier;
    }

    private static bool MatchesAuraTarget(DoctrineAbility ability, TowerController tower, TowerController singleTarget)
        => !ability.SingleTarget || ReferenceEquals(tower, singleTarget);

    private static bool HasDifferentArchetypeNearby(TowerController tower, IReadOnlyList<TowerController> towers,
        float radiusTiles, GameBalanceConfig config)
    {
        float radiusPixels = radiusTiles * config.TilePixelSize;
        float radiusSquared = radiusPixels * radiusPixels;
        foreach (var other in towers)
        {
            if (ReferenceEquals(other, tower) || other.Definition.Archetype == tower.Definition.Archetype) continue;
            if (tower.GlobalPosition.DistanceSquaredTo(other.GlobalPosition) <= radiusSquared) return true;
        }
        return false;
    }
}
