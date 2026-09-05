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
- **Status:** Superseded by D29. Flagged as the first thing worth doing in M3, since
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
- **Status:** Superseded by D29. Its follow-ups: (1) a targeted check of the Command Post's
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

### D27 - T3 mortar is added to the reusable mission smoke scene
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-02
- **Decision:** Add the GDD-specified T3 Field Mortar as a data resource,
  pooled mortar-shell scene, and placed tower in `scenes_root/mission.tscn`.
  Point-targeted fire events are logged as ground-point events rather than
  assuming every tower shot has a live unit target.
- **Rationale:** M2 prompt 15 requires T3's densest-cluster path to be
  exercised, and the existing debug logger had an invalid single-target
  assumption that became visible as soon as the mortar was placed.
- **Status:** Active.

### D28 - User art-design document is the visual asset reference
- **Decided by:** User
- **Date:** 2026-09-02
- **Decision:** Treat `docs/FRONTS OF WAR ART DESIGN.md` as the active visual
  direction and placeholder-art reference for Fronts of War. Do not generate
  or integrate placeholder art until the project reaches the M3 art pass;
  notify the User when that gate is ready.
- **Rationale:** The visual language should be established before assets are
  generated, while the current prototype remains focused on readable systems.
- **Status:** Active.

### D29 - M2 closes with authored grey-box content and event-based attribution
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-02
- **Decision:** Close M2 after adding four ground enemy definitions, a
  12-wave authored sequence, formal tests for upgrades/statuses/abilities/
  targeting/content, and per-tower damage attribution through
  `EnemyDamagedEvent.Source`. The post-mortem remains defeat-only until M3
  implements victory and full mission flow.
- **Rationale:** These additions satisfy the M2 grey-box acceptance criteria
  without introducing M3 content or replacing the GDD's primitive prototype
  art strategy.
- **Status:** Active.

### D30 - M3 uses a primitive-art vertical slice with scene-based flow
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-02
- **Decision:** Complete M3 with data-authored Arsenal and B1 boss systems,
  a reusable MapRuntime build/wave progression, separate briefing/loadout/
  results scenes, a post-mortem navigation handoff, and an eight-step
  pause-and-highlight tutorial. Keep the existing `--mission`/headless path
  for direct debugging and leave M4 content work untouched.
- **Rationale:** This satisfies the GDD §19 M3 acceptance checks while
  preserving the GDD's fixed-path, no-micromanagement, data-over-code rules.
  Primitive rendering remains consistent with the existing prototype and
  makes the project ready for the user's planned placeholder-art pass.
- **Status:** Active.

### D31 - Map Planner is an editor-only M3.5 interstitial
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-02
- **Decision:** Implement the catalog browser, authored plan model, geometry
  validation, scoring, deterministic candidate generation, manual editing,
  and accepted-plan export as a Godot EditorPlugin between M3 and M4. Keep
  generated candidates out of runtime mission code and require human review
  before an accepted plan becomes ordinary authored map data.
- **Rationale:** This follows the user's map-planner brief and GDD §§15.6,
  17.1, 18.1, and 19 while preserving fixed authored routes, the eight-map
  launch scope, and the post-M3 one-in-one-out rule.
- **Status:** Active.

### D32 - M4 prompt 27 includes all five remaining universal archetypes
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Treat the numbered M4 implementation ladder as authoritative
  for milestone placement: implement T2, T5, T6, T7, and T8 in M4. The three
  individual roster scope labels that said M5 were corrected to M4. Keep M5
  focused on signatures, air systems, and support enemies.
- **Rationale:** GDD §19 explicitly assigns all five remaining archetypes to
  M4 prompt 27, while the individual labels were internally inconsistent.
  The correction preserves one coherent implementation order.
- **Status:** Active.

### D33 - Nation leans preserve a shared universal roster
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Represent nation identity with six `NationProfile` resources
  that apply small stat leans to shared `TowerDefinition` data. Keep each
  authored lean inside the ±15% envelope and validate the complete roster's
  DPS-per-Supply result against the ±3% parity tolerance.
- **Rationale:** This satisfies GDD §§8 and 19 prompt 28 without creating
  nation-specific behavior scripts or duplicating the universal tower roster.
- **Status:** Active.

### D34 - Siege targeting uses an interface-backed path hold and event
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Let `EnemyManager` obtain `ISiegeTarget` positions through a
  provider, hold E12 at its authored range along the route, and publish a
  typed bombard event. Towers decide locally whether their pad is Enclosed
  and whether the shell is in range.
- **Rationale:** The split preserves low coupling while making Siege
  suppression, Enclosed immunity, and the 11-tile stop point testable without
  direct enemy-to-tower references.
- **Status:** Active.

### D35 - Threat Value uses a review-oriented editor estimate
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Use effective HP × leak cost, with readable air/siege/swarm/
  elite modifiers, as the Wave Editor's Threat Value estimate. Pair it with
  a conservative 50 HP/s reference kill estimate for pacing warnings; these
  values are authoring diagnostics, not runtime combat stats.
- **Rationale:** The GDD defines the inputs and warnings but does not provide
  fixed weights. Keeping the estimate isolated in `ThreatValueCalculator`
  makes future balance review local and prevents editor heuristics from
  changing gameplay.
- **Status:** Active.

### D36 - Initial art pass is environment-only and review-gated
- **Decided by:** User
- **Date:** 2026-09-03
- **Decision:** After M4, organize the art workspace and generate an initial
  ten-asset review set focused on theater, terrain, flavor props, and shared
  presentation. Hold nation-specific units and all enemy art until the user
  reviews that those implementations are correct.
- **Rationale:** This establishes the visual direction and reusable theater
  kit without prematurely locking unit silhouettes or national variants.
  Generated images remain review references until they pass the art spec's
  native-scale, silhouette, contrast, blur, grayscale, and gameplay screenshot
  checks.
- **Status:** Active.

---

### D37 - M5 uses data-authored signature controllers and a shared air/support contract
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Implement the five M5 signatures as separate small controllers
  driven by one exported `SignatureDefinition` resource, with one signature
  registered per mission. Use a shared air-corridor resource for E8, extend
  the target contract with concealment/reveal state and damage attribution,
  and keep E9/E10/E11 mechanics on `EnemyController` partial support logic.
  Use primitive, redundant telegraphs until the held nation art review is
  complete.
- **Rationale:** This satisfies GDD §§8.2, 10.2, 15.1, and 19 prompts 31–38
  without duplicating six balance systems or introducing nation-specific unit
  art. Where the GDD leaves a value open, RAF level damage rises 20% per
  level, Recon's speed aura uses a 4-tile radius, and a map owns one straight
  authored air corridor; these are isolated data values for later playtest
  tuning. Signatures retain their specified finite charges, caps, cooldowns,
  range restrictions, and counterplay limits.
- **Status:** Active.

### D38 - Art uses catalogued family slots with placeholder-first resolution
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Add a JSON art catalog that maps every inventory family to a
  stable production directory, filename pattern, placeholder type, and
  review status. Add reusable SVG placeholder textures and an
  `ArtAssetSprite` slot scene that resolves a placeholder by default and an
  approved production path only after an explicit catalog/status opt-in.
  Keep the current gameplay primitives as the default until art is approved.
- **Rationale:** The art specification defines production ranges and family
  kits rather than one fixed file per future variant. A family-level catalog
  preserves those ranges while giving artists and agents stable paths. The
  explicit approval gate prevents the ten generated review images from
  silently becoming production art and keeps the user's held nation/unit/
  enemy review boundary intact.
- **Status:** Active.

### D39 - The initial ten art images are direction-approved, not runtime-approved
- **Decided by:** User
- **Date:** 2026-09-03
- **Decision:** Treat all ten generated art images as successful art-direction
  references. Keep each image's asset status at `REVIEW` until its specific
  gameplay/UI placement, native scale, readability, and replacement role are
  confirmed. Some may remain references rather than ship as runtime assets.
- **Rationale:** The images establish a visual north star without forcing
  every generated composition into a role it may not fit. This preserves the
  art acceptance gates and the placeholder-first workflow.
- **Status:** Superseded by D40.

### D40 - The initial art images are approved for conditional integration
- **Decided by:** User
- **Date:** 2026-09-03
- **Decision:** Approve the jungle foliage, Western Europe farmhouse, Western
  Europe farm-supply cluster, all six terrain/route images, and the commander
  map-table frame for their catalogued roles. Terrain and route images must
  pass an adjacency and path-continuity check with neighboring tiles before
  placement. The map-table frame is approved for command-table presentation
  UI such as menu, briefing, or mission select, pending layout checks.
- **Rationale:** These assets fit the established art direction and are
  promising production candidates. The adjacency condition protects route
  readability and prevents attractive standalone tiles from creating broken
  map seams or ambiguous paths. The UI condition protects the frame's neutral
  center space and keeps it from being forced into an unsuitable battlefield
  role.
- **Status:** Active.

### D41 - Approved art enters presentation and Bocage environment layers
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Use the approved command-table frame in the briefing scene and
  use the approved Western Europe ground, hedgerow, farmhouse, and farm-supply
  cluster in the Bocage Crossroads environment layer. Keep terrain/route art
  in a separate six-tile adjacency review scene and do not mix theaters in
  the production mission. Keep gameplay units and tower logic primitive-first.
- **Rationale:** This gives the selected art a real, reversible integration
  point while respecting the user's terrain-connectivity condition and the
  art spec's rule that routes remain readable. Separating the test board from
  the mission avoids treating visually attractive but unvalidated cross-
  theater joins as final map construction.
- **Status:** Active.

### D42 - Environment generation uses a shared style lock and fixed route sockets
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Maintain one copy-ready prompt queue with a canonical project
  style lock, one palette/reference anchor per theater, and an individual
  numbered prompt plus exact filename for every active inventory output.
  Route tiles are 1024×1024 with 256 px sockets centered on named edges and
  use ten standard topologies per theater; each theater also receives ten
  material-transition tiles. Keep tower, nation-unit, and enemy identity
  prompts on hold pending their requested implementation review.
- **Rationale:** Standalone review images from different theaters cannot form
  a production route family even when their edges happen to align. A fixed
  mechanical socket contract makes adjacency testable, while the shared style
  lock and theater-specific anchors preserve visual consistency across many
  separately generated assets without erasing regional differences.
- **Status:** Active.

### D43 - Generated route families use review-only loading and normalized edge caps
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Generate the Western Europe route family from five visual
  anchors and derive rotational companions mechanically. Normalize every
  connected edge with the same feathered cap: the outer 32 pixels are exact
  matches, followed by a 128-pixel blend into each generated interior. Add an
  explicit `AllowReviewAsset` opt-in to art slots and keep all ten entries at
  `REVIEW` in a dedicated closed-loop inspection scene.
- **Rationale:** Separate image generations produced useful painted interiors
  but inconsistent route widths at tile boundaries. Shared edge caps make
  adjacency deterministic without pretending the remaining internal blends
  are production-approved. Review-only loading preserves the existing rule
  that unapproved art cannot silently replace gameplay visuals.
- **Status:** Superseded by D44.

### D44 - Route gameplay corridors are a separate layer from painted terrain
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Do not bake a broad shared corridor into each full painted route
  tile. Treat terrain art as the varied geographic base layer and render the
  gameplay route/shoulder as a separate same-theater layer assembled from
  fixed topology masks and a shared route material. The route layer uses a
  centered 256-pixel edge socket, fixed road and shoulder widths, and a soft
  handoff into the tile interior. The current ten Western Europe v01 images
  remain `REVIEW` references only; they are not promoted to production route
  art until they are re-authored or composited through this layered contract.
