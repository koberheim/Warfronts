using Godot;
using FrontsOfWar.Combat;
using FrontsOfWar.Enemies;
using System.Collections.Generic;

namespace FrontsOfWar.UI.Theme;

// The UI icon registry (docs/UI_DESIGN_SPEC.md §6). Icons are monochrome
// SVGs under assets/ui/icons/<id>.svg, tinted at use via Modulate or a
// Button's icon colors, so one file serves paper and slate backgrounds.
// Get() returns null (never throws) for an id with no file yet, so a screen
// can ship before every glyph exists and fall back to text.
public static class UiIcons
{
    public const string Directory = "res://assets/ui/icons/";

    private static readonly Dictionary<string, Texture2D> Cache = new();

    public static Texture2D Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (Cache.TryGetValue(id, out var cached)) return cached;

        string path = Directory + id + ".svg";
        var texture = ResourceLoader.Exists(path) ? ResourceLoader.Load<Texture2D>(path) : null;
        Cache[id] = texture;
        return texture;
    }

    public static string ForDamageType(DamageType type) => type switch
    {
        DamageType.SmallArms => "damage_small_arms",
        DamageType.Explosive => "damage_explosive",
        DamageType.ArmorPiercing => "damage_armor_piercing",
        DamageType.AntiAir => "damage_anti_air",
        _ => null,
    };

    public static string ForArmorClass(ArmorClass armor) => armor switch
    {
        ArmorClass.Soft => "armor_soft",
        ArmorClass.Hardened => "armor_hardened",
        ArmorClass.Armored => "armor_armored",
        ArmorClass.Heavy => "armor_heavy",
        _ => null,
    };

    public static string ForEnemyArchetype(EnemyArchetype archetype) => archetype switch
    {
        EnemyArchetype.BasicInfantry => "enemy_infantry",
        EnemyArchetype.FastInfantry => "enemy_fast_infantry",
        EnemyArchetype.SwarmInfantry => "enemy_swarm",
        EnemyArchetype.ArmoredInfantry => "enemy_armored_infantry",
        EnemyArchetype.LightVehicle => "enemy_light_vehicle",
        EnemyArchetype.MediumArmor => "enemy_medium_armor",
        EnemyArchetype.HeavyArmor => "enemy_heavy_armor",
        EnemyArchetype.AirUnit => "enemy_air",
        EnemyArchetype.Support => "enemy_support",
        EnemyArchetype.Escort => "enemy_escort",
        EnemyArchetype.Recon => "enemy_recon",
        EnemyArchetype.Siege => "enemy_siege",
        _ => null,
    };

    // Tower icons are keyed by TowerDefinition.Id (t1_automatic_gun …
    // t9_command_post); signature towers use "tower_signature".
    public static string ForTower(string towerId) => string.IsNullOrEmpty(towerId) ? null : "tower_" + towerId;
}
