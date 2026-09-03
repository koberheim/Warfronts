# Fronts of War

A stylized, fictionalized WWII top-down tower defense game for Steam/PC.
See [docs/GDD.md](docs/GDD.md) for the full design document — it is the
source of truth for scope, mechanics, and the production plan.

## Structure

- `godot-project/` — the Godot 4.x (C#) game project. Active development.
- `docs/` — design docs, led by `GDD.md`.
  - `docs/DECISIONS.md` — the project's decision log, with who decided what.
  - `docs/PROGRESS.md` — live tracker against GDD §19's implementation ladder.
  - [`docs/FRONTS OF WAR ART DESIGN.md`](docs/FRONTS%20OF%20WAR%20ART%20DESIGN.md) — visual direction and placeholder-art reference.
  - `docs/fronts_of_war_map_planner_design_spec.md` and `maps/` — design-time map planning specification and 100-template catalog.
  - `docs/archive/` — four documents superseded by GDD v1.1 (an old Unity
    migration guide, and the pre-GDD tower/enemy design docs). Kept for
    history, not for implementation — see `docs/archive/README.md`.
- `legacy-phaser-prototype/` — an earlier Phaser.js/TypeScript isometric
  prototype, superseded by the GDD's Godot/top-down decision (see GDD §3.2).
  Kept for reference only; its tower/enemy naming fed into the GDD's national
  rosters, but none of its code is used going forward.
- `CUT.md` — ideas deliberately cut from scope, per GDD §18.2.
- `CLAUDE.md` / `AGENTS.md` — agent operating rules for this repo. Any
  coding agent working here (Claude Code, Codex, or otherwise) should read
  one of these first — they're kept identical in content.

## Getting started

Requires [Godot 4.7+ (.NET/C# build)](https://godotengine.org/) and the
.NET SDK.

```bash
cd godot-project
dotnet build FrontsOfWar.csproj   # verify the C# build
godot --path .                    # open in the editor
godot --headless --path . --run-tests=CoreTests --quit-after 1200
```

## Status

M3 vertical-slice systems are implemented in primitive grey-box form: Arsenal
friendlies, the Breakthrough Panzer boss, briefing/loadout/mission/results
flow, and the integrated tutorial. M3.5 adds the editor-only Map Planner, and
M4 contains the five remaining universal archetypes, six data-authored nation
profiles with parity validation, E3/E7/E12 enemy mechanics, and the Wave
Editor dock, and M5 now contains all five signatures, air corridors, E8–E11,
and the Balance Dashboard. See
[docs/PROGRESS.md](docs/PROGRESS.md).

The standalone `dotnet build` command requires an online restore of
`Godot.NET.Sdk/4.7.2`; the Godot Mono installation can currently load and
run the project using its bundled tooling. See `docs/DECISIONS.md` entries
D13 and D25.

Art replacement plumbing is documented in
`godot-project/assets/art/ART_ASSET_PATHWAYS.md`; new visual references should
use the catalogued `ArtAssetSprite` slot rather than hardcoded texture paths.
