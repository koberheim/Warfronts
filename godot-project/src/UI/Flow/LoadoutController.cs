using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Doctrines;
using FrontsOfWar.Meta;
using FrontsOfWar.Nations;
using FrontsOfWar.Towers;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Flow;

// GDD §13.3. BuildSlotResourcePaths is the six-slot source of truth. A
// signature is a Resource path alongside tower paths, never a seventh card.
// The current build bar still reads its TowerDefinition-only projection.
public partial class LoadoutController : Node2D
{
    private readonly List<MissionCatalog.Entry<TowerDefinition>> _roster = new();
    private readonly Dictionary<string, Button> _towerButtons = new();
    private Label _slotsLabel;
    private Label _warning;
    private Label _signature;
    private Button _signatureButton;
    private Button _deploy;
    private MissionDefinition _mission;
    private NationProfile _nation;

    public override void _Ready()
    {
        GameLoop.Instance?.Time.Resume();
        _mission = GD.Load<MissionDefinition>(MissionSession.CurrentMissionPath);
        _nation = GD.Load<NationProfile>($"res://assets/data/nations/{MissionSession.CurrentNationId}.tres");
        _roster.AddRange(MissionCatalog.DiscoverBuildTowers());
        EnsureSelectionIsValid();
        BuildScreen();
    }

