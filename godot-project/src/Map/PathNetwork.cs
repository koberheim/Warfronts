using Godot;

namespace FrontsOfWar.Map;

// Wraps a Path2D so enemies can query position by distance or by normalized
// progress without touching Godot's Curve2D API directly (GDD §19 prompt 6).
// Expects a single child Path2D node named "Route".
public partial class PathNetwork : Node2D
{
    [Export] public string PathId = "main";
    [Export] public int ActiveFromWave = 1;
    [Export] public int ActiveUntilWave = -1;
    public bool IsAvailableAtWave(int wave) => wave >= ActiveFromWave && (ActiveUntilWave < 0 || wave <= ActiveUntilWave);

    private Path2D _route;

    public float LengthPixels { get; private set; }

    public override void _Ready()
    {
        _route = GetNodeOrNull<Path2D>("Route");
        if (_route == null)
        {
            GD.PushError($"PathNetwork '{Name}' has no child Path2D named 'Route'.");
            return;
        }
        LengthPixels = _route.Curve.GetBakedLength();
    }

    public Vector2 GetPositionAtDistance(float distancePixels)
        => _route.ToGlobal(_route.Curve.SampleBaked(Mathf.Clamp(distancePixels, 0f, LengthPixels)));

    public Vector2 GetPositionAtProgress(float progress01)
        => GetPositionAtDistance(Mathf.Clamp(progress01, 0f, 1f) * LengthPixels);

    public float GetClosestDistance(Vector2 worldPosition)
    {
        if (_route?.Curve == null || LengthPixels <= 0f) return 0f;
        float bestDistance = 0f;
        float bestSquared = float.MaxValue;
        const int samples = 128;
        for (int i = 0; i <= samples; i++)
        {
            float distance = LengthPixels * i / samples;
            float squared = worldPosition.DistanceSquaredTo(GetPositionAtDistance(distance));
            if (squared < bestSquared) { bestSquared = squared; bestDistance = distance; }
        }
        return bestDistance;
    }
}

public sealed class PathNetworkSet
{
    private readonly System.Collections.Generic.Dictionary<string, PathNetwork> _paths = new(System.StringComparer.Ordinal);
    public void Add(PathNetwork path) { if (path != null && !string.IsNullOrWhiteSpace(path.PathId)) _paths[path.PathId] = path; }
    public PathNetwork Resolve(string pathId, PathNetwork fallback = null)
        => pathId != null && _paths.TryGetValue(pathId, out var path) ? path : fallback;
    public int Count => _paths.Count;
    public PathNetwork ResolveForWave(string pathId, int wave, PathNetwork fallback)
    {
        // Empty IDs are the legacy default; authored non-empty IDs must resolve.
        var path = string.IsNullOrWhiteSpace(pathId) || pathId == "main" && !_paths.ContainsKey("main")
            ? fallback : Resolve(pathId);
        if (path == null) throw new System.InvalidOperationException($"Wave {wave} references unknown path '{pathId}'.");
        if (!path.IsAvailableAtWave(wave)) throw new System.InvalidOperationException($"Path '{path.PathId}' is closed on wave {wave}.");
        return path;
    }
}
