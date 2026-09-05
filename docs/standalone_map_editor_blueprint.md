# Fronts of War — Standalone Map Planner / Map Editor Blueprint

**Status:** Phases 0–15 implementation complete; the legacy planner dock is
unregistered and its source is retained only for comparison. Executable
packaging remains blocked only by the local Godot Mono export-template
installation.

**Date:** 2026-09-04

**Scope:** Developer-only map authoring tool combining the existing M3.5
planner with manual production-map editing. The GDD remains authoritative;
this document resolves the standalone-editor architecture where the GDD and
the older planner specification are silent.

## 1. Executive Summary

The recommended end state is one developer-only Godot application path inside
the existing `godot-project`, launched through the required repository-root
`Launch-MapEditor.ps1` launcher (which supplies `--map-editor`). It should use
a dedicated `scenes_root/map_editor.tscn` and editor namespace, while sharing
the same C# assembly, `assets/`, art catalog,
planner algorithms, geometry, validation, and runtime map loader. The normal
game continues to launch through `boot.tscn`; no player-facing menu reaches
the editor. A player export preset excludes the editor scene from player
builds, and release builds reject the editor flag.

The long-term canonical map asset should be a new Godot `Resource`,
`MapDefinition`, saved as a text `.tres` under `assets/data/maps/`. It is
separate from `MapPlanDefinition`: the latter remains a normalized,
planner-oriented candidate/interchange model; the former contains the exact
authored terrain, art instances, paths, pads, markers, metadata, and
provenance that the game loads. A shared conversion step turns an accepted
planner candidate into a draft `MapDefinition`.

This is an architectural change, not a UI swap. The repository currently has
no production `MapDefinition`, map registry, terrain authoring model, or
runtime map loader. `MissionDefinition.MapId` is metadata only; the playable
map is still a hand-wired `mission.tscn`. `MapRuntime` assumes one
`PathNetwork`, and `WaveRunner` currently ignores `SpawnGroup.PathId`. Those
gaps are the critical path before generated and manually-authored maps can
become playable.

The implementation order is therefore:

```text
shared map domain
    → serialization and loader
    → editor shell
    → read-only rendering
    → commands and undo/redo
    → catalog-driven placement
    → terrain / art / pads
    → multi-path runtime integration
    → generation conversion
    → validation / preview / publish
    → M3.5 cleanup
```

## 2. Repository Findings

### 2.1 Project and conventions

- `godot-project/` is the Godot root.
- There is one `FrontsOfWar.csproj`, targeting `net8.0` with
  `Godot.NET.Sdk/4.7.2`; Godot C# does not currently provide the project with
  separate assembly targets.
- Namespaces mirror folders (`FrontsOfWar.Map`, `FrontsOfWar.Art`, etc.).
- `.tscn`, `.tres`, `.cs`, and JSON are text-native and suitable for agents and
  source control.
- `GameLoop` is an autoload with a fixed 60 Hz simulation. Gameplay must not
  be driven from editor or runtime `_Process` callbacks.
- `GameBalanceConfig` is the only balance tuning surface. The editor may read
  it for previews but must not add a second balance configuration.
- `EventBus` is the established low-coupling channel. Editor UI should use
  editor-local events/commands and only bridge to shared/runtime events at
  the boundary.
- The worktree contains unrelated untracked `.import` files from the current
  environment-art pass. They are preserved and are not part of this blueprint
  change.

### 2.2 Current art and asset state

`ArtAssetCatalog` loads `assets/data/art/art_asset_catalog.json` and resolves
stable catalog IDs to approved production paths or placeholders. The catalog
currently contains 241 entries: 10 `APPROVED`, 211 `REVIEW`, 17
`PLACEHOLDER_READY`, and 3 `HOLD`. Categories encode theater and broad use,
for example `theaters/western_europe/vegetation` and
`theaters/pacific/architecture`.

`ArtAssetSprite` already provides the correct replacement seam for runtime
art: a scene stores `AssetId`, not a texture path, and can fall back to a
placeholder. The catalog model does not yet expose explicit tags, biome,
thumbnail, scale limits, compatibility, or cluster/prefab metadata. These
are catalog-schema extensions, not hard-coded editor lists.

Terrain art exists as review/approved image assets and adjacency-review
scenes, but there is no runtime tile grid, tile instance model, adjacency
solver, collision contract, or terrain map loader.

### 2.3 Current map/runtime state

The only actual playable map is `scenes_root/mission.tscn`. It hand-wires:

- one `MapRuntime`;
- one `PathNetwork` containing one `Path2D/Route` and a `Curve2D`;
- eleven `BuildPad` instances;
- three pre-placed towers and one command post;
- environment `ArtAssetSlot` instances;
- one `AirCorridorDefinition` resource;
- one wave sequence.

`MissionDefinition` contains `Id`, `Title`, `Act`, `MapId`, wave sequence,
briefing, and one star objective. It does not point to or load a map asset.
`Boot` starts the mission scene directly for `--mission`/headless runs and
the main menu otherwise. There is no map registry or publish pipeline.

Runtime systems that are reusable at the boundary include `PathNetwork`,
`PathFollower`, `BuildPad`, `TowerPlacementService`,
`AirCorridorDefinition`, `ArtAssetSprite`, `MapRuntime`, and the data-authored
tower/enemy/wave resources. They need adapters or extensions; the editor
must not copy their gameplay rules.

## 3. Existing M3.5 Map Planner Architecture

The actual data flow is:

```text
map_layout_catalog_100.json
    ↓ MapLayoutCatalog.LoadFromProject()
MapLayoutTemplate + topology/planner/compatibility metadata
    ↓ MapCandidateGenerator.GenerateSingle / Generate
seeded MapPlanDefinition candidates
    ├─ MapPlanGeometry
    ├─ PadSuggestionService
    ├─ MapPlanMetricsCalculator
    ├─ MapPlanScorer
    └─ MapPlanValidator
    ↓
MapPlannerDock / MapPlannerCanvas
    ↓ JSON draft or accepted plan under assets/data/{drafts,maps/plans}
```

The catalog has 100 unique records and ten families: `SERPENTINE`, `LOOP`,
`HAIRPIN`, `MERGE`, `SPLIT_MERGE`, `DUAL_LANE`, `CROSSING`, `HUB`, `GAUNTLET`,
and `ASYMMETRIC`. Entries carry ground-entry/objective counts, logical route
count, topology counts, target path-length bands, shared-coverage labels,
recommended pad ranges, difficulty, constraints, anti-patterns, air hints,
and fixed/deterministic/runtime-procedural compatibility flags.

`MapPlanDefinition` uses a 100 × 56.25 normalized design canvas. It contains
entries, one objective, polyline paths, point-based air-corridor data, pads,
zones, gimmick hooks, metrics, validation, status, source template ID, and
seed. This is a good candidate model and review artifact, but not a complete
production map: it has no terrain instances, art transforms, asset IDs,
stable object identity beyond convention, runtime path curves, map metadata,
or full marker semantics.

The planner's known implementation limitations that matter to migration are:

- `MapPlannerCanvas` is an intentionally thin prototype. Left-click appends
  to the first path; right-click adds a pad. There is no real selection,
  transform, snapping, undo, asset placement, or outliner.
- `MapPlannerDock` is tightly coupled to `EditorPlugin` dock lifecycle and
  uses direct filesystem reads/writes for the first saved file.
- `MapCandidateGenerator` produces deterministic candidates through
  `SeededRandom`, but its topology output is still simplified polyline data.
- `MapPlanScorer` recomputes validation/metrics and uses planner heuristics;
  it is not runtime balance validation.
- `MapPlanValidator` checks normalized canvas bounds, entries, path lengths,
  path references, crossings, pad overlap, and route separation. It does not
  check assets, terrain, runtime loader compatibility, transforms, or map
  registry references.
- `PadSuggestionService` is deterministic and useful, but currently contains
  a default-config lookup inside `CreatePad`; this should be corrected when
  it becomes shared production infrastructure.
- `MapPlanSerializer` is a useful JSON round-trip helper, but its current
  unversioned plan output is not sufficient for the production map format.

## 4. Runtime Map Pipeline

### 4.1 What works today

```text
Boot
  → scenes_root/mission.tscn
  → hand-authored MapRuntime node tree
  → PathNetwork._Ready() reads Path2D/Curve2D
  → MapRuntime._Ready() creates managers and reads node paths
  → WaveRunner spawns enemies on the one PathNetwork
  → EnemyController uses PathFollower or AirCorridorDefinition
  → BuildPad + TowerPlacementService handle player tower placement
  → ArtAssetSprite resolves catalog IDs to art/placeholders
```

