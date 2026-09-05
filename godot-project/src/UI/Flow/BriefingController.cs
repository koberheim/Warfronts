using Godot;
using System.Linq;
using FrontsOfWar.Combat;
using FrontsOfWar.Core;
using FrontsOfWar.Meta;
using FrontsOfWar.Nations;
using FrontsOfWar.UI.Theme;
using FrontsOfWar.Waves;

namespace FrontsOfWar.UI.Flow;

// Mission briefing (GDD §13.1; docs/UI_DESIGN_SPEC.md §8.2): an operation
// order on paper - stamp, title, act kicker, typewriter body, and an
// intelligence row derived from the wave sequence (wave count, known
// threats, signature available). Fictionalized framing throughout (§14.2).
public partial class BriefingController : Node2D
{
    public override void _Ready()
    {
        GameLoop.Instance?.Time.Resume();
        var mission = GD.Load<MissionDefinition>(MissionSession.CurrentMissionPath);
        if (!string.IsNullOrEmpty(mission?.Title)) MissionSession.LastMissionTitle = mission.Title;
        var nation = GD.Load<NationProfile>($"res://assets/data/nations/{MissionSession.CurrentNationId}.tres");

        var content = FlowScreen.Build(this);
        var column = FlowScreen.PaperSheet(content, 900f, 640f, Control.LayoutPreset.Center, out _);

        column.AddChild(UiFactory.Label("StampLabel", "OPERATION ORDER", uppercase: true));
        column.AddChild(UiFactory.Label("PaperTitleLabel", mission?.Title ?? MissionSession.LastMissionTitle, uppercase: true));
        string nationName = nation?.DisplayName ?? "Campaign";
        column.AddChild(UiFactory.Label("PaperSubheadingLabel", $"ACT {UiFactory.Roman(mission?.Act ?? 1)} · {nationName.ToUpperInvariant()} CAMPAIGN"));
        column.AddChild(UiFactory.Rule(true));

        var body = UiFactory.Wrapped("PaperMonoLabel", mission?.BriefingText ?? "");
        body.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        column.AddChild(body);

        column.AddChild(UiFactory.Rule(true));
        column.AddChild(IntelligenceRow(mission?.WaveSequence, nation));

        var back = UiFactory.Button("PaperButton", "Back", () => GetTree().ChangeSceneToFile("res://scenes_root/campaign_selection.tscn"));
        var next = UiFactory.Button("PrimaryButton", "Continue to Loadout", () => GetTree().ChangeSceneToFile("res://scenes_root/loadout.tscn"));
        FlowScreen.ActionRow(column, next, back);
        next.GrabFocus();
    }

    private static HBoxContainer IntelligenceRow(WaveSequence sequence, NationProfile nation)
    {
        var row = UiFactory.HBox(32);
        var waves = sequence?.Waves ?? System.Array.Empty<WaveDefinition>();
        var groups = waves.SelectMany(w => w.Groups).Where(g => g.Enemy != null).ToList();

        row.AddChild(Chip("wave", $"{waves.Length} waves", "Waves"));

        var threats = UiFactory.VBox(2);
        threats.AddChild(UiFactory.Label("PaperSmallLabel", "Known threats"));
        var badges = UiFactory.HBox(10);
        threats.AddChild(badges);
        bool armor = groups.Any(g => g.Enemy.ArmorClass is ArmorClass.Armored or ArmorClass.Heavy);
        bool air = groups.Any(g => g.Enemy.IsAir) || waves.Any(w => w.IsAirWave);
        bool boss = groups.Any(g => g.Enemy.IsBoss) || waves.Any(w => w.IsBossWave);
        bool siege = groups.Any(g => g.Enemy.Archetype == Enemies.EnemyArchetype.Siege);
        if (armor) badges.AddChild(Badge("armor_heavy", "Armor"));
        if (air) badges.AddChild(Badge("threat_air", "Air"));
        if (siege) badges.AddChild(Badge("threat_siege", "Siege"));
        if (boss) badges.AddChild(Badge("threat_boss", "Boss"));
        if (!armor && !air && !boss && !siege) badges.AddChild(UiFactory.Label("PaperBodyLabel", "Ground forces"));
        row.AddChild(threats);

        string signaturePath = nation == null ? "" : MissionCatalog.ResolveSignatureResourcePath(nation.SignatureId);
        string signature = string.IsNullOrEmpty(signaturePath)
            ? "No signature authored"
            : ResourceLoader.Load(signaturePath).Get("DisplayName").AsString();
        bool unlocked = nation != null && UnlockService.IsSignatureUnlocked(ProfileStore.Current, nation.Id);
        row.AddChild(Chip("tower_signature", unlocked ? signature : "Locked", unlocked ? "Signature ready" : $"{signature} — 1 mission"));
        return row;
    }

    private static Control Chip(string iconId, string value, string caption)
    {
        var box = UiFactory.VBox(2);
        box.AddChild(UiFactory.Label("PaperSmallLabel", caption));
        box.AddChild(Badge(iconId, value));
        return box;
    }

    private static Control Badge(string iconId, string text)
    {
        var row = UiFactory.HBox(6);
        var icon = UiFactory.Icon(iconId, 22, UiPalette.Ink);
        if (icon != null) row.AddChild(icon);
        row.AddChild(UiFactory.Label("PaperBodyLabel", text));
        return row;
    }
}
