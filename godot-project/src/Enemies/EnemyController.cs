using Godot;
using FrontsOfWar.UI.Theme;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using System;
using System.Collections.Generic;

namespace FrontsOfWar.Enemies;

// One enemy instance (GDD §19 prompt 8). Movement and damage are driven by
// explicit SimTick calls from EnemyManager, not Godot's _PhysicsProcess —
// see GameLoop's fixed-tick model (§15.4): every enemy must advance exactly
// the same amount regardless of render framerate or game speed.
public partial class EnemyController : Node2D, ITargetable, IDamageReceiver, IPoolLifecycle
{
    [Export] public EnemyDefinition Definition;

    private PathFollower _pathFollower;
    private float _currentHp;
    private float _maxHp;
    private float _hpScaleMultiplier = 1f;
    private Vector2 _previousPosition;
    private bool _softBlocked;
    private float _cohesionLeadProgress;
    private float _siegeBombardRemaining;
    private CanvasItem _bossSkirtVisual;
    private AirCorridorDefinition _airCorridor;
    private float _airProgress;
    private float _shieldRemaining;
    private bool _revealed;
    private bool _inCanopy;
    private float _mudSpeedMultiplier = 1f;
    private Func<IReadOnlyList<EnemyController>> _enemyProvider;
    private EnemyController _repairTarget;

    public BossPhaseController BossPhase { get; private set; }
    public MultiPhaseBossController MultiPhaseBoss { get; private set; }
    public PathNetwork PathNetwork { get; private set; }

    public StatusController Status { get; private set; } = new();

    public float CurrentHp => _currentHp;
    public bool IsAlive => _currentHp > 0f;
    public bool IsAir => Definition?.IsAir == true;
    // Canopy (GDD §11.1 M6 Snowy Forest Pass): a path-level gimmick, not a
    // per-enemy trait like E11 Recon's, but the same reveal machinery below
    // (IsRevealed, SetRevealed, Status.IsSpotted) applies to both sources
    // uniformly - "reuses E11's system entirely" per GDD §11.2.
    public bool IsConcealed => Definition?.SpecialAbilityId == "recon_concealment" || _inCanopy;
    public bool IsRevealed => !IsConcealed || _revealed || Status.IsSpotted;
    public float PathProgress => IsAir && _airCorridor != null ? _airProgress : _pathFollower?.Progress ?? 0f;
    public float PathDistancePixels => IsAir && _airCorridor != null
        ? _airProgress * _airCorridor.LengthPixels : _pathFollower?.DistanceTraveled ?? 0f;
    public bool ReachedEnd => IsAir && _airCorridor != null ? _airProgress >= 1f : _pathFollower?.ReachedEnd ?? false;
    public Vector2 Velocity { get; private set; }
    public float MaxHp => _maxHp;
    public float ShieldRemaining => _shieldRemaining;
    public EnemyController RepairTarget => _repairTarget;
    // Every Initialize call starts a distinct lease of this pooled node. Any
    // system that retains an EnemyController across ticks (notably a direct-
    // fire projectile) must capture this value and reject a later generation.
    public ulong PoolGeneration { get; private set; }

    public bool IsPoolGenerationCurrent(ulong generation)
        => generation == PoolGeneration && IsAlive;

    public void Initialize(PathNetwork path, float hpScaleMultiplier = 1f, AirCorridorDefinition airCorridor = null)
    {
        PoolGeneration++;
        Status.Reset();
        PathNetwork = path;
        _pathFollower = new PathFollower(path);
        _hpScaleMultiplier = hpScaleMultiplier;
        _maxHp = Definition.BaseHp * hpScaleMultiplier;
        _currentHp = _maxHp;
        _airCorridor = airCorridor;
        if (Definition.IsAir && _airCorridor == null)
            _airCorridor = new AirCorridorDefinition
            {
                EntryPosition = path.GetPositionAtDistance(0f),
                ObjectivePosition = path.GetPositionAtDistance(path.LengthPixels),
            };
        _airProgress = 0f;
        _shieldRemaining = Definition.Archetype == EnemyArchetype.Escort ? Definition.EscortShieldMaxHp : 0f;
        _revealed = false;
        _inCanopy = false;
        _mudSpeedMultiplier = 1f;
        _softBlocked = false;
        _previousPosition = Vector2.Zero;
        Velocity = Vector2.Zero;
        _enemyProvider = null;
        _repairTarget = null;
        bool isMultiPhase = (Definition.MultiPhaseHpThresholds?.Length ?? 0) > 0;
        BossPhase = Definition.IsBoss && !isMultiPhase ? new BossPhaseController(Definition) : null;
        MultiPhaseBoss = Definition.IsBoss && isMultiPhase ? new MultiPhaseBossController(Definition) : null;
        _cohesionLeadProgress = 0f;
        _siegeBombardRemaining = Definition.SpecialAbilityId == "siege_bombard"
            ? Definition.SiegeBombardIntervalSeconds : 0f;
        _bossSkirtVisual = GetNodeOrNull<CanvasItem>("Skirt");
        if (_bossSkirtVisual != null) _bossSkirtVisual.Visible = true;
        GlobalPosition = Definition.IsAir && _airCorridor != null
            ? _airCorridor.EntryPosition : path.GetPositionAtDistance(0f);
        QueueRedraw();
    }

