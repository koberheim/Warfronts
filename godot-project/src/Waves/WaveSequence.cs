using Godot;

namespace FrontsOfWar.Waves;

// Authored mission wave list used by the M2 grey-box mission. Keeping the
// sequence in a Resource makes the preview and future mission flow consume
// the same data rather than duplicating wave order in code.
[GlobalClass]
public partial class WaveSequence : Resource
{
    [Export] public WaveDefinition[] Waves = System.Array.Empty<WaveDefinition>();
}
