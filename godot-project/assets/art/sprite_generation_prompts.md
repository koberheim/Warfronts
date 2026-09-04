# Fronts of War — sprite generation prompt document

## STATUS: HOLD — reference/planning document, not a production queue

This document was assembled by scanning the repo's current design and art
documentation (`docs/GDD.md` §§6, 8, 9, 10, 14, 16; `docs/FRONTS OF WAR ART
DESIGN.md` §§1–10; `godot-project/assets/art/ART_ASSET_INVENTORY.md`,
`ART_GENERATION_PROMPTS.md`, `ART_ASSET_PATHWAYS.md`, and
`assets/data/art/art_asset_catalog.json`). It exists so the prompts are ready
to paste **when the project is ready for them**, not before.

Every category covered here — nation-specific tower art, tower upgrade
states, national insignia, national firing/tracer art, signature-tower art,
nation-specific friendly units, and all enemy-archetype art — is currently
marked **`HOLD`** in `art_asset_catalog.json`, with the note *"Structured
only. Do not generate or integrate until the implementation review clears
nation and enemy identity art"* (towers) and *"Held per the user's review
request"* / *"Held until implementation and silhouette review"* (units,
enemies). **That gate is still active.** Do not generate or integrate any
art from this file until that review is complete and this banner is updated
or removed. Environment/terrain art (the four theater kits, shared build
pads, UI frame) is a separate, already-active pipeline in
`ART_GENERATION_PROMPTS.md` — this file does not touch or duplicate that
work.

---

## 1. Global art style rules

Paste the block below once per image-generation conversation (the same
practice `ART_GENERATION_PROMPTS.md` already uses for the environment kits),
then send one row's prompt at a time. Keep one nation, or one archetype
family, per conversation, and attach the first accepted output as a
reference for the rest of that batch.

### 1.1 Unit & Tower Style Lock (paste-ready prefix)

> Create production sprite art for **Fronts of War**, a painterly storybook
> 2D tower-defense game with an **80% stylized / 20% grounded** visual
> direction. Use a true top-down mechanical footprint with a subtle 65–75°
> overhead visual cheat (objects reveal a little side/front surface to read
> volume — a tank shows hull side, a gun shows its carriage and wheels — but
> the object's ground footprint stays strictly top-down and does not lean or
> foreshorten). Broad, confident, slightly irregular shapes; exaggerate
> function (bigger guns read as more dangerous, heavier armor reads as
> visibly larger); compressed detail; rich but controlled color; soft
> painted edge separation; no photorealism, no toy-like comedy, no cartoon
> faces or exaggerated anatomy. Single canonical orientation, facing straight
> up the frame (12 o'clock / north) — the game engine rotates the sprite
> freely at runtime, so do not draw the subject at an angle and do not
> generate separate directional (N/S/E/W) versions. Transparent PNG
> background, one centered object, complete silhouette, no cropping, no
> ground plate or environment plate, no black halo or vignette, no text,
> letters, numbers, or watermark. A soft local contact shadow is allowed only
> if it stays contained inside the object's own footprint. Do not paint an
> outline, glow, or team-color ring around the subject — friendly/enemy
> distinction (white outline vs. dark outline) is applied by the game engine,
> not the art. No blood, wounds, corpses, dying poses, civilians, war-crime
> or atrocity imagery, real political figures, real fascist or Nazi
> iconography (no swastikas, no SS runes, no Party symbols), or national
> caricature. All insignia must be the fictionalized marks specified per
> nation below — never a real-world national emblem.

### 1.2 Perspective and rotation — why this deviates from a typical 4-direction sprite sheet

The GDD's camera is **mechanically top-down** with units that **free-rotate**
in the engine (`docs/GDD.md` §16.1–16.2), and the art spec's "overhead cheat"
is explicitly *"a visual illusion, not an actual perspective camera"*
(`FRONTS OF WAR ART DESIGN.md` §3.2–3.3). That means a unit or tower needs
**one canonical top-down orientation**, not the four/eight directional frames
a typical top-down RPG character needs — the engine handles facing by
rotating that single sprite. Generating N/S/E/W variants would produce art
that doesn't match how this game actually renders units, so every prompt
below asks for one orientation only.

### 1.3 Size hierarchy (reference px at native zoom, GDD §16.2)

| Class | Reference size |
|---|---|
| Infantry | 32 px |
| Light Vehicle | 44 px |
| Medium Armor | 56 px |
| Heavy Armor | 76 px |
| Air | 88 px (plus drop shadow) |

Generate each cutout at **2× its reference size** on the transparent canvas
(matching the existing build-pad cutout convention of a 256 px canvas for a
128 px gameplay footprint) so there's clean headroom for downsampling.

### 1.4 Motion — frames needed per unit (GDD §16.2)

- **Infantry:** a 4-frame walk/gait cycle, single orientation. This is a leg
  animation loop, not four facing directions.
- **Vehicles:** one static hull/body image plus a 2-frame wheel/track-roll
  cycle. Body rotation is handled by the engine.
- **Air:** one static image; a propeller blur is enough, no additional
  frames.
- **Towers:** static. Two art states total per tower — a shared **base**
  sprite for Levels 1–2, and a shared **branch** sprite for Levels 3–4 (GDD
  §16.3). Do not generate four separate level sprites.
- **Death/destruction:** infantry disperse into a puff plus a dropped token;
  vehicles become a static wreck sprite that fades over 3s. No gore, no dying
  poses (GDD §16.2, §14.3).

Generate each frame as its own image (one prompt, one output), the same way
`ART_GENERATION_PROMPTS.md` already works — packed grid sprite sheets are
harder for image generators to keep aligned than individual frames assembled
afterward into a Godot `SpriteFrames` resource.

### 1.5 Armor-class shape language (GDD §16.2)

| Armor class | Shape cue |
|---|---|
| Soft | Small, loose silhouette |
| Hardened | Visible plating shapes |
| Armored | Tracked and turreted |
| Heavy | Visibly oversized, longer gun |

Shape must carry armor class before any icon does — every generated unit
should pass a "solid black silhouette" identifiability test at native size.

### 1.6 Fictionalized insignia (GDD §14.3 — exact wording, do not substitute a real emblem)

| Nation | Insignia mark |
|---|---|
| Germany | A plain dark cross with a distinct geometric border invented for this game |
| Japan | An abstract solar disc variant, with a different ray count and framing than any historical marking |
| Soviet Union | An abstract red star with a distinct inner geometry |
| Italy, Britain, United States | Similarly abstracted roundels and stars, each instantly readable as "that nation's side" and not a real emblem |

Nationality is deliberately the **weakest** visual signal in the game — one
small insignia pip per unit/tower, never a repeated pattern, flag, or banner
(GDD §16.2).

### 1.7 Tower layering: base emplacement + rotating turret

Archetypes T1–T7 fire at targets, and GDD §16.3 already assumes the base
emplacement (sandbags, concrete, dugout) is reused while only the weapon
changes between a tower's base state and its branch state — which is
exactly the split a rotating turret needs. Generate these seven archetypes
as **two separate layers** instead of one fused image:

- **Emplacement layer** — the static base (sandbags, mount, carriage,
  dugout, crates). Never rotates in-engine. **One image, shared across all
  four tower levels** for that nation+archetype — do not regenerate it for
  the branch upgrade.
- **Turret/weapon layer** — the gun itself, cropped at its mount point, no
  surrounding base. **Rotates in-engine** to track the tower's target. Needs
  **two states**: a base-state image (L1–L2) and a branch-state image
  (L3–L4, reflecting the branch's weapon change — e.g. Germany's T6 Heavy
  Turret branch becomes the longer-barreled Tiger Tank Platform gun).

Generate both layers on the **same canvas size**, with the weapon's pivot
point at the exact canvas center, so overlaying them at (0,0) in Godot
composites correctly without extra offset math — the turret `Sprite2D`
rotates to aim at the target; the emplacement layer's rotation always stays
at zero.

**Exceptions — keep these as single fused pieces, not split:**
- **T8 Minefield/Route Denial** — a triggered trap, not an aimed weapon.
- **T9 Command Post** — GDD: *"This tower never shoots."* No turret to
  rotate. A few nations' T9 already describes a rotating radar dish
  (Britain's Radar Early Warning Tower, Germany's Radar Flak Tower); those
  *may* optionally get the same two-layer split for a passive continuous
  spin, but that's a cosmetic nicety, not a requirement.
- **T6's coaxial machine gun** is modeled as part of the single turret piece
  alongside the main gun, not a third layer — GDD gives it independent
  targeting mechanically, but splitting it visually isn't worth the added
  production cost for a secondary weapon.

File naming for the split: the emplacement layer drops the `{state}` field
entirely (it has none); the turret layer keeps it (`base` or `branch`). See
the updated file path contract in §4.

---

## 2. Nation sections

Each section below gives: the nation's visual-identity tendencies (from
`FRONTS OF WAR ART DESIGN.md` §9, quoted directly — these are **art-direction
tendencies, not team colors**), its 9 tower archetypes with GDD's own
national name and stat-leaning per archetype, its signature tower, and any
enemy-archetype variants the GDD has already named for that nation. Scope
tags (`[VS]` vertical slice, `[L]` launch, `[P1]` post-launch-deferred) are
carried over from the GDD so this doc doesn't imply a generation priority the
design doesn't have — **do not prompt `[P1]` rows until they move into
scope.**