The current scene is the source of truth for map geometry. `MapRuntime` does
not load a map ID, `MapId` is not resolved, and the mission scene's fixed
nodes are not generated from a data asset. The current wave runtime has
`SpawnGroup.SpawnPointId` and `PathId` fields, but `WaveRunner.SpawnOne()`
passes only the one `_path` to `EnemyManager.Spawn`; multi-entry/multi-path
behavior is therefore not implemented.

### 4.2 Required target pipeline

```text
MissionDefinition.MapId
  → MapRegistry resolves MapDefinition .tres
  → MapLoader validates schema and references
  → MapSceneFactory builds terrain/art/path/pad/marker nodes at load time
  → MapRuntime receives a MapRuntimeContext with all authored networks
  → WaveRunner resolves authored SpawnPointId + PathId
  → existing enemy/tower/combat systems run unchanged where possible
```

The loader may instantiate map-authored nodes during map load. The pooling
rule applies to transient gameplay objects during a wave; it does not forbid
building the static map at startup. A loaded map must be disposable and
reloadable without leaving editor-only state or managers behind.

### 4.3 Runtime gaps to close

1. Add `MapDefinition` and a registry/loader.
2. Split `MapRuntime` map construction from mission simulation; it is already
   above the project's ~300-line script target.
3. Extend `PathNetwork` usage from one route to a collection of named paths.
4. Pass `SpawnPointId` and `PathId` from `WaveRunner` through
   `EnemyManager` to `EnemyController`.
5. Preserve fixed per-spawn path assignment; do not introduce dynamic
   pathfinding or runtime route generation, both explicitly out of scope.
6. Define terrain and art collision/clearance semantics before allowing the
   editor to publish a map.

## 5. Reuse / Refactor / Extend / Replace Matrix

| Existing component | Keep | Refactor | Extend | Replace | Reason |
|---|---:|---:|---:|---:|---|
| `MapPlannerConfig` |  | ✓ |  |  | Shared planner tuning; inject it consistently and keep it separate from gameplay balance. |
| `MapLayoutTemplate` | ✓ |  |  |  | Correct catalog DTO; add optional filter helpers only. |
| `MapLayoutCatalog` | ✓ |  | ✓ |  | Add indexed/filterable queries and cache invalidation for editor refresh. |
| `MapPlanDefinition` | ✓ | ✓ |  |  | Retain as normalized candidate/interchange model; add explicit schema/provenance rather than making it the production model. |
| `MapPlanGeometry` | ✓ |  | ✓ |  | Reuse pure segment/path math; add curve conversion, bounds, clearance, and stable coordinate conversion. |
| `MapPlanMetricsCalculator` | ✓ |  | ✓ |  | Reuse planner metrics; separate candidate metrics from production diagnostics. |
| `MapPlanScorer` | ✓ |  | ✓ |  | Keep for candidate ranking; never treat score as a production-validity gate by itself. |
| `MapPlanValidator` | ✓ | ✓ | ✓ |  | Extract shared path checks and add production validators for IDs/assets/terrain/transforms/runtime references. |
| `MapCandidateGenerator` | ✓ |  | ✓ |  | Keep deterministic generation; expose configuration and provenance, then convert output to `MapDefinition`. |
| `CandidateDiversity` | ✓ |  |  |  | Candidate-batch concern; no production role. |
| `PadSuggestionService` | ✓ | ✓ |  |  | Keep algorithm; remove default-config leakage and return explicit generated-vs-authored state. |
| `MapPlanSerializer` | ✓ | ✓ |  |  | Keep for candidate JSON compatibility; add versioned migration and do not use it as the canonical production serializer. |
| `MapPlannerPlugin` |  |  |  | ✓ | `EditorPlugin` lifecycle is not a standalone application. Keep temporarily during migration. |
| `MapPlannerDock` |  | ✓ |  | ✓ | Extract planner presentation/controller logic; replace dock shell with standalone editor panels. |
| `MapPlannerCanvas` |  | ✓ |  | ✓ | Reuse draw conventions only; replace prototype input with viewport/document/tool services. |
| `PathNetwork` | ✓ | ✓ | ✓ |  | Keep runtime query wrapper; support named networks and data-built curves. |
| `PathFollower` | ✓ |  |  |  | Movement math remains valid once a specific network is supplied. |
| `BuildPad` | ✓ |  | ✓ |  | Add stable authored ID/tag/metadata and editor visualization hooks without coupling to editor UI. |
| `TowerPlacementService` | ✓ |  |  |  | Runtime player placement remains separate from editor tower-node authoring. |
| `AirCorridorDefinition` | ✓ |  | ✓ |  | Extend to stable ID, entry/objective marker references, and authored direction/width data. |
| `ArtAssetCatalog` | ✓ | ✓ | ✓ |  | Add tags, theater/biome, thumbnail, scale/compatibility fields while keeping stable IDs and status behavior. |
| `ArtAssetSprite` | ✓ |  | ✓ |  | Use as the runtime visual slot; add catalog-status and transform-safe refresh behavior. |
| `MissionDefinition` | ✓ |  | ✓ |  | Add map registry/path reference and runtime map settings without duplicating map geometry. |
| `DataValidator` | ✓ | ✓ | ✓ |  | Share reference checks and add map-file validation; keep headless CLI path. |

Migration rule: retain the old plugin until the standalone shell can browse,
generate, save, and reopen the same planner candidates. Then deprecate its
UI, leaving only shared planner classes until Phase 15.

## 6. Requirements Gap Analysis

### Already exists

- 100-template catalog and typed loader.
- Deterministic candidate generation with seeded randomness.
- Candidate scoring, diversity filtering, path geometry, crossing checks,
  pad suggestions, and planner validation.
- Basic accepted/draft plan JSON round trips.
- Stable art catalog IDs, placeholders, approved/review status, and
  replacement-friendly sprite slots.
- Fixed ground paths, air corridor resource, discrete pad runtime semantics,
  tower definitions, enemy definitions, wave data, and data validation.
- Project UI theme, fonts, palette, and screenshot verification conventions
  suitable for a developer tool with a compact dark workbench treatment.

### Partially exists

- Runtime map loading: hand-authored `mission.tscn` only; no data loader.
- Paths: one runtime `PathNetwork`; planner has multiple polylines but runtime
  does not consume `PathId`.
- Tower nodes: runtime build pads exist, but map-authored stable pad assets do
  not.
- Art placement: scene slots exist, but no editor palette, transforms, or
  persisted instances.
- Terrain: art files and review scenes exist, but no terrain domain model or
  adjacency/collision contract.
- Validation: planner and global resource validator exist; production map
  validation does not.
- Mission data: `MapId` exists, but it does not resolve a playable map.
- Launch: normal game and editor plugin paths exist; a standalone developer
  application path does not.

### New work

- `MapDefinition` and all authored child data types.
- Versioned `.tres` production serialization and migrations.
- Map registry, loader, scene factory, and publish flow.
- Standalone shell, document lifecycle, outliner, inspector, viewport, camera,
  asset palette, selection, transform tools, and command history.
- Stable object IDs and deterministic ordering.
- Runtime support for multiple named paths and authored spawn/path mapping.
- Terrain tile placement/replacement/rotation rules.
- Production map diagnostics and editor overlays.
- Autosave/recovery, recent maps/assets, layers/locking, and optional
  cluster-authoring enhancements.

### Architectural change required

- The runtime must consume the same map document that the editor saves.
- The editor must depend on shared map domain/serialization/catalog services,
  never on `EditorPlugin` APIs.
- `MapRuntime` must accept a loaded map context rather than requiring all
  gameplay geometry to be embedded in a scene.
- `MissionDefinition.MapId` must resolve through a registry.

## 7. Recommended Standalone Architecture

### 7.1 Launch decision

**Recommendation: same Godot project, dedicated developer-only scene and
developer export preset.**

Alternatives considered:

- **Separate Godot project:** strongest visual separation, but duplicates
  project settings, import state, theme, asset paths, and likely the C# build;
  sharing `res://` resources is awkward and creates two launch/build systems.
- **EditorPlugin only:** already exists and reuses resources, but cannot feel
  like a standalone application and remains dependent on the Godot editor.
- **Separate executable/assembly target:** attractive isolation, but the
  current Godot C# setup has one `.csproj`; adding a second project is a large
  build/dependency change with little benefit at this stage.
- **Dedicated scene in the existing project:** reuses everything, has a simple
  launch path, and can be excluded from the player export. This is the lowest
  maintenance option.

