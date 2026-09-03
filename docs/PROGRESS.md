# Progress against GDD §19's implementation ladder

Live tracker, updated as work lands. Checkboxes correspond 1:1 to GDD §19's
numbered prompts. Milestone exit criteria are GDD §17.1.

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
39–45. Not started.

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
  theater/terrain/flavor/UI assets are generated and marked `REVIEW`.
- [x] Deliberately held nation-specific units, enemies, enemy wrecks,
  national insignia, and tower/unit identity art pending implementation
  review, per the user's request.

---

## Historical blockers / current follow-up

No design blockers. D17 is superseded by D25; M4 prompts 27–30 and M5
prompts 31–34 are implemented. The next work is M6 doctrine and mode work,
after a .NET-enabled Godot playtest pass and review of the signature balance.
- **D17:** test framework selection is historical; D25 records the active
  GoDotTest choice and working headless command.
- **Art reference:** `docs/FRONTS OF WAR ART DESIGN.md` is the active
  visual-direction reference. The post-M4 environment-only generation pass
  created ten review assets; nation-specific units and enemies remain held.
- **Standalone .NET restore:** `dotnet build` cannot resolve
  `Godot.NET.Sdk/4.7.2` without access to NuGet; the Mono Godot build can
  still run the current project and smoke scene.
- **Map planner:** `docs/fronts_of_war_map_planner_design_spec.md`, the 100-
  template catalog, and the `map_planner` editor dock implement the M3.5
  design-time workflow. Accepted exports remain authored review artifacts;
  runtime procedural generation is still out of scope.
