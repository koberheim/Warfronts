using Godot;

namespace FrontsOfWar.Art;

// Data-only socket mask for one reusable route topology. The mask is separate
// from terrain texture so neighboring tiles can be checked deterministically.
[GlobalClass]
public partial class RouteTopologyDefinition : Resource
{
    [Export] public string Id = "route_topology";
    [Export] public bool North;
    [Export] public bool East;
    [Export] public bool South;
    [Export] public bool West;

    public bool HasSocket(string edge) => edge switch
    {
        "N" => North,
        "E" => East,
        "S" => South,
        "W" => West,
        _ => false
    };
}
