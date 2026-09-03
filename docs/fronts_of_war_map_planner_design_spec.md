# FRONTS OF WAR — MAP PLANNER & ASSISTED MAP GENERATOR
## Design Specification v1.0

## 1. Purpose

Build a **design-time, human-in-the-loop Map Planner** for *Fronts of War* that can:

1. browse a reusable library of proven tower-defense route topologies;
2. generate candidate route geometry from a selected topology template;
3. recommend build-pad locations based on route exposure and tower-role coverage;
4. score candidates for gameplay readability, route value, pad value, and structural variety;
5. overlay geographic and art-production guidance without corrupting gameplay geometry;
6. save an approved candidate as an ordinary hand-authored map plan that can be refined in Godot.

This is **not runtime procedural generation**. Shipping maps remain authored, deterministic maps. The generator is an internal design tool that accelerates the first 60–80% of map planning and then hands control to the designer.

This interpretation is required to remain compatible with the existing GDD's explicit `[X]` on procedural maps and its fixed-path/fixed-pad architecture.

---

## 2. Research basis

Across path-based tower defense games, the recurring structural variables are:

- number of entrances and exits/objectives;
- route length;
- number of independent lanes;
- splits and merges;
- crossings / close-proximity passes;
- loops and repeated tower exposure;
- choke points;
- amount of shared tower coverage between lanes;
- placement restrictions / high-value central positions;
- route timing or staged activation.

The supplied 100-template catalog encodes these as parameters instead of trying to copy commercial maps.

### Topology families

| Family | Core gameplay property |
|---|---|
| SERPENTINE | Long single route with bends |
| LOOP | Repeated exposure to the same central coverage |
| HAIRPIN | Adjacent passes create double-duty tower positions |
| MERGE | Multiple entries become one shared defense |
| SPLIT_MERGE | Deterministic branch coverage choices |
| DUAL_LANE | Separate defenses with limited shared support |
| CROSSING | Premium intersection / timing positions |
| HUB | Central control zone serving several approaches |
| GAUNTLET | Short route / low reaction time |
| ASYMMETRIC | Unequal route lengths, bypasses, staged entries, or topology changes |

---

## 3. Relationship to existing Fronts of War rules

The planner MUST preserve these project rules:

- Top-down, fixed visible ground routes.
- Discrete fixed build pads.
- 1–3 ground entry points.
- 1 objective for standard maps.
- Named deterministic paths where splits exist.
- Air units use separately authored air corridors.
- No runtime dynamic pathfinding.
- No maze-building.
- No generated map is considered production content until explicitly accepted and saved by the designer.
- Standard launch-map pad target remains 18–34.
- Map gimmicks remain simple pad tags, path availability switches, or stat/timer modifiers.
- Gameplay geometry comes before geographic dressing and environmental storytelling.

---

## 4. Tool modes

### 4.1 Browse Mode
Browse/filter the 100-template catalog.

Filters:
- family;
- entrance count;
- route count;
- path length;
- shared-coverage rating;
- difficulty band;
- primary design lesson;
- launch-candidate flag;
- theater compatibility;
- desired pad count.

Selecting a template shows:
- normalized route thumbnail;
- topology graph;
- expected strengths/weaknesses;
- default scoring profile;
- recommended map types.

### 4.2 Plan Mode
Create a map manually using planner primitives:
- Entry Marker;
- Objective Marker;
- Ground Path;
- Split Node;
- Merge Node;
- Crossing Marker;
- Air Corridor;
- Build Pad;
- Reserved Hero-Asset Zone;
- Gameplay Clear Zone;
- Geographic Barrier;
- Optional Gimmick Marker.

### 4.3 Generate Mode
Generate 8–24 candidate plans from:
- chosen topology template;
- aspect ratio;
- entry side preferences;
- objective location preference;
- target route length;
- target pad count;
- minimum route separation;
- desired central/shared coverage;
- difficulty target;
- theater preset;
- seed.

Generation MUST be deterministic by seed.

### 4.4 Evaluate Mode
Score and explain the candidate.

### 4.5 Export/Accept Mode
An accepted candidate becomes normal project data:
- `MapPlanDefinition` resource;
- one or more named path definitions;
- build-pad list;
- air-corridor definitions;
- clear-zone and decoration-zone hints;
- scoring report;
- generation metadata / seed for provenance.

Acceptance removes any implication that the game will regenerate the layout at runtime.

---

## 5. Data model

```text
MapLayoutTemplate
  id
  displayName
  family
  prevalenceTier
  topologyRequirements
  parameterRanges
  designLessons[]
  scoringWeights
  recommendedDifficulty
  generatorRules[]
  antiPatterns[]

MapPlanDefinition
  id
  displayName
  sourceTemplateId
  seed
  canvasSize
  entries[]
  objective
  paths[]
  airCorridors[]
  pads[]
  geographicZones[]
  reservedArtZones[]
  gimmicks[]
  metrics
  validation
  status  // Draft, Candidate, Accepted, Rejected
```

### Path data

