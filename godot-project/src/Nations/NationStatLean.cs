using Godot;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Nations;

[GlobalClass]
public partial class NationStatLean : Resource
{
    [Export] public TowerArchetype Archetype;
    [Export] public string StatId = "";
    [Export] public float Multiplier = 1f;
}
