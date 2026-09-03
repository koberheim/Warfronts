using Godot;
using System.Collections.Generic;

namespace FrontsOfWar.Core;

// Deterministic, per-mission RNG (GDD §15.1 principle 4). Gameplay code must
// route all randomness through an instance of this rather than GD.Randf /
// System.Random, so replays and balance testing get identical spawn ordering
// for a given seed.
public class SeededRandom
{
    private readonly RandomNumberGenerator _rng = new();

    public ulong Seed { get; }

    public SeededRandom(ulong seed)
    {
        Seed = seed;
        _rng.Seed = seed;
    }

    public float NextFloat() => _rng.Randf();
    public float NextFloat(float from, float to) => _rng.RandfRange(from, to);
    public int NextInt(int fromInclusive, int toExclusive) => _rng.RandiRange(fromInclusive, toExclusive - 1);
    public bool NextBool(float trueChance = 0.5f) => _rng.Randf() < trueChance;

    public T PickWeighted<T>(IReadOnlyList<T> items, IReadOnlyList<float> weights)
    {
        float total = 0f;
        for (int i = 0; i < weights.Count; i++) total += weights[i];

        float roll = NextFloat(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < items.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative) return items[i];
        }
        return items[^1];
    }
}
