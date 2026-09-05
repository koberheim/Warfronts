# Progress against GDD §19's implementation ladder

Live tracker, updated as work lands. Checkboxes correspond 1:1 to GDD §19's
numbered prompts. Milestone exit criteria are GDD §17.1.

## Current release audit — 2026-09-05

The active, evidence-based completion tracker is `docs/RELEASE_COMPLETION.md`.
Older checkboxes below describe implementation history, not release sign-off.
The User confirmed the external M3 playtest gate passed. The audit found
missing mission-map binding, prototype defenses in player flow, unsafe save
edge cases, incomplete pooling and a verification false-positive; those were
repaired (D72-D74) and the mission now resolves the authored Bocage
Crossroads map (D73). A follow-on session found the working tree's build
itself broken (a name collision in mid-flight settings code, D75); once
fixed, the full headless suite - 14 GoDotTest suites, the data validator, and
the mission smoke run - passes end to end. Launch content (7 more maps, 11
more missions), player export, presentation/art/audio, remaining modes, and
settings/accessibility UI remain unfinished; the project is not yet
release-ready. See `docs/RELEASE_COMPLETION.md` for the per-item ledger.

## M0 — Foundation

1. [x] Godot 4.x project structure per §15.2, `.gitignore`, Git LFS config,
   C# `.csproj` with namespace folders. **Partial:** structure, gitignore,
   LFS, and csproj exist; the Mono Godot build loads the project and
   `dotnet build --no-restore` succeeds. The headless smoke run also
   starts successfully.
2. [x] `GameLoop` autoload, fixed 60Hz tick via `_PhysicsProcess`,
   `TimeController` for 1×/2×/3×/pause. Written and exercised by the
   headless smoke run.
3. [x] `EventBus` (typed pub/sub) and `ObjectPool<T>`. Written; unit tests
   passing in `CoreTests`.
4. [x] `GameBalanceConfig`, `DamageTable`, `ArmorClass`/`DamageType` enums,
   pure `ResolveDamage`. All 16 table cells and the Spotted modifier pass in
   `CoreTests`.
5. [x] `SeededRandom` wired to the per-mission `MapRuntime.MissionSeed`.
   Repeatable integer, float, and boolean sequences pass in `CoreTests`;
   randomized gameplay consumers will arrive with later mission content.

**M0 exit criteria (§17.1):** project structure, autoloads, damage resolver
with unit tests. **Met:** the Mono Godot headless test run passes all twelve
current `CoreTests`.

## M1 — Core loop grey-box

6. [x] `PathNetwork` / `PathFollower` — enemies walk the authored route at
   the correct speed.
