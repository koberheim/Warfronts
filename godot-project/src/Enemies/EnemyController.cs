using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Map;

namespace FrontsOfWar.Enemies;

// One enemy instance (GDD §19 prompt 8). Movement and damage are driven by
// explicit SimTick calls from EnemyManager, not Godot's _PhysicsProcess —
// see GameLoop's fixed-tick model (§15.4): every enemy must advance exactly
// the same amount regardless of render framerate or game speed.
public partial class EnemyController : Node2D, ITargetable
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

    public BossPhaseController BossPhase { get; private set; }
    public PathNetwork PathNetwork { get; private set; }

    public StatusController Status { get; } = new();

    public float CurrentHp => _currentHp;
    public bool IsAlive => _currentHp > 0f;
    public bool IsAir => Definition.IsAir;
    public float PathProgress => _pathFollower?.Progress ?? 0f;
    public float PathDistancePixels => _pathFollower?.DistanceTraveled ?? 0f;
    public bool ReachedEnd => _pathFollower?.ReachedEnd ?? false;
    public Vector2 Velocity { get; private set; }

    public void Initialize(PathNetwork path, float hpScaleMultiplier = 1f)
    {
        PathNetwork = path;
        _pathFollower = new PathFollower(path);
        _hpScaleMultiplier = hpScaleMultiplier;
        _maxHp = Definition.BaseHp * hpScaleMultiplier;
        _currentHp = _maxHp;
        BossPhase = Definition.IsBoss ? new BossPhaseController(Definition) : null;
        _cohesionLeadProgress = 0f;
        _siegeBombardRemaining = Definition.SpecialAbilityId == "siege_bombard"
            ? Definition.SiegeBombardIntervalSeconds : 0f;
        _bossSkirtVisual = GetNodeOrNull<CanvasItem>("Skirt");
        GlobalPosition = path.GetPositionAtDistance(0f);
    }

    public void SimTick(float tickDeltaSeconds)
    {
        if (_pathFollower == null || !IsAlive) return;

        Status.Tick(tickDeltaSeconds);
        BossPhase?.Tick(tickDeltaSeconds);

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
        _currentHp = Mathf.Max(0f, _currentHp - dealt);

        EventBus.Instance?.Publish(new EnemyDamagedEvent(this, dealt, multiplier, type, source));

        if (_currentHp <= 0f)
            EventBus.Instance?.Publish(new EnemyKilledEvent(this, Definition.Bounty));
    }

    public void SetSoftBlocked(bool blocked) => _softBlocked = blocked;

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
        if (!IsAlive || (!Definition.IsBoss && _currentHp >= _maxHp) || _maxHp <= 0f) return;

        const float barWidth = 42f;
        const float barHeight = 4f;
        const float yOffset = -30f;
        float fraction = _currentHp / _maxHp;

        DrawRect(new Rect2(-barWidth / 2f, yOffset, barWidth, barHeight), new Color(0.15f, 0.15f, 0.15f, 0.9f));
        DrawRect(new Rect2(-barWidth / 2f, yOffset, barWidth * fraction, barHeight), new Color(0.75f, 0.15f, 0.15f, 1f));

        if (BossPhase is { IsSkirtIntact: true })
        {
            float skirtFraction = BossPhase.SkirtMaxHp > 0f ? BossPhase.SkirtHp / BossPhase.SkirtMaxHp : 0f;
            DrawRect(new Rect2(-barWidth / 2f, yOffset - 6f, barWidth, barHeight), new Color(0.1f, 0.1f, 0.1f, 0.9f));
            DrawRect(new Rect2(-barWidth / 2f, yOffset - 6f, barWidth * skirtFraction, barHeight), new Color(0.85f, 0.65f, 0.2f, 1f));
        }

        DrawArmorGlyph(new Vector2(-barWidth / 2f - 7f, yOffset + barHeight / 2f));

        float badgeX = barWidth / 2f + 6f;
        float badgeY = yOffset + barHeight / 2f;
        if (Status.IsSuppressed)
        {
            DrawCircle(new Vector2(badgeX, badgeY), 3f, new Color(0.6f, 0.6f, 0.6f));
            badgeX += 8f;
        }
        if (Status.IsSpotted)
            DrawCircle(new Vector2(badgeX, badgeY), 3f, new Color(0.85f, 0.2f, 0.2f));
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
