using Godot;
using FrontsOfWar.Art;

namespace FrontsOfWar.Debug;

public static partial class DataValidator
{
    // Only reachable from ValidateProjectData() — the catalog is a JSON
    // file, not a .tres, so synthetic in-memory tests (ValidateResources)
    // never touch it. Missing placeholder art is incomplete content, not a
    // broken reference, so it is a warning.
    private static void ValidateArtCatalog(DataValidationReport report)
    {
        var catalog = ArtAssetCatalog.LoadFromProject();
        foreach (var entry in catalog.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.PlaceholderPath)) continue;
            if (!ResourceLoader.Exists(entry.PlaceholderPath))
                report.AddWarning(ArtAssetCatalog.ProjectPath,
                    $"Entry '{entry.Id}' placeholder path does not exist: {entry.PlaceholderPath}");
        }
    }
}
