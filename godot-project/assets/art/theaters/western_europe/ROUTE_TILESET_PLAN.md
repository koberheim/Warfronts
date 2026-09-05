# Western Europe route tileset plan

Concrete generation plan for the sunken-lane route family, at the terrain
grid decided in D64 (one placement cell = one gameplay tile = 64px). This
supersedes the sizing assumptions in the original `WE-ROUTE-*` batch in
`ART_GENERATION_PROMPTS.md` (built around a 1024px tile before D64 existed)
without discarding its art — see §5.

Ground-material tiles (grass, dirt, etc.) are a separate concern from this
plan; they tile the 64px base grid directly. This file covers only the
**route overlay layer** (D44/D45: a separate transparent layer composited on
top of ground material, not baked into it).

---

## 1. The seam problem, answered directly

**Correction from the first draft of this plan:** it originally repeated
D43's "outer 32px matches exactly, then blends" as if that guaranteed a
seamless join. It doesn't, and the actual test maps proved it — visible
zigzags at every boundary. I checked why: both existing prep scripts
(`Prepare-WesternEuropeTopologyOverlay.ps1`,
`Prepare-WesternEuropeRouteMaterial.ps1`) only crop, reposition, and strip
background artifacts. **Neither one blends anything.** The "matching edge"
requirement was only ever a sentence in a text prompt to an image
generator, and independent generations do not reproduce the same road-edge
position, angle, or curve closely enough for a hard pixel cut to look
seamless — that's what a zigzag is: two independently-painted edges meeting
at a line, each slightly different.

**The fix (D66): stop requiring the generator to be exact, and force the
blend deterministically instead.** Every route piece gets a soft **alpha
feather** on its socket edges — alpha ramps linearly from full opacity at
`FeatherWidth` px inward down to fully transparent at the true edge, applied
by a script (`tools/art/Add-RouteSocketFeather.ps1`), not requested in the
prompt. Adjacent pieces are then placed **overlapping by that same
`FeatherWidth`**, rather than flush edge-to-edge. Wherever they overlap,
whichever piece is drawn later in the scene tree (on top) fades from
transparent to opaque across the overlap zone, smoothly revealing the piece
underneath instead of cutting against it. This works regardless of how well
the two pieces' painted road edges actually agree, because there is no hard
cut line anywhere — only a gradient. Verified working: the script was
smoke-tested against an existing generated tile and produces a clean linear
alpha ramp (measured 0 / 13 / 64 / 128 / 191 / 255 at 0 / 5 / 24 / 48 / 72 /
96px from the edge, against a 96px `FeatherWidth`).

**Rotation is deliberately not used to save generation work here**, even
though it's the right call for units/towers (D3) — two reasons converge on
the same answer:
- **Established precedent (D44→D45):** an earlier pass tried deriving
  rotated companions from shared material and found it "visibly patterned"
  with "unattractive overlaps" at junctions — reversed in favor of unique
  painted art per topology.
- **Baked lighting (art spec §36-37):** terrain lighting is "largely baked
  into artwork" with "consistent light direction" as an explicit rule.
  Rotating a painted piece 90° rotates its shadows and rut shading with it,
  which is fine for a small independent unit sprite but reads as a visible
  seam across a large, edge-to-edge terrain surface. A symmetric 4-way
  crossroads is the one exception — it has no orientation to begin with, so
  one piece covers all four approaches by definition, not by rotating it.

## 2. Grid, socket, and feather contract

| Property | Value |
|---|---|
| Placement cell | 64px = 1 gameplay tile (D64) |
| Route piece footprint | 4×4 cells = **256×256px** final, placed as one `TerrainInstance` group per D64 |
| Generation canvas | **512×512px** (2× supersample, matching the existing build-pad/route convention of generating above final size for downsampling headroom) |
| Socket opening (at generation resolution) | **256px**, centered on the edge — reuses D45's number unchanged, now explicitly scaled: this is the *512px-canvas* figure |
| Final in-game road width | **128px = 2 gameplay tiles** after the standard 2:1 downsample |
| Shoulder margin | 64px (1 tile) on each side of the road within the final footprint |
| **Feather width (D66)** | **96px at generation resolution (48px final)**, applied via `Add-RouteSocketFeather.ps1` to every socket edge — replaces D43's "exact match" requirement entirely |
| **Placement overlap (D66)** | Adjacent pieces overlap by the feather width on their shared edge, not flush — the overlap *is* the blend zone |
| Draw order | Later-placed piece draws on top; its feathered edge dissolves into whatever is beneath. Any consistent scene-tree order works as long as it's consistent along a given path run. |

Production step order per piece: generate (§4) → run the existing
`Prepare-WesternEurope*.ps1` background cleanup → run
`Add-RouteSocketFeather.ps1` → place in the adjacency-test scene with the
overlap above → visually verify per §6 before `REVIEW` → `APPROVED`.

**Why 2 tiles wide, not D45's original 4:** D45 was decided before any
production map's actual tile dimensions existed anywhere in the repo. The
map-editor blueprint's worked example (Bocage Crossroads: 28×18 tiles) is
now on record, and a 4-tile-wide road would be roughly a seventh of that
map's width — disproportionate for what GDD calls a "sunken lane," and
inconsistent with GDD's own signal that a 1-tile-wide bridge is a
deliberately narrow chokepoint (implying normal roads are only modestly
wider, not four times wider). 2 tiles keeps the lane narrow and leaves real
room for build pads alongside it.