The editor scene is reached only when all of these are true:

1. a developer command-line flag or launcher requests it;
2. the build is a debug/developer build;
3. the editor scene is present in the developer export.

Normal `Boot` behavior stays unchanged for players. No main-menu button is
added. The player export preset excludes `scenes_root/map_editor.tscn` and
editor-only source/resources where Godot's export filtering permits it. The
release `Boot` path ignores `--map-editor` and starts the game.

The required repository-root launcher is `Launch-MapEditor.ps1`. It resolves
`godot-project/` relative to its own location, accepts an optional
`-GodotMono` override, otherwise uses `$env:GODOT_MONO` and the same documented
fallback policy as `tools/Run-HeadlessChecks.ps1`, verifies that both the
binary and `project.godot` exist, and launches:

```powershell
powershell -ExecutionPolicy Bypass -File .\Launch-MapEditor.ps1
```

It must forward optional Godot arguments, quote paths containing spaces, emit
an actionable error when the .NET-enabled Godot executable is missing, and
never modify `project.godot` or the normal game launch configuration. Direct
`--map-editor` invocation remains supported for automation, but the launcher
is the normal developer workflow.

### 7.2 Dependency diagram

```text
┌──────────────────────────────┐
│ Developer Map Editor         │
│ shell / viewport / tools / UI │
└──────────────┬───────────────┘
               │ commands + editor events
┌──────────────▼───────────────┐
│ Shared Map Authoring Domain   │
│ MapDefinition / IDs / coords  │
│ transforms / paths / markers  │
└───────┬──────────┬───────────┘
        │          │
┌───────▼──────┐ ┌─▼────────────────┐
│ Serialization │ │ Planner services │
│ load/save/    │ │ catalog/generate │
│ migration     │ │ geometry/score   │
└───────┬──────┘ └─┬────────────────┘
        │          │
        └────┬─────┘
             ▼
┌──────────────────────────────┐
│ Shared Runtime Map Services   │
│ MapRegistry / MapLoader /     │
│ MapSceneFactory / validation  │
└──────────────┬───────────────┘
               ▼
┌──────────────────────────────┐
│ Fronts of War Runtime        │
│ MapRuntime / PathNetwork /   │
│ BuildPad / enemies / towers  │
└──────────────────────────────┘

Editor-only: UI, tools, commands, undo, autosave, preferences, overlays.
Shared: map domain, IDs, coordinates, catalog, serialization, geometry,
validation, conversion, loader contracts.
Runtime-only: simulation, combat, player HUD, transient pools.
```

### 7.3 Folder proposal

```text
godot-project/
  scenes_root/
    boot.tscn
    map_editor.tscn                 # developer-only
  src/
    Map/
      Authoring/
        MapDefinition.cs
        MapAuthoringData.cs
        MapCoordinateSystem.cs
        MapObjectId.cs
        MapRegistry.cs
        MapSerializer.cs
        MapSchemaMigrator.cs
        MapLoader.cs
        MapValidation.cs
        MapPlanConverter.cs
      Runtime/
        MapSceneFactory.cs
        RuntimeMapContext.cs
        PathNetworkSet.cs
      Planning/                       # existing shared planner code
    Editor/
      Application/MapEditorController.cs
      Documents/MapDocument.cs
      Commands/...
      Selection/...
      Viewport/MapEditorViewport.cs
      Palette/AssetPaletteController.cs
      Outliner/MapOutliner.cs
      Inspector/MapPropertyInspector.cs
      Validation/MapDiagnosticsPanel.cs
      Generation/MapGenerationPanel.cs
      Recovery/...
  scenes/editor/
    map_editor_viewport.tscn
    map_editor_asset_preview.tscn
  assets/data/maps/                    # canonical production .tres files
  assets/data/maps/drafts/              # optional drafts, not shipped
  assets/data/maps/recovery/            # ignored/editor-only
  assets/data/map_layout_templates/     # existing planner catalog
  assets/data/art/                      # existing catalog
```

The proposed `Authoring` and `Runtime` folders are logical ownership
boundaries; implementation should respect the repository's namespace-per-
folder rule. No separate assembly is required for Phase 1.

## 8. Proposed Production Map Data Model

`MapDefinition` is a `Resource` and the canonical source of truth. It is not
the mission wave resource and contains no transient editor state.

```text
MapDefinition
  SchemaVersion: int
  Metadata: MapMetadata
  CoordinateSystem: MapCoordinateSystem
  Terrain: TerrainInstance[]
  Paths: PathDefinition[]
  AirCorridors: AirCorridorDefinition[]
  TowerNodes: TowerPlacementNode[]
  Assets: MapAssetInstance[]
  Clusters: ClusterInstance[]
  Markers: GameplayMarker[]
  Zones: MapZone[]
  Gimmicks: MapGimmick[]
  Provenance: GenerationProvenance
```

### 8.1 Metadata and coordinates

`MapMetadata` contains stable `Id`, display name, theater, biome, dimensions
in tiles, terrain-set ID, authoring status, notes, and optional campaign
usage IDs. It does not contain viewport state, selected object, recent assets,
or editor layout.

Use **tile-space map coordinates** for authored gameplay and transforms. One
tile is converted to pixels only by a shared `MapCoordinateSystem` using
`GameBalanceConfig.TilePixelSize` (currently 64 px). Planner candidates remain
100 × 56.25 normalized units and convert once at acceptance. This aligns
runtime range/speed values, pad geometry, path lengths, and art placement on
one explicit scale.

### 8.2 Terrain instances

```text
TerrainInstance
  Id: stable object ID
  AssetId: ArtAssetCatalog ID
  Cell: integer grid coordinate
  RotationQuarterTurns: 0..3
  TerrainSetId
  AdjacencyVariant / socket metadata
  CollisionClass
  EditorTags
```

Terrain has no arbitrary scale field. Rotation is first-class and constrained
to the tile's supported quarter-turns. Terrain placement/replacement must be
validated against grid occupancy, route sockets, adjacency, and collision
before publish.

**Cell size (D64): one terrain cell = one gameplay tile = 64px**, matching
`GameBalanceConfig.TilePixelSize` exactly — `Cell` is a direct tile-space
coordinate, not a separate terrain-specific unit, so it needs no converter
beyond the one `MapCoordinateSystem` already defines for every other authored
object. This was an open gap (see former §22 note, now resolved) because the
environment-art pipeline's 1024×1024px export size (D42) had never been
validated as a placement grid: at 1024px a single terrain tile covers 16×16
gameplay tiles, and the blueprint's own Bocage Crossroads example (§10,
`WidthTiles=28 HeightTiles=18`) is barely 2 tiles wide at that grain — nowhere
near enough resolution for a map editor to actually compose varied terrain.
1:1 was chosen over coarser alternatives (e.g. matching the existing 256px/
4-tile route-socket width from D42) because it is the only size that divides
every map dimension exactly with no rounding edge case, regardless of what a
given map's `WidthTiles`/`HeightTiles` turn out to be. **Route pieces are
unaffected** and keep working as larger multi-cell overlay composites layered
on top of the ground grid, exactly as the existing layered-route system
already does (`WE-MATERIAL-001` + topology overlays) — a straight route
segment still spans several 64px cells, it just aligns to the same base grid
instead of its own. Terrain source art should be authored/generated as
repeatable 64px (or a clean multiple for supersampled export, e.g. 256px at
4× for downsampling headroom) tiles going forward; the existing 1024px
Western Europe route-family batch remains valid as a visual/style reference
but should not be treated as production placement art at that size.

### 8.3 Non-terrain map assets

```text
MapAssetInstance
  Id: stable object ID
  AssetId: catalog ID
  Category / Layer
  Position: Vector2 in tile space
  RotationRadians
  Scale: Vector2 (uniform by default)
  DefaultScale: Vector2
  Enabled
  EditorTags / RuntimeTags
  Optional collision/navigation profile
```

Uniform scale is V1. Numeric scale, reset-to-default, min/max catalog limits,
move, rotate, duplicate, replace, delete, and undo/redo are required.
Non-uniform scale is deferred unless an asset explicitly declares it safe;
the catalog must be the source of that capability, not a per-screen guess.

### 8.4 Paths and markers

```text
PathDefinition
  Id: stable path ID
  EntryMarkerId
  ObjectiveMarkerId
  Points[] in tile space
  CurveMode / baked runtime curve data
  BranchGroupId
  ActiveFromWave / ActiveUntilWave
  Tags

GameplayMarker
  Id
  Kind: GroundEntry | Objective | SpawnPoint | PathJunction |
        AirEntry | AirObjective | CameraBounds | NoPlacementZone |
        RestrictedTerrain | ScriptedEvent (only when runtime needs it)
  Position / optional shape
  PathId
  Metadata
```

