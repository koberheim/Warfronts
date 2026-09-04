using Godot;

namespace FrontsOfWar.Art;

// Deterministic gameplay-route underlay for review and future terrain
// compositing. Painted terrain remains a separate layer beneath this one.
[Tool]
public partial class RouteTopologyLayer : Node2D
{
    [Export] public RouteTopologyDefinition Topology;
    [Export] public float TileSize = 1024f;
    [Export] public float RouteWidth = 256f;
    [Export] public float ShoulderWidth = 48f;
    [Export] public Color ShoulderColor = new(0.27f, 0.23f, 0.16f, 0.90f);
    [Export] public Color RouteColor = new(0.34f, 0.28f, 0.20f, 0.96f);
    [Export] public Color WearColor = new(0.20f, 0.17f, 0.12f, 0.55f);

    public override void _Ready() => QueueRedraw();

    public override void _Draw()
    {
        if (Topology == null) return;

        var center = new Vector2(TileSize * 0.5f, TileSize * 0.5f);
        DrawBranch(center, new Vector2(TileSize * 0.5f, 0f), Topology.North);
        DrawBranch(center, new Vector2(TileSize, TileSize * 0.5f), Topology.East);
        DrawBranch(center, new Vector2(TileSize * 0.5f, TileSize), Topology.South);
        DrawBranch(center, new Vector2(0f, TileSize * 0.5f), Topology.West);

        var joinRadius = (RouteWidth + ShoulderWidth * 2f) * 0.5f;
        DrawCircle(center, joinRadius, ShoulderColor);
        DrawCircle(center, RouteWidth * 0.5f, RouteColor);
        DrawCircle(center, RouteWidth * 0.12f, WearColor);
    }

    public bool HasSocket(string edge) => Topology != null && Topology.HasSocket(edge);

    private void DrawBranch(Vector2 start, Vector2 end, bool enabled)
    {
        if (!enabled) return;

        var points = new Vector2[9];
        for (var i = 0; i < points.Length; i++)
        {
            var t = i / (float)(points.Length - 1);
            points[i] = start.Lerp(end, t);
        }

        DrawPolyline(points, ShoulderColor, RouteWidth + ShoulderWidth * 2f, true);
        DrawPolyline(points, RouteColor, RouteWidth, true);
        DrawPolyline(points, WearColor, RouteWidth * 0.24f, true);
    }
}