- **Rationale:** Exact outer-edge matching alone still leaves visible repeated
  interior bands. Replacing a large rectangular strip creates a different
  seam, while independent full-tile generations cannot guarantee route width
  or shoulder continuity. Separating gameplay geometry from geographic art
  protects the GDD's sacred Layer 1 map geometry and permits arbitrary tile
  adjacency without erasing theater-specific texture and variation.
- **Status:** Superseded by D45.

### D45 - Each route topology gets unique interior art behind a shared socket contract
- **Decided by:** Codex under standing delegation
- **Date:** 2026-09-03
- **Decision:** Keep route gameplay geometry separate from terrain, but give
  each route topology its own painted transparent overlay: straight, corner,
  T-junction, cross, and entry. Only the edge socket contract is shared: the
  centered 256-pixel road opening, fixed road/shoulder widths, and a short
  clean continuation beyond the tile border. Topology-specific art owns the
  interior curve and junction so meeting points are painted once rather than
  produced by overlapping repeated straight strips.
- **Rationale:** The shared branch material made corners and intersections
  visibly patterned and created unattractive overlaps. Unique topology art
  preserves the user's desired variation while the data-authored socket mask
  still guarantees map-independent adjacency.
- **Status:** Active.

---

### D46 - Headless detection uses DisplayServer, not an OS feature tag
- **Decided by:** Claude under standing delegation
- **Date:** 2026-09-03
- **Context:** `Boot` routed headless launches straight into the mission and
  `TutorialController` skipped its pause-and-highlight cards when
  `OS.HasFeature("headless")` was true. Godot 4.7 has no `headless` feature
  tag (verified with a throwaway `SceneTree` script: `has_feature` returned
  false while `DisplayServer.get_name()` returned `headless`), so every
  headless smoke run since M3 sat on the briefing screen or paused behind the
  tutorial and printed nothing. The M4/M5 "smoke run" claims in
  `docs/PROGRESS.md` were therefore not exercising the mission.
- **Decision:** Detect headless mode with `DisplayServer.GetName() ==
  "headless"` everywhere. The canonical smoke command is now
  `godot --headless --path . --fixed-fps 60 --quit-after 5400` (90 simulated
  seconds; `--fixed-fps` makes each frame advance one fixed step so the run
  is deterministic and fast). A run must print zero error/exception lines and
  at least one `[kill]` line.
- **Status:** Active. Any future headless-only branch must use the same check.

### D47 - Signature charge regeneration starts only after a charge is missing
- **Decided by:** Claude under standing delegation
- **Date:** 2026-09-03
- **Context:** The M5 GoDotTest suite had never been run (the previous
  session only had the non-.NET Godot binary). Running it on the Mono build
  exposed two failures. `RafScrambleController` kept its regen timer at zero
  while full, so the first tick after spending a charge immediately refunded
  it; the GDD (§8.2.2) wants one charge per 22/18/14 s.
- **Decision:** A regeneration cycle begins when a charge is first missing and
  waits the full authored interval before granting. The second failure was a
  test-isolation problem, not a gameplay bug: the minefield-vs-concealed check
  registered the recon with the same `EnemyManager` as an earlier Escort whose
  shield pool absorbed the blast (correct behaviour per §5.6). The test now
  gives the recon its own manager. All 32 tests across the five suites pass
  on the Mono build.
- **Status:** Active.

