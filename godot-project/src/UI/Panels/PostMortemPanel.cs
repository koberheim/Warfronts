using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Debug;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using FrontsOfWar.Towers;
using FrontsOfWar.UI.Theme;
using System.Collections.Generic;
using System.Linq;

namespace FrontsOfWar.UI.Panels;

// The post-mortem panel (GDD §12.9, §19 prompt 21) - "the game's teaching
// system." Collects leaks, damage by type and per-tower attribution during
// the mission, then hands a report to PostMortemReport on victory or defeat.
// Implements the one suggestion rule the GDD gives as its worked example
// (armor leaked heavily + low AP damage share → suggest AP).
public partial class PostMortemPanel : CanvasLayer
{
    [Export] public NodePath MissionPath;

    private MapRuntime _mission;
    private PostMortemReport _report;

    private readonly Dictionary<string, int> _leaksByEnemyId = new();
    private readonly Dictionary<string, EnemyDefinition> _enemyById = new();
    private readonly Dictionary<DamageType, float> _damageByType = new();
    private readonly Dictionary<string, float> _damageByTower = new();
    private readonly Dictionary<string, int> _investedByTower = new();
    private readonly Dictionary<string, string> _towerNames = new();

    public override void _Ready()
    {
        _mission = GetNode<MapRuntime>(MissionPath);
        _report = new PostMortemReport { Visible = false };
        AddChild(_report);
        UiFactory.Anchor(_report, Control.LayoutPreset.Center, 0, 0);

        EventBus.Instance?.Subscribe<EnemyLeakedEvent>(OnEnemyLeaked);
        EventBus.Instance?.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        EventBus.Instance?.Subscribe<DefenseLineDepletedEvent>(OnDefenseLineDepleted);
        EventBus.Instance?.Subscribe<MissionCompletedEvent>(OnMissionCompleted);

        if (ScreenshotCapture.UiStateIs("postmortem")) Callable.From(() => Show(false)).CallDeferred();
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<EnemyLeakedEvent>(OnEnemyLeaked);
        EventBus.Instance?.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        EventBus.Instance?.Unsubscribe<DefenseLineDepletedEvent>(OnDefenseLineDepleted);
        EventBus.Instance?.Unsubscribe<MissionCompletedEvent>(OnMissionCompleted);
    }

    private void OnEnemyLeaked(EnemyLeakedEvent evt)
    {
        string id = evt.Enemy.Definition.Id;
        _leaksByEnemyId[id] = _leaksByEnemyId.GetValueOrDefault(id) + 1;
        _enemyById[id] = evt.Enemy.Definition;
    }

    private void OnEnemyDamaged(EnemyDamagedEvent evt)
    {
        _damageByType[evt.DamageType] = _damageByType.GetValueOrDefault(evt.DamageType) + evt.DamageDealt;
        if (evt.Source == null) return;

        string id = evt.Source.SourceId;
        _damageByTower[id] = _damageByTower.GetValueOrDefault(id) + evt.DamageDealt;
        if (evt.Source is TowerController tower)
        {
            _investedByTower[id] = tower.Upgrade.TotalInvested;
            _towerNames[id] = tower.Definition?.DisplayName ?? id;
        }
    }

    private void OnDefenseLineDepleted(DefenseLineDepletedEvent evt) => Show(false);
    private void OnMissionCompleted(MissionCompletedEvent evt) => Show(evt.Victory);

    public void Show(bool victory)
    {
        if (_report.Visible) return;
        _report.Build(BuildData(victory), Retry, Results);
        _report.Visible = true;
        UiFactory.FadeIn(_report);
        _report.RetryButton.GrabFocus();
    }

    private void Retry()
    {
        MissionSession.ResetMission();
        GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn");
    }

    private void Results() => GetTree().ChangeSceneToFile("res://scenes_root/results.tscn");

    private PostMortemReport.ReportData BuildData(bool victory)
    {
        float totalDamage = _damageByType.Values.Sum();
        var data = new PostMortemReport.ReportData
        {
            Victory = victory,
            MissionTitle = MissionSession.LastMissionTitle,
            UnspentSupply = _mission.Supply?.Balance ?? 0,
            UnspentCommandPoints = _mission.CommandPoints?.Balance ?? 0,
            Suggestion = BuildSuggestion(totalDamage),
        };
        foreach (var (id, count) in _leaksByEnemyId.OrderByDescending(pair => pair.Value))
            data.Leaks.Add((_enemyById[id], count));
        foreach (DamageType type in System.Enum.GetValues<DamageType>())
            data.DamageByType[type] = _damageByType.GetValueOrDefault(type);
        foreach (var entry in _damageByTower.OrderByDescending(pair => pair.Value))
        {
            int invested = _investedByTower.GetValueOrDefault(entry.Key);
            data.Towers.Add((_towerNames.GetValueOrDefault(entry.Key, entry.Key), entry.Value, invested > 0 ? entry.Value / invested : 0f));
        }
        return data;
    }

    // The exact worked example from GDD §12.9: "IF leaked_armor_share > 0.4
    // AND ap_damage_share < 0.2 THEN suggest_AP".
    private string BuildSuggestion(float totalDamage)
    {
        int totalLeaks = _leaksByEnemyId.Values.Sum();
        int armorLeaks = _leaksByEnemyId.Keys
            .Where(id => _enemyById[id].ArmorClass is ArmorClass.Armored or ArmorClass.Heavy)
            .Sum(id => _leaksByEnemyId[id]);

        float armorLeakShare = totalLeaks > 0 ? (float)armorLeaks / totalLeaks : 0f;
        float apDamageShare = totalDamage > 0f ? _damageByType.GetValueOrDefault(DamageType.ArmorPiercing) / totalDamage : 0f;

        if (armorLeakShare > 0.4f && apDamageShare < 0.2f)
            return "Heavy armor got through and your damage output against it was low. Consider an Anti-Tank Gun, an Armored Emplacement, or Heavy Artillery.";

        return "Review the leaks above and adjust your build before the next attempt.";
    }
}
