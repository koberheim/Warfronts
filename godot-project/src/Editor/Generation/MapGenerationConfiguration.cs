#if DEBUG
using System;
using FrontsOfWar.Map.Planning;

namespace FrontsOfWar.Editor.Generation;

public sealed class MapGenerationConfiguration
{
    public string TemplateId { get; init; } = "";
    public ulong Seed { get; init; } = 1;
    public int CandidateCount { get; init; } = 12;
    public int TargetPads { get; init; } = 22;
    public string Theater { get; init; } = "";
    public bool RequireSeparateRoutes { get; init; }

    public MapGenerationConfiguration Normalize()
        => new() { TemplateId = TemplateId ?? "", Seed = Seed, CandidateCount = Math.Clamp(CandidateCount, 1, 100), TargetPads = Math.Clamp(TargetPads, 1, 64), Theater = Theater ?? "", RequireSeparateRoutes = RequireSeparateRoutes };

    public bool IsDeterministicEquivalent(MapGenerationConfiguration other)
        => other != null && TemplateId == other.TemplateId && Seed == other.Seed && CandidateCount == other.CandidateCount && TargetPads == other.TargetPads && Theater == other.Theater && RequireSeparateRoutes == other.RequireSeparateRoutes;
}
#endif
