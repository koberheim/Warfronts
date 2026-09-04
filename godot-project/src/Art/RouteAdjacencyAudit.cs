using System.Linq;
using Godot;

namespace FrontsOfWar.Art;

// Review-only validator for a grid of RouteTopologyLayer nodes. It checks
// every immediate horizontal and vertical neighbor, not just a hand-picked
// loop, and reports the result in the scene plus the debugger.
[Tool]
public partial class RouteAdjacencyAudit : Node
{
    [Export] public float CellSize = 160f;
    [Export] public NodePath StatusLabelPath = new("AuditLabel");

    public override void _Ready() => CallDeferred(nameof(RunAudit));

    private void RunAudit()
    {
        var layers = GetParent().GetChildren()
            .OfType<RouteTopologyLayer>()
            .ToArray();
        var checks = 0;
        var failures = 0;

        for (var i = 0; i < layers.Length; i++)
        {
            for (var j = i + 1; j < layers.Length; j++)
            {
                var delta = layers[j].Position - layers[i].Position;
                if (delta.IsEqualApprox(new Vector2(CellSize, 0f)))
                {
                    checks++;
                    if (layers[i].HasSocket("E") != layers[j].HasSocket("W")) failures++;
                }
                else if (delta.IsEqualApprox(new Vector2(0f, CellSize)))
                {
                    checks++;
                    if (layers[i].HasSocket("S") != layers[j].HasSocket("N")) failures++;
                }
            }
        }

        var status = GetParent().GetNodeOrNull<Label>(StatusLabelPath);
        if (status != null)
            status.Text = failures == 0
                ? $"ADJACENCY AUDIT  /  PASS  /  {checks} neighbor joins match"
                : $"ADJACENCY AUDIT  /  FAIL  /  {failures} of {checks} joins mismatch";
        if (failures > 0)
            GD.PushError($"Route adjacency review found {failures} mismatched socket joins.");
    }
}
