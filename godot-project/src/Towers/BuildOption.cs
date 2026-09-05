using Godot;
using FrontsOfWar.Nations;

namespace FrontsOfWar.Towers;

// A presentation/placement view of authored data, never another balance asset.
public sealed class BuildOption
{
    public Resource Resource { get; }
    public TowerDefinition Tower => Resource as TowerDefinition;
    public bool IsMinefield => Tower?.Archetype == TowerArchetype.Minefield;
    public bool IsSignature => Resource is SignatureDefinition or ArsenalDefinition;
    public string Id => Resource switch
    {
        TowerDefinition tower => tower.Id, SignatureDefinition signature => signature.Id,
        ArsenalDefinition arsenal => arsenal.Id, _ => "",
    };
    public string Name => Resource switch
    {
        TowerDefinition tower => tower.DisplayName, SignatureDefinition signature => signature.DisplayName,
        ArsenalDefinition arsenal => arsenal.DisplayName, _ => "Unavailable",
    };
    public int Cost => Resource switch
    {
        TowerDefinition tower => tower.PreForkStatsForLevel(1).Cost,
        SignatureDefinition signature => signature.LevelCosts[0], ArsenalDefinition arsenal => arsenal.LevelCosts[0], _ => 0,
    };
    public PackedScene Scene => Resource switch
    {
        TowerDefinition tower => tower.ControllerScene, SignatureDefinition signature => signature.ControllerScene,
        ArsenalDefinition arsenal => arsenal.ControllerScene, _ => null,
    };
    public BuildOption(Resource resource) { Resource = resource; }
    public static BuildOption Load(string path, NationProfile nation = null)
    {
        var resource = ResourceLoader.Load(path);
        if (resource is TowerDefinition tower && nation != null) resource = nation.CreateTowerVariant(tower);
        return new BuildOption(resource);
    }
}
