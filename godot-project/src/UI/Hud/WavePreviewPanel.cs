using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Enemies;
using FrontsOfWar.Map;
using FrontsOfWar.UI.Theme;
using FrontsOfWar.Waves;
using System.Collections.Generic;
using System.Linq;

namespace FrontsOfWar.UI.Hud;

// HUD zone B (docs/UI_DESIGN_SPEC.md §8.4; GDD §10.7, §19 prompt 19): the
// wave heading and the teletype strip with three tiers of disclosure -
// N+1 in full (glyphs with counts, armor, threat badges), N+2 glyphs only,
// N+3 threat badges only. The information-vs-spoiler tiering is the part
// the GDD calls mechanically important; icons come from the registry and
// fall back to display names, never ids.
public partial class WavePreviewPanel : CanvasLayer
{
    [Export] public NodePath MissionPath;

    private MapRuntime _mission;
    private Label _heading;
    private readonly VBoxContainer[] _cards = new VBoxContainer[3];
    private Control _airBadge;
    private int _shownWave = -1;

    public override void _Ready()
    {
        _mission = GetNode<MapRuntime>(MissionPath);

        var column = UiFactory.VBox(4);
        column.Alignment = BoxContainer.AlignmentMode.Begin;
        AddChild(column);
        UiFactory.Anchor(column, Control.LayoutPreset.CenterTop, 0, 10);

        _heading = UiFactory.Label("HeadingLabel", "WAVE", HorizontalAlignment.Center, uppercase: true);
        column.AddChild(_heading);

        var strip = UiFactory.Panel("TeletypePanel");
        strip.CustomMinimumSize = new Vector2(860f, 72f);
        column.AddChild(strip);

        var row = UiFactory.HBox(10);
        strip.AddChild(row);
        for (int i = 0; i < _cards.Length; i++)
        {
            if (i > 0) row.AddChild(Rule());
            _cards[i] = UiFactory.VBox(2);
            _cards[i].SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _cards[i].SizeFlagsStretchRatio = i == 0 ? 1.5f : 1f;
            row.AddChild(_cards[i]);
        }
        _airBadge = (Control)UiFactory.Icon("air_warning", 28, UiPalette.Amber) ?? UiFactory.Label("PaperMonoLabel", "AIR");
        _airBadge.TooltipText = "Air units are coming within the next three waves";
        _airBadge.Visible = false;
        row.AddChild(_airBadge);

        EventBus.Instance?.Subscribe<WaveStartedEvent>(OnWaveStarted);
        EventBus.Instance?.Subscribe<BuildPhaseStartedEvent>(OnBuildPhaseStarted);
        Callable.From(Refresh).CallDeferred();
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
        EventBus.Instance?.Unsubscribe<BuildPhaseStartedEvent>(OnBuildPhaseStarted);
    }

    private static ColorRect Rule() => new()
    {
        CustomMinimumSize = new Vector2(1f, 0f),
        Color = UiPalette.InkMuted with { A = 0.35f },
        MouseFilter = Control.MouseFilterEnum.Ignore,
    };

    private void OnWaveStarted(WaveStartedEvent evt)
    {
        Refresh();
        UiFactory.Pulse(_heading);
    }

    private void OnBuildPhaseStarted(BuildPhaseStartedEvent evt) => Refresh();

    private void Refresh()
    {
        if (_mission?.Waves == null) return;
        int current = _mission.Waves.CurrentWaveNumber;
        int total = Mathf.Max(_mission.TotalWaves, current);
        _heading.Text = total > 0 ? $"WAVE {current} / {total}" : $"WAVE {current}";

        var upcoming = _mission.Waves.PeekUpcoming(3);
        FillCard(_cards[0], upcoming.Count > 0 ? upcoming[0] : null, Tier.Full, "NEXT");
        FillCard(_cards[1], upcoming.Count > 1 ? upcoming[1] : null, Tier.ArchetypesOnly, "THEN");
        FillCard(_cards[2], upcoming.Count > 2 ? upcoming[2] : null, Tier.ThreatsOnly, "AFTER");
        _airBadge.Visible = upcoming.Any(HasAir);
        _shownWave = current;
    }

    private enum Tier { Full, ArchetypesOnly, ThreatsOnly }

    private void FillCard(VBoxContainer card, WaveDefinition wave, Tier tier, string kicker)
    {
        foreach (var child in card.GetChildren()) child.QueueFree();

        if (wave == null)
        {
            card.AddChild(UiFactory.Label("PaperMonoLabel", kicker == "NEXT" ? "NO FURTHER WAVES" : "—"));
            return;
        }

        card.AddChild(UiFactory.Label("PaperMonoLabel", $"{kicker} · WAVE {wave.WaveNumber}"));
        var row = UiFactory.HBox(6);
        card.AddChild(row);

        if (tier != Tier.ThreatsOnly)
        {
            var groups = tier == Tier.Full
                ? wave.Groups.Where(g => g.Enemy != null).ToList()
                : wave.Groups.Where(g => g.Enemy != null).GroupBy(g => g.Enemy.Archetype).Select(g => g.First()).ToList();
            foreach (var group in groups)
            {
                string name = string.IsNullOrEmpty(group.Enemy.DisplayName) ? group.Enemy.Archetype.ToString() : group.Enemy.DisplayName;
                var icon = UiFactory.Icon(UiIcons.ForEnemyArchetype(group.Enemy.Archetype), 22, UiPalette.Ink);
                if (icon != null) { icon.TooltipText = name; row.AddChild(icon); }
                else row.AddChild(UiFactory.Label("PaperMonoLabel", name));
                if (tier == Tier.Full) row.AddChild(UiFactory.Label("PaperMonoLabel", $"×{group.Count}"));
            }
        }

        if (tier == Tier.Full)
            foreach (var armor in wave.Groups.Where(g => g.Enemy != null).Select(g => g.Enemy.ArmorClass).Distinct())
                AddBadge(row, UiIcons.ForArmorClass(armor), $"{armor} armor");

        var badges = ThreatBadges(wave).ToList();
        if (tier == Tier.ThreatsOnly && badges.Count == 0)
            row.AddChild(UiFactory.Label("PaperMonoLabel", "Ground forces"));
        foreach (var (id, label) in badges) AddBadge(row, id, label);
    }

    private static void AddBadge(Container row, string iconId, string label)
    {
        var icon = UiFactory.Icon(iconId, 20, UiPalette.InkMuted);
        if (icon != null) { icon.TooltipText = label; row.AddChild(icon); }
        else row.AddChild(UiFactory.Label("PaperMonoLabel", label));
    }

    private static bool HasAir(WaveDefinition wave) => wave.IsAirWave || wave.Groups.Any(g => g.Enemy?.IsAir == true);

    private static IEnumerable<(string, string)> ThreatBadges(WaveDefinition wave)
    {
        var archetypes = wave.Groups.Where(g => g.Enemy != null).Select(g => g.Enemy.Archetype).ToHashSet();
        if (HasAir(wave)) yield return ("threat_air", "Air incoming");
        if (archetypes.Contains(EnemyArchetype.Siege)) yield return ("threat_siege", "Siege guns");
        if (archetypes.Contains(EnemyArchetype.Support) || archetypes.Contains(EnemyArchetype.Escort)) yield return ("threat_support", "Support vehicles");
        if (archetypes.Contains(EnemyArchetype.Recon)) yield return ("threat_concealed", "Concealed units");
        if (wave.IsBossWave || wave.Groups.Any(g => g.Enemy?.IsBoss == true)) yield return ("threat_boss", "Boss");
    }
}
