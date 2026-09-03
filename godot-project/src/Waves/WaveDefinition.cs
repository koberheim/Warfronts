using Godot;

namespace FrontsOfWar.Waves;

// One wave (GDD §10.4). previewTags/isAirWave/isBossWave/buildTimeOverride
// land with the wave preview UI at M2 (§19 prompt 19).
[GlobalClass]
public partial class WaveDefinition : Resource
{
    [Export] public int WaveNumber = 1;
    [Export] public SpawnGroup[] Groups = System.Array.Empty<SpawnGroup>();
}