7. [x] `BuildPad` — hover highlight and click both publish events; nothing
   consumes the click yet (that's the build menu, M2 UI work).
8. [x] `EnemyController` + `EnemyDefinition`, Defense Line ledger — HP,
   armor class, and leak handling all verified in the smoke run (D19).
9. [x] Spatial grid + `TargetingService` — used by both towers in the smoke
   run; formal <1ms/40-tower perf check not yet run (no 40-tower scene
   exists yet — will validate for real once content exists at M4+).
10. [x] `TowerController` + `TowerDefinition`, T1 Automatic Gun hitscan —
    verified: damage matches the table exactly (see D19).
11. [x] `ProjectileSystem`, T4 Anti-Tank Gun — verified: leading shots hit,
    damage matches the table exactly (see D19).
12. [x] `SupplyLedger` — kill bounty income verified live; end-of-wave/
    early-call and end-of-wave income are now exercised by the M3 mission
    flow.
13. [x] `WaveRunner` + `WaveDefinition`/`SpawnGroup`, single-wave playback —
    verified: authored spawn groups fire on their exact schedule.

**M0/M1 gap closed:** formal automated unit tests now exist and pass.
**M0 closeout:** formal automated core tests now exist and pass through the
Mono Godot headless runner. The live smoke run remains the integration check
for the mission scene.

## M2 — Slice systems

14. [x] `TowerUpgradeController`: 4 levels, branch fork at L3, GDD §7.4 cost
    curve, sell with 4s full-refund window. Automated tests now cover the
    exact cost curve, branch selection, four levels, 75% refund, and 4-second
    full-refund window. The panel buttons remain a manual UI interaction.
15. [x] T3 Field Mortar / T9 Command Post support systems: densest-cluster
    point-targeting (T3) and non-stacking aura + CP generation (T9) are both
    implemented and wired into a live test scene (one Command Post next to
    the machine gun and mortar). Verified: the smoke run includes mortar
    ground-point fire with zero gameplay exceptions, and `CoreTests` checks
    the exact +12% range / +8% rate-of-fire aura. The mortar uses authored
    T3 data and a pooled mortar-shell scene; `CoreTests` verifies its
    point-targeting profile, minimum range, blast radius, and densest-cluster
    selection.
16. [x] `StatusController` (Suppressed, Spotted) with the 4-second
    non-refreshing cap. Wired into `EnemyController` (movement speed
    penalty, Spotted damage bonus) and `TowerController` (a parallel
    tower-suppression hook for the future Siege enemy). Automated tests now
    cover application, refresh capping, and expiration; live Siege/Spotted
    sources arrive with later enemy/tower content.
17. [x] `CommandPointLedger` and the three universal abilities (Artillery
    Strike, Rally, Emergency Repair) with cooldowns and CP cost checks.
    Implemented as `AbilitySystem`; callable via
    `MapRuntime.ActivateAbility(...)`. `AbilityHotbar` now provides bottom-
    right buttons, CP shortfall feedback, cooldown state, keys 1–3, and
    click-to-target for the point abilities. Paused-mode input is enabled;
    paused-mode logic is covered by `CoreTests`; automated physical mouse/key
    input coverage remains a UI follow-up.
18. [x] In-mission HUD (Supply, Command Points, Defense Line, wave counter,
    speed/pause control) — `HudController`, built as Control nodes on a
    `CanvasLayer`, live-updating from the same events the smoke-test logger
    uses. Verified running without errors; visual appearance not checked
    (no screenshot taken — this session has no way to see the rendered
    output, only headless logs).
19. [x] Wave preview strip — `WavePreviewPanel`, three tiers of disclosure
    (full detail / archetypes-only / threat-badge-only) as specified.
    **Simplified:** text-only, no icon art; the authored M2 sequence now
    queues all 12 waves and the resource test verifies the three-wave preview
    source data.
20. [x] Tower inspection panel — `TowerInspectionPanel`. Click-to-open,
    live stats, upgrade (with cost/affordability check), sell (with
    refund), all wired to the real ledgers. Prototype glyph rows show
    "Strong vs / Weak vs" and lifetime damage-per-Supply now uses per-tower
    damage attribution from the combat event pipeline.
21. [x] Post-mortem panel — `PostMortemPanel`. Leak tally, damage-by-type
    breakdown, unspent resources, and the exact suggestion rule from GDD
    §12.9's worked example (heavy armor leaked + low AP damage share →
    suggests AP towers). **Simplified:** triggers only on defeat
    (`DefenseLineDepletedEvent`) during the M2 close; M3 now also triggers it
    on victory through the mission-complete event. Most/least effective tower
    data is included using the shared damage-source event data.
22. [x] Enemy health bars, armor-class glyphs, status badges, and the
    ricochet/ineffective-damage floating number feedback. Verified in the
    smoke run (health bars only draw once damaged, glyphs differ by armor
    class shape not just color, damage numbers are color+prefix coded).

**M2 verification note:** the formal automated core suite now covers the
upgrade/refund math, Command Post aura, status lifecycle, paused ability
activation, authored 12-wave/four-enemy content, Field Mortar targeting, and
per-tower damage attribution. The live mission smoke run remains the
integration check; visual screenshot review and physical UI input remain
manual checks.

**M2 exit criteria (§17.1):** four tower archetypes, four ground enemies,
12 authored waves, upgrades, Command Points, abilities, HUD, wave preview,
speed/pause controls, and post-mortem reporting. **Met:** all twelve
`CoreTests` pass the M2 checks and the Mono Godot mission smoke run includes
all active M2 systems without gameplay exceptions.

## M3 — Vertical slice

23. [x] Arsenal of Democracy signature: data-authored rifle squad, Jeep, and
    light-tank unlocks; production timer; five-unit cap; backward path travel;
    white-outline friendly rendering; and three-second soft-block release.
24. [x] B1 Breakthrough Panzer: data-authored Heavy boss with a 2,000 HP
    armor skirt, 3× Explosive skirt damage, visible phase transition, +30%
    post-skirt speed, and timed Basic Infantry adds.
25. [x] Mission flow: briefing → loadout → mission → post-mortem → results,
    with one-click retry and a direct `--mission` debug launch.
26. [x] Eight-prompt integrated tutorial: pauses the simulation, teaches the
    live systems in sequence, and resumes after the final prompt.

**M3 verification:** C# build succeeds with 0 warnings/errors; the Mono
Godot GoDotTest suite passes the M3 boss phase, Arsenal resource, and mission
flow checks. M3 remains primitive-art only; visual playtest and orphan-node
reporting are still manual editor checks.

## M3.5 — Design-time Map Planner

- [x] Catalog foundation: the 100-entry layout catalog loads into typed
    template data from `godot-project/assets/data/map_layout_templates/`.
- [x] Authored `MapPlanDefinition` data model with normalized 100 x 56.25
    coordinates, paths, entries, objective, pads, zones, air-corridor data,
    staged hooks, metrics, validation, status, and JSON save/load.
- [x] Planner geometry and diagnostics: pure C# path lengths, bends,
    connectivity checks, accidental/intentional crossing checks, separation,
    pad exposure metrics, weighted score components, and deterministic pad
    suggestions.
