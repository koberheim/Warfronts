namespace FrontsOfWar.Map;

// Event payloads published on FrontsOfWar.Core.EventBus by map-layer nodes.
// Kept as plain structs so Publish<T> never allocates on the hot path.
public readonly struct BuildPadHoverChangedEvent
{
    public readonly BuildPad Pad;
    public readonly bool IsHovered;

    public BuildPadHoverChangedEvent(BuildPad pad, bool isHovered)
    {
        Pad = pad;
        IsHovered = isHovered;
    }
}

public readonly struct BuildPadClickedEvent
{
    public readonly BuildPad Pad;

    public BuildPadClickedEvent(BuildPad pad)
    {
        Pad = pad;
    }
}
