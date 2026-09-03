namespace FrontsOfWar.Core;

// Lets GameLoop drive a mission's per-tick system order without depending on
// FrontsOfWar.Map concretely (GDD §15.1 principle 3). MapRuntime implements
// this.
public interface ISimTickable
{
    void SimTick(float tickDeltaSeconds);
}