Every prompt row below assumes the **Unit & Tower Style Lock** (§1.1) is
already in context. Tower filenames use the reserved catalog pattern
`res://assets/art/towers/national/{nation}/{archetype}/{state}_v{version}.png`;
enemy filenames use
`res://assets/art/enemies/archetypes/{nation}/{archetype}/{state}_v{version}.png`;
friendly-unit filenames use
`res://assets/art/units/national_skins/{nation}/{unit_family}/{variant}_v{version}.png`.

---

### 2.1 United States

**Visual identity** (art spec §9): olive drab, warm canvas, khaki, exposed
steel, practical modular construction, timber/crate supply language.
Character: **industrial, practical, standardized.**

**Insignia:** abstracted roundel/star (§1.6).

#### Towers

Per §1.7, T1–T7 are split into an emplacement layer (static, shared across
L1–L4) and a turret/weapon layer (rotates in-engine; base state shown here,
L1–L2). T8/T9 have no turret and stay single fused pieces.

| # | Archetype | US name | Leaning (GDD §8.2.1) | Scope | Emplacement layer (shared L1–L4) | Turret layer — base state (L1–L2) |
|---|---|---|---|---|---|---|
| T1 | Automatic Gun | Browning MG Nest | Baseline | [VS] | EMPLACEMENT, US Browning MG Nest base only — a circular sandbag-and-timber-stake ring around an empty gun mount, with an ammo crate, folded canvas, and a helmet prop, practical modular US construction, insignia pip only, no weapon. | TURRET, US Browning MG Nest weapon only — a water-cooled machine gun with exposed steel receiver and a feeding ammo belt, cropped at its tripod mount, single top-down orientation, centered on its rotation pivot. |
| T2 | Marksman Post | Ranger Sniper Post | +8% RoF, −5% range | [L] | EMPLACEMENT, US Ranger Sniper Post base only — an elevated timber-and-sandbag hide platform with olive-drab canvas screening, empty firing slot, no weapon. | TURRET, US Ranger Sniper Post weapon only — a long-barreled rifle on a bipod, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T3 | Field Mortar | M3 Halftrack Turret | +10% RoF, −8% blast | [VS] | EMPLACEMENT, US M3 Halftrack Turret base only — a dug-in halftrack carriage with exposed steel plating and a folded khaki canvas tarp, empty mortar mount, no weapon. | TURRET, US M3 Halftrack Turret weapon only — an 81mm mortar tube, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T4 | Anti-Tank Gun | Bazooka Squad | −12% cost, +12% RoF, −15% range | [VS] | EMPLACEMENT, US Bazooka Squad base only — a low sandbag rest beside a stacked-rocket supply crate, empty firing position, no weapon. | TURRET, US Bazooka Squad weapon only — a shoulder-fired anti-tank launcher, single top-down orientation, centered on its rotation pivot. |
| T5 | Flak Battery | M45 Quadmount AA | +12% RoF, −8% dmg | [L] | EMPLACEMENT, US M45 Quadmount AA base only — a ring-shaped exposed-steel mount platform, empty center, no weapon. | TURRET, US M45 Quadmount AA weapon only — a four-barreled anti-aircraft gun cluster, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T6 | Armored Emplacement | Sherman Tank Emplacement | Baseline | [L] | EMPLACEMENT, US Sherman Tank Emplacement base only — a dug-in hull front and tracks, olive-drab paint, empty turret ring, no gun. | TURRET, US Sherman Tank Emplacement weapon only — the Sherman-inspired turret with main gun and coaxial machine gun modeled together, white star insignia pip only, single top-down orientation, centered on its rotation pivot. |
| T7 | Heavy Artillery | 105mm Howitzer Battery | Precision leaning | [L] | EMPLACEMENT, US 105mm Howitzer Battery base only — a wide split-trail carriage with stacked shell crates, empty gun cradle, no weapon. | TURRET, US 105mm Howitzer Battery weapon only — the howitzer barrel and breech, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T8 | Minefield/Route Denial | Combat Engineer Minefield | +1 charge, −10% dmg | [L] | ROUTE PROP, US Combat Engineer minefield marker cluster, low warning stakes and a taped perimeter over disturbed earth, base state. | n/a — triggered trap, no turret. |
| T9 | Command Post | Jeep Recon Tower | +1.0 aura radius | [VS] | STRUCTURE, US Jeep Recon Tower, a jeep with a raised radio antenna and map table beside a small canvas awning, no weapon, base state. | n/a — GDD: "this tower never shoots." |

