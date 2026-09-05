using Godot;

namespace FrontsOfWar.Map.Authoring;

[GlobalClass]
public partial class GenerationProvenance : Resource
{
    [Export] public string SourceTemplateId = "";
    [Export] public long Seed;
    [Export] public string GeneratorVersion = "";
    [Export] public float InitialScore;
    [Export] public MapProperty[] InitialMetrics = System.Array.Empty<MapProperty>();
    [Export] public string ConvertedAtUtc = "";
    [Export] public string AcceptedBy = "";
    [Export(PropertyHint.MultilineText)] public string Notes = "";
}