    private void BuildScreen()
    {
        var content = FlowScreen.Build(this);
        var margin = UiFactory.Margin(150, 34, 110, 34);
        margin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        content.AddChild(margin);
        var column = UiFactory.VBox(10);
        margin.AddChild(column);

        var heading = UiFactory.HBox(12);
        column.AddChild(heading);
        heading.AddChild(UiFactory.Label("PaperTitleLabel", "LOADOUT", uppercase: true));
        heading.AddChild(UiFactory.Spacer(expand: true));
        heading.AddChild(UiFactory.Label("PaperHeadingLabel", _nation?.DisplayName ?? MissionSession.CurrentNationId, uppercase: true));
        column.AddChild(UiFactory.Rule(true));

        var body = UiFactory.HBox(28);
        body.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        column.AddChild(body);
        body.AddChild(BuildTowerSection());
        body.AddChild(BuildCommandSection());

        _warning = UiFactory.Wrapped("PaperSmallLabel", "");
        _warning.AddThemeColorOverride("font_color", UiPalette.Amber);
        column.AddChild(_warning);

        var back = UiFactory.Button("PaperButton", "Back", () => GetTree().ChangeSceneToFile("res://scenes_root/briefing.tscn"));
        _deploy = UiFactory.Button("PrimaryButton", $"Deploy to {_mission?.Title ?? MissionSession.LastMissionTitle}",
            () => GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn"));
        FlowScreen.ActionRow(column, _deploy, back);
        RefreshSelection();
        _deploy.GrabFocus();
    }

    private Control BuildTowerSection()
    {
        var box = UiFactory.VBox(8);
        box.CustomMinimumSize = new Vector2(680f, 0f);
        box.AddChild(UiFactory.Label("PaperSubheadingLabel", "BUILD SLOTS — CHOOSE SIX"));
        _slotsLabel = UiFactory.Wrapped("PaperBodyLabel", "");
        box.AddChild(_slotsLabel);
        box.AddChild(UiFactory.Button("PaperButton", "Recommended Loadout", SelectRecommended));

        var grid = new GridContainer { Columns = 3 };
        grid.AddThemeConstantOverride("h_separation", 8);
        grid.AddThemeConstantOverride("v_separation", 8);
        box.AddChild(grid);
        foreach (var entry in _roster)
        {
            var button = UiFactory.Button("PaperButton", "", () => ToggleTower(entry));
            button.CustomMinimumSize = new Vector2(205f, 74f);
            button.TooltipText = "Add or remove from the six build slots";
            _towerButtons[entry.Path] = button;
            grid.AddChild(button);
        }
        return box;
    }

    private Control BuildCommandSection()
    {
        var box = UiFactory.VBox(12);
        box.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        box.AddChild(UiFactory.Label("PaperSubheadingLabel", "COMMAND PLAN"));
        box.AddChild(DoctrineSection());
        box.AddChild(DifficultySection());

        var signatureBox = UiFactory.Panel("PaperPanel");
        var signatureColumn = UiFactory.VBox(4);
        signatureBox.AddChild(signatureColumn);
        signatureColumn.AddChild(UiFactory.Label("PaperSubheadingLabel", "NATIONAL SIGNATURE SLOT"));
        _signature = UiFactory.Wrapped("PaperBodyLabel", "");
        signatureColumn.AddChild(_signature);
        _signatureButton = UiFactory.Button("PaperButton", "", ToggleSignature);
        signatureColumn.AddChild(_signatureButton);
        box.AddChild(signatureBox);
        return box;
    }

    private Control DoctrineSection()
    {
        var box = UiFactory.VBox(5);
        box.AddChild(UiFactory.Label("PaperBodyLabel", "Doctrine"));
        var group = new ButtonGroup();
        var profile = ProfileStore.Current;
        var ids = _nation?.DoctrineIds ?? Array.Empty<string>();
        for (int index = 0; index < ids.Length; index++)
        {
            string id = ids[index];
            var doctrine = DoctrineSystem.LoadDoctrine(MissionSession.CurrentNationId, id);
            bool unlocked = UnlockService.IsDoctrineUnlocked(profile, MissionSession.CurrentNationId, index);
            var button = UiFactory.Button("PaperButton", doctrine?.DisplayName ?? id, () => MissionSession.SelectedDoctrineId = id);
            button.ToggleMode = true;
            button.ButtonGroup = group;
            button.Disabled = !unlocked;
            button.TooltipText = unlocked ? doctrine?.PassiveDescription ?? "" : $"Complete {(index == 1 ? 2 : 4)} missions with {_nation?.DisplayName}.";
            button.SetPressedNoSignal(unlocked && MissionSession.SelectedDoctrineId == id);
            box.AddChild(button);
        }
        return box;
    }

    private Control DifficultySection()
    {
        var box = UiFactory.VBox(5);
        box.AddChild(UiFactory.Label("PaperBodyLabel", "Difficulty"));
        var row = UiFactory.HBox(6);
        box.AddChild(row);
        var group = new ButtonGroup();
        var profile = ProfileStore.Current;
        foreach (Difficulty difficulty in Enum.GetValues<Difficulty>())
        {
            bool unlocked = UnlockService.IsDifficultyUnlocked(profile, difficulty);
            var button = UiFactory.Button("PaperButton", difficulty.ToString(), () => MissionSession.SelectedDifficulty = difficulty);
            button.ToggleMode = true;
            button.ButtonGroup = group;
            button.Disabled = !unlocked;
            button.TooltipText = unlocked ? "" : difficulty == Difficulty.Veteran
                ? "Complete any mission on Regular." : "Complete any mission on Veteran.";
            button.SetPressedNoSignal(unlocked && MissionSession.SelectedDifficulty == difficulty);
            row.AddChild(button);
        }
        return box;
    }

    private void ToggleTower(MissionCatalog.Entry<TowerDefinition> entry)
    {
        if (MissionSession.BuildSlotResourcePaths.Remove(entry.Path)) { SyncTowerProjection(); RefreshSelection(); return; }
        if (MissionSession.BuildSlotResourcePaths.Count >= 6)
        {
            _warning.Text = "Six build slots are already occupied. Remove a tower before adding another.";
            return;
        }
        MissionSession.BuildSlotResourcePaths.Add(entry.Path);
        SyncTowerProjection();
        RefreshSelection();
    }

    private void ToggleSignature()
    {
        string path = AvailableSignaturePath();
        if (string.IsNullOrEmpty(path)) return;
        if (MissionSession.BuildSlotResourcePaths.Remove(path)) { SyncTowerProjection(); RefreshSelection(); return; }
        if (MissionSession.BuildSlotResourcePaths.Count >= 6)
        {
            _warning.Text = "Six build slots are already occupied. Remove a tower before adding the signature.";
            return;
        }
        MissionSession.BuildSlotResourcePaths.Add(path);
        SyncTowerProjection();
        RefreshSelection();
    }

    private void SelectRecommended()
    {
        SetRecommendedBuildSlots();
        RefreshSelection();
    }

    private void SetRecommendedBuildSlots()
    {
        string signature = AvailableSignaturePath();
        MissionSession.BuildSlotResourcePaths = _roster.Take(string.IsNullOrEmpty(signature) ? 6 : 5)
            .Select(entry => entry.Path).ToList();
        if (!string.IsNullOrEmpty(signature)) MissionSession.BuildSlotResourcePaths.Add(signature);
        SyncTowerProjection();
    }

    private void EnsureSelectionIsValid()
    {
        string signature = AvailableSignaturePath();
        var validPaths = new HashSet<string>(_roster.Select(entry => entry.Path));
        if (!string.IsNullOrEmpty(signature)) validPaths.Add(signature);
        MissionSession.BuildSlotResourcePaths = MissionSession.BuildSlotResourcePaths
            .Where(validPaths.Contains).Distinct().Take(6).ToList();
        if (MissionSession.BuildSlotResourcePaths.Count != 6) SetRecommendedBuildSlots();

        var profile = ProfileStore.Current;
        var doctrines = _nation?.DoctrineIds ?? Array.Empty<string>();
        int selectedDoctrine = Array.IndexOf(doctrines, MissionSession.SelectedDoctrineId);
        if (selectedDoctrine < 0 || !UnlockService.IsDoctrineUnlocked(profile, MissionSession.CurrentNationId, selectedDoctrine))
            MissionSession.SelectedDoctrineId = doctrines.FirstOrDefault(id =>
                UnlockService.IsDoctrineUnlocked(profile, MissionSession.CurrentNationId, Array.IndexOf(doctrines, id))) ?? "";

        if (!UnlockService.IsDifficultyUnlocked(profile, MissionSession.SelectedDifficulty))
            MissionSession.SelectedDifficulty = Difficulty.Regular;

        SyncTowerProjection();
    }

    private void RefreshSelection()
    {
        var selected = MissionSession.BuildSlotResourcePaths;
        _slotsLabel.Text = string.Join("  ", Enumerable.Range(0, 6).Select(index =>
        {
            if (index >= selected.Count) return $"{index + 1}. Empty";
            string path = selected[index];
            var resource = ResourceLoader.Load(path);
            return resource == null ? $"{index + 1}. Empty" : $"{index + 1}. {resource.Get("DisplayName").AsString()}";
        }));
        foreach (var entry in _roster)
        {
            bool chosen = selected.Contains(entry.Path);
            _towerButtons[entry.Path].Text = $"{entry.Resource.DisplayName}\n{DamageName(entry.Resource.DamageType)}{(chosen ? "  ✓" : "")}";
        }

        bool armor = _mission?.WaveSequence?.Waves?.SelectMany(wave => wave.Groups).Any(group =>
            group.Enemy?.ArmorClass is ArmorClass.Armored or ArmorClass.Heavy) == true;
        bool air = _mission?.WaveSequence?.Waves?.SelectMany(wave => wave.Groups).Any(group => group.Enemy?.IsAir == true) == true;
        var kit = _roster.Where(entry => selected.Contains(entry.Path)).Select(entry => entry.Resource).ToList();
        var warnings = new List<string>();
        if (armor && !kit.Any(tower => tower.DamageType == DamageType.ArmorPiercing)) warnings.Add("This mission includes armor. No armor-piercing tower is selected.");
        if (air && !kit.Any(tower => tower.DamageType == DamageType.AntiAir || tower.AirOnly)) warnings.Add("This mission includes air units. No anti-air tower is selected.");
        _warning.Text = string.Join("  ", warnings);
        _deploy.Disabled = selected.Count != 6;

        string signaturePath = AvailableSignaturePath();
        if (string.IsNullOrEmpty(signaturePath))
        {
            _signature.Text = "Locked — complete one mission with this nation to unlock its signature.";
            _signatureButton.Text = "Signature locked";
            _signatureButton.Disabled = true;
        }
        else
        {
            var resource = ResourceLoader.Load(signaturePath);
            string name = resource.Get("DisplayName").AsString();
            bool selectedSignature = selected.Contains(signaturePath);
            _signature.Text = selectedSignature
                ? $"Selected: {name}. This occupies one of the six build slots."
                : $"Available: {name}. Add it by replacing one of the six tower slots.";
            _signatureButton.Text = selectedSignature ? "Remove signature" : "Add signature to a build slot";
            _signatureButton.Disabled = false;
        }
    }

    private string AvailableSignaturePath()
        => _nation != null && UnlockService.IsSignatureUnlocked(ProfileStore.Current, _nation.Id)
            ? MissionCatalog.ResolveSignatureResourcePath(_nation.SignatureId)
            : "";

    // BuildBar has not yet migrated to generic Resources. Keep it from
    // attempting to cast a signature while the parent runtime adds dispatch.
    private void SyncTowerProjection()
    {
        var towerPaths = new HashSet<string>(_roster.Select(entry => entry.Path));
        MissionSession.Loadout = MissionSession.BuildSlotResourcePaths.Where(towerPaths.Contains).ToList();
    }

    private static string DamageName(DamageType type) => type switch
    {
        DamageType.SmallArms => "Small Arms",
        DamageType.Explosive => "Explosive",
        DamageType.ArmorPiercing => "Armor-Piercing",
        DamageType.AntiAir => "Anti-Air",
        _ => type.ToString(),
    };
}
