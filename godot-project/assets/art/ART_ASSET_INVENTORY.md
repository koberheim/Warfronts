# Art asset inventory

Status values: `NEEDED` = not created, `REVIEW` = generated and awaiting
human approval, `APPROVED` = cleared for integration, `HOLD` = intentionally
excluded from this pass.

This inventory is based on `docs/FRONTS OF WAR ART DESIGN.md` §§11–20,
§§33–43, and `docs/GDD.md` §§3.4, 9, 11, 13, 14, and 16. It treats each
theater as a reusable kit. Counts are production targets from the art spec,
not new gameplay scope.

## Initial generated review set

| ID | Asset | Theater/kit | Status | File |
|---|---|---|---|---|
| ART-ENV-001 | Bocage grass and field ground tile | Western Europe / terrain | APPROVED | `theaters/western_europe/terrain/ground_bocage_grass_v01.png` |
| ART-ENV-002 | Sunken muddy lane segment | Western Europe / terrain | APPROVED | `theaters/western_europe/terrain/road_sunken_lane_v01.png` |
| ART-ENV-003 | Sculpted hedgerow wall segment | Western Europe / terrain | APPROVED | `theaters/western_europe/terrain/hedgerow_wall_v01.png` |
| ART-ENV-004 | Norman-style stone farmhouse | Western Europe / architecture | APPROVED | `theaters/western_europe/architecture/hero_stone_farmhouse_v01.png` |
| ART-ENV-005 | Farmyard supply prop cluster | Western Europe / flavor | APPROVED | `theaters/western_europe/flavor/cluster_farmyard_supply_v01.png` |
| ART-ENV-006 | Mediterranean rocky wadi ground tile | Mediterranean / terrain | APPROVED | `theaters/mediterranean/terrain/ground_rocky_wadi_v01.png` |
| ART-ENV-007 | Eastern Europe snowbound road tile | Eastern Europe / terrain | APPROVED | `theaters/eastern_europe/terrain/road_snowbound_forest_v01.png` |
| ART-ENV-008 | Pacific jungle mud-track tile | Pacific / terrain | APPROVED | `theaters/pacific/terrain/road_jungle_mud_track_v01.png` |
| ART-ENV-009 | Pacific jungle foliage cluster | Pacific / vegetation | APPROVED | `theaters/pacific/vegetation/cluster_jungle_foliage_v01.png` |
| ART-ENV-010 | Commander’s painted map-table frame | Shared / UI | APPROVED | `shared/ui/commander_map_table_frame_v01.png` |

These are review images, not yet connected to scenes or considered
production-ready. They deliberately contain no nation-specific units,
enemy silhouettes, people, real insignia, political symbols, or gore.

### Direction review note

The user has confirmed that all ten images are successful art-direction
references. ART-ENV-001 through ART-ENV-009 are approved for their described
environmental roles. The terrain/route assets still require an adjacency and
path-continuity check before being placed as connected map tiles. ART-ENV-010
is approved for command-table presentation UI such as menu, briefing, or
mission select, pending layout and safe-center-space checks.

## Western Europe route-family generation batch

| IDs | Asset family | Status | Files | Review scene |
|---|---|---|---|---|
| WE-ROUTE-001–010 | Sunken-lane straight, corners, T-junctions, cross, and entry | REVIEW | `theaters/western_europe/terrain/route_sunken_lane_*_v01.png` | `scenes/art/western_europe_route_review.tscn` |

All ten outputs are organized at 1024×1024. Rotated companions derive from
the same straight, corner, and T-junction anchors, and every connected edge
uses the same feathered socket cap with an exact shared outer band. They must
pass the review scene's seam, internal-blend, route-width, and gameplay-scale
checks before any entry becomes `APPROVED` or replaces mission terrain.

