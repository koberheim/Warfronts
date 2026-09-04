# Fronts of War — UI design specification

Version 1.0 (2026-09-04). Owner: UI lead agent. Companion to GDD §13 (UI/UX),
§3.4 (war-table framing), §16 (art), and `docs/FRONTS OF WAR ART DESIGN.md`
§29–31 (hybrid war table, UI materials, iconography). Where this document and
the GDD disagree, the GDD wins; this document adds the concrete visual system
the GDD leaves open.

Everything here is implemented through one theme resource
(`godot-project/assets/ui/theme/fow_theme.tres`), one palette mirror for
`_Draw` code (`src/UI/Theme/UiPalette.cs`), one icon registry
(`src/UI/Theme/UiIcons.cs`), and the material textures under
`assets/ui/materials/`. Screens set `ThemeTypeVariation` names from §7 and
never invent their own colors, fonts, or style boxes.

---

## 1. Identity

**The commander's map table around a living painted battlefield.** The
interface is the wooden rim, brass fittings, paper dispatches and grease
pencil of a field headquarters table. The battlefield inside it is a painted
map with counters on it. Precision comes from modern information hierarchy;
warmth comes from the physical materials.

Reference feel: Kingdom Rush's clarity, a field-manual's iconography, a 1940s
operations room's typography. Not: a debug overlay, a web dashboard, a
mobile F2P skin, a sci-fi HUD.

### Principles (in tiebreak order)

1. **Readable at 2× speed at 1920×1080.** Every number the player acts on
   is ≥ 16 px; every glyph is legible at 24 px; every state has a shape
   difference, not only a color difference (GDD §13.9).
2. **Material is the skin, hierarchy is the structure.** Paper, brass and
   wood carry the menus, briefings, cards and reports. The in-mission HUD
   uses them lightly: dark painted-metal panels with brass accents, so the
   battlefield stays the brightest thing on screen (art doc §29).
3. **Diegetic where it is free, plain where it is not.** The wave preview is
   a teletype strip; tooltips and tower cards are paper; the speed control is
   a brass lever plate. Buttons remain obviously buttons.
4. **One accent means one thing.** Amber = selection / attention / build mode.
   Red = danger / defeat / defense line critical. Blue = shields. Green =
   healing / success. Brass = primary action. Nothing else borrows these.
5. **No motion faster than 3 Hz, nothing longer than 200 ms.** Hover and
   press feedback is instant; panels fade or slide in ≤ 180 ms; nothing
   loops except the radial cooldown sweep and the build-timer ring.

---

## 2. Canvas and layout

- **Design resolution 1920×1080**, `canvas_items` stretch, `expand` aspect,
  **fullscreen by default** (`window/size/mode=3` in `project.godot`). The
  reference display is a 24-inch 1080p monitor: every screen must fit
  exactly 1920×1080 with nothing clipped, and wider windows gain
  battlefield, not stretched chrome. UI is authored in 1080p pixels and
  anchors to screen edges or center, never to absolute coordinates.
- **Spacing unit 8 px.** Safe margin from the screen edge: 24 px. Panel
  content padding: 14/10 (slate) or 30/26 (paper). Gaps between cards: 8 px.
- **Battlefield framing.** The mission scene carries a `Camera2D`
  (`zoom 1.6`, centered on the authored playfield) and a full-screen
  war-table frame behind the world (`FramePanel` on a `CanvasLayer` at
  layer −10). Gameplay stays in world space; HUD stays in screen space.
  World→screen conversion for anchored UI goes through
  `Node2D.GetGlobalTransformWithCanvas()`; screen→world for targeting goes
  through `GetViewport().GetCanvasTransform().AffineInverse()`.

### 2.1 In-mission HUD zones (1920×1080)

```
┌────────────────────────────────────────────────────────────────────────┐
│ [A] Resources    [B] Wave counter + teletype preview strip   [C] Speed │  y 0–112
│                                                                        │
│                                                                        │
│                         B A T T L E F I E L D                          │
│                                                                        │
│ [F] Inspection card (anchored to the selected tower)                   │
│                                                                        │
│ [D] Build timer / Call wave   [E] Build bar (6 cards)   [G] Abilities  │  y 920–1080
└────────────────────────────────────────────────────────────────────────┘
```

