using Godot;
using FrontsOfWar.Map;
using System.Linq;

namespace FrontsOfWar.UI.Hud;

// The wave preview strip (GDD §10.7, §19 prompt 19) — three tiers of
// disclosure so the player can plan three waves out but only fully see one
// wave ahead. Text-only for now (no archetype icon art yet); the
// information-vs-spoiler tiering itself is the part GDD calls out as
// mechanically important, and that's what this implements.
public partial class WavePreviewPanel : CanvasLayer
{
    [Export] public NodePath MissionPath;

    private MapRuntime _mission;
    private Label _nextLabel;
    private Label _afterLabel;
    private Label _thirdLabel;

    public override void _Ready()
    {
        _mission = GetNode<MapRuntime>(MissionPath);

        var box = new HBoxContainer { Position = new Vector2(400, 60) };
        AddChild(box);

        _nextLabel = MakeCard(box, "N+1");
        _afterLabel = MakeCard(box, "N+2");
        _thirdLabel = MakeCard(box, "N+3");
    }

    private static Label MakeCard(Container parent, string title)
    {
        var panel = new PanelContainer { CustomMinimumSize = new Vector2(160, 60) };
        parent.AddChild(panel);
        var label = new Label { Text = title };
        panel.AddChild(label);
        return label;
    }

    public override void _Process(double delta) => Refresh();

    private void Refresh()
    {
        var upcoming = _mission.Waves.PeekUpcoming(3);

        // N+1: full detail — archetype + count.
        _nextLabel.Text = upcoming.Count > 0 ? DescribeFull(upcoming[0]) : "—";

        // N+2: archetype names only, no counts.
        _afterLabel.Text = upcoming.Count > 1 ? DescribeArchetypesOnly(upcoming[1]) : "—";

        // N+3: threat badges only (e.g. "Air incoming"), no composition.
        _thirdLabel.Text = upcoming.Count > 2 ? DescribeThreatBadgeOnly(upcoming[2]) : "—";
    }

    private static string DescribeFull(Waves.WaveDefinition wave)
        => $"Wave {wave.WaveNumber}\n" + string.Join("\n", wave.Groups.Select(g => $"{g.Count}x {g.Enemy.Id}"));

    private static string DescribeArchetypesOnly(Waves.WaveDefinition wave)
        => $"Wave {wave.WaveNumber}\n" + string.Join(", ", wave.Groups.Select(g => g.Enemy.Id).Distinct());

    private static string DescribeThreatBadgeOnly(Waves.WaveDefinition wave)
    {
        bool hasAir = wave.Groups.Any(g => g.Enemy.IsAir);
        return hasAir ? "⚠ Air incoming" : "Ground forces";
    }
}