    public void OnRentedFromPool()
    {
        // Initialize supplies the definition/path and performs the full reset.
        // This hook intentionally only establishes an inert pre-initialize
        // state so a rented node can never expose data from its prior lease.
        ResetForPool(clearDefinition: false);
    }

    public void OnReturnedToPool() => ResetForPool(clearDefinition: true);

    private void ResetForPool(bool clearDefinition)
    {
        _pathFollower = null;
        PathNetwork = null;
        _currentHp = 0f;
        _maxHp = 0f;
        _hpScaleMultiplier = 1f;
        _previousPosition = Vector2.Zero;
        _softBlocked = false;
        _cohesionLeadProgress = 0f;
        _siegeBombardRemaining = 0f;
        _airCorridor = null;
        _airProgress = 0f;
        _shieldRemaining = 0f;
        _revealed = false;
        _inCanopy = false;
        _mudSpeedMultiplier = 1f;
        _enemyProvider = null;
        _repairTarget = null;
        Velocity = Vector2.Zero;
        BossPhase = null;
        MultiPhaseBoss = null;
        Status.Reset();
        if (_bossSkirtVisual != null) _bossSkirtVisual.Visible = true;
        if (clearDefinition) Definition = null;
        Position = Vector2.Zero;
        QueueRedraw();
    }

    public void SetEnemyProvider(Func<IReadOnlyList<EnemyController>> provider) => _enemyProvider = provider;

    public void SimTick(float tickDeltaSeconds)
    {
        if (_pathFollower == null || !IsAlive) return;

        Status.Tick(tickDeltaSeconds);
        BossPhase?.Tick(tickDeltaSeconds);
        if (MultiPhaseBoss != null)
        {
            MultiPhaseBoss.UpdatePhase(_currentHp, _maxHp);
            MultiPhaseBoss.Tick(tickDeltaSeconds);
            if (MultiPhaseBoss.ConsumeBombardReady(out float bombardRange, out float bombardDuration))
                EventBus.Instance?.Publish(new EnemySiegeBombardEvent(this, GlobalPosition, bombardRange, bombardDuration));
        }

        if (Definition.Archetype == EnemyArchetype.Support) TickFieldRepair(tickDeltaSeconds);
        if (Definition.IsAir && _airCorridor != null)
        {
            // B3 Bomber Wing (GDD §10.3): "each destroyed bomber... slows the
            // survivors by 20%" needs to apply here too - air units skip the
            // ground movement branch below entirely, so FormationState()
            // must be consulted on both paths.
            float airSpeedMultiplier = FormationState().speedMultiplier;
            _previousPosition = GlobalPosition;
            float distance = Mathf.Max(1f, _airCorridor.LengthPixels);
            _airProgress = Mathf.Min(1f, _airProgress + Definition.MoveSpeedTilesPerSec * airSpeedMultiplier *
                GameBalanceConfigAutoload.Config.TilePixelSize * tickDeltaSeconds / distance);
            GlobalPosition = _airCorridor.EntryPosition.Lerp(_airCorridor.ObjectivePosition, _airProgress);
            Velocity = tickDeltaSeconds > 0f ? (GlobalPosition - _previousPosition) / tickDeltaSeconds : Vector2.Zero;
            QueueRedraw();
            return;
        }

        if (Definition.SpecialAbilityId == "siege_bombard")
        {
            _siegeBombardRemaining -= tickDeltaSeconds;
            if (_siegeBombardRemaining <= 0f)
            {
                EventBus.Instance?.Publish(new EnemySiegeBombardEvent(this, GlobalPosition,
                    Definition.SiegeBombardRangeTiles, Definition.SiegeSuppressionDurationSeconds));
                _siegeBombardRemaining = Definition.SiegeBombardIntervalSeconds;
            }
        }

        _previousPosition = GlobalPosition;
        var config = GameBalanceConfigAutoload.Config;
        float speedMultiplier = Status.IsSuppressed ? config.SuppressedMoveSpeedMultiplier : 1f;
        speedMultiplier *= NearbyReconSpeedMultiplier();
        speedMultiplier *= _mudSpeedMultiplier;
        speedMultiplier *= FormationState().speedMultiplier;
        if (Definition.SpecialAbilityId == "swarm_cohesion" &&
            _cohesionLeadProgress - PathProgress >= Definition.CohesionCatchupThreshold)
            speedMultiplier *= Definition.CohesionCatchupSpeedMultiplier;
        if (!_softBlocked && MultiPhaseBoss?.IsHalted != true)
            _pathFollower.Advance(Definition.MoveSpeedTilesPerSec * (BossPhase?.SpeedMultiplier ?? MultiPhaseBoss?.SpeedMultiplier ?? 1f),
                speedMultiplier, tickDeltaSeconds, config.TilePixelSize);
        GlobalPosition = _pathFollower.CurrentPosition;
        Velocity = tickDeltaSeconds > 0f ? (GlobalPosition - _previousPosition) / tickDeltaSeconds : Vector2.Zero;

        QueueRedraw();
    }