| Zone | Anchor | Size | Contents |
|---|---|---|---|
| A Resources | top-left, 24/16 | ≈ 360×96 | Supply (icon, number, `+income/wave` caption), Command Points (icon, number), Defense Line segmented bar (20 segments, label `17 / 20`) |
| B Wave | top-center | ≈ 880×104 | `WAVE 4 / 12` heading; below it the teletype strip with three cards N+1 / N+2 / N+3 (§8.4); air-warning badge at the strip's right end when any previewed wave has air |
| C Time | top-right, 24/16 | ≈ 300×56 | Brass lever plate: three segments `1× 2× 3×`, the active one pressed; Pause button; Menu (gear) button |
| D Build phase | bottom-left, 24/24 | ≈ 320×120 | During build time: `BUILD PHASE` caption, countdown ring with seconds, `Call Wave Early  +NN Supply` primary button. During a wave: `ENGAGED` caption and nothing else |
| E Build bar | bottom-center | 6 × 120×128 + gaps | One card per loadout tower: hotkey plate top-left, icon, name, cost with supply icon; disabled with `Need +NN` when unaffordable; amber pressed state while selected |
| F Inspection | world-anchored | 340 wide | Paper card, see §8.5 |
| G Abilities | bottom-right, 24/24 | 4 × 104×128 | Cards 1–3 universal, 4 doctrine; radial cooldown sweep over the icon; CP cost badge; status line above the row |

Pause: a `PAUSED — planning mode` banner (slate strong, amber text) at
top-center just below the wave strip; the pause menu (§8.7) is a paper card
centered on screen.

---

## 3. Materials

| Material | Asset | Used for |
|---|---|---|
| War-table frame | `assets/art/shared/ui/commander_map_table_frame_v01.png` (approved ART-ENV-010), as `FramePanel` 9-patch (margins 330/250) | Full-screen backdrop of every flow screen and the mission's battlefield |
| Paper card | `assets/ui/materials/paper_card_9p.png` (parchment crop of the frame, deckled edge, drop shadow), `PaperPanel` | Briefing order, loadout cards, doctrine cards, tooltips, inspection card, tutorial card, pause menu, post-mortem report, results sheet |
| Teletype strip | `assets/ui/materials/teletype_strip_9p.png`, `TeletypePanel` | Wave preview strip |
| Brass plate | `assets/ui/materials/brass_plate_9p.png`, `PrimaryButton` | The one primary action per screen; the speed lever plate |
| Wood rail | `assets/ui/materials/wood_rail_h_9p.png`, `WoodRailPanel` | Optional top/bottom rails on flow screens; not used in the mission HUD |
| Painted metal (slate) | `StyleBoxFlat` in the theme, `SlatePanel` / `SlatePanelStrong` / `SlotPanel` | All in-mission HUD chrome |

Rule: **slate in the HUD, paper in the menus.** The only paper inside the
mission is anchored to a tower (inspection), the tutorial, the pause menu and
the post-mortem — things that pause or overlay play.

---

## 4. Palette

Tokens live in the theme; `UiPalette` mirrors them for `_Draw`. Hex values
are the specification.

| Token | Hex | Role |
|---|---|---|
| `ink` | `#2B221A` | Text on paper |
| `ink_muted` | `#5C4E3E` | Secondary text on paper, paper separators |
| `paper` | `#E8DCC0` | Paper fill (flat), paper buttons |
| `paper_dark` | `#D6C6A2` | Paper hover |
| `paper_edge` | `#B9A57C` | Paper pressed, paper rules |
| `wood_dark` / `wood_mid` | `#3A2416` / `#5A3A22` | Wood rails (texture), drop shadows on paper |
| `brass` / `brass_hi` / `brass_lo` | `#C9A24A` / `#E8CB7A` / `#8A6A2A` | Primary action, hover border, lever plate, progress fill |
| `slate` | `#1C2128` @ 93 % | HUD panel fill |
| `slate_hi` | `#2A313B` | HUD buttons, slots |
| `slate_line` | `#4A5262` | HUD borders, separators |
| `cream` | `#EFE3C8` | Text on slate |
| `cream_muted` | `#B9AE95` | Secondary text on slate |
| `olive` | `#6B7A3D` | Friendly/Allied accent (aura rings, friendly outlines) |
| `amber` | `#E0A83A` | Selection, build mode, focus ring, pause banner |
| `red` | `#B8362B` | Defense line ≤ 25 %, defeat, enemy accent, suppression overlay |
| `blue` | `#4FA3C7` | Shields |
| `green` | `#5F9E4A` | Repair, success, unlock |
| `stamp` | `#A5372B` | Ink-stamp headings on paper (`StampLabel`) |
| `sa` / `he` / `ap` / `aa` | `#D9C36A` / `#E0763A` / `#7FA7D9` / `#A9D9E8` | Damage types (always with their glyph) |
| `grey` | `#9A9A9A` | Ineffective hits, disabled |

