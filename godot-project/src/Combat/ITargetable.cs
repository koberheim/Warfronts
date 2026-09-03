using Godot;

namespace FrontsOfWar.Combat;

// What TargetingService needs from an enemy, without Combat depending on the
// Enemies namespace (GDD §15.1 principle 3 — a tower does not know what a
// wave is; by extension, targeting code does not need to know EnemyController).
public interface ITargetable
{
    Vector2 GlobalPosition { get; }

    // 0 at the path entry, 1 at the objective. Drives First/Last priority.
    float PathProgress { get; }

    float CurrentHp { get; }
    bool IsAir { get; }
    bool IsAlive { get; }
    Vector2 Velocity { get; }

    // Lets towers/projectiles deal damage without Combat depending on the
    // Enemies namespace concretely (GDD §15.1 principle 3). No isSpotted
    // parameter — Spotted is the target's own status (see StatusController),
    // not something the attacker decides per shot.
    void ApplyDamage(float baseDamage, DamageType type);
}
