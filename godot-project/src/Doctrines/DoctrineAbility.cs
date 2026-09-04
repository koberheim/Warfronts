using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Enemies;

namespace FrontsOfWar.Doctrines;

// One doctrine's fourth ability, parameterized data for one of the six
// shared DoctrineAbilityKind behaviors (GDD §7.6, §8.3, §19 prompt 39).
// Every field defaults to a neutral/no-op value; a given Kind only reads
// the subset of fields relevant to it (see DoctrineSystem's dispatch).
[GlobalClass]
public partial class DoctrineAbility : Resource
{
    [Export] public DoctrineAbilityKind Kind;
    [Export] public int CommandPointCost;
    [Export] public float CooldownSeconds = 20f;
    [Export] public bool OncePerMission;

    // PointBlast / LineBlast / StatusApplication.
    [Export] public float Damage;
    [Export] public DamageType DamageType = DamageType.Explosive;
    [Export] public float RadiusTiles;
    [Export] public float DurationSeconds;

    // LineBlast only.
    [Export] public DoctrineLineMode LineMode = DoctrineLineMode.DrawnLine;
    [Export] public float LengthTiles;
    [Export] public float WidthTiles;
    [Export] public float SweepSeconds;

    // AuraBuff only. RadiusTiles <= 0 (the default) means "every tower on
    // the map" — none of the 18 doctrines' buffs are radius-limited, so a
    // radius-gated AuraBuff, while structurally supported by the Kind, has
    // no authored example (see the implementation report).
    [Export] public float RateOfFireMultiplier = 1f;
    [Export] public float DamageMultiplier = 1f;
    [Export] public float RangeMultiplier = 1f;
    [Export] public bool SuppressionImmunity;
    [Export] public bool SingleTarget;

    // SpawnFriendly only.
    [Export] public FriendlyUnitDefinition FriendlyUnit;
    [Export] public PackedScene FriendlyUnitScene;
    [Export] public int FriendlyCount = 1;

    // StatusApplication only ("Suppressed" or "Spotted" — the game's only
    // two non-damage statuses, StatusController).
    [Export] public string StatusId = "";

    // InstantRefund/utility only — UtilityId is one of DoctrineUtilityId's
    // closed set. ArchetypeFilter (-1 = any) and SalvoCount serve fire_all;
    // DefenseLineBonus serves defense_line_bonus (paired with OncePerMission
    // above).
    [Export] public string UtilityId = "";
    [Export] public int ArchetypeFilter = -1;
    [Export] public int SalvoCount = 1;
    [Export] public int DefenseLineBonus;
}