| WE-MATERIAL-001 | Shared Western Europe sunken-lane route material | REVIEW | `shared/route_materials/western_europe/route_material_sunken_lane_v01.png` | `scenes/art/western_europe_layered_route_review.tscn` |
| WE-ROUTE-OVERLAY-NE-001 | Unique painterly Western Europe sunken-lane North/East corner overlay | REVIEW | `theaters/western_europe/terrain/route_overlays/route_overlay_sunken_lane_ne_v01.png` | `scenes/art/western_europe_layered_route_review.tscn` |
| WE-ROUTE-OVERLAY-WN-001 | Unique painterly Western Europe sunken-lane West/North corner overlay | REVIEW | `theaters/western_europe/terrain/route_overlays/route_overlay_sunken_lane_wn_v01.png` | `scenes/art/western_europe_layered_route_review.tscn` |
| WE-ROUTE-OVERLAY-ES-001 | Unique painterly Western Europe sunken-lane East/South corner overlay | REVIEW | `theaters/western_europe/terrain/route_overlays/route_overlay_sunken_lane_es_v01.png` | `scenes/art/western_europe_layered_route_review.tscn` |
| WE-ROUTE-OVERLAY-SW-001 | Unique painterly Western Europe sunken-lane South/West corner overlay | REVIEW | `theaters/western_europe/terrain/route_overlays/route_overlay_sunken_lane_sw_v01.png` | `scenes/art/western_europe_layered_route_review.tscn` |
| WE-ROUTE-OVERLAY-T-NES-001 | Unique painterly Western Europe sunken-lane North/East/South T-junction overlay | REVIEW | `theaters/western_europe/terrain/route_overlays/route_overlay_sunken_lane_t_nes_v01.png` | `scenes/art/western_europe_layered_route_review.tscn` |
| WE-ROUTE-OVERLAY-T-ESW-001 | Unique painterly Western Europe sunken-lane East/South/West T-junction overlay | REVIEW | `theaters/western_europe/terrain/route_overlays/route_overlay_sunken_lane_t_esw_v01.png` | `scenes/art/western_europe_layered_route_review.tscn` |
| WE-ROUTE-OVERLAY-CROSS-001 | Unique painterly Western Europe sunken-lane four-way cross overlay | REVIEW | `theaters/western_europe/terrain/route_overlays/route_overlay_sunken_lane_cross_v01.png` | `scenes/art/western_europe_layered_route_review.tscn` |
| WE-ROUTE-OVERLAY-ENTRY-N-001 | Unique painterly Western Europe sunken-lane North entry overlay | REVIEW | `theaters/western_europe/terrain/route_overlays/route_overlay_sunken_lane_entry_n_v01.png` | `scenes/art/western_europe_layered_route_review.tscn` |

These topology-specific overlays are the D45 production-direction test set.
They keep the shared 256-pixel centered edge sockets and 48-pixel shoulders,
while painting each interior corner or junction once as a single continuous
route. The shared route material remains available as a temporary fallback for
topologies that do not yet have an overlay, but it is not the desired final
treatment for corners or junctions.

The shared material is a separate route-layer asset. It remains `REVIEW` until
the layered proof is visually accepted over the Western Europe ground tile.

## Placeholder-art plumbing

The complete pathway map is maintained in
`../data/art/art_asset_catalog.json`. Every inventory family has a stable
production directory, filename pattern, placeholder type, and status;
generated review files receive exact item-level entries. The placeholder SVGs
live in `placeholders/` and are intentionally generic: they prove wiring and
layout without being mistaken for approved art.

Use `scenes/art/art_asset_slot.tscn` for any new scene art reference. Set its
catalog `AssetId`; the slot resolves the placeholder by default and only
loads an approved production path after the catalog entry is marked
`APPROVED` and `UseApprovedAsset` is enabled. The sample
`scenes/art/art_placeholder_gallery.tscn` exercises representative entries.

The folder structure and catalog also reserve pathways for held tower, unit,
and enemy art. Those entries remain `HOLD` and are not generated or wired
into gameplay until the implementation review is complete.

## Individual generation prompt queue

`ART_GENERATION_PROMPTS.md` is the copy-ready production queue for every
active inventory family. It contains one numbered prompt per output, a shared
style lock, theater palette anchors, exact route-edge socket rules, ten route
topologies and ten transition tiles per theater, target filenames, generation
order, and acceptance checks. Use one theater per generation conversation and
keep its first accepted output attached as a style reference.