- [x] Candidate generation: deterministic seeded geometry for all ten
    catalog families, including merge, split/merge, dual-lane, crossing, hub,
    gauntlet, and asymmetric topology hooks. Candidate diversity filtering is
    stable and human-reviewable.
- [x] Editor dock: catalog browser, normalized canvas, manual path/pad input,
    score and diagnostics inspector, overlay selector, candidate list, seed,
    regenerate, draft save, and accepted-plan export. Export writes ordinary
    authored JSON under `assets/data/maps/plans/`; the mission never calls the
    planner or generated data.
- [x] Automated checks cover all 100 catalog records, unique IDs, objective
    declarations, deterministic generation, 1,000 generated candidates,
    crossing intent, advanced topology validity, pad limits, score stability,
    diversity, and save/load round trips.

**M3.5 verification:** the Mono Godot test runner passes the map-planning
suite and the existing core suite; the editor-only plugin is scanned by the
Mono Godot editor without runtime map-generation hooks. No new shipping maps
were added and M4 remains unopened.

## M4 through M8

27. [x] Remaining universal archetype data: T2 Marksman Post, T5 Flak
    Battery, T6 Armored Emplacement, T7 Heavy Artillery, and T8 Minefield.
    Each has authored L1/L2 data plus both L3/L4 branches, with generic
    target-domain, secondary-fire, status, salvo, and minefield-rule hooks.
    The M4 resource tests verify the GDD profiles and branch differences.
28. [x] `NationProfile`, national stat leaning, and the ±15%/±3% validators.
    Six nation resources apply data-authored stat leans to shared tower
    definitions; the validator checks the envelope and roster DPS-per-Supply
    parity.
29. [x] E3 Swarm Infantry, E7 Heavy Armor, and E12 Siege Artillery. Swarm
    cohesion catch-up, Heavy suppression immunity, and Siege bombardment are
    implemented. Siege stops about 11 tiles from the nearest non-enclosed
    tower and broadcasts six-second suppression every eight seconds.
30. [x] Wave Editor plugin with a GraphEdit timeline, Threat Value graph-like
    group blocks, pacing warnings, JSON export, and one-click mission
    playtest launch.
31–35. [x] Five data-authored national signatures: RAF Scramble Command,
    Katyusha Storm Battery, Blitzkrieg Command Post, Bersaglieri Charge Post,
    and Special Attack Airfield. Each has its authored charge/cooldown model,
    player activation, readable primitive telegraph, and named counterplay
    mitigations.
36. [x] Air corridors and E8 Air Unit. Air enemies use an authored straight
    corridor, share the spatial targeting grid, and are only acquired by
    air-capable tower domains; RAF interception and Flak both work through
    the same target contract.
37. [x] E9 Support repair, E10 Escort shared shield pool, and E11 Recon
    concealment/speed aura. Command Posts and Spotted reveal concealed
    enemies; minefields deliberately trigger them without target acquisition.
38. [x] Balance Dashboard editor plugin. It reports each nation's
    DPS-per-Supply value, envelope/parity errors, and includes an injected
    violation check.
39. [x] Doctrines: `DoctrineDefinition` = one neutral-by-default passive
    row plus one ability of six shared kinds (point blast, line blast, aura
    buff, spawn friendly, instant refund/utility, status). All 18 authored as
    data, three US choices on the loadout screen, hotbar slot 4 / key 4.
    Inert until their hooks exist: terrain-tag passives, Italy's national
    relocation, Fortified Line's immunity radius (D51). `DoctrineTests` 14/14.
