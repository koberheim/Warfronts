using Godot;

namespace FrontsOfWar.Map.Authoring;

public enum PathCurveMode { Polyline, Cubic }

[GlobalClass]
public partial class PathDefinition : Resource
{
    [Export] public string Id = "";
    [Export] public string EntryMarkerId = "";
    [Export] public string ObjectiveMarkerId = "";
    [Export] public Vector2[] Points = System.Array.Empty<Vector2>();
    [Export] public PathCurveMode CurveMode;
    [Export] public Vector2[] BakedRuntimePoints = System.Array.Empty<Vector2>();
    [Export] public string BranchGroupId = "";
    [Export] public int ActiveFromWave = 1;
    [Export] public int ActiveUntilWave = -1;
    [Export] public string[] Tags = System.Array.Empty<string>();
}
