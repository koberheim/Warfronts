# CLAUDE.md — agent rules for Fronts of War

This file governs how Claude Code (and any other coding agent working in
this repo — see `AGENTS.md`, which mirrors this content) should operate.
Read it before doing anything else in this repository.

## 1. The GDD is the source of truth

`docs/GDD.md` is the single authoritative design document. If a request, an
old file, or your own instinct conflicts with it, **the GDD wins** — stop
and flag the conflict rather than silently picking a side. Do not implement
anything tagged **[X]** in the GDD (explicitly out of scope; see §18.1).
Anything not tagged in the GDD does not exist — don't invent scope.

Do not treat anything in `docs/archive/` as a design source. Those four
files predate GDD v1.1 and are superseded (see `docs/archive/README.md` for
exactly what changed and why). If you're tempted to check them for a stat or
a name, check GDD §6/§8/§10 instead — they already absorbed what was worth
keeping.

## 2. Log every real decision

`docs/DECISIONS.md` is the project's decision log. Whenever you make a
call that isn't already pinned down by the GDD — a library choice, a folder
layout detail, an interpretation of an ambiguous GDD passage, a workaround
for a broken tool — add an entry: what was decided, **who decided it**
(`User` if they said so explicitly, `Claude` if you decided it under
standing delegation, `Joint` if it came out of back-and-forth), why, and its
status (`Active` / `Superseded` / `Open`). Follow the format already in the
file. This is not optional busywork — the project is being built by AI
agents across many sessions with no continuous human memory of the small
calls, and this file is how the next session (or the next agent) avoids
re-litigating or accidentally reversing something.

If you cut an idea rather than build it, record it in `CUT.md` instead
(GDD §17.3, §18.2) — not in `DECISIONS.md`.

## 3. Follow the architecture principles (GDD §15.1) without exception

1. **Data over code.** Towers, enemies, waves, maps, nations, doctrines are
   Godot `Resource` assets (`.tres`), never hardcoded. Adding content should
   never require a new script.
2. **One tuning surface.** All balance constants live in `GameBalanceConfig`
   (`src/Core/GameBalanceConfig.cs`). No magic numbers in behavior scripts.
3. **Low coupling.** Systems talk through `EventBus` (typed pub/sub) or
   interfaces, never direct references across systems. A tower does not know
   what a wave is.
4. **Determinism where it matters.** Fixed 60Hz tick via `GameLoop`
   (`_PhysicsProcess`, never `_Process`, never `Engine.TimeScale` for game
   speed). All gameplay randomness goes through a seeded `SeededRandom`
   instance — never `GD.Randf`/`System.Random` directly in gameplay code.
5. **Pool everything transient.** Projectiles, effects, damage numbers,
   enemies come from `ObjectPool<T>`. Never `PackedScene.Instantiate()`
   mid-wave.
6. **Small scripts.** No gameplay file over ~300 lines. If one's growing
   past that, it's doing two jobs — split it.
7. **Text-native, agent-editable.** `.tscn`/`.tres`/`.cs`/`.gd` are plain
   text; prefer editing them directly over requiring a live editor session.
   A running Godot editor with MCP tooling is a convenience for inspection,
   screenshots, and headless test runs — never assume it's available.

## 4. Follow the implementation order (GDD §19)

GDD §19 is a numbered prompt ladder from M0 (Foundation) through M8
(Modes & platform). Each numbered item names its own acceptance check.
Work through them roughly in order — later milestones depend on earlier
ones being real and tested, not stubbed. Don't jump ahead to content
(towers, enemies, maps) before the systems they depend on exist and pass
their acceptance check. If you deviate from the ladder's order, note why in
`DECISIONS.md`.

Current status: see `docs/PROGRESS.md`, kept up to date as work lands.

## 5. Anti-scope-creep (GDD §18)

After the vertical slice (M3), nothing enters the project without an
equal-sized thing leaving it (§18.2 — the one-in-one-out rule, tracked via
`CUT.md`). Before M3, resist the urge to add polish, extra towers, or extra
systems beyond what the current milestone's acceptance check asks for — the
M3 gate exists specifically so effort doesn't get spent multiplying an unfun
core (§17.2).

## 6. Content policy (GDD §14) is absolute, not a style guide

No Nazi or real fascist iconography, no named real political figures, no
Holocaust/genocide/atrocity content in any form, no civilians as targets, no
gore, no "Nazi" as a faction/unit/brand name. All national insignia are
fictionalized per the GDD's spec. If you're adding any art brief, copy,
audio line, or codex text, check it against GDD §14.3 before it ships. This
is not a judgment call to make casually — when in doubt, ask.

## 7. Working in this repo

- **Godot project root:** `godot-project/` (this is what you open in the
  editor / point `godot --headless --path .` at, not the repo root).
- **C# build check:** `cd godot-project && dotnet build FrontsOfWar.csproj`
  — verifies syntax/types without needing Godot itself.
- **Godot headless check:** `godot --headless --path . --check-only` — needs
  a **.NET-enabled** Godot build (see the open item below). Standard
  GDScript-only Godot binaries cannot load this project's C# autoloads and
  will fail with `No loader found for resource ... .cs`.
- **Known open blocker:** the Godot binary present on this machine as of
  `docs/DECISIONS.md` entry D13 is the non-.NET build. If headless/editor
  checks fail with the error above, that's why — don't assume the C# code
  itself is broken; verify with `dotnet build` first, then check which
  Godot binary is being invoked.
- **Namespace-per-folder:** `src/Core`, `src/Combat`, `src/Towers`, etc. map
  to `FrontsOfWar.Core`, `FrontsOfWar.Combat`, `FrontsOfWar.Towers`, and so
  on — keep new files in the folder matching their namespace.
- **`legacy-phaser-prototype/`** is dead code kept for reference only (see
  `docs/DECISIONS.md` D8/D9). Do not port code from it. Do not add features
  to it. If you're not sure whether something in it is worth reusing, the
  answer is almost certainly no — check GDD.md first.

## 8. When you're not sure

If a request conflicts with the GDD, if you're about to build something
tagged **[X]**, if an architecture principle above would have to be broken
to do what's asked, or if you're about to make a call with no clear answer
in the GDD and no obvious "right" default — stop and ask rather than
guessing. Log the resolution in `docs/DECISIONS.md` either way.

## 9. End-of-step recap

At the end of every development step, provide a brief plain-English recap
that covers: what was done, any review or decisions needed from the User, and
the next development step. Keep it concise and include it in the final
handoff after verification and any commit/push work.
