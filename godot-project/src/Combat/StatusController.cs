using Godot;

namespace FrontsOfWar.Combat;

// Suppressed and Spotted (GDD §5.5, §19 prompt 16) — the game's only two
// non-damage statuses. One instance per enemy, owned by EnemyController.
// Suppressed refreshes on reapplication but is hard-capped at a total
// continuous duration (GDD's Bersaglieri Charge Post spec: "non-refreshing
// beyond 4s total from any source") — tracked via _suppressedTotalApplied,
// which resets only once Suppressed fully expires.
public class StatusController
{
    private float _suppressedRemaining;
    private float _suppressedTotalApplied;
    private float _spottedRemaining;

    public bool IsSuppressed => _suppressedRemaining > 0f;
    public bool IsSpotted => _spottedRemaining > 0f;
    public void Reset()
    {
        _suppressedRemaining = 0f;
        _suppressedTotalApplied = 0f;
        _spottedRemaining = 0f;
    }

    public void ApplySuppressed(float durationSeconds, float hardCapSeconds)
    {
        if (_suppressedRemaining <= 0f) _suppressedTotalApplied = 0f;

        float headroom = Mathf.Max(0f, hardCapSeconds - _suppressedTotalApplied);
        _suppressedRemaining = Mathf.Min(Mathf.Max(_suppressedRemaining, durationSeconds), headroom);
    }

    public void ApplySpotted(float durationSeconds)
        => _spottedRemaining = Mathf.Max(_spottedRemaining, durationSeconds);

    public void Tick(float tickDeltaSeconds)
    {
        if (_suppressedRemaining > 0f)
        {
            _suppressedRemaining -= tickDeltaSeconds;
            _suppressedTotalApplied += tickDeltaSeconds;
        }
        if (_spottedRemaining > 0f) _spottedRemaining -= tickDeltaSeconds;
    }
}
