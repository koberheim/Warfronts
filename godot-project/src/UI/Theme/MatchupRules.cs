using System.Collections.Generic;
using System.Linq;
using FrontsOfWar.Combat;

namespace FrontsOfWar.UI.Theme;

// "Strong vs / Weak vs" rows (GDD §13.5) derived from the real damage table
// (§5.4) so the UI can never disagree with the simulation: strong is any
// armor class the type hits at full value or better, weak is any class it
// hits at less than half. Anti-Air is the one special case - it only ever
// resolves against Air targets.
public static class MatchupRules
{
    private const float StrongThreshold = 1.0f;
    private const float WeakThreshold = 0.5f;

    public static string DamageTypeName(DamageType type) => type switch
    {
        DamageType.SmallArms => "Small Arms",
        DamageType.Explosive => "Explosive",
        DamageType.ArmorPiercing => "Armor-Piercing",
        DamageType.AntiAir => "Anti-Air",
        _ => type.ToString(),
    };

    public static IReadOnlyList<(string IconId, string Label)> StrongVs(DamageType type)
    {
        if (type == DamageType.AntiAir) return new[] { ("threat_air", "Air") };
        return ArmorClasses(type, m => m >= StrongThreshold);
    }

    public static IReadOnlyList<(string IconId, string Label)> WeakVs(DamageType type)
    {
        if (type == DamageType.AntiAir) return new[] { ("ineffective", "Ground") };
        return ArmorClasses(type, m => m < WeakThreshold);
    }

    public static string StrongVsText(DamageType type) => string.Join(", ", StrongVs(type).Select(e => e.Label));
    public static string WeakVsText(DamageType type) => string.Join(", ", WeakVs(type).Select(e => e.Label));

    private static IReadOnlyList<(string IconId, string Label)> ArmorClasses(DamageType type, System.Func<float, bool> predicate)
        => System.Enum.GetValues<ArmorClass>()
            .Where(armor => predicate(DamageTable.Default.Multiplier(type, armor)))
            .Select(armor => (UiIcons.ForArmorClass(armor), armor.ToString()))
            .ToList();
}