The editor may present split/merge/crossing controls, but the runtime model
must remain fixed authored named paths and fixed per-spawn assignments. No
dynamic pathfinding or maze-building is introduced.

### 8.5 Tower nodes

```text
TowerPlacementNode
  Id: stable node ID
  Position in tile space
  RotationRadians only if runtime semantics need it
  PadTag flags: Standard | Elevated | Enclosed | Coastal
  AllowedArchetypeIds[] or category restriction when required
  Enabled
  FootprintProfile
  GeneratedSuggestion: bool
  RuntimeMetadata
```

Generated pad suggestions become ordinary nodes with
`GeneratedSuggestion=true`; any manual edit clears that flag or records an
override. No arbitrary scale is allowed.

### 8.6 Clusters/prefabs

No existing scene/prefab authoring system is present. V1 therefore supports a
cluster as either a catalog-authored placeable art asset or a data-authored
group of child `MapAssetInstance`s with one parent transform. Placement,
movement, rotation, scale, duplication, and deletion work on the parent.
Expand/unpack, create-cluster-from-selection, and arbitrary nested prefab
editing are later enhancements after the runtime representation is proven.

### 8.7 Provenance and status

```text
GenerationProvenance
  SourceTemplateId
  Seed
  GeneratorVersion
  InitialScore
  InitialMetrics
  ConvertedAtUtc
  AcceptedBy / Notes

MapAuthoringStatus: Draft | Review | Production | Deprecated
```

Provenance is informational and must never regenerate or overwrite the
authored document. Manual edits are authoritative.

## 9. Proposed Asset Catalog Architecture

Keep `art_asset_catalog.json` as the machine-readable source and expand its
entry schema. The editor queries `ArtAssetCatalog`, never raw directories.

Recommended optional fields:

```text
tags[]
theater
biome
thumbnail_path
scalable
uniform_scale_only
min_scale
max_scale
placement_layer
compatibility_ids[]
terrain_socket_profile
cluster_child_schema / prefab_scene_path
```

Existing category strings remain backward-compatible and provide defaults for
theater/category. Missing thumbnails use the resolved production image or the
existing placeholder. `APPROVED` assets are publishable; `REVIEW` assets are
browseable in developer mode with a clear status badge but are blocked by the
publish validator unless an explicit review override is recorded; `HOLD`
assets stay hidden from normal placement by default.

Palette behavior:

1. category tree (`Terrain`, `Architecture`, `Vegetation`, `Flavor`,
   `Clusters`, etc.);
2. theater/biome filters;
3. search over display name, ID, tags, and `items`;
4. list/grid toggle with thumbnail, status, and scale capability;
5. compatible-only mode during Replace;
6. recent assets in editor-only preferences;
7. placement mode on selection; Escape/right click/tool change exits.

Adding a normal asset requires an art file plus a catalog record and import;
no editor code or hard-coded list changes. The catalog loader should expose an
explicit `Refresh()` for developer sessions and keep the existing cache
behavior for runtime.

## 10. Proposed Serialization / File Strategy

Use **Godot text `Resource` `.tres`** for canonical production maps. This is
the repository's stated map direction in GDD §15.2, is loadable by the real
runtime, is source-control friendly, and avoids duplicating a JSON-to-runtime
import step. Store stable catalog IDs rather than machine paths. Use ordered
arrays and stable IDs; the editor normalizes ordering before save.

Keep existing JSON for the 100-template catalog and planner candidate
interchange. Do not silently convert old accepted planner JSON in place:
open it as a candidate, convert it to a new draft `.tres`, and retain the
source/provenance reference.

Abbreviated production map example:

```ini
[gd_resource type="Resource" script_class="MapDefinition" load_steps=12 format=3]

[ext_resource type="Script" path="res://src/Map/Authoring/MapDefinition.cs" id="1"]
[ext_resource type="Script" path="res://src/Map/Authoring/MapMetadata.cs" id="3"]
[ext_resource type="Script" path="res://src/Map/Authoring/PathDefinition.cs" id="4"]
[ext_resource type="Script" path="res://src/Map/Authoring/TowerPlacementNode.cs" id="5"]
[ext_resource type="Script" path="res://src/Map/Authoring/MapAssetInstance.cs" id="6"]
[ext_resource type="Resource" path="res://assets/data/map/air_corridor_default.tres" id="2"]

[sub_resource type="Resource" id="MapMetadata"]
script = ExtResource("3")
Id = "bocage_crossroads"
DisplayName = "Bocage Crossroads"
Theater = "western_europe"
Biome = "bocage"
WidthTiles = 28
HeightTiles = 18

[sub_resource type="Resource" id="Path_main"]
script = ExtResource("4")
Id = "path_main"
EntryMarkerId = "entry_west"
ObjectiveMarkerId = "objective_farmhouse"
Points = [Vector2(2, 8), Vector2(8, 8), Vector2(12, 12), Vector2(22, 12), Vector2(26, 9)]

[sub_resource type="Resource" id="Pad_001"]
script = ExtResource("5")
Id = "pad_001"
Position = Vector2(8, 6)
Tag = 0

[sub_resource type="Resource" id="Asset_001"]
script = ExtResource("6")
Id = "asset_001"
AssetId = "review.ART-ENV-004"
Position = Vector2(23, 8)
RotationRadians = 0.0
Scale = Vector2(0.85, 0.85)

[resource]
script = ExtResource("1")
SchemaVersion = 1
Metadata = SubResource("MapMetadata")
Paths = Array[Resource]([SubResource("Path_main")])
AirCorridors = Array[Resource]([ExtResource("2")])
TowerNodes = Array[Resource]([SubResource("Pad_001")])
Assets = Array[Resource]([SubResource("Asset_001")])
```

The exact generated syntax will be verified against Godot's `ResourceSaver`
before Phase 2 is accepted; the example is an architectural fixture, not a
file to copy verbatim.

Schema strategy:

- `SchemaVersion` is required and starts at 1.
- Load rejects missing/unsupported future versions with a visible diagnostic.
- A `MapSchemaMigrator` performs explicit v1→v2 transformations in memory,
  then saves only after the user confirms the migrated document.
- Save uses a temporary file and atomic rename where possible, as `SaveSystem`
  already does for player saves.
- A corrupt canonical file is never overwritten or replaced silently; the
  editor opens a recovery/read-only error state and offers Save As.
- Autosave/recovery files live outside `assets/data/maps/` and are ignored by
  source control.

## 11. Proposed Editor UI

The tool should read as a quiet war-room workbench: slate workspace, brass
selection accents, paper-like document panels, and restrained typography from
the existing `fow_theme.tres`. The memorable element is a large, clean map
board with explicit gameplay overlays; chrome stays subordinate to placement.

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│ Fronts of War Map Editor   File  Edit  View  Generate  Map  Test   [map] [*] │
├───────────────┬───────────────────────────────────────────────┬──────────────┤
│ ASSET PALETTE │                                               │ INSPECTOR    │
│ category      │                                               │ selected     │
│ theater       │                 MAP BOARD                     │ position     │
│ search        │     terrain / paths / pads / art / markers    │ rotation     │
│ [grid/list]   │                                               │ scale        │
│ thumbnails    │                                               │ asset + tags │
│               │                                               │ runtime data │
├───────────────┤                                               ├──────────────┤
│ OUTLINER      │                                               │ GENERATION   │
│ Map           │                                               │ template     │
│ ├ Terrain     │                                               │ seed         │
│ ├ Paths       │                                               │ candidates   │
│ ├ Tower nodes │                                               │ score/report │
│ ├ Architecture│                                               │              │
│ ├ Vegetation  │                                               │              │
│ └ Markers     │                                               │              │
├───────────────┴───────────────────────────────────────────────┴──────────────┤
│ TOOL: Select Move Rotate Scale   snap 1 tile   overlays   [Validate] [Save]  │
│ diagnostics / generation results / output status                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

The board gets the largest area. The left palette/outliner are split panes;
the right inspector/generation pane is tabbed so generation controls do not
consume permanent viewport width. Diagnostics stay persistent in a bottom
drawer so an error can select/focus its object.

Core controls use sentence case and visible keyboard focus. The editor uses
the existing UI palette and type system, but it is a developer workbench, not
the player HUD: no mission resources, combat HUD, or player-facing flow
screens are loaded.

## 12. Asset Placement / Transform Workflow

