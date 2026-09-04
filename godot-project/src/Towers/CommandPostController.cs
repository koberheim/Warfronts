using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using System.Collections.Generic;

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
        float multiplier = Definition?.DisplayName?.Contains("Radar", System.StringComparison.OrdinalIgnoreCase) == true ? 2f : 1f;
        float radius = Mathf.Max(0f, Upgrade.CurrentStats().AuraRadiusTiles) * tilePixelSize * multiplier * DoctrineAuraRadiusMultiplier;
        foreach (var enemy in enemies ?? System.Array.Empty<EnemyController>())
            if (enemy != null && enemy.IsConcealed && enemy.GlobalPosition.DistanceTo(GlobalPosition) <= radius)
                enemy.SetRevealed(true);
    }

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
