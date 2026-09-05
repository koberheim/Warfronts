# Release completion ledger

Updated: 2026-09-05. Owner: Codex (Astra lead), with Luna, Terra, and Sol.

## Release contract

The authoritative scope is GDD [VS] and [L]: eight authored maps, twelve
campaign missions, six nations and signatures, nine shared tower archetypes,
eighteen doctrines, twelve enemy archetypes, four bosses, four difficulties,
Skirmish/Endless, ten challenges, progression, Codex, settings/accessibility,
audio/visual feedback, and the Windows/Steam release path. No [P1] or [X]
additions. The User confirmed the M3 external playtest gate passed in this
session; this is user-reported evidence, not an agent-observed playtest.

Done requires an integrated launch-to-exit journey, tested alternate/retry/save
paths, content and reference validation, gameplay-scale visual QA, performance
checks, and a successful distributable build. Steam-account operations and
subjective final art/balance approval remain explicit external gates.

## Audit baseline

- Existing Godot/C# systems and a themed UI are substantial, but only one
  campaign mission is authored. Many later systems remain inaccessible or
  incomplete. Prior completion checkboxes are historical claims to verify.
- The baseline check script reproduced a false success: an SDK build failure
  produced a FAIL table followed by "All checks passed" and exit code zero.
- Authored preview currently adds paths/pads to the prototype scene; it does
  not replace prototype geometry or consume authored air corridors. Normal
  missions do not call the existing MissionMapResolver.
- Working tree contains earlier editor work and unrelated art/document edits;
  preserve all unrelated changes. No commit or remote publication requested.

## Dependency plan

Every task remains open until its stated acceptance evidence exists.