### Terrain

1. Choose `Terrain` and a tile set in the palette.
2. Cursor shows a grid-snapped ghost with socket/compatibility status.
3. Click to place; `R` rotates by the tile's supported quarter-turn.
4. Select an existing tile to move, rotate, replace, duplicate, or delete.
5. Illegal overlap, route break, missing socket, or unsupported rotation is
   rejected with a diagnostic before the document mutates.
6. Every mutation is one undoable command; batch placement is one compound
   command.

Terrain never exposes arbitrary scale. The runtime loader receives the same
   grid cell and rotation data used by the editor.

### Non-terrain art

1. Choose a category, filter theater/biome, search, and select a thumbnail.
2. Enter placement mode and click one or more positions.
3. Use `W` move, `E` rotate, `S` scale, numeric inspector fields, and `D`
   duplicate. Escape/right click exits placement mode.
4. Uniform scale is linked by default. `Reset scale` returns to catalog
   default. Catalog min/max limits are enforced.
5. Replace filters the palette to compatible IDs and preserves position,
   rotation, scale, layer, and compatible metadata.
6. A cluster behaves as one selected parent in V1. Expand/unpack is later.

Suggested shortcuts: `Ctrl+N` New, `Ctrl+O` Open, `Ctrl+S` Save,
`Ctrl+Shift+S` Save As, `Ctrl+Z` Undo, `Ctrl+Y` Redo, `Delete` delete,
`Ctrl+D` duplicate, `Ctrl+C/X/V` copy/cut/paste, `F` frame selection,
`G` toggle grid, `1/2/3/4` Select/Move/Rotate/Scale, `R` rotate,
`Escape` cancel/deselect, arrows nudge, Shift+arrows fine nudge, and
`Shift`/`Ctrl` multi-select according to platform conventions.

## 13. Editor Feature Prioritization

### V1 required

- standalone developer launch and normal game separation;
- versioned map domain and Save/Open/New/Save As/dirty state;
- read-only rendering and camera controls;
- single/multi-select, move/rotate/scale where legal, delete/duplicate;
- command-based undo/redo;
- outliner and inspector;
- catalog palette with category/search/thumbnail/status;
- terrain placement with grid/rotation/compatibility;
- art placement and replacement;
- tower nodes and runtime-required markers;
- path editing sufficient for the runtime model;
- validation diagnostics with focus/select;
- generated candidate conversion;
- Test Map through the real runtime;
- publish validation and duplicate-ID protection.

### Strongly recommended

- box selection, copy/paste, context menus, keyboard nudging;
- layers, visibility, locking, selection filters;
- recent maps/assets and editor preferences;
- autosave/recovery;
- runtime camera/bounds preview;
- cluster placement as a parent object;
- path direction, lengths, coverage, pad-value, and exclusion overlays;
- review-art status filtering and missing-reference diagnostics.

### Later enhancement

- unpack/edit/repack clusters;
- create reusable clusters from selected objects;
- scatter placement, density, spacing, randomized rotation/scale;
- favorites and richer asset tagging UI;
- non-uniform scale for explicitly safe assets;
- multi-document tabs, collaborative locking, advanced prefab nesting.

### Not appropriate

- player access to the editor;
- runtime procedural generation or runtime map mutation;
- dynamic pathfinding/maze construction;
- destructible terrain, terrain deformation, or editor simulation that changes
  gameplay rules;
- raw filesystem browsing as the normal asset workflow;
- a second balance/configuration system;
- storing selection, camera, favorites, thumbnails, or autosave state in
  production map files.

## 14. Incremental Implementation Plan

Every phase ends with a checkpoint. Tasks below are deliberately small enough
to assign independently. “Likely files” are targets; exact names may change
only with a logged decision.

### Phase 0 — Repository analysis / architecture (complete)

**0.1 Write blueprint.** Objective: capture findings, decisions, model, file
format, UI, risks, and sequence. Files: this document,
`docs/DECISIONS.md`, `docs/PROGRESS.md`. Dependencies: repository inspection.
Tests: link/path review. Manual verification: compare against GDD and request.
Done: architecture is explicit and no broad code was added.

**0.2 Record migration boundary.** Objective: identify old planner UI as
temporary and separate `MapPlanDefinition` from production map data. Files:
decision log and planner spec status note. Done: no agent treats accepted
planner JSON as runtime content.

Checkpoint: the repository analysis and architecture can be reviewed.

Cannot yet: launch an editor, author a production map, or load one at runtime.

### Phase 1 — Standalone application shell

**1.1 Add the repository launcher.** Objective: make
`Launch-MapEditor.ps1` the one-command developer entry point. Files:
repository-root `Launch-MapEditor.ps1`, `tools/README.md`. Dependencies: none.
Implementation: resolve paths from `$PSScriptRoot`; accept `-GodotMono` and
optional forwarded arguments; fall back to `$env:GODOT_MONO` using the same
policy as `Run-HeadlessChecks.ps1`; require a .NET-enabled Godot binary and a
valid `godot-project/project.godot`; launch with `--path <project>
--map-editor`; preserve spaces safely. Automated tests: script parameter/path
resolution and missing-binary failure where practical. Manual: run from the
repository root and from another working directory. Done: one command opens
the map editor or reports exactly how to configure the Godot path.

**1.2 Add developer launch routing.** Objective: `Boot` recognizes
`--map-editor` only in developer builds and changes to the editor scene.
Files: `src/Core/Boot.cs`, `scenes_root/map_editor.tscn`. Tests: Boot argument
routing. Manual: launcher opens the editor; normal game still opens main
menu/mission exactly as before. Done: no player-facing route exists.

**1.3 Create editor root/controller.** Objective: create the themed shell and
viewport/panel regions without map mutation. Files: `src/Editor/Application/*`,
`scenes_root/map_editor.tscn`. Tests: scene load/required child assertions.
Manual: 1920×1080 screenshot. Done: shell stable at target resolution.

**1.4 Add developer export separation.** Objective: define developer and
player export behavior. Files: `export_presets.cfg`, launcher/docs. Tests:
release flag rejection where build tooling permits. Manual: player export has
no editor scene; developer run does. Done: launch boundary documented.

Checkpoint: editor window launches independently; normal game launch works.

**Phase 1 status (2026-09-04): complete.** The repository-root launcher,
debug-only `Boot` route, allowlisted screenshot routing, themed 1920×1080
workbench shell, player/developer Windows presets, Release/editor compile
guards, and export-required tracked solution are implemented. `CoreTests`
passes 17/17, including launch-boundary and required-region checks. A real
1920×1080 capture passed visual review, and a player PCK inspection confirmed
that the editor scene, source files, and compiled type marker are absent.
Full executable export remains machine-dependent because Godot export
templates are not installed here; the preset and Release build both validate.

Cannot yet: open or render a production map.

### Phase 2 — Production map domain + serialization

**Status:** Complete (2026-09-04). The canonical resource model, schema-v1
serializer, document lifecycle, and live File menu are implemented and covered
by eleven passing `MapAuthoringTests`.

**2.1 Implement stable IDs and coordinate types.** Objective: centralize ID
format, tile-space coordinates, rotation, and scale limits. Files:
`src/Map/Authoring/MapObjectId.cs`, `MapCoordinateSystem.cs`, data classes.
Tests: ID uniqueness/format and normalized-to-tile conversion. Manual: inspect
serialized values. Done: no editor subsystem invents its own coordinates.

**2.2 Implement `MapDefinition` resources.** Objective: add metadata, terrain,
assets, paths, pads, markers, zones, gimmicks, provenance. Files:
`src/Map/Authoring/*.cs`. Tests: construct valid/invalid documents. Manual:
create a tiny in-memory map. Done: all requested model categories exist.

**2.3 Implement versioned save/load.** Objective: deterministic `.tres` save,
stable list ordering, atomic Save As, explicit schema errors. Files:
`MapSerializer.cs`, `MapSchemaMigrator.cs`, document service. Tests: round trip,
deterministic output, corrupt/future version handling. Manual: create/save/
close/reopen comparison. Done: structurally identical map data survives.

**2.4 Implement document lifecycle.** Objective: New/Open/Save/Save As/Close,
dirty state, unsaved-change confirmation. Files: `src/Editor/Documents/*`.
Tests: state transitions and refusal to discard dirty data. Manual: exercise
all prompts. Done: no silent loss.

Checkpoint: empty and tiny authored maps round-trip through `.tres`.

Cannot yet: render real assets or play a loaded map.

### Phase 3 — Read-only map rendering