The queue intentionally omits prompts for held tower, nation-unit, and enemy
identity art. Those categories remain listed as `HOLD` until their
implementation review is complete.

## Shared assets

### Gameplay and map readability

- `NEEDED` Build-pad base family: cleared earth, sandbag corner,
  timber-framed, concrete/foundation slab, bunker/ruin interior, and coastal
  platform; each needs empty, available, selected, unavailable, and occupied
  states using shape plus color.
- `NEEDED` Build-pad overlays: hover, build-menu-open glow, range-preview
  anchor, Enclosed marker, Elevated marker, and Coastal marker.
- `NEEDED` Route markers: entry marker, objective marker, route shoulder,
  intersection, merge, split, and bridge approach.
- `NEEDED` Shared transition pieces: 10–15 road/terrain edges per theater
  kit, including dirt-to-grass, road-to-field, road-to-rock, road-to-snow,
  road-to-water, and path shoulder variants.
- `NEEDED` Shared obstruction and damage overlays: shell crater, scorch mark,
  rubble patch, broken timber, wreck fragment, oil stain, tire rut,
  footprints, mud patch, weeds, leaf pile, stones, and sand ripples.

### Command-table and UI presentation

- `NEEDED` Commander’s map-table frame: wood, brass fittings, paper map
  surface, edge shadows, map pins, grease-pencil marks, and neutral center
  space for the playable battlefield.
- `NEEDED` Paper mission slip, briefing card, objective stamp, operation
  marker, compass/scale, faction-neutral map pins, and warning strip.
- `NEEDED` Shared armor/damage/status glyph family: bullet, burst, chevron,
  wing, shield shapes, Suppressed, Spotted, Shielded, and air-warning badge.
- `NEEDED` Weather/lighting overlays: overcast, night, dust/sandstorm,
  rain/mist, snow atmosphere, tide line, and restrained haze.

## Western Europe kit

Target: 4–6 ground materials, 3–5 route treatments, 10–15 transitions,
5–8 tree variants, 6–10 bushes, 8–12 grass/plant variants, 4–8 structures,
2–4 hero structures, 25–40 props, 12–20 clusters, and 20–30 decals.

- `NEEDED` Terrain: patchwork field, rich grass, damp earth, mud, gravel,
  limestone, sunken lane, hedgerow wall, drainage ditch, orchard floor,
  stone wall, and road shoulders.
- `NEEDED` Vegetation: orchard trees, broadleaf trees, hedge masses, bushes,
  tall grass, wheat clumps, weeds, fallen leaves, and field-edge plants.
- `NEEDED` Architecture: Norman stone farmhouse, ruined church, village
  facade, stone bridge, barn, farm shed, mill/windmill, and drainage/culvert
  pieces; damaged versions where useful.
- `NEEDED` Flavor families: farm (fence, gate, cart, hay, trough, barrels,
  tools), road (sign, milestone, utility pole, broken fence, debris), supply
  (crate, barrel, tarp, pallet, ammunition box, handcart), defensive
  (sandbags, stakes, camouflage net, field radio, barrier), and damage
  (crater, rubble, scorch, broken timber, wreck fragment).
- `NEEDED` Clusters: supply dump, abandoned position, farmyard, road repair,
  field kitchen, hedgerow defensive position, and damaged farmhouse edge.

## Mediterranean / North Africa kit

- `NEEDED` Terrain: warm sand, dusty ochre, pale limestone, gravel, rocky
  outcrop, wadi bed, dry grass, olive-grove soil, vineyard rows, coastal
  sand, and turquoise/shallow-water edge.
- `NEEDED` Vegetation: olive trees, scrub, dry bushes, palms, thorny brush,
  vineyard foliage, reed clumps, and sparse grasses.
- `NEEDED` Architecture: stucco house, stone village wall, fuel depot,
  watch post, cistern, depot shed, coastal battery, and wadi bridge/culvert.