### D48 - Tower placement runs through a data-declared scene and a plain placement service
- **Decided by:** Claude under standing delegation (implemented by a Sonnet
  worker under the lead's brief)
- **Date:** 2026-09-03
- **Context:** Nothing consumed `BuildPadClickedEvent`; the mission ran only
  on pre-placed grey-box towers, so the GDD §4.2 "full build/upgrade/sell
  loop" was missing. Four archetypes (T2, T5, T6, T7) had data but no scene.
- **Decision:**
  - `TowerDefinition.ControllerScene` (mirroring `EnemyDefinition`) names the
    scene the build bar instantiates; every archetype `.tres` declares it.
    T8 Minefield leaves it null because §7.5 makes minefields free-placement
    on path segments — that UI is still open.
  - `TowerPlacementService` (plain C#, owned by `MapRuntime`) is the single
    place that spends Supply, instantiates, registers (T9 goes to
    `CommandPostManager`), marks the pad, and remembers the pad→tower link so
    selling frees the pad. `BuildPad` stays a dumb presentation node.
  - It writes `Position = pad.GlobalPosition` before the node enters the
    tree, which is only correct while `TowerContainer`/`CommandPostContainer`
    sit at the world origin (true of every scene today). A map that nests
    those containers under a transformed node must convert first.
  - The six-tower loadout lives in `MissionSession.Loadout` as resource paths
    (static state must survive scene changes); the default is the GDD's
    recommended US Mission 1 set. §13.3's drag-and-drop loadout screen is
    still deferred.
  - Build-bar layout (bottom-centre, x 340–720 at the default 1152×648
    viewport) is a judgment call to clear the build-phase label and the
    ability hotbar; revisit when the map-table frame UI lands.
  - Known gap carried forward: `CommandPostController` publishes no click
    event, so a placed Command Post cannot be inspected or sold yet.
- **Status:** Active.

### D49 - Data Validator scope and the first bug it caught
- **Decided by:** Claude under standing delegation (implemented by a Sonnet
  worker under the lead's brief)
- **Date:** 2026-09-03
- **Decision:** GDD §19 prompt 45 is implemented as a plain-C# validator
  (`src/Debug/DataValidator*.cs`) with two front doors that call the same
  function: the `addons/data_validator` editor menu item (Project ▸ Tools ▸
  Validate Data) and `godot --headless --path . --validate-data`, which exits
  1 on any error. `tools/Run-HeadlessChecks.ps1` chains build → every
  GoDotTest suite (discovered by scanning `tests/`) → validator → the D46
  smoke run, for manual pre-commit use (hook wiring is documented, never
  auto-installed).
  - Duplicate Ids are checked in one pooled namespace across tower, enemy,
    nation, signature, arsenal, and friendly-unit definitions.
  - `NationProfile.SignatureId` must resolve against the union of
    `SignatureDefinition.Id` and `ArsenalDefinition.Id`.
  - Missing L3/L4 branch data is a **warning** while the four VS towers are
    still un-authored; it becomes an error once all nine archetypes carry
    both branches (see D50).
- **First catch:** `united_states.tres` pointed at `arsenal_of_democracy`
  while the Arsenal resource's Id was `us_arsenal_of_democracy`. The Arsenal
  side was renamed to `arsenal_of_democracy`, matching the un-prefixed ids of
  the other five signatures. Nothing resolved the id at runtime yet, so the
  bug was dormant.
- **Side note:** a real editor scan surfaced a pre-existing
  `InvalidCastException` in `BalanceDashboardDock.LoadProfiles()` during the
  initial C# domain load (the Wave Editor already guards the same case with
  a generic-Resource fallback). Open, low priority.
- **Status:** Active.

### D50 - VS towers carry full L1-L4 data; upgrade costs round half up in integer hundredths
- **Decided by:** Claude under standing delegation (data authored by a Sonnet
  worker under the lead's brief)
- **Date:** 2026-09-04
- **Context:** T1/T3/T4/T9 only had L1 (sometimes L2) stat blocks and no
  branches, so `TowerUpgradeController.CurrentStats()` dereferenced a null
  branch at L3 — upgrading any vertical-slice tower to level 3 through the
  inspection panel would have crashed a live mission. The Data Validator's
  branch warning (D49) exposed it.
- **Decision:**
  - All nine archetypes now author L2 plus two `TowerBranch` sub-resources
    (L3/L4) from GDD §6 using only existing `TowerStatBlock` fields:
    Sustained Fire / Suppressive Fire (T1), Barrage / Smoke Rounds (T3),
    Sabot Rounds / Rapid Loader (T4), Forward Observer / Logistics Depot
    (T9). Branch effects the stat block cannot express are approximated
    (e.g. Sabot piercing as a small blast radius; the Forward Observer's
    Spotted marking is authored as data but `CommandPostController` does not
    yet consume it). The validator now treats a missing branch as an error.
  - The inspection panel offers both branch buttons at the fork (§13.5).
  - `UpgradeCost` computes in integer hundredths with half-up rounding.
    The old `MathF.Round` (banker's rounding on float products) produced
    172 for T3 L3 and 472 for T9 L4 where §7.4's table says 173 and 473;
    the GDD wins. Doctrine cost multipliers are folded in before rounding.
- **Status:** Active.

### D51 - Doctrines are one passive schema plus six shared ability behaviours
- **Decided by:** Claude under standing delegation (implemented by a Sonnet
  worker under the lead's brief)
- **Date:** 2026-09-04
- **Decision:** GDD §19 prompt 39 is implemented as `DoctrineDefinition` =
  one `DoctrinePassive` row (neutral-by-default multipliers gated by optional
  archetype / pad-tag / terrain-tag filters) plus one `DoctrineAbility` whose
  `Kind` is one of PointBlast, LineBlast, AuraBuff, SpawnFriendly,
  InstantRefund (utility, with a closed `UtilityId` set), StatusApplication.
  All 18 doctrines are `.tres` rows under `assets/data/doctrines/`; no code
  branches on a doctrine id (`DoctrineTests` asserts the closed sets).
  `DoctrineSystem` recomputes passive multipliers every tick, the same way
  Command Post auras work, and the doctrine ability is hotbar slot 4 / key 4
  in a sibling control (`DoctrineAbilitySlot`).
  - Every `LineBlast` mode (drawn line, path segment sweep, from map edge)
    resolves through the existing path-corridor targeting rather than
    free-form geometry, keeping it one shared behaviour.
  - Numbers the GDD leaves open: cooldowns by CP cost (3→25 s, 4→20 s,
    5→35 s, 6→40 s, anchored on Artillery Strike's 4 CP / 20 s); line
    blasts deal 30 damage per CP over a 6×2-tile corridor; Concentrated
    Fire's radius is 7 tiles (Rally's). Enemy-targeted abilities pick the
    nearest living enemy within 2 tiles of the click.
  - `MinefieldExtraCharges` and `MinefieldCapBonus` share one code path;
    the GDD describes the same "+3 charges" outcome for both.
  - Inert until their hooks exist: `TerrainTagFilter` (Desert Rats — maps
    carry no terrain tags), `RelocationFree` (Celere — Italy's national
    relocation mechanic is unbuilt; the Redeploy ability's own relocation
    works), `SuppressionImmunityRadiusBonusTiles` (Fortified Line).
  - Doctrine choice is United States-only for now because no nation picker
    exists; `DoctrineSystem.LoadDoctrine` takes the nation id so that
    changes when §13.2 lands.
- **Status:** Active.

### D52 - Prompt 41 (progression/save) taken before prompt 40 (map gimmicks)
- **Decided by:** Claude under standing delegation
- **Date:** 2026-09-04
- **Context:** `CLAUDE.md` §4 asks for ladder order and a logged reason for
  any deviation. After prompts 39 and 45 the remaining budget for this
  completion pass covered one more system.
- **Decision:** Build prompt 41 (stars, unlocks, mastery, versioned save,
  plus prompt 43's Null platform service) ahead of prompt 40. Gimmicks are
  per-map data whose maps (2–8) do not exist yet, so they would ship as
  untestable systems on a single prototype map; progression and a
  migration-ready save format are cheap now and expensive to retrofit
  (§12.8), and they make the results screen meaningful. Prompt 40 is next in
  order when work resumes.
- **Status:** Active.

### D53 - Progression, stars, mastery, and a versioned save (prompt 41) plus the Null platform service
- **Decided by:** Claude under standing delegation (implemented by a Sonnet
  worker under the lead's brief)
- **Date:** 2026-09-04
- **Decision:**
  - `SaveSystem` writes one JSON profile at `user://saves/profile.json`
    (schema version 2) atomically via a temp file; a missing or corrupt file
    yields a fresh profile and the corrupt file is moved aside. Migrations
    run as a chain from the file's version; v1→v2 adds `MasteryXp` and
    `TutorialCompleted`. `ProgressionTests` proves a hand-written v1 save
    loads in the v2 build (§19 prompt 41's acceptance check).
  - `MissionDefinition` (§10.4 subset) and `StarObjectiveDefinition` are
    data; Bocage Crossroads is authored as `m01_bocage_crossroads.tres` with
    the star-3 objective "no more than 8 towers". `MissionStatsCollector`
    listens to placement/kill/completion events and hands a snapshot to the
    results screen, which records stars (best-of merge), Faction Mastery XP,
    unlock deltas, and two example achievements, then saves.
  - `UnlockService` is pure over the profile and encodes §9.5 exactly.
    `MasteryService` is cosmetic-only (§12.2 is absolute); its XP base (100),
    difficulty multipliers (0.75 / 1.00 / 1.35 / 1.75), extra-star bonus
    (25%), and ten rank thresholds live in `GameBalanceConfig` because the
    GDD gives the formula's shape but not its numbers.
  - `IPlatformService` + `NullPlatformService` exist now (prompt 43's Null
    half); GodotSteam integration remains external, unstarted work.
  - "Towers built" counts Command Posts. "Won without losing a Defense Line
    point" is evaluated as full integrity at victory, so a mid-mission
    repair back to full would count — acceptable until a per-hit tracker
    exists. `MissionSession` (Core) now references Meta types; a directed
    dependency accepted for the M3 flow's simplicity.
  - Skirmish/Endless completions (prompt 42) must use their own recording
    path so they do not feed `CampaignMissionsCompleted`.
  - Also closed in this pass: a placed Command Post can be inspected,
    upgraded, and sold (it now publishes `CommandPostClickedEvent` and the
    inspection panel works through the shared upgrade controller), removing
    the gap noted in D48. `MapRuntime` is 304 lines; split it when the next
    responsibility lands.
- **Status:** Active.

### D54 - UI overhaul foundation: 1080p canvas, one theme, war-table materials, screenshot verification
- **Decided by:** Claude under standing delegation (User asked for a full
  UI/UX overhaul; the GDD fixes the war-table identity but not the system)
- **Date:** 2026-09-04
- **Decision:**
  - `docs/UI_DESIGN_SPEC.md` is the UI system of record beneath GDD §13 /
    §3.4 and the art doc's §29-31: identity, 1920×1080 zones, materials,
    palette tokens, type scale, icon ids, component variations, per-screen
    specs, motion, accessibility hooks, and the screenshot checklist.
  - Design resolution is 1920×1080 with `canvas_items` stretch and `expand`
    aspect, fullscreen by default (User: the game must fit cleanly on a
    24-inch 1080p monitor and be sized for full-screen play; GDD Pillar 1
    names 1080p as the legibility bar). The mission
    scene gains a `Camera2D` (zoom 1.6) so the authored playfield fills the
    table; world-anchored UI converts through `GetGlobalTransformWithCanvas`.
  - One project-wide `Theme` (`assets/ui/theme/fow_theme.tres`, hand-
    authored text) carries every font, color and style box; screens select
    named type variations and never build ad-hoc `StyleBoxFlat`s.
    `UiPalette` mirrors the tokens for `_Draw` code; `UiIcons` resolves
    monochrome SVG glyph ids and returns null for a missing file so screens
    degrade to text rather than crash.
  - Fonts are vendored OFL faces (Oswald, Barlow, Courier Prime) under
    `assets/art/fonts/` with their licenses; no system fonts.
  - UI materials are derived from the approved ART-ENV-010 frame (paper
    card, teletype strip, wood rails cropped/processed from it) plus one
    procedural brass plate, generated once and committed as PNGs under
    `assets/ui/materials/`; the frame itself is used as a 9-patch backdrop.
    Tower/unit identity art stays on hold per the art inventory; UI glyphs
    for towers and enemies are field-manual symbols, not unit art.
  - `EnemyDefinition.DisplayName` added (data) so the wave strip and reports
    show names, never ids.
  - Dev-only `--screenshot-dir` / `--screen` / `--screenshot-frames` /
    `--skip-tutorial` flags (`src/Debug/ScreenshotCapture.cs`, routed by
    `Boot`) capture the real viewport so UI work is verified by looking at
    it; they need a window and are not part of `Run-HeadlessChecks.ps1`.
  - A main menu (§13.1) and pause menu (§13.7) are built as part of this
    pass ahead of ladder prompt 42, because the results/pause flows need a
    destination and the overhaul establishes the visual language they use;
    modes, settings and the codex stay unbuilt and are shown locked.
- **Status:** Active.

### D55 - UI overhaul screens: HUD, cards, flow screens, pause menu, glyph set
- **Decided by:** Claude under standing delegation (User asked for the full
  UI/UX overhaul; D54 fixed the system, this pass builds the screens on it)
- **Date:** 2026-09-04
- **Decision:** Every screen in `docs/UI_DESIGN_SPEC.md` §8 is now built on
  the D54 theme: main menu (new), briefing, loadout, results, the mission
  HUD zones A-G, the tower inspection card with its world-space selection
  overlay, the tutorial card, the pause menu (new), and the post-mortem
  report with its damage-by-type chart. Calls made where the spec or GDD
  left room:
  - **Glyphs are generated, not drawn by hand.** `tools/art/
    generate_ui_icons.py` writes the 79 monochrome SVGs in spec §6 from
    geometric primitives so the set can be regenerated as one unit; they
    import at 2x with mipmaps for clean downscaling. Nation marks are the
    plain abstract shapes the spec allows (star-in-ring, segmented roundel,
    lozenge, shield, chevron, disc-in-hexagon) - no crosses, no real
    insignia (GDD §14.3).
  - **Matchup rows derive from `DamageTable`** (`MatchupRules`): strong is a
    multiplier ≥ 1.0, weak is < 0.5, Anti-Air is Air-only. The UI can never
    disagree with the simulation's table.
  - **Branch cards show a stat diff, not a description.** `TowerBranch` has
    no description field; the fork offers "Damage 45 → 62 · Range …" per
    branch instead, which is the information the GDD's diff preview asks
    for anyway. Adding authored branch copy is a data task for later.
  - **GhostButton is slate-only.** Its muted-cream text is unreadable on
    paper, so tertiary actions on paper (Back, Quit, Skip tutorial, Results)
    use `PaperButton`; spec §7/§8 updated to match.
  - **Time hotkeys live in `TimeControls`.** The spec described Space / P as
    existing; they were not implemented anywhere. Space cycles, + / - step,
    P pauses (GDD §7.7), Esc opens the pause menu after build mode, ability
    targeting and the inspection card have had their chance to consume it
    (unhandled input runs in reverse tree order, so the pause menu is the
    HUD's first child).
  - **Sim exposes what the HUD shows.** `MapRuntime.EarlyCallBonusNow` is the
    single source for the "Call Wave Early +NN" figure (the button credits
    exactly it), `MapRuntime.TotalWaves` feeds "WAVE 4 / 12",
    `AbilitySystem.CooldownSeconds` feeds the radial sweep,
    `TowerUpgradeController.PreviewStats` feeds the diff preview, and
    `TowerController.CurrentTarget` feeds the target line. `MapRuntime` is
    now 312 lines; D53's "split on the next responsibility" note stands.
  - **Screenshot states.** `--ui-state=pause|inspect|postmortem` (dev-only,
    only honoured alongside `--screenshot-dir`) opens overlays that need a
    click so they can be verified from the command line.
  - **Boot enters the main menu** in normal play; `--mission`, `--screen`
    and the headless smoke run are unchanged. Pause Abandon returns to the
    briefing, Quit to Menu to the main menu; every flow screen resumes the
    autoload `TimeController` on entry because it outlives the mission.
  - Damage numbers are counter-scaled by the camera zoom so they read at
    the spec's 18 px. Pooling them (§15.1 principle 5) remains an open gap
    noted in PROGRESS, unchanged by this pass.
- **Status:** Active.

### D56 - Sprite generation prompt reference doc, held with the art it covers
- **Decided by:** Claude under standing delegation (User asked for a
  comprehensive AI-image-generator prompt document for nation/tower/unit
  sprites, scanning existing GDD and art docs)
- **Date:** 2026-09-04
- **Decision:** Wrote `godot-project/assets/art/sprite_generation_prompts.md`,
  alongside the existing `ART_GENERATION_PROMPTS.md`. Calls made where the
  request left room:
  - **Directional frames dropped in favor of single-orientation + rotation.**
    The user's request template asked for N/S/E/W directional frame prompts.
    D3 already establishes this game's units as free-rotating single
    top-down sprites, not 8-direction sheets, so the doc generates one
    canonical orientation per unit/tower plus the small walk/track-roll
    frame set GDD §16.2 actually calls for, and explains the deviation
    in-document rather than silently dropping the user's template shape.
  - **File paths follow the reserved catalog schema exactly**
    (`towers/national/{nation}/{archetype}/...`,
    `units/national_skins/{nation}/{unit_family}/...`,
    `enemies/archetypes/{nation}/{archetype}/...`) already defined in
    `art_asset_catalog.json`'s `held.*` entries, rather than inventing a new
    layout.
  - **Marked HOLD, matching the existing gate.** Every category this doc
    covers (nation tower art, tower upgrade states, insignia, signature
    towers, friendly/enemy unit skins) is already `status: HOLD` in the
    catalog pending the user's implementation review. The new doc carries an
    explicit banner not to generate or integrate from it until that review
    clears — it's a ready-to-use reference, not a green light.
  - **Only prompted enemy/nation variants the GDD already names.** Several
    enemy archetypes (E3, E4, E9-E12) have no nation-specific variant names
    yet in GDD §10.2; the doc lists what exists per nation and explicitly
    flags the gaps rather than inventing new unit names.
- **Reference:** GDD §16.1-16.3, §8.2, §10.2, §14.3; `FRONTS OF WAR ART
  DESIGN.md` §3, §9-10; `art_asset_catalog.json` `held.*` entries.
- **Status:** Active (as a held reference document — not wired into any
  generation or integration pipeline).

### D57 - Tower sprites split into a static emplacement layer plus a rotating turret layer
- **Decided by:** User (asked directly whether the gun should visually
  rotate to track its target, offered as a choice after the first generated
  tower image fused the sandbags and gun into one piece)
- **Date:** 2026-09-04
- **Decision:** Every T1-T7 tower archetype (the seven that actually fire)
  is now prompted as two separate images instead of one: a static
  **emplacement layer** (sandbags/mount/carriage, never rotates, one image
  shared across all 4 tower levels) and a **turret/weapon layer** (cropped
  at its mount point, rotates in-engine to face the target, needs a base
  state for L1-L2 and a branch state for L3-L4). This reuses GDD §16.3's own
  reasoning that "the base emplacement... is shared and only the weapon
  changes" between levels - the split was already implied, just not spelled
  out for rendering. T8 (Minefield, a triggered trap) and T9 (Command Post,
  which GDD says "never shoots") keep a single fused image; two nations'
  T9s with a described rotating radar dish (Britain, Germany) may
  optionally get the same split later as a cosmetic nicety, not a
  requirement. `sprite_generation_prompts.md` §1.7 documents the layering
  contract and pivot-alignment rule; all six nations' tower tables in §2 and
  the file-path contract in §4 were rewritten to match. At the time of this
  entry, no code consumed this yet (`TowerController.cs` had no
  sprite/rotation wiring) — D58 wired it up the same session.
- **Reference:** GDD §16.2-16.3; supersedes/refines D56's single-image
  assumption for T1-T7 only.
- **Status:** Active.

### D58 - Turret rotation wired into TowerController, reusing TurnRateSeconds
- **Decided by:** Claude under standing delegation (User confirmed the D57
  split, then asked directly to implement the rotation, not just document
  it)
- **Date:** 2026-09-04
- **Decision:** `TowerController.SimTick` now rotates a tower's `Turret`
  child node (if present) toward its current target every tick, capped by a
  turn speed derived from the existing `TowerStatBlock.TurnRateSeconds`
  stat. Calls made where GDD/D57 left room:
  - **`TurnRateSeconds` reinterpreted for a second purpose.** It already
    existed as a mechanical "time to acquire a new target" stat (GDD §6);
    there was no separate "visual turn speed" stat and adding one would
    duplicate a number the balance sheet already tunes. Reused it as "time
    for a full 180 deg swing" (`maxRadiansPerTick = PI / TurnRateSeconds *
    delta`), so the same GDD-balanced number (e.g. T1's 0.4s Fast vs T4's
    1.1s Slow) now also drives how fast the art turns - consistent with the
    corner case GDD already calls out for T4 ("punishes placement on tight
    corners where targets pass quickly").
  - **Indirect-fire archetypes (T3, T7) needed a second aim source.** They
    target a ground point via `DensestCluster`, not `_currentTarget` -
    added a `_turretAimPoint` field set from that point so their turret
    still tracks something. Direct-fire archetypes track the live
    `_currentTarget.GlobalPosition` instead of a cached point, so the
    turret continues following a moving target between shots rather than
    aiming at a stale snapshot.
  - **Rotation always runs on SimTick, not `_Process`.** Matches this
    class's existing rule (GDD §15.1 principle 4, and the file's own
    header comment) that everything here moves on GameLoop's fixed tick,
    never a variable-rate callback - keeps turret motion deterministic
    alongside targeting/firing rather than adding a second, undocumented
    update path.
  - **Retrofitted the 7 existing tower placeholder scenes**
    (`scenes/towers/tower_*.tscn` for T1-T7) rather than waiting for real
    art: found that 6 of the 7 already separated their weapon piece
    (`Barrel`/`BarrelA`+`BarrelB`/`Scope`) from the base `Visual` polygon -
    wrapped each in a new `Turret` `Node2D` parent so `GetNodeOrNull
    <Node2D>("Turret")` finds it. T1's scene was missing a weapon piece
    entirely (just a flat square + label) - added one to match the other
    six. T8/T9 have no `Turret` node (by design, D57), so rotation is a
    silent no-op for them via the existing null check.
  - **Verification:** `dotnet build` succeeds. Could not run the
    `Chickensoft.GoDotTest` suite (`BuildTests`, `CoreTests`,
    `DoctrineTests`) to confirm at runtime - no `godot` binary is present
    in this environment at all (not even the non-.NET build noted in D13).
    Reviewed the relevant tests by hand instead: `CoreTests` constructs a
    bare `TowerController` with no child nodes and calls `_Ready()`
    directly, which is safe (`GetNodeOrNull` returns null, and
    `UpdateTurretRotation` no-ops on a null `_turret`); `BuildTests` places
    real towers through the actual `.tscn` files and only asserts
    Supply/registration/pad state, none of which this change touches.
    Flagging this as reviewed-not-run rather than claiming a passing test
    run.
- **Reference:** GDD §6 (TurnRateSeconds/turn rate per archetype), §15.1
  principle 4; refines D57.
- **Status:** Active. Runtime verification via the Godot test suite is
  still open - re-run `BuildTests`/`CoreTests`/`DoctrineTests` on a machine
  with a working Godot binary before trusting this beyond the code review
  above.

### D59 - Writing voice: WW2 dispatch prose with a 15% Catch-22 register
- **Decided by:** User (asked for a WW2-era press-prose style guide with a
  "10-15%" Catch-22 satirical influence); boundaries below decided by
  Claude against GDD §14
- **Date:** 2026-09-04
- **Decision:** `docs/WRITING_STYLE_GUIDE.md` now governs all player-facing
  text - briefings, codex, results copy, achievements, tutorial, UI. Base
  register is 1940s wire-dispatch prose; roughly 15% is a dry Catch-22
  seam. Calls made where the request met GDD policy:
  - **The satire targets the institution, never the combatants.** GDD
    §14.3 bans national caricature outright and says in as many words that
    no nation's units are "cowardly, fanatical, primitive, or comic," and
    §10.1 extends the no-stereotyping rule to codex text. So the permitted
    targets are paperwork, requisitions, doctrine-as-document, command
    abstraction, and supply arithmetic; nations, soldiers, units,
    casualties, civilians, and anything in §14.3's banned list are off
    limits as material. This is also what Heller actually satirizes, so the
    constraint costs the voice nothing.
  - **Radio chatter carries zero voice.** GDD §14.3 requires barks be
    "short, generic, tactical" and operational only. Rather than let the
    style guide quietly contradict that, the register table sets barks (and
    tower/enemy names, tutorial prompts, and UI labels) to no-voice,
    no-satire, and says so explicitly.
  - **Clarity outranks voice.** GDD §13.10's bar is that a player learns
    the whole interface in Mission 1; the guide's final checklist item is
    "would removing the voice make it clearer? Then remove the voice."
  - **Dose is defined per surface, not globally.** "10-15%" is unusable as
    a per-sentence rule, so it became a frequency budget: one satirical
    beat per briefing, one line per codex entry, never twice in a row, with
    results/post-mortem copy as the safest home for it.
  - **Samples live in HTML comments** per the user's request, including a
    deliberate over-dose/correct-dose calibration pair, so they read as
    reference rather than as approved shipping strings.
- **Reference:** GDD §14.2-14.3, §10.1, §9.2 (briefing length), §13.10;
  `docs/UI_DESIGN_SPEC.md` §5 (Courier Prime carries briefing/report text).
- **Status:** Active, except the 85/15 dose split, superseded by D63
  (75/25). No existing strings have been rewritten to match yet - the one
  authored briefing in `m01_bocage_crossroads.tres` still predates this
  guide.

### D60 - Register the complete non-held environment art queue as review assets

- **Decided by:** User (requested flavor, vegetation, and architecture generation
  without touching the parallel UI work); Codex (kept the assets review-only)
- **Date:** 2026-09-04
- **Decision:** Generate every active numbered vegetation, architecture, flavor
  prop, and authored-cluster prompt in ART_GENERATION_PROMPTS.md with the
  built-in ChatGPT ImageGen tool. Place each v01 PNG in its prompt-specified
  theater folder, normalize vegetation/props to 512×512 and
  architecture/clusters to 1024×1024 while preserving alpha, and add exact
  item-level catalog records with REVIEW status. Family catalog entries for
  these categories also move from PLACEHOLDER_READY to REVIEW.
- **Why:** This replaces generic environment placeholders with organized,
  traceable candidates while respecting the art spec's requirement that assets
  remain unapproved until native-scale, grayscale, blur, and gameplay screenshot
  checks pass. UI/shared presentation art and held tower, unit, enemy, and
  insignia categories remain outside this pass.
- **Reference:** ART_GENERATION_PROMPTS.md, docs/FRONTS OF WAR ART DESIGN.md
  §§40–50, GDD §14.
- **Status:** Active. Manual gameplay-context art acceptance remains open.

### D61 - Standalone map editor shares the Godot project but uses a developer-only application path
- **Decided by:** Codex under standing delegation (User requested a repository-backed implementation blueprint)
- **Date:** 2026-09-04
- **Decision:** Build the future Fronts of War Map Editor as a dedicated
  developer-only Godot scene and launch path inside the existing
  `godot-project`, backed by a new shared `MapDefinition` Resource saved as a
  text `.tres` under `assets/data/maps/`. Keep `MapPlanDefinition` as the
  normalized M3.5 candidate/interchange model and convert accepted candidates
  into editable production maps. Keep the existing planner plugin during
  migration, then retire its UI after standalone parity. Normal player launch
  remains on `boot.tscn`; the editor flag is accepted only by developer builds
  and the player export excludes the editor scene. A required repository-root
  `Launch-MapEditor.ps1` resolves the Godot project and .NET-enabled Godot
  executable, supplies `--map-editor`, forwards optional arguments, and is the
  normal one-command developer workflow.
- **Why:** The repository has one C# project, shared `res://` resources, a
  stable catalog/planner stack, and no existing runtime `MapDefinition` or
  map loader. A second Godot project would duplicate configuration and import
  state; an EditorPlugin cannot provide the requested standalone workflow.
  The GDD expects authored map resources, fixed deterministic routes, and no
  runtime procedural generation. A separate production model is necessary
  because the current planner model has no terrain instances, art transforms,
  stable object IDs, or complete runtime path metadata.
- **Reference:** `docs/standalone_map_editor_blueprint.md`; GDD §§3.1, 11.2,
  15.1, 15.2, 15.6, 18.1; supersedes the standalone UI/export assumptions in
  `docs/fronts_of_war_map_planner_design_spec.md` while retaining its planner
  algorithms and catalog concepts.
- **Status:** Active.

### D62 - Phase 1 map editor uses debug and export-time isolation
- **Decided by:** Codex under standing delegation (User approved proceeding
  from the reviewed blueprint and specifically requested a launcher)
- **Date:** 2026-09-04
- **Decision:** Implement the standalone map editor's first phase as a
  developer workbench reached only through the repository-root
  `Launch-MapEditor.ps1` and a Debug-only `--map-editor` route in `Boot`.
  The shell reuses the project theme and presents named asset-palette,
  hierarchy, viewport, inspector, diagnostics, and status regions without
  creating or mutating map data. Player separation has three layers:
  `--screen` is now an explicit five-scene allowlist and cannot name the map
  editor; the `Windows Player` preset excludes `map_editor.tscn` and
  `src/Editor/*`; and the editor C# types are enclosed in `#if DEBUG` so they
  are absent from Release assemblies. `Windows Developer` retains all editor
  resources and is documented as a Debug export.
  - **Godot's C# export prerequisite is now repository-owned.** The 4.7.2
    exporter refuses to publish this project without `FrontsOfWar.sln`, so
    that one solution is exempted from the repository's general `*.sln`
    ignore rule and maps Release to Release explicitly.
  - **Existing `EditorPlugin` entry classes are editor-only.** The four
    plugin entry scripts now use Godot's documented `#if TOOLS` guard. This
    resolves their pre-existing Release compile failure without removing the
    docks or changing editor behavior.
  - **Verification:** Debug and Release builds both succeed with zero
    warnings/errors; CoreTests passes 17/17; headless editor startup succeeds;
    launcher success/failure paths pass; the 1920×1080 editor capture passed
    visual inspection. A generated player PCK contains the main menu but no
    editor scene, source path, or compiled `MapEditorController` marker. Full
    Windows executable export is not runnable on this machine until Godot
    4.7.2 Mono export templates are installed.
- **Reference:** D61; `docs/standalone_map_editor_blueprint.md` Phase 1; GDD
  §15.1 principle 7 and §18.1; Godot editor-plugin and export filtering
  documentation.
- **Status:** Active.

### D63 - Catch-22 dose raised from 15% to 25%
- **Decided by:** User
- **Date:** 2026-09-04
- **Decision:** `docs/WRITING_STYLE_GUIDE.md` §2's mix is now 75% dispatch
  prose / 25% Catch-22 (was 85/15, D59). Read the request as a frequency and
  prominence change, not a tone change, since D59's hard boundaries (GDD
  §14.3: satire never touches a nation, its soldiers, casualties, or the
  banned-content list; no winking; delivery stays deadpan) are policy, not
  taste, and a numeric dial can't move them. Concretely: mission briefings
  go from one late beat to two (situation still stated straight before the
  first one), codex entries from one closing line to one-or-two with room
  to build a beat across them, and results/post-mortem copy is now named
  the surface where the register can carry the whole flavor line. The §7
  checklist's flat "cut to one beat" rule became "cut to §5's budget" so it
  does not re-impose the old 15% ceiling by accident. All five samples in
  §8 updated to demonstrate the new dose, including a second beat added to
  the Bocage Crossroads briefing sample and a loop-back sentence added to
  the Field Mortar codex sample.
- **Reference:** refines D59; GDD §14.3 boundaries unchanged.
- **Status:** Active. Supersedes D59's specific dose numbers; D59's targeting
  rules and register-by-surface structure stand.

### D64 - Terrain grid cell size: one cell = one gameplay tile (64px)
- **Decided by:** Claude under standing delegation (user asked to plan
  Western Europe tileset generation, flagged that "the test tiles for
  terrain are not small enough," and confirmed writing this into the
  blueprint as its Phase 6.1 answer rather than holding for the parallel
  map-editor effort)
- **Date:** 2026-09-04
- **Decision:** `TerrainInstance.Cell` (`standalone_map_editor_blueprint.md`
  §8.2) is one gameplay tile = 64px, matching `GameBalanceConfig
  .TilePixelSize` exactly - no separate terrain-unit converter. This was an
  open gap: the blueprint defined `Cell` as "an integer grid coordinate"
  with no pixel size, deferred to Phase 6.1 ("Define terrain set/socket
  contracts," not yet implemented - Phase 2 is next per PROGRESS.md), and
  §22/the risk table both flagged the terrain connectivity contract as
  unresolved. The only existing terrain-tile precedent was D42's
  1024x1024px environment-art export size, which had never been validated
  as a placement grid - it covers 16x16 gameplay tiles per piece, and
  against the blueprint's own worked example (Bocage Crossroads, 28x18
  tiles) that's barely two placements across the whole map. 64px was
  chosen over a coarser option that would have matched D42's existing
  256px/4-tile route-socket width, because 64px is the only size that
  divides any map's `WidthTiles`/`HeightTiles` with zero rounding
  remainder, since those dimensions are already expressed in that same
  unit. Route pieces are unaffected - they remain larger multi-cell overlay
  composites on the same grid, matching the layered-route work already in
  progress for Western Europe. Written directly into the blueprint (§8.2,
  §6.1's task text, §19 as decision 12, and §22) rather than left as a
  separate side note, since that document is Sol's build target for the
  standalone map editor and needs to be the single source of truth.
- **Reference:** `docs/standalone_map_editor_blueprint.md` §8.2, §19.12,
  §22; refines D42 (that decision's pixel size stands for art export/review
  resolution, not placement grid).
- **Status:** Active as the working answer, written where the map editor's
  builder (Sol) will read it. Not confirmed by Sol directly - flag for
  review if their implementation needs something else before Phase 6 locks
  it in.

## Note on D60-D62 vs D63-D64 ordering

D60-D62 (environment art queue, map editor architecture, map editor Phase 1)
and D63-D64 (Catch-22 dose, terrain grid cell) were written concurrently by
two different agents working this repository at the same time (see
`AGENTS.md`) without visibility into each other's in-flight edits, which
briefly produced duplicate D60/D61 numbers. Renumbered on discovery per this
log's own append-don't-rewrite rule; no entry's content changed, only D63
and D64's numbers and this note were added. If you are an agent writing here
concurrently with another session, re-read the file's true tail immediately
before appending, not a cached view from earlier in your turn.

### D65 - Western Europe route tileset: 2-tile road width, 17-piece topology set, no rotation reuse
- **Decided by:** Claude under standing delegation (user asked for a
  concrete Western Europe tileset plan, flagged seam alignment as the most
  critical requirement and asked whether blending was possible, and asked
  for a full path-topology variety: bends, T's, 4-ways, dead ends, curves)
- **Date:** 2026-09-04
- **Decision:** `godot-project/assets/art/theaters/western_europe/
  ROUTE_TILESET_PLAN.md` is the concrete generation plan for the route
  overlay layer (D44/D45's separate transparent layer over ground
  material), built on D64's 64px cell. Key numbers: route pieces are a
  4x4-cell (256x256px final) footprint generated at 512x512px (2x
  supersample); the socket opening is D45's existing 256px figure
  unchanged, now explicit that this is the *generation-resolution* number,
  giving a **128px (2-tile) final road width** rather than D45's implicit
  4-tile width. 17 pieces cover the full topology: straight x2, a new
  curved-straight variant x2 (answers the "curved paths" request as a
  drop-in alternate at the same socket, not a new socket type), corner x4,
  T-junction x4, crossroads x1 (no orientation - symmetric by definition),
  dead-end/entry x4.
  - **Revised D45's road width, didn't just adopt it.** D45 predates any
    production map having real tile dimensions on record. The map-editor
    blueprint's Bocage Crossroads example (28x18 tiles, from D64's own
    evidence) makes a 4-tile-wide road disproportionate - roughly a
    seventh of the map's width - and GDD's framing of a 1-tile bridge as a
    deliberately narrow chokepoint implies normal roads are only modestly
    wider, not 4x wider. 2 tiles was chosen to fit that evidence.
  - **Rotation stays off the table for route art**, matching D44/D45's own
    finding (rotated/shared branch material read as "visibly patterned"
    with "unattractive overlaps") plus a second, independent reason: art
    spec §36-37 bakes lighting into terrain art with an explicit
    "consistent light direction" rule, so a rotated piece's shadows would
    visibly mismatch its neighbors across a large edge-to-edge surface -
    unlike the free-rotation approach that's correct for small independent
    unit/tower sprites (D3, D64's own layering work).
  - **Existing REVIEW art is kept as a style reference, not discarded.**
    The 8 existing route-overlay files (corners, 2 T's, cross, 1 entry) and
    the WE-ROUTE-001-010 batch already proved the shapes are achievable;
    they're superseded as production dimensions but stay in place to
    re-author from rather than starting blind.
- **Reference:** refines D45 (road width only; the socket/layering/
  unique-topology-art principles stand); builds on D64; GDD's bridge/lane
  width framing (§11.1 M3).
- **Status:** Active as the working plan. Like D64, not confirmed against
  Sol's map editor requirements directly - the 64px cell it's built on
  carries the same caveat.

### D66 - Route tile seams forced by deterministic alpha feather + overlap, not by prompt instruction
- **Decided by:** Claude under standing delegation (user reported that
  previous test maps built from the D42/D43/D45 route tiles had visible
  zigzag seams between tiles)
- **Date:** 2026-09-04
- **Decision:** Route tile edges no longer rely on the image generator
  matching pixels across independently-generated pieces. New script
  `tools/art/Add-RouteSocketFeather.ps1` applies a linear alpha ramp (full
  opacity at `FeatherWidth` px inward, down to 0 at the true edge; corner
  regions use the min of the two applicable edge ramps) to a piece's socket
  edges. Adjacent pieces are placed overlapping by that same width instead
  of flush, so whichever piece draws on top dissolves smoothly into the one
  beneath regardless of how well their painted content actually agrees.
  Default `FeatherWidth` is 96px at the 512px generation canvas (48px
  final), matching `ROUTE_TILESET_PLAN.md` §2. The generation prompt
  template was also corrected to stop asking for pixel-matched edges, since
  that ask was never enforceable and produced false confidence.
  - **Root cause found before proposing a fix.** Read both existing prep
    scripts (`Prepare-WesternEuropeTopologyOverlay.ps1`,
    `Prepare-WesternEuropeRouteMaterial.ps1`, both D42-era). Neither
    performs any blending — they only crop, reposition, and strip
    flattened-background artifacts. D43's "outer 32px exact match, then
    blend" was therefore only ever prompt text asking the generator to
    cooperate; nothing in the pipeline enforced it, which is exactly why it
    produced zigzags once real test maps were assembled from independently
    generated pieces.
  - **Verified, not just written.** Ran the script against an existing
    generated tile (`route_overlay_sunken_lane_ne_v01.png`) and confirmed a
    clean linear alpha ramp by direct pixel measurement (0 / 13 / 64 / 128
    / 191 / 255 at 0 / 5 / 24 / 48 / 72 / 96px from the edge against a 96px
    width) before writing it into the plan as the answer.
  - **`ROUTE_TILESET_PLAN.md` updated in place**, not left inconsistent
    with this fix: §1 now explains the diagnosis and correction plainly
    (including that the plan's own first draft repeated the same
    unenforced claim), §2's socket contract adds feather width and overlap
    placement, §4's prompt template drops the "match pixels" ask, and §6's
    acceptance checklist adds "ran through the feather script" and changes
    the adjacency test from edge-to-edge to overlapping placement.
- **Reference:** `tools/art/Add-RouteSocketFeather.ps1`;
  `godot-project/assets/art/theaters/western_europe/ROUTE_TILESET_PLAN.md`
  §1-2, §4, §6; refines D43 (supersedes its unenforced exact-match claim,
  keeps its "route is a separate overlay layer" and per-topology-unique-art
  principles); builds on D65.
- **Status:** Active. Visually verified on real content: composited two
  actual independently-generated tiles (`route_overlay_sunken_lane_ne_v01`
  + `_wn_v01`) both flush/unprocessed and feathered/overlapping, side by
  side. The flush version reproduces the reported zigzag plainly (a visible
  kink in the hedgerow line, an abrupt road-color shift). The feathered
  version's seam is not detectable at the same crop and zoom - the
  hedgerow line reads as one continuous curve. Comparison images sent to
  the user. Still open: this was a standalone image composite, not yet run
  through the real Godot adjacency-test scene or the new plan's actual
  512px generation size (the test used the existing 1024px-era tiles at a
  proportionally-scaled 192px feather width) - confirm both before treating
  the technique as production-proven end to end.

### D67 - Canonical map documents use tile-space schema-v1 Godot Resources
- **Decided by:** Codex under standing delegation (user approved continuation
  of the standalone map editor blueprint and specifically requested a usable
  launcher or opening manual)
- **Date:** 2026-09-04
- **Decision:** Phase 2's canonical authored map is `MapDefinition`, a text
  `.tres` Resource graph under `assets/data/maps`. Positions and dimensions are
  stored in gameplay tile space (D64); object IDs are globally unique stable
  lowercase strings; rotations use quarter turns where legal; scalable placed
  assets use uniform scale only. Air corridors have an authoring Resource in
  tile units and will convert to the existing pixel-space runtime definition at
  the Phase 3/13 boundary rather than changing runtime semantics early.
  - Schema version 1 is mandatory. Missing, unsupported, future, corrupt, and
    structurally invalid resources fail explicitly; no implicit migration is
    invented before an older production schema exists.
  - Save normalization sorts unordered collections by stable ID/key while
    preserving ordered geometry. Godot's randomized external-resource suffixes
    are canonicalized after temporary serialization so equivalent map graphs
    produce identical source-control text.
  - Save As writes a sibling temporary resource, stages any previous file as a
    backup, then replaces it; validation or serialization failure leaves the
    last known-good resource intact.
  - `MapDocument` owns path/dirty state separately from authored data. The live
    File menu exposes New/Open/Save/Save As/Close and requires an explicit
    Save/Discard/Cancel decision before replacing dirty state.
  - A double-click `.cmd` wrapper and detailed opening manual supplement the
    PowerShell launcher. A distributable Windows executable remains blocked
    only by missing Godot 4.7.2 Mono export templates, not project code.
- **Reference:** `docs/standalone_map_editor_blueprint.md` Phase 2; GDD
  §15.1 principles 1, 3, 4, and 7; builds on D61, D62, and D64.
- **Status:** Active.

### D68 - Phases 3–4 render and mutate through a shared editor command boundary
- **Decided by:** Codex under standing delegation (user asked to continue the
  standalone map editor through Phases 3 and 4)
- **Date:** 2026-09-04
- **Decision:** Map loading is split into `MapRegistry` (ID-to-repository path)
  and `MapLoader` (ID/path loading). `MapSceneFactory` converts a canonical
  `MapDefinition` into a deterministic render snapshot, keeping the viewport
  independent of asset-catalog availability. The viewport owns camera/grid and
  hit testing; `SelectionService` owns the selection set; `MapDocument` owns
  dirty state and delegates every mutation to `IMapEditCommand`/`CommandHistory`.
  - Transform, delete, duplicate, copy, and paste commands use deep map
    snapshots for exact undo/redo and validate the resulting document before a
    command enters history. Fresh stable IDs are generated only for explicit
    user duplication/paste operations.
  - The editor uses authored placeholders for assets until Phase 5 catalog
    integration; this keeps Phase 3 useful for inspecting geometry without
    inventing a second asset-resolution system.
  - The inspector edits only position, rotation, and legal scale fields and
    issues the same commands as viewport tools. Terrain scale and unsupported
    object transforms are refused at the command boundary.
  - Window-close requests are intercepted so dirty documents cannot exit
    without the existing Save/Discard/Cancel decision.
- **Reference:** `docs/standalone_map_editor_blueprint.md` Phases 3–4; GDD
  §15.1 principles 1, 3, 4, 6, and 7; builds on D67.
- **Status:** Active.

### D69 - Catalog, planner, validation, and runtime preview share one map pipeline
- **Decided by:** Codex under standing delegation (user asked to complete the remaining standalone editor phases)
- **Date:** 2026-09-04
- **Decision:** The remaining standalone editor phases use pure shared services
  for catalog queries, tile-snapped placement, terrain rules, path/pad/marker
  authoring, deterministic planner conversion, production diagnostics,
  preferences/recovery, and runtime preview. The editor invokes those services
  through commands; runtime consumes the resulting MapDefinition through
  MapLoader and MapRuntimeDataFactory. A checked-in smoke fixture proves the
  real mission loader handoff, while the existing mission scene remains the
  default when no --map-id is supplied.
- **Why:** This preserves the GDD's data-over-code and low-coupling rules,
  makes generated candidates ordinary editable maps, and prevents the editor
  from growing a second gameplay or asset-resolution implementation.
- **Reference:** standalone map editor blueprint Phases 5–15; GDD §§15.1,
  15.6, 18.1.
- **Status:** Active.

### D70 - Retire the legacy map-planner dock after standalone parity
- **Decided by:** Codex under standing delegation (user asked to complete the remaining standalone editor phases)
- **Date:** 2026-09-04
- **Decision:** Remove the legacy `addons/map_planner` Godot editor plugin and
  its dock/canvas UI, remove its editor-plugin registration, and retain the
  shared `FrontsOfWar.Map.Planning` domain services as the supported candidate
  generation path for the standalone editor.
- **Why:** Phases 5–14 now cover catalog placement, production validation,
  planner conversion, publishing, runtime preview, recovery, and launch. The
  old dock would otherwise leave two competing authoring surfaces and violate
  the Phase 15 migration checkpoint.
- **Reference:** standalone map editor blueprint Phases 15.1–15.4; GDD
  §§15.1, 15.6, and 18.1.
- **Status:** Active.

### D71 - Release completion mandate and reported M3 playtest gate
- **Decided by:** User (scope and playtest confirmation); Codex (execution order)
- **Date:** 2026-09-05
- **Decision:** Audit and execute the remaining GDD launch scope using Astra
  coordination with Luna/Terra/Sol. The User explicitly confirmed that three
  unfamiliar players finished the slice and at least one requested a replay.
  Treat the M3 gate as passed on that report, without claiming agent-observed
  playtest evidence. Track reviewable tasks and acceptance evidence in
  `docs/RELEASE_COMPLETION.md`.
- **Why:** Foundational correctness and truthful verification precede further
  content, despite earlier implementation/completion labels. Repair current
  runtime, save, pooling and build defects before prompt 40/content expansion.
  This short deviation from the ladder reduces regression risk; no new scope
  or cuts are introduced.
- **Status:** Active.

### D72 - Fail-closed verification and separated combat fixture
- **Decided by:** Codex
- **Date:** 2026-09-05
- **Decision:** Verification requires successful native exits and nonempty,
  zero-failure/zero-skip test summaries. Retain the old prebuilt combat layout
  only under `tests/fixtures/combat_smoke.tscn`, reached with developer-only
  `--smoke-test`. Normal missions resolve their MissionDefinition MapId and
  validate all wave/path references before simulation; no prototype fallback.
- **Why:** The old runner falsely passed a failed SDK build. The corrected
  runner then caught a real path_0/prototype-main mismatch that isolated tests
  missed. A combat fixture is useful evidence but is not the player journey.
- **Reference:** GDD §§15.1, 15.7, 19; release ledger R01/R03.
- **Status:** Active.

### D73 - Authored Bocage migration remains a review candidate
- **Decided by:** Codex
- **Date:** 2026-09-05
- **Decision:** Supply the missing `bocage_crossroads` resource with two entries
  merging into one route, 22 pads and hedgerow Enclosed tags. Copy the existing
  twelve-wave composition to a campaign-specific resource and assign second
  spawn groups to the northern entry. Keep the old sequence for regression.
  Use already-approved catalog assets without changing their approval status.
  The new layout stays Review pending visual QA and balance playtests.
- **Why:** The prior mission MapId did not resolve at all. This restores the
  documented map/runtime contract without treating generated candidate data
  or automated checks as human approval of a final shipping map.
- **Reference:** GDD §§11.1, 15.1; release ledger R03/R09/R14.
- **Status:** Active.

### D74 - Bounded pooled enemy and friendly leases
- **Decided by:** Codex
- **Date:** 2026-09-05
- **Decision:** Prewarm authored enemy/friendly scenes before combat and freeze
  their capacities. Queue excess requests FIFO until a lease returns. Track a
  generation on each lease so retained targets cannot damage a recycled unit.
  Reset status, shields, engagement, reveal and path state on reuse.
- **Why:** Pool exhaustion must not allocate mid-wave, drop authored spawns or
  leak previous-unit state. Sol hit its model usage limit after partial edits;
  Astra completed and reviewed that assignment locally. Projectile/effect pool
  coverage still needs its separate R16 audit.
- **Reference:** GDD §15.1; release ledger R05.
- **Status:** Active.

### D75 - Build break from an interrupted settings-system edit; full verification now passes
- **Decided by:** Claude
- **Date:** 2026-09-05
- **Decision:** Picked up the Codex-coordinated release audit (D71-D74) where it
  stopped. The working tree had `dotnet build` failing: the new
  `src/Core/UserSettings.cs` (mid-flight prompt 44/R12 work) declared a
  private `static InputBindingData Key(Key key)` helper in the same class
  where it built `BindingDefinitions` with `Key(Key.P)` literals - the method
  name shadowed the `Godot.Key` enum for the rest of the file, so every
  `Key.P`-style member access failed to resolve (CS0119). Renamed the helper
  to `KeyBinding` (its only call sites); no other file referenced the old
  name. Re-ran `tools/Run-HeadlessChecks.ps1` afterward: build, all 14
  discovered GoDotTest suites (including `RuntimeMapIntegrationTests` 7/7,
  `PlayerFlowPersistenceTests` 6/6, `PoolingTests` 3/3, `CampaignSelectionTests`
  4/4), `--validate-data` (0/0 across 67 resources), and the smoke run (0
  errors, 19 kills) all pass.
- **Why:** `docs/RELEASE_COMPLETION.md`'s evidence log still marked R03/R06 as
  "in progress"/"pending" and did not mention R04/R05's newer suites at all -
  it was written before this uncommitted batch landed and before the build
  broke. The mission's `MissionDefinition.MapId` now resolves to the authored
  `bocage_crossroads` map resource (D73), and a `CampaignSelectionController`
  reachable from the main menu's "Campaign" button already drives nation/
  mission selection (`MissionCatalog`) - both ledger rows were more complete
  than their status text suggested. `src/Core/UserSettings.cs` and
  `src/Meta/PlayerSettings.cs` exist but are not yet called from anywhere
  (`Boot`, pause menu, main menu) and have no dedicated test suite; the
  Settings screen itself, and several §13.8/13.9 fields the GDD lists
  (VSync/frame cap/resolution picker, screen shake, subtitles, damage-number
  and tutorial-hint toggles, confirm-before-sell, targeting-priority
  defaults), are not implemented at all - there is no camera-shake or
  subtitle system yet for those settings to control.
- **Reference:** GDD §13.8-13.9, prompt 44; release ledger R03/R05/R06/R12.
- **Status:** Active.

### D76 - Godot Mono Windows export templates installed; R02 closed with a real .exe
- **Decided by:** User (approved the download); Claude (execution)
- **Date:** 2026-09-05
- **Decision:** Downloaded the official `Godot_v4.7.2-stable_mono_export_templates.tpz`
  (~1.15 GB, from the godotengine/godot-builds GitHub release) and installed
  only the four `windows_{debug,release}_x86_64{,_console}.exe` files this
  project's presets use into `%APPDATA%\Godot\export_templates\
  4.7.2.stable.mono\`, rather than extracting the full archive (which also
  carries Linux/macOS/Android/iOS/web templates this project never exports
  to). Then ran `tools/Build-Windows.ps1 -Preset "Windows Player"` for the
  first time; it produced a real `FrontsOfWar.exe`/`.pck` that boots headless
  with zero errors and carries no `map_editor`/`MapEditorController` string in
  either file.
- **Why:** R02/R13 needed an actual export attempt, not just config
  inspection, to be trustworthy evidence; the templates were the one missing
  external dependency. The first extraction attempt targeted a C:-drive temp
  path and failed - this machine's C: drive was completely full (0 bytes
  free) - so the retry targeted E: (where `%APPDATA%` for this profile
  actually resolves, and where 700+ GB is free) and only extracted the four
  needed entries instead of the whole archive. The C: drive's near-zero free
  space is a pre-existing condition on the User's machine, unrelated to this
  repo; flagged to the User rather than silently worked around.
- **Reference:** GDD §15.7 (release path); release ledger R02/R13.
- **Status:** Active.

### D77 - R07 wired: minefield route placement and the Forward Observer branch
- **Decided by:** Claude
- **Date:** 2026-09-05
- **Decision:** Wired two fully-implemented-but-never-called systems flagged
  in `docs/PROGRESS.md`'s known-gaps list. (1) `SpecialPlacementService` now
  gets constructed in `MapRuntime` and ticked every frame; `BuildBar` routes
  a selected Minefield card to a click-anywhere-on-route placement mode
  (reusing `AbilityHotbar`'s screen-to-world canvas-transform conversion)
  instead of pad glow, and shows a "placed/max" counter on the card per GDD
  §6 T8. This uncovered a deeper gap: `t8_minefield.tres` had no
  `ControllerScene` at all, and no `.tscn` for `MinefieldController` existed
  in the repository - placement could never have worked regardless of UI
  wiring. Added `scenes/towers/tower_minefield.tscn` (the controller draws
  its own visual, so it's just the script on a bare `Node2D`) and set
  `ControllerScene` on the resource. (2) `CommandPostController` gained
  `TickSpottedPulse` (Forward Observer: mark the strongest enemy in
  `RangeTiles` Spotted every `StatusDurationSeconds` - reusing that field as
  both cadence and duration, exactly as `MinefieldController` already does
  for its own periodic Suppressed trigger) and `RevealTargets` now reveals
  Air units map-wide specifically on that branch, per GDD §6 T9. Added
  `tests/PlacementIntegrationTests.cs` (6/6) covering both systems.
- **Why:** R07's stated acceptance ("Minefield route placement... Forward
  Observer... costs match data") named exactly these two gaps. Pad
  restrictions and cost-curve validation were already covered by existing
  tests/the Data Validator, so R07 needed only these two pieces.
- **Scope not covered (deferred to R16 balance/nation-parity, or R06/R15
  UX):** a placed minefield has no click/inspect/sell UI (`MinefieldController`
  has no click-Area2D, unlike `TowerController`/`CommandPostController`);
  Japan's field-count cap of 9 and doctrine field-cap bonuses (Island
  Defense's stated +3) are unwired - `SpecialPlacementService.
  ExtraMinefieldCapacity` is a neutral `() => 0` for every mission; the
  existing (pre-dating this session) British "double-radius Concealed
  reveal" hack keys off `DisplayName.Contains("Radar")`, which also fires for
  Germany's identically-named "Radar Flak Tower" - not something this change
  introduced, but worth fixing alongside the nation-parity pass; the British
  "Forward Observer marks two targets simultaneously" lean is unwired (the
  pulse already reads `TowerStatBlock.SalvoCount` as its target count and
  defaults to 1, so this needs only a nation-data value, not new code).
- **Reference:** GDD §6 (T8, T9); release ledger R07.
- **Status:** Active.

### D78 - Campaign Selection alliance/nation button sizing and centering fixed
- **Decided by:** User (reported the visual bug); Claude (fix)
- **Date:** 2026-09-05
- **Decision:** The User ran the game and reported the Allies/Axis buttons on
  `campaign_selection.tscn` were "too big by probably 50%, and off-center /
  asymmetrical." Root cause: `CampaignSelectionController.BuildAllianceSelection`
  set a 500x300 `CustomMinimumSize` (compare the far busier, four-line nation
  cards at only 370x360) on two `Button`s inside an `HBoxContainer` with no
  `Alignment` set (defaults to left/`BEGIN`) and no vertical shrink flag (so
  the buttons stretched to fill the row's `ExpandFill` height, growing well
  past 300). Fixed: `CustomMinimumSize` to 320x170, `SizeFlagsVertical =
  ShrinkCenter` on each button, `Alignment = Center` on the row. Applied the
  same `ShrinkCenter`/`Alignment = Center` fix to the adjacent nation-card row,
  which had the identical latent defect (not reported, but same code shape).
  Verified visually via a real screenshot (`--screen=campaign_selection`,
  newly added to the screenshot tool's route allowlist alongside
  `Boot.IsAllowedScreen`/`ResolveLaunchScene` - it didn't exist when this
  screen was built).
- **Why:** A concrete UI regression the User found by actually running the
  build - exactly the kind of thing headless tests can't catch (D55 already
  notes screenshot review is required for this reason).
- **Reference:** GDD §13.1/§9.1; UI_DESIGN_SPEC.md.
- **Status:** Active.

### D79 - Alliance/nation art is a known, deliberately deferred gap
- **Decided by:** User (flagged); Claude (logged)
- **Date:** 2026-09-05
- **Decision:** The User noted `campaign_selection.tscn`'s alliance and
  nation choices need real art (insignia/emblems), not just text buttons.
  No art was generated this session. Tracked as a known gap for the R14 art
  pass rather than actioned now.
- **Why:** Nation-specific and insignia art has been deliberately held
  pending implementation review since the post-M4 art prep pass (see the
  "Deliberately held nation-specific units, enemies... national insignia"
  note in `docs/PROGRESS.md` and D36/D39/D40) - generating it now would cut
  ahead of that established, User-directed review gate rather than follow it.
- **Reference:** release ledger R14; `docs/PROGRESS.md` art sections.
- **Status:** Open.

### D80 - R12 Settings screen wired; a real fullscreen-default regression caught first
- **Decided by:** Claude
- **Date:** 2026-09-05
- **Decision:** Built the Settings screen GDD §13.8-13.9 describes, scoped to
  what `UserSettings`/`PlayerSettings` (D75) actually back: Video (fullscreen,
  UI scale), Audio (five volumes), Accessibility (colorblind palette, visual
  effects intensity), Controls (full rebinding, conflict detection, reset).
  Since `fow_theme.tres` only defines styles for `Label`/`PanelContainer`/
  `Button` variations (docs/UI_DESIGN_SPEC.md §7), every control is a themed
  `Button` - discrete stepped rows (e.g. volumes as Off/25/50/75/100%) instead
  of a native `HSlider`, avoiding an unstyled widget clashing with the paper
  aesthetic. `SettingsPanel` (`src/UI/Menus/`) is the shared content, used by
  both `SettingsController` (a normal flow screen reachable from the main
  menu) and `PauseMenu` (a second card swapped in over the pause card - no
  scene change, so opening Settings mid-mission never touches GameLoop/
  MissionSession state). `Boot.StartMission` now calls `UserSettings.Apply`
  on every launch, including headless (it no-ops window-specific calls there).
- **Why:** This was the exact task mid-flight when the build broke (D75) -
  the direct continuation of that work. Wiring `Apply` at boot surfaced a
  real bug before it could ship: `PlayerSettings.Fullscreen` defaulted to
  `false`, so every fresh profile would have silently launched windowed the
  moment this code path actually ran, contradicting `project.godot`'s
  fullscreen-by-default setting (the "Launch fullscreen at 1080p by default"
  commit). Fixed the default to `true` and verified the fix live via a real
  screenshot (`--screen=settings`) showing "Fullscreen" pressed on a fresh
  profile, not just a unit test.
- **Scope not covered** (needs new backing systems, not just UI, so
  deliberately deferred rather than half-built): VSync/resolution/frame cap,
  screen shake (no camera-shake system exists), subtitles (no VO/subtitle
  system exists), default game speed, auto-pause-on-wave-complete,
  confirm-before-sell, targeting-priority defaults, tutorial-hints/damage-
  number toggles. The in-game Codex is a separate, still-locked main-menu
  button, unrelated to this settings work.
- **Reference:** GDD §13.8-13.9, prompt 44; release ledger R12.
- **Status:** Active.

### D81 - Volume rows became real sliders (PaperSlider)
- **Decided by:** User; Claude (implementation)
- **Date:** 2026-09-05
- **Decision:** The User asked for the five volume rows to be actual
  draggable slider bars, not the stepped Off/25/50/75/100% buttons D80
  shipped. Added `PaperSlider` (`src/UI/Theme/`), a `Control` that paints its
  own track/fill/handle in `_Draw()` rather than using Godot's native
  `HSlider` (which needs its own grabber icon texture to look intentional -
  new asset work `fow_theme.tres` doesn't have yet). It fires `ValueChanged`
  continuously while dragging (applied live - just an in-memory
  `AudioServer` bus volume) and `DragEnded` once on release, so
  `ProfileStore.TrySave` writes the profile to disk once per drag rather
  than on every mouse-motion event. Display, UI Scale, Colorblind Palette and
  Visual Effects stay stepped button rows - those are categorical choices,
  not continuous ranges, so a slider doesn't fit them.
- **Why:** A direct, specific UI request. The debounced save was a
  correctness call, not requested explicitly: an undebounced save would have
  written the profile to disk many times per second while scrubbing.
- **Reference:** GDD §13.8; release ledger R12; supersedes D80's volume rows.
- **Status:** Active.

### D82 - R08 map gimmick systems implemented (Tide, Sandstorm, Mud, Canopy, clipped arcs)
- **Decided by:** Claude
- **Date:** 2026-09-05
- **Decision:** Implemented all five gimmicks GDD §11.2 describes as
  runtime systems, generic and data-driven (via `MapGimmick`/
  `RuntimeGimmickData`, keyed by a free-form `Type` string and scoped by
  `PathIds`), independent of any specific map: **Tide** and **Sandstorm**
  are "active for the first N seconds of every M-second cycle" timers
  (matching the Sandstorm variant's own "20s on a 60s cycle" phrasing) that
  gate `GimmickSystem.IsPathAvailable`/`GlobalRangeMultiplier`; **Mud** and
  **Canopy** are static per-path lookups (no timer - GDD doesn't describe
  either as cyclical). Mud reuses `EnemyArchetype` (LightVehicle/
  MediumArmor/HeavyArmor only - `GimmickRules.IsVehicle`) rather than
  `ArmorClass`, since Armored Infantry is armored but still walks on foot.
  Canopy ORs a new `_inCanopy` flag into `EnemyController.IsConcealed`
  alongside E11's existing `recon_concealment` check, so it falls through to
  the same `IsRevealed`/`SetRevealed`/Spotted machinery already built and
  tested. Ruined Town's clipped-range arc (the GDD's own "one genuinely
  expensive gimmick") is a facing+half-angle cone check
  (`GimmickRules.IsWithinArc`) added to `TowerController.IsValidTarget` - the
  single choke-point every firing mode (primary, densest-cluster, secondary)
  already routes through - with the authored facing/angle living on
  `BuildPad`/`TowerPlacementNode` and copied onto a placed tower exactly like
  `PadTag` already is.
- **Why:** R08's ledger acceptance named these five by name. GDD §18.3 lists
  the clipped-range arc as a *contingency* cut ("if the schedule slips, cut
  in this order") - `CUT.md` is empty, so nothing has actually invoked that
  clause, and the GDD's own §11.2 table treats it as in-scope ("justified
  because it defines one map's whole identity"); cutting it preemptively
  would have been picking a side without flagging the conflict.
- **Scope not covered, on purpose:** Tide's WaveRunner integration - closing
  a path should reroute new spawns onto a fallback per GDD prose ("pushing
  all enemies onto the upper road"), but `SpawnGroup` has no authored
  fallback-path concept, and no real tidal map (M7 Coastal Fortification,
  R09) exists yet to validate the exact intended behavior against.
  `GimmickSystem.IsPathAvailable` is real and tested; only that one consumer
  is deferred. Similarly, the arc gimmick covers only the targeting/range-
  shape half of the GDD's cost estimate ("a pie-slice range shape and a
  line-of-sight check") - there is no terrain-collision/wall model anywhere
  in the project for a true line-of-sight check, and building one is
  properly R09's job once Ruined Town's actual wall geometry is authored.
- **Reference:** GDD §11.1, §11.2, §18.3; release ledger R08.
- **Status:** Active.

### D83 - R10 remaining bosses (B2-B4) and Elite variants implemented
- **Decided by:** Claude
- **Date:** 2026-09-05
- **Decision:** Implemented B2 Armored Column Command, B3 Bomber Wing, B4
  Fortress Assault Group, and Elite Medium Armor's Frontal Plate (GDD §10.3)
  as generic, data-driven `EnemyDefinition` mechanics rather than one bespoke
  class per boss:
  - **B2 Convoy:** a command vehicle's aura (`ConvoyAuraRadiusTiles`/
    `ConvoyDamageResistancePercent`/`ConvoyGrantsSuppressionImmunity`)
    projects to any nearby enemy, reusing the exact nearby-ally-by-distance
    pattern `EnemyControllerSupport.cs` already established for E9 Support/
    E10 Escort/E11 Recon (query `_enemyProvider`, filter by radius) rather
    than inventing a parallel discovery mechanism. Its death-triggered
    "collapse escorts to 50% HP" needed a new `CapHealth` method - a direct
    HP cap that bypasses `ApplyDamage`'s whole resolution pipeline (armor
    multiplier, shields, Convoy resistance itself), since routing a scripted
    narrative beat through combat damage would let those systems blunt or
    absorb it.
  - **B3 Formation:** enemies sharing a `FormationGroupId` (matched by string,
    not object identity, so it works whether they're spawned from one
    shared resource or several) get a damage reduction while every member is
    alive, and each lost member applies a multiplicative speed penalty to
    survivors - on *both* the ground and air movement paths in
    `EnemyController.SimTick`, which are separate code branches; `BossTests`
    caught that the air branch was missed on the first pass.
  - **B4 multi-phase:** a new `MultiPhaseBossController`, distinct from B1's
    `BossPhaseController` (which is specifically a 2-phase armor-skirt
    model), for an arbitrary number of one-way HP-fraction-threshold phases
    with a 3-second halt on each transition (the GDD's "visible telegraph").
    Phase 2 ("becomes a Siege platform") reuses the Siege archetype's
    existing `SiegeBombardRangeTiles`/`IntervalSeconds`/
    `SuppressionDurationSeconds` fields, `EnemySiegeBombardEvent` (already
    consumed by `TowerController.OnEnemySiegeBombard`), and
    `AddDefinition`/`AddCount`/`AddIntervalSeconds` (already consumed by
    `EnemyManager`'s existing `BossAddsRequestedEvent` plumbing) - zero new
    consumer code needed for either. `EnemyManager.GetSiegeHoldDistance` now
    also checks `MultiPhaseBoss?.IsSiegePhase` so B4 holds at range during
    phase 2, the same as a real Siege unit.
  - **Frontal Plate:** a frontal damage-reduction cone reusing
    `GimmickRules.IsWithinArc` (D82) against the enemy's own heading
    (`Velocity.Angle()`), active only once damaged past
    `FrontalPlateActivateHpFraction`. Required adding `GlobalPosition` to
    `IDamageSource` (free - every implementer is already a `Node2D`) so
    `ApplyDamage` knows which direction a hit came from.
  - **Elite Swarm/Elite Siege** needed no new fields at all - `SpawnGroup.
    Count` (already exists, unconsumed comment aside - `HpMultiplierOverride`
    was already wired in `WaveRunner.cs:107`) and a stat-adjusted
    `EnemyDefinition` resource copy are genuinely "pure data, no new code" as
    GDD says.
  - Fixed a stale `DataValidator` warning ("Boss EnemyDefinition has no
    SkirtHp set") that assumed every boss uses B1's skirt model; it now also
    recognizes Convoy/Formation/MultiPhase as valid boss mechanics.
- **Why:** R10's ledger acceptance named these bosses/elites explicitly.
  Reusing existing plumbing (nearby-ally auras, Siege's bombard event, boss
  add-request event) instead of one bespoke system per boss kept the change
  proportional and testable - `BossTests` (8/8) caught two real bugs
  (`CapHealth` not triggering the Convoy collapse; the air movement branch
  missing the Formation speed penalty) before they could ship.
- **Deliberately simplified, and logged rather than silently dropped:**
  - B4 phase 2's adds use a single archetype (Swarm Infantry) instead of
    GDD's "waves of Swarm **and** Fast Infantry" - `AddDefinition` is a
    single-reference field shared with B1; giving it a list would touch B1's
    working, tested behavior for a B4-only need.
  - B4 phase 3's "simultaneous 3-bomber air element" is not built - it needs
    a new "spawn escort units on a phase transition" hook, which nothing in
    the game does yet. The phase's core mechanics (Suppression immunity,
    +50% speed, direct sprint via the halted-then-unleashed pathing) are all
    real and tested.
  - Frontal Plate requires the enemy to be currently moving
    (`Velocity != Vector2.Zero`) to have a heading to project a cone from; a
    perfectly stationary enemy gets no protection rather than an arbitrary
    one. GDD itself flags Frontal Plate as "the most cuttable mechanic in
    the game" if playtesting shows it unreadable.
  - No mission/wave data places any of these into an actual campaign mission
    - missions 8 (B2), 10 (B3), and 12 (B4) don't exist yet (R09).
- **Reference:** GDD §10.3; release ledger R10.
- **Status:** Active.

### D84 - R17 editor-parity audit: eight map-editor services have zero callers
- **Decided by:** Claude
- **Date:** 2026-09-05
- **Decision:** Audited every phase-5-through-14 claim in `docs/PROGRESS.md`'s
  "Standalone map editor — remaining pipeline phases" section against actual
  reachability, using the method: grep every service class name across
  `src/` and `tests/` excluding its own declaration file. A class with a real
  UI path shows up referenced from `MapEditorController.cs` or one of its
  bound panels, and usually also from `MapAuthoringTests.cs` (sanity-checked
  this against known-working services - `MapTransformCommand`,
  `SelectionService`, `MapClipboard`, `MapPublisher` - before trusting a zero
  result). Eight classes came back with **zero references anywhere outside
  their own file** - not the editor, not a test:
  `MapLayerService`, `MapScatterService`, `MapGameplayCommands`,
  `MapEditorPreferences`, `MapRecoveryService`, `MapPathEditing`,
  `TerrainRules`, `TerrainPlacementPreview`. A ninth, `MapGenerationService`
  (with its `MapGenerationConfiguration`), is referenced only by each other -
  self-consistent, but never called from the editor either. Concretely: the
  editor header's "Generate", "View", and "Map" menu buttons
  (`MapEditorController.BuildHeader`) are stubs that print "COMMANDS ARRIVE
  IN A LATER MAP-EDITOR PHASE" to the status bar
  (`ShowPhaseMessage`) - clicking any of them does nothing else. "Edit" is
  the same stub dressed as a hint (its real commands - undo/redo/delete/
  duplicate/copy/paste - work via keyboard shortcuts, which are wired and
  covered by `MapAuthoringTests`). `MapAssetPalettePanel` (Phase 5, genuinely
  reachable) only ever places decorative art-catalog props via
  `ArtPaletteQuery`/`MapAssetCommands.AddAsset` - there is no palette entry,
  button, or menu item anywhere that creates a new terrain tile, tower node,
  marker, zone, air corridor, or gimmick. `MapObjectLocator` (Phase 4.5,
  genuinely reachable) generically selects/moves/rotates/scales/deletes
  every one of those object kinds *once one already exists* in the loaded
  document - so the editor can inspect and transform authored content, but
  cannot originate most kinds of it. Corrected the affected checkboxes in
  `docs/PROGRESS.md` from `[x]` to `[~]`/`[ ]` with the specific gap named
  per phase, rather than leaving a blanket "complete" claim standing.
- **Why:** This is exactly R17's mandate ("audit services versus reachable
  UI; correct earlier unsupported completion claims") and the same pattern
  this session already found three times independently in gameplay code
  (`SpecialPlacementService`, `UserSettings`, `GimmickSystem` were all fully
  implemented with zero call sites before D77/D80/D82) - worth checking
  systematically in the one other large subsystem (the map editor) that
  accumulated many phases without an end-to-end reachability check between
  them.
- **Scope note:** this entry is the audit-and-correct half of R17 only.
  Building the missing creation UI (a real Generate menu with a
  configuration dialog, a Map menu for gimmick/marker/zone/corridor
  authoring, a terrain palette and painting tool, a preferences dialog with
  recent files, and an autosave-recovery prompt) is a substantial new body
  of work - comparable in size to everything else done this session
  combined - and was not started under this entry. It would need its own
  explicitly-scoped effort, not a silent expansion of R17's "audit and
  correct docs" mandate.
- **Reference:** GDD §15.6 (editor tooling); release ledger R17.
- **Status:** Active.

### D85 - Alliance/nation UI banners generated, integrated, and a real-flag request declined in part
- **Decided by:** User (design direction and generation); Claude (content-policy boundary, integration)
- **Date:** 2026-09-05
- **Decision:** Generated and integrated 8 banner images for
  `campaign_selection.tscn`'s alliance and nation cards (2 alliances + 6
  nations) - a deliberate, User-directed exception to D36's standing
  "hold nation-specific art" gate, scoped to this UI screen only (gameplay
  tower/unit/enemy identity art remains held). The User initially asked for
  era-accurate real flags and said real flag art was fine "despite the
  rules"; declined the Germany/Japan/Italy portion of that specifically -
  the actual WWII-era German state/war flag was the swastika flag and
  Japan's actual military ensign was the rayed war flag, both explicitly
  named first among GDD §14.3's absolute content rules, and not something
  generated regardless of any project-level override (a limit on the
  assistant's own output, independent of house rules). Landed on a middle
  path instead: real, uncharged national colors (a palette isn't protected
  content) plus a real flag-shaped composition, with the GDD's own invented
  emblem in place of the real one - Germany is the one exception, using the
  equipment palette (field-green/grey-green) instead of any historical flag
  colors, since both real options there are politically charged. Registered
  as `review.ART-UI-FLAG-001` through `008` in `art_asset_catalog.json` and
  `assets/art/ART_ASSET_INVENTORY.md`; files live at
  `assets/art/shared/ui/flags/` (moved there from the User's initial drop
  location, matching the existing `commander_map_table_frame_v01.png`
  precedent for shared UI illustration art). Integrated into
  `CampaignSelectionController`: each card is a borderless `TextureButton`
  (the banner *is* the button, not an image inset inside a themed
  `PaperButton`) with the nation/alliance name and nation stat summary as
  plain labels underneath, per the User's follow-up request to drop the
  paper-panel background entirely.
- **Why:** GDD §14 is explicitly absolute, not house style ("this is not a
  judgment call to make casually" per `CLAUDE.md`) - a direct user
  instruction to override it is exactly the documented case to flag rather
  than silently follow. The compromise (real colors + real flag shape +
  invented emblem) delivers what the User was actually after - the flags
  reading as era-grounded rather than fully abstract shapes - without either
  problem.
- **Status:** Active.

## How to add to this log

Append, don't rewrite history. One entry per decision: what was decided, who
decided it, why, and a link to the GDD section if one exists. Mark status as
**Active**, **Superseded by D_n_**, or **Open** (unresolved / needs a
decision). If a later decision reverses an earlier one, add a new entry and
update the old one's status rather than editing the old entry's content.