**Status:** Complete (2026-09-04). Repository-relative map loading, a
deterministic render snapshot, tile grid, centered map framing, zoom, middle-
mouse pan, cursor conversion, and selectable outliner entries are implemented.

**3.1 Build map registry and loader contract.** Objective: resolve `MapId` or
file path through repository-relative IDs. Files: `MapRegistry.cs`,
`MapLoader.cs`. Tests: resolve known/missing/duplicate IDs. Manual: open a test
map by ID. Done: no absolute paths.

**3.2 Implement editor scene factory.** Objective: render terrain, catalog art,
paths, air corridors, pads, and markers without editing. Files:
`MapSceneFactory.cs`, editor viewport nodes. Tests: node counts and transforms.
Manual: screenshot loaded sample. Done: visual document matches data.

**3.3 Add camera/grid/navigation.** Objective: pan, zoom, frame map/selection,
cursor coordinates, grid display, bounds. Files: `MapEditorViewport.cs`.
Tests: coordinate conversion. Manual: navigate at multiple window sizes.
Done: cursor and inspector agree on coordinates.

**3.4 Add outliner read-only selection bridge.** Objective: list stable IDs by
logical category and select/focus from either side. Files: `MapOutliner.cs`,
selection contracts. Done: every rendered authored object is discoverable.

Checkpoint: saved maps can be opened and inspected visually.

Cannot yet: mutate, place, or undo.

### Phase 4 — Core selection / transform / undo

**Status:** Complete (2026-09-04). Multi-selection, command history, transforms,
delete/duplicate/copy/paste, keyboard shortcuts, and a live transform inspector
are implemented. Deep strategic validation and terrain adjacency remain later
publish/editor phases as specified below.

**4.1 Implement selection service.** Objective: single/multi-selection,
deselect, viewport/outliner sync. Files: `SelectionService.cs`, input bridge.
Tests: selection set behavior. Manual: click/Shift/Ctrl/outliner.

**4.2 Implement command history.** Objective: execute/undo/redo commands and
compound batches. Files: `IMapEditCommand`, `CommandHistory`, document hooks.
Tests: history ordering, redo invalidation, compound rollback. Done: dirty
state derives from document mutations.

**4.3 Implement transform commands.** Objective: move/rotate/scale with
snapping and legal-type rules. Files: `MoveCommand`, `RotateCommand`,
`ScaleCommand`, gizmo. Tests: exact round trips, terrain scale rejection,
scale limits. Manual: drag, numeric edit, reset. Done: undo/redo all transforms.

**4.4 Implement delete/duplicate/copy/paste.** Objective: stable ID remapping
and batch semantics. Files: command classes, clipboard DTO. Tests: no ID
duplicates and full undo. Manual: duplicate selected groups. Done: pasted data
is source-control stable.

**4.5 Implement inspector.** Objective: expose ID, asset, transform, layer,
tags, runtime metadata with type-specific fields. Files: inspector controls.
Tests: property edits issue commands. Manual: edit values and reload.

Checkpoint: edit → Undo all → Redo all → Save → Reload passes.

Cannot yet: discover assets through a palette or author terrain rules.

### Phase 5 — Asset catalog / asset palette

**5.1 Extend catalog DTO/schema.** Objective: tags, theater/biome, thumbnail,
scalability, compatibility, status. Files: `ArtAssetCatalog.cs`, catalog JSON
schema/docs. Tests: old 241-entry catalog still loads. Manual: refresh after a
catalog edit. Done: no hard-coded asset list.

**5.2 Build palette queries.** Objective: category tree, search, filters,
status, compatible-only results. Files: `src/Editor/Palette/*`. Tests: query
combinations and replacement filtering. Manual: find hedge/tree assets.

**5.3 Build thumbnail/list controls.** Objective: grid/list toggle, fallback
thumbnail, status badge, recently used. Files: palette scenes/controllers.
Tests: missing production path uses placeholder. Manual: browse approved/review
assets. Done: normal placement never asks for a raw path.

**5.4 Implement placement mode.** Objective: select an asset and place one or
more instances with Escape/right-click/tool-change exit. Files: placement tool,
`AddAssetCommand`. Tests: one click/continuous placement/cancel. Manual:
place and reload. Done: placed assets persist as `AssetId` records.

Checkpoint: an asset can be found and placed without knowing its path.

Cannot yet: terrain adjacency enforcement or runtime preview.

### Phase 6 — Terrain authoring

**6.1 Define terrain set/socket contracts.** Objective: specify rotation,
adjacency, route socket, and collision class against the fixed 64px/1-tile
grid cell already decided in §8.2 (D64) — this phase implements the
adjacency/socket/collision rules and catalog wiring, not the cell size
itself. Files: terrain domain and catalog records. Tests:
compatible/incompatible neighbors. Manual: review three-by-three adjacency
scene. Done: rules are data-backed.

**6.2 Implement terrain palette/ghost.** Objective: grid-snapped preview and
rotation. Files: terrain palette/tool. Tests: snap and legal rotations.

**6.3 Implement terrain commands.** Objective: place/move/rotate/replace/
duplicate/delete with no arbitrary scale. Tests: command round trips and
illegal-operation refusal. Manual: construct a small layout. Done: all core
terrain operations work.

**6.4 Add terrain validation.** Objective: adjacency, occupancy, route socket,
collision and bounds diagnostics. Files: production validator extensions.
Done: invalid terrain cannot publish.

Checkpoint: a terrain layout can be manually constructed and edited.

Cannot yet: dress a complete map or load terrain in gameplay.

### Phase 7 — Environmental / architecture / flavor authoring

**7.1 Implement art instance renderer.** Objective: catalog ID to sprite,
layer/z-order, placeholder/review policy, persisted transforms. Tests: missing
asset and status behavior. Manual: compare map to saved data.

**7.2 Implement transform/replace workflows.** Objective: move/rotate/scale,
reset, numeric fields, duplicate/delete, compatible replacement. Tests: preserve
compatible metadata and reject incompatible gameplay fields. Manual: replace a
tree with another vegetation asset and verify transform preservation.

**7.3 Implement layers/visibility/locking.** Objective: editor-only controls
for Terrain, Paths, Tower Nodes, Architecture, Vegetation, Flavor, Gameplay,
Debug. Tests: locked/hidden objects cannot mutate. Manual: toggle layers.

**7.4 Implement V1 cluster instances.** Objective: place/move/rotate/scale a
cluster as one object using catalog asset or child-group data. Tests: parent
transform and duplication. Manual: place a review cluster. Done: no nested
prefab editor is required.

Checkpoint: a map can be visually dressed through the editor.

Cannot yet: create or edit runtime tower/path semantics end to end.

### Phase 8 — Tower placement nodes

**8.1 Add stable tower-node authoring.** Objective: create/select/move/delete/
duplicate nodes with pad tags and enabled state. Files: map domain, editor tool.
Tests: IDs, tag persistence, no scale. Manual: place 18–34 nodes.

**8.2 Convert planner suggestions.** Objective: import `PadPlan` as ordinary
nodes with generated provenance. Files: converter/service. Tests: suggestion
count and manual override precedence. Manual: edit generated pads then rerun
preview without overwriting authored nodes.

**8.3 Render runtime footprint/radius.** Objective: visually distinguish nodes
from art and show tag/footprint constraints. Tests: bounds. Manual: overlay
review at several zooms.

**8.4 Integrate runtime map construction.** Objective: `MapSceneFactory`
builds real `BuildPad` nodes from map data. Tests: tags/positions and runtime
placement recognition. Manual: run the map and place a tower.

Checkpoint: runtime recognizes manually-authored tower locations.

Cannot yet: support all authored multi-path gameplay.

### Phase 9 — Paths / gameplay markers

**9.1 Define marker kinds from runtime needs.** Objective: implement only
ground entries, objective, spawn/path references, air endpoints, bounds,
no-placement/restricted zones as justified by runtime. Tests: required marker
presence. Manual: inspect marker overlays.

**9.2 Implement path editing commands.** Objective: view/select/add/move/delete
points, paths, direction, entry/objective links, fixed branch groups. Tests:
geometry validity and undo/redo. Manual: edit a path and inspect length.

**9.3 Extend runtime path collection.** Objective: named `PathNetwork` set and
fixed spawn/path selection. Files: `PathNetwork`, `MapRuntime`,
`WaveRunner`, `EnemyManager`, `EnemyController`. Tests: two paths and two
spawn groups move on the requested path. Manual: play a split map.

**9.4 Add air corridor authoring.** Objective: move/rotate endpoints and width
where supported; runtime uses the same definition. Tests: air entry/objective
and length. Manual: verify air overlay and Flak coverage.

