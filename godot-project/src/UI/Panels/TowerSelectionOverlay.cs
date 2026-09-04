using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Towers;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.UI.Panels;

// World-space overlay for the selected tower (GDD §13.4: "the selected
// tower shows its range, its current target with a thin line"; UI spec
// §9): an amber 2 px range ring and a 1 px line to the live target.
// Scaled by the camera like everything else on the table.
public partial class TowerSelectionOverlay : Node2D
{
    private TowerController _tower;
    private CommandPostController _post;

    public void Select(TowerController tower)
    {
        _tower = tower;
        _post = null;
        Position = tower.GlobalPosition;
        Visible = true;
        QueueRedraw();
    }

    public void Select(CommandPostController post)
    {
        _post = post;
        _tower = null;
        Position = post.GlobalPosition;
        Visible = true;
        QueueRedraw();
    }

    public void Clear()
    {
        _tower = null;
        _post = null;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (Visible && _tower != null) QueueRedraw();
    }

    public override void _Draw()
    {
        float tile = GameBalanceConfigAutoload.Config.TilePixelSize;
        var amber = UiPalette.Amber;

        if (_post != null && IsInstanceValid(_post) && _post.Upgrade != null)
        {
            float aura = _post.Upgrade.CurrentStats().AuraRadiusTiles * tile * _post.DoctrineAuraRadiusMultiplier;
            if (aura > 0f) DrawArc(Vector2.Zero, aura, 0f, Mathf.Tau, 48, amber with { A = 0.9f }, 2f, true);
            return;
        }

        if (_tower == null || !IsInstanceValid(_tower) || _tower.Upgrade == null) return;
        float range = _tower.Upgrade.CurrentStats().RangeTiles * tile * _tower.AuraRangeMultiplier;
        if (range > 0f)
        {
            DrawCircle(Vector2.Zero, range, amber with { A = 0.08f });
            DrawArc(Vector2.Zero, range, 0f, Mathf.Tau, 48, amber with { A = 0.9f }, 2f, true);
        }

        var target = _tower.CurrentTarget;
        if (target is { IsAlive: true })
            DrawLine(Vector2.Zero, ToLocal(target.GlobalPosition), amber with { A = 0.8f }, 1f, true);
    }
}
