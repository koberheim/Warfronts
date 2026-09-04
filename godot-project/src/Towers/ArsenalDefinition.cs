using Godot;
using FrontsOfWar.Enemies;

namespace FrontsOfWar.Towers;

[GlobalClass]
public partial class ArsenalDefinition : Resource
{
    [Export] public string Id = "arsenal_of_democracy";
    [Export] public string DisplayName = "Arsenal of Democracy Factory";
    [Export] public int[] LevelCosts = { 650, 423, 748 };
    [Export] public float[] ProductionIntervals = { 14f, 10f, 7f };
    [Export] public int[] UnlockLevels = { 1, 2, 3 };
    [Export] public FriendlyUnitDefinition[] Units = System.Array.Empty<FriendlyUnitDefinition>();
    [Export] public PackedScene UnitScene;
}