Checkpoint: enemy traversal works on editor-authored multi-path maps.

Cannot yet: generate/convert candidates through the new document pipeline.

### Phase 10 — Planner / generator integration

**10.1 Build generation configuration panel.** Objective: catalog filters,
seed, candidate count, pad target, theater preset, and route constraints.
Files: `src/Editor/Generation/*`. Tests: config bounds and deterministic seed.

**10.2 Wrap existing planner services.** Objective: run generator, metrics,
scorer, diversity, pad suggestions, and validator without duplication. Tests:
existing `MapPlanningTests` remain green plus configuration tests.

**10.3 Build candidate preview/list.** Objective: thumbnails, score/metrics,
diagnostics, overlay selection. Manual: compare twelve candidates.

**10.4 Convert candidate to `MapDefinition`.** Objective: normalized-to-tile
conversion, named entries/objective/paths, pads, air hints, clear zones,
provenance. Tests: conversion round trip and no shared mutable references.
Manual: choose candidate, edit it, save as draft. Done: generated map enters
normal editing immediately.

**10.5 Protect authored maps.** Objective: regeneration always creates a new
candidate/draft or requires explicit replacement confirmation. Tests: authored
document unchanged after regeneration. Manual: attempt regeneration with a
dirty map.

Checkpoint: generate → choose → convert → edit works.

Cannot yet: publish or launch every authored map through the game.

### Phase 11 — Validation / diagnostics

**11.1 Split validation layers.** Objective: generated-plan validation,
production structural validation, runtime-load validation, and art/content
warnings. Tests: each severity category. Manual: deliberately break a map.

**11.2 Add diagnostics panel/focus.** Objective: Error/Warning/Info list with
select/focus offending object. Tests: issue-to-object mapping. Manual: click
every known error.

**11.3 Add overlays.** Objective: path direction/length, pad coverage,
separation, bounds, exclusion, grid, missing asset and route collision views.
Tests: overlay data follows document. Manual: compare overlay to diagnostics.

Checkpoint: known-invalid maps identify problems and blocking errors prevent
production status.

Cannot yet: test every map in the real runtime from one editor command.

### Phase 12 — Runtime preview

**12.1 Finish map registry/runtime loading.** Objective: mission or preview
launch resolves the selected `MapDefinition` through the real pipeline. Tests:
missing/duplicate map IDs and valid load. Manual: Test Map from editor.

**12.2 Add preview launch handoff.** Objective: save/flush document, launch
game with `--map-id`/developer preview args, return to editor. Files: Boot,
launcher, preview controller. Tests: argument mapping. Manual: edit → Test
Map → play → exit → editor.

**12.3 Add runtime smoke fixture.** Objective: a small map fixture exercises
terrain, rotated tile, scaled art, multiple paths, pads, and air corridor.
Tests: headless load and no errors. Manual: gameplay verification.

Checkpoint: editor-authored map launches through actual gameplay.

Cannot yet: make maps part of the normal production registry automatically.

### Phase 13 — Production publishing

**13.1 Implement publish validation gate.** Objective: require zero blocking
errors, approved required art, unique ID, valid runtime references. Tests:
each refusal reason. Manual: publish valid/invalid drafts.

**13.2 Implement repository organization.** Objective: save canonical map to
`assets/data/maps/`, update registry/index only if needed, preserve provenance.
Tests: duplicate ID/name protection and deterministic output. Manual: inspect
Git diff.

**13.3 Wire MissionDefinition.MapId.** Objective: normal game resolves the
published map while waves remain separate data. Tests: mission map lookup.
Manual: launch published map from game flow.

Checkpoint: a new map is validated, published, and available to the normal
game through the intended content path.

Checkpoint: the standalone editor can publish a validated map that the normal
game can load through the same runtime content path.

### Phase 14 — Production UX improvements

**14.1 Add box selection/context menus/shortcuts.** Tests: input behavior;
manual: keyboard-only common workflow.

**14.2 Add recent maps/assets/preferences.** Keep in editor user data, never in
production `.tres`. Tests: persistence isolation.

**14.3 Add autosave/recovery.** Use periodic recovery copy, never canonical
overwrite; clear only after successful save. Tests: crash/recovery simulation.

**14.4 Evaluate scatter/randomization.** Add only after core editing is fast;
all random placement uses a tool seed and remains an explicit command. Tests:
deterministic scatter and spacing.

Checkpoint: the core production workflow is efficient and recoverable.

Cannot yet: assume every future prefab or terrain system is solved.

### Phase 15 — M3.5 migration / cleanup — complete

**15.1 Compare old/new planner outputs.** Objective: prove candidate generation,
scoring, validation, diversity, and pad suggestions are unchanged unless a
logged bug fix says otherwise. Tests: existing planner suite and fixtures.

**15.2 Deprecate plugin UI.** The map planner editor dock registration was
removed from `project.godot` after standalone parity. The old source remains
unregistered for output comparison and safe rollback; it is not part of the
supported authoring workflow.

**15.3 Update documentation/tests.** Objective: map authoring, launch,
publishing, asset tagging, schema migration, and troubleshooting docs.
Tests: full `Run-HeadlessChecks.ps1` plus editor screenshots.

**15.4 Remove obsolete code safely.** Objective: delete only replaced UI and
dead adapters after a source search and review. Done: one long-term authoring
system remains.

Checkpoint: old M3.5 UI is retired without loss of planner behavior.

## 15. Phase Acceptance Criteria

| Phase | Pass condition |
|---|---|
| 0 | Blueprint, decision, progress, and migration boundary are documented. |
| 1 | `Launch-MapEditor.ps1` opens the developer editor from any working directory; ordinary game launch is unchanged and cannot reach it. |
| 2 | New → Save → Close → Reopen preserves structure and rejects corrupt/future schema safely. |
| 3 | A saved map renders terrain/art/paths/pads/markers and camera coordinates agree. |
| 4 | Transform/edit sequence can be fully undone/redone and reloaded. |
| 5 | A catalog asset is found, previewed, placed, and persisted without a raw path. |
| 6 | A terrain board can be built, rotated, replaced, and validated without scaling. |
| 7 | A map can be dressed with scalable/replacable art and clusters. |
| 8 | Runtime recognizes authored pads and generated suggestions become editable nodes. |
| 9 | Authored entries/paths/air corridors drive actual enemy traversal. |
| 10 | A deterministic candidate converts into an editable production map with provenance. |
| 11 | Invalid maps produce focused diagnostics and cannot publish. |
| 12 | Test Map uses the real game loader and returns cleanly. |
| 13 | Publish makes a unique validated map available to normal game content. |
| 14 | Recovery and high-frequency workflows are source-control-safe. |
| 15 | Standalone tool replaces M3.5 UI and all planner tests remain green. |

## 16. Automated Test Plan

Preserve all six existing `MapPlanningTests` unchanged where possible. Add:

- **Domain:** ID uniqueness, coordinate conversion, terrain rotation/scale
  rules, object type serialization, deterministic ordering.
- **Serialization:** `.tres` round trip, exact stable output fixture, missing
  schema, future schema, corrupt file, migration, atomic-save failure path.
- **Editing commands:** add/remove/move/rotate/scale/replace/duplicate,
  compound commands, undo/redo, copy/paste ID remapping, dirty state.
- **Catalog:** old catalog compatibility, query/filter/tag behavior,
  thumbnail fallback, status/publish policy, compatible replacement.
- **Production validation:** missing entry/objective, broken path, duplicate
  IDs, missing asset, out-of-bounds transform, terrain adjacency failure,
  invalid pad, unsupported scale, missing map registry entry.
- **Conversion:** deterministic seed, candidate → production map, generated
  pad provenance, normalized-to-tile geometry, no mutation of source candidate.
- **Runtime:** valid map loads, rotated terrain and scaled art retain transforms,
  pads place towers, multiple `PathId` values select correct networks, air
  corridor uses authored endpoints, mission `MapId` resolves.
- **Integration:** developer launch flag, player launch exclusion, Test Map
  handoff, publish registry update, full data validator.

Prefer pure C# tests for domain/commands and a small number of Godot scene
tests for resource loading and runtime construction. Avoid brittle pixel/UI
automation; use the existing screenshot workflow for visual review.

## 17. Migration Plan

1. Leave `addons/map_planner` enabled while Phases 1–10 land.
2. Extract no UI code from the plugin into shared classes; only planner/domain
   services move below the application boundary.
