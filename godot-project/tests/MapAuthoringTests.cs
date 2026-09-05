#if DEBUG
using System;
using System.Collections.Generic;
using Chickensoft.GoDotTest;
using Godot;
using FrontsOfWar.Editor.Documents;
using FrontsOfWar.Editor.Editing;
using FrontsOfWar.Editor.Rendering;
using FrontsOfWar.Editor.Palette;
using FrontsOfWar.Map;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.Map.Planning;

namespace FrontsOfWar.Tests;

public class MapAuthoringTests : TestClass
{
    public MapAuthoringTests(Node testScene) : base(testScene) { }

    [Test]
    public void StableIdsAndCoordinateConversionsUseOneContract()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < 256; i++)
        {
            string id = MapObjectId.New("Tower Node");
            Require(MapObjectId.IsValid(id), "generated object ID is valid");
            Require(ids.Add(id), "generated object ID is unique");
        }

        Require(MapObjectId.NormalizePrefix("  7 / Bad Prefix ") == "object_7_bad_prefix", "ID prefix normalization");
        var tile = new Vector2(1.5f, 2f);
        var pixel = MapCoordinateSystem.TileToPixel(tile, 64f);
        Require(pixel == new Vector2(96f, 128f), "tile-to-pixel conversion");
        Require(MapCoordinateSystem.PixelToTile(pixel, 64f) == tile, "pixel-to-tile round trip");
        Require(MapCoordinateSystem.SnapToTile(new Vector2(1.49f, 2.51f)) == new Vector2(1f, 3f), "tile snapping");
        Require(MapCoordinateSystem.NormalizeQuarterTurns(-1) == 3, "quarter-turn normalization");
        RequireApproximately(1.5f, MapCoordinateSystem.ClampUniformScale(2f, 0.5f, 1.5f), "scale clamp");
    }

    [Test]
    public void MapDefinitionCoversEveryAuthoredCategoryAndFindsStructuralErrors()
    {
        var map = BuildTinyMap();
        var valid = MapDefinitionValidator.Validate(map);
        Require(valid.IsValid, $"tiny map validates: {string.Join("; ", valid.Errors)}");
        Require(map.Terrain.Length == 2 && map.Assets.Length == 2 && map.Clusters.Length == 1, "visual categories exist");
        Require(map.Paths.Length == 1 && map.AirCorridors.Length == 1, "route categories exist");
        Require(map.TowerNodes.Length == 1 && map.Markers.Length == 4, "gameplay markers exist");
        Require(map.Zones.Length == 1 && map.Gimmicks.Length == 1 && map.Provenance != null, "zone/gimmick/provenance exist");

        map.Assets[0].Scale = new Vector2(1f, 2f);
        map.TowerNodes[0].Id = map.Terrain[0].Id;
        var invalid = MapDefinitionValidator.Validate(map);
        Require(!invalid.IsValid, "invalid map is rejected");
        Require(invalid.Errors.Exists(error => error.Contains("uniform", StringComparison.OrdinalIgnoreCase)), "non-uniform scale is diagnosed");
        Require(invalid.Errors.Exists(error => error.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)), "duplicate ID is diagnosed");
    }

    [Test]
    public void TresRoundTripIsDeterministicAndNormalized()
    {
        string directory = ScratchDirectory();
        string firstPath = $"{directory}/first.tres";
        string secondPath = $"{directory}/second.tres";
        try
        {
            MapSerializer.SaveAs(BuildTinyMap(), firstPath);
            MapSerializer.SaveAs(BuildTinyMap(), secondPath);
            string first = FileAccess.GetFileAsString(firstPath);
            string second = FileAccess.GetFileAsString(secondPath);
            Require(first == second, $"equivalent maps serialize identically: {DescribeFirstDifference(first, second)}");
            Require(!FileAccess.FileExists($"{directory}/first.tmp.tres"), "temporary save file is removed");

            var loaded = MapSerializer.Load(firstPath);
            Require(loaded.Metadata.Id == "tiny_authoring_test", "metadata round trips");
            Require(loaded.Terrain[0].Id == "terrain_a" && loaded.Terrain[1].Id == "terrain_b", "resources normalize by stable ID");
            Require(loaded.Paths[0].Points[1] == new Vector2(3f, 2f), "path geometry round trips in order");
            Require(loaded.Gimmicks[0].Parameters[0].Key == "period_seconds", "metadata normalizes by key");
        }
        finally
        {
            Cleanup(directory, "first.tres", "second.tres");
        }
    }

    [Test]
    public void EmptyDraftRoundTripsWithoutInventingGameplayContent()
    {
        string directory = ScratchDirectory();
        string path = $"{directory}/empty.tres";
        try
        {
            var empty = MapDefinition.CreateNew("empty_draft", "Empty Draft", 4, 3);
            MapSerializer.SaveAs(empty, path);
            var loaded = MapSerializer.Load(path);
            Require(loaded.Paths.Length == 0 && loaded.Terrain.Length == 0 && loaded.TowerNodes.Length == 0,
                "empty draft remains empty");
            Require(loaded.Metadata.WidthTiles == 4 && loaded.Metadata.HeightTiles == 3, "empty draft dimensions round trip");
        }
        finally
        {
            Cleanup(directory, "empty.tres");
        }
    }

    [Test]
    public void SchemaAndCorruptResourceFailuresAreExplicit()
    {
        var missing = MapDefinition.CreateNew("missing_schema", "Missing Schema");
        missing.SchemaVersion = 0;
        var missingError = RequireThrows<MapSchemaException>(() => MapSchemaMigrator.ValidateAndMigrate(missing));
        Require(missingError.Kind == MapSchemaErrorKind.MissingVersion, "missing schema error kind");

        var future = MapDefinition.CreateNew("future_schema", "Future Schema");
        future.SchemaVersion = MapSchemaMigrator.CurrentSchemaVersion + 1;
        var futureError = RequireThrows<MapSchemaException>(() => MapSchemaMigrator.ValidateAndMigrate(future));
        Require(futureError.Kind == MapSchemaErrorKind.FutureVersion, "future schema error kind");

        string directory = ScratchDirectory();
        string corruptPath = $"{directory}/corrupt.tres";
        try
        {
            DirAccess.MakeDirRecursiveAbsolute(directory);
            using (var file = FileAccess.Open(corruptPath, FileAccess.ModeFlags.Write)) file.StoreString("not a Godot resource");
            RequireThrows<MapSerializationException>(() => MapSerializer.Load(corruptPath));
        }
        finally
        {
            Cleanup(directory, "corrupt.tres");
        }
    }

    [Test]
    public void InvalidSaveNeverReplacesLastKnownGoodMap()
    {
        string directory = ScratchDirectory();
        string path = $"{directory}/protected.tres";
        try
        {
            var map = BuildTinyMap();
            MapSerializer.SaveAs(map, path);
            string knownGood = FileAccess.GetFileAsString(path);
            map.Terrain[0].RotationQuarterTurns = 8;
            RequireThrows<MapSerializationException>(() => MapSerializer.SaveAs(map, path));
            Require(FileAccess.GetFileAsString(path) == knownGood, "failed save preserves known-good file");
        }
        finally
        {
            Cleanup(directory, "protected.tres");
        }
    }

    [Test]
    public void DocumentLifecycleCannotSilentlyDiscardDirtyState()
    {
        string directory = ScratchDirectory();
        string path = $"{directory}/document.tres";
        try
        {
            var document = new MapDocument();
            Require(document.TryNew(MapDefinition.CreateNew("document_test", "Document Test")), "new document opens");
            Require(document.IsDirty && string.IsNullOrEmpty(document.FilePath), "new document starts untitled and dirty");
            Require(!document.TryClose(), "dirty document refuses close without a decision");
            Require(!document.TryClose(() => UnsavedChangesChoice.Cancel), "cancel keeps dirty document open");

            document.SaveAs(path);
            Require(!document.IsDirty && document.FilePath == path, "Save As establishes clean file path");
            document.MarkDirty();
            Require(!document.TryNew(
                MapDefinition.CreateNew("replacement", "Replacement"),
                () => UnsavedChangesChoice.Save,
                () => false), "failed save refuses replacement");
            Require(document.Current.Metadata.Id == "document_test", "failed replacement preserves current map");
            Require(document.TryClose(() => UnsavedChangesChoice.Discard), "explicit discard permits close");
            Require(!document.IsOpen, "closed document clears state");
        }
        finally
        {
            Cleanup(directory, "document.tres");
        }
    }

    [Test]
    public void RegistryLoaderAndSceneFactoryResolveAndRenderAuthoredMap()
    {
        string directory = ScratchDirectory();
        string path = $"{directory}/tiny.tres";
        try
        {
            MapSerializer.SaveAs(BuildTinyMap(), path);
            Require(MapRegistry.ResolvePath("tiny_authoring_test", directory) == path, "registry resolves map ID");
            var loaded = MapLoader.Load(path);
            var snapshot = MapSceneFactory.Build(loaded);
            Require(snapshot.WidthTiles == 8 && snapshot.HeightTiles == 6, "render snapshot keeps map dimensions");
            Require(snapshot.Items.Length == 12 && snapshot.Paths.Length == 2, "render snapshot includes every visual category");
            Require(Array.Exists(snapshot.Items, item => item.Id == "pad_a"), "tower node is discoverable in render snapshot");
        }
        finally { Cleanup(directory, "tiny.tres"); }
    }

    [Test]
    public void SelectionAndCommandHistoryTransformUndoAndRedoExactly()
    {
        var map = BuildTinyMap();
        var document = new MapDocument();
        Require(document.TryNew(map), "editor document opens for command test");
        var before = ((MapAssetInstance)MapObjectLocator.Find(map, "asset_a").Resource).PositionTiles;
        document.Apply(MapTransformCommand.Move(map, new[] { "asset_a" }, new Vector2(1f, 2f)));
        var asset = () => (MapAssetInstance)MapObjectLocator.Find(map, "asset_a").Resource;
        Require(asset().PositionTiles == before + new Vector2(1f, 2f), "move command changes selected object");
        Require(document.Undo() && asset().PositionTiles == before, "undo restores exact transform");
        Require(document.Redo() && asset().PositionTiles == before + new Vector2(1f, 2f), "redo reapplies exact transform");

        var selection = new SelectionService();
        selection.Set("asset_a"); selection.Toggle("asset_b");
        Require(selection.SelectedIds.Count == 2 && selection.PrimaryId == "asset_b", "selection supports additive multi-select");
    }

    [Test]
    public void DeleteDuplicateCopyAndPastePreserveUniqueIDs()
    {
        var map = BuildTinyMap();
        var document = new MapDocument();
        Require(document.TryNew(map), "editor document opens for object operations");
        document.Apply(MapObjectOperations.Duplicate(map, new[] { "asset_a" }));
        Require(map.Assets.Length == 3 && MapDefinitionValidator.Validate(map).IsValid, "duplicate adds a valid unique object");
        Require(document.Undo() && map.Assets.Length == 2, "duplicate is undoable");
        MapClipboard.Copy(map, new[] { "asset_a" });
        document.Apply(MapClipboard.Paste(map, new Vector2(2f, 0f)));
        Require(map.Assets.Length == 3 && MapDefinitionValidator.Validate(map).IsValid, "paste remaps IDs and preserves validity");
        document.Apply(MapObjectOperations.Delete(map, new[] { "asset_a" }));
        Require(map.Assets.Length == 2 && MapDefinitionValidator.Validate(map).IsValid, "delete is a validated command");
    }

    [Test]
    public void CompoundCommandsRollBackWhenOneOperationFails()
    {
        var map = BuildTinyMap();
        var document = new MapDocument();
        Require(document.TryNew(map), "editor document opens for compound command test");
        Vector2 before = map.Assets[0].PositionTiles;
        var commands = new IMapEditCommand[]
        {
            MapTransformCommand.Move(map, new[] { "asset_b" }, Vector2.One),
            new MapSnapshotCommand("forced failure", _ => throw new InvalidOperationException("expected test failure")),
        };
        RequireThrows<InvalidOperationException>(() => document.Apply(new CompoundMapEditCommand("compound", commands)));
        Require(map.Assets[0].PositionTiles == before, "failed compound restores earlier commands");
        Require(!document.CanUndo, "failed compound does not enter history");
    }

    [Test]
    public void CatalogQueryPlacementAndPublishDiagnosticsUseStableIDs()
    {
        var catalog = new FrontsOfWar.Art.ArtAssetCatalog
        {
            Entries = new List<FrontsOfWar.Art.ArtAssetEntry>
            {
                new() { Id = "vegetation.tree", Name = "Tree", Category = "Vegetation", Status = "APPROVED", Tags = new List<string> { "hedge" } },
                new() { Id = "review.crate", Name = "Supply Crate", Category = "Flavor", Status = "REVIEW" },
            },
        };
        var results = ArtPaletteService.Query(catalog, new ArtPaletteQuery { Search = "hedge", ApprovedOnly = true });
        Require(results.Count == 1 && results[0].Id == "vegetation.tree", "catalog query filters tags and approval");
        var map = MapDefinition.CreateNew("palette_test", "Palette Test", 8, 6);
        map.Markers = new[]
        {
            new GameplayMarker { Id = "entry_0", Kind = GameplayMarkerKind.GroundEntry, PositionTiles = Vector2.Zero },
            new GameplayMarker { Id = "objective_0", Kind = GameplayMarkerKind.Objective, PositionTiles = new Vector2(7, 5) },
        };
        map.Paths = new[] { new PathDefinition { Id = "path_0", EntryMarkerId = "entry_0", ObjectiveMarkerId = "objective_0", Points = new[] { Vector2.Zero, new Vector2(7, 5) } } };
        map.TowerNodes = new[] { new TowerPlacementNode { Id = "pad_0", PositionTiles = new Vector2(4, 3) } };
        map.Assets = Array.Empty<MapAssetInstance>();
        var document = new MapDocument();
        Require(document.TryNew(map), "palette test document opens");
        document.Apply(MapAssetCommands.AddAsset(map, "vegetation.tree", "Vegetation", new Vector2(2.4f, 2.6f)));
        Require(map.Assets.Length == 1 && map.Assets[0].PositionTiles == new Vector2(2, 3), "catalog placement snaps to tile");
        var publish = MapProductionValidator.Validate(map, catalog, true);
        Require(publish.CanPublish, "approved catalog map passes production diagnostics");
    }

    [Test]
    public void PlannerConversionIsDeterministicAndRuntimeDataKeepsPathsPadsAndAir()
    {
        var plan = new MapPlanDefinition
        {
            Id = "candidate_42", DisplayName = "Candidate 42", Seed = 42,
            Canvas = new PlanPoint(100, 56), Entries = new List<PlanPoint> { new(2, 20) }, Objective = new PlanPoint(96, 28),
            Paths = new List<PathPlan> { new() { Id = "path_0", StartEntryId = "entry_0", Points = new List<PlanPoint> { new(2, 20), new(96, 28) } } },
            Pads = new List<PadPlan> { new() { Id = "pad_0", Position = new PlanPoint(40, 20), Tag = "Elevated" } },
        };
        var map = MapPlanConverter.ToMapDefinition(plan);
        var runtime = MapRuntimeDataFactory.Build(map);
        Require(runtime.Paths.Count == 1 && runtime.Pads.Count == 1 && runtime.Pads[0].Tag == PadTag.Elevated, "planner output converts to runtime path and pad data");
        Require(map.Provenance != null && map.Provenance.SourceTemplateId == "", "conversion preserves provenance fields");
        Require(plan.Paths[0].Points[0].X == 2, "source plan is not mutated");
    }

    private static MapDefinition BuildTinyMap()
    {
        var map = MapDefinition.CreateNew("tiny_authoring_test", "Tiny Authoring Test", 8, 6);
        map.Metadata.Theater = "western_europe";
        map.Metadata.Biome = "bocage";
        map.Metadata.TerrainSetId = "terrain_test";
        map.Metadata.CampaignUsageIds = new[] { "mission_b", "mission_a" };
        map.Terrain = new[]
        {
            new TerrainInstance { Id = "terrain_b", AssetId = "terrain.grass", Cell = new Vector2I(1, 0), EditorTags = new[] { "ground", "base" } },
            new TerrainInstance { Id = "terrain_a", AssetId = "terrain.road", Cell = new Vector2I(0, 0) },
        };
        map.Assets = new[]
        {
            new MapAssetInstance { Id = "asset_b", AssetId = "review.tree", Category = "Vegetation", PositionTiles = new Vector2(2f, 1f) },
            new MapAssetInstance { Id = "asset_a", AssetId = "review.crate", Category = "Flavor", PositionTiles = new Vector2(4f, 3f) },
        };
        map.Clusters = new[]
        {
            new ClusterInstance
            {
                Id = "cluster_a", PositionTiles = new Vector2(5f, 1f),
                Children = new[] { new MapAssetInstance { Id = "cluster_child_a", AssetId = "review.sandbags", Category = "Flavor" } },
            },
        };
        map.Markers = new[]
        {
            new GameplayMarker { Id = "objective_main", Kind = GameplayMarkerKind.Objective, PositionTiles = new Vector2(7f, 2f) },
            new GameplayMarker { Id = "entry_main", Kind = GameplayMarkerKind.GroundEntry, PositionTiles = new Vector2(0f, 2f) },
            new GameplayMarker { Id = "air_entry", Kind = GameplayMarkerKind.AirEntry, PositionTiles = new Vector2(0f, 0f) },
            new GameplayMarker { Id = "air_objective", Kind = GameplayMarkerKind.AirObjective, PositionTiles = new Vector2(7f, 5f) },
        };
        map.Paths = new[]
        {
            new PathDefinition
            {
                Id = "path_main", EntryMarkerId = "entry_main", ObjectiveMarkerId = "objective_main",
                Points = new[] { new Vector2(0f, 2f), new Vector2(3f, 2f), new Vector2(7f, 2f) },
            },
        };
        map.AirCorridors = new[]
        {
            new MapAirCorridorDefinition
            {
                Id = "air_main", EntryMarkerId = "air_entry", ObjectiveMarkerId = "air_objective",
                EntryPositionTiles = Vector2.Zero, ObjectivePositionTiles = new Vector2(7f, 5f), WidthTiles = 3f,
            },
        };
        map.TowerNodes = new[]
        {
            new TowerPlacementNode { Id = "pad_a", PositionTiles = new Vector2(3f, 1f), Tag = PadTag.Standard },
        };
        map.Zones = new[]
        {
            new MapZone { Id = "zone_a", Kind = MapZoneKind.GameplayLane, CenterTiles = new Vector2(4f, 2f), SizeTiles = new Vector2(8f, 3f) },
        };
        map.Gimmicks = new[]
        {
            new MapGimmick
            {
                Id = "gimmick_a", Type = "tide_cycle", PathIds = new[] { "path_main" },
                Parameters = new[] { new MapProperty { Key = "period_seconds", Value = "90" } },
            },
        };
        map.Provenance = new GenerationProvenance
        {
            SourceTemplateId = "template_test", Seed = 42, GeneratorVersion = "test",
            ConvertedAtUtc = "2026-09-04T00:00:00Z",
            InitialMetrics = new[] { new MapProperty { Key = "score", Value = "1" } },
        };
        return map;
    }

    private static string ScratchDirectory()
        => $"user://map_authoring_tests/{Guid.NewGuid():N}";

    private static void Cleanup(string directory, params string[] files)
    {
        foreach (string file in files)
        {
            string path = $"{directory}/{file}";
            if (FileAccess.FileExists(path)) DirAccess.RemoveAbsolute(path);
            string backup = path.TrimSuffix(".tres") + ".backup.tres";
            if (FileAccess.FileExists(backup)) DirAccess.RemoveAbsolute(backup);
            string temp = path.TrimSuffix(".tres") + ".tmp.tres";
            if (FileAccess.FileExists(temp)) DirAccess.RemoveAbsolute(temp);
        }
        if (DirAccess.DirExistsAbsolute(directory)) DirAccess.RemoveAbsolute(directory);
    }

    private static T RequireThrows<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T exception) { return exception; }
        throw new InvalidOperationException($"Assertion failed: expected {typeof(T).Name}");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {message}");
    }

    private static void RequireApproximately(float expected, float actual, string message)
    {
        if (MathF.Abs(expected - actual) > 0.0001f)
            throw new InvalidOperationException($"Assertion failed: {message}; expected {expected}, got {actual}");
    }

    private static string DescribeFirstDifference(string first, string second)
    {
        string[] left = first.Replace("\r", "").Split('\n');
        string[] right = second.Replace("\r", "").Split('\n');
        for (int i = 0; i < Math.Max(left.Length, right.Length); i++)
            if (i >= left.Length || i >= right.Length || left[i] != right[i])
                return $"line {i + 1}: '{(i < left.Length ? left[i] : "<missing>")}' != '{(i < right.Length ? right[i] : "<missing>")}'";
        return "no line difference";
    }
}
#endif
