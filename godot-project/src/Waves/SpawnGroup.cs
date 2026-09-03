using Godot;
using FrontsOfWar.Enemies;

namespace FrontsOfWar.Waves;

// One spawn group within a wave (GDD §10.4). pathId/spawnPointId (for
// multi-entry maps) and eliteFlag/hpMultiplierOverride are deferred until a
// map with more than one entry point exists (M4+).
[GlobalClass]
public partial class SpawnGroup : Resource
{
    [Export] public EnemyDefinition Enemy;
    [Export] public int Count = 1;
    [Export] public float StartDelaySeconds;
    [Export] public float IntervalSeconds = 1f;
}
