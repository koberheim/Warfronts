# Initial art generation log

Generated 2026-09-03 with the built-in ChatGPT image-generation tool. All
outputs are first-pass review references and remain `REVIEW` until cropped,
cleaned, imported at native scale, and checked in gameplay screenshots.

All prompts used the same guardrails: painterly storybook 2D, 80% stylized /
20% grounded, overhead-with-cheat top-down view, broad readable shapes,
restrained texture, no text/watermark, no people, no units, no enemies, no
real or fictional political insignia, no propaganda, no gore.

| ID | Prompt focus | Output |
|---|---|---|
| ART-ENV-001 | Western Europe bocage grass/patchwork field ground tile; rich greens, ochres, damp earth, overcast | `theaters/western_europe/terrain/ground_bocage_grass_v01.png` |
| ART-ENV-002 | Western Europe sunken muddy lane; readable central route, raised banks, wheel wear | `theaters/western_europe/terrain/road_sunken_lane_v01.png` |
| ART-ENV-003 | Western Europe hedgerow wall segment; irregular foliage masses, roots, earthen bank, isolated cutout | `theaters/western_europe/terrain/hedgerow_wall_v01.png` |
| ART-ENV-004 | Western Europe Norman-style stone farmhouse; broad roof, limestone walls, damaged roof state, isolated hero structure | `theaters/western_europe/architecture/hero_stone_farmhouse_v01.png` |
| ART-ENV-005 | Western Europe farmyard supply cluster; crates, barrels, hay, cart, fence, tarp | `theaters/western_europe/flavor/cluster_farmyard_supply_v01.png` |
| ART-ENV-006 | Mediterranean rocky wadi; dusty ochre, limestone, dry channel, sparse scrub, open route | `theaters/mediterranean/terrain/ground_rocky_wadi_v01.png` |
| ART-ENV-007 | Eastern Europe snowbound forest road; packed snow, conifers, birch, cold mud | `theaters/eastern_europe/terrain/road_snowbound_forest_v01.png` |
| ART-ENV-008 | Pacific jungle mud track; wet mud, puddles, roots, ferns, clustered tropical foliage | `theaters/pacific/terrain/road_jungle_mud_track_v01.png` |
| ART-ENV-009 | Pacific jungle foliage cluster; palm, broad leaves, ferns, vine, volcanic rock, isolated cutout | `theaters/pacific/vegetation/cluster_jungle_foliage_v01.png` |
| ART-ENV-010 | Shared commander’s map-table frame; wood, brass, paper map border, clean central play area | `shared/ui/commander_map_table_frame_v01.png` |

## Follow-up review

- Review transparent-background behavior for the isolated assets. The image
  viewer displays transparent pixels as black, while some outputs also show a
  dark vignette in the preview; verify the actual alpha channel and clean or
  regenerate before approval.
- Check all assets at the intended 64 px reference-tile scale and in Clean,
  Typical, and Stress screenshots before integrating them into scenes.
- If the style direction is accepted, generate family variants next rather
  than isolated one-offs: road transitions, foliage variants, build pads,
  and the Western Europe prop families.
