using Godot;

namespace FrontsOfWar.Map.Authoring;

// Type and parameters stay data-driven. Runtime behavior is implemented only
// for the GDD-authored gimmicks during its later milestone, not in this model.
[GlobalClass]
public partial class MapGimmick : Resource
{
    [Export] public string Id = "";
    [Export] public string Type = "";
    [Export] public bool Enabled = true;
    [Export] public string[] PathIds = System.Array.Empty<string>();
    [Export] public MapProperty[] Parameters = System.Array.Empty<MapProperty>();
}
