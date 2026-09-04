using Godot;
using FrontsOfWar.Waves;

namespace FrontsOfWar.Meta;

// A campaign mission's identity and framing (GDD §10.4, trimmed to what the
// briefing/results/progression flow needs). Wave composition itself already
// lives in WaveSequence/WaveDefinition (§19 prompt 19) — this Resource just
// points at one rather than duplicating it.
[GlobalClass]
public partial class MissionDefinition : Resource
{
    [Export] public string Id = "";
    [Export] public string Title = "";
    [Export] public int Act = 1;
    [Export] public string MapId = "";
    [Export] public WaveSequence WaveSequence;
    [Export] public string BriefingText = "";
    [Export] public StarObjectiveDefinition StarObjective;
}
