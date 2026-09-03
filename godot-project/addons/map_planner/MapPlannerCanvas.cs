using Godot;
using System.Collections.Generic;
using FrontsOfWar.Map.Planning;

[Tool]
public partial class MapPlannerCanvas : Control
{
    public MapPlanDefinition Plan { get; set; }
    public string Overlay { get; set; } = "Route Exposure";

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("18212b"));
        if (Plan == null || Plan.Canvas == null) return;
        foreach (var zone in Plan.Zones) DrawZone(zone);
        foreach (var path in Plan.Paths) DrawPath(path);
        foreach (var pad in Plan.Pads) DrawPad(pad);
        for (int i = 0; i < Plan.Entries.Count; i++) DrawCircle(ToCanvas(Plan.Entries[i]), 7f, new Color("63d391"));
        DrawCircle(ToCanvas(Plan.Objective), 8f, new Color("e36b6b"));
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (Plan == null || @event is not InputEventMouseButton mouse || !mouse.Pressed) return;
        var point = FromCanvas(mouse.Position);
        if (mouse.ButtonIndex == MouseButton.Left && Plan.Paths.Count > 0)
            Plan.Paths[0].Points.Add(point);
        else if (mouse.ButtonIndex == MouseButton.Right)
            Plan.Pads.Add(new PadPlan { Id = $"pad_{Plan.Pads.Count}", Position = point, StrategicRole = "Standard" });
        else return;
        MapPlanScorer.Score(Plan);
        QueueRedraw();
        AcceptEvent();
    }

    private void DrawPath(PathPlan path)
    {
        var points = new List<Vector2>();
        foreach (var point in path.Points) points.Add(ToCanvas(point));
        if (points.Count > 1) DrawPolyline(points.ToArray(), path.IntentionalCrossing ? new Color("e3b341") : new Color("7cc7ff"), 4f, true);
    }

    private void DrawPad(PadPlan pad)
    {
        float radius = Overlay == "Pad Score" ? Mathf.Clamp(4f + pad.Exposure.Score, 5f, 13f) : 6f;
        DrawCircle(ToCanvas(pad.Position), radius, pad.StrategicRole == "Shared" ? new Color("e3b341") : new Color("d7dce2"));
    }

    private void DrawZone(PlanZone zone)
    {
        Color color = zone.Type == "GameplayClearZone" ? new Color(0.25f, 0.75f, 0.55f, 0.08f) :
            zone.Type == "Storytelling" ? new Color(0.75f, 0.4f, 0.9f, 0.1f) : new Color(0.2f, 0.5f, 0.8f, 0.05f);
        var center = ToCanvas(zone.Center);
        var size = new Vector2(zone.Size.X / Plan.Canvas.X * Size.X, zone.Size.Y / Plan.Canvas.Y * Size.Y);
        DrawRect(new Rect2(center - size / 2f, size), color, true);
    }

    private Vector2 ToCanvas(PlanPoint point)
        => new(point.X / Plan.Canvas.X * Size.X, point.Y / Plan.Canvas.Y * Size.Y);

    private PlanPoint FromCanvas(Vector2 point)
        => new(Mathf.Clamp(point.X / Mathf.Max(1f, Size.X) * Plan.Canvas.X, 0f, Plan.Canvas.X),
            Mathf.Clamp(point.Y / Mathf.Max(1f, Size.Y) * Plan.Canvas.Y, 0f, Plan.Canvas.Y));
}
