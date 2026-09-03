# Fronts of War art workspace

This folder is organized by reusable production kits, not by individual
missions. The active inventory is [ART_ASSET_INVENTORY.md](ART_ASSET_INVENTORY.md),
and the first-pass prompt record is [ART_GENERATION_LOG.md](ART_GENERATION_LOG.md).

## Rules

The machine-readable slot catalog is `../data/art/art_asset_catalog.json` and
the replacement workflow is [ART_ASSET_PATHWAYS.md](ART_ASSET_PATHWAYS.md).
New scenes should use `scenes/art/art_asset_slot.tscn` and set a catalog
`AssetId` rather than hardcoding a texture path.

- `theaters/` contains location-specific terrain, architecture, vegetation,
  flavor props, clusters, and decals.
- `shared/` contains assets reused across theaters: roads, build pads, map
  frame/UI materials, common overlays, and common VFX.
- `misc/` contains non-theater assets such as the command-table frame,
  objective markers, and authoring references.
- `towers/`, `units/`, and `enemies/` are intentionally held for a later
  review pass. Their future pathways exist in the catalog, but no identity
  art is generated or integrated in this pass.
  Do not generate or integrate them from this inventory during the current
  task.

Binary art belongs under the relevant kit folder. Use descriptive, stable
names with a family and variant suffix, for example
`western_europe/terrain/ground_bocage_grass_v01.png`.
