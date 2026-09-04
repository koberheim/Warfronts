# Art asset pathways

The machine-readable source of truth for art slots is
`res://assets/data/art/art_asset_catalog.json`. It records each inventory
family, its stable production directory, filename pattern, placeholder, and
status. The catalog is family-level while work is only planned, because the
art specification gives production ranges rather than one fixed file per
variant. Generated review assets receive exact item-level entries so they can
be inspected without changing a whole family's approval state.

## Runtime replacement path

`scenes/art/art_asset_slot.tscn` contains an `ArtAssetSprite` node. A scene
only needs to set its `AssetId`; it does not need to know which placeholder or
final PNG is currently active.

The slot behavior is:

1. `PLACEHOLDER_READY` and `REVIEW` entries use the reusable SVG placeholder.
2. A final asset is placed in the catalogued production directory using the
   filename pattern.
3. After the asset passes the art acceptance gates, set the entry to
   `APPROVED` and fill in `production_path`.
4. Set `UseApprovedAsset = true` on the slot that should display it.

Until that last step, the existing primitive visuals remain the gameplay
default. This keeps review images from silently becoming production art.

The approved command-table frame is wired into `scenes_root/briefing.tscn`.
The approved Western Europe ground, hedgerow, farmhouse, and supply cluster
are wired into the `ArtEnvironment` layer of `scenes_root/mission.tscn`.
`scenes/art/terrain_adjacency_test.tscn` displays the six approved
terrain/route images together for manual seam, path-continuity, and shoulder
review. Different theaters are intentionally shown as test cases, not mixed
into one production map.

`scenes/art/western_europe_route_review.tscn` is the first same-theater
production-family review board. It loads the ten `REVIEW` sunken-lane entries
through `ArtAssetSprite.AllowReviewAsset`, displays eight in a touching closed
loop, and shows the two T-junctions, crossroad, and entry separately. Review
permission is opt-in and does not make review art available to normal mission
slots.

## Generation prompt path

`ART_GENERATION_PROMPTS.md` is the human-facing production queue that feeds
these runtime pathways. Each numbered entry supplies an exact output filename
inside the folder contract below. Its canonical style lock keeps the four
theater kits visually related, while each theater has its own palette anchor.

Route art uses a fixed 1024×1024 tile contract with 256 px centered edge
sockets and ten named topologies per theater. Each theater also has ten
material-transition prompts. Generate and validate one complete route family
before generating decorative variants; never connect tiles from different
theater families in a production map.

## Folder contract

- `shared/` holds reusable build pads, route markers, transitions, decals,
  UI, weather, destruction, VFX, and interaction art.
- `theaters/` holds the four reusable theater kits, each split into terrain,
  vegetation, architecture, flavor, clusters, and decals.
- `misc/references/` holds silhouette, grayscale, blur, and palette sheets.
- `towers/`, `units/`, and `enemies/` contain future-held identity pathways;
  they are catalogued but intentionally have no generated art in this pass.

Do not rename a catalog ID once a scene uses it. Create a new versioned file
inside the stable production directory instead.