- `NEEDED` Flavor families: fuel drums, jerry cans, canvas shade, crates,
  pallets, handcart, stone markers, rope, fencing, ammunition boxes, radio,
  sandbags, camouflage net, and broken equipment.
- `NEEDED` Clusters and decals: fuel dump, desert supply stop, wadi crossing,
  rocky gun position, village courtyard, tire tracks, sand ripples, dust
  patches, rock scatter, and heat-haze overlay.

## Eastern Europe kit

- `NEEDED` Terrain: muted forest floor, dark mud, broad field, birch-leaf
  ground, snow, packed snow road, ice/frozen water, rail ballast, rusted
  industrial ground, and cold blue-grey water.
- `NEEDED` Vegetation: birch variants, conifers, bare winter trees, dense
  forest masses, bushes, reeds, snow grass, fallen branches, and logging
  debris.
- `NEEDED` Architecture: wooden hut, logging camp, rail station, factory,
  warehouse, maintenance shed, bridge, signal tower, and damaged industrial
  facade.
- `NEEDED` Flavor families: sleepers, rail tools, toolbox, oil drums,
  maintenance cart, timber stack, firewood, crates, snow-covered tarp,
  barricades, field telephone, sandbags, and rusted scrap.
- `NEEDED` Clusters and decals: rail maintenance, logging camp, frozen
  checkpoint, factory yard, abandoned position, snow disturbance, tire ruts,
  muddy wheel tracks, rust stains, broken boards, and frost/snow overlays.

## Pacific kit

- `NEEDED` Terrain: deep jungle floor, warm mud, wet mud, volcanic grey rock,
  coral beach, beach sand, shallow/deep water, river ford, airstrip surface,
  and timber track shoulders.
- `NEEDED` Vegetation: palms, broad jungle canopy, bamboo/timber vegetation,
  vines, ferns, broadleaf bushes, reeds, grasses, fallen fronds, and low
  foliage masses that do not resemble infantry silhouettes.
- `NEEDED` Architecture: timber/bamboo hut, jungle bunker, supply dump,
  airstrip control hut, fuel shelter, bridge/ford structure, observation
  post, and weathered depot.
- `NEEDED` Flavor families: crates, fuel drums, tarpaulin, pallet, handcart,
  radio equipment, timber stacks, rope, bamboo stakes, sandbags, coral
  rubble, broken planks, and airfield markers.
- `NEEDED` Clusters and decals: jungle cache, river ford, airfield supply,
  bunker perimeter, timber worksite, wet footprints, tire tracks, shell
  marks, leaf piles, mud splashes, and waterline/tide overlays.

## Miscellaneous and later-held categories

- `NEEDED` Shared destruction states: intact/damaged/ruined versions for
  hero buildings, bridges, depots, and defensive props. No civilians or
  graphic aftermath.
- `NEEDED` Shared visual effects: muzzle flashes, tracers, projectile glows,
  mortar/artillery arcs, impact markers, dust puffs, smoke puffs, suppression
  indicator, spotted indicator, shield segment, and non-graphic defeat puffs.
- `NEEDED` Objective and interaction assets: objective structure markers,
  build-menu radial anchors, selection rings, range-circle material, target
  line, and tower placement confirmation.
- `NEEDED` Documentation/reference assets: black-silhouette test sheets,
  grayscale test sheets, 25%-blur test sheets, and theater palette swatches.
- `HOLD` Nation-specific tower art, tower upgrade states, national insignia,
  firing/tracer art, and signature-tower art.
- `HOLD` Nation-specific unit art, all enemy art, enemy wrecks, and friendly
  unit art. These are excluded until the implementation review requested by
  the user is complete.

## Acceptance gates for every generated asset

Before changing `REVIEW` to `APPROVED`, check the art spec’s silhouette,
native-scale, function, contrast, rotation, style, detail, tone, grayscale,
blur, and screenshot tests. Generated images are references until they have
been cropped/cleaned, imported at the intended size, and checked in Clean,
Typical, and Stress gameplay screenshots.
