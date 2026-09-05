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

    // B2 Armored Column Command's Convoy mechanic (GDD §10.3): the command
    // vehicle projects resistance/immunity to nearby escorts and, on death,
    // collapses them to a fraction of their HP. Reuses the same nearby-ally
    // aura pattern E9 Support/E10 Escort/E11 Recon already use via
    // EnemyController._enemyProvider - see EnemyControllerBoss.cs.
    [ExportGroup("Convoy (Boss B2)")]
    [Export] public float ConvoyAuraRadiusTiles;
    [Export] public float ConvoyDamageResistancePercent;
    [Export] public bool ConvoyGrantsSuppressionImmunity;
    [Export] public float ConvoyCollapseHpFraction = 1f;

    // B3 Bomber Wing's Formation mechanic (GDD §10.3): enemies sharing a
    // FormationGroupId get a damage reduction while every member (up to
    // FormationSize) is alive; each lost member applies an additional
    // multiplicative speed penalty to the survivors.
    [ExportGroup("Formation (Boss B3)")]
    [Export] public string FormationGroupId = "";
    [Export] public int FormationSize;
    [Export] public float FormationDamageReductionPercent;
    [Export] public float FormationBreakSpeedPenaltyPercent;

    // B4 Fortress Assault Group's multi-phase mechanic (GDD §10.3):
    // thresholds are fractions of max HP (e.g. [0.6, 0.25]) at which the
    // boss advances to the next one-way phase. Phase 2 (index 1) reuses the
    // Siege archetype's existing bombard fields/event below; phase 3
    // (index 2) grants suppression immunity and this speed multiplier.
    [ExportGroup("Multi-Phase (Boss B4)")]
    [Export] public float[] MultiPhaseHpThresholds = System.Array.Empty<float>();
    [Export] public float Phase3SpeedMultiplier = 1.5f;

    // Elite Medium Armor's Frontal Plate (GDD §10.2/§10.3: "the most
    // cuttable mechanic in the game" if playtesting shows it unreadable):
    // reduced damage from sources within a frontal cone of this enemy's
    // current heading, active only once it drops to ActivateHpFraction of
    // max HP (1 = active from the start, for a non-Elite unit that has it).
    [ExportGroup("Frontal Plate (Elite)")]
    [Export] public bool HasFrontalPlate;
    [Export] public float FrontalPlateActivateHpFraction = 1f;
    [Export] public float FrontalPlateDamageReductionPercent = 0.2f;
    [Export] public float FrontalPlateHalfAngleDegrees = 60f;

    // Deviation from GDD §15.3's schema (Claude decision, see
    // docs/DECISIONS.md): the full NationalSkin[] system is M4+ scope. Until
    // then, each EnemyDefinition points directly at the scene it spawns.
    [Export] public PackedScene ControllerScene;
}