Route pieces are **not** locked to a repeating mega-grid the way ground
material is — a piece can start at any cell coordinate along the path. Only
piece-to-piece edge matching is required, so this works regardless of a
given map's total tile dimensions.

## 3. Full topology set

17 pieces. `NEW` = not in D45's original scope; `RE-AUTHOR` = a version
exists in `terrain/route_overlays/` but at the old footprint/road-width and
needs regenerating at this contract; `MISSING` = never generated.

| Piece | Orientations | Status | Filenames |
|---|---|---|---|
| **Straight** | N-S, E-W | MISSING (the old batch used a shared fallback material here, never a unique overlay — see §5) | `route_overlay_sunken_lane_straight_ns_v01.png`, `..._straight_ew_v01.png` |
| **Curved straight** *(NEW — answers "curved paths")* | N-S, E-W | NEW | `..._curve_ns_v01.png`, `..._curve_ew_v01.png` |
| **90° corner** | NE, ES, SW, WN | RE-AUTHOR (all 4 exist) | `..._ne_v01.png`, `..._es_v01.png`, `..._sw_v01.png`, `..._wn_v01.png` |
| **T-junction** | NES (W closed), ESW (N closed), NEW (S closed), NSW (E closed) | 2 RE-AUTHOR, 2 MISSING | `..._t_nes_v01.png`, `..._t_esw_v01.png`, `..._t_new_v01.png`, `..._t_nsw_v01.png` |
| **4-way crossroads** | none (symmetric) | RE-AUTHOR (1 exists) | `..._cross_v01.png` |
| **Dead end / entry** | N, E, S, W | 1 RE-AUTHOR, 3 MISSING | `..._entry_n_v01.png`, `..._entry_e_v01.png`, `..._entry_s_v01.png`, `..._entry_w_v01.png` |

The curved-straight variant isn't a new socket type — it connects the same
opposite-edge pair as a plain straight, at the identical socket, so it's a
drop-in visual alternate. Use it to break up long straight runs so a road
doesn't read as a single repeated tile. A curved *corner* variant (a wider
arc instead of a hard 90°) is a reasonable later addition but isn't required
for a complete, functional set — deferred rather than blocking this plan.

## 4. Generation prompts

Prepend the **canonical style lock** from `ART_GENERATION_PROMPTS.md` §"How
to use this file" plus the Western Europe palette anchor, unchanged. Then
use this shared socket clause on every prompt below, followed by the
per-piece differentiator. **Do not ask the generator to match pixels across
tiles** — that instruction failed in practice (§1) and the feather script
guarantees the blend regardless of what comes out here. The prompt only
needs a plausible, roughly-centered road of roughly the right width at each
named edge:

> ROUTE TILE, 512×512px, sunken-lane dirt track between low hedgerow banks,
> Western Europe palette. Road-and-shoulder roughly 256px wide, centered on
> each named edge this piece uses. No buildings, units, or props. No border
> or frame.

| Piece | Differentiator |
|---|---|
| Straight N-S | Road opening on North and South edges only; straight run, minor rut/weed variation along its length. |
| Straight E-W | Road opening on East and West edges only; straight run, minor rut/weed variation. |
| Curve N-S | Road opening on North and South edges only; a single gentle S-curve between them, same socket width at both ends. |
| Curve E-W | Road opening on East and West edges only; a single gentle S-curve between them. |
| Corner NE | Road opening on North and East edges only; one continuous 90° bend between them, hedgerow filling the inside corner. |
| Corner ES / SW / WN | Same pattern, rotated to the named edge pair — but painted uniquely per §1, not rotated in-engine. |
| T-junction NES | Road opening on North, East, and South edges; a single junction converging cleanly, no overlap patterning. West edge is closed hedgerow. |
| T-junction ESW / NEW / NSW | Same pattern, named edges open, remaining edge closed. |
| Crossroads | Road opening on all four edges, one clean four-way convergence, fully symmetric. |
| Entry N (and E/S/W) | Road opening on the named edge only; the opposite end terminates as a mapped-out dead end (low barrier, overgrown verge, or a hard tile-edge cut suitable for an off-map entry marker) rather than continuing toward the tile center. |

## 5. What happens to the existing REVIEW art

The 8 existing `route_overlays/` files (4 corners, 2 T-junctions, 1 cross, 1
entry) and the `WE-ROUTE-001–010` batch stay exactly where they are, still
`REVIEW`, still useful as **style/proportion references** for the
re-authoring pass — they already proved the corner/T/cross/entry shapes are
achievable at a consistent style. They are not production art at this
contract's dimensions and should not be promoted to `APPROVED` as-is; treat
regenerating them at the corrected footprint/road-width as a re-author, not
a from-scratch redesign.

## 6. Acceptance

Before any piece moves `REVIEW` → `APPROVED`:

1. Ran through `Add-RouteSocketFeather.ps1` — a piece with a hard,
   unfeathered edge does not enter review at all.
2. Passes the existing acceptance gates (silhouette, native-scale, contrast,
   rotation N/A here, style, grayscale, blur, screenshot — per
   `ART_ASSET_INVENTORY.md`).
3. Placed **overlapping by the feather width** (§2) — not edge-to-edge —
   with every piece it can legally neighbor in a real adjacency-test scene
   (extend `western_europe_route_review.tscn` rather than building a new
   one) and confirmed no visible seam, gradient band, or zigzag at 100% and
   25% zoom. This is the check that was skipped last time; it's the one
   that matters most.
4. A full loop using only this set (e.g. straight → corner → straight →
   T-junction → cross → dead end) reads as one continuous lane, not a
   repeating pattern, at normal play zoom.