40. [x] Map gimmicks per §11.2 (D82): `GimmickSystem` implements Tide,
    Sandstorm, Mud, Canopy, and Ruined Town's clipped-range arc, all
    data-driven via `MapGimmick`/`RuntimeGimmickData` and independently
    tested (`GimmickTests` 7/7) - none of the eight launch maps exist yet to
    wire them into (R09/prompt 26's map roster). Tide's WaveRunner spawn-
    rerouting and the arc gimmick's true wall/line-of-sight geometry are
    deliberately not built (see D82).
41. [x] Progression: `MissionDefinition`/`StarObjectiveDefinition` data,
    three-star evaluation (§11.3), §9.5 unlock gates, cosmetic-only Faction
    Mastery (§12.3), and a versioned JSON save with a real v1→v2 migration
    and corrupt-file fallback (§12.8). The results screen records and shows
    stars, XP, rank, and new unlocks. `ProgressionTests` 18/18 (D53).
42. Not started (Skirmish, Endless, modifiers; needs a main menu).
43. [~] `IPlatformService` + `NullPlatformService` and achievement ids exist;
    GodotSteam integration not started.
44. [~] Settings screen (D80): video (fullscreen, UI scale), audio (five
    volumes), accessibility (colorblind palette, visual effects intensity),
    and full control remapping, reachable from the main menu and the pause
    menu. `SettingsTests` 4/4. Not covered: VSync/resolution/frame cap,
    screen shake, subtitles, and the gameplay toggles (default speed,
    auto-pause, confirm-before-sell, targeting-priority defaults, tutorial-
    hints, damage numbers) - none of those have a backing system yet.
45. [x] Data Validator (see the completion-pass section below).

**M4 prompts 28–30 verification:** the .NET build succeeds with 0
warnings/errors. Mono Godot passes `M4TowerTests` 3/3, `M4NationEnemyWaveTests`
3/3, and `CoreTests` 15/15. The Mono Godot editor scan initializes both
editor plugins; only the machine's known editor-cache/IDE-metadata permission
messages remain. M4 is complete; M5 prompts 31–38 are implemented.

**M5 prompts 31–38 verification:** the .NET build succeeds with 0 warnings
and 0 errors. `M5SignatureAirTests` covers signature data, RAF three-pass and
air interception, the remaining signature activation paths, air-corridor
movement, E9/E10/E11 behavior, minefield-vs-concealed interaction, and Flak
domain rules. The installed Godot executable is the known non-.NET build, so
the GoDotTest suite and editor plugin scan cannot run on this machine; the
source build is the available verification until a .NET-enabled Godot binary
is installed.

## Post-M4 art preparation

- [x] Created the reusable art-kit folder structure under
  `godot-project/assets/art/`, with theater, shared, miscellaneous, and
  explicitly held unit/enemy categories.
- [x] Created the art inventory and generation log. Ten initial
  theater/terrain/flavor/UI assets are generated; all ten are approved for
  conditional integration, with terrain adjacency and UI layout checks still
  required before placement.
- [x] Deliberately held nation-specific units, enemies, enemy wrecks,
  national insignia, and tower/unit identity art pending implementation
  review, per the user's request.

## Post-M5 art plumbing

- [x] Added the 57-entry art catalog under
  `godot-project/assets/data/art/`: stable family-level pathways for planned
  art, exact entries for the ten original anchors and ten Western Europe
  route-review assets, plus statuses and placeholder mappings for active and
  held inventory families.
- [x] Added reusable SVG placeholder templates, physical leaf-directory
  anchors, `ArtAssetSprite` replacement slots, and a representative
  placeholder gallery. Gameplay remains primitive-first until art is approved.
- [x] Added the six-tile terrain adjacency review scene and wired the approved
  command-table frame into briefing plus the selected Western Europe ground,
  hedgerow, farmhouse, and supply art into the Bocage Crossroads environment
  layer. Gameplay logic and primitive unit art remain unchanged.
- [x] Added a copy-ready individual art prompt queue covering every active
  inventory family: 460 numbered entries, 456 explicit image output paths,
  ten compatible route topologies and ten material transitions per theater,
  shared style/palette locks, and per-asset acceptance checks. Held tower,
  nation-unit, and enemy identity art remains intentionally unprompted.
- [x] Generated and organized the ten-piece Western Europe sunken-lane route
  family as 1024×1024 review candidates. Added catalog entries, opt-in review
  loading, shared feathered edge caps, and a touching closed-loop review scene.
  The family remains `REVIEW`; the closed loop confirmed the user's concern
  that exact outer edge caps do not solve repeated interior bands. D44 now
  defines the production fix: shared route geometry/material layered over
  varied terrain, with fixed sockets and a soft interior handoff.
- [x] Implemented the first D44 layered-route proof: reusable topology masks as
  `.tres` data, a deterministic fixed-width route/shoulder renderer, and an
  arbitrary mixed-neighbor 3x3 review scene with an audit of all 12 internal
  joins. The proof passes the .NET build and headless Godot load; it is not yet
  promoted to the live mission route.
- [x] Generated and wired the first shared painterly Western Europe route
  material as a transparent 352x1024 review asset. It remains a fallback only;
  the desired route art is now one unique transparent overlay per topology.
- [x] Generated and wired the complete first D45 Western Europe
  topology-overlay review set: four corners, two T-junctions, a four-way
  cross, and a North entry. Each is a coherent transparent 1024x1024 route
  surface with the shared socket contract; all matching cells in the proof
  scene now use their topology overlay. The family remains `REVIEW`.

## Post-M5 completion pass (2026-09-03 / 04)

A lead agent audited the repository against the code rather than the docs
and closed the gaps that blocked a playable, verifiable build. Decision log
entries D46–D50 record the reasoning.

- [x] **Verification actually runs.** Godot 4.7 has no `headless` feature
  tag, so every headless smoke run since M3 had silently idled on the
  briefing screen or behind the paused tutorial. Detection now uses
  `DisplayServer.GetName()`; the canonical smoke command is
  `godot --headless --path . --fixed-fps 60 --quit-after 5400` (90 simulated
  seconds, must print zero error/exception lines and at least one `[kill]`).
  The M4/M5 "smoke run" notes above predate this fix and were not exercising
  the mission.
- [x] **M5 suite run for the first time** on the Mono build: an RAF charge
  regenerated instantly after a spend (fixed) and one test leaked an Escort
  shield into the minefield check (isolated). All suites pass.
- [x] **Players can build.** Bottom-centre build bar (six loadout towers,
  costs, hotkeys Q–Y, shortfall labels), pad glow in build mode, range
  preview on hover, click-to-place through `TowerPlacementService`, pad
  freed on sell, no selling while Suppressed. T2/T5/T6/T7 gained primitive
  scenes; every archetype `.tres` declares its `ControllerScene`. Ten more
  pads on Bocage Crossroads (two Elevated, two Enclosed). `BuildTests` 5/5.
- [x] **Prompt 45 Data Validator** (`addons/data_validator` menu item and
  `--validate-data` CLI, exit 1 on errors) plus
  `tools/Run-HeadlessChecks.ps1`, which chains build → every suite →
  validator → smoke run. Its first run caught a dormant signature-id
  mismatch in the US profile. `DataValidatorTests` 4/4.
- [x] **VS towers can be upgraded to L4.** T1/T3/T4/T9 had no branch data
  and would have thrown at level 3; all nine archetypes now carry L2 and both
  L3/L4 branches per GDD §6, the inspection panel offers both branches at
  the fork, and upgrade costs reproduce §7.4 exactly (half-up rounding in
  integer hundredths). `M4TowerTests` 5/5.
- [x] **Prompt 39 doctrines** landed as pure data over six shared behaviours
  (D51); every one of the 18 rows loads through the validator.
- [x] **Prompt 41 progression and save** landed (D53), with prompt 43's
  Null platform service.
- [x] A placed Command Post can now be inspected, upgraded, and sold.
- [x] Balance Dashboard no longer throws during the editor's initial script
  scan; the stale `data_*/` ignore pattern that hid the validator addon is
  anchored to the project root.

**Automated checks after this pass:** `dotnet build` 0 warnings / 0 errors;
CoreTests 15, M4TowerTests 5, M4NationEnemyWaveTests 3, M5SignatureAirTests
5, MapPlanningTests 6, BuildTests 6, DataValidatorTests 4, DoctrineTests 14,
ProgressionTests 18 — all passing; `--validate-data` 0 errors / 0 warnings
across 64 resources; smoke run 0 errors with kills.

---

## UI overhaul (2026-09-04)

The User asked for a full UI/UX overhaul on a 24-inch 1080p target. D54
laid the system down (`docs/UI_DESIGN_SPEC.md`, one theme, vendored fonts,
war-table materials, the screenshot tool); D55 built every screen on it.

- [x] **System (D54).** 1920×1080 `canvas_items` canvas, fullscreen by
  default, `fow_theme.tres` with every font/color/style box, `UiPalette` /
  `UiIcons` mirrors for `_Draw` code, paper/teletype/brass/wood 9-patches
  derived from the approved frame, `ScreenshotCapture` dev flags.
- [x] **Glyph set.** `tools/art/generate_ui_icons.py` writes the 79
  monochrome SVGs of spec §6 (resources, time, waves, damage types, armor
  classes, statuses, threats, matchups, abilities, towers, enemies,
  progress, abstract nation marks) into `assets/ui/icons/`.
- [x] **Flow screens.** New main menu (Campaign live; Skirmish / Endless /
  Codex / Settings shown locked), briefing as an operation order with an
  intelligence row derived from the wave sequence, loadout with the six
  recommended-kit cards and a live doctrine toggle group plus the §13.3
  AP/AA warning banner, results with stars, mastery bar and unlock lines.
  `Boot` enters the main menu; `--mission` / `--screen` / headless unchanged.
- [x] **Mission HUD.** Resources with projected income and the segmented
  Defense Line bar; wave heading and teletype strip (glyphs, counts, armor,
  threat badges, air warning); brass speed lever, pause and menu buttons
  with Space / + / - / P keys; build-phase countdown ring and the exact
  "Call Wave Early +NN"; build bar tower cards with hotkey plates, glyphs,
  costs, shortfalls and paper tooltips; four ability cards with radial
  cooldown sweeps and CP badges (doctrine slot included); pause banner and
  pause menu (Resume / Restart / Settings-later / Abandon with confirm /
  Quit to Menu). The war-table frame now sits behind the battlefield.
- [x] **Cards and overlays.** Paper inspection card anchored beside the
  tower (level pips, stats with damage glyph, strong/weak rows derived from
  `DamageTable`, attribution, upgrade with diff preview, branch fork with
  per-branch diffs, sell, close) with a world-space range ring and target
  line; tutorial card with step pips and Skip; post-mortem report with a
  damage-by-type chart and Retry focused; damage numbers, build pads, enemy
  health bars and the Command Post aura ring re-palettised.
- [x] **Verified by screenshots** at 1920×1080 for main menu, briefing,
  loadout, results, mission (engaged and mid-wave), tutorial, pause menu,
  inspection card and post-mortem (`--ui-state`), see `tools/README.md`.

**Automated checks after this pass:** `dotnet build` 0 warnings / 0 errors;
CoreTests 15, M4TowerTests 5, M4NationEnemyWaveTests 3, M5SignatureAirTests
5, MapPlanningTests 6, BuildTests 6, DataValidatorTests 4, DoctrineTests 14,
ProgressionTests 18 — all passing; `--validate-data` 0 errors / 0 warnings
across 64 resources; smoke run 0 errors with 18 kills.

## Historical blockers / current follow-up

No design blockers. Prompts 1–39, 41, and 45 (plus 43's Null half and 44's
core Settings screen, D80) are implemented and verified on the Mono build;
see the completion-pass section above. GDD §11.2's map gimmick systems
(Tide, Sandstorm, Mud, Canopy, clipped range arcs, D82) and B2-B4/Elite boss
mechanics (D83) are also implemented and tested ahead of the map/mission
content that will use them. Remaining ladder work in order: 42
(modes/modifiers), 43 (Steam via GodotSteam), 44's uncovered fields (see its
checkbox above), then M6–M8 content (maps 2–8, 12 missions - including
placing B2/B3/B4 into missions 8/10/12 - codex; the main menu itself exists
since the UI overhaul).

