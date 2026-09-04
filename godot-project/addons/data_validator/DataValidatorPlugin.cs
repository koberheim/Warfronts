using Godot;
using FrontsOfWar.Debug;

// GDD §15.6 item 4 / §19 prompt 45: "Validate Data" under Project > Tools.
// Runs the same DataValidator.ValidateProjectData() call as the headless
// `--validate-data` CLI path (src/Core/Boot.cs) so the editor and headless
// forms agree by construction.
[Tool]
public partial class DataValidatorPlugin : EditorPlugin
{
    private const string MenuItemName = "Validate Data";

    public override void _EnterTree()
    {
        AddToolMenuItem(MenuItemName, new Callable(this, nameof(RunValidation)));
    }

    public override void _ExitTree()
    {
        RemoveToolMenuItem(MenuItemName);
    }

    private void RunValidation()
    {
        var report = DataValidator.ValidateProjectData();

        foreach (var issue in report.Issues)
        {
            if (issue.Severity == DataValidationSeverity.Error)
                GD.PrintErr(issue.ToString());
            else
                GD.Print(issue.ToString());
        }
        GD.Print($"SUMMARY: {report.ErrorCount} error(s), {report.WarningCount} warning(s) across {report.ResourcesChecked} resource(s) checked.");

        if (report.HasErrors)
            GD.PushError($"Data Validator found {report.ErrorCount} error(s). See Output panel for details.");

        var dialog = new AcceptDialog
        {
            Title = "Data Validator",
            DialogText =
                $"{report.ErrorCount} error(s), {report.WarningCount} warning(s) across {report.ResourcesChecked} resource(s).\n" +
                "Full report printed to the Output panel.",
        };
        dialog.Confirmed += () => dialog.QueueFree();
        dialog.Canceled += () => dialog.QueueFree();
        AddChild(dialog);
        dialog.PopupCentered();
    }
}
