using System.Collections.Generic;
using FrontsOfWar.Map.Planning;

namespace FrontsOfWar.Towers;

public sealed class MinefieldField
{
    public PlanPoint Position { get; set; } = new();
    public int Charges { get; set; }
    public float ArmedAfterSeconds { get; set; }
}

public static class MinefieldRules
{
    public static bool CanPlace(IReadOnlyList<MinefieldField> fields, PlanPoint position,
        int maximumFields, float minimumSpacing)
    {
        if (fields.Count >= maximumFields) return false;
        foreach (var field in fields)
            if (MapPlanGeometry.Distance(field.Position, position) < minimumSpacing) return false;
        return true;
    }

    public static bool TryTrigger(MinefieldField field, float distanceToEnemy,
        float triggerRadius, float armingRemaining)
    {
        if (field.Charges <= 0 || armingRemaining > 0f || distanceToEnemy > triggerRadius) return false;
        field.Charges--;
        return true;
    }
}
