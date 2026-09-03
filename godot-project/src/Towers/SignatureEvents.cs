using Godot;

namespace FrontsOfWar.Towers;

public readonly struct SignatureClickedEvent
{
    public readonly SignatureControllerBase Signature;
    public SignatureClickedEvent(SignatureControllerBase signature) => Signature = signature;
}

public readonly struct SignatureActivatedEvent
{
    public readonly SignatureControllerBase Signature;
    public readonly Vector2 TargetPoint;
    public readonly int ChargesRemaining;
    public SignatureActivatedEvent(SignatureControllerBase signature, Vector2 targetPoint, int chargesRemaining)
    {
        Signature = signature;
        TargetPoint = targetPoint;
        ChargesRemaining = chargesRemaining;
    }
}

public readonly struct SignatureTelegraphEvent
{
    public readonly SignatureControllerBase Signature;
    public readonly Vector2 TargetPoint;
    public readonly float DurationSeconds;
    public SignatureTelegraphEvent(SignatureControllerBase signature, Vector2 targetPoint, float durationSeconds)
    {
        Signature = signature;
        TargetPoint = targetPoint;
        DurationSeconds = durationSeconds;
    }
}
