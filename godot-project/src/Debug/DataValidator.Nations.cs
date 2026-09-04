using System.Collections.Generic;
using System.Linq;
using FrontsOfWar.Combat;
using FrontsOfWar.Enemies;
using FrontsOfWar.Nations;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Debug;

public static partial class DataValidator
{
    private static void ValidateSignature(string path, SignatureDefinition sig, DataValidationReport report)
    {
        if (sig.LevelCosts == null || sig.LevelCosts.Length != 3)
            report.AddError(path, $"SignatureDefinition LevelCosts must have length 3 (found {sig.LevelCosts?.Length ?? 0}).");

        if (sig.ChargeCaps == null || sig.ChargeCaps.Length != 3)
            report.AddError(path, $"SignatureDefinition ChargeCaps must have length 3 (found {sig.ChargeCaps?.Length ?? 0}).");

        if (sig.ChargeRegenSeconds == null || sig.ChargeRegenSeconds.Length != 3)
            report.AddError(path,
                $"SignatureDefinition ChargeRegenSeconds must have length 3 (found {sig.ChargeRegenSeconds?.Length ?? 0}).");
    }

    // Runs NationBalanceValidator (the ±15% envelope / ±3% parity rule) over
    // whatever NationProfile resources were found, plus every SignatureId
    // cross-reference. Only invoked when at least one NationProfile is
    // present, so synthetic tests that don't care about nations (duplicate
    // Id, broken SpawnGroup, ...) never pick up an unrelated "expected six
    // profiles" error.
    private static void ValidateNations(
        List<(string Path, NationProfile Nation)> nationEntries,
        List<TowerDefinition> roster,
        HashSet<string> signatureAndArsenalIds,
        DataValidationReport report)
    {
        var profiles = nationEntries.Select(n => n.Nation).ToList();
        var balance = NationBalanceValidator.Validate(profiles, roster);
        foreach (var error in balance.Errors)
            report.AddError(DataRoot + "nations/", error);

        foreach (var (path, nation) in nationEntries)
        {
            if (string.IsNullOrWhiteSpace(nation.SignatureId))
            {
                report.AddError(path, "NationProfile has an empty SignatureId.");
                continue;
            }

            if (!signatureAndArsenalIds.Contains(nation.SignatureId))
                report.AddError(path,
                    $"SignatureId '{nation.SignatureId}' does not match any SignatureDefinition/ArsenalDefinition Id.");
        }
    }

    // GDD §15.6 "enemies with no listed counters": for every ground enemy at
    // least one roster tower's damage type must reach a 1.0x multiplier
    // against its armor class (DamageTable, GDD §5.4); every air enemy needs
    // at least one roster tower whose L1 TargetDomain can acquire Air.
    private static void ValidateCounters(
        List<(string Path, EnemyDefinition Enemy)> enemyEntries,
        List<TowerDefinition> roster,
        DataValidationReport report)
    {
        var groundDamageTypes = roster.Select(t => t.DamageType).Distinct().ToList();
        bool anyAirTower = roster.Any(t =>
            t.Levels != null && t.Levels.Length > 0 && t.Levels[0] != null &&
            (t.Levels[0].TargetDomain == TargetDomain.Air || t.Levels[0].TargetDomain == TargetDomain.GroundAndAir));

        foreach (var (path, enemy) in enemyEntries)
        {
            if (enemy.IsAir)
            {
                if (!anyAirTower)
                    report.AddError(path, "Air enemy has no roster tower whose L1 TargetDomain is Air or GroundAndAir.");
                continue;
            }

            bool hasCounter = groundDamageTypes.Any(type => DamageTable.Default.Multiplier(type, enemy.ArmorClass) >= 1.0f);
            if (!hasCounter)
                report.AddError(path,
                    $"Ground enemy (ArmorClass={enemy.ArmorClass}) has no roster tower damage type reaching a 1.0x multiplier.");
        }
    }
}