Enemies never take a nation hue in UI: enemy accent is always `red`, friendly
is always `olive`, matching GDD §13.9 "faction color handling". Colorblind
palettes (§13.9) will be delivered later as alternate theme resources that
override only the accent tokens; nothing in this spec depends on hue alone.

---

## 5. Typography

Three families, all OFL-licensed and vendored under `assets/art/fonts/`:

| Family | File | Voice | Use |
|---|---|---|---|
| **Oswald** (variable) | `oswald/Oswald-Variable.ttf` | condensed poster / stencil | Titles, headings, captions, primary buttons, hotkey plates. Always uppercase when used as a heading (`Label.Uppercase = true`) |
| **Barlow** | `barlow/Barlow-{Regular,Medium,SemiBold,Bold}.ttf` | clean mid-century grotesk | Body, numbers, every HUD value, buttons |
| **Courier Prime** | `courier_prime/CourierPrime-{Regular,Bold}.ttf` | teletype / typewriter | Wave strip, briefing body, post-mortem report, mission slips |

Scale (px at 1080p): title 44 · heading 24–26 · subheading 18 · body 16–17 ·
number 22 · small 13 · caption 14 · mono 15–16 · damage number 18 with a 4 px
dark outline. Never below 13 px.

---

## 6. Iconography

Monochrome SVG glyphs, 64×64 viewBox, single fill `#FFFFFF` (tinted in
place), 4 px minimum stroke, bold silhouette, no interior detail smaller than
6 px, no text. Style: illustrated field-manual symbols — simple, geometric,
slightly hand-cut. Files live at `assets/ui/icons/<id>.svg` and are resolved
through `UiIcons.Get(id)`; a missing id returns `null` and the caller falls
back to text.

Required ids:

| Group | Ids |
|---|---|
| Resources | `resource_supply` (supply crate), `resource_cp` (command flag), `resource_defense_line` (sandbag line) |
| Time | `speed_1`, `speed_2`, `speed_3` (one, two, three chevrons), `pause`, `play`, `settings`, `close`, `menu` |
| Waves | `wave`, `air_warning` (wing with exclamation), `call_wave_early` |
| Damage types | `damage_small_arms` (bullet), `damage_explosive` (burst), `damage_armor_piercing` (chevron), `damage_anti_air` (wing) |
| Armor classes | `armor_soft` (cloth square), `armor_hardened` (half shield), `armor_armored` (full shield), `armor_heavy` (double shield) |
| Status | `status_suppressed`, `status_spotted`, `status_shielded`, `status_concealed` |
| Threat badges | `threat_air`, `threat_siege`, `threat_support`, `threat_concealed`, `threat_boss` |
| Matchup | `matchup_strong` (filled check), `matchup_partial` (half circle), `matchup_weak` (cross), `ineffective` (down chevron) |
| Abilities | `ability_artillery_strike`, `ability_rally`, `ability_emergency_repair`, `ability_doctrine` |
| Towers (by `TowerDefinition.Id`) | `tower_t1_automatic_gun`, `tower_t2_marksman_post`, `tower_t3_field_mortar`, `tower_t4_anti_tank_gun`, `tower_t5_flak_battery`, `tower_t6_armored_emplacement`, `tower_t7_heavy_artillery`, `tower_t8_minefield`, `tower_t9_command_post`, `tower_signature` |
| Enemies (by archetype) | `enemy_infantry`, `enemy_fast_infantry`, `enemy_swarm`, `enemy_armored_infantry`, `enemy_light_vehicle`, `enemy_medium_armor`, `enemy_heavy_armor`, `enemy_air`, `enemy_support`, `enemy_escort`, `enemy_recon`, `enemy_siege`, `enemy_boss` |
| Progress | `star_filled`, `star_empty`, `rank_chevron`, `lock`, `check`, `upgrade_arrow`, `sell`, `level_pip_on`, `level_pip_off`, `branch_a`, `branch_b` |
| Nations | `nation_united_states`, `nation_britain`, `nation_soviet_union`, `nation_germany`, `nation_italy`, `nation_japan` — **fictionalized abstract roundels/shapes only** (GDD §14.3): a star-in-ring, a segmented roundel, a hammer-free red star variant is NOT allowed — use a plain five-point star, a plain roundel, a plain lozenge, a plain shield, a plain chevron, a plain disc. No crosses, no eagles, no real insignia. |

