using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using FrontsOfWar.Economy;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

// Soviet charge-and-release signature. The barrage is deliberately HE-only:
// it cleans up massed ground threats but is a poor answer to Heavy Armor and
// cannot touch Air, preserving the counterplay in the GDD.
public partial class KatyushaStormController : SignatureControllerBase, IDamageSource
{
    private sealed class Barrage
    {
        public readonly List<Vector2> ImpactPoints = new();
        public float Elapsed;
        public int NextImpact;
    }

    private Barrage _barrage;
    private float _chargePoints;
    private bool _autoFire;
    private TimeController.Speed _speedBeforeBarrage;

    public string SourceId => $"katyusha_storm_{Name}";
    public float ChargePoints => _chargePoints;
    public float FullCharge => GetFloat(Definition?.KatyushaFullCharge, 240f);
    public float ChargeRatio => FullCharge > 0f ? Mathf.Clamp(_chargePoints / FullCharge, 0f, 1f) : 0f;
    public bool IsBarrageActive => _barrage != null;
    public bool AutoFire { get => _autoFire; set => _autoFire = value; }

    public override void _Ready()
    {
        EnableSignatureInput();
        SetupSignatureClickArea(() => TryRelease());
        EventBus.Instance?.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
    }

    public override void Initialize(Map.PathNetwork path, System.Func<System.Collections.Generic.IReadOnlyList<ITargetable>> targetsProvider)
    {
        base.Initialize(path, targetsProvider);
        _chargePoints = 0f;
        _barrage = null;
    }

    public void SimTick(float delta)
    {
        if (Definition == null || Path == null) return;
        if (_barrage == null)
        {
            _chargePoints = Mathf.Min(FullCharge, _chargePoints + Definition.KatyushaChargePerSecond * delta);
            if (_autoFire && _chargePoints >= FullCharge) TryRelease();
            QueueRedraw();
            return;
        }

        _barrage.Elapsed += delta;
        float duration = Mathf.Max(0.1f, Definition.KatyushaImpactDurationSeconds);
        float interval = duration / Mathf.Max(1, _barrage.ImpactPoints.Count);
        while (_barrage.NextImpact < _barrage.ImpactPoints.Count &&
               _barrage.Elapsed + 0.0001f >= _barrage.NextImpact * interval)
        {
            var point = _barrage.ImpactPoints[_barrage.NextImpact];
            EventBus.Instance?.Publish(new SignatureTelegraphEvent(this, point, 0.6f));
            var targets = TargetsProvider?.Invoke();
            int hits = SignatureTargeting.ApplyBlast(targets, point,
                Definition.KatyushaBlastRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize,
                Definition.KatyushaRocketDamage, DamageType.Explosive, this);
            if (CurrentLevel >= 3)
            {
                float cap = GameBalanceConfigAutoload.Config.SuppressedDurationSeconds;
                foreach (var target in targets ?? System.Array.Empty<ITargetable>())
                    if (!target.IsAir && target.IsAlive && target.GlobalPosition.DistanceTo(point) <=
                        Definition.KatyushaBlastRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize)
                        if (target is EnemyController enemy) enemy.ApplySuppressed(cap, 4f);
            }
            _barrage.NextImpact++;
        }

        if (_barrage.NextImpact >= _barrage.ImpactPoints.Count && _barrage.Elapsed >= duration)
        {
            _barrage = null;
            GameLoop.Instance?.Time.SetSpeed(_speedBeforeBarrage);
        }
        QueueRedraw();
    }

    public bool TrySpendCommandPoints(CommandPointLedger ledger)
    {
        if (ledger == null || !ledger.TrySpend(Definition.KatyushaCommandPointCost)) return false;
        _chargePoints = Mathf.Min(FullCharge, _chargePoints + Definition.KatyushaCommandPointCharge);
        return true;
    }

    public void AddKillCharge()
        => _chargePoints = Mathf.Min(FullCharge, _chargePoints + Definition.KatyushaChargePerKill);

    private void OnEnemyKilled(EnemyKilledEvent evt) => AddKillCharge();

    public bool TryRelease()
    {
        if (_barrage != null || _chargePoints < FullCharge || Path == null) return false;
        int count = GetInt(Definition.KatyushaRocketCounts, 24);
        _barrage = new Barrage();
        _chargePoints = 0f;
        var targets = TargetsProvider?.Invoke();
        Vector2 cluster = Path.GetPositionAtDistance(Path.LengthPixels * 0.5f);
        var ground = new List<ITargetable>();
        foreach (var target in targets ?? System.Array.Empty<ITargetable>())
            if (!target.IsAir && target.IsAlive) ground.Add(target);
        var densest = TargetingService.SelectDensestClusterPoint(ground,
            Definition.KatyushaBlastRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize);
        if (densest.HasValue) cluster = densest.Value;
        for (int i = 0; i < count; i++)
        {
            float offset = (i % 7 - 3) * GameBalanceConfigAutoload.Config.TilePixelSize * 0.55f;
            float distance = Mathf.Clamp(Path.GetClosestDistance(cluster) + offset, 0f, Path.LengthPixels);
            _barrage.ImpactPoints.Add(Path.GetPositionAtDistance(distance));
        }
        _speedBeforeBarrage = GameLoop.Instance?.Time.CurrentSpeed ?? TimeController.Speed.Normal;
        GameLoop.Instance?.Time.SetSpeed(TimeController.Speed.Normal);
        EventBus.Instance?.Publish(new SignatureActivatedEvent(this, cluster, Charges));
        return true;
    }

    public override void _Draw()
    {
        DrawSignatureBase(new Color(0.55f, 0.35f, 0.27f), new Color(0.95f, 0.72f, 0.3f));
        float width = 32f * ChargeRatio;
        DrawRect(new Rect2(-16f, -17f, 32f, 3f), new Color(0.1f, 0.1f, 0.1f));
        DrawRect(new Rect2(-16f, -17f, width, 3f), new Color(0.95f, 0.72f, 0.3f));
        if (_barrage != null)
            foreach (var point in _barrage.ImpactPoints)
                DrawCircle(ToLocal(point), 5f, new Color(0.95f, 0.35f, 0.18f, 0.45f));
    }
}