3. Add an import command that reads accepted planner JSON and writes a draft
   `MapDefinition`, retaining `SourceTemplateId`, seed, score, metrics, and
   source filename in provenance.
4. Keep accepted JSON readable for one migration window; never overwrite it.
5. Use the standalone editor for all new production maps once Phases 10–13
   pass.
6. At Phase 15, remove plugin registration and retire dock/canvas classes.
   Completed 2026-09-04; planner/domain services remain available to the
   standalone generation workflow.
7. Keep `MapPlanDefinition` and planner services because candidate generation
   remains a supported first step of the production workflow.

The old `docs/fronts_of_war_map_planner_design_spec.md` should be retained as
the historical/M3.5 planner subsystem reference, with its standalone UI and
export assumptions superseded by this blueprint. The GDD and this blueprint
govern the new editor; archived docs remain non-authoritative.

## 18. Risks and Mitigations

| Risk | Mitigation |
|---|---|
| Building a second map interpretation | Make `MapDefinition` the shared source and use one loader for editor preview/runtime. |
| Runtime path model is less complete than planner model | Implement multi-path runtime before Phase 10 conversion is declared complete. |
| `.tres` output churn or unstable subresource IDs | Stable arrays/IDs, normalization, golden serialization fixtures, review diffs. |
| Art catalog has review/hold assets and incomplete metadata | Status badges, publish gate, placeholder fallback, catalog schema extensions. |
| Terrain art lacks a connectivity contract | Phase 6 defines sockets/adjacency/collision before broad authoring. |
| Editor grows into a monolith | Keep document, command, selection, viewport, catalog, and validation services separate; enforce small scripts. |
| Regeneration overwrites authored work | Candidate/document separation, dirty confirmation, immutable provenance. |
| Player export exposes developer UI | Debug-only launch guard, no menu route, developer export preset, release smoke test. |
| Preview diverges from real game | Launch the actual runtime scene/loader; do not build a fake simulator. |
| Overbuilding secondary UX | Do not start Phase 14 until Phases 1–13 acceptance gates pass. |
| Existing prototype map has 11 pads, not GDD's 22 | Treat it as a runtime fixture, not proof of production-map completeness; validate counts per map metadata. |
| `MapRuntime` already exceeds the script-size target | Split loader/context/path ownership before more responsibilities land. |

## 19. Early Architectural Decisions

1. **Application boundary:** same Godot project, dedicated developer scene,
   debug-only flag, developer export preset. Evidence: one C# project, shared
   `res://` resources, existing editor plugin, no separate assembly boundary.
2. **Production model:** separate `MapDefinition` resource, not an enlarged
   `MapPlanDefinition`. Evidence: planner model is normalized and lacks art,
   terrain, stable instances, and runtime metadata; GDD expects map resources.
3. **Canonical format:** versioned text `.tres`; planner catalog/candidates
   remain JSON. Evidence: GDD §15.2 names map `.tres` resources and the project
   uses text-native Godot resources throughout.
4. **Coordinates:** authored map data is tile space; planner math remains
   normalized; one shared converter uses `GameBalanceConfig.TilePixelSize`.
   Evidence: runtime gameplay ranges/speeds are tiles and current runtime
   scene positions are pixels.
5. **IDs:** every persisted object gets a stable authoring ID; IDs are not
   generated from array index during load. Evidence: wave/path/pad references
   and source-control safety require identity through reorder/edit.
6. **Catalog:** `ArtAssetCatalog` remains the discovery authority; category,
   tags, thumbnails, scale, and compatibility are metadata additions. Evidence:
   existing art slots already store stable IDs and resolve placeholders.
7. **Dependency direction:** editor → shared map domain/planner/catalog;
   runtime → shared map domain/loader; shared code never references editor UI.
8. **Undo/redo:** command stack over document mutations, with compound
   commands for batch operations. Evidence: Godot EditorUndoRedoManager is tied
   to `EditorPlugin`, while the standalone app needs the same behavior outside
   the editor.
9. **Clusters:** V1 treats them as a placeable parent asset/group; unpack and
   authoring are later. Evidence: no existing prefab/cluster scene contract.
10. **Runtime preview:** launch the real game/loader with a developer map ID;
    no duplicate preview simulation. Evidence: current runtime already owns
    movement/combat and should remain the authority.
11. **Publish:** canonical maps live in `assets/data/maps/`; publishing is a
    validation/registry operation, not an opaque copy to a machine-specific
    output directory.
12. **Terrain grid cell size (D64):** one `TerrainInstance.Cell` = one
    gameplay tile = 64px, matching `GameBalanceConfig.TilePixelSize` exactly.
    Evidence: the only existing terrain-tile precedent (D42's 1024×1024px
    environment-art export size) covers 16×16 gameplay tiles per placement,
    and against this blueprint's own worked map example (§10, Bocage
    Crossroads at 28×18 tiles) that leaves barely two placements across the
    whole map — far too coarse for a map editor to compose varied terrain.
    64px is also the only candidate size that divides any future map's
    `WidthTiles`/`HeightTiles` with no rounding remainder, since it's the
    base unit those dimensions are already expressed in. Route pieces are
    unaffected and remain larger multi-cell overlay composites on top of the
    same grid, per the existing layered-route approach.

## 20. Proposed Repository Changes

### First implementation wave

- Add `docs/standalone_map_editor_blueprint.md`.
- Add a decision-log entry for the architecture and a progress note.
- Add `src/Map/Authoring/` domain/serialization contracts.
- Add `src/Editor/` shell and `scenes_root/map_editor.tscn`.
- Add the repository-root `Launch-MapEditor.ps1`, developer launch routing,
  and launch documentation.

### Later additions

- `assets/data/maps/` canonical maps and registry.
- `assets/data/maps/drafts/` and ignored recovery location.
- Catalog schema fields and records for tags/thumbnails/scale/compatibility.
- Runtime `MapLoader`/`MapSceneFactory` and multi-path `PathNetwork` support.
- Editor scenes/controllers for palette, outliner, inspector, generation,
  validation, and viewport tools.
- Tests alongside each domain/runtime change.

### Eventual removals

- `MapPlannerPlugin.cs`, `MapPlannerDock.cs`, and `MapPlannerCanvas.cs` only
  after Phase 15 parity and review.
- Any temporary JSON-to-scene adapters once all missions use the canonical
  loader.

Do not alter the current mission scene or environment-art import files during
Phase 0.

## 21. Recommended Exact Implementation Order

1. Review and accept this blueprint/decision boundary.
2. Implement Phase 1.1–1.4 launcher and shell only.
3. Implement Phase 2.1–2.4 domain and document lifecycle.
4. Implement Phase 3.1–3.4 read-only loader/rendering.
5. Implement Phase 4.1–4.5 selection, commands, transforms, inspector.
6. Implement Phase 5.1–5.4 catalog palette and placement.
7. Implement Phase 6.1–6.4 terrain contract and authoring.
8. Implement Phase 7.1–7.4 art, layers, and V1 clusters.
9. Implement Phase 8.1–8.4 tower nodes and runtime pad construction.
10. Implement Phase 9.1–9.4 marker/path/air authoring and runtime multi-path.
11. Implement Phase 10.1–10.5 planner integration/conversion.
12. Implement Phase 11.1–11.3 diagnostics and overlays.
13. Implement Phase 12.1–12.3 real runtime preview.
14. Implement Phase 13.1–13.3 publish and `MissionDefinition.MapId`.
15. Run the full check suite and add screenshot/manual verification notes.
16. Only then consider Phase 14 UX improvements.
17. Finish with Phase 15 plugin migration/removal.

Independently parallelizable after Phase 2: catalog metadata preparation,
pure command tests, and planner conversion tests. Not independently safe:
runtime preview, publish, and path conversion depend on the shared loader and
multi-path runtime work.

## 22. Questions / Unknowns

These are repository gaps, not questions that block Phase 1:

- The terrain grid cell size is now decided (§8.2, D64: 64px = 1 gameplay
  tile). The socket/adjacency/collision *rules* on top of that grid are still
  not encoded in the art catalog or runtime; Phase 6.1 must establish those
  before production terrain publishing.
- The intended production map dimensions for each future map are not present
  as runtime data. Phase 2 should use explicit per-map metadata and convert
  the existing planner canvas only for generated drafts.
- The final cluster/prefab source format is not present. V1 should use a
  placeable catalog asset or flat authored child group and defer unpack/repack.
- The runtime map registry has no existing convention because only one map
  scene exists. Phase 13 should establish the first registry alongside the
  first canonical published map.

None of these justify implementing a second editor architecture or asking for
new product scope before Phase 1.
