using Godot;

namespace FrontsOfWar.Doctrines;

// A single neutral-by-default passive schema shared by all 18 doctrines
// (GDD §8.3, §19 prompt 39). Every field defaults to a no-op value so an
// unauthored field never silently changes behavior. ArchetypeFilter and
// PadTagFilter gate the per-tower multiplier bundle below (DamageMultiplier,
// RangeMultiplier, RangeBonusTiles, RateOfFireMultiplier, StatusDuration
// Multiplier, UpgradeCostMultiplier, and placement's TowerCostMultiplier);
// -1 on either means "any". TerrainTagFilter is authored but never matches
// today — maps carry no terrain tags yet (see the implementation report).
[GlobalClass]
public partial class DoctrinePassive : Resource
{
    [Export] public int ArchetypeFilter = -1;
    [Export] public int PadTagFilter = -1;
    [Export] public string TerrainTagFilter = "";

    [Export] public float TowerCostMultiplier = 1f;
    [Export] public float UpgradeCostMultiplier = 1f;
    [Export] public float DamageMultiplier = 1f;
    [Export] public float RangeMultiplier = 1f;
    [Export] public float RangeBonusTiles;
    [Export] public float RateOfFireMultiplier = 1f;
    [Export] public float StatusDurationMultiplier = 1f;

    // Global/mission-wide effects — not gated by ArchetypeFilter/PadTagFilter.
    [Export] public float SupplyIncomeMultiplier = 1f;
    [Export] public int DefenseLineBonus;
    [Export] public float CommandPostAuraRadiusMultiplier = 1f;
    [Export] public float SignatureRegenMultiplier = 1f;
    [Export] public int MinefieldExtraCharges;
    [Export] public int MinefieldCapBonus;
    [Export] public float MinefieldDamageMultiplier = 1f;

    // Combined Arms' own independent pair — proximity to a different
    // archetype, not gated by the filters above.
    [Export] public float NearDifferentArchetypeTiles;
    [Export] public float NearDifferentArchetypeDamageMultiplier = 1f;

    // Authored for Fortified Line (Japan) but inert — extending suppression
    // immunity from Enclosed towers to nearby non-Enclosed ones has no
    // per-tower neighbor-awareness hook today.
    [Export] public float SuppressionImmunityRadiusBonusTiles;

    // Authored for Celere (Italy) but inert — the GDD §8.2.5 national
    // relocation mechanic (sell-and-rebuild at 20% cost) itself isn't built
    // anywhere yet, only the Redeploy ability's own TryRelocate is.
    [Export] public bool RelocationFree;
}