**Branch states (L3–L4):** for T1–T7, regenerate only the **turret layer**
(the emplacement stays unchanged) using the GDD branch name/effect — e.g.
T6 Heavy Turret branch = "Jumbo Assault Tank," a heavier-armored turret
replacing the base Sherman turret. Save as
`.../{archetype}/turret_branch_v01.png`. T9 has no turret layer, so its
branches regenerate the full fused structure instead: Forward Observer
branch = "Naval Gunfire Spotter" (adds a radio mast and signal panel to the
Jeep Recon Tower); Logistics branch = "War Bonds Supply Depot" (adds
stacked supply crates and a collection-bin prop). Save T9's branch state as
`.../{archetype}/branch_v01.png`.

**Signature — Arsenal of Democracy Factory** `[VS]`

> STRUCTURE, US Arsenal of Democracy Factory, a compact prefabricated
> production shed with a rolling assembly-line door, exposed steel framing,
> stacked crates and a small rail spur, industrial/standardized US
> construction, single top-down orientation, three upgrade-tier readability
> (L1 modest shed → L3 larger multi-bay factory with a visible rail siding),
> insignia pip only, transparent background.
> File: `res://assets/art/towers/national/us/signature_arsenal_of_democracy/state_v01.png`

**Friendly units produced by the Factory** (`units/national_skins/us/`):
Rifle Squad (120 HP infantry, 4-frame walk cycle), Jeep (fast light vehicle,
2-frame wheel roll), Light Tank (medium vehicle, 2-frame track roll). Per
GDD §8.2.1: friendly units render with a **2px white outline, 85% scale,
lower z-index, white HP bars** — bake none of that into the art itself; it's
engine-applied, same rule as §1.1's friend/foe note.

#### Enemy archetype variants named for the US (GDD §10.2)

| Archetype | US variant name | Prompt |
|---|---|---|
| E1 Basic Infantry | US Rifle Squad | UNIT, US Rifle Squad, three-figure upright loose-file infantry group, olive-drab uniforms, distinct US helmet shape, single top-down orientation, 4-frame walk cycle. |
| E2 Fast Infantry/Scout | US Ranger Unit | UNIT, US Ranger Unit, two-figure forward-lean scout pair with light gear, dust trail, single top-down orientation, 4-frame walk cycle. |
| E5 Light Vehicle | Allied Supply Convoy | UNIT, Allied Supply Convoy truck, low wheeled cargo body with canvas tarp, single top-down orientation, static hull + 2-frame wheel roll. |
| E6 Medium Armor | Sherman Tank Column | UNIT, Sherman Tank Column vehicle, classic hull/turret/tracks silhouette, olive drab, single top-down orientation, static hull + 2-frame track roll. |
| E8 Air Unit | US Bomber Wing | UNIT, US Bomber Wing aircraft, wings-horizontal top-down silhouette with a moving ground shadow, static image, propeller blur only. |

No GDD-named US variant yet exists for E3, E4, E7, E9–E12 — do not invent
names for these; add rows here once the GDD or `NationProfile`/`EnemyThemeWeighting`
data names them.

---

### 2.2 Britain

**Visual identity:** muted olive, khaki, brown, canvas, slightly more
improvised field construction. Character: **practical, compact,
field-adapted.**

**Insignia:** abstracted roundel/star (§1.6).

#### Towers

Per §1.7, T1–T7 split into emplacement (static, shared L1–L4) and turret
(rotates in-engine; base state shown, L1–L2) layers. T8/T9 stay fused.

| # | Archetype | British name | Leaning (GDD §8.2.2) | Scope | Emplacement layer (shared L1–L4) | Turret layer — base state (L1–L2) |
|---|---|---|---|---|---|---|
| T1 | Automatic Gun | Vickers Machine Gun Nest | +8% range, −6% RoF | [L] | EMPLACEMENT, British Vickers Machine Gun Nest base only — a low sandbag crescent around an empty gun mount, improvised field construction, no weapon. | TURRET, British Vickers weapon only — a water-cooled machine gun, cropped at its low tripod mount, single top-down orientation, centered on its rotation pivot. |
| T2 | Marksman Post | SAS Ambush Post | +12% range, best-in-class Overwatch | [L] | EMPLACEMENT, British SAS Ambush Post base only — a camouflage-netted hide platform, compact and low-profile, empty firing slot, no weapon. | TURRET, British SAS Ambush Post weapon only — a long rifle barrel, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T3 | Field Mortar | Mortar Pit | Smoke branch +40% duration | [L] | EMPLACEMENT, British Mortar Pit base only — a sunken circular earthwork with stacked bomb crates, empty mortar mount, no weapon. | TURRET, British Mortar Pit weapon only — a mortar tube, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T4 | Anti-Tank Gun | 17-Pounder Gun Position | +12% range, −8% RoF | [L] | EMPLACEMENT, British 17-Pounder Gun Position base only — a sandbag revetment, empty gun cradle, no weapon. | TURRET, British 17-Pounder weapon only — a long-barreled anti-tank gun on its split-trail carriage, cropped at the mount, single top-down orientation, centered on its rotation pivot. |
| T5 | Flak Battery | Bofors AA Platform | +15% RoF, −10% dmg | [L] | EMPLACEMENT, British Bofors AA Platform base only — a four-legged mount base, empty center, no weapon. | TURRET, British Bofors weapon only — a single-barrel automatic AA gun, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T6 | Armored Emplacement | Churchill Tank Bunker | +10% range, −8% dmg | [L] | EMPLACEMENT, British Churchill Tank Bunker base only — a dug-in hull front and tracks, muted olive paint, empty turret ring, no gun. | TURRET, British Churchill weapon only — the heavily-sided Churchill-inspired turret, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T7 | Heavy Artillery | Royal Artillery Battery | Precision leaning, +8% range | [L] | EMPLACEMENT, British Royal Artillery Battery base only — a wide carriage with stacked ammunition crates, empty gun cradle, no weapon. | TURRET, British Royal Artillery weapon only — the field gun barrel and breech, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T8 | Minefield/Route Denial | Royal Engineers Minefield | +12% dmg, faster arming | [L] | ROUTE PROP, British Royal Engineers minefield marker cluster, low warning stakes with a hazard tape perimeter, base state. | n/a — triggered trap, no turret. |
| T9 | Command Post | Radar Early Warning Tower | +25% aura radius, double reveal | [L] | STRUCTURE, British Radar Early Warning Tower, a field radio mast with a rotating mesh antenna dish on a small timber platform, no weapon, base state. | n/a — GDD: "this tower never shoots" (optional passive-spin dish split per §1.7). |

