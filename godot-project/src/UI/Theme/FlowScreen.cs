using Godot;

namespace FrontsOfWar.UI.Theme;

// Shared scaffolding for the flow screens (docs/UI_DESIGN_SPEC.md §8.1-8.3,
// §8.9): the full-screen war-table frame on its own CanvasLayer, and paper
// sheets laid on it. The frame's 9-patch content margins keep everything
// inside the wooden rim at 1920x1080 and wider.
public static class FlowScreen
{
    // Returns the content area inside the frame (fills the table interior).
    public static Control Build(Node root)
    {
        var layer = new CanvasLayer();
        root.AddChild(layer);

        var frame = UiFactory.Panel("FramePanel");
        frame.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        frame.MouseFilter = Control.MouseFilterEnum.Ignore;
        layer.AddChild(frame);

        var content = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        frame.AddChild(content);
        return content;
    }

    // A paper sheet of a fixed size anchored inside the content area. The
    // returned column is where the screen puts its copy and controls.
    public static VBoxContainer PaperSheet(Control parent, float width, float height, Control.LayoutPreset anchor,
        out PanelContainer sheet, int marginX = 0, int marginY = 0, int separation = 10)
    {
        sheet = UiFactory.Panel("PaperPanel");
        sheet.CustomMinimumSize = new Vector2(width, height);
        parent.AddChild(sheet);
        UiFactory.Anchor(sheet, anchor, marginX, marginY);

        var column = UiFactory.VBox(separation);
        sheet.AddChild(column);
        return column;
    }

    // Bottom action row: the one primary action on the right, an optional
    // back/tertiary action on the left (spec §8: "Bottom-right: Primary.
    // Bottom-left: Back").
    public static HBoxContainer ActionRow(Container column, Button primary, Button back = null)
    {
        var row = UiFactory.HBox(8);
        row.SizeFlagsVertical = Control.SizeFlags.ShrinkEnd | Control.SizeFlags.Expand;
        if (back != null) row.AddChild(back);
        row.AddChild(UiFactory.Spacer(expand: true));
        primary.CustomMinimumSize = new Vector2(Mathf.Max(primary.CustomMinimumSize.X, 240f), 48f);
        row.AddChild(primary);
        column.AddChild(row);
        return row;
    }
}
