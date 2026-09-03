# Progress against GDD §19's implementation ladder

Live tracker, updated as work lands. Checkboxes correspond 1:1 to GDD §19's
numbered prompts. Milestone exit criteria are GDD §17.1.

## M0 — Foundation

1. [x] Godot 4.x project structure per §15.2, `.gitignore`, Git LFS config,
   C# `.csproj` with namespace folders. **Partial:** structure, gitignore,
   LFS, and csproj exist; the Mono Godot build loads the project and the
   headless smoke run starts successfully. Standalone `dotnet build` still
   needs an online restore of `Godot.NET.Sdk/4.7.2` in this environment.
2. [x] `GameLoop` autoload, fixed 60Hz tick via `_PhysicsProcess`,
   `TimeController` for 1×/2×/3×/pause. Written and exercised by the
   headless smoke run.
3. [x] `EventBus` (typed pub/sub) and `ObjectPool<T>`. Written; unit tests
   not yet written (blocked on D17 — test framework choice).
4. [x] `GameBalanceConfig`, `DamageTable`, `ArmorClass`/`DamageType` enums,
   pure `ResolveDamage`. Written; unit tests not yet written (D17).
5. [ ] `SeededRandom` wired to a per-mission seed. `SeededRandom` class is
   written; per-mission seeding isn't wired to anything yet since there's no
   mission flow (that's M3).

**M0 exit criteria (§17.1):** project structure, autoloads, damage resolver
with unit tests. **Not yet met** — automated unit tests are the remaining
gap (D17); Godot-side headless verification is now available via the Mono
build.

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
    early-call income implemented but not yet exercised (no wave-complete
    flow calls them yet — that's part of M3's mission flow).
13. [x] `WaveRunner` + `WaveDefinition`/`SpawnGroup`, single-wave playback —
    verified: authored spawn groups fire on their exact schedule.

**M0/M1 gap carried forward:** formal automated unit tests (D17) still
don't exist. Correctness so far is demonstrated by a live smoke run
(D19), which is real evidence but not a substitute for tests that catch
regressions automatically. Worth prioritizing early in M2.

## M2 — Slice systems

14. [x] `TowerUpgradeController`: 4 levels, branch fork at L3, GDD §7.4 cost
    curve, sell with 4s full-refund window. Verified by code review against
    the exact multipliers in the config; not yet exercised in the live
    smoke run (no UI action has upgraded/sold a tower in an automated test
    yet — the Tower Inspection Panel's buttons work when clicked by hand,
    but nothing scripts a click for headless verification).
15. [x] T3 Field Mortar / T9 Command Post support systems: densest-cluster
    point-targeting (T3) and non-stacking aura + CP generation (T9) are both
    implemented and wired into a live test scene (one Command Post next to
    the machine gun). Verified: the full 25-second smoke run has zero
    errors with the aura active. **Not yet separately verified:** the
    aura's exact numeric effect (+12% range / +8% rate of fire) — the
    smoke run confirms nothing broke, not that the bonus is precisely
    right; that needs a targeted check, noted as a near-term follow-up.
    T3 itself has no placed instance in the test scene yet (no mortar
    projectile/data asset authored) — the densest-cluster targeting code
    path is implemented but unexercised.
16. [x] `StatusController` (Suppressed, Spotted) with the 4-second
    non-refreshing cap. Wired into `EnemyController` (movement speed
    penalty, Spotted damage bonus) and `TowerController` (a parallel
    tower-suppression hook for the future Siege enemy). Not yet exercised
    live — nothing in the current test scene applies either status (no
    Siege enemy, no Marksman/Command Post Spotted source yet).
17. [x] `CommandPointLedger` and the three universal abilities (Artillery
    Strike, Rally, Emergency Repair) with cooldowns and CP cost checks.
    Implemented as `AbilitySystem`; callable via
    `MapRuntime.ActivateAbility(...)`. **Not yet wired to a hotbar UI** —
    the code path exists and compiles but has no on-screen button yet, so
    it's untested end-to-end. Near-term follow-up.
18. [x] In-mission HUD (Supply, Command Points, Defense Line, wave counter,
    speed/pause control) — `HudController`, built as Control nodes on a
    `CanvasLayer`, live-updating from the same events the smoke-test logger
    uses. Verified running without errors; visual appearance not checked
    (no screenshot taken — this session has no way to see the rendered
    output, only headless logs).
19. [x] Wave preview strip — `WavePreviewPanel`, three tiers of disclosure
    (full detail / archetypes-only / threat-badge-only) as specified.
    **Simplified:** text-only, no icon art; only tested against the one
    3-group test wave, so the "3 waves queued" case with real tier
    differences isn't exercised yet.
20. [x] Tower inspection panel — `TowerInspectionPanel`. Click-to-open,
    live stats, upgrade (with cost/affordability check), sell (with
    refund), all wired to the real ledgers. **Missing vs. full spec:**
    "Strong vs / Weak vs" icon rows, and lifetime damage-per-Supply
    tracking (needs per-tower damage attribution not yet threaded through
    the combat pipeline — see below).
21. [x] Post-mortem panel — `PostMortemPanel`. Leak tally, damage-by-type
    breakdown, unspent resources, and the exact suggestion rule from GDD
    §12.9's worked example (heavy armor leaked + low AP damage share →
    suggests AP towers). **Simplified:** triggers only on defeat
    (`DefenseLineDepletedEvent`) since there's no victory/mission-complete
    flow yet (that's M3's mission flow); doesn't identify most/least
    effective tower (same per-tower attribution gap as #20).
22. [x] Enemy health bars, armor-class glyphs, status badges, and the
    ricochet/ineffective-damage floating number feedback. Verified in the
    smoke run (health bars only draw once damaged, glyphs differ by armor
    class shape not just color, damage numbers are color+prefix coded).

**Known gap carried into M3:** per-tower lifetime damage attribution (for
"most/least effective tower" and damage-per-Supply) needs `EnemyDamagedEvent`
to carry a reference to whatever dealt the damage. Scoped out of this pass
to avoid touching the core damage pipeline again this late — worth doing
early in M3.

**Also carried forward:** formal automated unit tests (D17) still don't
exist. Everything above is verified by a live headless smoke run
(`scenes_root/mission.tscn`, `--headless --quit-after N`), which is real
evidence of correctness but doesn't run in CI or catch regressions
automatically.

## M3 — Vertical slice

23–26. Not started. **This is the real gate (§17.2)** — don't skip ahead to
content work before M1/M2 are solid and playable.

## M4 through M8

27–45. Not started.

---

## Known blockers

- **D17:** test framework (GoDotTest vs. GUT) not yet chosen — blocked on
  D13 resolving first, since evaluating either requires a working C# Godot
  instance. D13 is now resolved, so choosing and wiring the framework is
  actionable.
- **Standalone .NET restore:** `dotnet build` cannot resolve
  `Godot.NET.Sdk/4.7.2` without access to NuGet; the Mono Godot build can
  still run the current project and smoke scene.
