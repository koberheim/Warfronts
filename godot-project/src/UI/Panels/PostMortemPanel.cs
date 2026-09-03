using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Economy;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using System.Collections.Generic;
using System.Linq;

namespace FrontsOfWar.UI.Panels;

// The post-mortem panel (GDD §12.9, §19 prompt 21) — "the game's teaching
// system." Simplified vs. the full spec: shows leaks, damage dealt by type,
// and unspent resources, plus the one suggestion rule GDD gives as its
// worked example (armor leaked heavily + low AP damage share → suggest AP
// towers). Doesn't yet identify "most/least effective tower by damage-per-
// Supply" — that needs per-tower damage attribution, deferred (see
// docs/PROGRESS.md). Currently triggers only on defeat (DefenseLineDepleted)
// since there's no victory/mission-complete flow yet — that's M3.
public partial class PostMortemPanel : CanvasLayer
{
    [Export] public NodePath MissionPath;

    private MapRuntime _mission;
    private PanelContainer _panel;
    private Label _bodyLabel;

    private readonly Dictionary<string, int> _leaksByEnemyId = new();
    private readonly Dictionary<string, ArmorClass> _armorClassByEnemyId = new();
    private readonly Dictionary<DamageType, float> _damageByType = new();

    public override void _Ready()
    {
        _mission = GetNode<MapRuntime>(MissionPath);

        _panel = new PanelContainer { Position = new Vector2(300, 150), CustomMinimumSize = new Vector2(420, 260), Visible = false };
        AddChild(_panel);
        _bodyLabel = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _panel.AddChild(_bodyLabel);

        EventBus.Instance?.Subscribe<EnemyLeakedEvent>(OnEnemyLeaked);
        EventBus.Instance?.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        EventBus.Instance?.Subscribe<DefenseLineDepletedEvent>(OnDefenseLineDepleted);
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<EnemyLeakedEvent>(OnEnemyLeaked);
        EventBus.Instance?.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        EventBus.Instance?.Unsubscribe<DefenseLineDepletedEvent>(OnDefenseLineDepleted);
    }

    private void OnEnemyLeaked(EnemyLeakedEvent evt)
    {
        string id = evt.Enemy.Definition.Id;
        _leaksByEnemyId[id] = _leaksByEnemyId.GetValueOrDefault(id) + 1;
        _armorClassByEnemyId[id] = evt.Enemy.Definition.ArmorClass;
    }

    private void OnEnemyDamaged(EnemyDamagedEvent evt)
        => _damageByType[evt.DamageType] = _damageByType.GetValueOrDefault(evt.DamageType) + evt.DamageDealt;

    private void OnDefenseLineDepleted(DefenseLineDepletedEvent evt) => Show();

    public new void Show()
    {
        _panel.Visible = true;
        _bodyLabel.Text = BuildReport();
    }

    private string BuildReport()
    {
        var lines = new List<string> { "MISSION FAILED — Post-mortem", "" };

        lines.Add("Leaked:");
        if (_leaksByEnemyId.Count == 0) lines.Add("  (nothing leaked)");
        foreach (var (id, count) in _leaksByEnemyId) lines.Add($"  {count}x {id}");

        float totalDamage = _damageByType.Values.Sum();
        lines.Add("");
        lines.Add("Damage dealt by type:");
        foreach (DamageType type in System.Enum.GetValues<DamageType>())
        {
            float dealt = _damageByType.GetValueOrDefault(type);
            float share = totalDamage > 0f ? dealt / totalDamage : 0f;
            lines.Add($"  {type}: {dealt:F0} ({share:P0})");
        }

        lines.Add("");
        lines.Add($"Unspent Supply: {_mission.Supply.Balance}");
        lines.Add($"Unspent Command Points: {_mission.CommandPoints.Balance}");

        lines.Add("");
        lines.Add(BuildSuggestion(totalDamage));

        return string.Join("\n", lines);
    }

    // The exact worked example from GDD §12.9: "IF leaked_armor_share > 0.4
    // AND ap_damage_share < 0.2 THEN suggest_AP".
    private string BuildSuggestion(float totalDamage)
    {
        int totalLeaks = _leaksByEnemyId.Values.Sum();
        int armorLeaks = _leaksByEnemyId.Keys
            .Where(id => _armorClassByEnemyId[id] is ArmorClass.Armored or ArmorClass.Heavy)
            .Sum(id => _leaksByEnemyId[id]);

        float armorLeakShare = totalLeaks > 0 ? (float)armorLeaks / totalLeaks : 0f;
        float apDamageShare = totalDamage > 0f ? _damageByType.GetValueOrDefault(DamageType.ArmorPiercing) / totalDamage : 0f;

        if (armorLeakShare > 0.4f && apDamageShare < 0.2f)
            return "Suggestion: heavy armor got through and your damage output vs it was low. Consider: Anti-Tank Gun, Armored Emplacement, or Heavy Artillery.";

        return "Suggestion: review the leaks above and adjust your build for next time.";
    }
}
