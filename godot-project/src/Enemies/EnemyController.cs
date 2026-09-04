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
public partial class EnemyController : Node2D, ITargetable, IDamageReceiver
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
    private Func<IReadOnlyList<EnemyController>> _enemyProvider;
    private EnemyController _repairTarget;

    public BossPhaseController BossPhase { get; private set; }
    public PathNetwork PathNetwork { get; private set; }

    public StatusController Status { get; } = new();

    public float CurrentHp => _currentHp;
    public bool IsAlive => _currentHp > 0f;
    public bool IsAir => Definition.IsAir;
    public bool IsConcealed => Definition?.SpecialAbilityId == "recon_concealment";
    public bool IsRevealed => !IsConcealed || _revealed || Status.IsSpotted;
    public float PathProgress => IsAir && _airCorridor != null ? _airProgress : _pathFollower?.Progress ?? 0f;
    public float PathDistancePixels => IsAir && _airCorridor != null
        ? _airProgress * _airCorridor.LengthPixels : _pathFollower?.DistanceTraveled ?? 0f;
    public bool ReachedEnd => IsAir && _airCorridor != null ? _airProgress >= 1f : _pathFollower?.ReachedEnd ?? false;
    public Vector2 Velocity { get; private set; }
    public float MaxHp => _maxHp;
    public float ShieldRemaining => _shieldRemaining;
    public EnemyController RepairTarget => _repairTarget;

    public void Initialize(PathNetwork path, float hpScaleMultiplier = 1f, AirCorridorDefinition airCorridor = null)
    {
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
        BossPhase = Definition.IsBoss ? new BossPhaseController(Definition) : null;
        _cohesionLeadProgress = 0f;
        _siegeBombardRemaining = Definition.SpecialAbilityId == "siege_bombard"
            ? Definition.SiegeBombardIntervalSeconds : 0f;
        _bossSkirtVisual = GetNodeOrNull<CanvasItem>("Skirt");
        GlobalPosition = Definition.IsAir && _airCorridor != null
            ? _airCorridor.EntryPosition : path.GetPositionAtDistance(0f);
    }

    public void SetEnemyProvider(Func<IReadOnlyList<EnemyController>> provider) => _enemyProvider = provider;

    public void SimTick(float tickDeltaSeconds)
    {
        if (_pathFollower == null || !IsAlive) return;

        Status.Tick(tickDeltaSeconds);
        BossPhase?.Tick(tickDeltaSeconds);

        if (Definition.Archetype == EnemyArchetype.Support) TickFieldRepair(tickDeltaSeconds);
        if (Definition.IsAir && _airCorridor != null)
        {
            _previousPosition = GlobalPosition;
            float distance = Mathf.Max(1f, _airCorridor.LengthPixels);
            _airProgress = Mathf.Min(1f, _airProgress + Definition.MoveSpeedTilesPerSec *
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
        if (Definition.SpecialAbilityId == "swarm_cohesion" &&
            _cohesionLeadProgress - PathProgress >= Definition.CohesionCatchupThreshold)
            speedMultiplier *= Definition.CohesionCatchupSpeedMultiplier;
        if (!_softBlocked)
            _pathFollower.Advance(Definition.MoveSpeedTilesPerSec * (BossPhase?.SpeedMultiplier ?? 1f), speedMultiplier, tickDeltaSeconds, config.TilePixelSize);
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
        if (!IsAlive) return;

        float multiplier = DamageTable.Default.Multiplier(type, Definition.ArmorClass);
        float dealt = BossPhase?.ResolveDamage(baseDamage, type, Status.IsSpotted)
            ?? DamageResolver.ResolveDamage(baseDamage, type, Definition.ArmorClass, Status.IsSpotted, DamageTable.Default);
        if (BossPhase is { IsSkirtIntact: false } && _bossSkirtVisual != null)
            _bossSkirtVisual.Visible = false;
        dealt = AbsorbShieldedDamage(dealt);
        _currentHp = Mathf.Max(0f, _currentHp - dealt);

        EventBus.Instance?.Publish(new EnemyDamagedEvent(this, dealt, multiplier, type, source));

        if (_currentHp <= 0f)
            EventBus.Instance?.Publish(new EnemyKilledEvent(this, Definition.Bounty));
    }

    public void SetSoftBlocked(bool blocked) => _softBlocked = blocked;

    public void SetRevealed(bool revealed) => _revealed = revealed;

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

    public int ConsumeBossAddRequest() => BossPhase?.ConsumePendingAdds() ?? 0;

    public void ApplySuppressed(float durationSeconds, float hardCapSeconds)
    {
        if (Definition.SuppressionImmune) return;
        Status.ApplySuppressed(durationSeconds, hardCapSeconds);
    }
    public void ApplySpotted(float durationSeconds) => Status.ApplySpotted(durationSeconds);

    // Health bar only appears once damaged (GDD §13.6 — reduces clutter),
    // always carries the armor-class glyph on its left cap when visible,
    // and status badges to the right. Never color alone — glyph shapes
    // differ by armor class, matching the accessibility rule in §13.9.
    public override void _Draw()
    {
        if (!IsAlive || Definition == null || _maxHp <= 0f) return;

        const float barWidth = 42f;
        const float barHeight = 4f;
        const float yOffset = -30f;
        float fraction = _currentHp / _maxHp;

        if (Definition.IsBoss || _currentHp < _maxHp)
        {
            DrawRect(new Rect2(-barWidth / 2f, yOffset, barWidth, barHeight), UiPalette.Slate with { A = 0.9f });
            DrawRect(new Rect2(-barWidth / 2f, yOffset, barWidth * fraction, barHeight), UiPalette.Red);
        }

        if (BossPhase is { IsSkirtIntact: true })
        {
            float skirtFraction = BossPhase.SkirtMaxHp > 0f ? BossPhase.SkirtHp / BossPhase.SkirtMaxHp : 0f;
            DrawRect(new Rect2(-barWidth / 2f, yOffset - 6f, barWidth, barHeight), UiPalette.Slate with { A = 0.9f });
            DrawRect(new Rect2(-barWidth / 2f, yOffset - 6f, barWidth * skirtFraction, barHeight), UiPalette.Amber);
        }

        DrawArmorGlyph(new Vector2(-barWidth / 2f - 7f, yOffset + barHeight / 2f));

        float badgeX = barWidth / 2f + 6f;
        float badgeY = yOffset + barHeight / 2f;
        if (Status.IsSuppressed)
        {
            DrawCircle(new Vector2(badgeX, badgeY), 3f, UiPalette.Grey);
            badgeX += 8f;
        }
        if (Status.IsSpotted)
            DrawArc(new Vector2(badgeX, badgeY), 3f, 0f, Mathf.Tau, 12, UiPalette.Red, 1.5f, true);
        if (Definition.Archetype == EnemyArchetype.Escort && _shieldRemaining > 0f)
        {
            float radius = Definition.EscortShieldRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize;
            var bubble = new Vector2[7];
            for (int i = 0; i < bubble.Length; i++)
                bubble[i] = new Vector2(radius, 0f).Rotated(i * Mathf.Tau / bubble.Length);
            DrawPolyline(bubble, UiPalette.Blue with { A = 0.28f }, 2f, true);
            DrawRect(new Rect2(-21f, 24f, 42f, 3f), UiPalette.Slate with { A = 0.9f });
            DrawRect(new Rect2(-21f, 24f, 42f * (_shieldRemaining / Mathf.Max(1f, Definition.EscortShieldMaxHp)), 3f), UiPalette.Blue);
        }
        if (_repairTarget != null && _repairTarget.IsAlive)
            DrawLine(Vector2.Zero, ToLocal(_repairTarget.GlobalPosition), UiPalette.Green with { A = 0.9f }, 3f);
        if (IsAir)
        {
            DrawLine(new Vector2(-20f, 10f), new Vector2(20f, 10f), new Color(0.15f, 0.15f, 0.2f, 0.35f), 8f);
            DrawColoredPolygon(new[] { new Vector2(-18f, 0f), new Vector2(18f, 0f), new Vector2(0f, -8f) }, new Color(0.8f, 0.82f, 0.86f));
        }
        if (Definition.Archetype == EnemyArchetype.Support)
        {
            DrawRect(new Rect2(-15f, -10f, 30f, 20f), new Color(0.42f, 0.54f, 0.58f));
            DrawLine(new Vector2(0f, -10f), new Vector2(10f, -22f), new Color(0.75f, 0.78f, 0.68f), 3f);
        }
        if (Definition.Archetype == EnemyArchetype.Escort)
        {
            DrawRect(new Rect2(-17f, -11f, 34f, 22f), new Color(0.48f, 0.5f, 0.58f));
            DrawLine(new Vector2(-12f, -15f), new Vector2(-6f, 15f), new Color(0.75f, 0.78f, 0.84f), 3f);
            DrawLine(new Vector2(12f, -15f), new Vector2(6f, 15f), new Color(0.75f, 0.78f, 0.84f), 3f);
        }
        if (Definition.Archetype == EnemyArchetype.Recon)
        {
            DrawCircle(Vector2.Zero, 8f, new Color(0.65f, 0.7f, 0.72f, 0.45f));
            for (int i = 0; i < 8; i += 2)
                DrawArc(Vector2.Zero, 12f, i * Mathf.Tau / 8f, (i + 1) * Mathf.Tau / 8f, 4, new Color(0.75f, 0.8f, 0.82f, 0.7f), 2f);
        }
    }

    // Deliberately distinct shapes, not just colors (Soft: square, Hardened:
    // small circle, Armored: larger circle, Heavy: diamond) — GDD §5.3's
    // "cloth square / half shield / full shield / double shield" reading,
    // approximated with primitives until real icon art exists.
    private void DrawArmorGlyph(Vector2 center)
    {
        switch (Definition.ArmorClass)
        {
            case ArmorClass.Soft:
                DrawRect(new Rect2(center - new Vector2(2.5f, 2.5f), new Vector2(5f, 5f)), new Color(0.85f, 0.85f, 0.8f));
                break;
            case ArmorClass.Hardened:
                DrawCircle(center, 3f, new Color(0.75f, 0.75f, 0.75f));
                break;
            case ArmorClass.Armored:
                DrawCircle(center, 4f, new Color(0.7f, 0.72f, 0.78f));
                break;
            case ArmorClass.Heavy:
                var points = new[]
                {
                    center + new Vector2(0f, -5f), center + new Vector2(5f, 0f),
                    center + new Vector2(0f, 5f), center + new Vector2(-5f, 0f),
                };
                DrawColoredPolygon(points, new Color(0.85f, 0.7f, 0.3f));
                break;
        }
    }
}