**Branch states:** for T1–T7, regenerate only the **turret layer** — e.g.
T6 Heavy Turret branch = "Churchill Crocodile Support" (a turret with an
added flame-projector nozzle, towed fuel trailer silhouette attached).
Save as `.../{archetype}/turret_branch_v01.png`.

**Signature — RAF Scramble Command** `[L]`

> STRUCTURE, British RAF Scramble Command, a small forward airfield radio
> hut with a raised aerial mast and a wind-sock pole, ground-crew fuel drums
> stacked nearby, practical field-adapted British construction, single
> top-down orientation, upgrade tiers show a larger hut and a second aerial,
> insignia pip only, transparent background.
> File: `res://assets/art/towers/national/britain/signature_raf_scramble_command/state_v01.png`

Sortie strike VFX (fighters strafing a corridor) belongs in the shared
projectiles/effects set (§3), tinted to Britain's ramp.

#### Enemy archetype variants named for Britain (GDD §10.2)

| Archetype | British variant name | Prompt |
|---|---|---|
| E1 Basic Infantry | British Infantry Section | UNIT, British Infantry Section, three-figure upright loose-file infantry group, muted-olive uniforms, distinct British helmet shape, single top-down orientation, 4-frame walk cycle. |
| E2 Fast Infantry/Scout | British Commando Team | UNIT, British Commando Team, two-figure forward-lean scout pair, dust trail, single top-down orientation, 4-frame walk cycle. |
| E7 Heavy Armor | Churchill Assault Tank | UNIT, Churchill Assault Tank, 1.4× scale heavy tank silhouette with wide tracks and a long gun, single top-down orientation, static hull + 2-frame track roll. |
| E8 Air Unit | RAF Bomber Formation | UNIT, RAF Bomber Formation aircraft, wings-horizontal top-down silhouette with a moving ground shadow, static image, propeller blur only. |

No GDD-named British variant yet exists for E3–E6, E9–E12.

---

### 2.3 Soviet Union

**Visual identity:** earthy green, brown, dark steel, raw timber, rugged
simplified construction. Character: **robust, blunt, functional.**

**Insignia:** abstract red star with a distinct inner geometry (§1.6).

#### Towers

Per §1.7, T1–T7 split into emplacement (static, shared L1–L4) and turret
(rotates in-engine; base state shown, L1–L2) layers. T8/T9 stay fused.

| # | Archetype | Soviet name | Leaning (GDD §8.2.3) | Scope | Emplacement layer (shared L1–L4) | Turret layer — base state (L1–L2) |
|---|---|---|---|---|---|---|
| T1 | Automatic Gun | Maxim MG Bunker | −15% cost, −8% dmg | [L] | EMPLACEMENT, Soviet Maxim MG Bunker base only — a rough log-and-earth bunker face, empty gun port, rugged raw-timber construction, no weapon. | TURRET, Soviet Maxim weapon only — a wheeled water-cooled machine gun, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T2 | Marksman Post | Siberian Sniper Nest | +12% dmg, −8% RoF | [L] | EMPLACEMENT, Soviet Siberian Sniper Nest base only — a snow-dusted log hide, dark steel accents, empty firing slot, no weapon. | TURRET, Soviet Siberian Sniper weapon only — a long rifle barrel, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T3 | Field Mortar | Red Army Mortar Squad | −12% cost, +20% blast, +scatter | [L] | EMPLACEMENT, Soviet Red Army Mortar Squad base only — a rough earth pad with stacked bomb crates, blunt functional construction, empty mortar mount, no weapon. | TURRET, Soviet Red Army Mortar weapon only — a mortar tube, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T4 | Anti-Tank Gun | Anti-Tank Rifle Team | −18% cost, −12% dmg, +10% RoF | [L] | EMPLACEMENT, Soviet Anti-Tank Rifle Team base only — a rough earthwork, empty firing position, no weapon. | TURRET, Soviet Anti-Tank Rifle weapon only — a long anti-tank rifle on a low bipod, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T5 | Flak Battery | DShK AA Mount | −12% cost, −8% dmg | [L] | EMPLACEMENT, Soviet DShK AA Mount base only — an anti-aircraft tripod base, dark steel, empty center, no weapon. | TURRET, Soviet DShK weapon only — a heavy machine gun, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T6 | Armored Emplacement | T-34 Defensive Turret | −12% cost | [L] | EMPLACEMENT, Soviet T-34 Defensive Turret base only — a dug-in hull front and tracks, earthy green paint, empty turret ring, no gun. | TURRET, Soviet T-34 weapon only — the sloped T-34-inspired turret, red star insignia pip only, single top-down orientation, centered on its rotation pivot. |
| T7 | Heavy Artillery | Field Artillery Battery | +20% blast, +12% scatter | [L] | EMPLACEMENT, Soviet Field Artillery Battery base only — a rugged carriage with stacked shells, empty gun cradle, no weapon. | TURRET, Soviet Field Artillery weapon only — the field gun barrel and breech, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T8 | Minefield/Route Denial | Minefield Layer | −15% cost, +1 charge | [L] | ROUTE PROP, Soviet minefield marker cluster, low rough-timber warning stakes over disturbed earth, base state. | n/a — triggered trap, no turret. |
| T9 | Command Post | Commissar Command Post | +10% RoF aura, −10% range aura | [L] | STRUCTURE, Soviet Commissar Command Post, a rough timber field-command table with a map board and radio set under a canvas lean-to, no weapon, base state. | n/a — GDD: "this tower never shoots." |