Tinting: icons on slate use `cream`; on paper use `ink`; damage/armor glyphs
use their token color plus the glyph shape; disabled icons use `grey`.

---

## 7. Component library (theme type variations)

| Variation | Base | Look | Use |
|---|---|---|---|
| `TitleLabel` / `PaperTitleLabel` | Label | Oswald 600 44 | Screen titles |
| `HeadingLabel` / `PaperHeadingLabel` | Label | Oswald 500 24/26 | Section headings, tower name |
| `SubheadingLabel` / `PaperSubheadingLabel` | Label | Oswald 500 18, muted | Sub-sections, card kickers |
| `BodyLabel` / `PaperBodyLabel` | Label | Barlow 16/17 | Body copy |
| `SmallLabel` / `PaperSmallLabel` | Label | Barlow 13, muted | Captions, shortfall text |
| `NumberLabel` / `PaperNumberLabel` | Label | Barlow SemiBold 22 | Resource values, costs |
| `MonoLabel` / `PaperMonoLabel` | Label | Courier Prime 15/16 | Teletype and typewriter text |
| `CaptionLabel` | Label | Oswald 400 14, muted | Uppercase zone captions (`BUILD`, `ABILITIES`) |
| `StampLabel` | Label | Oswald 700 20, stamp red | `OPERATION ORDER`, `MISSION FAILED` stamps on paper |
| `DamageNumberLabel` | Label | Barlow Bold 18, outlined | Floating damage numbers |
| `SlatePanel` | PanelContainer | dark metal, 1 px line | HUD clusters |
| `SlatePanelStrong` | PanelContainer | dark metal, 1 px brass | Banners, popups |
| `SlotPanel` | PanelContainer | lighter slate, 4 px radius | Empty card slots |
| `PaperPanel` | PanelContainer | parchment 9-patch with shadow | Every card and sheet |
| `FramePanel` | PanelContainer | the war-table frame | Full-screen backdrop |
| `TeletypePanel` | PanelContainer | paper strip with feed lines | Wave preview |
| `WoodRailPanel` | PanelContainer | wood rail | Flow-screen rails |
| `Button` (default) | Button | slate, brass on hover | Secondary actions on slate |
| `PrimaryButton` | Button | brass plate, Oswald 20 ink | The one main action per screen (Deploy, Continue, Retry) |
| `PaperButton` | Button | cream, 2 px ink border | Actions on paper cards |
| `CardButton` | Button | slate card, amber when pressed/toggled | Build bar, ability cards, doctrine cards (toggle mode) |
| `GhostButton` | Button | outline only | Tertiary actions (Close, Back) |
| `PaperSeparator` | HSeparator | faint ink rule | Rules on paper |
| `PaperRichText` | RichTextLabel | ink body with bold/mono | Formatted paper copy |

States every interactive control must show: normal, hover, pressed/toggled,
disabled, keyboard focus (2 px amber ring from `sb_focus`). Disabled controls
keep their label readable (≥ 50 % alpha) and say *why* where it matters
(`Need +40`, `Cooling down 12s`, `Used this mission`).

Sizes: buttons ≥ 40 px tall; card buttons 120×128 (build) and 104×128
(abilities); hit targets ≥ 32 px.

---

## 8. Screens

### 8.1 Main menu (`scenes_root/main_menu.tscn`)

Full-screen `FramePanel`. Left third: a paper sheet with the title
`FRONTS OF WAR` (`PaperTitleLabel`, uppercase), a one-line kicker
(`PaperSubheadingLabel`: "A commander's table. Six nations. One front."),
and a vertical button column: **Campaign** (`PrimaryButton`) · Skirmish ·
Endless · Codex · Settings (all `PaperButton`, disabled with a small
`lock` icon and the caption "Later in development") · **Quit**
(`GhostButton`). Right two-thirds: empty table (later: campaign map).
Bottom-right small caption: build/version string. Campaign goes to Briefing.
`Boot` enters this screen unless `--mission` / `--screen` is given.

