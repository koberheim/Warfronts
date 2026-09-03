using Godot;
using FrontsOfWar.Combat;

namespace FrontsOfWar.Enemies;

[GlobalClass]
public partial class FriendlyUnitDefinition : Resource
{
    [Export] public string Id;
    [Export] public string DisplayName;
    [Export] public float MaxHp;
    [Export] public float DamagePerSecond;
    [Export] public DamageType DamageType;
    [Export] public float MoveSpeedTilesPerSec;
    [Export] public float DodgeChance;
    [Export] public float LifetimeSeconds = 45f;
}
