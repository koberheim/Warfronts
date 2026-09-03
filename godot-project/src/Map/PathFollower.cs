using Godot;

namespace FrontsOfWar.Map;

// Advances a single enemy's distance along a PathNetwork on each simulation
// tick (GDD §19 prompt 6). Speed is authored in tiles/sec (GDD §6); this is
// the one place that gets converted to pixels/sec.
public class PathFollower
{
    private readonly PathNetwork _path;

    public float DistanceTraveled { get; private set; }
    public float HoldDistancePixels { get; set; } = float.MaxValue;
    public float Progress => _path.LengthPixels > 0f ? DistanceTraveled / _path.LengthPixels : 0f;
    public bool ReachedEnd => DistanceTraveled >= _path.LengthPixels;
    public Vector2 CurrentPosition => _path.GetPositionAtDistance(DistanceTraveled);

    public PathFollower(PathNetwork path)
    {
        _path = path;
    }

    // speedMultiplier folds in per-segment modifiers (e.g. Mud, Suppressed)
    // and tickDeltaSeconds is 1/SimulationHz — always a fixed step, never
    // frame delta, so this stays exact regardless of render framerate.
    public void Advance(float speedTilesPerSec, float speedMultiplier, float tickDeltaSeconds, float tilePixelSize)
    {
        float nextDistance = DistanceTraveled + speedTilesPerSec * speedMultiplier * tilePixelSize * tickDeltaSeconds;
        nextDistance = Mathf.Min(Mathf.Min(nextDistance, HoldDistancePixels), _path.LengthPixels);
        DistanceTraveled = Mathf.Max(DistanceTraveled, nextDistance);
    }
}