```text
PathPlan
  id
  points[]
  length
  startEntryId
  objectiveId
  branchGroupId
  activeFromWave
  activeUntilWave
  direction
  tags[]
```

### Pad data

```text
PadPlan
  id
  position
  tag               // Standard/Elevated/Enclosed/Coastal
  routeExposure[]
  estimatedCoverage
  overlapScore
  strategicRole     // Corner, Choke, Shared, Backline, LongRange, Utility
  nearbyPadIds[]
```

---

## 6. Coordinate system

Use a normalized design canvas for planner math:

- Width = 100 design units.
- Height = 56.25 design units for 16:9.
- Route widths are represented independently from sprite pixels.
- Generation happens in normalized coordinates.
- Godot conversion happens on export.

This keeps templates resolution-independent and makes JSON catalog entries portable.

---

## 7. Candidate generation pipeline

1. **Select topology graph.**
2. **Place anchors**: entrances, objective, split/merge/cross nodes.
3. **Create coarse polylines** between graph nodes.
4. **Relax geometry** to enforce:
   - minimum curvature radius;
   - minimum route-to-route separation;
   - edge clearance;
   - minimum objective approach distance;
   - desired total route length.
5. **Smooth to cubic Bézier / Path2D-ready curves.**
6. **Identify strategic route features**:
   - bends;
   - hairpins;
   - shared range bands;
   - intersections;
   - merge zones;
   - late-defense zones;
   - long straight AP lanes.
7. **Generate pad candidates** around those features.
8. **Select pad set** using coverage diversity and anti-dominance rules.
9. **Reserve gameplay clear zones** around routes and pads.
10. **Apply theater geography suggestion pass**.
11. **Score candidate**.
12. Return best N candidates plus diagnostic reasons.

---

## 8. Pad-planning algorithm

A candidate pad receives feature scores:

- `route_time_in_range`: estimated seconds an average enemy is targetable;
- `unique_path_coverage`: number of distinct logical routes reached;
- `repeat_exposure`: same enemy re-enters range later;
- `corner_quality`: ability to maintain target time through a bend;
- `straight_lane_quality`: value for slow-traverse/direct AP towers;
- `indirect_fire_quality`: distance from minimum-range conflicts;
- `support_cluster_quality`: number of other useful pads within Command Post aura;
- `backline_value`: late leak insurance;
- `air_overlap`: air corridor exposure;
- `tag_bonus`: elevated/enclosed/coastal strategic value.

Pad selection constraints:
- avoid pads with effectively identical value;
- require a mix of premium, normal, and situational pads;
- avoid one single pad that dominates every tower archetype;
- ensure T1–T9 each have at least one reasonable placement role on the map unless a deliberate mission rule says otherwise;
- preserve 18–34 total pads;
- preserve visual breathing room around each pad.

---

## 9. Map scoring

Every candidate gets 0–100.

### A. Route Readability — 20
- obvious entry-to-objective flow;
- crossings visually understandable;
- no ambiguous overlapping paths.

### B. Strategic Coverage Variety — 20
- mixture of single-lane and shared-lane pad value;
- mixture of corner / straight / backline / support positions;
- no solved universal cluster.

### C. Counterplay Support — 15
- viable AP sightlines;
- viable mortar/artillery positions;
- viable Command Post clusters;
- trap-worthy route segments;
- viable AA coverage if air-enabled.

### D. Difficulty Shape — 10
- route length and shared coverage match requested difficulty;
- no accidental extreme shortcut.

### E. Pad Economy — 10
- 18–34 target;
- premium pads limited;
- enough alternatives to support nation diversity.

### F. Spatial Composition — 10
- usable non-gameplay regions exist for hero assets and environmental storytelling;
- gameplay lane and pad halos remain visually clean.

### G. Theater Plausibility — 5
- route can be explained by roads, bridges, cliffs, rail lines, village streets, etc.

### H. Technical Validity — 10
- all paths connected;
- deterministic branch mapping;
- minimum separations valid;
- objective reachable;
- no pad/path collision;
- Path2D export viable.

Hard-fail validation overrides score.

---

## 10. Diversity metrics

The generator must not simply produce 20 cosmetic variations of the same map.

For a batch, calculate:
- topology family distance;
- normalized route-shape distance;
- entry/objective placement distance;
- pad-value histogram distance;
- shared-coverage percentage;
- path-length difference;
- centrality distribution.

Reject near-duplicate candidates above a configurable similarity threshold.

---

## 11. Theater-aware planning

The art document establishes:
`Gameplay Geometry → Geographic Logic → Environmental Story`.

The generator MUST follow this order.

### Western Europe
Useful geography:
- hedgerows;
- sunken lanes;
- farm roads;
- village junctions;
- orchards;
- bridge approaches.

### North Africa / Mediterranean
- wadis;
- switchback roads;
- rocky outcrops;
- coastal roads;
- dry river crossings;
- village edges.

### Eastern Europe
- forest roads;
- rail junctions;
- factory roads;
- river crossings;
- snow passes;
- broad field approaches.

### Pacific
- jungle tracks;
- causeways;
- airfield roads;
- river fords;
- beach approaches;
- bunker networks.

