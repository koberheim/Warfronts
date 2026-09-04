using FrontsOfWar.Towers;

namespace FrontsOfWar.Debug;

public static partial class DataValidator
{
    // TowerDefinition.ControllerScene is deliberately not required here: T8
    // Minefield is free-placement (GDD §7.5) and leaves it null by design.
    private static void ValidateTower(string path, TowerDefinition tower, DataValidationReport report)
    {
        if (string.IsNullOrWhiteSpace(tower.Id))
            report.AddError(path, "TowerDefinition has an empty Id.");

        if (tower.Levels == null || tower.Levels.Length < 1)
        {
            report.AddError(path, "TowerDefinition must have at least one Levels entry.");
        }
        else if (tower.Levels[0] == null || tower.Levels[0].Cost <= 0)
        {
            report.AddError(path, "TowerDefinition L1 Cost must be greater than 0.");
        }

        ValidateBranches(path, tower, report);

        bool hasProjectileScene = tower.ProjectileScene != null;
        bool hasProjectileSpeed = tower.ProjectileSpeedTilesPerSec > 0f;
        if (hasProjectileSpeed && !hasProjectileScene)
            report.AddError(path, "ProjectileSpeedTilesPerSec > 0 but ProjectileScene is null.");
        else if (hasProjectileScene && !hasProjectileSpeed)
            report.AddWarning(path, "ProjectileScene is set but ProjectileSpeedTilesPerSec is 0.");
    }

    // GDD §6: every one of the nine archetypes forks into two L3/L4 branches.
    // All nine archetypes (including the "VS" scope towers T1/T3/T4/T9, per
    // docs/PROGRESS.md prompt 27) now have that content authored, so a
    // missing BranchA/BranchB is a real bug, not an expected gap — same as
    // an asymmetric branch (only one of BranchA/BranchB set) or a branch
    // with the wrong level count.
    private static void ValidateBranches(string path, TowerDefinition tower, DataValidationReport report)
    {
        bool hasA = tower.BranchA != null;
        bool hasB = tower.BranchB != null;

        if (hasA != hasB)
        {
            report.AddError(path, "TowerDefinition has only one of BranchA/BranchB set; both or neither are expected.");
            return;
        }

        if (!hasA)
        {
            report.AddError(path, "TowerDefinition has no L3/L4 branch data authored.");
            return;
        }

        ValidateBranchLevelCount(path, "BranchA", tower.BranchA, report);
        ValidateBranchLevelCount(path, "BranchB", tower.BranchB, report);
    }

    private static void ValidateBranchLevelCount(string path, string label, TowerBranch branch, DataValidationReport report)
    {
        int count = branch.Levels?.Length ?? 0;
        if (count != 2)
            report.AddError(path, $"{label} '{branch.Name}' must have exactly 2 levels (found {count}).");
    }
}