- **Known gameplay gaps carried forward:** the loadout screen is a fixed
  recommended kit, not §13.3's picker - `BuildOption`/`SpecialPlacementService.
  TryPlaceSignature` exist for dynamic signature build-slot placement but
  have no HUD call site yet (R06 follow-up). **Resolved since the paragraph
  above was written:** T8 Minefield now has real free-placement UI and its
  missing `.tscn`/`ControllerScene` (D77); the Forward Observer branch's
  Spotted marking is now consumed (D77); `EnemyManager`/`FriendlyUnitManager`
  pool through `ObjectPool<T>` (D74); the mission now resolves the authored
  Bocage Crossroads map, not the 11-pad prototype (D73, confirmed by
  `RuntimeMapIntegrationTests` this session).
- **Not verifiable by agents:** the M3 external playtest gate (§17.2). UI
  layout is now checked by real screenshots (`tools/README.md`); what an
  agent cannot judge is feel - hover/press timing and readability at 2x
  speed on a real 24-inch monitor.

- **Immediate art review:** review the complete D45 Western Europe overlay
  family in the updated proof scene. The current v01 closed-loop board remains
  a reference and should not be approved as production route art. The next
  implementation step is a mixed-neighbor visual check at gameplay scale,
  followed by straight-route overlays and then the other theaters.
- **D17:** test framework selection is historical; D25 records the active
  GoDotTest choice and working headless command.
- **Art reference:** `docs/FRONTS OF WAR ART DESIGN.md` is the active
  visual-direction reference. The post-M4 environment-only generation pass
  created ten review assets; nation-specific units and enemies remain held.
- **Environment generation pass (2026-09-04):** all 175 active
  flavor/vegetation/architecture prompts now have normalized RGBA v01 PNG
  candidates in the four theater kits. They are registered as REVIEW in
  the art catalog; gameplay-scale and Clean/Typical/Stress screenshot review
  remain open before approval. UI, shared presentation, tower, unit, and enemy
  art were intentionally left untouched.
- **Standalone .NET restore:** `dotnet build` cannot resolve
  `Godot.NET.Sdk/4.7.2` without access to NuGet; the Mono Godot build can
  still run the current project and smoke scene. **Resolved 2026-09-05:**
  this machine now has NuGet access (`dotnet restore` inside
  `tools/Build-Windows.ps1` succeeds) and the Godot 4.7.2 Mono Windows export
  templates are installed - a real `FrontsOfWar.exe` player build now exists
  under `godot-project/build/player/` (gitignored) and boots headlessly clean
  (D76). The prior "distributable .exe blocked" notes elsewhere in this file
  describe earlier sessions' state and are left as history.
- **Map planner:** `docs/fronts_of_war_map_planner_design_spec.md`, the 100-
  template catalog, and the `map_planner` editor dock implement the M3.5
  design-time workflow. Accepted exports remain authored review artifacts;
  runtime procedural generation is still out of scope.
- **Standalone map editor blueprint:** `docs/standalone_map_editor_blueprint.md`
  records the architecture and 16-phase path from shared map domain through
  publish and M3.5 plugin retirement. Phase 1 is complete: the repository
  launcher opens a debug-only standalone editor scene, the 1920×1080 themed
  workbench shell is in place, player/developer exports are separated, and
  player PCK inspection confirms no editor scene/source/type marker ships.
  Phase 2 is also complete: the shell can create, open, save, Save As, and
  close canonical schema-v1 `MapDefinition` resources with dirty prompts and
  validated deterministic `.tres` output. Phases 3 and 4 are now complete:
  loaded maps render as an inspectable tile-space board, and selection,
  transforms, undo/redo, duplicate, copy/paste, delete, and inspector edits are
  live. Phase 5 asset catalog/palette integration is next.

## Standalone map editor — Phase 1 shell (2026-09-04)

- [x] Repository-root `Launch-MapEditor.ps1` resolves the project from any
  working directory, accepts `-GodotMono`, falls back through
  `$env:GODOT_MONO` and D13, forwards Godot arguments, validates Mono/.NET,
  and reports an actionable missing-binary error.
- [x] `Boot.ResolveLaunchScene` gates `--map-editor` behind Debug and replaces
  arbitrary `--screen=<scene>` loading with the five documented screenshot
  routes. Normal player launch remains `boot.tscn` → main menu/mission.
- [x] `map_editor.tscn` builds the Fronts of War workbench shell: map board,
  asset palette/hierarchy, inspector, diagnostics, toolbar, and status rail.
  It intentionally owns no map document or mutation commands yet.
- [x] `Windows Player` excludes the editor scene/source and Release omits the
  editor types; `Windows Developer` retains them for Debug exports. The four
  existing Godot editor plugin entry classes now use the required `#if TOOLS`
  boundary, and `FrontsOfWar.sln` is tracked because Godot 4.7 requires it to
  publish the .NET project.

