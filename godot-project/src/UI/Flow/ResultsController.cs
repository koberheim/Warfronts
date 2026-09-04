using Godot;
using System.Collections.Generic;
using FrontsOfWar.Core;
using FrontsOfWar.Meta;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Flow;

// Results sheet (GDD §13.7, §19 prompt 41; docs/UI_DESIGN_SPEC.md §8.9).
// Records the finished mission into the saved profile (stars, Faction
// Mastery XP, unlock deltas), shows stars with their objective captions, the
// mastery rank/XP bar and unlock lines, then offers the one-click Retry as
// the default focused action.
public partial class ResultsController : Node2D
{
    public override void _Ready()
    {
        GameLoop.Instance?.Time.Resume();
        var mission = GD.Load<MissionDefinition>(MissionSession.CurrentMissionPath);
        var config = GameBalanceConfigAutoload.Config;
        var profile = ProfileStore.Current;
        var summary = RecordProgression(mission, profile, config);

        var content = FlowScreen.Build(this);
        var column = FlowScreen.PaperSheet(content, 900f, 680f, Control.LayoutPreset.Center, out _, 0, 0, 12);

        bool won = MissionSession.LastMissionWon;
        var stamp = UiFactory.Label("StampLabel", won ? "MISSION COMPLETE" : "MISSION FAILED", uppercase: true);
        if (won) stamp.AddThemeColorOverride("font_color", UiPalette.Green);
        column.AddChild(stamp);
        column.AddChild(UiFactory.Label("PaperTitleLabel", MissionSession.LastMissionTitle, uppercase: true));
        int totalWaves = mission?.WaveSequence?.Waves?.Length ?? 0;
        column.AddChild(UiFactory.Label("PaperSubheadingLabel", totalWaves > 0
            ? $"WAVE REACHED {MissionSession.LastWaveReached} / {totalWaves}"
            : $"WAVE REACHED {MissionSession.LastWaveReached}"));
        column.AddChild(UiFactory.Rule(true));

        column.AddChild(StarRow(summary?.StarsEarnedThisRun ?? new bool[3], mission?.StarObjective));
        column.AddChild(UiFactory.Rule(true));
        column.AddChild(MasteryRow(profile, summary, config));

        var unlocks = UiFactory.VBox(2);
        column.AddChild(unlocks);
        if (summary != null)
        {
            foreach (var achievement in summary.AchievementsUnlocked) unlocks.AddChild(Bullet("check", $"Achievement: {achievement}"));
            foreach (var message in summary.NewUnlockMessages) unlocks.AddChild(Bullet("lock", message));
        }
        if (summary == null) unlocks.AddChild(UiFactory.Label("PaperSmallLabel", "No mission record to score - reached without a completed mission."));

        var menu = UiFactory.Button("PaperButton", "Main Menu", () => GetTree().ChangeSceneToFile("res://scenes_root/main_menu.tscn"));
        var loadout = UiFactory.Button("PaperButton", "Change Loadout", () => { MissionSession.ResetMission(); GetTree().ChangeSceneToFile("res://scenes_root/loadout.tscn"); });
        var retry = UiFactory.Button("PrimaryButton", "Retry Mission", () => { MissionSession.ResetMission(); GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn"); });
        var row = FlowScreen.ActionRow(column, retry, menu);
        row.AddChild(loadout);
        row.MoveChild(loadout, 1);
        retry.GrabFocus();
    }

    // Null (nothing to score) when the results screen is reached without a
    // real MissionStatsCollector snapshot - e.g. a debug scene load.
    private static ProgressionSummary RecordProgression(MissionDefinition mission, PlayerProfile profile, GameBalanceConfig config)
    {
        var stats = MissionSession.LastResult;
        if (stats == null) return null;
        var summary = ProgressionService.RecordResult(profile, mission, MissionSession.CurrentNationId, stats, config);
        ProfileStore.Save();
        return summary;
    }

    private static Control StarRow(bool[] stars, StarObjectiveDefinition objective)
    {
        var row = UiFactory.HBox(32);
        row.Alignment = BoxContainer.AlignmentMode.Center;
        string[] captions = { "Complete the mission", "Hold at least 75% of the Defense Line", objective?.Description ?? "Mission objective" };
        for (int i = 0; i < 3; i++)
        {
            var box = UiFactory.VBox(4);
            box.CustomMinimumSize = new Vector2(220f, 0f);
            bool earned = i < stars.Length && stars[i];
            var star = UiFactory.Icon(earned ? "star_filled" : "star_empty", 56, earned ? UiPalette.Brass : UiPalette.InkMuted);
            if (star != null) { star.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter; box.AddChild(star); }
            var caption = UiFactory.Wrapped("PaperSmallLabel", captions[i]);
            caption.HorizontalAlignment = HorizontalAlignment.Center;
            box.AddChild(caption);
            row.AddChild(box);
        }
        return row;
    }

    private static Control MasteryRow(PlayerProfile profile, ProgressionSummary summary, GameBalanceConfig config)
    {
        float xp = (float)profile.MasteryXp.GetValueOrDefault(MissionSession.CurrentNationId);
        int rank = MasteryService.RankFor(xp, config);
        var thresholds = config.MasteryRankXpThresholds;
        float lower = rank >= 1 && rank - 1 < thresholds.Length ? thresholds[rank - 1] : 0f;
        float upper = rank < thresholds.Length ? thresholds[rank] : lower;

        var row = UiFactory.HBox(10);
        var chevron = UiFactory.Icon("rank_chevron", 24, UiPalette.Ink);
        if (chevron != null) row.AddChild(chevron);
        row.AddChild(UiFactory.Label("PaperBodyLabel", $"Faction Mastery · Rank {rank}"));
        var bar = new ProgressBar
        {
            MinValue = 0, MaxValue = 1, ShowPercentage = false,
            Value = upper > lower ? Mathf.Clamp((xp - lower) / (upper - lower), 0f, 1f) : 1f,
            CustomMinimumSize = new Vector2(260f, 14f),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
        };
        row.AddChild(bar);
        row.AddChild(UiFactory.Label("PaperNumberLabel", summary != null ? $"+{summary.MasteryXpGained:F0} XP" : $"{xp:F0} XP"));
        if (summary?.RankedUp == true)
        {
            var chip = UiFactory.Label("PaperSmallLabel", "RANK UP");
            chip.AddThemeColorOverride("font_color", UiPalette.Green);
            row.AddChild(chip);
        }
        return row;
    }

    private static Control Bullet(string iconId, string text)
    {
        var row = UiFactory.HBox(6);
        var icon = UiFactory.Icon(iconId, 16, UiPalette.Ink);
        if (icon != null) row.AddChild(icon);
        row.AddChild(UiFactory.Wrapped("PaperBodyLabel", text));
        return row;
    }
}