**Branch states:** for T1–T7, regenerate only the **turret layer** — e.g.
T7 Rocket Saturation branch = **Katyusha Rocket Truck**, a truck-mounted
multiple rocket-launcher rack turret replacing the base towed-gun turret
(the emplacement carriage stays the same). Save as
`.../{archetype}/turret_branch_v01.png`. T9 Logistics branch = "Rear Echelon
Depot" regenerates the full fused structure; save as
`.../{archetype}/branch_v01.png`.

**Signature — Katyusha Storm Battery** `[L]`

> STRUCTURE, Soviet Katyusha Storm Battery, a truck-mounted angled rack of
> rocket launch rails, dark steel and earthy-green paint, rugged functional
> Soviet construction, single top-down orientation, upgrade tiers show
> additional rail rows, red-star insignia pip only, transparent background.
> File: `res://assets/art/towers/national/soviet_union/signature_katyusha_storm_battery/state_v01.png`

The barrage's rising-rail wind-up and wide impact-marker sequence belong in
the shared projectiles/effects set (§3), tinted to the Soviet ramp.

#### Enemy archetype variants named for the Soviet Union (GDD §10.2)

| Archetype | Soviet variant name | Prompt |
|---|---|---|
| E1 Basic Infantry | Soviet Conscripts | UNIT, Soviet Conscripts, three-figure upright loose-file infantry group, earthy-green uniforms, distinct Soviet helmet shape, single top-down orientation, 4-frame walk cycle. |
| E2 Fast Infantry/Scout | Soviet Shock Troops (visual only) | UNIT, Soviet Shock Troops, two-figure forward-lean scout pair, dust trail, single top-down orientation, 4-frame walk cycle. |
| E5 Light Vehicle | Soviet Scout Car | UNIT, Soviet Scout Car, small low wheeled body, dark steel, single top-down orientation, static hull + 2-frame wheel roll. |
| E6 Medium Armor | T-34 Assault Tank | UNIT, T-34 Assault Tank, classic hull/turret/tracks silhouette, sloped armor read, single top-down orientation, static hull + 2-frame track roll. |
| E7 Heavy Armor | IS Heavy Tank | UNIT, IS Heavy Tank, 1.4× scale heavy tank silhouette with wide tracks and a long gun, single top-down orientation, static hull + 2-frame track roll. |

No GDD-named Soviet variant yet exists for E3, E4, E8–E12.

---

### 2.4 Germany

**Visual identity:** field green, darker grey-green, muted tan, angular
steel construction, precise equipment arrangements. Character: **angular,
engineered, compact.**

**Insignia:** a plain dark cross with a distinct geometric border invented
for this game — **never** a real-world cross marking (§1.6).

#### Towers

Per §1.7, T1–T7 split into emplacement (static, shared L1–L4) and turret
(rotates in-engine; base state shown, L1–L2) layers. T8/T9 stay fused.

| # | Archetype | German name | Leaning (GDD §8.2.4) | Scope | Emplacement layer (shared L1–L4) | Turret layer — base state (L1–L2) |
|---|---|---|---|---|---|---|
| T1 | Automatic Gun | MG42 Bunker | +18% RoF, +12% cost | [L] | EMPLACEMENT, German MG42 Bunker base only — a precise angular concrete-and-steel bunker face, empty gun port, engineered compact construction, no weapon. | TURRET, German MG42 weapon only — a machine gun, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T2 | Marksman Post | Jäger Marksman Post | +10% dmg, +10% cost | [L] | EMPLACEMENT, German Jäger Marksman Post base only — a camouflage-screened hide on angular steel supports, empty firing slot, no weapon. | TURRET, German Jäger weapon only — a long rifle barrel, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T3 | Field Mortar | Wehrmacht Mortar Team | +12% RoF, +10% cost | [L] | EMPLACEMENT, German Wehrmacht Mortar Team base only — a precise angular emplacement pad with stacked bomb crates, empty mortar mount, no weapon. | TURRET, German Wehrmacht Mortar weapon only — a mortar tube, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T4 | Anti-Tank Gun | Pak 40 Anti-Tank Gun | +18% dmg, +12% cost, +6% range | [L] | EMPLACEMENT, German Pak 40 base only — a precise angular earthwork, empty gun cradle, no weapon. | TURRET, German Pak 40 weapon only — the long-barreled anti-tank gun on its split-trail carriage, cropped at the mount, single top-down orientation, centered on its rotation pivot. |
| T5 | Flak Battery | Flak 88 Cannon | +15% dmg, +10% cost, best-in-class Dual Purpose | [L] | EMPLACEMENT, German Flak 88 base only — a cross-shaped angular mount base, empty center, no weapon. | TURRET, German Flak 88 weapon only — the tall high-velocity gun, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T6 | Armored Emplacement | Panzer IV Turret | +12% dmg, +15% cost | [L] | EMPLACEMENT, German Panzer IV base only — a dug-in hull front and tracks, field-green paint, empty turret ring, no gun. | TURRET, German Panzer IV weapon only — the boxy Panzer IV-inspired turret, dark-cross insignia pip only, single top-down orientation, centered on its rotation pivot. |
| T7 | Heavy Artillery | Nebelwerfer Rocket Battery | +10% dmg, −6% blast | [L] | EMPLACEMENT, German Nebelwerfer base only — a towed carriage base, empty rack mount, no weapon. | TURRET, German Nebelwerfer weapon only — the multi-tube angular rocket launcher rack, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T8 | Minefield/Route Denial | Teller Mine Field | +15% dmg, −1 charge | [L] | ROUTE PROP, German Teller Mine field marker cluster, precise angular warning stakes over disturbed earth, base state. | n/a — triggered trap, no turret. |
| T9 | Command Post | Radar Flak Tower | +12% cost, +10% aura strength, best-in-class Air reveal | [L] | STRUCTURE, German Radar Flak Tower, an angular steel lattice mast with a rotating mesh antenna dish, no weapon, base state. | n/a — GDD: "this tower never shoots" (optional passive-spin dish split per §1.7). |

**Branch states:** for T1–T7, regenerate only the **turret layer** — e.g.
T6 Heavy Turret branch = **Tiger Tank Platform**, a wider, longer-gunned
turret replacing the base Panzer IV turret (the emplacement stays the
same). Save as `.../{archetype}/turret_branch_v01.png`.

**Signature — Blitzkrieg Command Post** `[L]`

> STRUCTURE, German Blitzkrieg Command Post, a compact angular field-command
> bunker with a raised radio mast and map table visible through an open
> hatch, precise engineered German construction, single top-down
> orientation, upgrade tiers show a larger bunker footprint, dark-cross
> insignia pip only, transparent background.
> File: `res://assets/art/towers/national/germany/signature_blitzkrieg_command_post/state_v01.png`

