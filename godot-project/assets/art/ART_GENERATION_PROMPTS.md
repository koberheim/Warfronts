# Fronts of War individual art-generation prompts

This is the production prompt queue for every active, non-held item in
`ART_ASSET_INVENTORY.md`. Generate one numbered prompt at a time. The held
tower, national-unit, and enemy categories are listed at the end but do not
receive prompts until their implementation review is cleared.

## How to use this file

In a new image-generation conversation, paste the **Canonical style lock**
once, then send one numbered prompt at a time. Keep the same conversation for
one theater kit. Attach the first accepted image from that kit as a visual
reference for later prompts whenever the generator supports reference images.

Save each result to the path shown. Use `v01` for the first accepted output;
keep rejected generations outside the production folders.

## Canonical style lock

> Create production art for **Fronts of War**, a painterly storybook 2D
> tower-defense game with an **80% stylized / 20% grounded** visual direction.
> Use a true top-down mechanical footprint with a subtle 65–75 degree
> overhead visual cheat, broad confident slightly irregular shapes, strong
> primary silhouettes, compressed detail, rich but controlled color, broad
> hand-painted material masses, restrained fine texture, soft painted edge
> separation, and no photorealism or toy-like comedy. Environment should be
> lower contrast than units and towers. Keep roads, paths, build pads, and
> gameplay space immediately readable at 25% scale and in grayscale. No text,
> letters, numbers, watermark, people, soldiers, units, vehicles, real or
> fictional insignia, flags, propaganda, political symbols, blood, gore,
> corpses, or graphic aftermath unless a prompt explicitly asks for a neutral
> UI symbol. Preserve transparent backgrounds exactly where requested; never
> replace transparency with black, a vignette, a studio backdrop, or a glow.

## Tile and route socket contract

Apply this contract to every prompt marked **TILE** or **ROUTE TILE**:

- Output one square 1024×1024 PNG, orthographic top-down, no border or frame.
- Full-bleed terrain must match its theater palette at all four edges.
- A route socket is exactly 256 px wide, centered on the named edge, and
  enters perpendicular to that edge for at least 160 px before bending.
- Edge names are North/top, East/right, South/bottom, West/left.
- Use only the named sockets. Do not add accidental paths, trails, openings,
  bridges, or gaps touching another edge.
- Keep the outermost 32 px of every connected edge visually simple so two
  tiles can meet without a hard seam.
- Straight and corner variants in one route family must use the same road
  width, shoulder width, rut placement rhythm, ground values, and lighting.
- Do not bake buildings, units, props, UI, shadows from off-tile objects, or
  impassable foliage into route tiles.

## Cutout contract

Apply this to every prompt marked **CUTOUT**, **PROP**, **STRUCTURE**, or
**DECAL**: one centered object, complete silhouette, transparent background,
no cropping, no environment plate, no black halo, no vignette, no text, and a
soft local contact shadow contained inside the object's footprint only.

---

# Shared gameplay and map readability

## Build-pad bases

Generate the EMPTY state first for each base. Use that accepted image as the
reference for its other four states so the footprint and materials do not
change. Each output is a transparent 256×256 PNG centered on a 128 px circular
or square gameplay footprint.

### Cleared-earth base

- **SH-PAD-001 — empty**
  File: `shared/build_pads/base/build_pad_cleared_earth_empty_v01.png`
  Prompt: CUTOUT build pad, cleared compact earth with four subtle construction stakes and a broken chalk ring; quiet neutral state, no finished emplacement.
- **SH-PAD-002 — available**
  File: `shared/build_pads/base/build_pad_cleared_earth_available_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-001 exactly, available state with a complete pale ring and four outward tick marks, restrained green-gold accent.
- **SH-PAD-003 — selected**
  File: `shared/build_pads/base/build_pad_cleared_earth_selected_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-001 exactly, selected state with a double ring, four triangular pointer notches, and brighter amber highlight.
- **SH-PAD-004 — unavailable**
  File: `shared/build_pads/base/build_pad_cleared_earth_unavailable_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-001 exactly, unavailable state with broken ring segments and a clear diagonal barred shape, muted red-brown accent.
- **SH-PAD-005 — occupied**
  File: `shared/build_pads/base/build_pad_cleared_earth_occupied_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-001 exactly, occupied state with four inward corner brackets and a solid center footprint marker, cool steel-grey accent.

### Sandbag-corner base

- **SH-PAD-006 — empty**
  File: `shared/build_pads/base/build_pad_sandbag_corner_empty_v01.png`
  Prompt: CUTOUT build pad, four chunky low sandbag corner groups around open earth, large gaps between corners, subtle boundary stakes, clearly unfinished.
- **SH-PAD-007 — available**
  File: `shared/build_pads/base/build_pad_sandbag_corner_available_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-006 exactly, available state with complete pale ring and outward ticks, restrained green-gold accent.
- **SH-PAD-008 — selected**
  File: `shared/build_pads/base/build_pad_sandbag_corner_selected_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-006 exactly, selected state with double ring and four triangular pointer notches, bright amber highlight.
- **SH-PAD-009 — unavailable**
  File: `shared/build_pads/base/build_pad_sandbag_corner_unavailable_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-006 exactly, unavailable state with broken ring and diagonal barred shape, muted red-brown accent.
- **SH-PAD-010 — occupied**
  File: `shared/build_pads/base/build_pad_sandbag_corner_occupied_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-006 exactly, occupied state with inward steel corner brackets and solid center footprint marker.

### Timber-framed base

- **SH-PAD-011 — empty**
  File: `shared/build_pads/base/build_pad_timber_framed_empty_v01.png`
  Prompt: CUTOUT build pad, low rough timber square frame pegged into compact earth, open center, visibly prepared but not a completed tower.
- **SH-PAD-012 — available**
  File: `shared/build_pads/base/build_pad_timber_framed_available_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-011 exactly, available state with pale outer ring and outward ticks, restrained green-gold accent.
- **SH-PAD-013 — selected**
  File: `shared/build_pads/base/build_pad_timber_framed_selected_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-011 exactly, selected state with double amber ring and four triangular pointer notches.
- **SH-PAD-014 — unavailable**
  File: `shared/build_pads/base/build_pad_timber_framed_unavailable_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-011 exactly, unavailable state with broken red-brown ring and diagonal barred shape.
- **SH-PAD-015 — occupied**
  File: `shared/build_pads/base/build_pad_timber_framed_occupied_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-011 exactly, occupied state with steel inward brackets and solid center footprint marker.

### Concrete-foundation base

- **SH-PAD-016 — empty**
  File: `shared/build_pads/base/build_pad_concrete_foundation_empty_v01.png`
  Prompt: CUTOUT build pad, low cracked concrete foundation slab with four exposed anchor points, clean open center, no weapon or finished structure.
- **SH-PAD-017 — available**
  File: `shared/build_pads/base/build_pad_concrete_foundation_available_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-016 exactly, available state with pale ring and outward ticks, restrained green-gold accent.
- **SH-PAD-018 — selected**
  File: `shared/build_pads/base/build_pad_concrete_foundation_selected_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-016 exactly, selected state with double amber ring and triangular pointer notches.
- **SH-PAD-019 — unavailable**
  File: `shared/build_pads/base/build_pad_concrete_foundation_unavailable_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-016 exactly, unavailable state with broken red-brown ring and diagonal barred shape.
- **SH-PAD-020 — occupied**
  File: `shared/build_pads/base/build_pad_concrete_foundation_occupied_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-016 exactly, occupied state with inward steel brackets and solid center footprint marker.

### Bunker/ruin-interior base

- **SH-PAD-021 — empty**
  File: `shared/build_pads/base/build_pad_bunker_ruin_empty_v01.png`
  Prompt: CUTOUT build pad inside a shallow ruined bunker footprint, broken low masonry edges and open clean center, no debris that reads as a tower.
- **SH-PAD-022 — available**
  File: `shared/build_pads/base/build_pad_bunker_ruin_available_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-021 exactly, available state with pale ring and outward ticks, restrained green-gold accent.
- **SH-PAD-023 — selected**
  File: `shared/build_pads/base/build_pad_bunker_ruin_selected_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-021 exactly, selected state with double amber ring and triangular pointer notches.
- **SH-PAD-024 — unavailable**
  File: `shared/build_pads/base/build_pad_bunker_ruin_unavailable_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-021 exactly, unavailable state with broken red-brown ring and diagonal barred shape.
- **SH-PAD-025 — occupied**
  File: `shared/build_pads/base/build_pad_bunker_ruin_occupied_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-021 exactly, occupied state with inward steel brackets and solid center footprint marker.

### Coastal-platform base

- **SH-PAD-026 — empty**
  File: `shared/build_pads/base/build_pad_coastal_platform_empty_v01.png`
  Prompt: CUTOUT build pad, weathered timber-and-concrete coastal platform with salt wear and four mooring-like corner posts, open center, no weapon.
- **SH-PAD-027 — available**
  File: `shared/build_pads/base/build_pad_coastal_platform_available_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-026 exactly, available state with pale ring and outward ticks, restrained green-gold accent.
- **SH-PAD-028 — selected**
  File: `shared/build_pads/base/build_pad_coastal_platform_selected_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-026 exactly, selected state with double amber ring and triangular pointer notches.
