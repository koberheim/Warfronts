# Fronts of War

A stylized, fictionalized WWII top-down tower defense game for Steam/PC.
See [docs/GDD.md](docs/GDD.md) for the full design document — it is the
source of truth for scope, mechanics, and the production plan.

## Structure

- `godot-project/` — the Godot 4.x (C#) game project. Active development.
- `docs/` — design docs, led by `GDD.md`.
  - `docs/DECISIONS.md` — the project's decision log, with who decided what.
  - `docs/PROGRESS.md` — live tracker against GDD §19's implementation ladder.
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
```

## Status

Foundation (GDD Milestone M0) in progress: project structure, `GameLoop`
fixed-tick autoload, `EventBus`, `ObjectPool<T>`, `GameBalanceConfig`,
`SeededRandom`, and the damage resolver are scaffolded. See
[docs/PROGRESS.md](docs/PROGRESS.md) for the live checklist and
[docs/GDD.md §19](docs/GDD.md) for the full implementation prompt ladder.

**Known blocker:** the Godot binary currently installed on this machine
doesn't have C# support built in, so the C# scripts above compile
(`dotnet build`) but haven't been verified running inside Godot yet. See
`docs/DECISIONS.md` entry D13.