    // Applies one damage instance and publishes the outcome. Whether this
    // hit counts as "Spotted" (+25% damage, GDD §5.5) is this enemy's own
    // status, not something the attacker passes in.
    public void ApplyDamage(float baseDamage, DamageType type)
        => ApplyDamage(baseDamage, type, null);

    public void ApplyDamage(float baseDamage, DamageType type, IDamageSource source)
    {
        if (!IsAlive || Definition == null) return;

        float multiplier = DamageTable.Default.Multiplier(type, Definition.ArmorClass);
        float dealt = BossPhase?.ResolveDamage(baseDamage, type, Status.IsSpotted)
            ?? DamageResolver.ResolveDamage(baseDamage, type, Definition.ArmorClass, Status.IsSpotted, DamageTable.Default);
        if (BossPhase is { IsSkirtIntact: false } && _bossSkirtVisual != null)
            _bossSkirtVisual.Visible = false;
        dealt = AbsorbShieldedDamage(dealt);
        dealt *= ConvoyDamageResistanceMultiplier() * FormationState().damageMultiplier;
        if (source != null) dealt *= FrontalPlateDamageMultiplier(source.GlobalPosition);
        _currentHp = Mathf.Max(0f, _currentHp - dealt);

        EventBus.Instance?.Publish(new EnemyDamagedEvent(this, dealt, multiplier, type, source));

        if (_currentHp <= 0f)
        {
            TriggerConvoyCollapseOnDeath();
            EventBus.Instance?.Publish(new EnemyKilledEvent(this, Definition.Bounty));
        }
    }

    public void SetSoftBlocked(bool blocked) => _softBlocked = blocked;

    public void SetRevealed(bool revealed) => _revealed = revealed;

    // Set each tick by MapRuntime from GimmickSystem (Canopy/Mud, GDD §11.1
    // M6/M8) - both are queried by PathNetwork.PathId, so a system outside
    // this class supplies the lookup rather than EnemyController knowing
    // about MapGimmick/GimmickSystem itself.
    public void SetInCanopy(bool inCanopy) => _inCanopy = inCanopy;
    public void SetMudSpeedMultiplier(float multiplier) => _mudSpeedMultiplier = multiplier;

    // A scripted HP cap (B2's Convoy collapse, GDD §10.3: "instantly
    // collapses the escorts to 50% HP") - deliberately bypasses the whole
    // damage-resolution pipeline (armor multipliers, shields, Convoy/
    // Frontal Plate resistance) since this isn't combat damage, it's a
    // narrative-beat state change. Never raises HP.
    public void CapHealth(float maxAllowedHp)
    {
        if (!IsAlive || maxAllowedHp >= _currentHp) return;
        _currentHp = Mathf.Max(0f, maxAllowedHp);
        QueueRedraw();
        if (_currentHp <= 0f)
        {
            TriggerConvoyCollapseOnDeath();
            EventBus.Instance?.Publish(new EnemyKilledEvent(this, Definition.Bounty));
        }
    }

    public float RestoreHealth(float amount)
    {
        if (!IsAlive || amount <= 0f) return 0f;
        float restored = Mathf.Min(amount, _maxHp - _currentHp);
        _currentHp += restored;
        QueueRedraw();
        return restored;
    }

    public void SetCohesionLeadProgress(float leadProgress) => _cohesionLeadProgress = leadProgress;

    public void SetSiegeHoldDistance(float distancePixels)
    {
        if (_pathFollower != null) _pathFollower.HoldDistancePixels = distancePixels;
    }

    public int ConsumeBossAddRequest() => BossPhase?.ConsumePendingAdds() ?? MultiPhaseBoss?.ConsumePendingAdds() ?? 0;

    public void ApplySuppressed(float durationSeconds, float hardCapSeconds)
    {
        if (Definition?.SuppressionImmune == true) return;
        if (MultiPhaseBoss?.IsSuppressionImmune == true) return; // B4 phase 3
        if (IsConvoyProtectedFromSuppression()) return; // B2's Convoy aura
        Status.ApplySuppressed(durationSeconds, hardCapSeconds);
    }
    public void ApplySpotted(float durationSeconds) => Status.ApplySpotted(durationSeconds);

}
