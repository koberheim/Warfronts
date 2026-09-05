using Godot;

namespace FrontsOfWar.Map.Authoring;

// Deterministic key/value metadata. Arrays of these are used instead of
// dictionaries so ResourceSaver output can be normalized by key.
[GlobalClass]
public partial class MapProperty : Resource
{
    [Export] public string Key = "";
    [Export(PropertyHint.MultilineText)] public string Value = "";
}