The activation pulse (amber ring + chevrons on buffed towers) belongs in the
shared effects set (§3) as a status overlay, not baked into this sprite.

#### Enemy archetype variants named for Germany (GDD §10.2)

| Archetype | German variant name | Prompt |
|---|---|---|
| E1 Basic Infantry | Wehrmacht Infantry | UNIT, Wehrmacht Infantry, three-figure upright loose-file infantry group, field-green uniforms, distinct German helmet shape, single top-down orientation, 4-frame walk cycle. |
| E2 Fast Infantry/Scout | German Stormtroopers | UNIT, German Stormtroopers, two-figure forward-lean scout pair, dust trail, single top-down orientation, 4-frame walk cycle. |
| E5 Light Vehicle | German Kübel Patrol | UNIT, German Kübel Patrol vehicle, small low open-topped wheeled body, single top-down orientation, static hull + 2-frame wheel roll. |
| E6 Medium Armor | Panzer IV Tank | UNIT, Panzer IV Tank, classic hull/turret/tracks silhouette, field-green paint, single top-down orientation, static hull + 2-frame track roll. |
| E7 Heavy Armor | Tiger Heavy Tank | UNIT, Tiger Heavy Tank, 1.4× scale heavy tank silhouette with wide tracks and a long gun, single top-down orientation, static hull + 2-frame track roll. |
| E8 Air Unit | Luftwaffe Bomber Squadron | UNIT, Luftwaffe Bomber Squadron aircraft, wings-horizontal top-down silhouette with a moving ground shadow, static image, propeller blur only. |

No GDD-named German variant yet exists for E3, E4, E9–E12.

---

### 2.5 Italy

**Visual identity:** warm olive, sand, faded green, canvas, lighter
structural forms. Character: **lightweight, mobile, Mediterranean.**

**Insignia:** abstracted roundel/star (§1.6).

#### Towers

Per §1.7, T1–T7 split into emplacement (static, shared L1–L4) and turret
(rotates in-engine; base state shown, L1–L2) layers. T8/T9 stay fused.

| # | Archetype | Italian name | Leaning (GDD §8.2.5) | Scope | Emplacement layer (shared L1–L4) | Turret layer — base state (L1–L2) |
|---|---|---|---|---|---|---|
| T1 | Automatic Gun | Breda Machine Gun Nest | −10% cost, +10% RoF, −10% dmg | [L] | EMPLACEMENT, Italian Breda Machine Gun Nest base only — a low sand-colored sandbag mound, empty gun mount, mobile Mediterranean construction, no weapon. | TURRET, Italian Breda weapon only — a lightweight machine gun, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T2 | Marksman Post | Alpini Mountain Rifle Post | −10% cost, +20% range on Elevated | [L] | EMPLACEMENT, Italian Alpini Mountain Rifle Post base only — a rock-and-canvas hide, warm-olive canvas screening, empty firing slot, no weapon. | TURRET, Italian Alpini weapon only — a long rifle barrel, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T3 | Field Mortar | Italian Mortar Pit | −10% cost, Smoke branch +25% duration | [L] | EMPLACEMENT, Italian Mortar Pit base only — a light sand-colored emplacement pad with stacked bomb crates, empty mortar mount, no weapon. | TURRET, Italian Mortar Pit weapon only — a mortar tube, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T4 | Anti-Tank Gun | Anti-Tank Gun Battery | −10% cost, −8% dmg, +12% RoF | [L] | EMPLACEMENT, Italian Anti-Tank Gun Battery base only — a sand-colored earthwork, empty gun cradle, no weapon. | TURRET, Italian Anti-Tank weapon only — the lighter anti-tank gun on its split-trail carriage, cropped at the mount, single top-down orientation, centered on its rotation pivot. |
| T5 | Flak Battery | Breda 20mm AA Mount | −10% cost, +12% RoF | [L] | EMPLACEMENT, Italian Breda 20mm AA Mount base only — a compact tripod mount base, empty center, no weapon. | TURRET, Italian Breda 20mm weapon only — a light automatic AA gun, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T6 | Armored Emplacement | Carro Armato Turret | −12% cost, −10% dmg | [L] | EMPLACEMENT, Italian Carro Armato base only — a dug-in hull front and tracks, warm-olive/sand paint, empty turret ring, no gun. | TURRET, Italian Carro Armato weapon only — the lighter Carro Armato-inspired turret, single top-down orientation, centered on its rotation pivot. |
| T7 | Heavy Artillery | Coastal Artillery Gun | +22% range, +10% cost, −8% dmg | [L] | EMPLACEMENT, Italian Coastal Artillery Gun base only — a wide carriage with stacked shells, lighter structural forms, empty gun cradle, no weapon. | TURRET, Italian Coastal Artillery weapon only — the field gun barrel and breech, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T8 | Minefield/Route Denial | Guastatori Minefield | −10% cost, Concussion branch +25% duration | [L] | ROUTE PROP, Italian Guastatori minefield marker cluster, low sand-colored warning stakes over disturbed earth, base state. | n/a — triggered trap, no turret. |
| T9 | Command Post | Recon Motorcycle Outpost | −10% cost, +15% aura radius, −8% aura strength | [L] | STRUCTURE, Italian Recon Motorcycle Outpost, a sidecar motorcycle parked beside a small map table under a canvas awning, no weapon, base state. | n/a — GDD: "this tower never shoots." |

**Branch states:** for T1–T7, regenerate only the **turret layer** — e.g.
T4 Sabot branch = "Semovente Tank Destroyer" (an open-topped self-propelled
gun turret replacing the base Anti-Tank Gun turret). T7's Coastal-pad
variant "Naval Patrol Cannon" is a cost/name variant of the same turret, not
a new visual. Save as `.../{archetype}/turret_branch_v01.png`.

**Signature — Bersaglieri Charge Post** `[L]`

> STRUCTURE, Italian Bersaglieri Charge Post, a lightweight staging post with
> stacked kit bags and bicycles/motorcycles leaning against a low sand-colored
> wall, mobile Mediterranean construction, single top-down orientation,
> upgrade tiers show a larger staging area, insignia pip only, transparent
> background.
> File: `res://assets/art/towers/national/italy/signature_bersaglieri_charge_post/state_v01.png`

**Friendly units produced by the Charge Post**
(`units/national_skins/italy/`): Bersaglieri fast-infantry squad (4-frame
sprint cycle, dust trail per GDD §8.2.5). Same engine-applied friendly outline
rule as §2.1.

#### Enemy archetype variants named for Italy (GDD §10.2)

