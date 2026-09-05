using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using FrontsOfWar.Map.Authoring;

namespace FrontsOfWar.Map;

// Runtime behavior for GDD §11.2's map gimmicks - "one boolean or one timer"
// per gimmick, authored per map via MapGimmick/RuntimeGimmickData. Ticked
// every SimTick like every other per-mission manager (see
// MapRuntime.Simulation.cs); every query below is a pure lookup consulted by
// EnemyController (Mud/Canopy) and TowerController (Sandstorm, via
// MapRuntime resetting a per-tick multiplier the same way CommandPostManager
// does for its aura), so each gimmick type is independently testable without
// a live mission or an authored map (R08 acceptance) - none of the eight
// launch maps exist yet (R09).
//
// Tide's WaveRunner integration (rerouting new spawns off a flooded path
// onto a fallback) is deliberately not wired here: SpawnGroup has no
// authored "fallback path if primary is closed" concept yet, and there is
// no real tidal map (M7 Coastal Fortification, R09) to validate the exact
// intended behavior against. IsPathAvailable below is real and tested; only
// that one consumer is deferred.
public sealed class GimmickSystem
{
    public const string TideType = "tide";
    public const string SandstormType = "sandstorm";
    public const string MudType = "mud";
    public const string CanopyType = "canopy";

    private readonly List<GimmickInstance> _instances;

    public GimmickSystem(IReadOnlyList<RuntimeGimmickData> gimmicks)
    {
        _instances = (gimmicks ?? Array.Empty<RuntimeGimmickData>())
            .Where(gimmick => gimmick != null && gimmick.Enabled)
            .Select(gimmick => new GimmickInstance(gimmick))
            .ToList();
    }

    public void Tick(float deltaSeconds)
    {
        foreach (var instance in _instances) instance.Tick(deltaSeconds);
    }

    // Tide (GDD §11.1 M7): the path floods for its active window every
    // cycle, closing it entirely.
    public bool IsPathAvailable(string pathId)
    {
        if (string.IsNullOrEmpty(pathId)) return true;
        foreach (var instance in _instances)
            if (instance.Data.Type == TideType && instance.AppliesTo(pathId) && instance.IsActive) return false;
        return true;
    }

    // Sandstorm (GDD §11.1 M2 variant, Mission 10): a global range
    // multiplier while active; the strictest of any concurrently active
    // instance. 1f (no-op) when none is active.
    public float GlobalRangeMultiplier()
    {
        float multiplier = 1f;
        foreach (var instance in _instances)
            if (instance.Data.Type == SandstormType && instance.IsActive)
                multiplier = Mathf.Min(multiplier, instance.RangeMultiplier);
        return multiplier;
    }

    // Mud (GDD §11.1 M8): vehicles only, never infantry - the caller passes
    // its own vehicle classification (see GimmickRules.IsVehicle) so this
    // system stays ignorant of EnemyArchetype specifics.
    public float SpeedMultiplierForPath(string pathId, bool isVehicle)
    {
        if (!isVehicle || string.IsNullOrEmpty(pathId)) return 1f;
        float multiplier = 1f;
        foreach (var instance in _instances)
            if (instance.Data.Type == MudType && instance.AppliesTo(pathId))
                multiplier = Mathf.Min(multiplier, instance.SpeedMultiplier);
        return multiplier;
    }

    // Canopy (GDD §11.1 M6): a static per-path flag, not a timer - "sections
    // of road... are Concealed unless a Command Post covers them" reuses
    // E11's existing reveal system entirely; this only supplies the source
    // flag EnemyController.IsConcealed ORs in.
    public bool IsPathConcealed(string pathId)
    {
        if (string.IsNullOrEmpty(pathId)) return false;
        foreach (var instance in _instances)
            if (instance.Data.Type == CanopyType && instance.AppliesTo(pathId)) return true;
        return false;
    }

    private sealed class GimmickInstance
    {
        public readonly RuntimeGimmickData Data;
        public readonly float RangeMultiplier;
        public readonly float SpeedMultiplier;

        private readonly float _cycleSeconds;
        private readonly float _activeSeconds;
        private float _elapsed;

        public GimmickInstance(RuntimeGimmickData data)
        {
            Data = data;
            _cycleSeconds = Parameter(data, "cycle_seconds", 90f);
            _activeSeconds = Parameter(data, "active_seconds", 20f);
            RangeMultiplier = Parameter(data, "range_multiplier", 0.75f);
            SpeedMultiplier = Parameter(data, "speed_multiplier", 0.6f);
        }

        public bool AppliesTo(string pathId) => Data.PathIds.Count == 0 || Data.PathIds.Contains(pathId);

        // "Active" for the first ActiveSeconds of every CycleSeconds-long
        // cycle (matches the Sandstorm variant's own spec: "20s on a 60s
        // cycle"). A zero/negative cycle means always active - a
        // permanently closed route or a constant range penalty is a
        // legitimate authoring choice, not an error condition.
        public bool IsActive => _cycleSeconds <= 0f || _elapsed % _cycleSeconds < _activeSeconds;

        public void Tick(float deltaSeconds) => _elapsed += deltaSeconds;

        private static float Parameter(RuntimeGimmickData data, string key, float fallback)
            => data.Parameters.TryGetValue(key, out var raw)
                && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value : fallback;
    }
}

// Small geometry/classification helpers shared by the arc-clipped-range
// gimmick (GDD §11.1 M4 Ruined Town - the one genuinely expensive gimmick,
// "a pie-slice rather than a circle") and Mud's vehicle filter. Kept
// separate from GimmickSystem because both are pure functions consulted by
// TowerController/EnemyController directly, not per-mission state.
public static class GimmickRules
{
    // Full wall/line-of-sight geometry is out of scope (no terrain-collision
    // model exists yet - see docs/DECISIONS.md); this covers the targeting
    // half of the GDD's own cost estimate ("a pie-slice range shape and a
    // line-of-sight check"), which is what makes the constraint testable
    // independently of any specific map's wall geometry.
    public static bool IsWithinArc(Vector2 origin, float facingDegrees, float halfAngleDegrees, Vector2 target)
    {
        if (halfAngleDegrees >= 180f) return true;
        var offset = target - origin;
        if (offset == Vector2.Zero) return true;
        float angleDegrees = Mathf.RadToDeg(offset.Angle());
        float delta = Mathf.Wrap(angleDegrees - facingDegrees, -180f, 180f);
        return Mathf.Abs(delta) <= halfAngleDegrees;
    }

    public static bool IsVehicle(Enemies.EnemyArchetype archetype) => archetype
        is Enemies.EnemyArchetype.LightVehicle or Enemies.EnemyArchetype.MediumArmor or Enemies.EnemyArchetype.HeavyArmor;
}
