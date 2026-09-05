using System;
using FrontsOfWar.Art;

namespace FrontsOfWar.Map.Authoring;

public sealed class MapPublishResult
{
    public bool Published { get; init; }
    public string Path { get; init; } = "";
    public MapProductionValidationResult Validation { get; init; }
}

public static class MapPublisher
{
    public static MapPublishResult Publish(MapDefinition map, string root = "res://assets/data/maps", string acceptedBy = "editor", bool allowReplace = false)
    {
        var validation = MapProductionValidator.Validate(map, ArtAssetCatalog.LoadFromProject(), true, root);
        if (!validation.CanPublish) return new MapPublishResult { Published = false, Validation = validation };
        if (map.Metadata == null || string.IsNullOrWhiteSpace(map.Metadata.Id))
        {
            validation.Diagnostics.Add(new MapDiagnostic { Severity = MapDiagnosticSeverity.Error, Code = "MAP_ID", Message = "A unique map ID is required." });
            return new MapPublishResult { Published = false, Validation = validation };
        }
        try
        {
            string existing = MapRegistry.ResolvePath(map.Metadata.Id, root);
            string canonical = $"{root.TrimEnd('/')}/{map.Metadata.Id}.tres";
            if (!allowReplace || !string.Equals(existing, canonical, StringComparison.Ordinal))
            {
                validation.Diagnostics.Add(new MapDiagnostic { Severity = MapDiagnosticSeverity.Error, Code = "DUPLICATE_MAP_ID", Message = $"Map ID already exists at '{existing}'." });
                return new MapPublishResult { Published = false, Validation = validation };
            }
        }
        catch (MapRegistryException) { }
        map.Metadata.Status = MapAuthoringStatus.Production;
        map.Provenance ??= new GenerationProvenance();
        map.Provenance.AcceptedBy = acceptedBy;
        string output = $"{root.TrimEnd('/')}/{map.Metadata.Id}.tres";
        MapSerializer.SaveAs(map, output);
        return new MapPublishResult { Published = true, Path = output, Validation = validation };
    }
}