| Archetype | Italian variant name | Prompt |
|---|---|---|
| E1 Basic Infantry | Italian Riflemen | UNIT, Italian Riflemen, three-figure upright loose-file infantry group, warm-olive uniforms, distinct Italian helmet shape, single top-down orientation, 4-frame walk cycle. |
| E2 Fast Infantry/Scout | Italian Bersaglieri Scouts | UNIT, Italian Bersaglieri Scouts, two-figure forward-lean scout pair with a light motorcycle, dust trail, single top-down orientation, 4-frame walk cycle. |
| E5 Light Vehicle | Italian Armored Car | UNIT, Italian Armored Car, small low wheeled body, sand/olive paint, single top-down orientation, static hull + 2-frame wheel roll. |
| E6 Medium Armor | Carro Armato M13 | UNIT, Carro Armato M13, classic hull/turret/tracks silhouette, lighter structural proportions, single top-down orientation, static hull + 2-frame track roll. |

No GDD-named Italian variant yet exists for E3, E4, E7–E12.

---

### 2.6 Japan

**Visual identity:** khaki, olive-brown, warm canvas, dark metal,
timber/bamboo field construction where appropriate. Character: **compact,
field-built, terrain-adapted.**

**Insignia:** an abstract solar disc variant, with a different ray count and
framing than any historical marking — **never** a real-world sun emblem
(§1.6).

#### Towers

Per §1.7, T1–T7 split into emplacement (static, shared L1–L4) and turret
(rotates in-engine; base state shown, L1–L2) layers. T8/T9 stay fused.

| # | Archetype | Japanese name | Leaning (GDD §8.2.6) | Scope | Emplacement layer (shared L1–L4) | Turret layer — base state (L1–L2) |
|---|---|---|---|---|---|---|
| T1 | Automatic Gun | Type 92 Machine Gun Nest | +10% range, −8% RoF | [L] | EMPLACEMENT, Japanese Type 92 Machine Gun Nest base only — a low bamboo-and-earth emplacement, empty gun mount, terrain-adapted construction, no weapon. | TURRET, Japanese Type 92 weapon only — a machine gun, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T2 | Marksman Post | Sniper Hideout | +15% first-shot bonus, −8% RoF | [L] | EMPLACEMENT, Japanese Sniper Hideout base only — a foliage-screened timber hide, dark metal accents, empty firing slot, no weapon. | TURRET, Japanese Sniper Hideout weapon only — a rifle barrel, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T3 | Field Mortar | Imperial Mortar Squad | Baseline, −6% cost | [L] | EMPLACEMENT, Japanese Imperial Mortar Squad base only — a compact earth pad with stacked bomb crates, empty mortar mount, no weapon. | TURRET, Japanese Imperial Mortar weapon only — a mortar tube, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T4 | Anti-Tank Gun | Type 1 47mm AT Gun | −8% dmg, +12% RoF | [L] | EMPLACEMENT, Japanese Type 1 47mm AT Gun base only — a timber-and-earth revetment, empty gun cradle, no weapon. | TURRET, Japanese Type 1 weapon only — the compact anti-tank gun on its low carriage, cropped at the mount, single top-down orientation, centered on its rotation pivot. |
| T5 | Flak Battery | Type 96 AA Mount | Baseline | [L] | EMPLACEMENT, Japanese Type 96 AA Mount base only — a compact tripod mount base, empty center, no weapon. | TURRET, Japanese Type 96 weapon only — a light automatic AA gun, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T6 | Armored Emplacement | Type 97 Chi-Ha Turret | −10% cost, −12% dmg, +10% RoF | [L] | EMPLACEMENT, Japanese Type 97 Chi-Ha base only — a dug-in hull front and tracks, khaki/olive-brown paint, empty turret ring, no gun. | TURRET, Japanese Type 97 weapon only — the compact Chi-Ha-inspired turret, single top-down orientation, centered on its rotation pivot. |
| T7 | Heavy Artillery | Coastal Naval Gun | +18% range, +8% cost | [L] | EMPLACEMENT, Japanese Coastal Naval Gun base only — a wide carriage with stacked shells, empty gun cradle, no weapon. | TURRET, Japanese Coastal Naval Gun weapon only — the field gun barrel and breech, cropped at its mount, single top-down orientation, centered on its rotation pivot. |
| T8 | Minefield/Route Denial | Bamboo Spike Trap | +2 charges, +40% regen, cap 9 | [L] | ROUTE PROP, Japanese Bamboo Spike Trap marker cluster, low bamboo stakes over disturbed earth, base state. | n/a — triggered trap, no turret. |
| T9 | Command Post | Artillery Observation Tower | +2.0 range aura to indirect-fire towers | [L] | STRUCTURE, Japanese Artillery Observation Tower, a timber-and-bamboo watch platform with a spotting scope, no weapon, base state. | n/a — GDD: "this tower never shoots." |

**Branch states:** for T1–T7, regenerate only the **turret layer** using the
GDD branch name/effect (save as `.../{archetype}/turret_branch_v01.png`).
T8 AT branch = "Anti-Tank Minefield" (a distinct visual from the base
Bamboo Spike Trap — buried metal-cased charges rather than stakes; T8 has
no turret split, so this regenerates the full fused marker).

**Signature — Special Attack Airfield** `[L]`

Content-policy note carried from GDD §8.2.6: this tower was deliberately
renamed from "Kamikaze Airfield" to the neutral administrative term. It must
be presented purely as **a stored, one-use precision air strike** — no pilot
depicted, no self-destruction dramatized, no glorifying language in any
accompanying text. The strike resolves as an aircraft entering from the map
edge, diving, and an explosion — nothing else.

> STRUCTURE, Japanese Special Attack Airfield, a short timber-and-earth
> runway strip with a small control hut and a wind-sock pole, terrain-adapted
> compact construction, single top-down orientation, upgrade tiers show a
> longer runway and a second hut, solar-disc insignia pip only, transparent
> background. No pilot figure, no aircraft depicted in a self-destructive
> pose — the airfield itself is the subject, not the strike.
> File: `res://assets/art/towers/national/japan/signature_special_attack_airfield/state_v01.png`

The strike VFX itself (aircraft approach + explosion) belongs in the shared
effects set (§3) and must follow the same no-pilot, no-glorification rule.

