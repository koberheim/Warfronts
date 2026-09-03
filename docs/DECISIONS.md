# Decision log

A chronological record of decisions that shaped this project, with who made
each one. "User" means Kevin decided or explicitly confirmed; "Claude" means
the agent decided under standing delegation (e.g. "begin development
following the GDD") without a separate confirmation; "Joint" means it came
out of back-and-forth in a conversation turn. Entries link to the GDD section
that encodes the decision where one exists — the GDD stays the source of
truth for *what* was decided; this file is the record of *when, why, and by
whom*.

---

## Foundational (pre-dating this Claude Code session, recovered from GDD.md)

These are captured here because the GDD states the decision and its
rationale but not the meta-fact of who made the call or when it was
revisited — worth preserving before it's lost to editing history.

### D1 — Engine: Unity (superseded by D2)
- **Decided by:** User
- **Date:** GDD v1.0 (undated in-document; prior to the Godot revision)
- **Decision:** Build in Unity 2D, C#, ScriptableObject-driven data.
- **Status:** Superseded by D2. Kept here because D2's rationale only makes
  sense in contrast to this.

### D2 — Engine: Godot 4.x (C#), not Unity
- **Decided by:** User
- **Date:** GDD v1.1 ("Godot engine revision")
- **Decision:** Move off Unity onto Godot 4.x, native 2D, C# scripting.
- **Rationale:** This project's programming is done entirely by AI coding
  agents (Claude Code, Codex), not a human in an IDE. Godot's `.tscn`/`.tres`
  files are plain text and fully agent-editable without a running,
  GUI-attached editor process; Unity's `.unity`/`.prefab` YAML files are
  fragile to hand-edit and effectively require a live MCP-bridged Editor
  session, which isn't guaranteed available in headless/remote agent
  sessions. Godot's `Resource` type is a near-exact analog of
  `ScriptableObject`, so the data-over-code architecture ports with no
  conceptual change.
- **Reference:** GDD §3.2
- **Status:** Active, load-bearing for the whole project.

### D3 — Presentation: top-down with fixed build pads, not isometric or lane-based
- **Decided by:** User
- **Reference:** GDD §3.1
- **Rationale:** Top-down is the most legible way to show route, range, and
  threat simultaneously with no perspective distortion; a single sprite
  rotates freely with no 8-direction sheets, which matters a lot for a
  solo/AI-driven art budget. Rules out both the earlier isometric prototype's
  camera angle and a Kingdom-Rush-style fixed-lane layout.
- **Status:** Active. This is why the old Phaser prototype's isometric grid
  math (`IsoUtils.ts`, the 2:1 tile ratio) does not carry forward — see D8.

### D4 — Combat model: 4 damage types × 4 armor classes, nothing else
- **Decided by:** User
- **Reference:** GDD §5
- **Status:** Active. Replaces the old prototype's flat `damage - armor`
  subtraction model entirely (see D8, and `docs/archive/README.md`).

### D5 — Nine shared tower archetypes, one signature per nation
- **Decided by:** User
- **Reference:** GDD §6, §8.1
- **Rationale:** Six distinct-feeling armies from roughly one army's worth of
  engineering/balance surface. Replaces the old design docs' model of ten
  fully independent towers per nation.
- **Status:** Active.

### D25 - GoDotTest selected for the C# unit-test suite
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Use Chickensoft GoDotTest in the existing Godot project,
  pinned to package version 2.0.42. The boot scene routes
  `--run-tests=CoreTests` into the headless test runner and normal launches
  continue to the mission scene.
- **Rationale:** It is C#-first, runs inside the Mono Godot process, and
  supports headless command-line execution without introducing a second test
  project or a separate engine/runtime.
- **Status:** Active.

### D6 — No persistent meta-progression, no monetization beyond base price
- **Decided by:** User
- **Reference:** GDD §12.2, §18.1
- **Status:** Active, explicitly called out as having "no exceptions."

### D7 — Fictionalized theater framing; hard content rules on Nazi/fascist iconography, atrocities, civilians, gore
- **Decided by:** User
- **Reference:** GDD §14
- **Status:** Active, explicitly "absolute." Any future content (art briefs,
  copy, audio) must be checked against §14.3 before it ships.

---

## This session (2026-09-01) — "review docs, salvage what's useful, begin M0"

### D8 — The old Phaser prototype's code is not portable; its design-doc naming is
- **Decided by:** Claude, confirmed by User
- **Context:** User asked to review `docs/` and the existing project folder
  (a Phaser.js/TypeScript isometric prototype) against the GDD and begin
  development.
- **Decision:** None of `legacy-phaser-prototype/src/**` carries forward —
  wrong engine, wrong language, wrong camera angle (isometric vs. top-down),
  and a wholly different combat/data model (flat damage-minus-armor vs. the
  16-cell multiplier table). The two design-reference docs
  (`COUNTRIES_AND_TOWERS.md`, `ENEMIES_REFERENCE.md`) *did* feed into the
  GDD — most tower names (Browning MG Nest, Bazooka Squad, Pak 40, Katyusha
  Storm Battery, etc.) survive close to verbatim in GDD §8. Full comparison
  in `docs/archive/README.md`.
- **User confirmation:** presented this assessment via AskUserQuestion
  alongside the Godot-install question; user's reply ("Godot is already
  installed here...") implicitly accepted the assessment by proceeding.
- **Status:** Active.

### D9 — Keep the old prototype and superseded docs, don't delete
- **Decided by:** Claude
- **Decision:** Moved `src/`, `assets/`, `dist/`, build config, and
  `node_modules` into `legacy-phaser-prototype/`; moved the three superseded
  design docs plus the `.docx` into `docs/archive/` with a README explaining
  what changed and why. Nothing was deleted.
- **Rationale:** Per this session's own operating rules (see `CLAUDE.md`),
  in-progress or historical work gets moved aside, not destroyed, absent an
  explicit instruction to delete.
- **Status:** Active.

### D10 — Godot project structure follows GDD §15.2 exactly
- **Decided by:** Claude (mechanical — GDD §15.2 specifies the tree directly)
- **Decision:** Created `godot-project/` at the repo root (sibling to
  `docs/`, not replacing it) with the exact folder layout from §15.2:
  `addons/{wave_editor,balance_dashboard,map_pad_tool,data_validator}`,
  `assets/{art,audio,data}`, `scenes/`, `scenes_root/`, `src/<Namespace>/`,
  `tests/`.
- **Status:** Active.

### D11 — `.csproj` targets `net8.0` via `Godot.NET.Sdk/4.7.2`
- **Decided by:** Claude
- **Rationale:** Matches the installed Godot version (4.7.2) and the
  system's .NET SDK (10.0.400, which targets `net8.0` LTS builds fine).
  `Nullable` disabled and `LangVersion latest` chosen for lower ceremony in
  early scaffolding; revisit if the codebase would benefit from
  nullable-reference-type safety later.
- **Status:** Active, unverified in the Godot editor (see D13 — the
  installed Godot binary can't load C# yet).

### D12 — Renderer: `gl_compatibility`, not Forward+
- **Decided by:** Claude
- **Rationale:** GDD §15.4's performance budget explicitly targets "a
  2018-era integrated GPU laptop"; `gl_compatibility` is the broader-hardware
  choice for a fully 2D top-down game with no need for Forward+'s clustered
  lighting.
- **Status:** Active, worth revisiting once real performance data exists.

### D13 — Installed Godot binary lacks C# support — blocks headless verification
- **Decided by:** N/A (a discovered fact, not a decision)
- **Context:** `E:\Godot\Godot_v4.7.2-stable_win64.exe` is the standard
  (GDScript-only) build. Running `godot --headless --path . --check-only`
  against `godot-project/` fails to load any of the three C# autoloads
  (`EventBus.cs`, `GameLoop.cs`, `GameBalanceConfigAutoload.cs`) with
  `No loader found for resource ... (expected type: unknown)`, which is the
  standard symptom of a non-.NET Godot build being pointed at a C# project.
  `dotnet build FrontsOfWar.csproj` succeeds standalone (C# syntax/type
  checks are fine); only the Godot-side loader is missing.
- **Resolution (Claude, 2026-09-01):** downloaded the official
  `Godot_v4.7.2-stable_mono_win64.zip` from the GitHub releases page and
  extracted it to `E:\Godot\godot_mono\`. Confirmed via
  `godot --headless --path . --check-only` that it loads all three C#
  autoloads with zero errors (previously: three `No loader found` errors).
  **Note:** `--check-only` doesn't appear to auto-exit on this build — the
  process sits idle after printing the version banner and has to be killed.
  Use `--headless --path . --quit` (or `--quit-after N`) for automated
  checks going forward instead.
- **Status:** Resolved. Canonical .NET-enabled binary going forward:
  `E:\Godot\godot_mono\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64.exe`.
  The original non-.NET binary at `E:\Godot\Godot_v4.7.2-stable_win64.exe`
  is left in place untouched — not needed, but not in the way either.

### D14 — `GameBalanceConfig` values live as C# field defaults, not yet a `.tres` asset
- **Decided by:** Claude
- **Rationale:** GDD §15.1 principle 2 wants balance numbers on "one tuning
  surface," ideally a `.tres` `Resource` asset editable outside code (the
  spirit of "data over code," §15.1 principle 1). For M0, the C# class's
  `[Export]` field defaults *are* that single surface — no numbers are
  duplicated elsewhere — but no override asset exists yet at
  `res://assets/data/config/game_balance_config.tres`, so a designer can't
  yet tune values without editing code. This is a deliberate, temporary gap:
  authoring the `.tres` requires the editor to actually open, which is
  blocked by D13.
- **Status:** Open, tracked as an M0 follow-up once D13 resolves.

### D15 — Damage resolver interpretation: `AntiAir` row's Armored/Heavy cells are `0.00`
- **Decided by:** Claude
- **Context:** GDD §15.3 gives `ResolveDamage`'s signature as
  `(baseDamage, type, armor, isSpotted, table)` — no `isAir` parameter —
  while §5.4 describes Anti-Air multipliers only in terms of an air unit's
  Soft/Hardened armor class (Armored/Heavy air units don't exist per §5.3).
  Read literally, the function can't distinguish "AA fired at a Hardened
  ground unit" from "AA fired at a Hardened-armor air unit" by armor class
  alone.
- **Decision:** Encoded the `AntiAir` row as `{Soft: 1.00, Hardened: 0.75,
  Armored: 0.00, Heavy: 0.00}`. This matches §5.4's stated AA-vs-air values
  exactly, and makes AA vs. Armored/Heavy `ground` targets a non-issue in
  practice, since those cells are never reached — `TargetingService` (not
  yet built) is expected to enforce that AA-armed towers only ever acquire
  Air targets, and non-AA damage types only ever acquire ground targets, per
  §5.4's own note that "the tower simply does not acquire air targets."
- **Status:** Active, but should be re-checked once `TargetingService` (GDD
  §19 prompt 9) is implemented and can enforce the targeting-side half of
  this contract.

### D16 — Old `.claude/settings.local.json` permissions left untouched
- **Decided by:** Claude
- **Context:** The file allow-lists `npm run dev/build/type-check`, which
  belonged to the now-archived Phaser prototype.
- **Decision:** Left as-is rather than edited — not asked to, and it's
  low-risk clutter rather than something actively wrong. Flagging here so a
  future session (or User) can clean it up when convenient, e.g. swapping in
  `dotnet build`/`godot --headless` permissions instead.
- **Status:** Open, low priority.

### D18 — Custom `Resource` subclasses must be declared `type="Resource"` in hand-authored `.tres`/`.tscn` files, not their class name
- **Decided by:** Claude (a discovered technical constraint, encoded as a
  house rule)
- **Context:** While hand-authoring the first data assets for M1
  (`TowerDefinition`, `EnemyDefinition`, `WaveDefinition`, `SpawnGroup`,
  `TowerStatBlock` — all `[GlobalClass]` C# `Resource` subclasses),
  declaring them as `[gd_resource type="TowerDefinition" ...]` (using the
  actual class name) failed to load with `Cannot get class 'TowerDefinition'`
  / `Can't create sub resource of type 'TowerDefinition'`. This is because
  Godot only resolves a `[GlobalClass]`'s name to its type once it has built
  its global script class cache — which normally happens the first time the
  project is opened in the actual editor, not on a plain headless run.
- **Fix:** always declare `type="Resource"` in the `[gd_resource ...]` /
  `[sub_resource ...]` header, regardless of the actual C# class — the
  `script = ExtResource(...)` line inside the resource block is what
  actually attaches the real type at load time, and that always works
  headless with no editor warm-up required.
- **Status:** Active — treat this as a standing convention for every future
  hand-authored `.tres`/`.tscn` file referencing a custom `Resource`
  subclass, not a one-off fix. (An alternative would be to always open the
  project in the editor once per machine before headless runs, but relying
  on that defeats the point of everything here being agent- and
  CI-friendly — see GDD §15.1 principle 7.)

### D19 — M1 core loop verified end-to-end via a headless smoke run
- **Decided by:** Claude (verification, not really a decision — logged for
  the paper trail)
- **Context:** After D18's fix, ran
  `godot --headless --path . --quit-after 1500` (≈25 simulated seconds)
  against `scenes_root/mission.tscn`, which has a debug-only wave
  (`assets/data/missions/test_wave_01.tres`: 6× Basic Infantry, then 2×
  Medium Armor) auto-starting, plus a temporary `DebugEventLogger`
  (`src/Debug/DebugEventLogger.cs`) printing every gameplay event to
  stdout.
- **Result:** enemies spawned on schedule, moved along the path, were
  targeted and fired on by both the machine gun (T1, hitscan) and the
  anti-tank gun (T4, projectile with lead-prediction), took damage matching
  the GDD §5.4 multiplier table exactly (machine-gun fire vs. the armored
  tank landed ~0.8 damage per hit — 4 base × 0.20 multiplier — while the
  anti-tank shell landed ~94 damage — 75 base × 1.25 multiplier), died,
  and paid out the correct Supply bounty. Zero errors across the whole run.
- **Status:** M1's core-loop acceptance checks (GDD §19 prompts 6, 8, 9, 10,
  11, 12, 13) are substantively met by inspection of this run; see
  `docs/PROGRESS.md` for the itemized checklist. Formal automated unit
  tests are still open (D17).

### D20 — UI built programmatically in C#, not hand-authored `.tscn` layouts
- **Decided by:** Claude
- **Context:** M2 needed five UI pieces (HUD, wave preview, tower
  inspection panel, post-mortem panel, floating damage numbers). GDD §13
  assumes Control-node UI, typically hand-authored as scene files.
- **Decision:** built each as a C# script that constructs its own
  `Control`/`Label`/`Button` tree in `_Ready()`, rather than hand-writing
  `.tscn` files with anchors and offsets for each screen.
- **Rationale:** these UI pieces are still being iterated on heavily and a
  hand-written `.tscn` layout (anchors, offset math, theme overrides) is
  more failure-prone to author blind and harder to diff/review than C#
  code — and GDD §15.1 principle 7 already treats "text-native, agent-
  editable" as the goal, which C# satisfies just as well as `.tscn` does.
  `.tscn` remains the right choice for anything with real visual/spatial
  layout (the mission map, tower/enemy scenes) — this is specifically about
  screen-space UI chrome.
- **Status:** Active for now. Worth revisiting once the visual design
  actually needs iterating in-editor (dragging elements, previewing
  theming) — at that point hand-authored `.tscn` UI (or the editor's visual
  tools) will earn back their cost. Not a permanent architecture commitment.

### D21 — Godot's child-before-parent `_Ready()` order is a recurring trap — watch for it
- **Decided by:** Claude (a discovered engine behavior, documented as a
  standing caution)
- **Context:** Godot calls a node's `_Ready()` only after all of its
  children's `_Ready()` calls have completed. This bit twice in this
  session: (1) pre-placed towers under `MapRuntime` can't self-register
  with `TowerManager` in their own `_Ready()`, because `MapRuntime._Ready()`
  (which creates `TowerManager`) hasn't run yet — fixed by having
  `MapRuntime` explicitly scan its `TowerContainer` for towers itself,
  after its own managers exist. (2) `HudController`, as a child of the
  `MapRuntime` node, tried to read `_mission.Supply.Balance` in its own
  `_Ready()` before `MapRuntime._Ready()` had created that ledger — fixed
  by deferring the HUD's first refresh via `Callable.From(RefreshAll)
  .CallDeferred()`.
- **Status:** Active as a standing caution, not a one-off fix. Any new node
  that reads state from an ancestor inside its own `_Ready()` needs one of:
  (a) have the ancestor push data down after its own setup completes
  (what `MapRuntime` does for towers), (b) defer the read with
  `Callable.From(...).CallDeferred()` (what `HudController` does), or
  (c) read the state lazily (on first real use) rather than eagerly at
  `_Ready()` time.

### D22 — Per-tower lifetime damage attribution deliberately deferred out of M2
- **Decided by:** Claude
- **Context:** GDD §13.5 (tower inspection panel) and §12.9 (post-mortem
  panel) both want "damage per Supply invested" / "most and least effective
  tower," which requires knowing *which tower* dealt each point of damage.
  `EnemyDamagedEvent` currently carries the enemy, the amount, the
  multiplier, and the damage type — not a reference to the shooter.
- **Decision:** shipped M2's UI without this attribution rather than
  threading a "damage source" reference through `TowerController` →
  `EnemyController.ApplyDamage` → `EnemyDamagedEvent` → `Projectile` as
  well (projectiles need it too, since they resolve damage on impact, not
  at fire time). Both panels show everything else from GDD's spec; this one
  field is visibly marked "not yet shown" in each panel's code comments
  and in `docs/PROGRESS.md`, rather than silently faked with a placeholder
  number.
- **Rationale:** this touches the same combat call chain that's already
  been edited twice this session (once to drop `isSpotted` as a caller-
  supplied parameter, once to add `DamageType` to the event) — a third
  pass late in an already very large session raised the odds of a subtle
  regression more than the missing feature was worth right now.
- **Status:** Open. Flagged as the first thing worth doing in M3, since
  it's a small, well-understood change (add a nullable `object Source` to
  `EnemyDamagedEvent` and `ITargetable.ApplyDamage`, have `TowerController`
  pass `this`, have each panel accumulate `Dictionary<TowerController,
  float>`).

### D23 — M2 systems verified via smoke run; ability hotbar and Command Post aura not numerically re-checked
- **Decided by:** Claude (verification note, not a decision)
- **Context:** Ran the same headless smoke-test approach as D19 with all
  M2 systems wired into `scenes_root/mission.tscn` (a Command Post placed
  next to the machine gun; HUD, wave preview, tower inspection panel,
  post-mortem panel, and damage-number feedback all active). 25 simulated
  seconds, zero errors, combat behavior (kills, damage-table math) still
  matches D19's earlier confirmation.
- **What this does and doesn't prove:** confirms nothing crashes and the
  core loop still behaves correctly with the M2 systems present. Does
  **not** confirm the Command Post's aura numbers are exactly right (+12%
  range / +8% rate of fire is too subtle to spot in a fire-event log by
  eye), and does **not** exercise the three abilities at all — nothing in
  the test scene triggers them yet, since there's no hotbar UI wired to
  `MapRuntime.ActivateAbility(...)` (see `docs/PROGRESS.md` #17).
- **Status:** Open follow-ups: (1) a targeted check of the Command Post's
  numeric effect, (2) wiring the ability hotbar and confirming all three
  abilities actually fire.

### D17 — Testing framework (GoDotTest vs. GUT) not yet chosen
- **Decided by:** Unresolved
- **Context:** GDD §15.7 names both as acceptable, headless-runnable
  options. M0 prompt 3/4's unit tests (EventBus, ObjectPool, damage
  resolver) haven't been written yet — blocked on D13 (need a working C#
  Godot editor to add the NuGet package and confirm the headless test
  runner works) before committing to one.
- **Status:** Superseded by D25.

### D24 - Initial repository commit excludes machine-local agent settings
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-02
- **Decision:** The initial baseline commit includes the Godot conversion,
  authoritative docs, and the preserved legacy prototype, but excludes
  `.claude/settings.local.json` as machine-local permissions/configuration.
  The file is now ignored at the repository root.
- **Rationale:** It contains local tool allow-lists for the superseded Phaser
  workflow and is not part of the game or its reproducible build.
- **Status:** Active.

### D25 - GoDotTest selected for the C# unit-test suite
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Use Chickensoft GoDotTest in the existing Godot project,
  pinned to package version 2.0.42. The boot scene routes
  `--run-tests=CoreTests` into the headless test runner and normal launches
  continue to the mission scene.
- **Rationale:** It is C#-first, runs inside the Mono Godot process, and
  supports headless command-line execution without introducing a second test
  project or a separate engine/runtime.
- **Status:** Active.

### D26 - Universal ability hotbar uses a text-first prototype control
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Implement the three universal abilities in one programmatic
  bottom-right `AbilityHotbar` control. Buttons show the CP cost and live
  cooldown, keys 1–3 select or activate abilities, point abilities resolve
  on the next battlefield click, and Emergency Repair activates immediately.
- **Rationale:** This satisfies the current GDD hotbar interaction while the
  prototype still uses geometric placeholders. Keeping the control text-first
  preserves readability and avoids inventing icon art before the M3 visual
  language pass.
- **Status:** Active.

---

## How to add to this log

Append, don't rewrite history. One entry per decision: what was decided, who
decided it, why, and a link to the GDD section if one exists. Mark status as
**Active**, **Superseded by D_n_**, or **Open** (unresolved / needs a
decision). If a later decision reverses an earlier one, add a new entry and
update the old one's status rather than editing the old entry's content.
