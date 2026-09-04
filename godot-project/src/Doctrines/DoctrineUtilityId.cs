using System.Collections.Generic;

namespace FrontsOfWar.Doctrines;

// The closed set of DoctrineAbility.UtilityId values a Kind=InstantRefund
// ability may use (GDD §19 prompt 39). Every one of the 18 doctrines'
// utility abilities maps onto exactly one of these seven; DoctrineSystem's
// dispatch switch and DoctrineTests' data validation both key off this list
// so the set stays the single source of truth.
public static class DoctrineUtilityId
{
    public const string RefundTower = "refund_tower";
    public const string FireAll = "fire_all";
    public const string DetonateMinefields = "detonate_minefields";
    public const string RefillMinefields = "refill_minefields";
    public const string DefenseLineBonus = "defense_line_bonus";
    public const string RelocateTower = "relocate_tower";
    public const string ForceTarget = "force_target";

    private static readonly HashSet<string> All = new()
    {
        RefundTower, FireAll, DetonateMinefields, RefillMinefields,
        DefenseLineBonus, RelocateTower, ForceTarget,
    };

    public static bool IsValid(string id) => !string.IsNullOrEmpty(id) && All.Contains(id);
}
