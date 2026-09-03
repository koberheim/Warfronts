namespace FrontsOfWar.Waves;

public readonly struct WaveStartedEvent
{
    public readonly int WaveNumber;
    public WaveStartedEvent(int waveNumber) => WaveNumber = waveNumber;
}

// Published once every SpawnGroup in the wave has finished spawning — not
// once every enemy from the wave is dead. "All enemies cleared" is a
// higher-level concern for the mission flow landing at M3.
public readonly struct WaveSpawningCompleteEvent
{
    public readonly int WaveNumber;
    public WaveSpawningCompleteEvent(int waveNumber) => WaveNumber = waveNumber;
}

public readonly struct BuildPhaseStartedEvent
{
    public readonly int NextWaveNumber;
    public readonly float DurationSeconds;
    public BuildPhaseStartedEvent(int nextWaveNumber, float durationSeconds)
    {
        NextWaveNumber = nextWaveNumber;
        DurationSeconds = durationSeconds;
    }
}
