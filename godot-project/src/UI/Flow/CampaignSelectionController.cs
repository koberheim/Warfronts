using System;
using System.Linq;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Meta;
using FrontsOfWar.Nations;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Flow;

// GDD §9.1 / §13.1: alliance first, nation second, then the authored campaign
// map. The map list intentionally contains only real MissionDefinitions.
public partial class CampaignSelectionController : Node2D
{
    private enum SelectionStage { Alliance, Nation, CampaignMap }

    private SelectionStage _stage;

    public override void _Ready()
    {
        GameLoop.Instance?.Time.Resume();
        BuildScreen();
    }

    private void BuildScreen()
    {
        var content = FlowScreen.Build(this);
        var column = FlowScreen.PaperSheet(content, 1240f, 760f, Control.LayoutPreset.Center, out _, 32, 24, 12);
        column.AddChild(UiFactory.Label("StampLabel", _stage == SelectionStage.CampaignMap ? "CAMPAIGN MAP" : "CAMPAIGN SELECTION", uppercase: true));
        column.AddChild(UiFactory.Label("PaperTitleLabel", _stage switch
        {
            SelectionStage.Alliance => "CHOOSE YOUR ALLIANCE",
            SelectionStage.Nation => $"{MissionSession.SelectedAllianceId.ToUpperInvariant()} COMMAND",
            _ => "THE FRONT",
        }, uppercase: true));
        column.AddChild(UiFactory.Wrapped("PaperSmallLabel", _stage switch
        {
            SelectionStage.Alliance => "Choose the side whose campaign framing you want to follow.",
            SelectionStage.Nation => "Choose a nation. It changes its signature and doctrine choices, not map access.",
            _ => "Only authored operations are shown. Locked operations open after the preceding mission is completed.",
        }));
        column.AddChild(UiFactory.Rule(true));

        switch (_stage)
        {
            case SelectionStage.Alliance: BuildAllianceSelection(column); break;
            case SelectionStage.Nation: BuildNationSelection(column); break;
            default: BuildMissionList(column); break;
        }
    }

    private void BuildAllianceSelection(VBoxContainer column)
    {
        column.AddChild(UiFactory.Label("PaperSubheadingLabel", "1. ALLIANCE"));
        var cards = UiFactory.HBox(18);
        cards.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        cards.Alignment = BoxContainer.AlignmentMode.Center;
        column.AddChild(cards);
        foreach (string alliance in new[] { "Allies", "Axis" })
        {
            var (card, _) = BuildBannerCard(
                $"res://assets/art/shared/ui/flags/{alliance.ToLowerInvariant()}_banner_v01.png",
                new Vector2(320f, 170f), alliance.ToUpperInvariant(), "Continue to nation selection",
                () => SelectAlliance(alliance));
            cards.AddChild(card);
        }

        column.AddChild(UiFactory.Button("PaperButton", "Back", () => GetTree().ChangeSceneToFile("res://scenes_root/main_menu.tscn")));
    }

    private void BuildNationSelection(VBoxContainer column)
    {
        column.AddChild(UiFactory.Label("PaperSubheadingLabel", "2. NATION"));
        var nations = MissionCatalog.NationsForAlliance(MissionSession.SelectedAllianceId);
        var cards = new HBoxContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        cards.AddThemeConstantOverride("separation", 14);
        column.AddChild(cards);
        foreach (var entry in nations) cards.AddChild(NationCard(entry));

        var actions = UiFactory.HBox(10);
        column.AddChild(actions);
        actions.AddChild(UiFactory.Button("PaperButton", "Back to Alliance", () => { _stage = SelectionStage.Alliance; Rebuild(); }));
    }

    private Control NationCard(MissionCatalog.Entry<NationProfile> entry)
    {
        var nation = entry.Resource;
        string signaturePath = MissionCatalog.ResolveSignatureResourcePath(nation.SignatureId);
        string signatureName = string.IsNullOrEmpty(signaturePath)
            ? nation.SignatureId
            : ResourceLoader.Load(signaturePath).Get("DisplayName").AsString();
        string leans = nation.StatLeans.Length == 0
            ? "Balanced roster"
            : string.Join(", ", nation.StatLeans.Select(lean => lean.StatId.Replace('_', ' ')));

        var (card, title) = BuildBannerCard(NationBannerPath(nation.Id), new Vector2(280f, 275f),
            nation.DisplayName, "Select nation", () => SelectNation(nation),
            $"Signature: {signatureName}\nDoctrines: {nation.DoctrineIds.Length}\nStat leans: {leans}");
        if (nation.Id == MissionSession.CurrentNationId) title.AddThemeColorOverride("font_color", UiPalette.Amber);
        return card;
    }

