using Godot;
using FrontsOfWar.Towers;

namespace FrontsOfWar.Nations;

// A nation's identity is data, not a second set of tower classes.  Profiles
// apply small, inspectable stat leans to the universal tower roster.
[GlobalClass]
public partial class NationProfile : Resource
{
    [Export] public string Id = "";
    [Export] public string DisplayName = "";
    [Export] public string Alliance = "";
    [Export] public NationStatLean[] StatLeans = System.Array.Empty<NationStatLean>();
    [Export] public string[] BranchFlavorIds = System.Array.Empty<string>();
    [Export] public string SignatureId = "";

    public TowerDefinition CreateTowerVariant(TowerDefinition source)
        => NationStatApplicator.Apply(source, this);
}
