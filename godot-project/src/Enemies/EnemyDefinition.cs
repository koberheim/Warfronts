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
    [Export] public EnemyArchetype Archetype;
    [Export] public ArmorClass ArmorClass;
    [Export] public bool IsAir;
    [Export] public float BaseHp;
    [Export] public float MoveSpeedTilesPerSec;
    [Export] public int LeakCost;
    [Export] public int Bounty;

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
