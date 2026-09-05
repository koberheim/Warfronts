#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.Map.Planning;

namespace FrontsOfWar.Editor.Generation;

public sealed class MapGenerationCandidate
{
    public MapPlanDefinition Plan { get; init; }
    public MapProductionValidationResult Diagnostics { get; init; }
}

public static class MapGenerationService
{
    public static IReadOnlyList<MapGenerationCandidate> Generate(MapLayoutTemplate template, MapGenerationConfiguration configuration)
    {
        if (template == null) throw new ArgumentNullException(nameof(template));
        var config = (configuration ?? new MapGenerationConfiguration()).Normalize();
        return MapCandidateGenerator.Generate(template, config.Seed, config.CandidateCount)
            .Select(plan => new MapGenerationCandidate { Plan = plan, Diagnostics = new MapProductionValidationResult() }).ToArray();
    }

    public static MapDefinition Convert(MapGenerationCandidate candidate, string acceptedBy = "editor")
        => MapPlanConverter.ToMapDefinition(candidate?.Plan ?? throw new ArgumentNullException(nameof(candidate)), acceptedBy: acceptedBy);
}
#endif
