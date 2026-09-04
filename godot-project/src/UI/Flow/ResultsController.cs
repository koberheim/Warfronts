using System.Collections.Generic;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Meta;

namespace FrontsOfWar.UI.Flow;

// GDD §19 prompt 41: records the finished mission into the player's saved
// profile (stars, Faction Mastery XP, unlock deltas) and shows the result,
// then persists it, before offering the existing one-click retry.
public partial class ResultsController : Node2D
{
    public override void _Ready()
    {
        var box = new VBoxContainer { Position = new Vector2(180, 100), Size = new Vector2(660, 440) };
        AddChild(box);

        string result = MissionSession.LastMissionWon ? "MISSION COMPLETE" : "MISSION FAILED";
        box.AddChild(new Label
        {
            Text = $"{result}\n\n{MissionSession.LastMissionTitle}\nWave reached: {MissionSession.LastWaveReached}",
        });

        string progressionText = RecordProgressionAndBuildReport();
        if (!string.IsNullOrEmpty(progressionText))
            box.AddChild(new Label { Text = progressionText, AutowrapMode = TextServer.AutowrapMode.WordSmart });

        var retry = new Button { Text = "Retry Mission", CustomMinimumSize = new Vector2(240, 44) };
        retry.Pressed += () => { MissionSession.ResetMission(); GetTree().ChangeSceneToFile("res://scenes_root/mission.tscn"); };
        box.AddChild(retry);
        var menu = new Button { Text = "Back to Briefing", CustomMinimumSize = new Vector2(240, 44) };
        menu.Pressed += () => GetTree().ChangeSceneToFile("res://scenes_root/briefing.tscn");
        box.AddChild(menu);
    }

    // Returns "" (no progression to show) when the results screen is reached
    // without a real MissionStatsCollector snapshot — e.g. a debug scene
    // load that skipped the mission flow entirely.
    private string RecordProgressionAndBuildReport()
    {
        var stats = MissionSession.LastResult;
        if (stats == null) return "";

        var mission = GD.Load<MissionDefinition>(MissionSession.CurrentMissionPath);
        var config = GameBalanceConfigAutoload.Config;
        var profile = ProfileStore.Current;

        var summary = ProgressionService.RecordResult(profile, mission, MissionSession.CurrentNationId, stats, config);
        ProfileStore.Save();

        var lines = new List<string>
        {
            "",
            StarLine(summary.StarsEarnedThisRun, mission?.StarObjective),
            $"Mastery XP: +{summary.MasteryXpGained:F0} (Rank {summary.MasteryRankAfter})" +
                (summary.RankedUp ? "  — RANK UP!" : ""),
        };

        if (summary.AchievementsUnlocked.Count > 0)
            lines.Add("Achievements unlocked: " + string.Join(", ", summary.AchievementsUnlocked));
        if (summary.NewUnlockMessages.Count > 0)
            lines.Add("New unlocks: " + string.Join("; ", summary.NewUnlockMessages));

        return string.Join("\n", lines);
    }

    private static string StarLine(bool[] stars, StarObjectiveDefinition objective)
    {
        string Mark(bool earned) => earned ? "[*]" : "[ ]";
        string objectiveText = objective?.Description ?? "mission objective";
        return $"Stars: {Mark(stars[0])} Complete Mission   " +
            $"{Mark(stars[1])} Hold >=75% Defense Line   {Mark(stars[2])} {objectiveText}";
    }
}
