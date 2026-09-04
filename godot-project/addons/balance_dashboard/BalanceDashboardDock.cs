using Godot;
using FrontsOfWar.Nations;
using FrontsOfWar.Towers;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class BalanceDashboardDock : VBoxContainer
{
    private Label _summary;
    private Label _details;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(360f, 420f);
        AddChild(new Label { Text = "Nation Balance Dashboard" });
        _summary = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _details = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsVertical = SizeFlags.ExpandFill };
        AddChild(_summary);
        AddChild(_details);
        var buttons = new HBoxContainer();
        var run = new Button { Text = "Run Validator" };
        run.Pressed += Refresh;
        buttons.AddChild(run);
        var inject = new Button { Text = "Inject Violation" };
        inject.Pressed += RunInjectedViolation;
        buttons.AddChild(inject);
        AddChild(buttons);
        CallDeferred(nameof(Refresh));
    }

    private void Refresh()
    {
        if (!TryLoadData(out var profiles, out var roster)) return;
        var report = NationBalanceValidator.Validate(profiles, roster);
        _summary.Text = report.IsValid ? "PASS — all six nations are within the configured envelope and parity tolerance." : "FAIL — balance violations detected.";
        _details.Text = FormatReport(report);
    }

    private void RunInjectedViolation()
    {
        if (!TryLoadData(out var profiles, out var roster) || profiles.Count == 0) return;
        var injected = profiles[0];
        var lean = new NationStatLean { Archetype = TowerArchetype.AutomaticGun, StatId = "damage", Multiplier = 1.30f };
        injected.StatLeans = injected.StatLeans.Concat(new[] { lean }).ToArray();
        var report = NationBalanceValidator.Validate(profiles, roster);
        _summary.Text = report.IsValid ? "FAIL — injected violation was not detected." : "PASS — injected violation was detected.";
        _details.Text = FormatReport(report);
    }

    // During the editor's initial C# domain scan, script-backed Resources can
    // arrive as generic Resources and GD.Load<T> throws InvalidCastException
    // (the Wave Editor dock guards the same case). Ask for a manual re-run
    // rather than failing the deferred first refresh.
    private bool TryLoadData(out List<NationProfile> profiles, out List<TowerDefinition> roster)
    {
        try
        {
            profiles = LoadProfiles();
            roster = LoadRoster();
            return true;
        }
        catch (InvalidCastException)
        {
            profiles = null;
            roster = null;
            _summary.Text = "Scripts are still loading — press Run Validator.";
            _details.Text = "";
            return false;
        }
    }

    private static List<NationProfile> LoadProfiles()
    {
        var result = new List<NationProfile>();
        foreach (var id in new[] { "britain", "germany", "italy", "japan", "soviet_union", "united_states" })
        {
            var profile = GD.Load<NationProfile>($"res://assets/data/nations/{id}.tres");
            if (profile != null) result.Add(profile);
        }
        return result;
    }

    private static List<TowerDefinition> LoadRoster()
    {
        var result = new List<TowerDefinition>();
        for (int i = 1; i <= 9; i++)
        {
            string id = i switch { 1 => "t1_automatic_gun", 2 => "t2_marksman_post", 3 => "t3_field_mortar", 4 => "t4_anti_tank_gun", 5 => "t5_flak_battery", 6 => "t6_armored_emplacement", 7 => "t7_heavy_artillery", 8 => "t8_minefield", _ => "t9_command_post" };
            var definition = GD.Load<TowerDefinition>($"res://assets/data/towers/{id}.tres");
            if (definition != null) result.Add(definition);
        }
        return result;
    }

    private static string FormatReport(NationBalanceReport report)
    {
        var lines = report.DpsPerSupply.Select(pair => $"{pair.Key}: {pair.Value:0.0000}").ToList();
        if (report.Errors.Count > 0) lines.AddRange(report.Errors.Select(error => "ERROR: " + error));
        return string.Join("\n", lines);
    }
}
