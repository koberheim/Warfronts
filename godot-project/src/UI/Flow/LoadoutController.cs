using Godot;
using System.Collections.Generic;
using System.Linq;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Doctrines;
using FrontsOfWar.Meta;
using FrontsOfWar.Nations;
using FrontsOfWar.Towers;
using FrontsOfWar.UI.Hud;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Flow;

// Loadout screen (GDD §13.3, §19 prompt 39; docs/UI_DESIGN_SPEC.md §8.3).
// The six tower cards are the fixed recommended kit until §13.3's picker
// lands; the doctrine cards are a live toggle group. The AP/AA warning
// banner is information, not a block. United States only, since no other
// nation is selectable yet (see MapRuntime's doctrine note).
public partial class LoadoutController : Node2D
{
    private readonly List<TowerDefinition> _kit = new();

    public override void _Ready()
    {
        GameLoop.Instance?.Time.Resume();
        var mission = GD.Load<MissionDefinition>(MissionSession.CurrentMissionPath);
        var nation = GD.Load<NationProfile>($"res://assets/data/nations/{MissionSession.CurrentNationId}.tres");
        foreach (var path in MissionSession.Loadout)
        {
            var definition = GD.Load<TowerDefinition>(path);
            if (definition != null) _kit.Add(definition);
        }

        var content = FlowScreen.Build(this);
        var margin = UiFactory.Margin(230, 40, 120, 40);
        margin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        content.AddChild(margin);
        var column = UiFactory.VBox(12);
        margin.AddChild(column);

        var titleRow = UiFactory.HBox(12);
        column.AddChild(titleRow);
        titleRow.AddChild(UiFactory.Label("PaperTitleLabel", "LOADOUT", uppercase: true));
        titleRow.AddChild(UiFactory.Spacer(expand: true));
        var roundel = UiFactory.Icon($"nation_{MissionSession.CurrentNationId}", 40, UiPalette.Ink);
        if (roundel != null) titleRow.AddChild(roundel);
        var nationLabel = UiFactory.Label("PaperHeadingLabel", nation?.DisplayName ?? MissionSession.CurrentNationId, uppercase: true);
        nationLabel.VerticalAlignment = VerticalAlignment.Center;
        titleRow.AddChild(nationLabel);
        column.AddChild(UiFactory.Rule(true));

        var body = UiFactory.HBox(40);
        body.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        column.AddChild(body);
        body.AddChild(TowerColumn());
        body.AddChild(DoctrineColumn(nation));

        var warning = WarningBanner(mission);
        if (warning != null) column.AddChild(warning);

        var back = UiFactory.Button("PaperButton", "Back", () => GetTree().ChangeSceneToFile("res://scenes_root/briefing.tscn"));
        var deploy = UiFactory.Button("PrimaryButton", $"Deploy to {mission?.Title ?? MissionSession.LastMissionTitle}",
            () => GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn"));
        FlowScreen.ActionRow(column, deploy, back);
        deploy.GrabFocus();
    }

    private Control TowerColumn()
    {
        var box = UiFactory.VBox(8);
        box.AddChild(UiFactory.Label("PaperSubheadingLabel", "RECOMMENDED KIT · SIX TOWERS"));
        box.AddChild(UiFactory.Wrapped("PaperSmallLabel", "Build these from the in-mission build bar (Q–Y). The signature factory is pre-placed on the map."));
        var grid = new GridContainer { Columns = 3 };
        grid.AddThemeConstantOverride("h_separation", 8);
        grid.AddThemeConstantOverride("v_separation", 8);
        box.AddChild(grid);
        var hotkeys = new[] { "Q", "W", "E", "R", "T", "Y" };
        for (int i = 0; i < _kit.Count; i++) grid.AddChild(TowerCard(_kit[i], hotkeys[Mathf.Min(i, hotkeys.Length - 1)]));
        return box;
    }

    private static Control TowerCard(TowerDefinition definition, string hotkey)
    {
        var card = UiFactory.Panel("PaperPanel");
        card.CustomMinimumSize = new Vector2(216f, 176f);
        var column = UiFactory.VBox(4);
        card.AddChild(column);

        var header = UiFactory.HBox(6);
        column.AddChild(header);
        var icon = UiFactory.Icon(UiIcons.ForTower(definition.Id), 36, UiPalette.Ink);
        if (icon != null) header.AddChild(icon);
        header.AddChild(UiFactory.Spacer(expand: true));
        header.AddChild(UiFactory.Chip(hotkey, card));

        column.AddChild(UiFactory.Wrapped("PaperCardHeadingLabel", definition.DisplayName));
        var stats = definition.PreForkStatsForLevel(1);
        var typeRow = UiFactory.HBox(4);
        var glyph = UiFactory.Icon(UiIcons.ForDamageType(definition.DamageType), 16, UiPalette.Ink);
        if (glyph != null) typeRow.AddChild(glyph);
        typeRow.AddChild(UiFactory.Label("PaperSmallLabel", MatchupRules.DamageTypeName(definition.DamageType)));
        column.AddChild(typeRow);
        var costRow = UiFactory.HBox(4);
        var supply = UiFactory.Icon("resource_supply", 16, UiPalette.Ink);
        if (supply != null) costRow.AddChild(supply);
        costRow.AddChild(UiFactory.Label("PaperNumberLabel", stats.Cost.ToString()));
        column.AddChild(costRow);
        return card;
    }

    private Control DoctrineColumn(NationProfile nation)
    {
        var box = UiFactory.VBox(8);
        box.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        box.AddChild(UiFactory.Label("PaperSubheadingLabel", "DOCTRINE · PICK ONE"));
        var row = UiFactory.HBox(8);
        box.AddChild(row);
        var group = new ButtonGroup();
        var ids = nation?.DoctrineIds is { Length: > 0 } list ? list : new[] { "lend_lease", "airborne", "combined_arms" };
        foreach (var id in ids)
        {
            var doctrine = DoctrineSystem.LoadDoctrine(MissionSession.CurrentNationId, id);
            var card = DoctrineCard(doctrine, id, group);
            row.AddChild(card);
        }

        var difficulty = UiFactory.HBox(8);
        difficulty.AddChild(UiFactory.Label("PaperBodyLabel", "Difficulty"));
        var regular = UiFactory.Button("PaperButton", "Regular");
        regular.Disabled = true;
        regular.TooltipText = "Difficulty selection arrives with prompt 42";
        difficulty.AddChild(regular);
        box.AddChild(difficulty);
        return box;
    }

    private static Button DoctrineCard(DoctrineDefinition doctrine, string id, ButtonGroup group)
    {
        var card = UiFactory.Button("CardButton", "");
        card.ToggleMode = true;
        card.ButtonGroup = group;
        card.CustomMinimumSize = new Vector2(260f, 220f);
        card.SetPressedNoSignal(MissionSession.SelectedDoctrineId == id);
        card.Pressed += () => MissionSession.SelectedDoctrineId = id;

        var margin = UiFactory.Margin(12, 10, 12, 10);
        margin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        card.AddChild(margin);
        var column = UiFactory.VBox(6);
        margin.AddChild(column);
        var header = UiFactory.HBox(6);
        column.AddChild(header);
        header.AddChild(UiFactory.Label("HeadingLabel", doctrine?.DisplayName ?? id, uppercase: true));
        header.AddChild(UiFactory.Spacer(expand: true));
        var check = UiFactory.Icon("check", 20, UiPalette.Amber);
        if (check != null)
        {
            check.Visible = card.ButtonPressed;
            header.AddChild(check);
            card.Toggled += pressed => check.Visible = pressed;
        }
        column.AddChild(UiFactory.Wrapped("SmallLabel", $"Passive: {doctrine?.PassiveDescription}"));
        column.AddChild(UiFactory.Wrapped("SmallLabel", $"Ability [{doctrine?.AbilityName}]: {doctrine?.AbilityDescription}"));
        Hud.TowerCard.IgnoreMouse(margin);
        return card;
    }

    // GDD §13.3: "A warning banner appears if the loadout has no AP source
    // or no AA source and the mission contains armor or air."
    private Control WarningBanner(MissionDefinition mission)
    {
        var groups = mission?.WaveSequence?.Waves?.SelectMany(w => w.Groups).Where(g => g.Enemy != null).ToList();
        if (groups == null) return null;
        bool armor = groups.Any(g => g.Enemy.ArmorClass is ArmorClass.Armored or ArmorClass.Heavy);
        bool air = groups.Any(g => g.Enemy.IsAir);
        bool hasAp = _kit.Any(t => t.DamageType == DamageType.ArmorPiercing);
        bool hasAa = _kit.Any(t => t.DamageType == DamageType.AntiAir || t.AirOnly);
        var lines = new List<string>();
        if (armor && !hasAp) lines.Add("This mission includes armored units. You have no armor-piercing tower selected.");
        if (air && !hasAa) lines.Add("This mission includes air units. You have no anti-air tower selected.");
        if (lines.Count == 0) return null;

        var banner = UiFactory.Panel("SlatePanelStrong");
        var row = UiFactory.HBox(10);
        banner.AddChild(row);
        var icon = UiFactory.Icon("air_warning", 24, UiPalette.Amber);
        if (icon != null) row.AddChild(icon);
        row.AddChild(UiFactory.Wrapped("BodyLabel", string.Join("  ", lines)));
        return banner;
    }
}
