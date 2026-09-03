using FrontsOfWar.Map;

namespace FrontsOfWar.Enemies;

public static class SiegeRules
{
    public static bool ShouldSuppress(PadTag padTag, float distanceTiles, float rangeTiles)
        => padTag != PadTag.Enclosed && distanceTiles <= rangeTiles;
}
