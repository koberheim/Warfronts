using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using System.Linq;

namespace FrontsOfWar.Towers;

// One placed tower (GDD §19 prompts 10–11, 14). Targeting/firing is driven
// by explicit SimTick calls from TowerManager, in lockstep with GameLoop's
// fixed tick — never by _PhysicsProcess (GDD §15.4). Level/branch/cost
// bookkeeping is delegated to TowerUpgradeController; the Supply
// transaction itself (checking/spending balance) is the caller's job — this
// class only tracks what upgrading or selling would cost/refund.
public partial class TowerController : Node2D, IDamageSource, ISiegeTarget
{
    [Export] public TowerDefinition Definition;
    [Export] public PadTag PadTag = PadTag.Standard;

    // Copied from the placing BuildPad (GDD §11.1 M4 Ruined Town's "clipped
    // range arcs" - the one Enclosed-pad gimmick that isn't just a tag).
    // ArcHalfAngleDegrees >= 180 means "no clipping" (every other pad on
    // every other map). See GimmickRules.IsWithinArc.
    [Export] public float ArcFacingDegrees;
    [Export] public float ArcHalfAngleDegrees = 180f;

    // Set each tick by MapRuntime from GimmickSystem.GlobalRangeMultiplier
    // (GDD §11.1 M2 variant, Mission 10's Sandstorm) - reset to 1 (no-op)
    // whenever no sandstorm gimmick is active, same convention as
    // AuraRangeMultiplier below.
    public float GimmickRangeMultiplier = 1f;

    public TargetingProfile CurrentTargeting { get; set; }
    public TowerUpgradeController Upgrade { get; private set; }
    public float LifetimeDamage { get; private set; }
    public string SourceId => Name;
    public Vector2 SiegePosition => GlobalPosition;
    public bool IsSiegeImmune => PadTag == PadTag.Enclosed;

    // Set each tick by CommandPostManager before towers fire (GDD §7.5,
    // T9's aura); reset to 1 (no bonus) at the top of every tick. Multiple
    // Command Posts never stack — CommandPostManager keeps only the
    // strongest value it finds per tower, per §6 T9's "do not stack" rule.
    public float AuraRangeMultiplier = 1f;
    public float AuraRateOfFireMultiplier = 1f;
    public float SignatureRateOfFireMultiplier = 1f;
    public float SignatureProjectileVelocityMultiplier = 1f;
    public float SignatureTraverseSpeedMultiplier = 1f;
    public float SignatureIndirectDelayReductionSeconds;

    // Doctrine* fields (set each tick by DoctrineSystem's passive pass) and
    // the force_target/fire_all doctrine utility hooks live in
    // TowerController.Doctrine.cs — see that file.

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
    private float _secondaryCooldownRemaining;
    private ITargetable _currentTarget;
    private ulong _currentTargetGeneration;

    // The turret sprite (art doc sprite_generation_prompts.md §1.7 — a
    // separate rotating layer over the static emplacement). Null for
    // archetypes with no "Turret" child node (T8/T9, or any scene not yet
    // updated) — rotation is simply skipped for those.
    private Node2D _turret;
    // Indirect-fire archetypes (DensestCluster targeting) never set
    // _currentTarget, so the turret needs its own last-aimed-point to track.
    private Vector2? _turretAimPoint;

    // Read by the selection overlay (GDD §13.4: the selected tower shows
    // "its current target with a thin line"); null between engagements.
    public ITargetable CurrentTarget => _currentTarget;

