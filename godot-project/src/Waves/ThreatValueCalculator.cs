using Godot;
using FrontsOfWar.Enemies;

namespace FrontsOfWar.Waves;

// Threat Value is intentionally a readable tuning aid rather than a combat
// stat. It lets the Wave Editor compare authored pressure without changing
// the runtime simulation.
public static class ThreatValueCalculator
{
    public static float Calculate(WaveDefinition wave)
    {
        float total = 0f;
        if (wave?.Groups == null) return total;
        foreach (var group in wave.Groups)
        {
            if (group?.Enemy == null) continue;
            float hp = group.Enemy.BaseHp * (group.HpMultiplierOverride > 0f ? group.HpMultiplierOverride : 1f);
            float value = hp * Mathf.Max(1, group.Enemy.LeakCost) * Mathf.Max(0, group.Count);
            value *= ArchetypePenalty(group.Enemy);
            if (group.EliteFlag) value *= 1.25f;
            total += value;
        }
        return total / 100f;
    }

    private static float ArchetypePenalty(EnemyDefinition enemy)
    {
        if (enemy.IsAir) return 1.25f;
        return enemy.SpecialAbilityId switch
        {
            "siege_bombard" => 1.25f,
            "swarm_cohesion" => 1.10f,
            "support" => 1.15f,
            _ => 1f,
        };
    }
}
