using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Towers;
using FrontsOfWar.Waves;
using System;

namespace FrontsOfWar.Debug;

// Prints key gameplay events to stdout. Useful for headless smoke runs
// (GDD §15.7) where there's no visual to eyeball — subscribe/unsubscribe is
// symmetric via IDisposable so MapRuntime can own one cleanly per mission.
public class DebugEventLogger : IDisposable
{
    public DebugEventLogger()
    {
        EventBus.Instance?.Subscribe<WaveStartedEvent>(OnWaveStarted);
        EventBus.Instance?.Subscribe<WaveSpawningCompleteEvent>(OnWaveSpawningComplete);
        EventBus.Instance?.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        EventBus.Instance?.Subscribe<EnemyLeakedEvent>(OnEnemyLeaked);
        EventBus.Instance?.Subscribe<TowerFiredEvent>(OnTowerFired);
        EventBus.Instance?.Subscribe<SupplyChangedEvent>(OnSupplyChanged);
        EventBus.Instance?.Subscribe<DefenseLineChangedEvent>(OnDefenseLineChanged);
        EventBus.Instance?.Subscribe<DefenseLineDepletedEvent>(OnDefenseLineDepleted);
    }

    public void Dispose()
    {
        EventBus.Instance?.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
        EventBus.Instance?.Unsubscribe<WaveSpawningCompleteEvent>(OnWaveSpawningComplete);
        EventBus.Instance?.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        EventBus.Instance?.Unsubscribe<EnemyLeakedEvent>(OnEnemyLeaked);
        EventBus.Instance?.Unsubscribe<TowerFiredEvent>(OnTowerFired);
        EventBus.Instance?.Unsubscribe<SupplyChangedEvent>(OnSupplyChanged);
        EventBus.Instance?.Unsubscribe<DefenseLineChangedEvent>(OnDefenseLineChanged);
        EventBus.Instance?.Unsubscribe<DefenseLineDepletedEvent>(OnDefenseLineDepleted);
    }

    private void OnWaveStarted(WaveStartedEvent e) => GD.Print($"[wave] {e.WaveNumber} started");
    private void OnWaveSpawningComplete(WaveSpawningCompleteEvent e) => GD.Print($"[wave] {e.WaveNumber} spawning complete");
    private void OnEnemyKilled(EnemyKilledEvent e) => GD.Print($"[kill] {e.Enemy.Definition.Id} bounty={e.Bounty}");
    private void OnEnemyLeaked(EnemyLeakedEvent e) => GD.Print($"[leak] {e.Enemy.Definition.Id} cost={e.LeakCost}");
    private void OnTowerFired(TowerFiredEvent e)
    {
        string target = e.Target == null ? "ground point" : $"target hp={e.Target.CurrentHp:F1}";
        GD.Print($"[fire] {e.Tower.Definition.Id} -> {target}");
    }
    private void OnSupplyChanged(SupplyChangedEvent e) => GD.Print($"[supply] {e.NewBalance} (Δ{e.Delta:+0;-0})");
    private void OnDefenseLineChanged(DefenseLineChangedEvent e) => GD.Print($"[defense-line] {e.NewIntegrity} (Δ{e.Delta:+0;-0})");
    private void OnDefenseLineDepleted(DefenseLineDepletedEvent e) => GD.Print("[defense-line] DEPLETED");
}