**Verification:** Debug build 0 warnings / 0 errors; Release solution build
0 warnings / 0 errors; CoreTests 17/17; headless Godot editor startup clean;
launcher success from `docs/` and missing-binary failure both pass; real
1920×1080 screenshot reviewed; player PCK contains `main_menu.tscn` but no
`map_editor.tscn`, editor source path, or `MapEditorController` marker. A full
Windows executable was not produced because this machine has no Godot 4.7.2
Mono export templates installed.

## Standalone map editor — Phase 2 map documents (2026-09-04)

- [x] Added one tile-space coordinate contract (D64's 64px gameplay tile),
  stable lowercase object IDs, quarter-turn rotation, and uniform-scale rules.
- [x] Added schema-v1 Godot Resources for map metadata, terrain, placed assets,
  clusters, paths, air corridors, tower nodes, markers, zones, gimmicks, and
  generation provenance. Editor-only state is not serialized into the map.
- [x] Added validated deterministic `.tres` persistence: stable collection and
  external-resource IDs, explicit schema/corruption failures, and temporary +
  backup replacement so a failed save preserves the last known-good file.
- [x] Added New/Open/Save/Save As/Close to the live File menu with dirty state
  and Save/Discard/Cancel prompts. The standalone document remains isolated
  from player profile and mission state.
- [x] Added `Launch-MapEditor.cmd` for double-click startup and
  `docs/MAP_EDITOR_MANUAL.md` because a distributable `.exe` still cannot be
  produced without matching Godot Mono export templates.

**Verification:** Debug and Release builds 0 warnings / 0 errors;
MapAuthoringTests 7/7 and CoreTests 17/17; double-click launcher headless
startup clean; player PCK contains the normal main-menu scene but no editor
scene, editor source path, or editor controller/workflow type marker. Tests
cover empty/tiny round trips, deterministic output, schema errors, failed-save
preservation, and refusal to silently discard dirty state.

## Standalone map editor — Phases 3–4 rendering and editing (2026-09-04)

- [x] Phase 3.1: `MapRegistry` and `MapLoader` resolve repository-relative map
  IDs or `.tres` paths without absolute-path coupling.
- [x] Phase 3.2: `MapSceneFactory` builds a deterministic snapshot covering
  terrain, assets/clusters, paths, air corridors, tower nodes, markers, and
  zones. The viewport draws the result as clear authored placeholders ready
  for catalog sprites in Phase 5.
- [x] Phase 3.3–3.4: the board uses D64's 64px tile contract, centered map
  framing, grid majors, zoom-around-cursor, middle-mouse pan, cursor tile
  conversion, click selection, additive selection, and a live map hierarchy.
- [x] Phase 4.1–4.2: selection service and command history support multi-select,
  undo/redo, redo invalidation, and compound commands.
- [x] Phase 4.3–4.4: validated move/rotate/scale commands plus delete,
  duplicate, copy, and paste with fresh IDs and exact snapshot undo.
- [x] Phase 4.5: inspector exposes ID/category and editable position, rotation,
  and scale controls. Changes issue commands; terrain scale is disabled and
  uniform-scale rules remain enforced for scalable asset types.
- [x] Window-close protection now routes dirty documents through the same
  Save/Discard/Cancel prompt as File → Close.

**Verification:** `MapAuthoringTests` 11/11, including registry resolution,
render snapshot coverage, selection, transforms, exact undo/redo, duplicate,
copy/paste, and delete. Debug build 0 warnings / 0 errors; the previous
CoreTests 17/17 and Release build remain green. The live editor launches via
the root `.cmd`/PowerShell launcher; a distributable `.exe` remains blocked by
missing Godot Mono export templates.

## Standalone map editor — remaining pipeline phases (2026-09-04)

**Corrected 2026-09-05 (R17, D84):** the checkboxes below described service
classes as complete without verifying they were reachable from the actual
editor UI. An R17 audit (grepping every phase's service class for any
reference outside its own file, across `src/` and `tests/`) found several
with **zero** callers anywhere - not the editor, not a test. They compile
and, where checked, work correctly in isolation, but a person using the
standalone map editor cannot reach them. Corrected markers below; see D84
for the full per-phase evidence.

- [~] Phase 5: catalog DTO compatibility, searchable/filterable palette,
  thumbnail fallback, catalog-ID placement, and placement cancellation.
  Reachable and working (`MapAssetPalettePanel`, wired into
  `MapEditorController`) - but only for decorative art-catalog props
  (`ArtPaletteQuery`/`ArtAssetCatalog`), not for any gameplay object type.
- [~] Phase 6: 64px terrain socket/adjacency/occupancy rules, snapped terrain
  commands, rotation-only terrain editing, and terrain diagnostics.
  `TerrainRules`/`TerrainPlacementPreview` exist and compile but have no
  caller anywhere in `src/` or `tests/` - there is no way to paint a new
  terrain tile in the editor. An existing `TerrainInstance` can still be
  selected/moved/rotated generically via `MapObjectLocator` (see Phase 4.5).
- [~] Phases 7–9: catalog-backed runtime art slots, clusters, layer
  visibility/locking service, authored tower nodes, path-point commands,
  gameplay markers, air corridor data, and deterministic multi-path runtime
  path selection support. The **data model and runtime consumption** are
  real (`MapObjectLocator` generically selects/moves/rotates/scales every
  one of these object kinds once they exist in a document, and
  `MapRuntimeDataFactory`/`MapRuntimeAuthoringBuilder` consume them at
  mission load). The **creation tools** are not: `MapLayerService` (layer
  visibility/locking) and `MapGameplayCommands` (adding a new tower node,
  marker, zone, or air corridor) have zero callers anywhere. There is no way
  to create a new instance of any of these from inside the editor - only to
  select, move, or delete one that already exists in the loaded document
  (authored some other way: hand-edited `.tres`, or Phase 10's generation
  converter, if that itself becomes reachable).
- [~] Phase 10: deterministic generation configuration, planner reuse, and
  candidate-to-MapDefinition conversion with provenance and editable pads.
  `MapGenerationService`/`MapGenerationConfiguration`/`MapPlanConverter`
  exist and reference each other, but nothing calls them from the editor:
  the header's "Generate" menu button is a stub that only prints "GENERATE
  COMMANDS ARRIVE IN A LATER MAP-EDITOR PHASE" to the status bar
  (`MapEditorController.ShowPhaseMessage`). Same stub for the "View" and
  "Map" menu buttons, and "Edit" is a tooltip-style hint rather than a real
  menu (its actual commands - undo/redo/delete/duplicate/copy/paste - work
  via keyboard shortcuts, which are real and tested).
- [x] Phases 11–13: severity-coded diagnostics, focus mapping, runtime
  MapLoader handoff, Test Map launch, publish validation, canonical map
  repository output, and MissionDefinition MapId resolution. Confirmed
  reachable: `MapEditorController`'s diagnostics panel, "TEST MAP", and
  "PUBLISH" buttons all call real services (`MapDiagnosticsService`,
  `MapPreviewLauncher`, `MapPublisher`).
- [ ] Phase 14: editor-only recent-map/recent-asset preferences, recoverable
  autosave service, and seeded scatter command. `MapEditorPreferences`,
  `MapRecoveryService`, and `MapScatterService` all exist but have zero
  callers anywhere - no recent-files list, no autosave-recovery prompt on
  launch, and no scatter command are reachable from the editor UI at all.
- [x] Phase 15: standalone launch and shared planner/domain services are now
  the supported authoring path; the legacy planner source remains in the
  repository for comparison, but its Godot editor dock is no longer
  registered.

**Verification:** Godot Mono project build succeeds; all discovered suites pass
(Build 6, Core 17, DataValidator 4, Doctrine 14, M4 nation/enemy/wave 3, M4
tower 5, M5 signature/air 5, MapAuthoring 13, MapPlanning 6, Progression 18);
data validation reports 0 errors/0 warnings across 65 resources; authored
`editor_smoke_fixture` loads through the real mission scene; launcher screenshot
review passed at 1920×1080. A distributable `.exe` is still unavailable because
matching Godot Mono Windows export templates are not installed.
