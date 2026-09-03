using System.Collections.Generic;
using System.Linq;

namespace FrontsOfWar.Towers;

// Owns every placed Command Post and applies their non-stacking auras to
// combat towers each tick (GDD §19 prompt 15).
public class CommandPostManager
{
    private readonly List<CommandPostController> _posts = new();

    public IReadOnlyList<CommandPostController> Posts => _posts;

    public void Register(CommandPostController post) => _posts.Add(post);

    public void Tick(float tickDeltaSeconds, TowerManager towers, float tilePixelSize)
    {
        foreach (var post in _posts)
            post.SimTick(tickDeltaSeconds);

        foreach (var tower in towers.Towers)
        {
            tower.AuraRangeMultiplier = 1f;
            tower.AuraRateOfFireMultiplier = 1f;
        }

        foreach (var post in _posts)
            post.ApplyAuraTo(towers, tilePixelSize);
    }

    public int TotalCommandPointBonus() => _posts.Sum(p => p.CurrentCommandPointsPerWave);
    public int TotalSupplyPerWaveBonus() => _posts.Sum(p => p.CurrentSupplyPerWave);
}
