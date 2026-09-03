using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using System.Linq;

namespace FrontsOfWar.Towers;

// One placed tower (GDD §19 prompts 10–11, 14). Targeting/firing is driven
// by explicit SimTick calls from TowerManager, in lockstep with GameLoop's
// fixed tick — never by _PhysicsProcess (GDD §15.4). Level/branch/cost
// bookkeeping is delegated to TowerUpgradeController; the Supply
// transaction itself (checking/spending balance) is the caller's job — this
// class only tracks what upgrading or selling would cost/refund.
public partial class TowerController : Node2D
{
    [Export] public TowerDefinition Definition;

    public TargetingProfile CurrentTargeting { get; set; }
    public TowerUpgradeController Upgrade { get; private set; }

    // Set each tick by CommandPostManager before towers fire (GDD §7.5,
    // T9's aura); reset to 1 (no bonus) at the top of every tick. Multiple
    // Command Posts never stack — CommandPostManager keeps only the
    // strongest value it finds per tower, per §6 T9's "do not stack" rule.
    public float AuraRangeMultiplier = 1f;
    public float AuraRateOfFireMultiplier = 1f;

    // Enemy Siege units suppress towers on hit (GDD §5.6) — full disable,
    // never destroyed. Siege (E12) itself isn't implemented until M4, but
    // the hook exists now since the Emergency Repair ability (§7.6) already
    // needs something to clear.
    private float _suppressionRemaining;
    public bool IsSuppressed => _suppressionRemaining > 0f;

    // The Rally ability's temporary rate-of-fire buff (§7.6). Separate from
    // the Command Post aura multiplier above — a different source, and GDD
    // never says the two shouldn't stack.
    private float _rallyRemaining;
    private float _rallyRateOfFireMultiplier = 1f;

    private float _cooldownRemaining;
    private ITargetable _currentTarget;

    public override void _Ready()
    {
        CurrentTargeting = Definition.DefaultTargeting;
        Upgrade = new TowerUpgradeController(Definition, GameBalanceConfigAutoload.Config);
        SetupClickArea();
    }

    // Built in code rather than hand-added to each tower's .tscn (GDD §13.5:
    // "Opens on click, anchored to the tower") — every tower needs the same
    // click target, so this avoids repeating an Area2D/CollisionShape2D
    // pair across dozens of future tower scenes.
    private void SetupClickArea()
    {
        var area = new Area2D { InputPickable = true };
        var shape = new CollisionShape2D { Shape = new CircleShape2D { Radius = 18f } };
        area.AddChild(shape);
        AddChild(area);
        area.InputEvent += (viewport, @event, shapeIdx) =>
        {
            if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
                EventBus.Instance?.Publish(new TowerClickedEvent(this));
        };
    }

    public void SimTick(float tickDeltaSeconds, SpatialGrid grid, ProjectileManager projectileManager)
    {
        Upgrade.Tick(tickDeltaSeconds);
        if (_suppressionRemaining > 0f) _suppressionRemaining -= tickDeltaSeconds;
        if (_rallyRemaining > 0f) _rallyRemaining -= tickDeltaSeconds;
        if (IsSuppressed) return; // fully disabled — no targeting, no firing

        var stats = Upgrade.CurrentStats();
        float tilePixelSize = GameBalanceConfigAutoload.Config.TilePixelSize;
        float rangePixels = stats.RangeTiles * tilePixelSize * AuraRangeMultiplier;
        float minRangePixels = stats.MinRangeTiles * tilePixelSize;
        float rallyMultiplier = _rallyRemaining > 0f ? _rallyRateOfFireMultiplier : 1f;
        float rateOfFire = stats.RateOfFirePerSec * AuraRateOfFireMultiplier * rallyMultiplier;

        if (_cooldownRemaining > 0f) _cooldownRemaining -= tickDeltaSeconds;
        if (_cooldownRemaining > 0f) return;

        if (CurrentTargeting == TargetingProfile.DensestCluster)
        {
            FireAtDensestCluster(grid, rangePixels, minRangePixels, projectileManager, stats);
        }
        else
        {
            if (!IsValidTarget(_currentTarget, rangePixels, minRangePixels))
            {
                var candidates = grid.QueryRadius(GlobalPosition, rangePixels)
                    .Where(t => IsAcquirable(t) && IsValidTarget(t, rangePixels, minRangePixels));
                _currentTarget = TargetingService.SelectTarget(candidates, CurrentTargeting, GlobalPosition);
            }

            if (_currentTarget != null)
                Fire(_currentTarget, projectileManager, stats);
            else
                return;
        }

        _cooldownRemaining = 1f / Mathf.Max(0.01f, rateOfFire);
    }

    // Indirect-fire archetypes (T3 Field Mortar, T7 Heavy Artillery) target
    // a ground point rather than a single unit (GDD §6 T3, "Densest cluster"
    // priority).
    private void FireAtDensestCluster(SpatialGrid grid, float rangePixels, float minRangePixels,
                                       ProjectileManager projectileManager, TowerStatBlock stats)
    {
        var candidates = grid.QueryRadius(GlobalPosition, rangePixels)
            .Where(t => IsAcquirable(t) && IsValidTarget(t, rangePixels, minRangePixels))
            .ToList();
        if (candidates.Count == 0) return;

        float clusterRadiusPixels = Mathf.Max(stats.BlastRadiusTiles, 1f) * GameBalanceConfigAutoload.Config.TilePixelSize;
        var point = TargetingService.SelectDensestClusterPoint(candidates, clusterRadiusPixels);
        if (point == null) return;

        projectileManager.SpawnAtPoint(Definition, stats, GlobalPosition, point.Value);
        EventBus.Instance?.Publish(new TowerFiredEvent(this, null));
    }

    private bool IsAcquirable(ITargetable target)
    {
        if (Definition.AirOnly) return target.IsAir;
        if (Definition.GroundOnly) return !target.IsAir;
        return true;
    }

    private bool IsValidTarget(ITargetable target, float rangePixels, float minRangePixels)
    {
        if (target == null || !target.IsAlive) return false;
        float distSq = GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
        if (distSq > rangePixels * rangePixels) return false;
        if (minRangePixels > 0f && distSq < minRangePixels * minRangePixels) return false;
        return true;
    }

    private void Fire(ITargetable target, ProjectileManager projectileManager, TowerStatBlock stats)
    {
        if (Definition.ProjectileScene == null)
        {
            // Hitscan (T1 Automatic Gun): damage applies instantly.
            target.ApplyDamage(stats.DamagePerShot, Definition.DamageType);
        }
        else
        {
            projectileManager.Spawn(Definition, stats, GlobalPosition, target);
        }

        EventBus.Instance?.Publish(new TowerFiredEvent(this, target));
    }

    public void ApplySuppression(float durationSeconds) => _suppressionRemaining = Mathf.Max(_suppressionRemaining, durationSeconds);
    public void ClearSuppression() => _suppressionRemaining = 0f;

    public void ApplyRally(float durationSeconds, float rateOfFireMultiplier)
    {
        _rallyRemaining = Mathf.Max(_rallyRemaining, durationSeconds);
        _rallyRateOfFireMultiplier = rateOfFireMultiplier;
    }

    // Computes the refund and announces the sale; the caller (the tower
    // inspection panel) is responsible for crediting Supply, unregistering
    // this tower from TowerManager, and freeing the node — this method
    // doesn't have references to either.
    public int Sell()
    {
        int refund = Upgrade.SellRefund();
        EventBus.Instance?.Publish(new TowerSoldEvent(this, refund));
        return refund;
    }
}
