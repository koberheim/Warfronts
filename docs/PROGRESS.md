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
Godot GoDotTest suite passes 14/14 tests, including the boss phase and Arsenal
resource checks. M3 remains primitive-art only; visual playtest and orphan-node
reporting are still manual editor checks.

## M4 through M8

27–45. Not started.

---

## Historical blockers / current follow-up

No current hard blockers. D17 is superseded by D25; M4 is intentionally not
started. The next work is visual playtest/art insertion and iteration of the
M3 slice, not new content.
- **D17:** test framework selection is historical; D25 records the active
  GoDotTest choice and working headless command.
- **Art reference:** `docs/FRONTS OF WAR ART DESIGN.md` is now the active
  visual-direction reference. Placeholder art generation is intentionally
  deferred until the M3 art pass; request it when that gate is ready.
- **Standalone .NET restore:** `dotnet build` cannot resolve
  `Godot.NET.Sdk/4.7.2` without access to NuGet; the Mono Godot build can
  still run the current project and smoke scene.
- **Map planner specification:** `docs/fronts_of_war_map_planner_design_spec.md`
  and the 100-template catalog are present as design-time tooling input. The
  planner itself is intentionally not implemented before the M3 gate closes.