### 8.2 Briefing (`briefing.tscn`)

Full-screen `FramePanel`. Centered paper sheet 900×640: stamp
`OPERATION ORDER` top-left (`StampLabel`), mission title (`PaperTitleLabel`),
act/map kicker (`PaperSubheadingLabel`: "Act I · Western Europe"), a rule,
briefing body in `PaperMonoLabel` (typewriter), then a three-column
"intelligence" row: waves (`wave` icon + count), known threats (armor / air
badges derived from the wave sequence), signature available. Bottom-right:
**Continue to Loadout** (`PrimaryButton`). Bottom-left: **Back**
(`GhostButton`) to the main menu.

### 8.3 Loadout (`loadout.tscn`)

Full-screen `FramePanel`. Title row: `LOADOUT` + nation name and the
nation's fictional roundel icon. Left: six tower cards in a 3×2 grid
(`PaperPanel`, 200×150): tower icon, name (`PaperHeadingLabel` 20),
damage-type glyph + label, cost, hotkey plate. These are the fixed
recommended kit for now (§13.3's picker is later work); a caption says
"Recommended kit". Right: three doctrine cards (`CardButton`, toggle group,
260×220): name, `Passive:` line, `Ability [name]:` line; selected card shows
the amber state and a `check` icon. Below: difficulty row (label "Regular"
for now). Warning banner slot (§13.3) rendered when the loadout has no AP or
no AA source and the mission has armor or air — informational, `SlatePanelStrong`
with `air_warning`. Bottom-right: **Deploy to <mission>** (`PrimaryButton`).
Bottom-left: **Back** (`GhostButton`).

### 8.4 Mission HUD (`mission.tscn`, `HudController` + children)

Zones per §2.1. Details:

- **Resources (A).** A `SlatePanel` with three rows. Supply row: `resource_supply`
  icon 24 px, `NumberLabel` value, `SmallLabel` "+NN / wave" (projected
  income). CP row: `resource_cp` icon, `NumberLabel`. Defense Line row:
  `resource_defense_line` icon, a custom-drawn segmented bar 240×14 (20
  segments, `olive` fill, `red` fill at ≤ 25 %, `slate_hi` empty, 1 px
  `slate_line` between segments), `SmallLabel` "17 / 20". The whole panel
  pulses once (alpha 1→0.6→1, 180 ms) when a value drops.
- **Wave strip (B).** `HeadingLabel` "WAVE 4 / 12" centered. Below, a
  `TeletypePanel` 860×72 with three cards separated by faint rules:
  N+1 shows archetype icons with `×count` (`MonoLabel`) plus armor glyphs and
  threat badges; N+2 shows archetype icons only; N+3 shows threat badges only
  (`air_warning` etc.) or "Ground forces". Enemy names come from
  `EnemyDefinition.DisplayName`, never ids. The strip's right cap shows the
  `air_warning` badge in `amber` whenever any previewed wave contains air.
- **Time (C).** A `PrimaryButton`-styled lever plate split into three
  `CardButton`s (`1×` `2×` `3×`, toggle group) + `pause` button + `menu`
  button (opens the pause menu). Space cycles speed, P pauses (existing).
- **Build phase (D).** `SlatePanel` with `CaptionLabel` "BUILD PHASE",
  countdown ring (custom `_Draw`: 44 px ring, `amber` sweep, seconds in
  `NumberLabel`), and the `PrimaryButton` "Call Wave Early  +NN" where NN is
  the exact bonus from the wave runner / balance config. During a wave the
  panel shows `CaptionLabel` "ENGAGED" and hides the button.
- **Build bar (E).** `SlatePanel` with `CaptionLabel` "BUILD" and six
  `CardButton`s. Card layout top-to-bottom: hotkey plate (Oswald 14 on a
  `slate_hi` chip, top-left corner), tower icon 40 px, name (Barlow 13, two
  lines max), cost row (`resource_supply` 14 px + number). Unaffordable:
  disabled, cost row replaced with "Need +NN" in `SmallLabel`. Selected:
  toggled (amber). Hover shows a paper tooltip: damage type glyph + name,
  range in tiles, strong vs / weak vs rows.
- **Abilities (G).** `SlatePanel` with `CaptionLabel` "ABILITIES", a status
  line (`SmallLabel`) above the row, and four `CardButton`s: key plate, icon
  56 px with a radial cooldown sweep drawn over it (custom `_Draw`: `slate`
  at 70 % alpha sweeping clockwise, remaining seconds in `NumberLabel` 16),
  name, CP badge (`resource_cp` 12 px + cost). Fourth card is the doctrine
  ability (`ability_doctrine` icon unless a specific one exists) and shows
  "USED" when exhausted. Targeting mode: the selected card is toggled amber
  and the cursor line reads "Click a target" in the status line.
- **Pause banner.** `SlatePanelStrong`, `HeadingLabel` in `amber`:
  "PAUSED — PLANNING MODE · building and upgrading stay available".
- **On map.** Build pads glow amber (ring + soft fill) while a build card is
  selected; hovering a glowing pad shows the range ring (`amber` 2 px, 40 %
  fill). Selected tower: `amber` range ring, a 1 px `amber` line to its
  current target, and the inspection card. Command Post aura: `olive` ring
  at 35 % always visible (GDD §5.8: aura value is never hidden).

### 8.5 Tower inspection card (`TowerInspectionPanel`)

`PaperPanel` 340 wide, anchored 24 px right of the tower (flipped left when
it would leave the screen; clamped vertically). Contents top-to-bottom:

1. Header: tower icon 32 px, name (`PaperHeadingLabel` 20), level pips
   (four `level_pip_on/off` 10 px), branch name if chosen (`PaperSmallLabel`).
2. Stats (two-column `PaperBodyLabel`/`PaperNumberLabel`): Damage with
   damage-type glyph and label, Rate of fire, Range (tiles), DPS.
3. Matchups: two rows "Strong vs" / "Weak vs" with armor glyphs (24 px,
   tinted `ink`).
4. Attribution: "Lifetime damage" and "Damage / Supply" (`PaperNumberLabel`).
5. Actions: **Upgrade (cost)** `PaperButton` with a diff preview line under it
   (`PaperMonoLabel`: "Damage 45 → 62 · Range 5.0 → 5.5"); at the fork two
   half-width branch cards (`PaperButton`, name + cost + one-line description
   from `TowerBranch`); **Sell (refund)** `GhostButton` styled for paper;
   **Close** (`close` icon button). Esc or clicking elsewhere closes.
   Command Posts show aura radius and per-wave income instead of combat
   stats. Suppressed towers show a `red` "SUPPRESSED" stamp and a disabled
   Sell. Targeting priority dropdown (§13.5) is deferred until the sim
   exposes it.

### 8.6 Tutorial card (`TutorialController`)

`PaperPanel` 640 wide, top-center below the wave strip. Step pips (8), step
title (`PaperHeadingLabel` 20), body (`PaperBodyLabel`), **Next**
(`PrimaryButton`) and **Skip tutorial** (`GhostButton`). Copy stays as
authored; only the presentation changes.

### 8.7 Pause menu

Centered `PaperPanel` 420 wide: `StampLabel` "PAUSED", buttons **Resume**
(`PrimaryButton`, focused by default), Restart Mission, Settings (disabled,
"later"), Abandon Mission (opens an inline confirm row: "Abandon? Yes / No"),
Quit to Menu (`PaperButton`s). Opened by the HUD menu button or Esc when
nothing else is open. Game time is paused while open; building stays
available underneath only via the HUD (the card blocks clicks under itself).

### 8.8 Post-mortem (`PostMortemPanel`)

Centered `PaperPanel` 760×620 titled with a stamp: `MISSION COMPLETE`
(`green`-tinted stamp) or `MISSION FAILED` (`StampLabel`). Sections with
`PaperSubheadingLabel` headers: **Leaked** (icon + `×count` + name per
archetype, or "Nothing leaked"), **Damage dealt by type** — a custom-drawn
four-bar chart (SA/HE/AP/AA, bar color = damage token, glyph + percent
label), **Most / least effective tower** (name + damage per Supply),
**Unspent** (Supply, CP), **Suggestion** (`PaperMonoLabel`, the rules-table
line). Buttons: **Retry Mission** (`PrimaryButton`, **focused by default**,
one click) and **Results** (`PaperButton`).

### 8.9 Results (`results.tscn`)

Full-screen `FramePanel`. Centered paper sheet: stamp (`MISSION COMPLETE` /
`MISSION FAILED`), mission title, "Wave reached NN / NN". Three large stars
(`star_filled`/`star_empty`, 56 px) with their objective captions under
each. Mastery row: rank chevron + "Rank N", XP bar (`ProgressBar`) with
"+NN XP", "RANK UP" chip in `green` when applicable. Unlock / achievement
lines as a bulleted `PaperBodyLabel` list. Buttons: **Retry Mission**
(`PrimaryButton`, focused by default) · Change Loadout (`PaperButton`, to
loadout) · Main Menu (`GhostButton`).

---

## 9. World overlays (drawn in world space, scaled by the camera)

- **Enemy health bar:** 42×5 px world, appears only when damaged (existing);
  background `slate` 90 %, fill `red`; shield segment `blue` above; armor
  glyph on the left cap drawn as a shape (existing shapes are the spec until
  icon textures are used); status badges to the right as 6 px shapes:
  suppressed = grey disc, spotted = red ring, shielded = blue arc.
- **Friendly units:** 2 px `cream` outline, `olive` accent.
- **Damage numbers:** `DamageNumberLabel`; strong hits in `he` orange,
  partial in `cream`, ineffective in `grey` with the `▼` prefix (existing
  rule), rising 30 px/s over 0.9 s.
- **Build pads:** idle = soft dark square 50 % (existing); build mode =
  `amber` ring 2 px + 25 % fill; hover = 45 % fill.
- **Range rings:** 2 px, 48 segments, `amber` for the player's selection,
  `olive` for auras.

---

## 10. Motion and feedback

- Hover: instant style swap. Press: instant. Toggle: instant.
- Panels appearing (inspection, tutorial, pause, post-mortem): fade 0→1 over
  150 ms; no scale bounce.
- Resource change: single 180 ms alpha pulse on the value label.
- Wave start: the wave heading pulses once; no screen flash.
- Nothing flashes above 3 Hz (GDD §13.9). Cooldown sweeps and the build
  timer ring are continuous, not blinking.

## 11. Accessibility hooks

- UI scale (§13.9) will use the root viewport content scale factor; nothing
  in this spec uses absolute font sizes outside the theme.
- Colorblind palettes will be alternate theme resources overriding the
  accent tokens only; every accent in §4 already pairs with a glyph or shape.
- Keyboard: every screen has a sensible focus order and a default-focused
  primary action (Retry on defeat, Resume on pause, Continue/Deploy on flow
  screens). Esc closes the topmost card. Hotkeys shown on every card.

## 12. Content policy checklist (GDD §14) for UI work

- No real insignia, crosses, eagles, flags, or emblems in nation icons —
  abstract shapes only. No named real figures in copy. Enemy names are
  generic ("Heavy Tank", "Siege Gun"). Stamps and paperwork are fictional and
  faction-neutral. Nothing depicts injury.

## 13. Verification

Every UI change is checked with real screenshots, not by reading code:

```
godot --path . --resolution 1920x1080 --position 0,0 --screen=<name> \
      --screenshot-dir=<abs dir> [--screenshot-frames=40,900] [--skip-tutorial]
```

`--screen` accepts `main_menu`, `briefing`, `loadout`, `mission`, `results`.
Then `tools/Run-HeadlessChecks.ps1` must stay green (build, all suites,
validator, smoke run).

Review checklist per screen: nothing clipped at 1920×1080; nothing
overlapping; every value ≥ 13 px; every interactive control shows hover,
pressed, disabled and focus; every color-coded element has a glyph; copy
passes §12; no absolute positions that break when the window widens.

## 14. Ownership map

| Area | Files |
|---|---|
| Theme, palette, icons registry, materials | `assets/ui/theme/fow_theme.tres`, `src/UI/Theme/*.cs`, `assets/ui/materials/*`, `assets/ui/icons/*.svg` |
| Flow screens | `src/UI/Flow/{MainMenu,Briefing,Loadout,Results}Controller.cs`, `scenes_root/{main_menu,briefing,loadout,results}.tscn`, `src/Core/Boot.cs` |
| Mission HUD chrome | `src/UI/Hud/*.cs`, `scenes_root/mission.tscn` (Camera, backdrop, HUD nodes) |
| Mission cards and overlays | `src/UI/Panels/*.cs`, `src/UI/Flow/TutorialController.cs`, `src/UI/DamageNumberSpawner.cs`, `src/Map/BuildPad.cs`, `scenes/map/build_pad.tscn`, `src/Enemies/EnemyController.cs` (`_Draw` only) |
