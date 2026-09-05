using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Debug;
using FrontsOfWar.Doctrines;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Meta;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.Towers;
using FrontsOfWar.Waves;
using System.Linq;

namespace FrontsOfWar.Map;

public partial class MapRuntime
{
    public void SimTick(float tickDeltaSeconds)
    {
        if (_missionOver) return;
        var config = GameBalanceConfigAutoload.Config;

        Waves.Tick(tickDeltaSeconds);
        FriendlyUnits.Tick(tickDeltaSeconds, Enemies);
        _arsenal?.SimTick(tickDeltaSeconds);

        // Gimmicks (GDD §11.2) must set each enemy's Canopy/Mud state before
        // Enemies.Tick applies movement this same frame.
        Gimmicks.Tick(tickDeltaSeconds);
        foreach (var enemy in Enemies.Enemies)
        {
            string pathId = enemy.PathNetwork?.PathId;
            enemy.SetInCanopy(Gimmicks.IsPathConcealed(pathId));
            enemy.SetMudSpeedMultiplier(Gimmicks.SpeedMultiplierForPath(pathId, GimmickRules.IsVehicle(enemy.Definition.Archetype)));
        }
        Enemies.Tick(tickDeltaSeconds);
        _spatialGrid.Rebuild(Enemies.GetTargetables());
        CommandPosts.RevealTargets(Enemies.Enemies, config.TilePixelSize);
        CommandPosts.TickSpottedPulse(tickDeltaSeconds, Enemies.Enemies, config.TilePixelSize);
        CommandPosts.Tick(tickDeltaSeconds, Towers, config.TilePixelSize);
        Towers.ResetSignatureModifiers();
        Signatures.Tick(tickDeltaSeconds);
        Minefields.Tick(tickDeltaSeconds);
        SpecialPlacement?.Tick(tickDeltaSeconds);
        float gimmickRangeMultiplier = Gimmicks.GlobalRangeMultiplier();
        foreach (var tower in Towers.Towers) tower.GimmickRangeMultiplier = gimmickRangeMultiplier;
        Towers.Tick(tickDeltaSeconds, _spatialGrid, Projectiles);
        Projectiles.Tick(tickDeltaSeconds, _spatialGrid);
        Abilities.Tick(tickDeltaSeconds, _spatialGrid);
        Doctrines?.Tick(tickDeltaSeconds, _spatialGrid);

        if (!_victoryPublished && !_waitingForBuild && !Waves.IsRunning && Enemies.Enemies.Count == 0 && Enemies.PendingSpawnCount == 0
            && Waves.PeekUpcoming(1).Count > 0)
        {
            if (Waves.CurrentWaveNumber > 0) Supply.Credit(Supply.EndOfWaveIncome(Waves.CurrentWaveNumber));
            BeginBuildPhase();
        }

        if (_waitingForBuild)
        {
            _buildTimeRemaining -= tickDeltaSeconds;
            if (_buildTimeRemaining <= 0f)
            {
                _waitingForBuild = false;
                Waves.StartNextWave();
            }
        }

        if (!_victoryPublished && !_waitingForBuild && !Waves.IsRunning && Enemies.Enemies.Count == 0 && Enemies.PendingSpawnCount == 0
            && Waves.PeekUpcoming(1).Count == 0 && Waves.CurrentWaveNumber > 0)
        {
            Supply.Credit(Supply.EndOfWaveIncome(Waves.CurrentWaveNumber));
            _victoryPublished = true;
            _missionOver = true;
            MissionSession.LastMissionWon = true;
            MissionSession.LastWaveReached = Waves.CurrentWaveNumber;
            EventBus.Instance?.Publish(new MissionCompletedEvent(true));
        }
    }

    public void RegisterTower(TowerController tower) => Towers.Register(tower);
    public void StartWave(WaveDefinition wave) => Waves.StartWave(wave);

    public float BuildTimeRemaining => Mathf.Max(0f, _buildTimeRemaining);
    public bool IsBuildPhase => _waitingForBuild;
    public int TotalWaves { get; private set; }

    // The exact Supply the "Call Wave Early" button pays right now (GDD
    // §7.7: "shows the exact bonus Supply before you commit") - the HUD reads
    // this so the number shown is the number credited.
    public int EarlyCallBonusNow => _waitingForBuild
        ? Supply.EarlyCallBonus(Waves.PeekUpcoming(1)[0].WaveNumber, Mathf.Clamp(_buildTimeRemaining / Mathf.Max(1f, _buildPhaseDuration), 0f, 1f))
        : 0;

    private void BeginBuildPhase()
    {
        var next = Waves.PeekUpcoming(1).FirstOrDefault();
        if (next == null) return;
        var config = GameBalanceConfigAutoload.Config;
        _buildPhaseDuration = next.IsBossWave ? config.BossBuildTimeSeconds : Difficulty switch
        {
            Difficulty.Recruit => config.BuildTimeRecruit,
            Difficulty.Veteran => config.BuildTimeVeteran,
            Difficulty.Elite => config.BuildTimeElite,
            _ => config.BuildTimeRegular,
        };
        _buildTimeRemaining = _buildPhaseDuration;
        _waitingForBuild = true;
        EventBus.Instance?.Publish(new BuildPhaseStartedEvent(next.WaveNumber, _buildPhaseDuration));
    }

    public void CallNextWaveEarly()
    {
        if (!_waitingForBuild) return;
        Supply.Credit(EarlyCallBonusNow);
        _waitingForBuild = false;
        Waves.StartNextWave();
    }

    public bool ActivateAbility(Economy.AbilityType type, Vector2 targetPoint)
        => Abilities.TryActivate(type, targetPoint, CommandPoints, Towers, DefenseLine);

    public bool ActivateDoctrineAbility(Vector2 primaryPoint, Vector2? secondaryPoint = null,
        TowerController towerTarget = null, BuildPad padTarget = null)
        => Doctrines?.TryActivate(primaryPoint, _spatialGrid, secondaryPoint, towerTarget, padTarget) ?? false;

    private void OnBossAddsRequested(BossAddsRequestedEvent evt)
    {
        if (evt.Boss?.BossPhase == null || evt.Boss.Definition.AddDefinition == null) return;
        for (int i = 0; i < evt.Count; i++)
            Enemies.Spawn(evt.Boss.Definition.AddDefinition, evt.Boss.PathNetwork, evt.Boss.GetParent(), 1f);
    }

    private void OnBossReachedObjective(BossReachedObjectiveEvent evt)
    {
        if (!_missionOver) DefenseLine.ForceDeplete();
    }

    private void OnDefenseLineDepleted(DefenseLineDepletedEvent evt)
    {
        if (_missionOver) return;
        _missionOver = true;
        MissionSession.LastMissionWon = false;
        MissionSession.LastWaveReached = Waves?.CurrentWaveNumber ?? 0;
        EventBus.Instance?.Publish(new MissionCompletedEvent(false));
    }
}
