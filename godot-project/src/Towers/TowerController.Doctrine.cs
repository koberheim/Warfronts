using Godot;
using FrontsOfWar.Combat;

namespace FrontsOfWar.Towers;

// Doctrine-facing surface of TowerController (GDD §19 prompt 39), split out
// of TowerController.cs to keep that file under the ~300-line guideline.
public partial class TowerController
{
    // Set each tick by DoctrineSystem's passive pass, the same
    // "recompute every tick, take the current value" pattern as the
    // Aura*/Signature* fields in TowerController.cs. DoctrineRangeBonusTiles
    // is additive (in tiles); everything else here is multiplicative.
    public float DoctrineDamageMultiplier = 1f;
    public float DoctrineRangeMultiplier = 1f;
    public float DoctrineRangeBonusTiles;
    public float DoctrineRateOfFireMultiplier = 1f;
    public float DoctrineStatusDurationMultiplier = 1f;
    public bool DoctrineSuppressionImmune;

    // A doctrine's force_target utility (GDD §8.2.4 Kampfgruppe's
    // Concentrated Fire) overrides normal target acquisition for a duration,
    // falling back to normal targeting once it expires or the forced target
    // dies/leaves range — see IsValidTarget in TowerController.cs.
    private ITargetable _forcedTarget;
    private float _forcedTargetRemaining;

    public void ForceTarget(ITargetable target, float durationSeconds)
    {
        _forcedTarget = target;
        _forcedTargetRemaining = durationSeconds;
    }

    // A doctrine's fire_all utility (GDD's Coordinated Fire / Counterattack).
    // Zeroing the cooldown and re-running SimTick with delta=0 reuses the
    // normal targeting/firing path exactly, including range and suppression
    // checks — a suppressed or empty-range tower correctly does nothing.
    public void ForceFire(int shots, SpatialGrid grid, ProjectileManager projectileManager)
    {
        for (int i = 0; i < Mathf.Max(1, shots); i++)
        {
            _cooldownRemaining = 0f;
            SimTick(0f, grid, projectileManager);
        }
    }
}