    private static string NationBannerPath(string nationId) => string.IsNullOrEmpty(nationId)
        ? null : $"res://assets/art/shared/ui/flags/nation_banner_{nationId}_v01.png";

    // Fictionalized nation/alliance banners (GDD §14.3; see docs/DECISIONS.md
    // D85). The button *is* the banner - a borderless TextureButton, not a
    // themed PaperButton with the image inset inside it - with the label(s)
    // living underneath as plain (non-interactive) text, per the User's
    // request. A faint brighten on hover stands in for the PaperButton
    // theme's own hover feedback, which a bare TextureButton doesn't have.
    private static (Control Card, Label Title) BuildBannerCard(string texturePath, Vector2 bannerSize,
        string title, string tooltip, Action onPressed, string subtitle = null)
    {
        var column = UiFactory.VBox(6);
        column.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;

        var texture = string.IsNullOrEmpty(texturePath) ? null : GD.Load<Texture2D>(texturePath);
        var button = new TextureButton
        {
            TextureNormal = texture,
            IgnoreTextureSize = true,
            StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = bannerSize,
            TooltipText = tooltip,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
        };
        button.Pressed += onPressed;
        button.MouseEntered += () => button.Modulate = new Color(1.15f, 1.15f, 1.1f);
        button.MouseExited += () => button.Modulate = Colors.White;
        column.AddChild(button);

        var titleLabel = UiFactory.Label("SubheadingLabel", title, HorizontalAlignment.Center, uppercase: true);
        column.AddChild(titleLabel);
        if (subtitle != null)
            column.AddChild(UiFactory.Label("SmallLabel", subtitle, HorizontalAlignment.Center));

        return (column, titleLabel);
    }

    private void BuildMissionList(VBoxContainer column)
    {
        var missions = MissionCatalog.DiscoverMissions();
        var profile = ProfileStore.Current;
        var list = UiFactory.VBox(10);
        list.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        column.AddChild(list);
        foreach (var mission in missions)
        {
            bool unlocked = MissionCatalog.IsCampaignMissionUnlocked(profile, mission, missions);
            string text = $"ACT {UiFactory.Roman(mission.Resource.Act)}  {mission.Resource.Title}";
            if (!unlocked) text += "  — Complete the previous mission";
            var button = UiFactory.Button(unlocked ? "PaperButton" : "PaperButton", text, unlocked ? () => StartMission(mission) : null);
            button.Disabled = !unlocked;
            button.CustomMinimumSize = new Vector2(0f, 58f);
            list.AddChild(button);
        }
        if (missions.Count == 0) list.AddChild(UiFactory.Label("PaperBodyLabel", "No authored campaign operations are available."));

        column.AddChild(UiFactory.Button("PaperButton", "Back to Nation Selection", () => { _stage = SelectionStage.Nation; Rebuild(); }));
    }

    private void SelectAlliance(string alliance)
    {
        MissionSession.SelectedAllianceId = alliance;
        _stage = SelectionStage.Nation;
        Rebuild();
    }

    private void SelectNation(NationProfile nation)
    {
        MissionSession.CurrentNationId = nation.Id;
        MissionSession.SelectedAllianceId = nation.Alliance;
        MissionSession.SelectedDoctrineId = nation.DoctrineIds.FirstOrDefault() ?? "";
        _stage = SelectionStage.CampaignMap;
        Rebuild();
    }

    private void StartMission(MissionCatalog.Entry<MissionDefinition> mission)
    {
        MissionSession.CurrentMissionPath = mission.Path;
        GetTree().ChangeSceneToFile("res://scenes_root/briefing.tscn");
    }

    private void Rebuild()
    {
        foreach (var child in GetChildren()) child.QueueFree();
        Callable.From(BuildScreen).CallDeferred();
    }
}