**Deferred `[P1]` — do not prompt yet:** "Assault Infantry Post" (renamed
from "Banzai Infantry Post" per GDD §8.2.6, explicitly deferred) and the Zero
Fighter Beacon (→ T10, deferred with all four nations' air-beacon towers).

#### Enemy archetype variants named for Japan (GDD §10.2)

| Archetype | Japanese variant name | Prompt |
|---|---|---|
| E1 Basic Infantry | Japanese Infantry Platoon | UNIT, Japanese Infantry Platoon, three-figure upright loose-file infantry group, khaki/olive-brown uniforms, distinct Japanese helmet shape, single top-down orientation, 4-frame walk cycle. |
| E2 Fast Infantry/Scout | Japanese Recon Patrol | UNIT, Japanese Recon Patrol, two-figure forward-lean scout pair, dust trail, single top-down orientation, 4-frame walk cycle. |

No GDD-named Japanese variant yet exists for E3–E7, E9–E12.

---

## 3. Shared projectiles and effects

These are nation-neutral base assets, tinted per nation using each nation's
tendencies from §2 (a restrained tint/recolor pass, not a redesign — GDD
§16.2 treats nationality as the weakest visual signal even here). File path:
`res://assets/art/shared/vfx/{effect}_v{version}.png`, `CUTOUT` contract
(§1.1) unless noted otherwise.

| Effect | Notes | Prompt |
|---|---|---|
| Muzzle flash | Per damage-type variant (Small Arms, Armor-Piercing, Explosive) | CUTOUT, small directional muzzle-flash burst, [damage-type] color cue, transparent background, no smoke trail baked in. |
| Tracer | Per nation tint (GDD §6: "German MG42... tracer color") | CUTOUT, short straight tracer streak with a soft glow core, transparent background, single orientation for engine rotation along the firing line. |
| Projectile glow (AP shot) | — | CUTOUT, small elongated AP tracer-round glow, transparent background. |
| Mortar/artillery arc marker | Ground-plane indicator, not a sprite in the usual sense | CUTOUT, dashed arc trajectory line with a small shell icon, transparent background, UI-legible at small scale. |
| Impact marker — Small Arms (ricochet) | GDD: "a distinctly weak, high, metallic ping... is the fastest teaching signal in the entire game" — the visual companion to that ping | CUTOUT, small bright ricochet spark burst, thin and weak-looking, transparent background. |
| Impact marker — Armor-Piercing | — | CUTOUT, sharp small penetration spark burst, transparent background. |
| Impact marker — Explosive | — | CUTOUT, broad soft-edged explosion burst with a brief smoke puff, no debris depicting people or gore, transparent background. |
| Impact marker — Anti-Air | — | CUTOUT, small airburst flak puff, transparent background. |
| Dust puff | Vehicle movement | CUTOUT, small soft ground dust puff, transparent background. |
| Smoke puff / Suppression cloud | Tied to the Suppressed status | CUTOUT, soft grey-white smoke puff cluster, transparent background, readable at small scale as "obscured." |
| Suppressed status indicator | Non-color-coded per accessibility rule (GDD §13.9) | CUTOUT, red hatching overlay pattern plus a small pinned-down glyph, transparent background, distinct from color alone. |
| Spotted status indicator | — | CUTOUT, small target-reticle glyph, transparent background. |
| Shield segment (Escort/Cover Screen) | Translucent hex bubble per GDD E10 | CUTOUT, single translucent hexagonal shield-bubble segment tile, transparent background, tileable edge. |
| Non-graphic defeat puff | Infantry death resolution — explicitly not gore | CUTOUT, soft pale dispersal puff with a small dropped-token silhouette, no body, no blood, transparent background. |
| Vehicle wreck | Static faded hulk, 3s fade in engine | CUTOUT, static burned-out vehicle hulk silhouette matching the source vehicle's shape, no visible remains, transparent background. |

---

## 4. Formatting and sprite-sheet notes

- **One image per prompt, not a packed grid.** This repo's existing
  environment-art pipeline (`ART_GENERATION_PROMPTS.md`) generates one
  numbered output per prompt rather than asking a generator for a full sprite
  sheet in one shot, because generators keep frame alignment and spacing far
  more reliably per-image than in a packed grid. Follow the same pattern
  here: generate each walk/roll frame, each tower state, and each VFX piece
  as its own transparent PNG, then assemble frames into a Godot
  `AnimatedSprite2D` / `SpriteFrames` resource (GDD §16.2) after the fact.
- **Canvas size:** generate at 2× the unit's reference size from §1.3 (e.g.
  a 32px infantry sprite → 64px canvas; conventionally rendered at a larger
  working resolution such as 256px or 512px square, then downsampled, the
  same way the existing build-pad cutouts use a 256px canvas for a 128px
  gameplay footprint).
- **Versioning:** save the first accepted output per item as `_v01`; keep
  rejected generations out of the production folders, matching the existing
  convention in `ART_GENERATION_PROMPTS.md`.
- **File path contract** (from `assets/data/art/art_asset_catalog.json`):

  | Category | Path pattern |
  |---|---|
  | Tower — emplacement layer (T1–T7 only, no `{state}`, shared L1–L4) | `res://assets/art/towers/national/{nation}/{archetype}/emplacement_v{version}.png` |
  | Tower — turret layer (T1–T7 only, rotates in-engine) | `res://assets/art/towers/national/{nation}/{archetype}/turret_{state}_v{version}.png` |
  | Tower — fused piece (T8, T9, and all six signature towers) | `res://assets/art/towers/national/{nation}/{archetype}/{state}_v{version}.png` |
  | Friendly unit (national) | `res://assets/art/units/national_skins/{nation}/{unit_family}/{variant}_v{version}.png` |
  | Enemy archetype (national skin) | `res://assets/art/enemies/archetypes/{nation}/{archetype}/{state}_v{version}.png` |
  | Shared VFX | `res://assets/art/shared/vfx/{effect}_v{version}.png` |

  `{state}` is `base` (L1–L2) or `branch` (L3–L4).

  Nation folder slugs: `us`, `britain`, `soviet_union`, `germany`, `italy`,
  `japan` (matching the lowercase, underscore-separated pattern already used
  for theater folders like `western_europe`).
- **Acceptance checklist:** before treating any generated output as usable,
  run it through the same checks `ART_GENERATION_PROMPTS.md` already applies
  to environment art — silhouette test (identifiable as a solid black shape
  at native size), native-scale check, function/readability check, contrast
  against the target theater's ground tiles, rotation check (does it still
  read correctly when the engine rotates it?), style consistency against the
  Unit & Tower Style Lock, grayscale test, and a 25%-blur test.
- **Friend/foe and status overlays are engine-applied, not art.** Outlines,
  HP bars, Suppressed hatching, and Spotted reticles are drawn by the game,
  not baked into the sprite — keep every generated unit/tower sprite "clean"
  of these.