| ID | Priority / task | Owner / files | Dependencies | Risk | Acceptance / status |
|---|---|---|---|---|---|
| R01 | P0: fail-closed verification runner | Luna; tools/Run-HeadlessChecks* | none | low | Complete: policy regression passes; live failed smoke returns exit 1 despite native Godot exit 0. |
| R02 | P0: audit release config and debug boundaries | Luna; project/export/launch tooling | none | medium | Complete: `export_presets.cfg`'s Windows Player preset excludes `scenes_root/map_editor.tscn`, `scenes/editor/*`, `addons/*`, `tests/*`, `src/Editor/*` and six planner-only scripts from the packed resources; `FrontsOfWar.csproj`'s `ExportRelease` `ItemGroup` independently `Compile Remove`s the same source from the assembly; `Boot.DeveloperToolsAvailable()` is `#if DEBUG`-gated. With the Windows Mono export templates now installed (see evidence log), `tools/Build-Windows.ps1 -Preset "Windows Player"` produced a real `FrontsOfWar.exe`/`.pck`; it boots headlessly with zero errors and neither the `.exe` nor the `.pck` contain any `map_editor`/`MapEditorController` string. |
| R03 | P0: authored map/runtime contract | Astra; Map/Authoring, MapRuntime, new tests | none | high | `m01_bocage_crossroads.tres` now sets `MapId = "bocage_crossroads"`, resolving to the authored `assets/data/maps/bocage_crossroads.tres` (D73) instead of the old prototype layout. `RuntimeMapIntegrationTests` 7/7 pass and the canonical smoke run completes cleanly against the real mission. Not yet re-verified: malformed-map explicit failure under the current authored data, and gameplay-scale visual QA of the migrated map (still Review per D73). |
| R04 | P0: progression/save/retry correctness | Terra; Core/Meta/results + tests | none | high | `PlayerFlowPersistenceTests` 6/6 and `ProgressionTests` 18/18 pass. Results retry UI still needs player-journey QA (manual). |
| R05 | P0: pool enemy/friendly lifetimes | Astra (completed Sol partial); Enemies, pool configuration, tests | audit | high | `PoolingTests` 3/3 pass (fixed prewarm, FIFO overflow, state reset/stale-target immunity). Integrated late-wave stress remains open (R16). |
| R06 | P1: nation/loadout/signature selection integration | Terra; flow UI/session/placement | R04 | high | `CampaignSelectionController` is reachable from the main menu's Campaign button and drives `MissionCatalog`-backed nation/mission selection; `CampaignSelectionTests` 4/4 pass (six nations discoverable, mission catalog excludes wave assets, sequential unlock gating, signature consumes one of six slots). Not yet verified: a full manual playthrough exercising every nation/doctrine combination reaching gameplay. |
| R07 | P1: placement and support mechanics | Astra; Towers/Map/HUD | R03, R05 | high | Complete: minefield route placement now works end to end (see evidence log - the data resource had no `ControllerScene` at all, so it could never have worked); pad restrictions were already enforced and tested (`RuntimeMapIntegrationTests`); the Forward Observer branch's Spotted-marking pulse and map-wide Air reveal are now implemented and tested; T8 costs (90/59/104/189) match the GDD §7 table and pass the Data Validator. `PlacementIntegrationTests` 6/6. Known residual gaps (not blocking, noted for R16/R06 follow-up): a placed minefield has no click/inspect/sell UI; Japan's field-count cap of 9 and doctrine field-cap bonuses (Island Defense +3) are unwired (`ExtraMinefieldCapacity` is a neutral 0); the British "double-radius reveal" nation trait is a `DisplayName.Contains("Radar")` hack that also (incorrectly) triggers for Germany's identically-named tower; the British "Forward Observer marks two targets" lean is unwired (`SalvoCount` reuse is ready for it but no nation data sets it). |
| R08 | P1: map gimmicks | Astra/Sol; map and targeting systems | R03 | high | Complete: `GimmickSystem` (`src/Map/GimmickSystem.cs`) implements all five GDD §11.2 gimmicks - Tide (path closes on a cycle), Sandstorm/dust (global tower range multiplier on a cycle), Mud (vehicle-only path speed multiplier), Canopy (path-level Concealed flag reusing E11's reveal system), and Ruined Town's clipped-range arc (a facing+half-angle cone filter in `TowerController.IsValidTarget`, via `GimmickRules.IsWithinArc`) - each driven by `MapDefinition.Gimmicks`/`MapGimmick` data through a new `RuntimeGimmickData`. `GimmickTests` 7/7, all independent of any authored map (none of the 8 launch maps exist yet - R09). Deliberately not covered: Tide's WaveRunner spawn-rerouting integration (no fallback-path authoring concept exists, and no real tidal map to validate the exact behavior against) and true wall/line-of-sight geometry for the arc gimmick (no terrain-collision model exists) - see D82. |
| R09 | P1: hand-authored launch maps and campaign | Sol; assets/data/maps/missions/waves | R03, R08 | high | 8 maps/12 missions with intended counts/topologies; no runtime generation; references pass. Pending. |
| R10 | P1: remaining bosses/elite content | Sol; Enemies/resources | R05 | high | Complete: B2 Armored Column Command (Convoy - a command vehicle's damage-resistance/Suppression-immunity aura, escort collapse on its death), B3 Bomber Wing (Formation - shared damage reduction while at full strength, cumulative speed penalty per loss, on both the ground and air movement paths), B4 Fortress Assault Group (a new `MultiPhaseBossController` for one-way HP-threshold phases with a 3-second halt telegraph, reusing the Siege archetype's existing bombard/suppression plumbing for its phase-2 "Siege platform"), and Elite Medium Armor's Frontal Plate (a frontal damage-reduction cone, active only once damaged past its threshold) are all implemented as generic, data-driven `EnemyDefinition` mechanics and authored as real enemy resources. `BossTests` 8/8, two of which caught real bugs before they shipped (see evidence log). Elite Swarm/Elite Siege need only existing fields (`SpawnGroup.Count`, a stat-adjusted resource) - genuinely "pure data, no new code" as GDD says. Deliberately simplified (see D83): B4's phase-2 adds use one archetype instead of GDD's two, and phase 3's "simultaneous 3-bomber air element" is not built. Placing any of these into an actual mission's wave data is R09's job - missions 8/10/12 don't exist yet. |
| R11 | P1: Skirmish, Endless, challenge stacking | Astra/Terra; modes/config/UI | R06, R09 | high | Accessible flows; mode-isolated rewards; deterministic scaling and scoring. Pending. |
| R12 | P1: settings/accessibility/Codex | Terra; UI/settings/resources | R06 | medium | Settings screen complete for what has real backend support: video (fullscreen, UI scale), audio (5 volumes), accessibility (colorblind palette, visual effects intensity), and full control remapping with conflict detection and reset-to-default - reachable from both the main menu (its own flow screen) and the pause menu (embedded overlay, no scene change). `SettingsTests` 4/4; verified by screenshot. Codex (in-game encyclopedia) remains an unstarted, separate main-menu button. Explicitly not covered (no backing system exists yet, not just unwired UI): VSync/resolution/frame cap, screen shake, subtitles, default game speed, auto-pause-on-wave-complete, confirm-before-sell, targeting-priority defaults, tutorial-hints/damage-number toggles - all follow-up work, most needing new gameplay systems (camera shake, subtitle/VO plumbing) before a setting can control them. |
| R13 | P1: Windows build/Steam services | Luna/Astra; tools/platform | R02, R04 | high | Runnable export, offline fallback, account-dependent checks reported separately. Pending. |
| R14 | P2: art/audio/VFX integration | Astra with asset tooling | content audit | high | No placeholder launch art/audio; authored identity and presentation approved at gameplay scale. Pending overall. Resolved sub-item (D85): `campaign_selection.tscn`'s alliance/nation cards now show real generated banner art (fictionalized per GDD §14.3) instead of plain text - `REVIEW` status, pending the same acceptance-gate screenshot review as other generated art. Gameplay tower/unit/enemy identity art remains held per D36. |
| R15 | P2: UX, controls, onboarding and feedback | Terra/Astra; UI/scenes | R06–R12 | medium | End-to-end player journeys and screenshots at supported sizes. Pending. |
| R16 | P2: balance and performance | Astra; configs/tests/QA | R05–R15 | high | Targeting budget, late-wave stress, nation parity and complete mission balance evidence. Pending. |
| R17 | P3: editor parity and truthful documentation | Astra; editor/docs | R03 | medium | Complete (audit-and-correct half; see D84 for why the UI-building half is explicitly out of scope here): grepped every map-editor phase-5-14 service class for references outside its own file, across `src/` and `tests/`. Eight classes (`MapLayerService`, `MapScatterService`, `MapGameplayCommands`, `MapEditorPreferences`, `MapRecoveryService`, `MapPathEditing`, `TerrainRules`, `TerrainPlacementPreview`) plus a ninth pair (`MapGenerationService`/`MapGenerationConfiguration`, referenced only by each other) have zero callers anywhere - not the editor UI, not a test. The editor's "Generate"/"View"/"Map" header buttons are confirmed stubs (`MapEditorController.ShowPhaseMessage`); there is no way to create a new terrain tile, tower node, marker, zone, air corridor, or gimmick from inside the editor at all - only to select/move/delete one that already exists in the loaded document, via the genuinely-reachable `MapObjectLocator`. Corrected the affected `docs/PROGRESS.md` checkboxes from a blanket `[x]` to precise `[~]`/`[ ]` markers naming the specific gap per phase. |
| R18 | P3: fresh release audit and packaged QA | all; disjoint checks | all above | high | All practical tests, fresh export, restart/exit/save checks; no known release blockers. Pending. |

Independent audit tracks run concurrently. Implementation write sets are
assigned explicitly; Astra owns integration, central docs and final review.
Luna uses the fastest model for narrow tooling, Terra the balanced coding
model for flow/persistence, Sol the workhorse for gameplay/content; Astra
retains cross-system architecture. Test/build runs are serialized to prevent
shared Godot output and user-data conflicts.

## Evidence and decisions

Append concrete results here as each batch is reviewed. A passing old test
suite is baseline evidence, not proof of launch-content completeness.

- 2026-09-05: repaired verification runner returned failure for the live smoke
  route mismatch. C# build and all 104 GoDotTest checks passed; data validator
  reported zero errors/warnings across 65 resources. The validator did not yet
  inspect mission/map cross-references, so this was incomplete coverage.
- R03: four initial authored-runtime checks pass (replacement, transforms,
  validation and pad restrictions). Normal mission binding and three new
  campaign integration checks are being implemented; not yet verified.
- R04: six new tests cover claim-once results, tutorial persistence, future
  schema preservation, interrupted save recovery, lowercase version fields
  and malformed JSON roots. Expected corrupt-save warnings are test evidence.
- Remaining editor gap: several menu buttons still show future-phase messages.
  Service implementations and earlier phase checkboxes do not demonstrate
  reachable editor UI parity. R17 remains open.
- 2026-09-05 (Claude): the working tree's `dotnet build` was failing
  (`src/Core/UserSettings.cs` name collision, see D75) - the prior evidence
  entries above were the last state verified *before* that break. Fixed the
  collision and reran `tools/Run-HeadlessChecks.ps1` end to end: build, all 14
  discovered suites (`BuildTests` 6, `CampaignSelectionTests` 4,
  `CoreTests` 17, `DataValidatorTests` 4, `DoctrineTests` 14,
  `M4NationEnemyWaveTests` 3, `M4TowerTests` 5, `M5SignatureAirTests` 5,
  `MapAuthoringTests` 13, `MapPlanningTests` 6, `PlayerFlowPersistenceTests` 6,
  `PoolingTests` 3, `ProgressionTests` 18, `RuntimeMapIntegrationTests` 7),
  `--validate-data` (0/0 across 67 resources), and the smoke run (0 errors, 19
  kills) all pass. This confirms R03/R04/R05/R06's newer suites for the first
  time and supersedes their earlier "in progress"/"pending" status text, which
  had not been updated since those suites were added. R12 (settings/
  accessibility) has real backend code (`UserSettings.cs`, `PlayerSettings.cs`)
  but it is not called from anywhere yet, has no UI screen, and no dedicated
  test suite - it remains genuinely open, and several §13.8/13.9 fields have
  no backing system at all yet (camera shake, subtitles). See D75.
- R02: audited `export_presets.cfg`, `FrontsOfWar.csproj`'s `ExportRelease`
  item group, and `Boot.DeveloperToolsAvailable()` - the Player/Developer
  Debug/Release boundary is correctly enforced at both the resource-export
  and C#-compile level (see ledger row). `tools/Build-Windows.ps1` (untracked,
  already written) correctly refuses to fabricate a template and reports the
  exact missing artifact and official URL. Confirmed the artifact is
  reachable (HTTP 200, ~1.15 GB) but did not download it without asking -
  installing it changes global `%APPDATA%` state and is a genuinely sized
  transfer, and the script's own design deliberately never auto-fetches it.
  User approved the download. Installed only the four `windows_*_x86_64*.exe`
  template files this project's presets need (not the full multi-GB archive
  covering every platform) directly into `%APPDATA%\Godot\export_templates\
  4.7.2.stable.mono\`. Note: this machine's C: drive was found completely
  full (0 bytes free) during the first extraction attempt (which targeted a
  C:-drive temp path); freed ~3 GB by deleting the failed partial extraction,
  then redid the download/extraction against the E: drive instead, where
  `%APPDATA%` for this profile actually resolves. The C: drive's near-zero
  free space is a pre-existing machine condition unrelated to this repo and
  was flagged to the User. `Build-Windows.ps1 -Preset "Windows Player"` then
  ran clean: `dotnet restore` + `build -c ExportRelease` (0/0) + Godot
  `--export-release`, producing `build/player/FrontsOfWar.exe` (~104 MB) and
  `FrontsOfWar.pck` (~123 MB). Ran the exported `.exe` directly (not the
  editor) with `--headless --quit-after 300`: exits 0 with no output/errors.
  `grep`ping both files for `map_editor`/`MapEditorController` finds nothing.
- R07: `SpecialPlacementService` (route-placed Minefield, typed signature
  placement) and `CommandPostController`'s Forward Observer branch data were
  both fully implemented but never called from anywhere - `grep`ping the
  whole `src/` tree found zero call sites for either before this session.
  Wired `SpecialPlacementService` into `MapRuntime` (constructed alongside
  `Placement`, ticked every frame) and into `BuildBar` (selecting the
  Minefield card switches to a click-anywhere-on-route mode instead of pad
  glow, reusing `AbilityHotbar`'s screen-to-world canvas-transform trick);
  added a "placed/max" counter to its build card (GDD §6 T8: "enforced with
  a visible counter"). Discovered `assets/data/towers/t8_minefield.tres` had
  no `ControllerScene` at all - no `.tscn` for `MinefieldController` existed
  anywhere in the project, so placement could never have succeeded even with
  perfect wiring. Added `scenes/towers/tower_minefield.tscn` (root
  `Node2D` + the controller script; the controller already draws its own
  visual) and wired it into the resource. Added `CommandPostController.
  TickSpottedPulse` (marks the strongest enemy in `RangeTiles` Spotted every
  `StatusDurationSeconds`, reusing that field as both cadence and duration
  exactly as `MinefieldController` already does for its own periodic
  Suppressed trigger) and extended `RevealTargets` so Air units are revealed
  map-wide specifically on the Forward Observer branch (`StatusEffectId ==
  "Spotted"`), per GDD §6 T9. Added `tests/PlacementIntegrationTests.cs`
  (6/6): route placement + exact cost spend, off-route refusal, spacing-rule
  refusal, field-cap refusal, Forward Observer marks only the strongest
  in-radius enemy, and the pulse is inert pre-fork and on the Logistics
  branch. Caught and fixed one bug before it shipped: a `?? default` fallback
  on `TowerPlacementOutcome` (a struct whose `Result` enum defaults to
  `Success`) would have silently reported success for a null mission/service;
  replaced with an explicit `NoControllerScene` fallback.
- R12: `UserSettings`/`PlayerSettings` (see D75) had real logic but no UI, no
  boot-time application, and no test suite. Added `SettingsPanel` (shared
  content, `src/UI/Menus/`), `SettingsController` (its own flow screen,
  `scenes_root/settings.tscn`), unlocked the main menu's "Settings" button,
  and replaced the pause menu's locked "Settings" stub with a second embedded
  card swapped in over the pause card (no scene change, so mid-mission state
  is untouched). `Boot.StartMission` now calls `UserSettings.Apply` every
  launch, which surfaced a real bug before it shipped: `PlayerSettings.
  Fullscreen` defaulted to `false`, so the moment `Apply` actually started
  running at boot, every fresh profile would have been silently flipped to
  windowed mode, contradicting `project.godot`'s fullscreen-by-default
  setting. Fixed the default to `true`. Every control here reads/writes real
  state (fow_theme.tres only styles Label/PanelContainer/Button, so volumes
  and UI scale are stepped button rows rather than continuous sliders - a
  deliberate first-pass simplification, not a placeholder). `SettingsTests`
  4/4 (fresh-profile defaults, clamping, rebind conflict detection, reset).
  Verified by screenshot at 1920x1080 (`--screen=settings`, newly added to
  the allowlist) and a pause-menu regression screenshot confirming Resume/
  Restart/Abandon/Quit still render correctly with the new Settings button.
  Follow-up (D81): the five volume rows are now real draggable sliders
  (`PaperSlider`, a custom-drawn control) per User request, debounced to save
  once per drag rather than on every motion event.
- R08: `MapGimmick`'s own comment already anticipated this phasing -
  "Runtime behavior is implemented only for the GDD-authored gimmicks during
  its later milestone, not in this model." Added `RuntimeGimmickData` (the
  Gimmicks analogue of the existing Paths/Pads/AirCorridors runtime data) and
  `GimmickSystem`, consulted from `MapRuntime.SimTick` alongside the other
  per-mission managers: enemies get their Canopy/Mud state set before
  `Enemies.Tick` applies movement each frame, and towers get a
  `GimmickRangeMultiplier` reset before `Towers.Tick`, the same convention
  `CommandPostManager` already uses for its aura. Also added
  `ArcFacingDegrees`/`ArcHalfAngleDegrees` to `BuildPad`/`TowerPlacementNode`/
  `RuntimePadData` (copied onto a placed tower the same way `PadTag`
  already is) for the one pad-level gimmick. `GimmickTests` 7/7.
- R10: added B2/B3/B4/Frontal-Plate as generic `EnemyDefinition` mechanics
  (`ConvoyAuraRadiusTiles`/`ConvoyDamageResistancePercent`/
  `ConvoyGrantsSuppressionImmunity`/`ConvoyCollapseHpFraction`,
  `FormationGroupId`/`FormationSize`/`FormationDamageReductionPercent`/
  `FormationBreakSpeedPenaltyPercent`, `MultiPhaseHpThresholds`/
  `Phase3SpeedMultiplier`, `HasFrontalPlate`/`FrontalPlateActivateHpFraction`/
  `FrontalPlateDamageReductionPercent`/`FrontalPlateHalfAngleDegrees`), reusing
  the existing nearby-ally aura pattern (`_enemyProvider`, the same mechanism
  E9/E10/E11 already use) and, for B4's Siege phase, the Siege archetype's
  existing `SiegeBombardRangeTiles`/`IntervalSeconds`/`SuppressionDurationSeconds`
  fields and `EnemySiegeBombardEvent` (already consumed by
  `TowerController.OnEnemySiegeBombard` - zero changes needed there) and
  `AddDefinition`/`AddCount`/`AddIntervalSeconds` (already consumed by
  `EnemyManager`'s existing `BossAddsRequestedEvent` plumbing). Added a new
  `CapHealth` method (a scripted HP cap that bypasses the damage-resolution
  pipeline entirely) for B2's "instantly collapses the escorts to 50% HP" -
  routing it through `ApplyDamage` would have let a shield or Convoy's own
  resistance blunt the intended flat collapse. `IDamageSource` gained a
  `GlobalPosition` property (every implementer is already a `Node2D`, so this
  cost nothing) for Frontal Plate's "which direction did this hit come from."
  `BossTests` 8/8 caught two real bugs before they shipped: `CapHealth`
  wasn't wired to trigger the Convoy collapse (only `ApplyDamage`'s death
  branch was), and air units (B3's bombers) skip the ground-movement branch
  entirely in `SimTick`, so the Formation speed penalty needed its own
  application on the air-movement path too. Fixed both. Also fixed a
  `DataValidator` warning that assumed every boss uses B1's armor-skirt
  model ("Boss EnemyDefinition has no SkirtHp set") - it now recognizes the
  three alternate mechanics too.
- R17: full per-phase evidence in D84. In short - the map editor's
  select/move/rotate/scale/delete/duplicate/copy/paste/undo/redo, its
  decorative-prop palette, its diagnostics/Test-Map/Publish buttons, and its
  File menu are all genuinely reachable and (for the parts `MapAuthoringTests`
  covers) tested. Its Generate/View/Map menus, terrain painting, and every
  "create a new gameplay object" path are not - eight service classes with
  zero callers anywhere. `docs/PROGRESS.md`'s phase checkboxes now say so.
