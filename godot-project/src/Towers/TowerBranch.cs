using Godot;

namespace FrontsOfWar.Towers;

// One upgrade-fork branch (GDD §6 "Branch fork (L3)"). Levels holds exactly
// 2 stat blocks: index 0 = L3, index 1 = L4. Actual per-archetype branch
// *behavior* differences (Sustained Fire vs. Suppressive Fire, etc.) are
// M4+ content work — for now a branch is just an alternate stat curve.
[GlobalClass]
public partial class TowerBranch : Resource
{
    [Export] public string Name;
    [Export] public TowerStatBlock[] Levels = System.Array.Empty<TowerStatBlock>();
}
