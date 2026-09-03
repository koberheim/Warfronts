namespace FrontsOfWar.Core;

public readonly struct MissionCompletedEvent
{
    public readonly bool Victory;
    public MissionCompletedEvent(bool victory) => Victory = victory;
}

public readonly struct TutorialStepChangedEvent
{
    public readonly int Step;
    public TutorialStepChangedEvent(int step) => Step = step;
}