- **SH-PAD-029 — unavailable**
  File: `shared/build_pads/base/build_pad_coastal_platform_unavailable_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-026 exactly, unavailable state with broken red-brown ring and diagonal barred shape.
- **SH-PAD-030 — occupied**
  File: `shared/build_pads/base/build_pad_coastal_platform_occupied_v01.png`
  Prompt: CUTOUT build pad matching SH-PAD-026 exactly, occupied state with inward steel brackets and solid center footprint marker.

## Build-pad overlays

- **SH-PAD-031 — hover overlay**
  File: `shared/build_pads/overlays/build_pad_overlay_hover_v01.png`
  Prompt: CUTOUT transparent hover overlay only, soft pale-gold segmented ring with four outward ticks, no base material, readable at 64 px.
- **SH-PAD-032 — build-menu-open glow**
  File: `shared/build_pads/overlays/build_pad_overlay_menu_open_v01.png`
  Prompt: CUTOUT transparent build-menu overlay only, restrained radial amber glow with eight short mechanical notches, no text or base.
- **SH-PAD-033 — range-preview anchor**
  File: `shared/build_pads/overlays/build_pad_overlay_range_anchor_v01.png`
  Prompt: CUTOUT transparent range anchor, small central crosshair plus four corner brackets, pale cyan and white, shape-readable without color.
- **SH-PAD-034 — Enclosed marker**
  File: `shared/build_pads/overlays/build_pad_marker_enclosed_v01.png`
  Prompt: CUTOUT transparent Enclosed pad marker, compact roof-and-wall shield silhouette, neutral stone grey, no letters.
- **SH-PAD-035 — Elevated marker**
  File: `shared/build_pads/overlays/build_pad_marker_elevated_v01.png`
  Prompt: CUTOUT transparent Elevated pad marker, stacked upward chevrons over a low platform silhouette, pale gold, no letters.
- **SH-PAD-036 — Coastal marker**
  File: `shared/build_pads/overlays/build_pad_marker_coastal_v01.png`
  Prompt: CUTOUT transparent Coastal pad marker, simple wave-and-platform silhouette, muted turquoise and white, no letters.

## Route markers

- **SH-ROUTE-001 — entry marker**
  File: `shared/route_markers/route_marker_entry_v01.png`
  Prompt: CUTOUT route-entry marker, bold inward-pointing arrowhead inside a broken circular boundary, faction-neutral, no text.
- **SH-ROUTE-002 — objective marker**
  File: `shared/route_markers/route_marker_objective_v01.png`
  Prompt: CUTOUT objective marker, defended-map-pin silhouette inside a double diamond, faction-neutral, readable in grayscale, no text.
- **SH-ROUTE-003 — route shoulder marker**
  File: `shared/route_markers/route_marker_shoulder_v01.png`
  Prompt: CUTOUT subtle route-shoulder guide, two parallel broken edge strokes with small outward ticks, low contrast, no center line.
- **SH-ROUTE-004 — intersection marker**
  File: `shared/route_markers/route_marker_intersection_v01.png`
  Prompt: CUTOUT four-way intersection symbol with four equal arms and a hollow center, faction-neutral, no arrows or text.
- **SH-ROUTE-005 — merge marker**
  File: `shared/route_markers/route_marker_merge_v01.png`
  Prompt: CUTOUT merge symbol, two equal paths converging into one broad path, strong silhouette, no text.
- **SH-ROUTE-006 — split marker**
  File: `shared/route_markers/route_marker_split_v01.png`
  Prompt: CUTOUT split symbol, one broad path dividing into two equal paths, strong silhouette, no text.
- **SH-ROUTE-007 — bridge approach marker**
  File: `shared/route_markers/route_marker_bridge_approach_v01.png`
  Prompt: CUTOUT bridge-approach symbol, short road leading into two chunky bridge rails, no text or faction marks.

## Theater transition tiles

Each output is a TILE with no route sockets unless the prompt says otherwise.
The dividing boundary should cross the tile center and reach opposite edges at
matching midpoint positions, allowing rotated and mirrored use.

### Western Europe transitions

- **SH-TRANS-WE-001 — dirt to grass**
  File: `shared/transitions/transition_western_europe_dirt_to_grass_v01.png`
  Prompt: TILE Western Europe damp compact dirt blending naturally into rich bocage grass across the center; irregular painted edge, matching overcast light.
- **SH-TRANS-WE-002 — road to field**
  File: `shared/transitions/transition_western_europe_road_to_field_v01.png`
  Prompt: TILE Western Europe muddy road shoulder blending into ochre harvested field; one 256 px road socket on West only, clean crop boundary.
- **SH-TRANS-WE-003 — road to rock**
  File: `shared/transitions/transition_western_europe_road_to_rock_v01.png`
  Prompt: TILE Western Europe muddy lane shoulder blending into pale limestone rubble and compact earth; one West road socket only.
- **SH-TRANS-WE-004 — road to snow**
  File: `shared/transitions/transition_western_europe_road_to_snow_v01.png`
  Prompt: TILE Western Europe muddy lane receiving a thin late-winter snow cover; West and East route sockets, gradual snow accumulation without changing road width.
- **SH-TRANS-WE-005 — road to water**
  File: `shared/transitions/transition_western_europe_road_to_water_v01.png`
  Prompt: TILE Western Europe muddy lane ending at a shallow drainage ford; North road socket and South water opening, no bridge or vehicles.
- **SH-TRANS-WE-006 — path shoulder**
  File: `shared/transitions/transition_western_europe_path_shoulder_v01.png`
  Prompt: TILE Western Europe straight North–South lane shoulder strip, 256 px road width, damp grass, sparse stones, clean matching route sockets.
- **SH-TRANS-WE-007 — grass to orchard floor**
  File: `shared/transitions/transition_western_europe_grass_to_orchard_v01.png`
  Prompt: TILE Western Europe rich bocage grass blending across tile center into darker orchard soil and restrained leaf litter, no trees or route sockets.
- **SH-TRANS-WE-008 — mud to gravel**
  File: `shared/transitions/transition_western_europe_mud_to_gravel_v01.png`
  Prompt: TILE Western Europe damp mud blending across tile center into compact grey-brown farm gravel, irregular low-contrast boundary, no route sockets.
- **SH-TRANS-WE-009 — lane to stone bridge**
  File: `shared/transitions/transition_western_europe_lane_to_bridge_v01.png`
  Prompt: TILE Western Europe muddy lane with North and South 256 px sockets, compacting into pale limestone bridge-approach paving at tile center, no bridge structure.
- **SH-TRANS-WE-010 — lane to village ground**
  File: `shared/transitions/transition_western_europe_lane_to_village_v01.png`
  Prompt: TILE Western Europe muddy lane with West and East 256 px sockets, shoulders changing from damp grass to worn limestone village ground, no buildings.

### Mediterranean transitions

- **SH-TRANS-MED-001 — dirt to grass**
  File: `shared/transitions/transition_mediterranean_dirt_to_grass_v01.png`
  Prompt: TILE Mediterranean dusty ochre earth blending into sparse dry olive grass across the center, warm limestone flecks, hard sun softened for readability.
- **SH-TRANS-MED-002 — road to field**
  File: `shared/transitions/transition_mediterranean_road_to_field_v01.png`
  Prompt: TILE Mediterranean dusty road shoulder blending into dry cultivated field rows; one West road socket only.
- **SH-TRANS-MED-003 — road to rock**
  File: `shared/transitions/transition_mediterranean_road_to_rock_v01.png`
  Prompt: TILE Mediterranean dusty road blending into pale limestone and rocky outcrop fragments; one West road socket only.
- **SH-TRANS-MED-004 — road to wadi**
  File: `shared/transitions/transition_mediterranean_road_to_wadi_v01.png`
  Prompt: TILE Mediterranean dusty road with West and East 256 px sockets crossing a dry rocky wadi at tile center, same road width and pale stone shoulders, no bridge.
- **SH-TRANS-MED-005 — road to water**
  File: `shared/transitions/transition_mediterranean_road_to_water_v01.png`
  Prompt: TILE Mediterranean dusty coastal road ending at turquoise shallow-water ford; North road socket and South water opening, no bridge.
- **SH-TRANS-MED-006 — path shoulder**
  File: `shared/transitions/transition_mediterranean_path_shoulder_v01.png`
  Prompt: TILE Mediterranean straight North–South dusty road shoulder, 256 px width, sand, limestone, sparse dry grass, matching sockets.
- **SH-TRANS-MED-007 — sand to limestone**
  File: `shared/transitions/transition_mediterranean_sand_to_limestone_v01.png`
  Prompt: TILE Mediterranean warm sand blending across tile center into pale weathered limestone ground, irregular broad boundary, no route sockets.
- **SH-TRANS-MED-008 — dry grass to olive soil**
  File: `shared/transitions/transition_mediterranean_grass_to_olive_soil_v01.png`
  Prompt: TILE Mediterranean sparse dry grass blending into reddish olive-grove soil and restrained leaf litter, no trees, rows, or route sockets.
- **SH-TRANS-MED-009 — road to village ground**
  File: `shared/transitions/transition_mediterranean_road_to_village_v01.png`
  Prompt: TILE Mediterranean dusty road with West and East 256 px sockets, shoulders shifting into pale worn village limestone, no structures.
- **SH-TRANS-MED-010 — coastal sand to shallow water**
  File: `shared/transitions/transition_mediterranean_coastal_sand_to_water_v01.png`
  Prompt: TILE Mediterranean pale coastal sand on West blending into turquoise shallows on East, shoreline continuous North–South, no route sockets.

### Eastern Europe transitions

- **SH-TRANS-EE-001 — dirt to grass**
  File: `shared/transitions/transition_eastern_europe_dirt_to_grass_v01.png`
  Prompt: TILE Eastern Europe dark wet earth blending into muted forest grass and birch leaves, cold overcast light.
- **SH-TRANS-EE-002 — road to field**
  File: `shared/transitions/transition_eastern_europe_road_to_field_v01.png`
  Prompt: TILE Eastern Europe muddy road shoulder blending into broad muted field; one West road socket only, deep wheel wear held away from edge.
- **SH-TRANS-EE-003 — road to rock**
  File: `shared/transitions/transition_eastern_europe_road_to_rock_v01.png`
  Prompt: TILE Eastern Europe muddy road blending into cold grey stone and industrial gravel; one West road socket only.
- **SH-TRANS-EE-004 — road to snow**
  File: `shared/transitions/transition_eastern_europe_road_to_snow_v01.png`
  Prompt: TILE Eastern Europe muddy road gradually becoming packed snow road; West and East route sockets, identical width and rut rhythm.
- **SH-TRANS-EE-005 — road to water**
  File: `shared/transitions/transition_eastern_europe_road_to_water_v01.png`
  Prompt: TILE Eastern Europe packed road ending at a shallow cold blue-grey ford with thin edge ice; North road socket and South water opening.
- **SH-TRANS-EE-006 — path shoulder**
  File: `shared/transitions/transition_eastern_europe_path_shoulder_v01.png`
  Prompt: TILE Eastern Europe straight North–South muddy road shoulder, 256 px width, birch leaves, cold grass, matching sockets.
- **SH-TRANS-EE-007 — forest floor to snow**
  File: `shared/transitions/transition_eastern_europe_forest_to_snow_v01.png`
  Prompt: TILE Eastern Europe dark forest floor blending into fresh snow across tile center, broad irregular accumulation edge, no trees or route sockets.
- **SH-TRANS-EE-008 — mud to industrial ground**
  File: `shared/transitions/transition_eastern_europe_mud_to_industrial_v01.png`
  Prompt: TILE Eastern Europe dark mud blending into worn concrete, cinders, and restrained rust-stained industrial ground, no structures or route sockets.
- **SH-TRANS-EE-009 — ballast to factory yard**
  File: `shared/transitions/transition_eastern_europe_ballast_to_factory_v01.png`
  Prompt: TILE Eastern Europe dark rail ballast blending across tile center into compact factory-yard earth and concrete fragments, no rails or route sockets.
- **SH-TRANS-EE-010 — frozen water to snow bank**
  File: `shared/transitions/transition_eastern_europe_ice_to_snow_bank_v01.png`
  Prompt: TILE Eastern Europe blue-grey frozen water on West blending into a rounded snow-covered bank on East, boundary continuous North–South, no route.

### Pacific transitions

- **SH-TRANS-PAC-001 — dirt to grass**
  File: `shared/transitions/transition_pacific_dirt_to_grass_v01.png`
  Prompt: TILE Pacific warm wet jungle earth blending into low tropical ground cover, deep greens and volcanic-grey flecks, no tall foliage.
- **SH-TRANS-PAC-002 — road to field**
  File: `shared/transitions/transition_pacific_road_to_field_v01.png`
  Prompt: TILE Pacific mud-track shoulder blending into a cleared airfield margin; one West road socket only, compact weathered ground.
- **SH-TRANS-PAC-003 — road to rock**
  File: `shared/transitions/transition_pacific_road_to_rock_v01.png`
  Prompt: TILE Pacific jungle mud track blending into volcanic grey rock and coral rubble; one West road socket only.
- **SH-TRANS-PAC-004 — road to coral ground**
  File: `shared/transitions/transition_pacific_road_to_coral_v01.png`
  Prompt: TILE Pacific jungle mud track with West and East 256 px sockets, shoulders changing into pale compact coral ground, same route width and rut rhythm.
- **SH-TRANS-PAC-005 — road to water**
  File: `shared/transitions/transition_pacific_road_to_water_v01.png`
  Prompt: TILE Pacific jungle mud track ending at a shallow turquoise river ford; North road socket and South water opening, no bridge.
- **SH-TRANS-PAC-006 — path shoulder**
  File: `shared/transitions/transition_pacific_path_shoulder_v01.png`
  Prompt: TILE Pacific straight North–South jungle mud-track shoulder, 256 px width, roots and ferns kept outside the route, matching sockets.
- **SH-TRANS-PAC-007 — jungle floor to wet mud**
  File: `shared/transitions/transition_pacific_jungle_to_wet_mud_v01.png`
  Prompt: TILE Pacific deep leaf-covered jungle floor blending across tile center into broad wet umber mud, no tall foliage, tracks, or route sockets.
- **SH-TRANS-PAC-008 — mud to volcanic rock**
  File: `shared/transitions/transition_pacific_mud_to_volcanic_rock_v01.png`
  Prompt: TILE Pacific warm mud blending into blue-grey porous volcanic ground across an irregular broad boundary, no route sockets.
- **SH-TRANS-PAC-009 — airstrip to jungle margin**
  File: `shared/transitions/transition_pacific_airstrip_to_jungle_v01.png`
  Prompt: TILE Pacific compacted coral airstrip surface blending into low jungle ground cover at tile center, no markings, tall foliage, or route sockets.
- **SH-TRANS-PAC-010 — coral beach to shallow water**
  File: `shared/transitions/transition_pacific_coral_beach_to_water_v01.png`
  Prompt: TILE Pacific coral beach on West blending into blue-green shallow water on East, shoreline continuous North–South with restrained foam, no route.

## Shared obstruction and damage decals

- **SH-DECAL-001 — shell crater**
  File: `shared/decals/obstruction_damage/decal_shell_crater_v01.png`
  Prompt: DECAL shallow non-graphic shell crater, irregular dark earth rim and exposed soil, transparent outside, no smoke or remains.
- **SH-DECAL-002 — scorch mark**
  File: `shared/decals/obstruction_damage/decal_scorch_mark_v01.png`
  Prompt: DECAL soft radial soot and heat-darkened ground stain, irregular transparent edges, no active flame.
- **SH-DECAL-003 — rubble patch**
  File: `shared/decals/obstruction_damage/decal_rubble_patch_v01.png`
  Prompt: DECAL low scattered masonry rubble, broad readable chunks, flat enough not to resemble a gameplay obstacle.
- **SH-DECAL-004 — broken timber**
  File: `shared/decals/obstruction_damage/decal_broken_timber_v01.png`
  Prompt: DECAL three or four splintered timber pieces in a loose flat arrangement, no weapon shapes.
- **SH-DECAL-005 — wreck fragment**
  File: `shared/decals/obstruction_damage/decal_wreck_fragment_v01.png`
  Prompt: DECAL small anonymous bent metal fragments, neutral military-industrial shapes, no insignia or recognizable vehicle identity.
- **SH-DECAL-006 — oil stain**
  File: `shared/decals/obstruction_damage/decal_oil_stain_v01.png`
  Prompt: DECAL irregular dark oil stain with faint brown-blue sheen, transparent feathered edge, no container.
- **SH-DECAL-007 — tire rut**
  File: `shared/decals/obstruction_damage/decal_tire_rut_v01.png`
  Prompt: DECAL two short parallel muddy tire impressions, subtle and repeatable, no vehicle.
- **SH-DECAL-008 — footprints**
  File: `shared/decals/obstruction_damage/decal_footprints_v01.png`
  Prompt: DECAL short trail of abstract boot impressions in mud, no person, no blood, low contrast.
- **SH-DECAL-009 — mud patch**
  File: `shared/decals/obstruction_damage/decal_mud_patch_v01.png`
  Prompt: DECAL damp irregular mud patch with two broad value masses and restrained puddle highlights.
- **SH-DECAL-010 — weeds**
  File: `shared/decals/obstruction_damage/decal_weeds_v01.png`
  Prompt: DECAL low scattered weeds viewed top-down, broad leaves, no infantry-like vertical silhouettes.
- **SH-DECAL-011 — leaf pile**
  File: `shared/decals/obstruction_damage/decal_leaf_pile_v01.png`
  Prompt: DECAL wind-gathered low leaf pile, muted green-brown-orange, irregular transparent boundary.
- **SH-DECAL-012 — stones**
  File: `shared/decals/obstruction_damage/decal_stones_v01.png`
  Prompt: DECAL loose scatter of flat small stones in three sizes, neutral grey-brown, soft contained shadows.
- **SH-DECAL-013 — sand ripples**
  File: `shared/decals/obstruction_damage/decal_sand_ripples_v01.png`
  Prompt: DECAL shallow wind-shaped sand ripples, warm ochre, seamless feathered transparency, no footprints.

---

# Shared command-table and UI prompts

## Mission paperwork and neutral markers

- **SH-UI-001 — paper mission slip**
  File: `shared/ui/mission_paperwork/ui_paper_mission_slip_v01.png`
  Prompt: CUTOUT aged cream mission slip with folded corners, faint blank rule lines and clip shadow, no readable text or symbols.
- **SH-UI-002 — briefing card**
  File: `shared/ui/mission_paperwork/ui_briefing_card_v01.png`
  Prompt: CUTOUT sturdy off-white briefing card with tabbed edge, empty central writing area, no text or insignia.
- **SH-UI-003 — objective stamp**
  File: `shared/ui/mission_paperwork/ui_objective_stamp_v01.png`
  Prompt: CUTOUT abstract stamped double-diamond objective symbol in muted red ink, distressed edges, no letters or real emblem.
- **SH-UI-004 — operation marker**
  File: `shared/ui/mission_paperwork/ui_operation_marker_v01.png`
  Prompt: CUTOUT faction-neutral metal map marker, circular base with raised chevron and pin, brass and dark steel, no insignia.
- **SH-UI-005 — compass and scale**
  File: `shared/ui/mission_paperwork/ui_compass_scale_v01.png`
  Prompt: CUTOUT map compass rose and scale graphic using ticks and shapes only, no letters or numbers, dark ink on transparent background.
- **SH-UI-006 — neutral map-pin set**
  File: `shared/ui/mission_paperwork/ui_neutral_map_pins_v01.png`
  Prompt: CUTOUT set of five faction-neutral brass map pins with distinct circle, square, triangle, diamond, and chevron heads; no flags.
- **SH-UI-007 — warning strip**
  File: `shared/ui/mission_paperwork/ui_warning_strip_v01.png`
  Prompt: CUTOUT horizontal paper warning strip with alternating dark and amber angled blocks, distressed print, no text.

## Readability glyphs

- **SH-GLYPH-001 — bullet / small arms**
  File: `shared/glyphs/glyph_bullet_v01.png`
  Prompt: CUTOUT 32 px gameplay glyph, bold single cartridge silhouette for Small Arms, white and dark outline, no text.
- **SH-GLYPH-002 — burst / explosive**
  File: `shared/glyphs/glyph_burst_v01.png`
  Prompt: CUTOUT 32 px gameplay glyph, chunky rounded-angular blast silhouette for Explosive damage, shape-first, no text.
- **SH-GLYPH-003 — chevron / armor piercing**
  File: `shared/glyphs/glyph_chevron_v01.png`
  Prompt: CUTOUT 32 px gameplay glyph, sharp forward chevron penetrating a plate silhouette for Armor-Piercing, no text.
- **SH-GLYPH-004 — wing / anti-air**
  File: `shared/glyphs/glyph_wing_v01.png`
  Prompt: CUTOUT 32 px gameplay glyph, broad wing crossed by upward sight line for Anti-Air, no text.
- **SH-GLYPH-005 — shield / armor**
  File: `shared/glyphs/glyph_shield_v01.png`
  Prompt: CUTOUT 32 px gameplay glyph, broad geometric shield with heavy lower edge, grayscale-readable, no text.
- **SH-GLYPH-006 — Suppressed**
  File: `shared/glyphs/glyph_suppressed_v01.png`
  Prompt: CUTOUT 32 px status glyph, grey dust-cloud silhouette with bold downward arrow, no text.
- **SH-GLYPH-007 — Spotted**
  File: `shared/glyphs/glyph_spotted_v01.png`
  Prompt: CUTOUT 32 px status glyph, red crosshair reticle with open center and four separated arms, no text.
- **SH-GLYPH-008 — Shielded**
  File: `shared/glyphs/glyph_shielded_v01.png`
  Prompt: CUTOUT 32 px status glyph, cyan segmented hexagonal shield ring, distinct from the solid armor shield, no text.
- **SH-GLYPH-009 — air warning**
  File: `shared/glyphs/glyph_air_warning_v01.png`
  Prompt: CUTOUT 32 px warning glyph, top-down wing silhouette inside an open warning diamond, amber and white, no text.

## Weather and lighting overlays

Each output is a transparent 1920×1080 overlay with no opaque frame.

- **SH-WEATHER-001 — overcast**
  File: `shared/weather/weather_overcast_v01.png`
  Prompt: Transparent full-screen overcast-light overlay, cool soft grey edge falloff and subtle cloud-value variation, center remains clear and readable.
- **SH-WEATHER-002 — night**
  File: `shared/weather/weather_night_v01.png`
  Prompt: Transparent full-screen restrained blue-black night wash with slightly lighter route center, no stars, moon, or opaque darkness.
- **SH-WEATHER-003 — dust/sandstorm**
  File: `shared/weather/weather_dust_sandstorm_v01.png`
  Prompt: Transparent full-screen warm dust streaks and edge haze, sparse center, no object silhouettes, suitable for reduced-opacity animation.
- **SH-WEATHER-004 — rain/mist**
  File: `shared/weather/weather_rain_mist_v01.png`
  Prompt: Transparent full-screen light diagonal rain strokes and low mist wisps, sparse center, no heavy fog.
- **SH-WEATHER-005 — snow atmosphere**
  File: `shared/weather/weather_snow_atmosphere_v01.png`
  Prompt: Transparent full-screen sparse snow flecks and soft cold haze, varied flake sizes, no whiteout.
- **SH-WEATHER-006 — tide line**
  File: `shared/weather/weather_tide_line_v01.png`
  Prompt: Transparent horizontal tide-edge overlay with shallow foam, wet sand darkening, and irregular waterline, no beach plate.
- **SH-WEATHER-007 — restrained haze**
  File: `shared/weather/weather_restrained_haze_v01.png`
  Prompt: Transparent full-screen neutral atmospheric haze, subtle corners and broad soft bands, center nearly clear.

---

# Shared destruction, VFX, interaction, and reference prompts

## Destruction states

Create each subject as a matched three-image set. No civilians, remains, gore,
active fire, or ideological markings.

- **SH-DEST-001 — hero building intact**
  File: `shared/destruction/hero_building_intact_v01.png`
  Prompt: STRUCTURE faction-neutral stone hero building, intact broad roof and strong top-down silhouette, complete isolated footprint.
- **SH-DEST-002 — hero building damaged**
  File: `shared/destruction/hero_building_damaged_v01.png`
  Prompt: STRUCTURE exact SH-DEST-001 building with limited roof holes, exposed rafters, small rubble masses, same footprint and angle.
- **SH-DEST-003 — hero building ruined**
  File: `shared/destruction/hero_building_ruined_v01.png`
  Prompt: STRUCTURE exact SH-DEST-001 building reduced to readable low walls, broken roof silhouette and dark interior, same footprint.
- **SH-DEST-004 — bridge intact**
  File: `shared/destruction/bridge_intact_v01.png`
  Prompt: STRUCTURE faction-neutral short stone-and-timber bridge, intact deck, straight route clearance, isolated transparent cutout.
- **SH-DEST-005 — bridge damaged**
  File: `shared/destruction/bridge_damaged_v01.png`
  Prompt: STRUCTURE exact SH-DEST-004 bridge with cracked rail, missing planks and minor rubble, route still visually readable.
- **SH-DEST-006 — bridge ruined**
  File: `shared/destruction/bridge_ruined_v01.png`
  Prompt: STRUCTURE exact SH-DEST-004 bridge collapsed at center, broken abutments and debris, same total footprint.
- **SH-DEST-007 — depot intact**
  File: `shared/destruction/depot_intact_v01.png`
  Prompt: STRUCTURE faction-neutral compact field depot, low roof, loading awning, stacked neutral supply forms, isolated cutout.
- **SH-DEST-008 — depot damaged**
  File: `shared/destruction/depot_damaged_v01.png`
  Prompt: STRUCTURE exact SH-DEST-007 depot with partial roof damage, scattered boards and soot, no active flame.
- **SH-DEST-009 — depot ruined**
  File: `shared/destruction/depot_ruined_v01.png`
  Prompt: STRUCTURE exact SH-DEST-007 depot as low non-graphic ruin, collapsed roof and broad rubble masses, same footprint.
- **SH-DEST-010 — defensive prop intact**
  File: `shared/destruction/defensive_prop_intact_v01.png`
  Prompt: PROP faction-neutral sandbag-and-timber field barrier, intact compact silhouette, no weapon or insignia.
- **SH-DEST-011 — defensive prop damaged**
  File: `shared/destruction/defensive_prop_damaged_v01.png`
  Prompt: PROP exact SH-DEST-010 barrier with displaced sandbags and one broken timber, same footprint.
- **SH-DEST-012 — defensive prop ruined**
  File: `shared/destruction/defensive_prop_ruined_v01.png`
  Prompt: PROP exact SH-DEST-010 barrier collapsed into low scattered sandbags and timber, non-graphic.

## Combat VFX

Each output is a centered transparent 512×512 effect sprite or short 4×1
frame strip where motion is requested. Use strong graphic shapes and keep the
effect readable over all theater palettes.

- **SH-VFX-001 — muzzle flash**
  File: `shared/vfx/vfx_muzzle_flash_v01.png`
  Prompt: Transparent 4×1 frame strip of a bright directional wedge-star muzzle flash, warm white core, amber edge, rapid expansion and fade.
- **SH-VFX-002 — tracer**
  File: `shared/vfx/vfx_tracer_v01.png`
  Prompt: Transparent horizontal tracer sprite, short bright core with tapered amber tail, no continuous laser beam.
- **SH-VFX-003 — projectile glow**
  File: `shared/vfx/vfx_projectile_glow_v01.png`
  Prompt: Transparent compact projectile glow, bright center with narrow directional halo, crisp and small, no explosion.
- **SH-VFX-004 — mortar/artillery arc marker**
  File: `shared/vfx/vfx_artillery_arc_marker_v01.png`
  Prompt: Transparent dotted arc-path marker with fading round segments and a small landing chevron, no text.
- **SH-VFX-005 — impact marker**
  File: `shared/vfx/vfx_impact_marker_v01.png`
  Prompt: Transparent circular impact warning, broken amber ring with four inward wedges, clear empty center, no text.
- **SH-VFX-006 — dust puff**
  File: `shared/vfx/vfx_dust_puff_v01.png`
  Prompt: Transparent 4×1 frame strip of chunky overlapping tan dust puffs expanding and dissipating rapidly.
- **SH-VFX-007 — smoke puff**
  File: `shared/vfx/vfx_smoke_puff_v01.png`
  Prompt: Transparent 4×1 frame strip of broad cool-grey painted smoke puffs, short-lived and non-obscuring.
- **SH-VFX-008 — suppression indicator**
  File: `shared/vfx/vfx_suppression_indicator_v01.png`
  Prompt: Transparent status effect, low grey dust cloud with two bold downward chevrons, no text.
- **SH-VFX-009 — spotted indicator**
  File: `shared/vfx/vfx_spotted_indicator_v01.png`
  Prompt: Transparent red crosshair pulse with open center, four separated arms and one soft outward echo ring.
- **SH-VFX-010 — shield segment**
  File: `shared/vfx/vfx_shield_segment_v01.png`
  Prompt: Transparent cyan hexagonal shield segment with bright rim, dark inner face and subtle crack-ready geometry.
- **SH-VFX-011 — defeat puff**
  File: `shared/vfx/vfx_defeat_puff_v01.png`
  Prompt: Transparent 4×1 frame strip of non-graphic neutral dust-and-smoke disappearance puff with one small abstract dropped token.

## Objective and interaction assets

- **SH-INT-001 — objective structure marker**
  File: `shared/interaction/interaction_objective_marker_v01.png`
  Prompt: CUTOUT objective marker, bold defended-house silhouette inside a double diamond and ground ring, faction-neutral, no text.
- **SH-INT-002 — build-menu radial anchor**
  File: `shared/interaction/interaction_build_radial_anchor_v01.png`
  Prompt: CUTOUT eight-segment radial anchor with open center and chunky wedge sockets, parchment-white and amber, no icons or text.
- **SH-INT-003 — selection ring**
  File: `shared/interaction/interaction_selection_ring_v01.png`
  Prompt: CUTOUT transparent circular selection ring with four bold corner brackets and broken intermediate arcs, white and gold.
- **SH-INT-004 — range-circle material**
  File: `shared/interaction/interaction_range_circle_v01.png`
  Prompt: CUTOUT transparent soft range-circle fill and crisp dashed rim, pale cyan, center mostly transparent, no text.
- **SH-INT-005 — target line**
  File: `shared/interaction/interaction_target_line_v01.png`
  Prompt: CUTOUT horizontal dashed targeting line with tapered arrowhead and faint dark under-stroke, no text.
- **SH-INT-006 — placement confirmation**
  File: `shared/interaction/interaction_placement_confirmation_v01.png`
  Prompt: CUTOUT placement-confirmation burst, four outward corner brackets and a strong check-shaped silhouette without letters.

## Documentation/reference assets

These are assembled after the relevant production assets exist. Use the named
inputs as references; do not invent new unit or tower art.

- **SH-REF-001 — black-silhouette sheet**
  File: `misc/references/reference_black_silhouette_test_v01.png`
  Prompt: Arrange the supplied approved assets at native gameplay scale as solid black silhouettes on white, evenly spaced, no labels or alterations.
- **SH-REF-002 — grayscale sheet**
  File: `misc/references/reference_grayscale_test_v01.png`
  Prompt: Arrange the supplied approved assets at native gameplay scale in grayscale on neutral mid-grey, preserving value contrast, no labels.
- **SH-REF-003 — 25-percent blur sheet**
  File: `misc/references/reference_blur_25_percent_test_v01.png`
  Prompt: Arrange the supplied gameplay screenshot at 25% size with a strong uniform blur, preserving composition, no added marks or text.
- **SH-REF-004 — theater palette sheet**
  File: `misc/references/reference_theater_palette_swatches_v01.png`
  Prompt: Create four clean unlabeled horizontal color-swatch rows sampled from the supplied approved theater references, eight broad swatches per row, no symbols.

---

# Theater route topology prompts

Generate all ten route topologies for each theater from the same accepted
straight tile reference. These are the minimum connectivity set used by the
map planner: straight NS, straight EW, corners NE/ES/SW/WN, T-junctions
NES/ESW, cross NESW, and a North entry/dead-end. The socket contract above is
mandatory.

## Western Europe sunken-lane route set

- **WE-ROUTE-001 — straight NS** — `theaters/western_europe/terrain/route_sunken_lane_ns_v01.png`
  Prompt: ROUTE TILE Western Europe sunken muddy lane with North and South sockets only, raised damp grassy banks, restrained wheel ruts, overcast light.
- **WE-ROUTE-002 — straight EW** — `theaters/western_europe/terrain/route_sunken_lane_ew_v01.png`
  Prompt: ROUTE TILE exact WE-ROUTE-001 materials with East and West sockets only, straight lane and matching raised banks.
- **WE-ROUTE-003 — corner NE** — `theaters/western_europe/terrain/route_sunken_lane_ne_v01.png`
  Prompt: ROUTE TILE exact WE-ROUTE-001 materials with North and East sockets only, broad readable quarter-turn.
- **WE-ROUTE-004 — corner ES** — `theaters/western_europe/terrain/route_sunken_lane_es_v01.png`
  Prompt: ROUTE TILE exact WE-ROUTE-001 materials with East and South sockets only, broad readable quarter-turn.
- **WE-ROUTE-005 — corner SW** — `theaters/western_europe/terrain/route_sunken_lane_sw_v01.png`
  Prompt: ROUTE TILE exact WE-ROUTE-001 materials with South and West sockets only, broad readable quarter-turn.
- **WE-ROUTE-006 — corner WN** — `theaters/western_europe/terrain/route_sunken_lane_wn_v01.png`
  Prompt: ROUTE TILE exact WE-ROUTE-001 materials with West and North sockets only, broad readable quarter-turn.
- **WE-ROUTE-007 — T NES** — `theaters/western_europe/terrain/route_sunken_lane_t_nes_v01.png`
  Prompt: ROUTE TILE exact WE-ROUTE-001 materials with North, East, and South sockets only, spacious T-junction and no West opening.
- **WE-ROUTE-008 — T ESW** — `theaters/western_europe/terrain/route_sunken_lane_t_esw_v01.png`
  Prompt: ROUTE TILE exact WE-ROUTE-001 materials with East, South, and West sockets only, spacious T-junction and no North opening.
- **WE-ROUTE-009 — cross NESW** — `theaters/western_europe/terrain/route_sunken_lane_cross_v01.png`
  Prompt: ROUTE TILE exact WE-ROUTE-001 materials with all four sockets, broad four-way crossroads and clean central sightline.
- **WE-ROUTE-010 — North entry** — `theaters/western_europe/terrain/route_sunken_lane_entry_n_v01.png`
  Prompt: ROUTE TILE exact WE-ROUTE-001 materials with North socket only, lane ending in a compact cleared-earth turnaround before tile center.

## Mediterranean dusty-road route set

- **MED-ROUTE-001 — straight NS** — `theaters/mediterranean/terrain/route_dusty_road_ns_v01.png`
  Prompt: ROUTE TILE Mediterranean dusty ochre road with North and South sockets only, pale limestone shoulders, sparse dry grass, warm controlled light.
- **MED-ROUTE-002 — straight EW** — `theaters/mediterranean/terrain/route_dusty_road_ew_v01.png`
  Prompt: ROUTE TILE exact MED-ROUTE-001 materials with East and West sockets only, straight road and matching shoulders.
- **MED-ROUTE-003 — corner NE** — `theaters/mediterranean/terrain/route_dusty_road_ne_v01.png`
  Prompt: ROUTE TILE exact MED-ROUTE-001 materials with North and East sockets only, broad readable quarter-turn.
- **MED-ROUTE-004 — corner ES** — `theaters/mediterranean/terrain/route_dusty_road_es_v01.png`
  Prompt: ROUTE TILE exact MED-ROUTE-001 materials with East and South sockets only, broad readable quarter-turn.
- **MED-ROUTE-005 — corner SW** — `theaters/mediterranean/terrain/route_dusty_road_sw_v01.png`
  Prompt: ROUTE TILE exact MED-ROUTE-001 materials with South and West sockets only, broad readable quarter-turn.
- **MED-ROUTE-006 — corner WN** — `theaters/mediterranean/terrain/route_dusty_road_wn_v01.png`
  Prompt: ROUTE TILE exact MED-ROUTE-001 materials with West and North sockets only, broad readable quarter-turn.
- **MED-ROUTE-007 — T NES** — `theaters/mediterranean/terrain/route_dusty_road_t_nes_v01.png`
  Prompt: ROUTE TILE exact MED-ROUTE-001 materials with North, East, and South sockets only, spacious T-junction and no West opening.
- **MED-ROUTE-008 — T ESW** — `theaters/mediterranean/terrain/route_dusty_road_t_esw_v01.png`
  Prompt: ROUTE TILE exact MED-ROUTE-001 materials with East, South, and West sockets only, spacious T-junction and no North opening.
- **MED-ROUTE-009 — cross NESW** — `theaters/mediterranean/terrain/route_dusty_road_cross_v01.png`
  Prompt: ROUTE TILE exact MED-ROUTE-001 materials with all four sockets, broad crossroads and clean central sightline.
- **MED-ROUTE-010 — North entry** — `theaters/mediterranean/terrain/route_dusty_road_entry_n_v01.png`
  Prompt: ROUTE TILE exact MED-ROUTE-001 materials with North socket only, road ending in a compact gravel turnaround before tile center.

## Eastern Europe packed-snow-road route set

- **EE-ROUTE-001 — straight NS** — `theaters/eastern_europe/terrain/route_packed_snow_ns_v01.png`
  Prompt: ROUTE TILE Eastern Europe packed snow road with North and South sockets only, dark mud wheel tracks, birch-leaf shoulders, cold overcast light.
- **EE-ROUTE-002 — straight EW** — `theaters/eastern_europe/terrain/route_packed_snow_ew_v01.png`
  Prompt: ROUTE TILE exact EE-ROUTE-001 materials with East and West sockets only, straight road and matching shoulders.
- **EE-ROUTE-003 — corner NE** — `theaters/eastern_europe/terrain/route_packed_snow_ne_v01.png`
  Prompt: ROUTE TILE exact EE-ROUTE-001 materials with North and East sockets only, broad readable quarter-turn.
- **EE-ROUTE-004 — corner ES** — `theaters/eastern_europe/terrain/route_packed_snow_es_v01.png`
  Prompt: ROUTE TILE exact EE-ROUTE-001 materials with East and South sockets only, broad readable quarter-turn.
- **EE-ROUTE-005 — corner SW** — `theaters/eastern_europe/terrain/route_packed_snow_sw_v01.png`
  Prompt: ROUTE TILE exact EE-ROUTE-001 materials with South and West sockets only, broad readable quarter-turn.
- **EE-ROUTE-006 — corner WN** — `theaters/eastern_europe/terrain/route_packed_snow_wn_v01.png`
  Prompt: ROUTE TILE exact EE-ROUTE-001 materials with West and North sockets only, broad readable quarter-turn.
- **EE-ROUTE-007 — T NES** — `theaters/eastern_europe/terrain/route_packed_snow_t_nes_v01.png`
  Prompt: ROUTE TILE exact EE-ROUTE-001 materials with North, East, and South sockets only, spacious T-junction and no West opening.
- **EE-ROUTE-008 — T ESW** — `theaters/eastern_europe/terrain/route_packed_snow_t_esw_v01.png`
  Prompt: ROUTE TILE exact EE-ROUTE-001 materials with East, South, and West sockets only, spacious T-junction and no North opening.
- **EE-ROUTE-009 — cross NESW** — `theaters/eastern_europe/terrain/route_packed_snow_cross_v01.png`
  Prompt: ROUTE TILE exact EE-ROUTE-001 materials with all four sockets, broad snowy crossroads and clean central sightline.
- **EE-ROUTE-010 — North entry** — `theaters/eastern_europe/terrain/route_packed_snow_entry_n_v01.png`
  Prompt: ROUTE TILE exact EE-ROUTE-001 materials with North socket only, road ending in a compact muddy-snow turnaround before tile center.

## Pacific jungle-mud-track route set

- **PAC-ROUTE-001 — straight NS** — `theaters/pacific/terrain/route_jungle_mud_ns_v01.png`
  Prompt: ROUTE TILE Pacific jungle mud track with North and South sockets only, warm wet mud, shallow puddles, roots and low ferns outside the route.
- **PAC-ROUTE-002 — straight EW** — `theaters/pacific/terrain/route_jungle_mud_ew_v01.png`
  Prompt: ROUTE TILE exact PAC-ROUTE-001 materials with East and West sockets only, straight track and matching shoulders.
- **PAC-ROUTE-003 — corner NE** — `theaters/pacific/terrain/route_jungle_mud_ne_v01.png`
  Prompt: ROUTE TILE exact PAC-ROUTE-001 materials with North and East sockets only, broad readable quarter-turn.
- **PAC-ROUTE-004 — corner ES** — `theaters/pacific/terrain/route_jungle_mud_es_v01.png`
  Prompt: ROUTE TILE exact PAC-ROUTE-001 materials with East and South sockets only, broad readable quarter-turn.
- **PAC-ROUTE-005 — corner SW** — `theaters/pacific/terrain/route_jungle_mud_sw_v01.png`
  Prompt: ROUTE TILE exact PAC-ROUTE-001 materials with South and West sockets only, broad readable quarter-turn.
- **PAC-ROUTE-006 — corner WN** — `theaters/pacific/terrain/route_jungle_mud_wn_v01.png`
  Prompt: ROUTE TILE exact PAC-ROUTE-001 materials with West and North sockets only, broad readable quarter-turn.
- **PAC-ROUTE-007 — T NES** — `theaters/pacific/terrain/route_jungle_mud_t_nes_v01.png`
  Prompt: ROUTE TILE exact PAC-ROUTE-001 materials with North, East, and South sockets only, spacious T-junction and no West opening.
- **PAC-ROUTE-008 — T ESW** — `theaters/pacific/terrain/route_jungle_mud_t_esw_v01.png`
  Prompt: ROUTE TILE exact PAC-ROUTE-001 materials with East, South, and West sockets only, spacious T-junction and no North opening.
- **PAC-ROUTE-009 — cross NESW** — `theaters/pacific/terrain/route_jungle_mud_cross_v01.png`
  Prompt: ROUTE TILE exact PAC-ROUTE-001 materials with all four sockets, broad jungle crossroads and clean central sightline.
- **PAC-ROUTE-010 — North entry** — `theaters/pacific/terrain/route_jungle_mud_entry_n_v01.png`
  Prompt: ROUTE TILE exact PAC-ROUTE-001 materials with North socket only, track ending in a compact muddy turnaround before tile center.

---

# Western Europe kit

Use ART-ENV-001 through ART-ENV-005 as visual references. Palette: rich
greens, ochre fields, damp earth, limestone grey, muted red-brown roofs,
soft overcast lighting. Terrain outputs are TILE images. Vegetation, structures,
props, clusters, and decals follow the Cutout contract.

## Western Europe terrain and boundaries

- **WE-TERR-001 — patchwork field** — `theaters/western_europe/terrain/ground_patchwork_field_v01.png`
  Prompt: TILE irregular patchwork farmland with two broad crop-value masses and subtle plow direction, no roads, hedges, buildings, or edge openings.
- **WE-TERR-002 — rich grass** — `theaters/western_europe/terrain/ground_rich_grass_v01.png`
  Prompt: TILE rich bocage grass with broad green value variation, sparse tiny weeds, no paths or tall plants, seamless on all edges.
- **WE-TERR-003 — damp earth** — `theaters/western_europe/terrain/ground_damp_earth_v01.png`
  Prompt: TILE compact damp brown earth with broad darker moisture patches and restrained stones, no ruts or route openings.
- **WE-TERR-004 — mud** — `theaters/western_europe/terrain/ground_mud_v01.png`
  Prompt: TILE soft Western Europe mud with shallow puddle accents and broad churned texture, no tire tracks, footprints, or paths.
- **WE-TERR-005 — gravel** — `theaters/western_europe/terrain/ground_gravel_v01.png`
  Prompt: TILE compact grey-brown farm gravel with broad tonal grouping and sparse limestone pieces, no road boundary.
- **WE-TERR-006 — limestone ground** — `theaters/western_europe/terrain/ground_limestone_v01.png`
  Prompt: TILE pale limestone earth with worn patches, moss-darkened cracks and restrained rubble, seamless edges, no structures.
- **WE-TERR-007 — drainage ditch NS** — `theaters/western_europe/terrain/drainage_ditch_ns_v01.png`
  Prompt: TILE shallow narrow drainage ditch running North to South at tile center, damp banks and sparse reeds, no road sockets.
- **WE-TERR-008 — orchard floor** — `theaters/western_europe/terrain/ground_orchard_floor_v01.png`
  Prompt: TILE subdued orchard floor with short grass, leaf litter and broad soft tree-shadow patches only, no visible trees or path.
- **WE-TERR-009 — stone wall EW** — `theaters/western_europe/terrain/stone_wall_ew_v01.png`
  Prompt: CUTOUT long low dry-stone wall segment aligned East–West, square matching ends, moss and limestone, transparent background.
- **WE-TERR-010 — stone wall corner** — `theaters/western_europe/terrain/stone_wall_corner_ne_v01.png`
  Prompt: CUTOUT matching WE-TERR-009 dry-stone wall turning from North to East, same height, thickness, materials, and end sockets.
- **WE-TERR-011 — hedgerow straight** — `theaters/western_europe/terrain/hedgerow_straight_ew_v01.png`
  Prompt: CUTOUT sculpted bocage hedgerow on an earthen bank, East–West, broad irregular foliage masses, square matching ends, no black backdrop.
- **WE-TERR-012 — hedgerow corner** — `theaters/western_europe/terrain/hedgerow_corner_ne_v01.png`
  Prompt: CUTOUT matching WE-TERR-011 hedgerow turning North to East, continuous bank and foliage, same end sockets.
- **WE-TERR-013 — road shoulder left** — `theaters/western_europe/terrain/road_shoulder_left_v01.png`
  Prompt: CUTOUT long Western Europe muddy-road left shoulder strip with damp grass and shallow bank, transparent beyond the edge.
- **WE-TERR-014 — road shoulder right** — `theaters/western_europe/terrain/road_shoulder_right_v01.png`
  Prompt: CUTOUT mirrored companion to WE-TERR-013, identical width, materials and lighting, right-side road shoulder.

## Western Europe vegetation

Each output is a transparent 512×512 cutout. Keep shadows local and foliage
silhouettes clearly unlike infantry.

- **WE-VEG-001 — orchard tree 01** — `theaters/western_europe/vegetation/tree_orchard_apple_v01.png`
  Prompt: CUTOUT compact apple orchard tree, irregular rounded canopy in five broad masses, visible short trunk, sparse muted fruit accents.
- **WE-VEG-002 — orchard tree 02** — `theaters/western_europe/vegetation/tree_orchard_pear_v01.png`
  Prompt: CUTOUT pear orchard tree matching WE-VEG-001 style, taller asymmetrical canopy, five broad masses, visible trunk.
- **WE-VEG-003 — orchard tree 03** — `theaters/western_europe/vegetation/tree_orchard_old_v01.png`
  Prompt: CUTOUT old wind-shaped orchard tree, low split trunk and sparse asymmetrical canopy, same palette and scale as WE-VEG-001.
- **WE-VEG-004 — broadleaf tree 01** — `theaters/western_europe/vegetation/tree_broadleaf_oak_v01.png`
  Prompt: CUTOUT mature oak, broad irregular canopy with six major foliage masses, sturdy visible trunk and soft contained shadow.
- **WE-VEG-005 — broadleaf tree 02** — `theaters/western_europe/vegetation/tree_broadleaf_elm_v01.png`
  Prompt: CUTOUT mature elm matching WE-VEG-004 scale, elongated asymmetrical canopy, restrained internal leaf texture.
- **WE-VEG-006 — hedge mass** — `theaters/western_europe/vegetation/hedge_mass_v01.png`
  Prompt: CUTOUT dense low hedge mass, long irregular silhouette, three broad green values, no earthen wall or flowers dominating.
- **WE-VEG-007 — bush green** — `theaters/western_europe/vegetation/bush_green_v01.png`
  Prompt: CUTOUT low irregular leafy bush, broad medium-green masses, no round lollipop shape.
- **WE-VEG-008 — bush flowering** — `theaters/western_europe/vegetation/bush_white_flower_v01.png`
  Prompt: CUTOUT low bocage bush with sparse tiny white flower accents, dark outer silhouette and restrained detail.
- **WE-VEG-009 — bush bramble** — `theaters/western_europe/vegetation/bush_bramble_v01.png`
  Prompt: CUTOUT tangled low bramble bush, angular irregular spread, dark green-brown palette, no human-like silhouette.
- **WE-VEG-010 — tall grass clump** — `theaters/western_europe/vegetation/grass_tall_clump_v01.png`
  Prompt: CUTOUT broad fan-shaped tall grass clump, muted rich green, readable as one mass at 25% scale.
- **WE-VEG-011 — wheat clump** — `theaters/western_europe/vegetation/plant_wheat_clump_v01.png`
  Prompt: CUTOUT compact ochre wheat clump, broad grouped stalk masses, no individual fine-line overload.
- **WE-VEG-012 — weeds** — `theaters/western_europe/vegetation/plant_weeds_v01.png`
  Prompt: CUTOUT low mixed weed patch with broad leaves and two heights, subdued green, no flowers forming visual noise.
- **WE-VEG-013 — fallen leaves** — `theaters/western_europe/vegetation/plant_fallen_leaves_v01.png`
  Prompt: DECAL low irregular fallen-leaf patch in muted green, brown and ochre, transparent feathered edge.
- **WE-VEG-014 — field-edge plants** — `theaters/western_europe/vegetation/plant_field_edge_v01.png`
  Prompt: CUTOUT horizontal field-edge plant strip combining low grass, weeds and a few broad leaves, square matching ends.

## Western Europe architecture

Each output is a transparent 1024×1024 structure cutout with a readable roof
silhouette and no cropped walls or off-object shadow.

- **WE-ARCH-001 — Norman farmhouse intact** — `theaters/western_europe/architecture/hero_norman_farmhouse_intact_v01.png`
  Prompt: STRUCTURE intact Norman limestone farmhouse, broad red-brown tiled roof, courtyard wing, chunky chimney and restrained shutters, no people.
- **WE-ARCH-002 — Norman farmhouse damaged** — `theaters/western_europe/architecture/hero_norman_farmhouse_damaged_v01.png`
  Prompt: STRUCTURE exact WE-ARCH-001 farmhouse with limited roof holes, exposed rafters and small rubble, same footprint and angle.
- **WE-ARCH-003 — ruined church** — `theaters/western_europe/architecture/hero_ruined_church_v01.png`
  Prompt: STRUCTURE small limestone village church ruin, broken nave roof and square bell tower, broad rubble masses, no graves or religious text.
- **WE-ARCH-004 — village facade** — `theaters/western_europe/architecture/structure_village_facade_v01.png`
  Prompt: STRUCTURE compressed row of two attached limestone village facades with muted shutters and broad roof forms, isolated footprint.
- **WE-ARCH-005 — stone bridge** — `theaters/western_europe/architecture/structure_stone_bridge_v01.png`
  Prompt: STRUCTURE short limestone arch bridge, straight 256 px route deck, low parapets, moss-darkened edges, transparent surroundings.
- **WE-ARCH-006 — barn** — `theaters/western_europe/architecture/structure_barn_v01.png`
  Prompt: STRUCTURE broad timber-and-stone barn, large roof silhouette, one prominent door shape, weathered red-brown and grey palette.
- **WE-ARCH-007 — farm shed** — `theaters/western_europe/architecture/structure_farm_shed_v01.png`
  Prompt: STRUCTURE small rough timber farm shed with lean-to roof and two large material masses, no loose props outside footprint.
- **WE-ARCH-008 — windmill** — `theaters/western_europe/architecture/hero_windmill_v01.png`
  Prompt: STRUCTURE compact stone windmill with broad four-blade silhouette laid visibly over the roof plane, no motion blur or text.
- **WE-ARCH-009 — drainage culvert** — `theaters/western_europe/architecture/structure_drainage_culvert_v01.png`
  Prompt: STRUCTURE low limestone drainage culvert with short bridge deck and dark water opening, route clearance obvious from above.

## Western Europe individual props

Each output is a transparent 512×512 PROP cutout.

- **WE-PROP-001 — wooden fence** — `theaters/western_europe/flavor/prop_farm_fence_v01.png`
  Prompt: PROP short weathered wooden post-and-rail fence segment with square matching ends, no surrounding terrain.
- **WE-PROP-002 — farm gate** — `theaters/western_europe/flavor/prop_farm_gate_v01.png`
  Prompt: PROP chunky wooden farm gate between two short posts, closed position, strong diagonal brace silhouette.
- **WE-PROP-003 — cart** — `theaters/western_europe/flavor/prop_farm_cart_v01.png`
  Prompt: PROP empty two-wheel wooden farm cart, broad wheels and handles, no horse or people.
- **WE-PROP-004 — hay bale** — `theaters/western_europe/flavor/prop_farm_hay_v01.png`
  Prompt: PROP compact bound hay bale, broad ochre straw masses and two visible rope bands.
- **WE-PROP-005 — trough** — `theaters/western_europe/flavor/prop_farm_trough_v01.png`
  Prompt: PROP low stone livestock trough with dark water center, no animals or surrounding ground.
- **WE-PROP-006 — upright barrel** — `theaters/western_europe/flavor/prop_farm_barrel_upright_v01.png`
  Prompt: PROP upright weathered wooden barrel with thick hoops, oversized readable construction.
- **WE-PROP-007 — farm tools** — `theaters/western_europe/flavor/prop_farm_tools_v01.png`
  Prompt: PROP small grouped rake, shovel and pitchfork laid flat, broad shapes, no weapon-like presentation.
- **WE-PROP-008 — road sign** — `theaters/western_europe/flavor/prop_road_sign_v01.png`
  Prompt: PROP blank weathered wooden direction sign with two arrow boards, no writing, symbols, or place names.
- **WE-PROP-009 — milestone** — `theaters/western_europe/flavor/prop_road_milestone_v01.png`
  Prompt: PROP squat pale limestone roadside milestone with blank worn face, no text or numbers.
- **WE-PROP-010 — utility pole** — `theaters/western_europe/flavor/prop_road_utility_pole_v01.png`
  Prompt: PROP short top-down utility pole with simple crossbar and two visible wires contained within frame, no signage.
- **WE-PROP-011 — broken fence** — `theaters/western_europe/flavor/prop_road_broken_fence_v01.png`
  Prompt: PROP collapsed short wooden fence section with two broken rails, low readable silhouette.
- **WE-PROP-012 — road debris** — `theaters/western_europe/flavor/prop_road_debris_v01.png`
  Prompt: PROP low cluster of boards, stones and one bent metal strip, no body parts or recognizable vehicle identity.
- **WE-PROP-013 — supply crate** — `theaters/western_europe/flavor/prop_supply_crate_v01.png`
  Prompt: PROP chunky closed wooden supply crate with diagonal brace, no writing, insignia, or ammunition visible.
- **WE-PROP-014 — supply barrel** — `theaters/western_europe/flavor/prop_supply_barrel_v01.png`
  Prompt: PROP dark painted steel supply barrel with two broad ribs, no markings or fuel symbol.
- **WE-PROP-015 — folded tarp** — `theaters/western_europe/flavor/prop_supply_tarp_v01.png`
  Prompt: PROP folded muted olive canvas tarp with broad creases and two visible tie straps.
- **WE-PROP-016 — pallet** — `theaters/western_europe/flavor/prop_supply_pallet_v01.png`
  Prompt: PROP low empty wooden pallet, exaggerated slat spacing for top-down readability.
- **WE-PROP-017 — ammunition box** — `theaters/western_europe/flavor/prop_supply_ammunition_box_v01.png`
  Prompt: PROP closed neutral military ammunition box with chunky handles, no text, caliber marks, or insignia.
- **WE-PROP-018 — handcart** — `theaters/western_europe/flavor/prop_supply_handcart_v01.png`
  Prompt: PROP small empty two-wheel handcart, broad wheel and handle shapes, no person.
- **WE-PROP-019 — sandbag wall** — `theaters/western_europe/flavor/prop_defensive_sandbags_v01.png`
  Prompt: PROP short curved sandbag wall, two chunky layers, warm khaki canvas, no weapon.
- **WE-PROP-020 — stakes** — `theaters/western_europe/flavor/prop_defensive_stakes_v01.png`
  Prompt: PROP compact group of three low timber defensive stakes, blunt ends, no wire or gore.
- **WE-PROP-021 — camouflage net** — `theaters/western_europe/flavor/prop_defensive_camouflage_net_v01.png`
  Prompt: PROP folded low camouflage net over two short supports, broad green-brown patches, no hidden equipment.
- **WE-PROP-022 — field radio** — `theaters/western_europe/flavor/prop_defensive_field_radio_v01.png`
  Prompt: PROP compact neutral field radio box with oversized dial shapes, handset and prominent antenna, no markings.
- **WE-PROP-023 — barrier** — `theaters/western_europe/flavor/prop_defensive_barrier_v01.png`
  Prompt: PROP low timber-and-sandbag road barrier, broad cross-braced silhouette, no sign or text.
- **WE-PROP-024 — crater** — `theaters/western_europe/flavor/prop_damage_crater_v01.png`
  Prompt: DECAL shallow damp-earth crater with grass-darkened rim, no smoke or remains.
- **WE-PROP-025 — rubble** — `theaters/western_europe/flavor/prop_damage_rubble_v01.png`
  Prompt: PROP low limestone-and-roof-tile rubble patch, broad pieces and transparent gaps.
- **WE-PROP-026 — scorch** — `theaters/western_europe/flavor/prop_damage_scorch_v01.png`
  Prompt: DECAL irregular black-brown scorch patch on damp earth, feathered transparent edge, no active fire.
- **WE-PROP-027 — broken timber** — `theaters/western_europe/flavor/prop_damage_broken_timber_v01.png`
  Prompt: PROP low fan of splintered dark timber from a farm structure, no sharp weapon-like arrangement.
- **WE-PROP-028 — wreck fragment** — `theaters/western_europe/flavor/prop_damage_wreck_fragment_v01.png`
  Prompt: PROP anonymous bent steel panel and small wheel fragment, no insignia or identifiable national vehicle.

## Western Europe authored clusters

Each output is a transparent 1024×1024 composition with clear negative space
around the cluster and no people.

- **WE-CLUSTER-001 — supply dump** — `theaters/western_europe/clusters/cluster_supply_dump_v01.png`
  Prompt: CUTOUT authored supply dump of crates, two barrels, folded tarp, pallet and handcart, compact triangular composition.
- **WE-CLUSTER-002 — abandoned position** — `theaters/western_europe/clusters/cluster_abandoned_position_v01.png`
  Prompt: CUTOUT abandoned field position with low sandbags, closed ammunition box, field radio and scattered neutral gear, no helmet or body.
- **WE-CLUSTER-003 — farmyard** — `theaters/western_europe/clusters/cluster_farmyard_v01.png`
  Prompt: CUTOUT farmyard cluster with cart, barrels, short fence, hay and tools, balanced L-shaped composition.
- **WE-CLUSTER-004 — road repair** — `theaters/western_europe/clusters/cluster_road_repair_v01.png`
  Prompt: CUTOUT road-repair cluster with gravel pile, shovel, boards, blank barrier and handcart, route-facing composition.
- **WE-CLUSTER-005 — field kitchen** — `theaters/western_europe/clusters/cluster_field_kitchen_v01.png`
  Prompt: CUTOUT unoccupied field-kitchen cluster with covered stove trailer, stacked pots, crates and folded canvas, no food text or people.
- **WE-CLUSTER-006 — hedgerow defensive position** — `theaters/western_europe/clusters/cluster_hedgerow_position_v01.png`
  Prompt: CUTOUT hedgerow-edge position with sandbags, camouflage net, radio and ammunition boxes, no weapon or soldiers.
- **WE-CLUSTER-007 — damaged farmhouse edge** — `theaters/western_europe/clusters/cluster_damaged_farmhouse_edge_v01.png`
  Prompt: CUTOUT environmental cluster of low limestone rubble, broken roof timber, shattered gate and scorch patch, non-graphic.
- **WE-CLUSTER-008 — small supply cache** — `theaters/western_europe/clusters/cluster_supply_cache_small_v01.png`
  Prompt: CUTOUT small alternate supply cache with three crates, one barrel and tied canvas, narrow composition.
- **WE-CLUSTER-009 — orchard worksite** — `theaters/western_europe/clusters/cluster_orchard_worksite_v01.png`
  Prompt: CUTOUT orchard worksite with baskets, ladder laid flat, hand tools and short fence, no people or fruit labels.
- **WE-CLUSTER-010 — culvert maintenance** — `theaters/western_europe/clusters/cluster_culvert_maintenance_v01.png`
  Prompt: CUTOUT drainage-maintenance cluster with boards, gravel, tools and blank barrier, low linear composition.
- **WE-CLUSTER-011 — abandoned checkpoint** — `theaters/western_europe/clusters/cluster_abandoned_checkpoint_v01.png`
  Prompt: CUTOUT empty roadside checkpoint with sandbags, blank barrier, radio crate and two supply boxes, no flag.
- **WE-CLUSTER-012 — field storage** — `theaters/western_europe/clusters/cluster_field_storage_v01.png`
  Prompt: CUTOUT covered farm storage with hay, tarp, barrels and pallet beside a short stone wall, compact composition.

## Western Europe decals

Each output is a transparent 512×512 DECAL.

- **WE-DECAL-001 — field wear** — `theaters/western_europe/decals/decal_field_wear_v01.png`
  Prompt: DECAL broad irregular flattened-grass wear patch with subtle damp earth showing through.
- **WE-DECAL-002 — hedgerow shadow** — `theaters/western_europe/decals/decal_hedgerow_shadow_v01.png`
  Prompt: DECAL soft irregular cool-green shadow strip for a hedgerow edge, transparent feathered boundary.
- **WE-DECAL-003 — road mud** — `theaters/western_europe/decals/decal_road_mud_v01.png`
  Prompt: DECAL muddy road smear with restrained parallel wheel wear, no fixed route edge.
- **WE-DECAL-004 — orchard marks** — `theaters/western_europe/decals/decal_orchard_marks_v01.png`
  Prompt: DECAL sparse fallen leaves, low grass wear and small fruit-darkened soil marks, no visible tree.
- **WE-DECAL-005 — drainage dampness** — `theaters/western_europe/decals/decal_drainage_damp_v01.png`
  Prompt: DECAL narrow damp-earth strip with moss and tiny puddle highlights for drainage edges.
- **WE-DECAL-006 — shell damage** — `theaters/western_europe/decals/decal_shell_damage_v01.png`
  Prompt: DECAL shallow crater fragments and churned dark earth, non-graphic, no smoke.
- **WE-DECAL-007 — limestone rubble** — `theaters/western_europe/decals/decal_limestone_rubble_v01.png`
  Prompt: DECAL scattered low limestone chips and two broad fragments, transparent gaps.
- **WE-DECAL-008 — scorch** — `theaters/western_europe/decals/decal_scorch_v01.png`
  Prompt: DECAL irregular soot and heat-darkened grass, no active flame.
- **WE-DECAL-009 — broken timber** — `theaters/western_europe/decals/decal_broken_timber_v01.png`
  Prompt: DECAL flat scattered roof and fence timber pieces, low silhouette.
- **WE-DECAL-010 — wreck fragment** — `theaters/western_europe/decals/decal_wreck_fragment_v01.png`
  Prompt: DECAL anonymous bent metal scraps with restrained rust, no national identity.
- **WE-DECAL-011 — tire ruts** — `theaters/western_europe/decals/decal_tire_ruts_v01.png`
  Prompt: DECAL short twin tire ruts in damp soil, repeatable ends, low contrast.
- **WE-DECAL-012 — boot trail** — `theaters/western_europe/decals/decal_boot_trail_v01.png`
  Prompt: DECAL short abstract boot-print trail through mud, no person or blood.
- **WE-DECAL-013 — puddles** — `theaters/western_europe/decals/decal_puddles_v01.png`
  Prompt: DECAL three shallow irregular puddles reflecting soft overcast sky, transparent surroundings.
- **WE-DECAL-014 — leaf scatter** — `theaters/western_europe/decals/decal_leaf_scatter_v01.png`
  Prompt: DECAL sparse wind-scattered green-brown leaves, restrained detail.
- **WE-DECAL-015 — wheat scatter** — `theaters/western_europe/decals/decal_wheat_scatter_v01.png`
  Prompt: DECAL low loose ochre wheat stems and chaff, no bundle or container.
- **WE-DECAL-016 — stone scatter** — `theaters/western_europe/decals/decal_stone_scatter_v01.png`
  Prompt: DECAL sparse flat limestone pebbles in three sizes, subtle shadows.
- **WE-DECAL-017 — moss patch** — `theaters/western_europe/decals/decal_moss_patch_v01.png`
  Prompt: DECAL irregular dark-green moss patch for stone or damp soil, transparent feathered edge.
- **WE-DECAL-018 — roof-tile fragments** — `theaters/western_europe/decals/decal_roof_tile_fragments_v01.png`
  Prompt: DECAL small muted red-brown broken roof tiles, broad shapes, no building.
- **WE-DECAL-019 — cart tracks** — `theaters/western_europe/decals/decal_cart_tracks_v01.png`
  Prompt: DECAL narrow paired wooden-wheel tracks with one shallow central hoof-free wear strip.
- **WE-DECAL-020 — grass edge** — `theaters/western_europe/decals/decal_grass_edge_v01.png`
  Prompt: DECAL irregular low grass border for blending dirt to field, square repeatable ends.

---

# Mediterranean / North Africa production kit

Use ART-ENV-006 as the palette and brushwork reference: sun-baked ochre,
pale limestone, dusty olive green, faded canvas, turquoise coastal accents,
and hard warm light with restrained shadows.

## Mediterranean terrain and boundaries

- **MED-TERR-001 — warm sand** — `theaters/mediterranean/terrain/ground_warm_sand_v01.png`
  Prompt: TILE seamless warm golden-beige sand, broad painted value variation, sparse tiny pebbles, no dunes, tracks, props, or route sockets.
- **MED-TERR-002 — dusty ochre earth** — `theaters/mediterranean/terrain/ground_dusty_ochre_v01.png`
  Prompt: TILE seamless compact ochre earth with broad dry brush texture and subtle pale dust blooms, no path or objects.
- **MED-TERR-003 — pale limestone ground** — `theaters/mediterranean/terrain/ground_pale_limestone_v01.png`
  Prompt: TILE seamless pale cream limestone ground, large irregular slabs and worn seams, low contrast, no constructed paving pattern.
- **MED-TERR-004 — dry gravel** — `theaters/mediterranean/terrain/ground_dry_gravel_v01.png`
  Prompt: TILE seamless warm grey-tan gravel, compressed detail with a few broad stones and quiet dust-filled gaps.
- **MED-TERR-005 — rocky outcrop** — `theaters/mediterranean/terrain/outcrop_limestone_v01.png`
  Prompt: CUTOUT broad low limestone outcrop, three readable stepped rock masses, sun-bleached top planes, compact gameplay footprint.
- **MED-TERR-006 — rocky wadi bed** — `theaters/mediterranean/terrain/ground_rocky_wadi_v01.png`
  Prompt: TILE alternate production pass matching ART-ENV-006, dry winding wadi bed contained inside the tile, pale stones and dusty channels, no route sockets or standing water.
- **MED-TERR-007 — dry grass ground** — `theaters/mediterranean/terrain/ground_dry_grass_v01.png`
  Prompt: TILE seamless straw-gold dry grass over warm soil, broad sparse clumps, readable calm gameplay field, no route.
- **MED-TERR-008 — olive-grove soil** — `theaters/mediterranean/terrain/ground_olive_grove_soil_v01.png`
  Prompt: TILE seamless reddish-tan cultivated soil with soft leaf litter and faint irregular tilling, no trees or straight rows.
- **MED-TERR-009 — vineyard rows** — `theaters/mediterranean/terrain/ground_vineyard_rows_v01.png`
  Prompt: TILE cultivated vineyard-ground pattern with four broad parallel soil rows running North–South, no plants, posts, route, or edge openings.
- **MED-TERR-010 — coastal sand** — `theaters/mediterranean/terrain/ground_coastal_sand_v01.png`
  Prompt: TILE seamless pale coastal sand with faint wind combing, occasional coral-white pebble, no water, footprints, or objects.
- **MED-TERR-011 — shallow-water edge** — `theaters/mediterranean/terrain/water_turquoise_shallow_edge_v01.png`
  Prompt: TILE coastline transition with land on West and clear turquoise shallow water on East, shoreline centered vertically, gentle pale foam, edge-continuous North and South, no route.
- **MED-TERR-012 — stone village wall** — `theaters/mediterranean/terrain/wall_stone_village_segment_v01.png`
  Prompt: CUTOUT straight low dry-stone village wall segment, pale limestone blocks, square repeatable ends, no gate or vegetation.
- **MED-TERR-013 — stone wall corner** — `theaters/mediterranean/terrain/wall_stone_village_corner_v01.png`
  Prompt: CUTOUT ninety-degree corner matching MED-TERR-012 exactly in wall width, stone scale, lighting, and endpoint shape.
- **MED-TERR-014 — wadi bank** — `theaters/mediterranean/terrain/boundary_wadi_bank_v01.png`
  Prompt: CUTOUT long irregular dry wadi-bank boundary, eroded ochre edge with pale embedded stones, square repeatable ends, low silhouette.

## Mediterranean vegetation

- **MED-VEG-001 — mature olive tree** — `theaters/mediterranean/vegetation/tree_olive_mature_v01.png`
  Prompt: CUTOUT mature olive tree, twisted broad trunk, rounded dusty silver-green crown in three bold leaf masses, compact shadow.
- **MED-VEG-002 — young olive tree** — `theaters/mediterranean/vegetation/tree_olive_young_v01.png`
  Prompt: CUTOUT young olive tree matching MED-VEG-001, narrow trunk and smaller asymmetric silver-green crown.
- **MED-VEG-003 — umbrella pine** — `theaters/mediterranean/vegetation/tree_umbrella_pine_v01.png`
  Prompt: CUTOUT Mediterranean umbrella pine, tall warm trunk, broad flattened dark-green canopy, unmistakable top-down silhouette.
- **MED-VEG-004 — date palm** — `theaters/mediterranean/vegetation/tree_date_palm_v01.png`
  Prompt: CUTOUT date palm, radial crown of eight broad painted fronds, warm textured trunk visible at center, no fruit detail.
- **MED-VEG-005 — dry scrub cluster** — `theaters/mediterranean/vegetation/cluster_dry_scrub_v01.png`
  Prompt: CUTOUT low dry scrub cluster, dusty sage and tan masses with irregular negative space, no human-like vertical silhouettes.
- **MED-VEG-006 — thorny brush** — `theaters/mediterranean/vegetation/bush_thorny_v01.png`
  Prompt: CUTOUT compact thorny brush, dark olive outer contour and ochre dead interior twigs, broad safe silhouette, no literal sharp detail.
- **MED-VEG-007 — vineyard foliage row** — `theaters/mediterranean/vegetation/row_vineyard_foliage_v01.png`
  Prompt: CUTOUT straight vineyard foliage row, low muted green vine masses on restrained wooden stakes, square repeatable ends.
- **MED-VEG-008 — reed clump** — `theaters/mediterranean/vegetation/cluster_reeds_v01.png`
  Prompt: CUTOUT waterside reed clump, warm green vertical strokes grouped into three broad masses, low brown base.
- **MED-VEG-009 — sparse grass tuft** — `theaters/mediterranean/vegetation/tuft_sparse_grass_v01.png`
  Prompt: CUTOUT small sparse straw grass tuft with a few dusty green blades, simple silhouette for repeated scatter.
- **MED-VEG-010 — mixed olive scrub mass** — `theaters/mediterranean/vegetation/mass_olive_scrub_v01.png`
  Prompt: CUTOUT wide low vegetation mass combining silver-green olive scrub, dry grass, and two limestone stones, clear irregular boundary.

## Mediterranean architecture

- **MED-ARCH-001 — stucco house** — `theaters/mediterranean/architecture/house_stucco_v01.png`
  Prompt: STRUCTURE compact flat-roofed stucco house, sun-faded cream walls, muted blue-grey shutters, simple rectangular footprint, no signs or flags.
- **MED-ARCH-002 — stone village wall gate** — `theaters/mediterranean/architecture/gate_stone_village_v01.png`
  Prompt: STRUCTURE low pale-stone wall gate with a wide empty central opening and weathered timber doors fixed open, no text or insignia.
- **MED-ARCH-003 — fuel depot** — `theaters/mediterranean/architecture/depot_fuel_v01.png`
  Prompt: STRUCTURE small neutral fuel depot, low corrugated roof, two anonymous cylindrical tanks, dusty concrete apron, no labels or faction markings.
- **MED-ARCH-004 — watch post** — `theaters/mediterranean/architecture/watch_post_v01.png`
  Prompt: STRUCTURE simple timber-and-stone watch post with canvas shade roof and open empty platform, broad readable footprint, no weapons or occupants.
- **MED-ARCH-005 — cistern** — `theaters/mediterranean/architecture/cistern_stone_v01.png`
  Prompt: STRUCTURE round pale-stone cistern with shallow domed cap, small access hatch, strong circular silhouette, no text.
- **MED-ARCH-006 — depot shed** — `theaters/mediterranean/architecture/shed_depot_v01.png`
  Prompt: STRUCTURE low weathered depot shed, cream stucco base, dusty corrugated roof, open dark doorway without visible contents.
- **MED-ARCH-007 — coastal battery ruin** — `theaters/mediterranean/architecture/ruin_coastal_battery_v01.png`
  Prompt: STRUCTURE abandoned low concrete coastal-battery emplacement, empty circular mounting well, chipped sun-bleached edges, no gun or insignia.
- **MED-ARCH-008 — wadi culvert** — `theaters/mediterranean/architecture/culvert_wadi_v01.png`
  Prompt: STRUCTURE short pale-stone culvert bridge spanning a dry channel, route deck North–South and exactly 256 px wide at both sockets, no vehicles.

## Mediterranean flavor props

- **MED-PROP-001 — fuel drums** — `theaters/mediterranean/flavor/prop_fuel_drums_v01.png`
  Prompt: PROP cluster of three dented anonymous fuel drums, faded ochre and blue-grey paint, no labels, symbols, or readable markings.
- **MED-PROP-002 — jerry cans** — `theaters/mediterranean/flavor/prop_jerry_cans_v01.png`
  Prompt: PROP four neutral metal jerry cans in a staggered group, dusty olive and tan, no insignia or lettering.
- **MED-PROP-003 — canvas shade** — `theaters/mediterranean/flavor/prop_canvas_shade_v01.png`
  Prompt: PROP small square faded-tan canvas sunshade on four rough poles, empty beneath, compact contained shadow.
- **MED-PROP-004 — supply crates** — `theaters/mediterranean/flavor/prop_supply_crates_v01.png`
  Prompt: PROP three weathered wooden supply crates, two stacked and one angled, blank sides without stencils.
- **MED-PROP-005 — pallet** — `theaters/mediterranean/flavor/prop_pallet_v01.png`
  Prompt: PROP rough sun-bleached timber pallet, broad slats, empty and fully visible.
- **MED-PROP-006 — handcart** — `theaters/mediterranean/flavor/prop_handcart_v01.png`
  Prompt: PROP empty two-wheel wooden handcart with long handles, warm dusty wear, no cargo.
- **MED-PROP-007 — stone markers** — `theaters/mediterranean/flavor/prop_stone_markers_v01.png`
  Prompt: PROP group of three low unmarked limestone boundary stones, irregular sizes, no carved text or symbols.
- **MED-PROP-008 — rope coil** — `theaters/mediterranean/flavor/prop_rope_coil_v01.png`
  Prompt: PROP thick tan rope in one broad readable coil with a short loose end, no hook.
- **MED-PROP-009 — wire fence segment** — `theaters/mediterranean/flavor/prop_fence_wire_v01.png`
  Prompt: PROP straight low plain-wire fence on rough wooden posts, square repeatable ends, no barbs at sprite scale.
- **MED-PROP-010 — ammunition boxes** — `theaters/mediterranean/flavor/prop_ammunition_boxes_v01.png`
  Prompt: PROP two anonymous closed wooden ammunition boxes with metal corners, no labels, text, or faction markings.
- **MED-PROP-011 — field radio** — `theaters/mediterranean/flavor/prop_field_radio_v01.png`
  Prompt: PROP compact neutral field radio case with handset and short aerial, no frequency text, logos, or insignia.
- **MED-PROP-012 — sandbag corner** — `theaters/mediterranean/flavor/prop_sandbag_corner_v01.png`
  Prompt: PROP low L-shaped corner of dusty tan sandbags, six broad bags, empty center, no weapon.
- **MED-PROP-013 — camouflage net** — `theaters/mediterranean/flavor/prop_camouflage_net_v01.png`
  Prompt: PROP folded and partly spread faded tan-and-olive camouflage net, broad patterned masses, no structure underneath.
- **MED-PROP-014 — broken equipment** — `theaters/mediterranean/flavor/prop_broken_equipment_v01.png`
  Prompt: PROP anonymous broken mechanical equipment fragments, one bent frame and two wheels, no recognizable vehicle, weapon, or insignia.

## Mediterranean clusters and decals

- **MED-CLUSTER-001 — fuel dump** — `theaters/mediterranean/clusters_decals/cluster_fuel_dump_v01.png`
  Prompt: CUTOUT compact fuel-dump dressing cluster with anonymous drums, jerry cans, two blank crates, pallet, and folded tarp; no people or vehicle.
- **MED-CLUSTER-002 — desert supply stop** — `theaters/mediterranean/clusters_decals/cluster_desert_supply_stop_v01.png`
  Prompt: CUTOUT small supply stop with canvas shade, blank crates, water cans, handcart, and open negative center, no faction identity.
- **MED-CLUSTER-003 — wadi crossing** — `theaters/mediterranean/clusters_decals/cluster_wadi_crossing_v01.png`
  Prompt: CUTOUT shallow dry-channel crossing dressing with pale stones, two timber planks, and sparse reeds, no road beyond footprint.
- **MED-CLUSTER-004 — rocky position** — `theaters/mediterranean/clusters_decals/cluster_rocky_position_v01.png`
  Prompt: CUTOUT empty defensive position of low limestone rocks, sandbags, blank ammunition boxes, and camouflage net, no gun or occupants.
- **MED-CLUSTER-005 — village courtyard** — `theaters/mediterranean/clusters_decals/cluster_village_courtyard_v01.png`
  Prompt: CUTOUT courtyard dressing with low stone edge, pottery, rope coil, blank crates, and small canvas shade, no people, food, or text.
- **MED-DECAL-001 — tire tracks** — `theaters/mediterranean/clusters_decals/decal_tire_tracks_v01.png`
  Prompt: DECAL short paired tire tracks pressed into dry ochre dust, repeatable ends, no vehicle.
- **MED-DECAL-002 — sand ripples** — `theaters/mediterranean/clusters_decals/decal_sand_ripples_v01.png`
  Prompt: DECAL three broad wind-made sand ripple bands, low contrast and transparent between strokes.
- **MED-DECAL-003 — dust patch** — `theaters/mediterranean/clusters_decals/decal_dust_patch_v01.png`
  Prompt: DECAL irregular pale dust bloom with soft painted edge, no cloud or raised plume.
- **MED-DECAL-004 — rock scatter** — `theaters/mediterranean/clusters_decals/decal_rock_scatter_v01.png`
  Prompt: DECAL sparse pale limestone pebble scatter in three readable sizes, transparent gaps.
- **MED-DECAL-005 — heat haze** — `theaters/mediterranean/clusters_decals/overlay_heat_haze_v01.png`
  Prompt: transparent full-frame 1024×1024 subtle heat-haze distortion texture, faint horizontal warm shimmer only, no vignette or opaque background.

---

# Eastern Europe production kit

Use ART-ENV-007 as the palette and brushwork reference: muted pine and birch
greens, cold blue-grey snow shadow, dark brown mud, weathered timber, rusted
industrial red, and flat winter light with strong value readability.

## Eastern Europe terrain and boundaries

- **EE-TERR-001 — muted forest floor** — `theaters/eastern_europe/terrain/ground_muted_forest_v01.png`
  Prompt: TILE seamless muted forest floor, dark loam, pine needles, sparse moss, and broad leaf shapes, no trees, route, or objects.
- **EE-TERR-002 — dark mud** — `theaters/eastern_europe/terrain/ground_dark_mud_v01.png`
  Prompt: TILE seamless cold dark-brown mud with broad wet and matte patches, no tracks, puddles, route, or props.
- **EE-TERR-003 — broad field** — `theaters/eastern_europe/terrain/ground_broad_field_v01.png`
  Prompt: TILE seamless muted green-brown field, broad flattened grass strokes and faint uneven cultivation, no straight path.
- **EE-TERR-004 — birch-leaf ground** — `theaters/eastern_europe/terrain/ground_birch_leaf_v01.png`
  Prompt: TILE seamless dark soil with restrained pale birch leaf scatter and sparse cool grass, calm low-contrast field.
- **EE-TERR-005 — fresh snow** — `theaters/eastern_europe/terrain/ground_fresh_snow_v01.png`
  Prompt: TILE seamless fresh snow, broad off-white surface with subtle blue-grey undulations, no tracks, footprints, route, or sparkle noise.
- **EE-TERR-006 — packed snow** — `theaters/eastern_europe/terrain/ground_packed_snow_v01.png`
  Prompt: TILE seamless compressed dirty snow, broad grey-blue pressure bands and thin exposed earth, no directional road or tracks.
- **EE-TERR-007 — frozen water** — `theaters/eastern_europe/terrain/water_frozen_v01.png`
  Prompt: TILE seamless frozen water, cold blue-grey ice with a few broad pale stress lines and cloudy depth, no open cracks or objects.
- **EE-TERR-008 — rail ballast** — `theaters/eastern_europe/terrain/ground_rail_ballast_v01.png`
  Prompt: TILE seamless dark angular rail ballast and cinder ground, broad compressed stone pattern, no rails, sleepers, or route.
- **EE-TERR-009 — industrial ground** — `theaters/eastern_europe/terrain/ground_rusted_industrial_v01.png`
  Prompt: TILE seamless worn industrial yard, cold compact earth, concrete fragments, restrained rust staining and oil-dark patches, no structures.
- **EE-TERR-010 — cold water** — `theaters/eastern_europe/terrain/water_cold_blue_grey_v01.png`
  Prompt: TILE seamless cold blue-grey water with broad slow ripples and dark painted depth, no shoreline, ice, foam, or objects.
- **EE-TERR-011 — rail line segment** — `theaters/eastern_europe/terrain/rail_line_straight_v01.png`
  Prompt: TILE straight railway running North–South, two rusted rails on weathered sleepers and dark ballast, exactly centered, no road sockets or train.
- **EE-TERR-012 — frozen river bank** — `theaters/eastern_europe/terrain/boundary_frozen_river_bank_v01.png`
  Prompt: CUTOUT long irregular frozen river bank, dark soil under a rounded snow lip with sparse reeds, square repeatable ends.
- **EE-TERR-013 — industrial fence** — `theaters/eastern_europe/terrain/boundary_industrial_fence_v01.png`
  Prompt: CUTOUT straight weathered utilitarian metal fence on concrete posts, low rust, square repeatable ends, no signs or barbed detail.

## Eastern Europe vegetation

- **EE-VEG-001 — mature birch** — `theaters/eastern_europe/vegetation/tree_birch_mature_v01.png`
  Prompt: CUTOUT mature birch tree, pale segmented trunks, rounded muted yellow-green crown in three broad masses, strong asymmetrical silhouette.
- **EE-VEG-002 — young birch group** — `theaters/eastern_europe/vegetation/tree_birch_young_group_v01.png`
  Prompt: CUTOUT group of three slim young birches with small separate crowns, clearly vegetation rather than human silhouettes.
- **EE-VEG-003 — spruce** — `theaters/eastern_europe/vegetation/tree_spruce_v01.png`
  Prompt: CUTOUT dense dark-green spruce, layered broad triangular bough masses, compact cold shadow, no snow.
- **EE-VEG-004 — snow-covered spruce** — `theaters/eastern_europe/vegetation/tree_spruce_snow_v01.png`
  Prompt: CUTOUT spruce matching EE-VEG-003 with broad off-white snow caps on upper boughs, dark green still visible.
- **EE-VEG-005 — bare winter tree** — `theaters/eastern_europe/vegetation/tree_winter_bare_v01.png`
  Prompt: CUTOUT broad old deciduous winter tree, thick readable branching silhouette with restrained fine twigs, no leaves.
- **EE-VEG-006 — dense forest mass** — `theaters/eastern_europe/vegetation/mass_dense_forest_v01.png`
  Prompt: CUTOUT wide forest-edge mass combining overlapping spruce and birch crowns, irregular boundary, no isolated trunk resembling a figure.
- **EE-VEG-007 — cold-climate bush** — `theaters/eastern_europe/vegetation/bush_cold_climate_v01.png`
  Prompt: CUTOUT low tangled bush, muted olive and brown broad strokes, sparse pale leaves, no berries.
- **EE-VEG-008 — frozen reeds** — `theaters/eastern_europe/vegetation/cluster_reeds_frozen_v01.png`
  Prompt: CUTOUT waterside reed clump, straw-brown stems grouped into broad masses with small snow caps at base.
- **EE-VEG-009 — snow grass** — `theaters/eastern_europe/vegetation/tuft_snow_grass_v01.png`
  Prompt: CUTOUT low dead-grass tuft emerging through irregular snow base, simple repeated-scatter silhouette.
- **EE-VEG-010 — fallen branches** — `theaters/eastern_europe/vegetation/cluster_fallen_branches_v01.png`
  Prompt: CUTOUT low tangle of three broad fallen branches, dark bark with sparse pale twigs, no cut lumber.
- **EE-VEG-011 — logging debris** — `theaters/eastern_europe/vegetation/cluster_logging_debris_v01.png`
  Prompt: CUTOUT low logging debris of bark strips, wood chips, two short log ends, and sparse needles, no tools.

## Eastern Europe architecture

- **EE-ARCH-001 — wooden hut** — `theaters/eastern_europe/architecture/hut_wooden_v01.png`
  Prompt: STRUCTURE compact weathered timber hut, steep muted roof, small porch, dark doorway, no occupants, signs, or national decoration.
- **EE-ARCH-002 — logging camp shed** — `theaters/eastern_europe/architecture/shed_logging_camp_v01.png`
  Prompt: STRUCTURE rough open-sided logging shed with timber roof and empty work bay, broad rectangular footprint, no people or machinery.
- **EE-ARCH-003 — rail station** — `theaters/eastern_europe/architecture/station_rail_small_v01.png`
  Prompt: STRUCTURE small provincial rail station, weathered plaster and timber, long platform edge, blank signboard with no text.
- **EE-ARCH-004 — factory block** — `theaters/eastern_europe/architecture/factory_block_v01.png`
  Prompt: STRUCTURE compact brick factory block with sawtooth roof, dark empty windows, one short smokestack with no smoke, no signage.
- **EE-ARCH-005 — warehouse** — `theaters/eastern_europe/architecture/warehouse_v01.png`
  Prompt: STRUCTURE low brick-and-corrugated warehouse, large closed loading doors, restrained rust, no labels or faction markings.
- **EE-ARCH-006 — maintenance shed** — `theaters/eastern_europe/architecture/shed_maintenance_v01.png`
  Prompt: STRUCTURE narrow rail maintenance shed with weathered timber walls and rusted roof, empty open end, no train or tools outside.
- **EE-ARCH-007 — timber bridge** — `theaters/eastern_europe/architecture/bridge_timber_v01.png`
  Prompt: STRUCTURE short timber road bridge with 256 px North and South route sockets, heavy cold-weather beams, no water plate or vehicles.
- **EE-ARCH-008 — signal tower** — `theaters/eastern_europe/architecture/tower_rail_signal_v01.png`
  Prompt: STRUCTURE small neutral railway signal cabin on a low tower base, broad windows, blank equipment face, no readable signals or text.
- **EE-ARCH-009 — damaged industrial facade** — `theaters/eastern_europe/architecture/facade_industrial_damaged_v01.png`
  Prompt: STRUCTURE freestanding damaged brick industrial facade, broken roofline and empty windows, stable broad silhouette, no fire, people, or graphic aftermath.

## Eastern Europe flavor props

- **EE-PROP-001 — rail sleepers** — `theaters/eastern_europe/flavor/prop_rail_sleepers_v01.png`
  Prompt: PROP stack of six weathered timber railway sleepers, dark creosote brown, broad squared ends.
- **EE-PROP-002 — rail tools** — `theaters/eastern_europe/flavor/prop_rail_tools_v01.png`
  Prompt: PROP low arrangement of a sledgehammer, track wrench, and crowbar, simplified readable shapes, no blood or damage.
- **EE-PROP-003 — toolbox** — `theaters/eastern_europe/flavor/prop_toolbox_v01.png`
  Prompt: PROP closed dented metal toolbox, muted industrial blue-grey with rusted corners, no logo or text.
- **EE-PROP-004 — oil drums** — `theaters/eastern_europe/flavor/prop_oil_drums_v01.png`
  Prompt: PROP three anonymous dark oil drums, weathered blue-grey and brown paint, no labels or symbols.
- **EE-PROP-005 — maintenance cart** — `theaters/eastern_europe/flavor/prop_maintenance_cart_v01.png`
  Prompt: PROP small empty four-wheel rail maintenance cart with simple timber deck and steel wheels, no engine or markings.
- **EE-PROP-006 — timber stack** — `theaters/eastern_europe/flavor/prop_timber_stack_v01.png`
  Prompt: PROP orderly stack of cut logs with alternating ends and two binding rails, no text or tools.
- **EE-PROP-007 — firewood pile** — `theaters/eastern_europe/flavor/prop_firewood_pile_v01.png`
  Prompt: PROP low irregular pile of split firewood, snow dusting on one side, compact readable silhouette.
- **EE-PROP-008 — supply crates** — `theaters/eastern_europe/flavor/prop_supply_crates_v01.png`
  Prompt: PROP three rough wooden supply crates, blank faces, dark cold-weather timber, one lid slightly offset but contents hidden.
- **EE-PROP-009 — snow-covered tarp** — `theaters/eastern_europe/flavor/prop_tarp_snow_covered_v01.png`
  Prompt: PROP low anonymous equipment mound under a muted green-grey tarp with broad snow cap, no identifiable weapon or vehicle shape.
- **EE-PROP-010 — timber barricade** — `theaters/eastern_europe/flavor/prop_barricade_timber_v01.png`
  Prompt: PROP low crossed-timber barricade with broad beams and restrained snow, no wire, sign, or insignia.
- **EE-PROP-011 — field telephone** — `theaters/eastern_europe/flavor/prop_field_telephone_v01.png`
  Prompt: PROP compact neutral field telephone box with handset and coiled cable, no text, logo, or faction markings.
- **EE-PROP-012 — sandbags** — `theaters/eastern_europe/flavor/prop_sandbags_v01.png`
  Prompt: PROP low curved row of weathered grey-brown sandbags with patchy snow, empty behind.
- **EE-PROP-013 — rusted scrap** — `theaters/eastern_europe/flavor/prop_rusted_scrap_v01.png`
  Prompt: PROP anonymous rusted metal scrap cluster, bent sheet, pipe, gear, and bracket, no recognizable weapon or vehicle.

## Eastern Europe clusters and decals

- **EE-CLUSTER-001 — rail maintenance** — `theaters/eastern_europe/clusters_decals/cluster_rail_maintenance_v01.png`
  Prompt: CUTOUT rail-maintenance dressing cluster with sleepers, maintenance cart, closed toolbox, and low tool arrangement, no workers or train.
- **EE-CLUSTER-002 — logging camp** — `theaters/eastern_europe/clusters_decals/cluster_logging_camp_v01.png`
  Prompt: CUTOUT logging-camp dressing cluster with timber stack, firewood, blank crates, and wood-chip scatter, no people or active machinery.
- **EE-CLUSTER-003 — frozen checkpoint** — `theaters/eastern_europe/clusters_decals/cluster_frozen_checkpoint_v01.png`
  Prompt: CUTOUT empty frozen checkpoint with timber barricade, sandbag corner, field telephone box, and snow-covered tarp, no insignia or occupants.
- **EE-CLUSTER-004 — factory yard** — `theaters/eastern_europe/clusters_decals/cluster_factory_yard_v01.png`
  Prompt: CUTOUT industrial-yard cluster with anonymous drums, rusted scrap, pallet, and blank crates on a compact footprint, no vehicle.
- **EE-CLUSTER-005 — abandoned position** — `theaters/eastern_europe/clusters_decals/cluster_abandoned_position_v01.png`
  Prompt: CUTOUT empty abandoned position with snow-dusted sandbags, blank ammunition boxes, broken boards, and cold ashes, no bodies or weapons.
- **EE-DECAL-001 — snow disturbance** — `theaters/eastern_europe/clusters_decals/decal_snow_disturbance_v01.png`
  Prompt: DECAL irregular churned snow patch revealing dark earth in broad non-graphic shapes, no explicit footprints.
- **EE-DECAL-002 — tire ruts** — `theaters/eastern_europe/clusters_decals/decal_tire_ruts_snow_v01.png`
  Prompt: DECAL paired tire ruts compressed into snow, short repeatable ends, dark slush at centers.
- **EE-DECAL-003 — muddy wheel tracks** — `theaters/eastern_europe/clusters_decals/decal_wheel_tracks_mud_v01.png`
  Prompt: DECAL paired broad wheel tracks through dark mud with restrained wet highlights, repeatable ends.
- **EE-DECAL-004 — rust stains** — `theaters/eastern_europe/clusters_decals/decal_rust_stains_v01.png`
  Prompt: DECAL irregular restrained orange-brown rust runoff and ring marks, transparent gaps, no object attached.
- **EE-DECAL-005 — broken boards** — `theaters/eastern_europe/clusters_decals/decal_broken_boards_v01.png`
  Prompt: DECAL four low scattered broken timber boards with broad splinter shapes, no nails emphasized.
- **EE-DECAL-006 — frost overlay** — `theaters/eastern_europe/clusters_decals/overlay_frost_v01.png`
  Prompt: transparent 1024×1024 edge-continuous pale frost texture with broad feathered crystals, subtle enough for gameplay visibility, no opaque background.
- **EE-DECAL-007 — snowfall overlay** — `theaters/eastern_europe/clusters_decals/overlay_snow_atmosphere_v01.png`
  Prompt: transparent 1024×1024 restrained atmospheric snowfall overlay, sparse soft flakes in three sizes, no storm whiteout or vignette.

---

# Pacific production kit

Use ART-ENV-008 and ART-ENV-009 as palette and brushwork references: deep
jungle green, warm red-brown and wet umber mud, volcanic blue-grey, pale
coral sand, weathered timber, humid soft light, and high silhouette clarity
without turning foliage into visual noise.

## Pacific terrain and boundaries

- **PAC-TERR-001 — deep jungle floor** — `theaters/pacific/terrain/ground_deep_jungle_v01.png`
  Prompt: TILE seamless deep jungle floor, warm dark loam, broad fallen leaves, subtle roots and moss, no plants rising above ground, route, or props.
- **PAC-TERR-002 — warm mud** — `theaters/pacific/terrain/ground_warm_mud_v01.png`
  Prompt: TILE seamless warm red-brown compact mud with broad dry and damp variation, no tracks, puddles, route, or vegetation.
- **PAC-TERR-003 — wet mud** — `theaters/pacific/terrain/ground_wet_mud_v01.png`
  Prompt: TILE seamless deep umber wet mud with restrained reflective patches and soft churned texture, no tracks or standing-water pools.
- **PAC-TERR-004 — volcanic rock** — `theaters/pacific/terrain/ground_volcanic_grey_rock_v01.png`
  Prompt: TILE seamless volcanic blue-grey rock, broad porous masses and dark seams, low contrast, no lava, route, or objects.
- **PAC-TERR-005 — coral beach** — `theaters/pacific/terrain/ground_coral_beach_v01.png`
  Prompt: TILE seamless crushed-coral beach ground, warm off-white chips with faint peach and grey variation, restrained detail, no water.
- **PAC-TERR-006 — beach sand** — `theaters/pacific/terrain/ground_beach_sand_v01.png`
  Prompt: TILE seamless pale tropical beach sand with broad damp-to-dry value variation, no tracks, shells, water, or objects.
- **PAC-TERR-007 — shallow water** — `theaters/pacific/terrain/water_shallow_v01.png`
  Prompt: TILE seamless clear blue-green shallow water with broad sand-color depth showing through and restrained painted ripples, no shore.
- **PAC-TERR-008 — deep water** — `theaters/pacific/terrain/water_deep_v01.png`
  Prompt: TILE seamless deep tropical water, dark teal-blue broad ripple masses with controlled highlights, no foam, shore, or objects.
- **PAC-TERR-009 — river ford** — `theaters/pacific/terrain/ground_river_ford_v01.png`
  Prompt: TILE shallow river ford running East–West, clear brown-green water across the full tile and a centered 256 px stony crossing running North–South, sockets only North and South.
- **PAC-TERR-010 — airstrip surface** — `theaters/pacific/terrain/ground_airstrip_compacted_v01.png`
  Prompt: TILE seamless broad compacted coral-and-earth airstrip surface, pale dusty center wear, no painted numbers, lines, aircraft, or objects.
- **PAC-TERR-011 — timber track shoulder** — `theaters/pacific/terrain/boundary_timber_track_shoulder_v01.png`
  Prompt: CUTOUT straight low timber-and-mud route shoulder segment, short uneven logs parallel to edge with jungle leaf litter, square repeatable ends.
- **PAC-TERR-012 — coral shoreline** — `theaters/pacific/terrain/shoreline_coral_v01.png`
  Prompt: TILE coastline transition with coral sand on West and blue-green shallow water on East, shoreline centered vertically and edge-continuous North–South, gentle foam, no route.
- **PAC-TERR-013 — volcanic outcrop** — `theaters/pacific/terrain/outcrop_volcanic_v01.png`
  Prompt: CUTOUT broad low volcanic rock outcrop, three dark blue-grey porous masses with moss accents, compact obstacle footprint.

## Pacific vegetation

- **PAC-VEG-001 — coconut palm** — `theaters/pacific/vegetation/tree_coconut_palm_v01.png`
  Prompt: CUTOUT coconut palm, radial crown of nine broad deep-green fronds, curved warm trunk visible at center, no detailed fruit.
- **PAC-VEG-002 — broad jungle canopy** — `theaters/pacific/vegetation/tree_broad_jungle_canopy_v01.png`
  Prompt: CUTOUT broad tropical tree crown in four overlapping green masses with warm trunk glimpses, bold asymmetrical top-down silhouette.
- **PAC-VEG-003 — bamboo cluster** — `theaters/pacific/vegetation/cluster_bamboo_v01.png`
  Prompt: CUTOUT dense bamboo cluster, grouped yellow-green stems and broad leaf masses, wide vegetation silhouette, no stake or fence appearance.
- **PAC-VEG-004 — vine curtain** — `theaters/pacific/vegetation/cluster_vines_v01.png`
  Prompt: CUTOUT low trailing jungle-vine mass with broad heart-shaped leaves and two looping stems, no upright person-like forms.
- **PAC-VEG-005 — fern cluster** — `theaters/pacific/vegetation/cluster_ferns_v01.png`
  Prompt: CUTOUT radial cluster of five broad tropical fern fronds in two green values, simple repeated-scatter silhouette.
- **PAC-VEG-006 — broadleaf bush** — `theaters/pacific/vegetation/bush_broadleaf_v01.png`
  Prompt: CUTOUT low dense broadleaf bush, three rounded leaf masses with warm shadow gaps, no individual leaf noise.
- **PAC-VEG-007 — river reeds** — `theaters/pacific/vegetation/cluster_river_reeds_v01.png`
  Prompt: CUTOUT humid river-edge reeds in broad bright and dark green groups, low mud base, no flowers.
- **PAC-VEG-008 — tropical grass** — `theaters/pacific/vegetation/tuft_tropical_grass_v01.png`
  Prompt: CUTOUT small tropical grass tuft, broad arching blades with a dark center, simple repeated-scatter footprint.
- **PAC-VEG-009 — fallen palm fronds** — `theaters/pacific/vegetation/cluster_fallen_fronds_v01.png`
  Prompt: CUTOUT three low fallen palm fronds overlapping on leaf litter, muted green-brown, no trunk.
- **PAC-VEG-010 — low jungle foliage mass** — `theaters/pacific/vegetation/mass_low_jungle_foliage_v01.png`
  Prompt: CUTOUT wide low foliage mass matching ART-ENV-009, layered ferns, broad leaves, and vines with one clear irregular boundary and no infantry-like silhouettes.
- **PAC-VEG-011 — canopy edge mass** — `theaters/pacific/vegetation/mass_jungle_canopy_edge_v01.png`
  Prompt: CUTOUT long dense jungle-edge barrier, overlapping broadleaf and palm crowns, square repeatable ends, dark center and readable outer contour.

## Pacific architecture

- **PAC-ARCH-001 — timber bamboo hut** — `theaters/pacific/architecture/hut_timber_bamboo_v01.png`
  Prompt: STRUCTURE compact timber-and-bamboo hut with steep weathered palm roof, raised floor, empty doorway, no people, signs, or faction markings.
- **PAC-ARCH-002 — jungle bunker** — `theaters/pacific/architecture/bunker_jungle_v01.png`
  Prompt: STRUCTURE low anonymous timber-and-earth jungle bunker, one dark empty firing slit, broad leaf camouflage, no weapon, occupant, or insignia.
- **PAC-ARCH-003 — supply dump shelter** — `theaters/pacific/architecture/shelter_supply_dump_v01.png`
  Prompt: STRUCTURE open-sided weathered timber supply shelter with corrugated roof and empty floor, no crates baked into the structure.
- **PAC-ARCH-004 — airstrip control hut** — `theaters/pacific/architecture/hut_airstrip_control_v01.png`
  Prompt: STRUCTURE small raised timber airstrip control hut, broad windows and simple stair, blank roof, no radio mast labels, flags, or people.
- **PAC-ARCH-005 — fuel shelter** — `theaters/pacific/architecture/shelter_fuel_v01.png`
  Prompt: STRUCTURE low corrugated-roof fuel shelter with two anonymous drums in deep shade, no labels, symbols, or vehicle.
- **PAC-ARCH-006 — timber ford bridge** — `theaters/pacific/architecture/bridge_timber_ford_v01.png`
  Prompt: STRUCTURE short rough timber bridge with exactly 256 px North and South route sockets, wet planks and simple log rails, no water plate or vehicle.
- **PAC-ARCH-007 — observation post** — `theaters/pacific/architecture/post_observation_v01.png`
  Prompt: STRUCTURE small elevated timber observation platform with palm-thatch shade, empty deck, broad stable silhouette, no weapon or occupant.
- **PAC-ARCH-008 — weathered depot** — `theaters/pacific/architecture/depot_weathered_v01.png`
  Prompt: STRUCTURE compact weathered timber depot with rusted corrugated roof, wide closed doors, moss accents, no text or insignia.

## Pacific flavor props

- **PAC-PROP-001 — supply crates** — `theaters/pacific/flavor/prop_supply_crates_v01.png`
  Prompt: PROP three weathered tropical hardwood supply crates, blank faces, two stacked and one separate, moss-darkened bottoms.
- **PAC-PROP-002 — fuel drums** — `theaters/pacific/flavor/prop_fuel_drums_v01.png`
  Prompt: PROP cluster of three anonymous fuel drums, faded blue-grey and olive paint with humid rust, no labels or symbols.
- **PAC-PROP-003 — tarpaulin** — `theaters/pacific/flavor/prop_tarpaulin_v01.png`
  Prompt: PROP folded and partly spread weathered olive tarpaulin with wet highlights and broad creases, no covered object silhouette.
- **PAC-PROP-004 — pallet** — `theaters/pacific/flavor/prop_pallet_v01.png`
  Prompt: PROP rough tropical timber pallet, empty, moss-darkened corners, broad readable slats.
- **PAC-PROP-005 — handcart** — `theaters/pacific/flavor/prop_handcart_v01.png`
  Prompt: PROP empty two-wheel timber handcart with broad handles and mud-dark wheels, no cargo.
- **PAC-PROP-006 — radio equipment** — `theaters/pacific/flavor/prop_radio_equipment_v01.png`
  Prompt: PROP neutral closed radio case, handset, cable coil, and short aerial grouped compactly, no text, logo, or faction marking.
- **PAC-PROP-007 — timber stack** — `theaters/pacific/flavor/prop_timber_stack_v01.png`
  Prompt: PROP orderly stack of rough cut tropical logs with two binding rails, damp dark bark and pale cut ends.
- **PAC-PROP-008 — rope coil** — `theaters/pacific/flavor/prop_rope_coil_v01.png`
  Prompt: PROP thick weathered tan rope in one broad coil with short loose end, no hook or anchor.
- **PAC-PROP-009 — bamboo stakes** — `theaters/pacific/flavor/prop_bamboo_stakes_v01.png`
  Prompt: PROP low bundled bamboo stakes laid horizontally, blunt simplified ends, no trap arrangement or blood.
- **PAC-PROP-010 — sandbags** — `theaters/pacific/flavor/prop_sandbags_v01.png`
  Prompt: PROP low curved row of damp olive-tan sandbags with moss-dark seams, empty behind, no weapon.
- **PAC-PROP-011 — coral rubble** — `theaters/pacific/flavor/prop_coral_rubble_v01.png`
  Prompt: PROP low scattered coral-limestone rubble in warm off-white and pale grey, three broad fragments and pebble gaps.
- **PAC-PROP-012 — broken planks** — `theaters/pacific/flavor/prop_broken_planks_v01.png`
  Prompt: PROP four weathered broken timber planks in a loose flat pile, wet dark ends, no nails emphasized.
- **PAC-PROP-013 — airfield markers** — `theaters/pacific/flavor/prop_airfield_markers_v01.png`
  Prompt: PROP three faction-neutral low runway edge markers, plain white-painted stones with no letters, numbers, flags, or lights.

## Pacific clusters and decals

- **PAC-CLUSTER-001 — jungle cache** — `theaters/pacific/clusters_decals/cluster_jungle_cache_v01.png`
  Prompt: CUTOUT concealed jungle cache with blank crates, folded tarp, radio case, rope, and broad foliage around an open center, no weapons or faction marks.
- **PAC-CLUSTER-002 — river ford** — `theaters/pacific/clusters_decals/cluster_river_ford_v01.png`
  Prompt: CUTOUT ford dressing with wet stones, two rough planks, reeds, and mud splash marks, no full water tile, people, or vehicle.
- **PAC-CLUSTER-003 — airfield supply** — `theaters/pacific/clusters_decals/cluster_airfield_supply_v01.png`
  Prompt: CUTOUT airfield supply cluster with anonymous drums, blank crates, pallet, handcart, and neutral edge markers, no aircraft or insignia.
- **PAC-CLUSTER-004 — bunker perimeter** — `theaters/pacific/clusters_decals/cluster_bunker_perimeter_v01.png`
  Prompt: CUTOUT empty jungle defensive dressing with damp sandbags, bamboo stakes laid low, camouflage foliage, and blank boxes, no weapon or occupants.
- **PAC-CLUSTER-005 — timber worksite** — `theaters/pacific/clusters_decals/cluster_timber_worksite_v01.png`
  Prompt: CUTOUT humid timber worksite with stacked logs, broken planks, rope coil, pallet, and wood chips, no workers or active machinery.
- **PAC-DECAL-001 — wet footprints** — `theaters/pacific/clusters_decals/decal_wet_footprints_v01.png`
  Prompt: DECAL short abstract boot-print trail in wet mud, low contrast, no person, blood, or bare feet.
- **PAC-DECAL-002 — tire tracks** — `theaters/pacific/clusters_decals/decal_tire_tracks_v01.png`
  Prompt: DECAL short paired tire ruts through warm mud with restrained wet highlights, repeatable ends.
- **PAC-DECAL-003 — shell marks** — `theaters/pacific/clusters_decals/decal_shell_marks_v01.png`
  Prompt: DECAL shallow non-graphic impact marks in mud and leaf litter, three broad churned spots, no smoke or debris plume.
- **PAC-DECAL-004 — leaf piles** — `theaters/pacific/clusters_decals/decal_leaf_piles_v01.png`
  Prompt: DECAL two low tropical leaf piles, broad palm and jungle leaf shapes in muted green-brown, transparent gaps.
- **PAC-DECAL-005 — mud splashes** — `theaters/pacific/clusters_decals/decal_mud_splashes_v01.png`
  Prompt: DECAL irregular flat warm-brown mud splashes and droplets, no raised spray or object.
- **PAC-DECAL-006 — waterline** — `theaters/pacific/clusters_decals/overlay_waterline_v01.png`
  Prompt: transparent 1024×1024 shoreline waterline overlay with one broad gentle foam trace and wet-sand darkening, edge-continuous left to right, no opaque ground.
- **PAC-DECAL-007 — tide sheen** — `theaters/pacific/clusters_decals/overlay_tide_sheen_v01.png`
  Prompt: transparent 1024×1024 restrained blue-green tide sheen and two broad reflected-light bands, no foam wall, objects, or vignette.

---

# Held categories — intentionally not prompted

Do not generate these until the implementation review requested by the user
is complete and their visual requirements are confirmed:

- Nation-specific tower art, tower upgrade states, national insignia,
  nation-specific firing/tracer art, and signature-tower art.
- Nation-specific friendly units, all enemy units, enemy wrecks, and other
  unit-specific art.

# Recommended generation order

1. Generate one accepted neutral ground tile and the ten route topologies for
   a single theater; run the adjacency test before producing variants.
2. Generate the theater transitions and boundary pieces against those
   accepted route and ground references.
3. Generate terrain materials, then vegetation, architecture, props,
   clusters, and decals, keeping one accepted anchor image attached.
4. Generate shared build pads and map markers after a theater kit proves the
   target gameplay scale.
5. Generate weather, destruction states, VFX, and interaction overlays only
   after Clean, Typical, and Stress screenshots establish their contrast.

# Per-asset acceptance checklist

- The output matches the requested file type, dimensions, transparency, and
  exact filename.
- Every route socket is centered, exactly 256 px wide, perpendicular for its
  first 160 px, and limited to the named edges.
- Route width, shoulders, ruts, edge ground, and lighting match every other
  tile in the same theater family.
- The primary silhouette and gameplay function remain readable at 25% scale,
  in grayscale, and during a quick blur test.
- No people, units, vehicles, text, watermark, insignia, flags, propaganda,
  political symbols, gore, or accidental black transparency backdrop appear.
- The asset is still `REVIEW` until it has been cleaned, imported, and tested
  in Clean, Typical, and Stress gameplay screenshots.
