using Godot;
using FrontsOfWar.Core;

namespace FrontsOfWar.Enemies;

// B4 Fortress Assault Group (GDD §10.3): thresholds are fractions of max HP
// at which the boss advances to the next phase. Distinct from
// BossPhaseController (B1's specific 2-phase armor-skirt model) since B4
// needs an arbitrary number of one-way phase transitions instead. Phase
// index only ever increases - a boss never un-transitions on a heal,
// matching every GDD boss's one-way narrative ("halts and becomes a Siege
// platform"). Phase 2 (index 1) reuses the Siege archetype's existing
// SiegeBombard*/AddDefinition fields and EnemySiegeBombardEvent/
// BossAddsRequestedEvent plumbing rather than inventing parallel systems.
public sealed class MultiPhaseBossController
{
    private readonly EnemyDefinition _definition;
    private float _addTimer;
    private float _bombardTimer;
    private int _pendingAdds;
    private float _haltRemaining;

    // 0-based: 0 is the authored baseline (before any threshold is crossed).
    public int PhaseIndex { get; private set; }

    // GDD §10.3: "each phase transition has a 3-second visible telegraph
    // (halt, animation, audio sting, HUD banner)." This covers the halt;
    // the audio/animation/banner polish is presentation work, same gap as
    // B1's "visual playtest still manual" note.
    public bool IsHalted => _haltRemaining > 0f;

    public MultiPhaseBossController(EnemyDefinition definition) => _definition = definition;

    public void UpdatePhase(float currentHp, float maxHp)
    {
        if (maxHp <= 0f) return;
        float fraction = currentHp / maxHp;
        var thresholds = _definition.MultiPhaseHpThresholds ?? System.Array.Empty<float>();
        int target = 0;
        for (int i = 0; i < thresholds.Length; i++)
            if (fraction <= thresholds[i]) target = i + 1;
        if (target <= PhaseIndex) return;

        PhaseIndex = target;
        _haltRemaining = 3f;
        _addTimer = 0f;
        _bombardTimer = 0f;
        EventBus.Instance?.Publish(new BossPhaseTransitionEvent(PhaseIndex));
    }

    public void Tick(float deltaSeconds)
    {
        if (_haltRemaining > 0f) { _haltRemaining -= deltaSeconds; return; }
        if (PhaseIndex != 1) return; // only the Siege phase bombards/spawns adds

        if (_definition.AddDefinition != null)
        {
            _addTimer += deltaSeconds;
            if (_addTimer >= Mathf.Max(0.1f, _definition.AddIntervalSeconds))
            {
                _addTimer = 0f;
                _pendingAdds += Mathf.Max(0, _definition.AddCount);
            }
        }
        _bombardTimer += deltaSeconds;
    }

    public bool ConsumeBombardReady(out float rangeTiles, out float durationSeconds)
    {
        rangeTiles = _definition.SiegeBombardRangeTiles;
        durationSeconds = _definition.SiegeSuppressionDurationSeconds;
        if (PhaseIndex != 1 || IsHalted) return false;
        if (_bombardTimer < Mathf.Max(0.1f, _definition.SiegeBombardIntervalSeconds)) return false;
        _bombardTimer = 0f;
        return true;
    }

    public int ConsumePendingAdds()
    {
        int count = _pendingAdds;
        _pendingAdds = 0;
        return count;
    }

    public bool IsSuppressionImmune => PhaseIndex >= 2;
    public bool IsSiegePhase => PhaseIndex == 1;
    public float SpeedMultiplier => PhaseIndex >= 2 ? Mathf.Max(1f, _definition.Phase3SpeedMultiplier) : 1f;
}

public readonly struct BossPhaseTransitionEvent
{
    public readonly int PhaseIndex;
    public BossPhaseTransitionEvent(int phaseIndex) => PhaseIndex = phaseIndex;
}
