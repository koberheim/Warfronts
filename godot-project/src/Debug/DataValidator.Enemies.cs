using FrontsOfWar.Enemies;

namespace FrontsOfWar.Debug;

public static partial class DataValidator
{
    private static void ValidateEnemy(string path, EnemyDefinition enemy, DataValidationReport report)
    {
        if (string.IsNullOrWhiteSpace(enemy.Id))
            report.AddError(path, "EnemyDefinition has an empty Id.");

        if (enemy.BaseHp <= 0f)
            report.AddError(path, "EnemyDefinition BaseHp must be greater than 0.");

        if (enemy.MoveSpeedTilesPerSec <= 0f)
            report.AddError(path, "EnemyDefinition MoveSpeedTilesPerSec must be greater than 0.");

        // GDD §7.8: leak costs are >= 1 for every archetype; bosses use
        // Instant Loss instead and may carry any LeakCost value (0 in the
        // shipped data — the boss reaching the objective ends the mission
        // directly, the ledger cost is irrelevant).
        if (!enemy.IsBoss && enemy.LeakCost < 1)
            report.AddError(path, "EnemyDefinition LeakCost must be at least 1 (bosses may use any value).");

        if (enemy.ControllerScene == null)
            report.AddError(path, "EnemyDefinition ControllerScene is null.");

        if (enemy.IsBoss && enemy.SkirtHp <= 0f)
            report.AddWarning(path, "Boss EnemyDefinition has no SkirtHp set.");
    }
}
