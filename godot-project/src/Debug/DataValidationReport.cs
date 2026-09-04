using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrontsOfWar.Debug;

// GDD §19 prompt 45 / §15.6 item 4: the report shape shared by the
// EditorPlugin menu command, the `--validate-data` headless CLI path, and
// DataValidatorTests. One line per issue: "ERROR|WARN  <res path>  <message>".
public enum DataValidationSeverity
{
    Warning,
    Error,
}

public sealed class DataValidationIssue
{
    public DataValidationSeverity Severity { get; }
    public string Path { get; }
    public string Message { get; }

    public DataValidationIssue(DataValidationSeverity severity, string path, string message)
    {
        Severity = severity;
        Path = path ?? "";
        Message = message ?? "";
    }

    public override string ToString()
    {
        string tag = Severity == DataValidationSeverity.Error ? "ERROR" : "WARN";
        return $"{tag}  {Path}  {Message}";
    }
}

public sealed class DataValidationReport
{
    public readonly List<DataValidationIssue> Issues = new();

    // Set by DataValidator.ValidateProjectData(); left at 0 for reports built
    // directly from ValidateResources() over a synthetic in-memory list.
    public int ResourcesChecked;

    public IEnumerable<DataValidationIssue> Errors => Issues.Where(i => i.Severity == DataValidationSeverity.Error);
    public IEnumerable<DataValidationIssue> Warnings => Issues.Where(i => i.Severity == DataValidationSeverity.Warning);
    public int ErrorCount => Issues.Count(i => i.Severity == DataValidationSeverity.Error);
    public int WarningCount => Issues.Count(i => i.Severity == DataValidationSeverity.Warning);
    public bool HasErrors => ErrorCount > 0;
    public int ExitCode => HasErrors ? 1 : 0;

    public void AddError(string path, string message)
        => Issues.Add(new DataValidationIssue(DataValidationSeverity.Error, path, message));

    public void AddWarning(string path, string message)
        => Issues.Add(new DataValidationIssue(DataValidationSeverity.Warning, path, message));

    public string BuildReportText()
    {
        var sb = new StringBuilder();
        foreach (var issue in Issues)
            sb.AppendLine(issue.ToString());
        sb.AppendLine(
            $"SUMMARY: {ErrorCount} error(s), {WarningCount} warning(s) across {ResourcesChecked} resource(s) checked.");
        return sb.ToString();
    }
}
