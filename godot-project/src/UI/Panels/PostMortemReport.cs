using Godot;
using System;
using System.Collections.Generic;
using FrontsOfWar.Combat;
using FrontsOfWar.Enemies;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Panels;

// The post-mortem sheet (docs/UI_DESIGN_SPEC.md §8.8; GDD §12.9): stamp,
// leaks, damage-by-type chart, tower effectiveness, unspent resources and
// the suggestion line, with Retry focused by default (GDD §13.7: "Retry must
// be the default focused button and must be one click").
public partial class PostMortemReport : PanelContainer
{
    public sealed class ReportData
    {
        public bool Victory;
        public string MissionTitle = "";
        public List<(EnemyDefinition Enemy, int Count)> Leaks = new();
        public Dictionary<DamageType, float> DamageByType = new();
        public List<(string Name, float Damage, float PerSupply)> Towers = new();
        public int UnspentSupply;
        public int UnspentCommandPoints;
        public string Suggestion = "";
    }

    public Button RetryButton { get; private set; }

    public void Build(ReportData data, Action onRetry, Action onResults)
    {
        ThemeTypeVariation = "PaperPanel";
        CustomMinimumSize = new Vector2(760f, 620f);
        foreach (var child in GetChildren()) child.QueueFree();

        var column = UiFactory.VBox(8);
        AddChild(column);

        var stamp = UiFactory.Label("StampLabel", data.Victory ? "MISSION COMPLETE" : "MISSION FAILED", uppercase: true);
        if (data.Victory) stamp.AddThemeColorOverride("font_color", UiPalette.Green);
        column.AddChild(stamp);
        column.AddChild(UiFactory.Label("PaperSubheadingLabel", $"POST-MORTEM · {data.MissionTitle.ToUpperInvariant()}"));
        column.AddChild(UiFactory.Rule(true));

        var body = UiFactory.HBox(28);
        body.SizeFlagsVertical = SizeFlags.ExpandFill;
        column.AddChild(body);

        var left = UiFactory.VBox(6);
        left.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        body.AddChild(left);
        var right = UiFactory.VBox(6);
        right.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        body.AddChild(right);

        left.AddChild(UiFactory.Label("PaperSubheadingLabel", "LEAKED"));
        if (data.Leaks.Count == 0) left.AddChild(UiFactory.Label("PaperBodyLabel", "Nothing leaked"));
        foreach (var (enemy, count) in data.Leaks)
        {
            var row = UiFactory.HBox(6);
            var icon = UiFactory.Icon(UiIcons.ForEnemyArchetype(enemy.Archetype), 22, UiPalette.Ink);
            if (icon != null) row.AddChild(icon);
            row.AddChild(UiFactory.Label("PaperNumberLabel", $"×{count}"));
            row.AddChild(UiFactory.Label("PaperBodyLabel", string.IsNullOrEmpty(enemy.DisplayName) ? enemy.Archetype.ToString() : enemy.DisplayName));
            left.AddChild(row);
        }

        left.AddChild(UiFactory.Spacer(0f, 6f));
        left.AddChild(UiFactory.Label("PaperSubheadingLabel", "MOST / LEAST EFFECTIVE TOWER"));
        if (data.Towers.Count == 0) left.AddChild(UiFactory.Label("PaperBodyLabel", "No tower damage recorded"));
        else
        {
            left.AddChild(TowerRow("Most", data.Towers[0]));
            if (data.Towers.Count > 1) left.AddChild(TowerRow("Least", data.Towers[^1]));
        }

        left.AddChild(UiFactory.Spacer(0f, 6f));
        left.AddChild(UiFactory.Label("PaperSubheadingLabel", "UNSPENT"));
        left.AddChild(UiFactory.Label("PaperBodyLabel", $"Supply {data.UnspentSupply} · Command Points {data.UnspentCommandPoints}"));

        right.AddChild(UiFactory.Label("PaperSubheadingLabel", "DAMAGE DEALT BY TYPE"));
        var chart = new DamageTypeChart { CustomMinimumSize = new Vector2(300f, 150f) };
        right.AddChild(chart);
        chart.SetValues(data.DamageByType);
        right.AddChild(UiFactory.Spacer(0f, 6f));
        right.AddChild(UiFactory.Label("PaperSubheadingLabel", "SUGGESTION"));
        right.AddChild(UiFactory.Wrapped("PaperMonoLabel", data.Suggestion));

        RetryButton = UiFactory.Button("PrimaryButton", "Retry Mission", onRetry);
        FlowScreen.ActionRow(column, RetryButton, UiFactory.Button("PaperButton", "Results", onResults));
    }

    private static Control TowerRow(string caption, (string Name, float Damage, float PerSupply) tower)
    {
        var row = UiFactory.HBox(8);
        var label = UiFactory.Label("PaperBodyLabel", caption);
        label.CustomMinimumSize = new Vector2(48f, 0f);
        row.AddChild(label);
        row.AddChild(UiFactory.Label("PaperBodyLabel", tower.Name));
        row.AddChild(UiFactory.Label("PaperNumberLabel", $"{tower.Damage:F0}"));
        row.AddChild(UiFactory.Label("PaperSmallLabel", $"{tower.PerSupply:F2} / Supply"));
        return row;
    }
}