    public override void _Ready()
    {
        CurrentTargeting = Definition.DefaultTargeting;
        Upgrade = new TowerUpgradeController(Definition, GameBalanceConfigAutoload.Config);
        _turret = GetNodeOrNull<Node2D>("Turret");
        SetupClickArea();
        EventBus.Instance?.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        EventBus.Instance?.Subscribe<EnemySiegeBombardEvent>(OnEnemySiegeBombard);
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        EventBus.Instance?.Unsubscribe<EnemySiegeBombardEvent>(OnEnemySiegeBombard);
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
        if (_forcedTargetRemaining > 0f) _forcedTargetRemaining -= tickDeltaSeconds;
        if (_currentTarget is EnemyController pooled && !pooled.IsPoolGenerationCurrent(_currentTargetGeneration)) _currentTarget = null;
        if (_forcedTarget is EnemyController forced && !forced.IsPoolGenerationCurrent(_forcedTargetGeneration)) _forcedTarget = null;
        if (IsSuppressed) return; // fully disabled — no targeting, no firing

        var stats = Upgrade.CurrentStats();
        UpdateTurretRotation(tickDeltaSeconds, stats.TurnRateSeconds);
        float tilePixelSize = GameBalanceConfigAutoload.Config.TilePixelSize;
        float effectiveRangeTiles = stats.RangeTiles * DoctrineRangeMultiplier + DoctrineRangeBonusTiles;
        float rangePixels = effectiveRangeTiles * tilePixelSize * AuraRangeMultiplier * GimmickRangeMultiplier;
        float minRangePixels = stats.MinRangeTiles * tilePixelSize;
        float rallyMultiplier = _rallyRemaining > 0f ? _rallyRateOfFireMultiplier : 1f;
        float rateOfFire = stats.RateOfFirePerSec * AuraRateOfFireMultiplier * SignatureRateOfFireMultiplier *
            rallyMultiplier * DoctrineRateOfFireMultiplier;

        FireSecondaryIfReady(tickDeltaSeconds, grid, stats);

        if (_cooldownRemaining > 0f) _cooldownRemaining -= tickDeltaSeconds;
        if (_cooldownRemaining > 0f) return;

        if (CurrentTargeting == TargetingProfile.DensestCluster)
        {
            FireAtDensestCluster(grid, rangePixels, minRangePixels, projectileManager, stats);
        }
        else
        {
            if (_forcedTargetRemaining > 0f && IsValidTarget(_forcedTarget, rangePixels, minRangePixels))
            {
                _currentTarget = _forcedTarget;
                _currentTargetGeneration = (_currentTarget as EnemyController)?.PoolGeneration ?? 0;
            }
            else if (!IsValidTarget(_currentTarget, rangePixels, minRangePixels))
            {
                var candidates = grid.QueryRadius(GlobalPosition, rangePixels)
                    .Where(t => IsAcquirable(t, stats) && IsValidTarget(t, rangePixels, minRangePixels));
                _currentTarget = TargetingService.SelectTarget(candidates, CurrentTargeting, GlobalPosition);
                _currentTargetGeneration = (_currentTarget as EnemyController)?.PoolGeneration ?? 0;
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
            .Where(t => IsAcquirable(t, stats) && IsValidTarget(t, rangePixels, minRangePixels))
            .ToList();
        if (candidates.Count == 0) return;

        float clusterRadiusPixels = Mathf.Max(stats.BlastRadiusTiles, 1f) * GameBalanceConfigAutoload.Config.TilePixelSize;
        var point = TargetingService.SelectDensestClusterPoint(candidates, clusterRadiusPixels);
        if (point == null) return;
        _turretAimPoint = point;

        int salvoCount = stats.SalvoCount < 1 ? 1 : stats.SalvoCount;
        for (int i = 0; i < salvoCount; i++)
            projectileManager.SpawnAtPoint(Definition, stats, GlobalPosition, point.Value, this);
        EventBus.Instance?.Publish(new TowerFiredEvent(this, null));
    }

    private bool IsAcquirable(ITargetable target, TowerStatBlock stats)
        => IsAcquirable(target, stats.TargetDomain, Definition);

    private bool IsAcquirable(ITargetable target, TargetDomain domain)
        => IsAcquirable(target, domain, Definition);

    private static bool IsAcquirable(ITargetable target, TargetDomain domain, TowerDefinition definition)
    {
        return domain switch
        {
            TargetDomain.Air => target.IsAir,
            TargetDomain.GroundAndAir => definition?.Archetype == TowerArchetype.FlakBattery,
            _ => !target.IsAir,
        };
    }

    private bool IsValidTarget(ITargetable target, float rangePixels, float minRangePixels)
    {
        if (target == null || !target.IsAlive) return false;
        if (target.IsConcealed && !target.IsRevealed) return false;
        float distSq = GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
        if (distSq > rangePixels * rangePixels) return false;
        if (minRangePixels > 0f && distSq < minRangePixels * minRangePixels) return false;
        if (ArcHalfAngleDegrees < 180f && !GimmickRules.IsWithinArc(GlobalPosition, ArcFacingDegrees, ArcHalfAngleDegrees, target.GlobalPosition))
            return false;
        return true;
    }

    // Swings the turret layer toward its target/aim-point, capped by
    // TurnRateSeconds — reused from its existing "time to acquire a new
    // target" meaning (GDD §6) as "time for a full 180 deg swing", so the
    // same balance number that gates targeting also gates the visual turn.
    private void UpdateTurretRotation(float tickDeltaSeconds, float turnRateSeconds)
    {
        if (_turret == null) return;
        Vector2? aimPoint = _currentTarget is { IsAlive: true } ? _currentTarget.GlobalPosition : _turretAimPoint;
        if (aimPoint == null) return;

        // Turret art is authored facing up/12 o'clock (sprite_generation_
        // prompts.md §1.1), not +X, so Vector2.Angle()'s atan2(y, x) needs a
        // +90 deg offset to match.
        float desiredRotation = (aimPoint.Value - GlobalPosition).Angle() + Mathf.Pi / 2f;
        float angleDiff = Mathf.Wrap(desiredRotation - _turret.Rotation, -Mathf.Pi, Mathf.Pi);
        float maxStep = (Mathf.Pi / Mathf.Max(0.01f, turnRateSeconds)) * tickDeltaSeconds;
        _turret.Rotation += Mathf.Clamp(angleDiff, -maxStep, maxStep);
    }

    private void Fire(ITargetable target, ProjectileManager projectileManager, TowerStatBlock stats)
    {
        float damage = stats.DamagePerShot * stats.DamageMultiplier * DoctrineDamageMultiplier;
        var damageType = stats.UsesDamageTypeOverride ? stats.DamageTypeOverride : Definition.DamageType;
        if (Definition.ProjectileScene == null)
        {
            // Hitscan (T1 Automatic Gun): damage applies instantly.
            if (target is EnemyController enemy)
                enemy.ApplyDamage(damage, damageType, this);
            else
                target.ApplyDamage(damage, damageType);
        }
        else
        {
            projectileManager.Spawn(Definition, stats, GlobalPosition, target, this);
        }

        if (stats.StatusEffectId == "Spotted" && target is EnemyController spotted)
            spotted.ApplySpotted(stats.StatusDurationSeconds * DoctrineStatusDurationMultiplier);

        EventBus.Instance?.Publish(new TowerFiredEvent(this, target));
    }

    private void FireSecondaryIfReady(float delta, SpatialGrid grid, TowerStatBlock stats)
    {
        if (stats.SecondaryDamagePerShot <= 0f || stats.SecondaryRateOfFirePerSec <= 0f) return;
        _secondaryCooldownRemaining -= delta;
        if (_secondaryCooldownRemaining > 0f) return;
        float rangeTiles = stats.SecondaryRangeTiles > 0f ? stats.SecondaryRangeTiles : stats.RangeTiles;
        float rangePixels = rangeTiles * GameBalanceConfigAutoload.Config.TilePixelSize * AuraRangeMultiplier * GimmickRangeMultiplier;
        var candidates = grid.QueryRadius(GlobalPosition, rangePixels)
            .Where(candidate => IsAcquirable(candidate, stats.SecondaryTargetDomain) &&
                IsValidTarget(candidate, rangePixels, 0f));
        var target = TargetingService.SelectTarget(candidates, stats.SecondaryTargeting, GlobalPosition);
        if (target == null) return;
        float damage = stats.SecondaryDamagePerShot * stats.DamageMultiplier * DoctrineDamageMultiplier;
        if (target is EnemyController enemy)
        {
            enemy.ApplyDamage(damage, stats.SecondaryDamageType, this);
            if (stats.StatusEffectId == "Suppressed")
            {
                float duration = stats.StatusDurationSeconds * DoctrineStatusDurationMultiplier;
                enemy.ApplySuppressed(duration, duration);
            }
        }
        else target.ApplyDamage(damage, stats.SecondaryDamageType);
        _secondaryCooldownRemaining = 1f / stats.SecondaryRateOfFirePerSec;
        EventBus.Instance?.Publish(new TowerFiredEvent(this, target));
    }

    public void ApplySuppression(float durationSeconds)
    {
        if (DoctrineSuppressionImmune) return;
        _suppressionRemaining = Mathf.Max(_suppressionRemaining, durationSeconds);
    }
    public void ClearSuppression() => _suppressionRemaining = 0f;

    public void ApplyRally(float durationSeconds, float rateOfFireMultiplier)
    {
        _rallyRemaining = Mathf.Max(_rallyRemaining, durationSeconds);
        _rallyRateOfFireMultiplier = rateOfFireMultiplier;
    }

    public override void _Draw()
    {
        if (SignatureRateOfFireMultiplier <= 1.01f) return;
        var chevron = new[]
        {
            new Vector2(-5f, -25f), new Vector2(0f, -20f), new Vector2(5f, -25f),
            new Vector2(3f, -27f), new Vector2(0f, -23f), new Vector2(-3f, -27f),
        };
        DrawColoredPolygon(chevron, new Color(1f, 0.68f, 0.2f));
    }

    private void OnEnemyDamaged(EnemyDamagedEvent evt)
    {
        if (ReferenceEquals(evt.Source, this)) LifetimeDamage += evt.DamageDealt;
    }

    private void OnEnemySiegeBombard(EnemySiegeBombardEvent evt)
    {
        float tileSize = GameBalanceConfigAutoload.Config.TilePixelSize;
        float distanceTiles = GlobalPosition.DistanceTo(evt.Position) / tileSize;
        if (SiegeRules.ShouldSuppress(PadTag, distanceTiles, evt.RangeTiles))
            ApplySuppression(evt.DurationSeconds);
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