The theater pass may reshape *terrain explanation* around the route but must not silently alter accepted gameplay topology.

---

## 12. Art-planning output

For every accepted map plan, produce a density overlay:

- Zone A: gameplay lane — LOW;
- Zone B: build-pad halo — LOW–MEDIUM;
- Zone C: general terrain — MEDIUM;
- Zone D: storytelling / non-playable — HIGH.

Also suggest:
- 4–8 hero/location asset slots;
- 8–16 prop-cluster zones;
- micro-detail zones;
- occlusion-risk warnings;
- screenshot-identity focal areas.

---

## 13. Gimmick hooks

The planner supports hooks, not bespoke simulations:

- pad tags: Standard/Elevated/Enclosed/Coastal;
- `activate_spawn_from_wave`;
- `enable_path_on_wave`;
- `disable_path_on_wave`;
- timed path availability;
- path-segment speed multiplier;
- path-segment Concealed flag;
- global temporary tower range multiplier.

Generated candidates should default to **no gimmick**. A standard map should not need a gimmick to be strategically interesting.

---

## 14. Editor UI recommendation

Implement as a Godot `EditorPlugin` dock plus a large central 2D editing panel.

Left:
- template browser;
- generation controls.

Center:
- map canvas;
- topology/path editing;
- pad editing;
- overlays.

Right:
- selected item inspector;
- score breakdown;
- validation issues.

Bottom:
- candidate strip;
- seed;
- Accept / Reject / Regenerate;
- Export.

Overlays:
- route heatmap;
- tower-range exposure heatmap;
- pad quality;
- shared-path coverage;
- air coverage;
- clear zones;
- art density zones.

---

## 15. MVP

### Phase A — Planner foundation
- load 100-template JSON;
- template browser;
- manual entry/objective/path editing;
- manual pad placement;
- validation;
- save/load `MapPlanDefinition`.

### Phase B — Metrics
- route length;
- curvature;
- shared coverage;
- pad exposure scoring;
- map score report;
- heatmap overlays.

### Phase C — Assisted generation
- seeded route candidate generation for SERPENTINE, HAIRPIN, LOOP, MERGE, DUAL_LANE;
- candidate diversity filtering;
- automatic pad suggestions.

### Phase D — Advanced topologies
- SPLIT_MERGE;
- CROSSING;
- HUB;
- ASYMMETRIC;
- staged routes.

### Phase E — Art/theater planning
- geographic presets;
- density zones;
- hero/prop-cluster suggestions;
- export annotations for map dressing.

Do not begin Phase E before generated layouts reliably produce strategically useful geometry.

---

## 16. Tests / acceptance criteria

### Data
- all 100 catalog entries deserialize;
- IDs unique;
- valid entrance count 1–3;
- objective count exactly 1;
- pad range never outside 18–34 unless explicitly marked experimental.

### Geometry
- 1,000 seeded generations per implemented family produce no disconnected routes;
- no invalid self-crossings unless topology requests a crossing;
- no route enters objective from an invalid endpoint;
- deterministic seed reproduces identical plan.

### Strategy
For each generated map:
- at least 3 distinct high-value pad clusters;
- no single pad scores >1.35× the second-best pad unless template explicitly calls for a centerpiece;
- at least one useful AP/direct-fire zone;
- at least one useful indirect-fire zone;
- at least one viable Command Post cluster;
- minefield-compatible path segments exist.

### Readability
- crossings and merges are flagged and visualized;
- route remains readable at gameplay zoom;
- build-pad halos are clear of reserved hero assets.

### Performance
- generate and score 24 candidates quickly enough for interactive editor use;
- all scoring is editor-time only and has zero runtime mission cost.

---

## 17. Repository / scope changes to make

Update the overall plan without changing the shipping feature contract:

1. Add **Map Planner & Assisted Generator** to editor tooling.
2. Clarify `[X] procedural maps` as:
   - `[X] runtime procedural maps / generated shipping maps without author approval`;
   - `[Tool] design-time assisted generation is permitted`.
3. Expand `/addons/map_pad_tool` into `/addons/map_planner`, or add a sibling tool if preserving the current plugin is cleaner.
4. Add catalog data under `/assets/data/map_layout_templates/`.
5. Add `MapPlanDefinition` under the map data model.
6. Add tests and validator rules.
7. Add implementation tasks to the prompt ladder before mass map production.
8. Keep the 8 launch-map content commitment unchanged.

---

## 18. Recommended production use

Use the generator to produce **candidate geometry**, not finished maps.

Recommended workflow:

`Choose design lesson → choose template family → generate 12–24 candidates → compare score/heatmaps → select 1–3 → manually edit → playtest greybox → accept topology → apply theater geography → place hero zones → build final art map → mission-specific wave tuning`

The human selection/playtest step is mandatory.

---

## 19. Catalog files

- `fronts_of_war_map_layout_catalog_100.json` — canonical machine-readable catalog.
- `fronts_of_war_map_layout_catalog_100.csv` — spreadsheet-friendly review log.

The JSON should be the source used by the planner.
