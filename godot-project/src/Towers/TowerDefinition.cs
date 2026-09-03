using Godot;
using FrontsOfWar.Combat;

namespace FrontsOfWar.Towers;

// Data for one tower (GDD §15.3). Trimmed to what M1–M2 need: no NationId
// or Visuals yet (those land with the nation system at M4). A tower with a
// null ProjectileScene fires as hitscan (T1); otherwise ProjectileSystem
// spawns a pooled, leading projectile (T4). Level progression and branch
// selection live in TowerUpgradeController, not here.
[GlobalClass]
public partial class TowerDefinition : Resource
{
    [Export] public string Id;
    [Export] public string BehaviorId = "";
    [Export] public TowerArchetype Archetype;
    [Export] public string DisplayName;
    [Export] public DamageType DamageType;
    [Export] public TargetingProfile DefaultTargeting = TargetingProfile.First;
    // L1 and L2 (index 0, 1) — shared by both branches, per GDD §6's
    // "Level 1 → 2 → [branch fork] → 3 → 4."
    [Export] public TowerStatBlock[] Levels = System.Array.Empty<TowerStatBlock>();
    [Export] public TowerBranch BranchA;
    [Export] public TowerBranch BranchB;
    [Export] public PackedScene ProjectileScene;
    [Export] public float ProjectileSpeedTilesPerSec;
    [Export] public bool GroundOnly = true;
    [Export] public bool AirOnly;

    // Covers only L1–L2 (pre-fork). L3–L4 come from whichever TowerBranch
    // was chosen — see TowerUpgradeController.CurrentStats().
    public TowerStatBlock PreForkStatsForLevel(int level) => Levels[Mathf.Clamp(level - 1, 0, Levels.Length - 1)];
}
