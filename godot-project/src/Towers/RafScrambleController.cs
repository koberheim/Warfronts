using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Map;
using System.Collections.Generic;

namespace FrontsOfWar.Towers;

// Britain's player-directed signature. A sortie is a finite, visible
// corridor strike; it is not a hidden global damage ability.
public partial class RafScrambleController : SignatureControllerBase, IDamageSource
{
    private sealed class Strike
    {
        public float CenterDistance;
        public float Elapsed;
        public int PassesApplied;
    }

    private readonly List<Strike> _strikes = new();
    private float _regenRemaining;

    public string SourceId => $"raf_scramble_{Name}";
    public int ActiveStrikeCount => _strikes.Count;
    public bool AutoScrambleEnabled { get; set; } = true;

    protected override int ChargeCapacity
        => base.ChargeCapacity + (PadTag == FrontsOfWar.Map.PadTag.Elevated ? Definition?.ElevatedExtraCharges ?? 0 : 0);

    public override void _Ready() { EnableSignatureInput(); SetupSignatureClickArea(); }

    public override void Initialize(PathNetwork path, System.Func<System.Collections.Generic.IReadOnlyList<ITargetable>> targetsProvider)
    {
        base.Initialize(path, targetsProvider);
        _regenRemaining = 0f;
        QueueRedraw();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion && IsTargeting)
        {
            QueueRedraw();
            return;
        }
        if (!TryGetTargetClick(@event, out Vector2 worldPoint)) return;
        if (@event is InputEventKey) return;
        TryActivateAtPoint(worldPoint);
        GetViewport().SetInputAsHandled();
    }

    public bool TryActivateAtPoint(Vector2 worldPoint)
    {
        if (Path == null || !TrySpendCharges(1)) return false;
        _strikes.Add(new Strike { CenterDistance = Path.GetClosestDistance(worldPoint) });
        EventBus.Instance?.Publish(new SignatureActivatedEvent(this, worldPoint, Charges));
        EventBus.Instance?.Publish(new SignatureTelegraphEvent(this, worldPoint, Definition.RafStrikeDurationSeconds));
        return true;
    }

    public void SimTick(float delta)
    {
        if (Definition == null || Path == null) return;
        TickCharge(delta);
        if (CurrentLevel >= 3 && AutoScrambleEnabled && _strikes.Count == 0 && HasAirAtObjective())
            TryActivateAtPoint(Path.GetPositionAtDistance(Path.LengthPixels));

        float passSpacing = Definition.RafPassCount > 1
            ? Definition.RafStrikeDurationSeconds / (Definition.RafPassCount - 1)
            : Definition.RafStrikeDurationSeconds;
        for (int i = _strikes.Count - 1; i >= 0; i--)
        {
            var strike = _strikes[i];
            strike.Elapsed += delta;
            while (strike.PassesApplied < Definition.RafPassCount &&
                   strike.Elapsed + 0.0001f >= strike.PassesApplied * passSpacing)
            {
                ResolvePass(strike.CenterDistance);
                strike.PassesApplied++;
            }
            if (strike.PassesApplied >= Definition.RafPassCount && strike.Elapsed >= Definition.RafStrikeDurationSeconds)
                _strikes.RemoveAt(i);
        }
        QueueRedraw();
    }

    private void TickCharge(float delta)
    {
        if (Charges >= MaxCharges) { _regenRemaining = 0f; return; }
        float interval = GetFloat(Definition.ChargeRegenSeconds, 22f);
        // A regeneration cycle only begins once a charge is missing, so a
        // freshly spent charge always waits the full authored interval
        // (GDD §8.2.2: "regenerating one every 22s").
        if (_regenRemaining <= 0f) _regenRemaining = interval;
        _regenRemaining -= delta;
        while (_regenRemaining <= 0f && Charges < MaxCharges)
        {
            Charges++;
            _regenRemaining += interval;
        }
        if (Charges >= MaxCharges) _regenRemaining = 0f;
    }

    private bool HasAirAtObjective()
    {
        Vector2 objective = Path.GetPositionAtDistance(Path.LengthPixels);
        float radius = Definition.RafAutoScrambleRadiusTiles * GameBalanceConfigAutoload.Config.TilePixelSize;
        foreach (var target in TargetsProvider?.Invoke() ?? System.Array.Empty<ITargetable>())
            if (target.IsAir && target.IsAlive && target.GlobalPosition.DistanceTo(objective) <= radius) return true;
        return false;
    }

    private void ResolvePass(float centerDistance)
    {
        float tile = GameBalanceConfigAutoload.Config.TilePixelSize;
        float length = Definition.RafCorridorLengthTiles * tile;
        float width = Definition.RafCorridorWidthTiles * tile;
        var targets = TargetsProvider?.Invoke();
        var air = SignatureTargeting.CorridorTargets(targets, Path, centerDistance, length, width, airOnly: true);
        if (air.Count > 0)
        {
            int limit = Mathf.Min(Definition.RafAirTargetLimit, air.Count);
            float damage = GetFloat(Definition.RafAntiAirDamage, 200f);
            for (int i = 0; i < limit; i++) SignatureTargeting.ApplyDamage(air[i], damage, DamageType.AntiAir, this);
            return;
        }

        var ground = SignatureTargeting.CorridorTargets(targets, Path, centerDistance, length, width, groundOnly: true);
        foreach (var target in ground)
        {
            SignatureTargeting.ApplyDamage(target, GetFloat(Definition.RafSmallArmsDamagePerPass, 55f), DamageType.SmallArms, this);
            SignatureTargeting.ApplyDamage(target, GetFloat(Definition.RafExplosiveDamagePerPass, 20f), DamageType.Explosive, this);
        }
    }

    public override void _Draw()
    {
        DrawSignatureBase(new Color(0.25f, 0.42f, 0.62f), new Color(0.75f, 0.85f, 1f));
        if (IsTargeting)
            DrawTargetingPreview(Definition?.RafCorridorLengthTiles ?? 8f, new Color(0.95f, 0.9f, 0.3f, 0.95f));
        foreach (var strike in _strikes)
        {
            float tile = GameBalanceConfigAutoload.Config.TilePixelSize;
            float half = (Definition?.RafCorridorLengthTiles ?? 8f) * tile * 0.5f;
            float start = Mathf.Max(0f, strike.CenterDistance - half);
            float end = Mathf.Min(Path?.LengthPixels ?? 0f, strike.CenterDistance + half);
            DrawDashedSegment(start, end,
                new Color(0.95f, 0.35f, 0.25f, 0.8f), 5f);
            float flight = Mathf.Clamp(strike.Elapsed / Mathf.Max(0.1f, Definition.RafStrikeDurationSeconds), 0f, 1f);
            Vector2 aircraft = ToLocal(Path.GetPositionAtDistance(Mathf.Lerp(start, end, flight)));
            DrawColoredPolygon(new[] { aircraft + new Vector2(-12f, 4f), aircraft + new Vector2(12f, 0f), aircraft + new Vector2(-12f, -4f) }, Colors.White);
        }
    }
}
