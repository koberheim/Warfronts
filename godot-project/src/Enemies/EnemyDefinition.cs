using Godot;
using FrontsOfWar.Combat;

namespace FrontsOfWar.Enemies;

// Data for one enemy archetype (GDD §15.3, §10.2). SpecialAbility and Skins
// are deferred past M1 — only the stats needed to move, take damage, and
// leak are here so far; national skins and special mechanics land with
// their respective milestones (M4/M5 per GDD §19).
[GlobalClass]
public partial class EnemyDefinition : Resource
{
    [Export] public string Id;
    [Export] public string DisplayName = "";
    [Export] public EnemyArchetype Archetype;
    [Export] public ArmorClass ArmorClass;
    [Export] public bool IsAir;
    [Export] public float BaseHp;
    [Export] public float MoveSpeedTilesPerSec;
    [Export] public int LeakCost;
    [Export] public int Bounty;

    [ExportGroup("Special Ability")]
    [Export] public string SpecialAbilityId = "";
    [Export] public int CohesionGroupSize = 1;
    [Export] public float CohesionCatchupSpeedMultiplier = 1f;
    [Export] public float CohesionCatchupThreshold = 0.05f;
    [Export] public bool SuppressionImmune;
    [Export] public float SiegeBombardRangeTiles = 11f;
    [Export] public float SiegeBombardIntervalSeconds = 8f;
    [Export] public float SiegeSuppressionDurationSeconds = 6f;

    [ExportGroup("Support / Air")]
    [Export] public float SupportRepairPercentMaxHpPerSecond = 0.04f;
    [Export] public float SupportRepairRangeTiles = 4f;
    [Export] public float EscortShieldMaxHp = 400f;
    [Export] public float EscortShieldRadiusTiles = 3.5f;
    [Export] public float ReconSpeedBonusMultiplier = 1.15f;
    [Export] public float ReconAuraRadiusTiles = 4f;

    [ExportGroup("Boss Phase")]
    [Export] public bool IsBoss;
    [Export] public float SkirtHp;
    [Export] public float SkirtDamageReduction = 0.5f;
    [Export] public float SkirtExplosiveMultiplier = 3f;
    [Export] public float PostSkirtSpeedMultiplier = 1.3f;
    [Export] public EnemyDefinition AddDefinition;
    [Export] public int AddCount = 2;
    [Export] public float AddIntervalSeconds = 12f;

    // Deviation from GDD §15.3's schema (Claude decision, see
    // docs/DECISIONS.md): the full NationalSkin[] system is M4+ scope. Until
    // then, each EnemyDefinition points directly at the scene it spawns.
    [Export] public PackedScene ControllerScene;
}
