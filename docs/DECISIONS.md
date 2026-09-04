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

## How to add to this log

Append, don't rewrite history. One entry per decision: what was decided, who
decided it, why, and a link to the GDD section if one exists. Mark status as
**Active**, **Superseded by D_n_**, or **Open** (unresolved / needs a
decision). If a later decision reverses an earlier one, add a new entry and
update the old one's status rather than editing the old entry's content.
