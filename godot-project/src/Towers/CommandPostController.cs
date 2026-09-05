using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using System.Collections.Generic;
using System.Linq;

namespace FrontsOfWar.Towers;

// T9 Command Post (GDD §6, §19 prompt 15) — the one archetype that never
// fires. Reuses TowerDefinition/TowerUpgradeController for its cost/level
// bookkeeping (it has the same 4-level, branch-fork shape as every other
// archetype), but is ticked by CommandPostManager, not TowerManager, since
// its only job each tick is projecting an aura rather than targeting/firing.
public partial class CommandPostController : Node2D
{
    [Export] public TowerDefinition Definition;

    public TowerUpgradeController Upgrade { get; private set; }

    // Set each tick by DoctrineSystem's passive pass (GDD §19 prompt 39 —
    // Airborne's "Command Posts grant +50% aura radius"). 1f (no-op) for any
    // mission with no doctrine loaded.
    public float DoctrineAuraRadiusMultiplier = 1f;

    public override void _Ready()
    {
        Upgrade = new TowerUpgradeController(Definition, GameBalanceConfigAutoload.Config);
        SetupClickArea();
    }

    // Same click target every combat tower builds for itself (see
    // TowerController.SetupClickArea) so a placed post can be inspected,
    // upgraded, and sold through the inspection panel (GDD §13.5).
    private void SetupClickArea()
    {
        var area = new Area2D { InputPickable = true };
        area.AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 18f } });
        AddChild(area);
        area.InputEvent += (viewport, @event, shapeIdx) =>
        {
            if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
                EventBus.Instance?.Publish(new CommandPostClickedEvent(this));
        };
    }

    public void SimTick(float tickDeltaSeconds)
    {
        Upgrade.Tick(tickDeltaSeconds);
        float radius = CurrentAuraRadiusPixels();
        if (!Mathf.IsEqualApprox(radius, _drawnAuraRadius)) QueueRedraw();
    }

    private float _drawnAuraRadius = -1f;

    private float CurrentAuraRadiusPixels()
        => Mathf.Max(0f, Upgrade.CurrentStats().AuraRadiusTiles) * GameBalanceConfigAutoload.Config.TilePixelSize * DoctrineAuraRadiusMultiplier;

    // The aura ring is always visible (GDD §5.8: the aura's value is never
    // hidden; UI spec §8.4/§9: olive, 35 %, 48 segments) and redraws only
    // when an upgrade or doctrine changes the radius.
    public override void _Draw()
    {
        if (Upgrade == null) return;
        _drawnAuraRadius = CurrentAuraRadiusPixels();
        if (_drawnAuraRadius <= 0f) return;
        DrawArc(Vector2.Zero, _drawnAuraRadius, 0f, Mathf.Tau, 48, UI.Theme.UiPalette.Olive with { A = 0.35f }, 2f, true);
    }

    public int CurrentCommandPointsPerWave => Upgrade.CurrentStats().CommandPointsPerWave;
    public int CurrentSupplyPerWave => Upgrade.CurrentStats().SupplyPerWave;

    public void RevealTargets(IReadOnlyList<EnemyController> enemies, float tilePixelSize)
    {
        var stats = Upgrade.CurrentStats();
        bool isForwardObserver = stats.StatusEffectId == "Spotted";
        float multiplier = Definition?.DisplayName?.Contains("Radar", System.StringComparison.OrdinalIgnoreCase) == true ? 2f : 1f;
        float radius = Mathf.Max(0f, stats.AuraRadiusTiles) * tilePixelSize * multiplier * DoctrineAuraRadiusMultiplier;
        foreach (var enemy in enemies ?? System.Array.Empty<EnemyController>())
        {
            if (enemy == null || !enemy.IsConcealed) continue;
            // Forward Observer (GDD §6 T9 branch fork): reveal extends to the
            // entire map for Air units specifically, not just the aura radius.
            bool mapWideAir = isForwardObserver && enemy.IsAir;
            if (mapWideAir || enemy.GlobalPosition.DistanceTo(GlobalPosition) <= radius)
                enemy.SetRevealed(true);
        }
    }

    // Forward Observer's own effect, distinct from the passive Concealed
    // reveal above: "Applies Spotted to the strongest enemy in a 10-tile
    // radius on a 4s rotation" (GDD §6 T9 branch fork). StatusDurationSeconds
    // does double duty as both the pulse cadence and the applied Spotted
    // duration, matching MinefieldController's identical reuse of the same
    // field for its own periodic Suppressed trigger. Inert on L1/L2 and on
    // the Logistics Depot branch, neither of which set StatusEffectId.
    public void TickSpottedPulse(float tickDeltaSeconds, IReadOnlyList<EnemyController> enemies, float tilePixelSize)
    {
        var stats = Upgrade.CurrentStats();
        if (stats.StatusEffectId != "Spotted" || stats.StatusDurationSeconds <= 0f)
        {
            _spottedPulseElapsed = 0f;
            return;
        }

        _spottedPulseElapsed += tickDeltaSeconds;
        if (_spottedPulseElapsed < stats.StatusDurationSeconds) return;
        _spottedPulseElapsed -= stats.StatusDurationSeconds;

        float radius = stats.RangeTiles * tilePixelSize;
        int targetCount = Mathf.Max(1, stats.SalvoCount);
        var strongest = (enemies ?? System.Array.Empty<EnemyController>())
            .Where(enemy => enemy != null && enemy.IsAlive && GlobalPosition.DistanceTo(enemy.GlobalPosition) <= radius)
            .OrderByDescending(enemy => enemy.CurrentHp)
            .Take(targetCount);
        foreach (var enemy in strongest) enemy.ApplySpotted(stats.StatusDurationSeconds);
    }

    private float _spottedPulseElapsed;

    // Sets (never adds — auras from multiple posts don't stack, GDD §6 T9)
    // this post's bonus on every tower within its radius, but only if it's
    // stronger than whatever that tower already has this tick.
    public void ApplyAuraTo(TowerManager towers, float tilePixelSize)
    {
        var stats = Upgrade.CurrentStats();
        float radiusPixels = stats.AuraRadiusTiles * tilePixelSize * DoctrineAuraRadiusMultiplier;
        float radiusSquared = radiusPixels * radiusPixels;
        float rangeMultiplier = 1f + stats.AuraRangeBonusPercent;
        float rofMultiplier = 1f + stats.AuraRateOfFireBonusPercent;

        foreach (var tower in towers.Towers)
        {
            if (GlobalPosition.DistanceSquaredTo(tower.GlobalPosition) > radiusSquared) continue;
            tower.AuraRangeMultiplier = Mathf.Max(tower.AuraRangeMultiplier, rangeMultiplier);
            tower.AuraRateOfFireMultiplier = Mathf.Max(tower.AuraRateOfFireMultiplier, rofMultiplier);
        }
    }
}
