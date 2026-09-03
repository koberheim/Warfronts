namespace FrontsOfWar.Core;

// Converts the player's speed selection into "ticks per physics frame" so
// GameLoop's fixed-order simulation stays exact at all speeds (GDD §15.4).
// Deliberately never touches Engine.TimeScale, which would also affect audio
// pitch and physics substeps we don't want tied to gameplay speed.
public class TimeController
{
    public enum Speed
    {
        Normal = 1,
        Fast = 2,
        Fastest = 3,
    }

    public Speed CurrentSpeed { get; private set; } = Speed.Normal;
    public bool IsPaused { get; private set; }

    public int TicksThisFrame => IsPaused ? 0 : (int)CurrentSpeed;

    public void SetSpeed(Speed speed) => CurrentSpeed = speed;

    public void CycleSpeed()
    {
        CurrentSpeed = CurrentSpeed switch
        {
            Speed.Normal => Speed.Fast,
            Speed.Fast => Speed.Fastest,
            _ => Speed.Normal,
        };
    }

    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;
    public void TogglePause() => IsPaused = !IsPaused;
}
