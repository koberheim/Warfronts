# FRONTS OF WAR — WWII Tower Defense
## Master Game Design Document & Solo Production Blueprint
**Document version:** 1.1 (Master Plan — Godot engine revision)
**Target platform:** Steam / Windows (PC-first, premium, single-player)
**Engine:** Godot 4.x (current stable at production start), native 2D
**Team:** 1 creator + Claude Code / Codex for programming + purchased/commissioned art & audio
**Target price:** $14.99 USD
**Target first-release production window:** 11–13 months from vertical slice start

> **Revision note (v1.1):** The original v1.0 plan specified Unity. After review, the engine decision was changed to **Godot 4.x**, specifically because this project's programming is done entirely by AI coding agents (Claude Code, Codex) rather than a human in an IDE with a mouse. See §3.2 for the full rationale. Every section below reflects the Godot decision; no other design, scope, or content commitment in this document changed.

---

## 0. How to read this document

Every feature in this document carries a scope tag. These tags are binding. If something is not tagged, it does not exist.

| Tag | Meaning |
|---|---|
| **[VS]** | Required for the vertical slice (playable, fun, shippable-quality 15 minutes) |
| **[L]** | Required for first commercial release (includes everything tagged VS) |
| **[P1]** | Post-launch free update / paid DLC candidate |
| **[X]** | Explicitly out of scope. Do not build. Do not prototype. |

Section 18 contains the anti-feature-creep contract. Section 19 contains the Claude Code implementation prompt ladder, which is the operational output of this plan.

---

## 1. Executive summary

**Fronts of War** is a premium, single-player, path-based tower defense game set in a stylized, fictionalized WWII "theaters of war" framing. The player picks an alliance (Allies or Axis), then a principal nation (United States, Britain, Soviet Union / Germany, Italy, Japan), and defends supply lines, crossroads, and fortified positions against escalating combined-arms waves from the opposing alliance.

The central design bet is a **shared archetype + faction expression** system: nine universal tower archetypes carry all mechanical weight, each nation reskins and stat-leans those nine, and each nation gets exactly **one genuinely unique signature tower** plus **three doctrines**. This produces six distinct-feeling armies from roughly one army's worth of engineering and balance surface.

The second design bet is **one campaign spine, two alliance framings**. Twelve missions, eight maps, mirrored briefings. Playing as Italy versus playing as the Soviet Union changes your toolkit, your enemy pool, and your presentation — it does not require twelve more maps.

The third design bet is **readability as the primary quality bar**. A player must always be able to answer, within two seconds of a leak: *what got through, why did my defense fail to stop it, and what should I have built instead?* Every art, UI, and systems decision in this document is subordinate to that question.

**The elevator pitch:** *Kingdom Rush's clarity and pace, a Company of Heroes silhouette language, and six national toolkits that actually feel different — in a tower defense you can finish a mission of on your lunch break.*

---

## 2. Design pillars

These six pillars are the tiebreakers. When two designs conflict, the higher pillar wins.

### Pillar 1 — Readability before everything
The player always understands: what an enemy is, what armor class it belongs to, what damage type beats it, where every tower can reach, what is currently being shot at, and why something died or didn't. Silhouette, color, icon, and motion all carry redundant information. If a mechanic cannot be made legible in a 1920×1080 window at 2× speed, it is cut, not simplified.

**Practical tests:** A new player with no WWII knowledge identifies "that is heavy armor and my machine guns are useless against it" by wave 6, unprompted. No damage number in the game requires arithmetic to act on.

### Pillar 2 — National identity without six balance ecosystems
Nations differ through: stat leaning on shared archetypes (±15% envelope), one unique signature tower, three doctrines, upgrade-branch flavor, and full audio-visual theming. Nations do **not** differ through: unique resource systems, unique enemy interactions, unique win conditions, or bespoke mechanics that only one nation's players ever learn.

**Practical test:** A balance change to the Anti-Tank Gun archetype should require re-tuning six numbers, not six systems.

### Pillar 3 — Combined arms and honest counterplay
Every mission asks the player to solve armor, air, infantry mass, and support/escort threats simultaneously. Every enemy archetype has **at least three viable counters** across **at least two nations' toolkits**. There are no trap towers and no mandatory towers. Placement, timing, and mix matter more than raw tower count.

**Practical test:** For each enemy archetype in the balance sheet, we can name three distinct counter builds that clear it, and no single tower that clears everything.

### Pillar 4 — Short missions, escalating pressure, clean finales
A mission is 10–15 minutes. The rhythm is plan → execute → spike → recover → escalate → finale. No wave outstays its welcome; no mission ends with three minutes of mopping up one slow tank. Each mission ends with a readable, mechanically fair boss or final assault that tests the specific lesson that mission taught.

**Practical test:** Median mission completion 12 minutes. No wave takes longer than 75 seconds from first spawn to last kill on a competent build.

### Pillar 5 — Replay through identity, not grind
Replay value comes from: six nations × three doctrines × eight maps × four difficulties × optional challenge modifiers × star objectives. It does **not** come from persistent stat upgrades, daily login rewards, or currency grinding. This is a premium product; the player's second playthrough must be *different*, not *stronger*.

**Practical test:** A player who has 100%'d the campaign as the United States has genuine new decisions to make on mission 1 as Japan.

### Pillar 6 — Production realism for a solo AI-assisted developer
Every system is designed to be implemented as small, low-coupling, individually testable C# scripts operating on data defined in Godot `Resource` files. Content is data, not code. Balance lives in one place. Nothing in the shipping game requires runtime pathfinding solvers, networking, procedural generation, physics simulation, or bespoke tooling beyond two simple editor plugins.

**Practical test:** Any single Claude Code task in Section 19 fits in one focused session and ends with a concrete acceptance check.

---

## 3. Committed design decisions

This section closes every open question in the brief. These are decisions, not options.

### 3.1 Gameplay presentation: **Option A — top-down path-based tower defense with fixed build pads**

**Decision:** Classic top-down (orthographic, slight artistic 3/4 lean in the painting, but mechanically pure top-down) path-based tower defense. Enemies travel visible roads and tracks from entry markers to a defended objective. Towers are placed on **discrete build pads** authored into each map.

**Why A wins:**
- **Readability.** A top-down map is the single most legible way to show route, range circles, and threat position simultaneously. Range is a circle on a plane — no perspective distortion, no occlusion, no "is that tower's range reaching the bridge or not?"
- **WWII theming.** Roads, rail lines, river crossings, hedgerow lanes, and mountain passes *are* the historical shape of the war. A route-based map reads instantly as a battlefield map-table. Ground armor advancing up a road toward an anti-tank gun is the genre's iconic image.
- **Solo development speed.** Top-down means a **single sprite can rotate 360° freely** and remain correct. No 8-direction sprite sheets. No 3D rigging. No animation retargeting. This one consequence saves an estimated 60–70% of the character art budget versus any side or isometric view.
- **Strategic placement.** Build pads near corners, chokes, and overlooks create real placement decisions and let the designer guarantee that every map has interesting spots and dead spots.
- **Godot implementation.** Path following via `Path2D`/`PathFollow2D`, range as a squared-distance check on a 2D plane, and `Node2D.rotation` are three of the simplest, most native things the engine does.
- **Map variety.** Route topology (loops, splits, merges, crossings, air corridors) is a cheap, high-yield variety lever that costs no new code once the path system exists.

**Why B (fixed-side horizontal lanes) is ruled out:** Lane defense is excellent for readability but structurally limits map variety to "how many lanes and what's in them." It fights the WWII theme — real theaters are about terrain and roads, not corridors. It also pushes toward a lane-clearing/idle feel that competes with a crowded genre space (lane defense/auto-battler) rather than the premium TD space we're targeting.

**Why C (node-and-road strategic map) is ruled out:** This is a strategy-layer concept masquerading as a TD presentation. It requires either abstracting combat (losing the moment-to-moment fun) or building a second simulation layer. It is a strong **[P1]** meta-campaign idea and a terrible first-release core.

### 3.2 Engine: **Godot 4.x, native 2D, C#**

**Decision (revised from v1.0):** Godot 4.x, using its native 2D pipeline and C# as the scripting language. This replaces the original Unity recommendation.

**Why this decision was revisited:** the original Unity pick was made on general engine merits — 2D fit, C# code quality, ScriptableObject-driven data workflow, Steamworks maturity. Those are all real. But this project's operating model is unusual: **there is no human writing code.** Claude Code and Codex are the entire programming staff, working across ordinary sessions and headless, GUI-less remote containers (this document is itself being produced in one). The engine choice has to be evaluated against *that* constraint first, not just against generic 2D-game merits.

**Why Godot wins under that constraint:**
- **The project format is plain text, and that is decisive.** Godot scenes (`.tscn`), resources (`.tres`), and scripts (`.gd`/`.cs`) are all human- and machine-readable text files. An agent can create a tower, wire a scene, add nodes, set exported properties, and define a new `Resource` type entirely through the file-editing tools it already has — no live editor session, no display, no bridge process required. Unity's equivalent files (`.unity` scenes, `.prefab` assets) are YAML with GUID/fileID cross-references that are fragile and effectively unsafe to hand-edit; manipulating them reliably requires a **running, GUI-attached Unity Editor process** driven through a live MCP bridge. That is a real dependency this project cannot always guarantee, since much of its development happens in headless cloud sessions with no display attached.
- **MCP tooling exists on both engines in 2026, so this isn't a tooling-availability argument — it's a dependency argument.** Unity shipped an official first-party MCP server in its CLI in June 2026, and it is polished; Godot has a healthy set of community MCP servers (GDAI MCP, StraySpark's Godot MCP, others) covering live inspection, screenshots, and headless test runs. Godot's advantage is that its MCP servers are a *convenience* layer on top of an already fully agent-editable text format, whereas Unity's MCP server is closer to a *requirement* for anything beyond raw script edits.
- **Godot's `Resource` type is a near-exact analog of Unity's `ScriptableObject`**, so the entire data-over-code architecture this document already commits to (§15.1) — towers, enemies, nations, doctrines, maps, and missions as data assets, not code — ports over with no conceptual change, only a syntax change.
- **Godot's 2D pipeline is native, not a mode layered onto a 3D-first engine.** This is a fully 2D top-down game; Godot's renderer, camera, and physics/collision systems for 2D are first-class citizens rather than "2D URP" running inside a 3D-oriented engine.
- **Free and open source.** No seat licensing, no runtime-fee exposure, no dependency on a vendor's pricing model for a solo premium release.
- **C# is still fully supported** (Mono/.NET build of Godot 4), which preserves this document's existing assumption of a statically typed, well-tooled, high-quality-AI-codegen language. GDScript remains available as a lighter-weight fallback for editor tooling and glue code where its simplicity is an asset.

**What is given up, honestly:** Godot's C# API has less training-data coverage than Unity's, and the community is smaller. Concretely, this means an agent will occasionally need to consult current Godot docs (via WebFetch) rather than relying purely on memorized idiom, and some Unity-specific ecosystem pieces (Steamworks.NET, DOTween, Addressables) need Godot-native replacements (§15.2, §15.5). None of this is a blocker; it is a small, one-time translation cost against a large, ongoing operational benefit.

**Engine version policy:** track the current stable Godot 4.x release at production start (not an LTS designation — Godot does not brand releases as LTS the way Unity does; instead, pin a specific stable minor version for the whole production and only move up deliberately between milestones, never mid-milestone).

### 3.3 UI framework: **Godot `Control`-node UI for all runtime UI. Custom `EditorPlugin` docks for editor tooling.**

**Decision and justification:**
- Runtime UI in this game is HUD-heavy and **world-anchored**: floating health bars, range circle overlays, build-pad radial menus, damage popups, tower tooltips positioned relative to world objects. Godot's `Control` nodes combined with `CanvasLayer` (for fixed HUD chrome) and world-space-anchored `Control`/`Node2D` hybrids (for floating health bars and tooltips, using `get_global_transform_with_canvas()` or a `Node2D` parent with billboarded `Control` children) cover both cases natively, and this is one of Godot's best-documented, most idiomatic workflows — it is *the* standard way Godot games build UI, not a secondary system.
- Because every `Control` node's layout, anchors, and bound script are themselves plain-text scene data, an agent can construct and modify entire UI screens by editing `.tscn` files directly, the same way it edits gameplay scenes. There is no separate UI serialization format to reason about, unlike Unity's uGUI-prefab-vs-UI-Toolkit-stylesheet split.
- Godot's theming system (`Theme` resources) is also text-based and centralizes fonts, colors, and 9-patch panel styles in one editable asset — a good fit for the "commander's map table" diegetic frame (§3.4) and for the accessibility requirements in §13.9 (UI scale, colorblind palettes) since a `Theme` swap can retint the whole UI from one file.
- **Editor tooling** (the Wave Editor and Balance Dashboard, §15.6) is built as Godot `EditorPlugin`s with `Control`-based custom docks — Godot's standard, first-class way to extend its own editor. Godot's `GraphEdit` node in particular is a strong native fit for the Wave Editor's timeline-of-spawn-blocks view, and needs little custom drawing code to get a professional-feeling tool.
- **Performance note:** as in the original plan, split UI concerns aggressively — a static HUD chrome layer, a dynamic per-unit overlay layer, and a tooltip layer — so that one floating health bar moving never forces a redraw of unrelated UI. Godot's `CanvasItem` redraw model rewards this the same way Unity's canvas-rebuild model did.

### 3.4 Art direction: **Clean 2D painted top-down battlefield with sprite units — presented in a "war table" frame**

**Decision:** Hand-painted (or asset-store-sourced and unified) top-down terrain tiles and props, with clean, high-contrast 2D sprite units viewed from directly above. The HUD frames the play area as a commander's map table — wooden edges, brass fittings, paper mission slips, grease-pencil annotations — which does three enormous amounts of work at once:

1. It **justifies abstraction**. Units are tokens on a table. This solves the tone problem (Section 14) elegantly: nobody depicts graphic violence on a war table.
2. It **hides art budget**. Prototype-quality unit sprites read as "counters" rather than as bad art. The frame carries perceived production value while the units stay cheap.
3. It **makes UI diegetic and cheap**. Tooltips are paper cards. The wave preview is a teletype strip. The speed control is a brass lever. This is high-perceived-polish UI made from a small number of reusable painted elements.

**Why not the alternatives:**

| Option | Verdict | Reason |
|---|---|---|
| Stylized 2D illustrated map-table | **Adopted as the framing layer** | Best tone fit, cheapest UI polish |
| Clean 2D sprite/painted battlefield | **Adopted as the play layer** | Free 360° rotation, no rigging, best readability-per-dollar |
| Low-poly 3D diorama | Rejected | Modeling + texturing + lighting + LOD + animation. 3–4× the asset workload for no readability gain in top-down. |
| 2.5D sprites on 3D terrain | Rejected | Worst of both: 3D terrain authoring cost plus sprite sorting bugs plus perspective range distortion. |

Full art specification in Section 13.

### 3.5 Other locked decisions

| Question | Decision |
|---|---|
| Do enemies attack towers? | **No — with one exception.** Enemy Siege/Artillery units *suppress* towers (temporary disable, 6s, visible red overlay). Nothing is ever destroyed. This preserves readability, prevents rebuild frustration, and still creates a real threat that demands counterplay. |
| Placement rules | **Discrete build pads** authored per map, plus **free placement of route-denial (mine/trap) items directly on path segments**. |
| Currency | **One build currency (Supply) + one ability currency (Command Points).** No third currency. |
| Persistent meta stat upgrades | **No.** [X] Unlocks are content, never power. |
| Hero units / commanders as controllable actors | **No.** [X] Commanders are presentation. Doctrines are the mechanical layer. |
| Pre-mission loadout | **Yes.** 6 tower slots from the nation's 10, plus 1 doctrine. |
| Multiplayer / co-op | **No.** [X] Not at launch, not planned. |
| Procedural maps | **No.** [X] All maps hand-authored. |
| Mod support | **No at launch.** [P1] — the data architecture makes it cheap later, which is reason enough not to do it now. |

---

## 4. Launch scope (firm)

This is the content contract. Additions require a deletion of equal size.

| Element | Vertical Slice [VS] | First Commercial Release [L] | Post-launch [P1] |
|---|---|---|---|
| **Maps** | 1 (Bocage Crossroads) | **8** | +4 (Mountain Pass, Airfield Perimeter, Fjord Landing, Steppe Rail Junction) |
| **Campaign missions** | 1 | **12** (3 acts × 4) | +6 "Second Front" campaign |
| **Skirmish maps** | 0 | **8** (all launch maps, all nations, 4 difficulties) | — |
| **Endless mode** | 0 | **8** (all launch maps) | Leaderboards |
| **Mission length** | 10 min | **10–15 min** (finales 15–18) | — |
| **Waves per mission** | 12 | **16–20** standard, **22–24** finale missions | — |
| **Tower archetypes** | **4** (Automatic Gun, Field Mortar, Anti-Tank Gun, Command Post) | **9** | +1 (Air Support Beacon) |
| **Nations** | 1 (United States) | **6** | — |
| **Towers per nation** | 5 | **10** (9 archetype + 1 signature) | 11 |
| **Total buildable tower entries** | 5 | **60** (54 archetype variants + 6 signatures) | 66 |
| **Signature towers** | 1 (Arsenal of Democracy) | **6** | — |
| **Doctrines** | 0 | **18** (3 per nation) | +6 |
| **Enemy archetypes** | 4 | **12** | +3 |
| **National enemy visual variants** | 3 | **~36** (12 archetypes × 3 nations per alliance, reskin-only) | — |
| **Bosses / major elites** | 1 | **4 bosses + 3 elite variants** | +2 |
| **Difficulties** | 1 | **4** (Recruit / Regular / Veteran / Elite) | Ironman modifier |
| **Upgrade levels per tower** | 3 | **4** (fork into 2 branches at level 3) | — |
| **Challenge modifiers** | 0 | **10** | +6 |
| **Achievements** | 0 | **40** | +10 |
| **Persistent meta-upgrades** | No | **No** | **No** |
| **Localization** | EN | **EN only at launch** | ES, DE, FR, RU, JA, ZH-Hans, PT-BR |
| **Voice** | None | **Non-verbal + short generic radio callouts, subtitled** | Full VO pass |

### 4.1 Content math sanity check

The scary number is "60 towers." It is not 60 towers of work. It is:

- **9 archetype behaviors** (real code — targeting, projectile, effect, upgrade fork logic)
- **54 `TowerDefinition` resource assets** (data rows: name, cost, ±stat leaning, art refs, SFX refs, branch names)
- **6 signature behaviors** (real code, one each)
- **~68 unique sprites** (9 archetypes × 6 nations = 54 base tower sprites + 6 signatures + 8 shared projectile/effect sets), each with 2 visual upgrade states rather than 4 (levels 1–2 share art, levels 3–4 share branch-specific art)

That is a tractable art commission and a tractable data-entry job. The engineering is nine behaviors plus six specials.

### 4.2 Vertical slice definition [VS]

**"Bocage Crossroads, United States, 12 waves."** Complete when a stranger can play it start to finish, lose once, understand why, change their build, and win — with no explanation from the developer.

Contents:
- 1 map, hand-painted at final quality (this is the art-direction proof)
- 1 nation (US) with 4 archetype towers + Arsenal of Democracy signature
- 4 enemy archetypes: Basic Infantry, Fast Infantry, Light Vehicle, Medium Armor
- 1 boss: Breakthrough Panzer
- 12 waves, hand-authored
- Full build/upgrade/sell loop, 4-level upgrades with branch fork
- Supply economy, Command Points, one tactical ability
- Wave preview, speed controls (1×/2×/3×), pause-with-build
- Victory/defeat screens with a **post-mortem panel** (see 12.9 — this is a VS requirement, not polish)
- Placeholder-but-consistent audio
- Steam not required

**Vertical slice is done when it is fun without content.** If 12 waves on one map with five towers is not fun, more content will not fix it. Do not proceed to Milestone 4 until an external playtester asks to play again.

---

## 5. Combat model: damage, armor, and counterplay

### 5.1 The model in one sentence

**Four damage types × four armor classes = a 16-cell multiplier table, plus one targeting flag (Air) and two non-damage statuses (Suppressed, Spotted). Nothing else.**

No flat reduction. No hidden modifiers. No per-unit resistances. No armor penetration values. No critical hits. No elemental stacking. The multiplier table is visible to the player in the codex and summarized as icons on every unit and tower card.

### 5.2 Damage types

| Type | Icon / Color | Description | Typical sources |
|---|---|---|---|
| **Small Arms (SA)** | Yellow • bullet | High rate, low per-shot, shreds unarmored bodies, useless on plate | Automatic Gun, Marksman, Armored Emplacement (secondary) |
| **Explosive (HE)** | Orange • burst | Area damage, good against clustered soft and light targets, degrades against heavy plate | Field Mortar, Heavy Artillery, Minefield (HE variant) |
| **Armor-Piercing (AP)** | Red • chevron | Single-target, high per-shot, scales *up* against armor, wasteful on infantry | Anti-Tank Gun, Armored Emplacement (primary), Marksman (partially) |
| **Anti-Air (AA)** | Cyan • wing | Only damage type that can hit the Air class. Cannot hit ground unless a tower branch explicitly grants it | Flak Battery |

### 5.3 Armor classes

| Class | Icon | Reads as | Examples |
|---|---|---|---|
| **Soft** | Cloth square | Unprotected infantry, unarmored trucks | Rifle Squad, Conscripts, Supply Convoy |
| **Hardened** | Half shield | Entrenched/armored infantry, light vehicles, half-tracks | Shock Troops, Armored Car, Support Vehicle |
| **Armored** | Full shield | Medium tanks, assault guns | Panzer IV, Sherman Column, T-34 |
| **Heavy** | Double shield | Heavy tanks, command vehicles, bosses | Tiger, Breakthrough Panzer, Command Column |
| **Air** | Wing outline | Flying units. *Targeting class, not armor class.* Air units also carry Soft or Hardened armor for damage math. | Bombers, Recon Aircraft |

### 5.4 The multiplier table

| | Soft | Hardened | Armored | Heavy |
|---|---|---|---|---|
| **Small Arms** | **1.00** | 0.55 | 0.20 | 0.08 |
| **Explosive** | 0.85 | **1.00** | 0.65 | 0.40 |
| **Armor-Piercing** | 0.45 | 0.85 | **1.25** | **1.50** |
| **Anti-Air** | — | — | — | — |

**Anti-Air vs Air:** 1.00. All other damage types vs Air: **0.00** (they cannot target it at all — the tower simply does not acquire air targets, which is clearer than doing 0 damage). Air units' Soft/Hardened armor class then modifies incoming AA damage: AA vs Soft-air 1.00, AA vs Hardened-air 0.75.

**Design intent per row:**
- Small Arms is the **efficiency** damage type: cheapest DPS in the game, but it collapses hard past Hardened. This creates the classic "my whole line is machine guns and a tank just walked through" learning moment — which is *good*, because it is unmistakable.
- Explosive is the **generalist**: never best, never useless, always fine. It is the safety net for confused players and the answer to swarms.
- Armor-Piercing is the **specialist**: expensive per point of DPS, wasted on infantry, mandatory against Heavy.
- Anti-Air is the **hard gate**: air waves are unblockable without it. This is intentional and is telegraphed three waves in advance every time (Section 10.3).

### 5.5 Statuses (non-damage)

Only two, both visually unmistakable:

| Status | Effect | Applied by | Visual |
|---|---|---|---|
| **Suppressed** | −40% move speed, −50% enemy special-ability frequency, duration 2.5s refreshing | Automatic Gun (branch), Field Mortar (smoke branch), Minefield (concussion branch), several doctrines | Grey dust cloud on the unit + downward arrow icon |
| **Spotted** | Target takes +25% damage from all sources; also reveals Concealed units | Command Post (Spotter branch), Marksman (branch), Recon doctrine abilities | Red crosshair reticle drawn over the unit |

**Concealed** is an enemy property, not a status: certain units are untargetable by towers without a Command Post or Spotted source in range. Concealed units render at 45% opacity with a dashed outline so the player *sees them coming* and understands the threat before it hurts.

### 5.6 What enemies can and cannot do

| Mechanic | In game? | Notes |
|---|---|---|
| Shields | **Yes**, one archetype only (Escort Vehicle) — a directional damage-absorbing bubble over nearby allies, with a visible HP bar of its own | Killing the source pops the bubble instantly. Readable, counterable. |
| Repair / heal | **Yes**, one archetype only (Support Vehicle) — repairs damaged vehicles in a small radius, visible green tether beam | The tether is the tell. Cut the tether, cut the problem. |
| Regeneration | **No** [X] | Invisible healing is anti-readability. Everything that heals must show a beam. |
| Camouflage / concealment | **Yes**, one archetype (Recon/Scout) | See 5.5. Always visible as a ghost; just not targetable. |
| Armor that changes mid-run | **No** [X] | An enemy's armor class never changes. Ever. |
| Immunities | **No** [X] | Nothing is immune to anything except the Air/ground targeting split. |
| Attacking towers | **Suppress only** (Siege archetype) | Never destroyed. 6s disable, red hatching overlay, audible warning. |

### 5.7 How the UI communicates counters

Four redundant channels, all shipping in [VS]:

1. **Unit cards.** Hovering any enemy (or opening the wave preview) shows its armor class icon and a three-row strip: `SA ✖ | HE ~ | AP ✔`. Filled check, half-circle, and cross glyphs — never color alone.
2. **Tower cards.** Every tower card shows its damage type badge and the phrase "Strong vs: [icons]" / "Weak vs: [icons]".
3. **Live combat feedback.** Damage numbers are color-coded *and* prefixed: ineffective hits show a small grey number with a downward chevron and play a distinct "ricochet" ping. This is the single most important teaching tool in the game — the player *hears* their machine guns failing before they read anything.
4. **Post-mortem panel** (Section 12.9). On defeat, a panel states plainly: "Leaked: 4× Panzer IV (Armored). Your damage output vs Armored was 12% of your total. Consider: Anti-Tank Gun, Armored Emplacement, or Heavy Artillery."

### 5.8 Anti-trap-tower rules

Enforced as hard design rules, checked at every content review:

- **No tower is ever a strict downgrade of another.** If two towers occupy the same role, they must differ on at least two of: range, damage type, targeting priority, cost curve.
- **No tower's value is hidden.** If a tower's benefit is an aura or a debuff, it must display a persistent visual radius and a numeric readout on its inspection panel.
- **Every archetype is worth building on every map.** If a map's route topology makes an archetype useless, the map is wrong, not the tower.
- **Every upgrade branch changes behavior, not just numbers.** A branch that adds +30% damage and nothing else is deleted and replaced.
- **The three-counter rule.** For every enemy archetype, the balance sheet must list three distinct tower-based counters that clear it at reasonable cost. If it has fewer, either the enemy is redesigned or a counter is added.

---

## 6. Tower archetype roster [L]

Nine archetypes. Every nation gets all nine, so the fundamental strategic vocabulary is universal and transferable. A tenth (Air Support Beacon) is deliberately held for post-launch.

**Universal rules for all towers:**
- Sell refund: **75%** of total invested Supply. **100% refund within 4 seconds of placement** (misclick protection) with a visible countdown ring.
- Towers may be **sold at any time, including mid-wave**, but cannot be sold while Suppressed.
- Upgrade path: **Level 1 → 2 → [branch fork] → 3 → 4.** Four levels; the fork happens when purchasing level 3, and is permanent unless the tower is sold.
- Upgrade cost multipliers vs base cost: **L2 = 0.65× · L3 = 1.15× · L4 = 2.10×.** Total invested at L4 ≈ 4.9× base.
- Every tower has a **targeting priority selector**: First / Last / Strongest / Weakest / Closest. Default per archetype is specified below. This is one dropdown, costs almost nothing, and adds enormous perceived depth.
- Range values are in **tiles** (1 tile = 1 Godot world unit at reference zoom, sized to a 64px reference sprite).
- All numbers below are **Level 1, Regular difficulty, nation-neutral baseline**. National leaning applies ±15% per Section 7.

### T1 — Automatic Gun
*The backbone. Cheap sustained anti-infantry.*

| Property | Value |
|---|---|
| Cost | 100 Supply |
| Damage type | Small Arms |
| Damage / shot | 4 |
| Rate of fire | 6.0 /s (24 SA DPS) |
| Range | 5.5 tiles |
| Targeting | Single target, hitscan tracer. Default priority: **First** |
| Targets | Ground only |
| Turn rate | Fast (0.4s to acquire new target) |

- **Role:** Volume damage against Soft. The tower you build first, most, and cheapest. Establishes the front line.
- **Effective against:** Basic Infantry, Fast Infantry, Swarm Infantry, Supply Convoy.
- **Weak against:** Everything Armored or above. Actively wasteful vs Heavy (0.08×).
- **Counterplay pressure it creates:** Encourages the player to over-invest in a comfortable tower and then get punished by wave 6's armor.
- **Branch fork (L3):**
  - **Sustained Fire** — +85% rate of fire while firing continuously at the same target (ramping over 2s, resets on target switch). Rewards long sightlines and Last/Strongest targeting.
  - **Suppressive Fire** — Applies **Suppressed** to the target and one adjacent enemy. −25% damage. Turns the tower into a control piece and a force multiplier for artillery.
- **National expression:** stat leaning + firing sound + tracer color + emplacement art. German MG42 leans rate-of-fire, Soviet Maxim leans cost, US Browning is baseline, Japanese Type 92 leans range.
- **Scope:** **[VS]**

### T2 — Marksman Post
*Long-range single-target elite killer.*

| Property | Value |
|---|---|
| Cost | 175 Supply |
| Damage type | Armor-Piercing (at 0.75× scale — see note) |
| Damage / shot | 60 |
| Rate of fire | 0.5 /s (30 raw DPS) |
| Range | **11.0 tiles** (longest direct-fire range in the game) |
| Targeting | Single target, instant. Default priority: **Strongest** |
| Targets | Ground only |

*Note on damage type:* the Marksman uses the AP multiplier row but at 0.75 scale, so it reads as "decent against everything, best against elites, never the answer to a tank column." Communicated on the card as **AP (light)**.

- **Role:** Deletes single high-value targets: Armored Infantry, Support Vehicles, Escort Vehicles, Concealed Scouts. The precision answer.
- **Effective against:** Armored Infantry, Support/Escort/Recon units, boss adds.
- **Weak against:** Swarms (rate of fire is far too low), Heavy Armor (not enough per-shot).
- **Branch fork (L3):**
  - **Overwatch** — Ignores targeting priority; automatically targets the highest-value support/escort/recon unit in range and applies **Spotted**. The "kill the healer" branch.
  - **Anti-Materiel** — +100% damage, −35% rate of fire, gains 1-tile splash. Pivots the tower toward vehicles.
- **National expression:** Britain's SAS Ambush Post leans range and gains a small stealth-reveal aura in flavor terms via the Overwatch branch; the Soviet Siberian Sniper Nest leans damage; Italy's Alpini Post leans cost and gains elevation bonus synergy.
- **Scope:** **[L]** (Milestone 5)

### T3 — Field Mortar
*Cheap indirect area damage with a minimum range.*

| Property | Value |
|---|---|
| Cost | 150 Supply |
| Damage type | Explosive |
| Damage / shell | 30 |
| Blast radius | 1.6 tiles (full damage 0.8, falloff to 40% at edge) |
| Rate of fire | 0.4 /s |
| Range | 8.0 tiles, **minimum range 2.0 tiles** |
| Travel time | 1.2s arc — **leads moving targets, can and will miss fast units** |
| Targeting | Ground point. Default priority: **Densest cluster** (custom priority, unique to indirect fire) |
| Targets | Ground only |

- **Role:** The swarm answer and the early generalist. Its minimum range and shell travel time are its costs — placement matters enormously.
- **Effective against:** Swarm Infantry, Basic Infantry, Hardened targets, anything slow or bunched.
- **Weak against:** Fast Infantry (outruns the shell), single targets (inefficient), Heavy Armor (0.40×).
- **Branch fork (L3):**
  - **Barrage** — Fires a 3-shell salvo on a 1.5× longer cooldown. Same total damage, far better against tight clusters, worse against strung-out lines.
  - **Smoke Rounds** — Alternating shells deal no damage but apply **Suppressed** in a 2.2-tile radius. Converts the mortar into the best control tower in the game.
- **National expression:** the German Wehrmacht Mortar Team leans rate of fire; the Soviet Red Army Mortar Squad leans cost and blast radius, leans down on accuracy (larger scatter); Britain's Mortar Pit is baseline with better Smoke.
- **Scope:** **[VS]**

### T4 — Anti-Tank Gun
*The armor answer. High per-shot AP, slow, ground-only.*

| Property | Value |
|---|---|
| Cost | 200 Supply |
| Damage type | Armor-Piercing |
| Damage / shot | 75 (94 vs Armored, 113 vs Heavy) |
| Rate of fire | 0.8 /s |
| Range | 7.5 tiles |
| Targeting | Single target, fast projectile (0.25s flight). Default priority: **Strongest** |
| Targets | Ground only. **Will not fire at Soft targets unless no valid armored target is in range** (auto-behavior, toggleable) |
| Turn rate | Slow (1.1s to acquire a new target) — punishes placement on tight corners where targets pass quickly |

- **Role:** The mandatory-by-wave-8 specialist. Straight-line road segments are its home.
- **Effective against:** Medium Armor, Heavy Armor, Light Vehicles, bosses.
- **Weak against:** Infantry (0.45×), swarms, air (cannot target), fast movers past its slow turret traverse.
- **Branch fork (L3):**
  - **Sabot Rounds** — +40% damage, projectile **pierces up to 3 units in a line**. The road-column deleter. Rewards placement facing down a straight.
  - **Rapid Loader** — +70% rate of fire, −20% damage, turret traverse becomes fast. The corner-and-crossroads variant.
- **National expression:** German Pak 40 leans damage (the "gun" nation); US Bazooka Squad leans cost and rate of fire, leans down on range; Soviet Anti-Tank Rifle Team is cheapest with lowest damage; Japanese Type 1 leans rate of fire.
- **Scope:** **[VS]**

### T5 — Flak Battery
*The only answer to air.*

| Property | Value |
|---|---|
| Cost | 175 Supply |
| Damage type | Anti-Air |
| Damage / shot | 12 |
| Rate of fire | 2.5 /s (30 AA DPS) |
| Range | 9.0 tiles (large radius — air routes are wide) |
| Targeting | Single target, leading projectile with small proximity burst (0.7 tile). Default priority: **First** |
| Targets | **Air only** at L1–L2 |

- **Role:** A hard gate the player must respect. Deliberately a "dead" tower during ground waves at level 1, which makes the L3 fork a genuine and interesting decision.
- **Effective against:** All air.
- **Weak against:** Literally everything on the ground until branched.
- **Branch fork (L3):**
  - **Dual Purpose** — Gains a secondary ground attack: 55 AP damage at 0.6/s, range 6.0. Now a hybrid, at a hybrid's efficiency cost. This is the branch that makes Flak worth pre-building.
  - **Predictive Fire** — +50% AA damage, +2.0 range, applies **Spotted** to air targets. The pure specialist for heavy air missions.
- **National expression:** the German **Flak 88** is the flagship — it leans hard into damage and gets the strongest Dual Purpose branch (this is the one place a nation gets a clearly best-in-slot variant, and it is offset by Germany's high costs elsewhere). Britain's Bofors leans rate of fire. Others baseline.
- **Anti-frustration rule:** Air waves are **always** announced with a distinct klaxon and a full-screen banner **three waves in advance**, and the wave preview marks them with a persistent wing icon. No player ever loses to a surprise air wave.
- **Scope:** **[L]** (Milestone 5)

### T6 — Armored Emplacement
*Dug-in tank turret. The expensive anchor.*

| Property | Value |
|---|---|
| Cost | 300 Supply |
| Damage type | Armor-Piercing (main gun) + Small Arms (coaxial) |
| Damage / shot | 45 AP main @ 1.0/s, plus 3 SA coax @ 4.0/s |
| Blast | Main gun has 0.9-tile splash |
| Range | 6.5 tiles |
| Targeting | Main gun: **Strongest**. Coax: independent, **Closest** |
| Targets | Ground only |

- **Role:** The all-rounder anchor tower. Expensive, handles both infantry and armor adequately, dominates neither. Its value is **not needing a second tower next to it** — it holds a lonely build pad by itself.
- **Effective against:** Mixed waves, medium armor, sustained pressure on a single choke.
- **Weak against:** Cost efficiency (two specialists always out-damage it), air, long range.
- **Branch fork (L3):**
  - **Heavy Turret** — Upgrades to the nation's heavy tank identity (Tiger, Churchill, IS-series flavor). +90% main gun damage, −30% rate of fire, +1 range. Loses coax.
  - **Assault Gun** — Main gun becomes Explosive with 1.8-tile splash, coax rate of fire doubled. The anti-infantry pivot.
- **National expression:** This is the most visually distinctive archetype and the strongest identity carrier — Sherman, Churchill, T-34, Panzer IV/Tiger, Carro Armato, Chi-Ha. Germany's Heavy Turret branch is explicitly the **Tiger Tank Platform** and leans damage; Britain's is the **Churchill Tank Bunker** and leans durability-as-range; the Soviet T-34 leans cost.
- **Scope:** **[L]** (Milestone 4)

### T7 — Heavy Artillery
*Map-spanning delayed barrage.*

| Property | Value |
|---|---|
| Cost | 350 Supply |
| Damage type | Explosive |
| Damage | 3-shell salvo, 45 HE per shell |
| Blast radius | 2.2 tiles per shell |
| Rate of fire | 0.2 /s (one salvo every 5s) |
| Range | **20.0 tiles** (effectively most of the map), **minimum range 6.0** |
| Fire delay | **2.5s** from firing sound to impact — a genuine lead-prediction problem |
| Targeting | Ground point, **Densest cluster**. Player may also **manually designate a fire zone** by clicking (persistent until changed) |
| Targets | Ground only |

- **Role:** Strategic damage that reaches the parts of the map towers can't. The player's answer to a distant lane they under-defended. Also the primary payoff for Suppression synergy — suppressed enemies don't outrun the shells.
- **Effective against:** Clustered anything, slow columns, siege units parked at long range, boss escorts.
- **Weak against:** Fast movers (they simply are not there when the shells land), single targets, anything within 6 tiles.
- **Branch fork (L3):**
  - **Precision Battery** (howitzer identity) — 1 shell instead of 3 with 2.6× damage, blast 1.4, fire delay reduced to 1.4s, gains +50% damage vs Armored/Heavy. The sniper artillery.
  - **Rocket Saturation** (Katyusha / Nebelwerfer identity) — 9 shells at 45% damage each scattered across a 4.5-tile radius, fire delay 3.2s. Blankets an area; devastating on swarms, unreliable on single targets.
- **National expression:** The Soviet **Katyusha Rocket Truck** and German **Nebelwerfer** are the Rocket Saturation branch names and both nations lean into it; the US **105mm Howitzer Battery** and British **Royal Artillery Battery** lean Precision. Italy's **Coastal Artillery Gun** leans range hard (+25%) and cost up.
- **Balancing risk:** Artillery that both reaches everywhere and hits everything trivializes maps. The fire delay is the balancing lever and must never be reduced below 1.4s on any branch or national variant. Log it in the balance dashboard as a hard floor.
- **Scope:** **[L]** (Milestone 5)

### T8 — Minefield / Route Denial
*Consumable placed damage. The only free-placement tower.*

| Property | Value |
|---|---|
| Cost | 90 Supply per field |
| Damage type | Explosive (HE variant) or Armor-Piercing (AT variant — a branch, not a separate tower) |
| Damage | 45 per charge |
| Blast radius | 1.2 tiles |
| Charges | 6, consumed on trigger. **Regenerates 1 charge per 12s** up to max |
| Placement | **Directly on a path segment**, not on a build pad. Minimum 2.5 tiles between fields. |
| Trigger | The first enemy to enter the radius, with a 0.4s arming delay |

- **Role:** Route denial and burst. Solves the "I have no build pad near that corner" problem, which makes it structurally important to map design freedom. Also the only tower whose value is entirely front-loaded, which makes it a real economic decision.
- **Effective against:** Swarms (HE), armored columns (AT branch), leakers, opening-wave pressure.
- **Weak against:** Sustained waves (charges run dry), air (cannot trigger), anything that arrives faster than the regen rate.
- **Branch fork (L3):**
  - **Anti-Tank Mines** — Charges become AP, 90 damage, radius 0.8, **only trigger on Hardened+**. Infantry walks safely over them. The armor-column trap.
  - **Concussion Charges** — Damage reduced 40%, but every trigger applies **Suppressed** in a 2.5-tile radius. The control-and-stall option, superb in front of artillery.
- **National expression:** Japan's **Bamboo Spike Trap** (HE base) and **Anti-Tank Minefield** (the AT branch, named) are literally two names for the two branches, and Japan leans into charge count (+2) and regen rate. Soviet **Minefield Layer** leans cost. British **Royal Engineers Minefield** leans damage and gets faster arming.
- **Scope:** **[L]** (Milestone 4)

### T9 — Command Post
*Support aura, recon, and the game's only economy source beyond kills.*

| Property | Value |
|---|---|
| Cost | 225 Supply |
| Damage | **None.** This tower never shoots. |
| Aura radius | 6.0 tiles, always drawn as a translucent ring |
| Base effects | +12% range and +8% rate of fire to all towers in radius; **reveals Concealed enemies** in radius; generates **+2 Command Points per wave** |
| Stacking | Auras from multiple Command Posts **do not stack**; the strongest applies. Prevents degenerate aura-stacking builds. |

- **Role:** The force multiplier and the strategic layer. It makes clustering worthwhile, it is the answer to Concealed units, and it feeds the ability economy.
- **Weakness:** Zero damage. Every Supply spent here is Supply not spent on DPS. On tight maps with few pads, it competes directly with a real tower — a genuine choice.
- **Branch fork (L3):**
  - **Forward Observer** — Applies **Spotted** to the strongest enemy in a 10-tile radius on a 4s rotation, and grants towers in the aura +1.5 range. This is where the **US Naval Gunfire Spotter**, **British Radar Early Warning Tower**, **Japanese Artillery Observation Tower**, and **German Radar Flak Tower** identities live. On the Forward Observer branch only, the tower additionally extends the aura's reveal to the *entire map* for Air units.
  - **Logistics Depot** — Generates **+22 Supply per wave** and +2 additional Command Points, aura effects reduced to +6% range / +4% rate of fire. This is where the **US War Bonds Supply Depot**, **Soviet Rear Echelon**, and equivalents live.

**On the economy tower question:** the brief asks whether a dedicated economy tower earns its place. **It does not** as a standalone archetype — a build-early-for-interest tower is a solved, slightly tedious pattern that rewards rote play. As a **branch of a tower that already does something**, it is excellent: the player trades support strength for income, on a tower they were considering anyway, in a limited pad slot. That is a real decision. Ship it as a branch. Never as its own tower. **[X]** to a standalone economy building.
- **Scope:** **[VS]** (base + both branches)

### T10 — Air Support Beacon **[P1]**
*Deliberately deferred.*

Called-in aircraft that strafe a designated route segment on a cooldown. The **US Mustang Air Support Beacon**, **British Spitfire Air Command**, **German Stuka Dive-Bomb Beacon**, and **Japanese Zero Fighter Beacon** all live here.

**Why deferred, on purpose:** two of six nations already have an air-themed *signature* tower (Britain's RAF Scramble Command, Japan's Special Attack Airfield). Shipping a universal air-strike archetype alongside them muddies both. Holding T10 for a free post-launch "Air War" update gives the game a strong, cheap, headline-worthy content beat and gives the two remaining nations (Soviet Union, Italy) new period-appropriate names at that time (Il-2 Ground Attack Signal, Regia Aeronautica Signal). This is a scheduling decision, not a design compromise.

### 6.1 Archetype coverage matrix

| Archetype | Soft | Hardened | Armored | Heavy | Air | Control | Utility | VS |
|---|---|---|---|---|---|---|---|---|
| T1 Automatic Gun | ★★★ | ★★ | — | — | — | ★ (branch) | — | ✔ |
| T2 Marksman Post | ★★ | ★★★ | ★★ | ★ | — | — | ★ (Spotted) | |
| T3 Field Mortar | ★★★ | ★★★ | ★★ | ★ | — | ★★★ (branch) | — | ✔ |
| T4 Anti-Tank Gun | ★ | ★★ | ★★★ | ★★★ | — | — | — | ✔ |
| T5 Flak Battery | — | ★ (branch) | ★★ (branch) | ★ (branch) | ★★★ | — | ★ (Spotted) | |
| T6 Armored Emplacement | ★★ | ★★ | ★★★ | ★★ | — | — | — | |
| T7 Heavy Artillery | ★★★ | ★★★ | ★★ | ★★ | — | — | ★★★ (reach) | |
| T8 Minefield | ★★★ | ★★ | ★★★ (branch) | ★★ | — | ★★ (branch) | ★★★ (placement) | |
| T9 Command Post | — | — | — | — | — | — | ★★★ | ✔ |

Every column has at least three ★★+ entries. The three-counter rule holds at the archetype level before national variance is applied.

---

## 7. Economy and build rules

### 7.1 Currencies

**Supply** — the build currency. Spent on placing and upgrading towers. Earned from kills and wave completion. Does not persist between missions.

**Command Points (CP)** — the ability currency. Spent on tactical abilities and on charging signature towers. Earned at a fixed rate per wave, plus Command Post generation. Capped at 12. Does not persist between missions.

**No third currency. No premium currency. No persistent currency.** [X]

### 7.2 Supply economy numbers

| Parameter | Value |
|---|---|
| Starting Supply | **500** (Recruit 650 / Regular 500 / Veteran 420 / Elite 380) |
| Kill income | **~60%** of total income. Each enemy carries a bounty roughly = 0.35 × its effective HP÷10, rounded. |
| End-of-wave income | **~40%**. Base 60 + (8 × wave number), so wave 1 pays 68 and wave 18 pays 204. |
| Early-call bonus | Calling the next wave early grants bonus Supply = **+35% of that wave's end-of-wave payout, scaled by the fraction of build time remaining**. Call it with 90% of the timer left, you get ~31% bonus. |
| Leak refund | None. Leaked enemies pay nothing. |
| Sell refund | **75%** of total invested, **100%** within 4s of placement |
| Difficulty income scalar | Recruit ×1.20 / Regular ×1.00 / Veteran ×0.90 / Elite ×0.82 |

**Preventing early snowball:** the early-call bonus is capped in absolute terms (max +90 Supply per wave regardless of scaling), and enemy bounties do **not** scale with difficulty (harder enemies have more HP but the same bounty, so higher difficulties are genuinely poorer). A player who rushes waves 1–5 perfectly ends wave 5 with roughly 15% more Supply than a cautious player, not 60%.

**Preventing late-game runaway:** end-of-wave income growth is **linear** (+8/wave) while enemy effective HP growth is **superlinear** (Section 10.5). By wave 15, a fully built board is no longer able to simply add towers — the player must upgrade and reposition. The Logistics Depot branch is deliberately flat (+22/wave, not a percentage) so it never compounds.

### 7.3 Command Point economy

| Source | Rate |
|---|---|
| Base | +3 CP per wave completed |
| Command Post (base) | +2 CP per wave, per post (does not stack-cap — three posts give +6) |
| Command Post (Logistics branch) | +4 CP per wave |
| Cap | 12 CP |

CP spends: tactical abilities (Section 7.6) cost 3–6 CP. Signature towers consume CP to accelerate their charge (Section 8).

The cap at 12 is important: it forces the player to *spend* rather than bank, which is what makes abilities feel like part of the moment-to-moment loop rather than a saved-up nuke.

### 7.4 Tower costs and limits

| Archetype | Base cost | L2 | L3 | L4 | Total at L4 |
|---|---|---|---|---|---|
| T1 Automatic Gun | 100 | 65 | 115 | 210 | 490 |
| T2 Marksman Post | 175 | 114 | 201 | 368 | 858 |
| T3 Field Mortar | 150 | 98 | 173 | 315 | 736 |
| T4 Anti-Tank Gun | 200 | 130 | 230 | 420 | 980 |
| T5 Flak Battery | 175 | 114 | 201 | 368 | 858 |
| T6 Armored Emplacement | 300 | 195 | 345 | 630 | 1470 |
| T7 Heavy Artillery | 350 | 228 | 403 | 735 | 1716 |
| T8 Minefield | 90 | 59 | 104 | 189 | 442 |
| T9 Command Post | 225 | 146 | 259 | 473 | 1103 |
| Signature (all nations) | 650 | 423 | 748 | — (3 levels only) | 1821 |

**Tower limits:**
- **No global tower cap.** The build-pad count per map is the limit, and it is a designed one (Section 9 specifies pad counts).
- **Minefield limit:** maximum 6 fields on the map at once, enforced with a visible counter, because they are free-placement.
- **Signature limit:** **1 per map** on small/medium maps, **2** on the two largest launch maps. Enforced hard; the build button greys out with a clear reason.
- **Command Post:** unlimited, but auras do not stack, which self-limits.

### 7.5 Build pads and placement

- Every map authors **18–34 build pads** depending on size, visible as subtle terrain markers (sandbag rings, cleared earth, foundation slabs) that light up on hover and glow when the build menu is open.
- Pads have **tags**: `Standard`, `Elevated`, `Enclosed`, `Coastal`. Tags act as bonuses, never as hard restrictions except where noted:
  - **Elevated** — +15% range to all towers, +1.0 range to indirect-fire (T3, T7). Fewer per map; contested.
  - **Enclosed** (bunkers, ruins, cellars) — the tower on it is **immune to Siege suppression**. Typically poorly positioned; a real trade.
  - **Coastal** — allows the Coastal Artillery / Naval Gun national variants of T7 at −20% cost. Cosmetic-plus on non-coastal-themed nations.
- **Minefields are the exception:** placed directly on path segments, no pad required, subject to the 2.5-tile spacing rule and the 6-field cap.

**Why pads and not free placement:** free placement in a top-down path game degenerates into maze-building or wall-of-turrets, both of which destroy the map designer's ability to create meaningful topology and both of which are far harder to balance. Pads let each map be a designed puzzle. They also let Claude Code implement placement as "click pad → menu" rather than as a collision/validity solver, which is a real week of saved work.

### 7.6 Tactical abilities

Every nation has access to the **same three universal abilities**. Doctrines (Section 8.3) each add a **fourth, doctrine-specific ability**. This keeps the ability system to 3 + 18 = 21 data entries over roughly 6 code behaviors.

| Ability | Cost | Effect | Cooldown |
|---|---|---|---|
| **Artillery Strike** | 4 CP | 120 HE damage in a 3-tile radius at a clicked point, 1.5s delay | 20s |
| **Rally** | 3 CP | All towers in a 7-tile radius gain +50% rate of fire for 8s | 30s |
| **Emergency Repair** | 5 CP | Instantly clears Suppression from all towers and restores 3 Defense Line HP | 45s |

Abilities are on a hotbar (keys 1–4), each with a radial cooldown and a CP cost badge. Clicking with insufficient CP shows the shortfall rather than doing nothing silently.

### 7.7 Time controls

- **1× / 2× / 3×** speed, bound to a brass lever in the frame and to `Space` (cycle) and `+`/`-`. Number keys are abilities.
- **Pause (`P` or `Esc`-lite):** the game pauses but **building and upgrading remain fully available**. This is a deliberate accessibility and depth choice — it removes execution pressure while preserving planning pressure, and it makes the game playable by people who cannot click quickly.
- **Inter-wave build time:** 25 seconds standard, 40 seconds before an announced boss or air wave, 15 seconds on Elite difficulty.
- **Call wave early:** always available during build time, single button, shows the exact bonus Supply before you commit.

### 7.8 Defense Line (the "lives" system)

The player has **Defense Line Integrity**, displayed as a segmented bar of **20** (Recruit 30 / Regular 20 / Veteran 12 / Elite 8).

Leak costs by archetype:

| Enemy class | Cost |
|---|---|
| Swarm / Fast Infantry | 1 |
| Basic Infantry / Armored Infantry | 1 |
| Light Vehicle / Support / Recon | 2 |
| Medium Armor / Escort | 3 |
| Siege | 3 |
| Heavy Armor | 5 |
| Air | 2 |
| Boss | **Instant loss** |

A boss reaching the objective ends the mission regardless of remaining integrity. This makes the finale genuinely tense without requiring the boss to be a health sponge.

---

## 8. Nations, signature towers, and doctrines

### 8.1 The faction expression system

Each nation expresses itself through exactly five channels. No sixth channel is permitted.

1. **Naming and art.** All 9 archetypes get a national name, sprite, muzzle effect, projectile tint, and firing sound.
2. **Stat leaning.** Each nation applies a fixed modifier profile across the archetypes, within a **±15% envelope** on any single stat, with a **net power budget of 0**. Encoded as a `NationProfile` resource: a list of `(archetype, stat, multiplier)` rows. Balance is checked by a spreadsheet (and the in-editor Balance Dashboard, §15.6) that sums each nation's DPS-per-Supply across the roster and asserts all six land within ±3% of each other.
3. **Branch flavor names.** Upgrade branches keep universal mechanics but carry national names (the Soviet T7 Rocket Saturation branch is literally "Katyusha"). Zero code cost, high identity yield.
4. **One signature tower.** Genuinely unique code. Six behaviors total.
5. **Three doctrines.** Each = 1 passive modifier + 1 unique tactical ability. 18 data entries over ~6 shared ability behaviors.

**Absolutely not permitted as faction expression:** unique currencies, unique enemy types, unique win conditions, unique maps, unique UI, unique wave structures. [X]

### 8.2 Nation-by-nation specification

---

#### 8.2.1 UNITED STATES — *"Production wins wars"*

**Mechanical identity (3 principles):**
1. **Cheapest re-tooling.** US towers sell for **85%** instead of 75%, and US upgrade costs are 8% lower. The US player is rewarded for adapting mid-mission rather than committing early.
2. **Best economy.** The Logistics Depot branch generates +30 Supply/wave instead of +22.
3. **Generalist statline.** No archetype is a standout; nothing is a weakness. The baseline nation, and the recommended starting nation.

| Archetype | US name | Leaning | Scope |
|---|---|---|---|
| T1 Automatic Gun | **Browning MG Nest** | Baseline | [VS] |
| T2 Marksman Post | **Ranger Sniper Post** | +8% rate of fire, −5% range | [L] |
| T3 Field Mortar | **M3 Halftrack Turret** *(81mm mortar carriage)* | +10% rate of fire, −8% blast radius | [VS] |
| T4 Anti-Tank Gun | **Bazooka Squad** | −12% cost, +12% rate of fire, −15% range | [VS] |
| T5 Flak Battery | **M45 Quadmount AA** *(new name; the US list had no AA entry)* | +12% rate of fire, −8% damage | [L] |
| T6 Armored Emplacement | **Sherman Tank Emplacement** | Baseline; Heavy Turret branch = "Jumbo Assault Tank" | [L] |
| T7 Heavy Artillery | **105mm Howitzer Battery** | Precision branch leaning: −0.2s fire delay | [L] |
| T8 Minefield | **Combat Engineer Minefield** *(new name)* | +1 charge, −10% damage | [L] |
| T9 Command Post | **Jeep Recon Tower** — Forward Observer branch = **Naval Gunfire Spotter**; Logistics branch = **War Bonds Supply Depot** | +1.0 aura radius | [VS] |
| **Signature** | **Arsenal of Democracy Factory** | — | [VS] |
| *Deferred [P1]* | Mustang Air Support Beacon (→ T10) | — | [P1] |

**Signature: Arsenal of Democracy Factory**

| Property | Value |
|---|---|
| Cost | 650 / 423 / 748 (3 levels) |
| Placement | Build pad. **1 per map.** |
| Model | **Continuous production**, not charge-and-release. Produces one friendly unit every 14s (L1) / 10s (L2) / 7s (L3) |
| Produced units | L1: Rifle Squad (120 HP, 14 SA DPS). L2 unlocks Jeep (90 HP, 22 SA DPS, fast, 30% dodge). L3 unlocks Light Tank (280 HP, 30 AP DPS). Production cycles through unlocked types. |
| Unit behavior | Spawns at the factory, **walks the path backwards toward the enemy entry**, engages the first enemy it meets, blocks that enemy (enemies stop to fight, creating a stall), fights until destroyed. Lifetime cap 45s. Max 5 friendly units alive. |
| Range | The factory itself has no attack. Its "range" is the path. |
| Targeting | Units auto-engage nearest enemy on their path segment. No player control. **[X]** to unit micromanagement. |
| Upgrade path | 3 levels, no branch fork (its unit unlocks *are* the fork) |
| Counterplay against it | Enemy Siege units suppress the factory. Fast enemies bypass engaged friendlies (blocking is soft — a stalled enemy resumes after 3s if the friendly can't kill it). |
| Visual readability | Friendly units render with a **thick white outline and a small national flag pip**. Blocking is shown by both units halting with a small clash effect. Friendly HP bars are white; enemy HP bars are red. |
| Balancing risks | **(a)** Path-blocking can trivially stall a lane if unbounded — solved by the 3s soft-block release and the 5-unit cap. **(b)** Friendlies obscure enemies visually in dense fights — solved by rendering friendlies on a lower sort layer at 85% scale. **(c)** It can invalidate a whole lane on maps with a long single approach — solved by the 1-per-map limit and by making its produced DPS deliberately mediocre; its value is *time*, not damage. |

**Doctrines:** *Lend-Lease* (all towers cost −6%, Supply income +8%; ability: **Materiel Drop**, 4 CP, instantly refund 100% of one tower's cost) · *Airborne* (Command Posts grant +50% aura radius; ability: **Paradrop**, 5 CP, spawn 3 Rifle Squads at any point on the path) · *Combined Arms* (towers within 4 tiles of a different archetype gain +10% damage; ability: **Coordinated Fire**, 4 CP, all towers instantly fire).

---

#### 8.2.2 BRITAIN — *"Know before they arrive"*

**Mechanical identity:**
1. **Information supremacy.** British Command Posts reveal Concealed units at **double radius** and the Forward Observer branch marks **two** targets simultaneously.
2. **Precision over volume.** British direct-fire towers lean **+10% range, −8% rate of fire** across the board. Long sightlines are British territory.
3. **Best Suppression.** British Smoke Rounds (T3 branch) have +40% duration and +0.6 radius. Britain is the control nation.

| Archetype | British name | Leaning | Scope |
|---|---|---|---|
| T1 | **Vickers Machine Gun Nest** — Sustained Fire branch = **Bren Gun Squad** | +8% range, −6% rate of fire | [L] |
| T2 | **SAS Ambush Post** | +12% range, Overwatch branch is best-in-class | [L] |
| T3 | **Mortar Pit** | Smoke branch +40% duration | [L] |
| T4 | **17-Pounder Gun Position** *(new name; British list had no AT gun)* | +12% range, −8% rate of fire | [L] |
| T5 | **Bofors AA Platform** | +15% rate of fire, −10% damage | [L] |
| T6 | **Churchill Tank Bunker** | +10% range, −8% damage; Heavy Turret branch = "Churchill Crocodile Support" (HE conversion) | [L] |
| T7 | **Royal Artillery Battery** | Precision branch leaning; +8% range | [L] |
| T8 | **Royal Engineers Minefield** | +12% damage, arming delay 0.2s | [L] |
| T9 | **Radar Early Warning Tower** | +25% aura radius, double reveal radius | [L] |
| **Signature** | **RAF Scramble Command** | — | [L] |
| *Deferred [P1]* | Spitfire Air Command (→ T10) | — | [P1] |

**Signature: RAF Scramble Command**

| Property | Value |
|---|---|
| Cost | 650 / 423 / 748 |
| Placement | Build pad, **1 per map**, prefers Elevated (gains +1 sortie slot on Elevated pads) |
| Model | **Cooldown-based sortie system.** Holds 2 sortie charges (3 at L2, 4 at L3), regenerating one every 22s (18s/14s). |
| Activation | **Player-directed.** Click the tower, then click any point on the map. A flight of fighters strafes a **3-tile-wide corridor along the path for 8 tiles** centered on that point. |
| Damage | 3 strafing passes over 4s, each dealing 55 SA + 20 HE along the corridor. Against **Air targets**, the sortie instead intercepts: 200 AA damage to up to 3 air units in the area. |
| Range | Whole map |
| Cooldown/charge | See above. May be manually held; charges cap. |
| Upgrade path | 3 levels: +charges, +damage, and at L3 unlocks **auto-scramble** (spends a charge automatically if an air unit is within 6 tiles of the objective — a safety net, toggleable) |
| Counterplay | Charges are finite; a wave that arrives faster than 22s per charge overwhelms it. It cannot be everywhere. |
| Visual readability | A dashed corridor preview follows the cursor during targeting. Aircraft sprites enter from the map edge, visibly fly the corridor, and exit — the player sees exactly what area was hit. |
| Balancing risks | **(a)** Manual activation means skilled players extract far more value than casual players; mitigated by auto-scramble at L3 and by making the base sortie damage moderate rather than deleting. **(b)** It is a hard counter to air, which risks making T5 Flak redundant for British players — mitigated by making the interception mode consume a full charge for only 3 targets, so a real bomber wave still needs Flak. **(c)** Targeting UI must pause-friendly: sortie targeting works while paused, which is a deliberate accessibility win. |

**Doctrines:** *Desert Rats* (towers gain +10% range on maps with open terrain tags; ability: **Smoke Screen**, 3 CP, Suppress all enemies in a 5-tile radius for 6s) · *Bomber Command* (RAF Scramble charges regenerate 25% faster; ability: **Bombing Run**, 6 CP, heavy HE damage along a drawn line) · *Home Guard* (Defense Line Integrity +6; ability: **Dig In**, 4 CP, all towers immune to Suppression for 15s).

---

#### 8.2.3 SOVIET UNION — *"Quantity has a quality of its own"*

**Mechanical identity:**
1. **Cheap and numerous.** Soviet tower base costs are **−14%**, damage **−10%**. More pads filled, less per pad. The nation that fills a map.
2. **Saturation over precision.** Soviet indirect fire (T3, T7) gets **+20% blast radius, −12% accuracy** (larger scatter). Soviet artillery blankets.
3. **Late-mission spike.** The Katyusha Storm Battery charges across a whole mission and pays off in the finale. Soviet play front-loads breadth and back-loads a hammer.

| Archetype | Soviet name | Leaning | Scope |
|---|---|---|---|
| T1 | **Maxim MG Bunker** | −15% cost, −8% damage | [L] |
| T2 | **Siberian Sniper Nest** | +12% damage, −8% rate of fire | [L] |
| T3 | **Red Army Mortar Squad** | −12% cost, +20% blast, +scatter | [L] |
| T4 | **Anti-Tank Rifle Team** | −18% cost, −12% damage, +10% rate of fire | [L] |
| T5 | **DShK AA Mount** *(new name; Soviet list had no AA entry)* | −12% cost, −8% damage | [L] |
| T6 | **T-34 Defensive Turret** | −12% cost; Heavy Turret branch = "IS Heavy Platform" | [L] |
| T7 | **Field Artillery Battery** — Rocket Saturation branch = **Katyusha Rocket Truck** | +20% blast, +12% scatter | [L] |
| T8 | **Minefield Layer** | −15% cost, +1 charge | [L] |
| T9 | **Commissar Command Post** — Logistics branch = "Rear Echelon Depot" | +10% rate-of-fire aura, −10% range aura | [L] |
| **Signature** | **Katyusha Storm Battery** | — | [L] |
| *Deferred [P1]* | Armored Train Cannon (map-specific emplacement on rail maps) | — | [P1] |

**Signature: Katyusha Storm Battery**

| Property | Value |
|---|---|
| Cost | 650 / 423 / 748 |
| Placement | Build pad, **1 per map**. Requires a `Standard` or `Elevated` pad (too large for Enclosed). |
| Model | **Charge-and-release.** Accumulates 1 charge point per second, plus 3 per enemy killed anywhere on the map. Full charge at 240 points (L1) / 200 (L2) / 170 (L3). Player may spend **4 CP to instantly add 40 charge points.** |
| Release | Manual (recommended) or auto-fire at full charge (toggleable). Fires a **battlefield-wide barrage**: 24 rockets scattered across **the entire visible path network**, weighted toward the densest enemy concentrations. |
| Damage | 60 HE per rocket, 1.8-tile blast, 3.5s of impacts. Total theoretical 1440 HE, realistically ~600–900 landed on a full board. |
| Range | Unlimited (whole map) |
| Upgrade path | 3 levels: faster charge, +6 rockets per level, and at L3 the barrage additionally applies **Suppressed** to everything it hits |
| Counterplay | Long recharge means it is a once- or twice-per-mission tool. Poor against a single heavy target (HE vs Heavy is 0.40×). Cannot hit air. |
| Visual readability | A vertical charge meter on the tower and a persistent HUD badge. On release: a 1.2s wind-up (the iconic launch rails elevating, a rising audio cue), then a wide, clearly-telegraphed impact sequence with impact markers drawn 0.6s before each shell lands. |
| Balancing risks | **(a)** A single button that clears the board is unsatisfying for everyone else in the build — mitigated by making it explicitly bad against Heavy Armor and useless against Air, so it *cleans up*, it does not *win*. **(b)** Charge from kills means a strong board charges it faster, which is a snowball; mitigated by making the time-based component dominant (240s of pure time vs. ~30 kills). **(c)** Screen-wide effects at 3× speed can be visually overwhelming — the barrage forces speed to 1× for its duration, which is also a great dramatic beat. |

**Doctrines:** *Deep Battle* (Heavy Artillery towers cost −15% and gain +2 range; ability: **Rolling Barrage**, 5 CP, a moving HE line that sweeps along a chosen path segment over 6s) · *Scorched Earth* (Minefields gain +3 charges and +20% damage; ability: **Demolition**, 4 CP, detonate all minefields at once for 150% damage) · *Guards Rifles* (Automatic Guns gain +20% rate of fire; ability: **Urra!**, 3 CP, all towers gain +30% damage for 6s and immunity to Suppression).

---

#### 8.2.4 GERMANY — *"Efficiency is a weapon"*

**Mechanical identity:**
1. **Expensive and lethal.** German base costs **+12%**, damage **+14%**. Fewer, better towers. The nation that punishes bad placement and rewards good placement.
2. **Best anti-armor and best AA.** The Pak 40 and Flak 88 are the strongest variants of T4 and T5 in the game. This is Germany's flagship strength and is paid for entirely by cost.
3. **Cluster synergy.** The Blitzkrieg Command Post makes tightly grouped towers dramatically better, which pushes German play toward fortress nodes rather than distributed coverage — a genuinely different board shape.

| Archetype | German name | Leaning | Scope |
|---|---|---|---|
| T1 | **MG42 Bunker** — Suppressive Fire branch = **Sturmgrenadier Post** | +18% rate of fire, +12% cost | [L] |
| T2 | **Jäger Marksman Post** *(new name; the German list had no marksman entry)* | +10% damage, +10% cost | [L] |
| T3 | **Wehrmacht Mortar Team** | +12% rate of fire, +10% cost | [L] |
| T4 | **Pak 40 Anti-Tank Gun** | **+18% damage**, +12% cost, +6% range | [L] |
| T5 | **Flak 88 Cannon** | **+15% damage**, +10% cost; **best-in-class Dual Purpose branch** (+25% ground damage vs other nations') | [L] |
| T6 | **Panzer IV Turret** — Heavy Turret branch = **Tiger Tank Platform** | +12% damage, +15% cost | [L] |
| T7 | **Nebelwerfer Rocket Battery** | Rocket Saturation leaning; +10% damage, −6% blast | [L] |
| T8 | **Teller Mine Field** *(new name; German list had no minefield)* | +15% damage, −1 charge | [L] |
| T9 | **Radar Flak Tower** — Forward Observer branch is best-in-class for **Air** reveal | +12% cost, +10% aura strength | [L] |
| **Signature** | **Blitzkrieg Command Post** | — | [L] |
| *Deferred [P1]* | Stuka Dive-Bomb Beacon (→ T10) | — | [P1] |

**Signature: Blitzkrieg Command Post**

| Property | Value |
|---|---|
| Cost | 650 / 423 / 748 |
| Placement | Build pad, **1 per map** |
| Model | **Toggled overdrive with a duty cycle.** Not always-on. The player activates it; it runs for 12s, then must recharge for 30s. Recharge is reduced by 4s per 2 CP spent. |
| Effect while active | All towers within **7.5 tiles** gain: **+45% rate of fire**, **+60% projectile velocity** (matters enormously for T3/T7 lead prediction and for T4 hitting fast movers), **+70% turret traverse speed**, and **−0.3s indirect-fire delay**. |
| Passive (while recharging) | +8% rate of fire in radius — a small consolation so the tower is never dead weight |
| Upgrade path | L2: +1.5 radius, duration 15s. L3: recharge 22s, and **the first 3s of each activation also applies Spotted to all enemies in radius** |
| Counterplay | It is a *timing* tower. Activating it into a lull wastes it. Its radius forces clustering, which makes German boards vulnerable to multi-lane maps and to Siege suppression (one Siege shell can disable three clustered towers). |
| Visual readability | Active state is unmissable: a warm amber pulse across the radius ring, a distinct rising audio motif, and every buffed tower gets a small amber chevron above it. A large HUD button shows the duty cycle as a filling arc. |
| Balancing risks | **(a)** +45% rate of fire on a cluster of six L4 towers is a spike large enough to trivialize a wave — mitigated by the short duration and long recharge (a 28% uptime), and by the radius being small enough that six L4 towers is genuinely most of a German board's budget. **(b)** It rewards a single fortress, which can degenerate on single-lane maps — this is why 4 of the 8 launch maps have split or multi-entry topology. **(c)** The projectile-velocity buff is mechanically subtle; the tooltip must say plainly *"artillery and mortars will lead targets much better."* |

**Doctrines:** *Panzer Doctrine* (Armored Emplacements cost −12% and gain +1 range; ability: **Counterattack**, 5 CP, all Armored Emplacements immediately fire 3 shots) · *Fortress* (Enclosed pads grant +20% damage; ability: **Entrench**, 4 CP, towers gain +25% range for 10s) · *Kampfgruppe* (Blitzkrieg recharge −20%; ability: **Concentrated Fire**, 4 CP, all towers in radius target the same enemy for 6s).

---

#### 8.2.5 ITALY — *"Speed, position, disruption"*

**Mechanical identity:**
1. **Mobility.** Italian towers can be **relocated to another empty pad for 20% of their invested cost** (all other nations must sell at 75% and rebuild). This is Italy's headline mechanic and it is genuinely distinct: the Italian player rebuilds their board between waves in response to the wave preview.
2. **Cheap and fast.** Italian base costs **−10%**, upgrade costs **−10%**, damage **−8%**. Excellent early tempo, weakest raw late-game ceiling.
3. **Disruption.** Italian towers apply Suppression more readily: the T1 Suppressive branch, T3 Smoke branch, and T8 Concussion branch all get +25% effect duration.

*Design note on Italy:* Italy is the nation most at risk of being written as "the weak one," which the brief explicitly forbids and which would also be lazy. Italy is instead **the flexible one**. The relocation mechanic gives it a strategic tool no other nation has, and it is genuinely strong — a nation that can re-solve the board every wave is powerful in the hands of a planner. Its lower raw numbers are the price of that flexibility, not a national characterization.

| Archetype | Italian name | Leaning | Scope |
|---|---|---|---|
| T1 | **Breda Machine Gun Nest** — Sustained Fire branch = **Bersaglieri Rapid Fire Squad** | −10% cost, +10% rate of fire, −10% damage | [L] |
| T2 | **Alpini Mountain Rifle Post** | −10% cost; **+20% range on Elevated pads** (double the normal elevation bonus) | [L] |
| T3 | **Italian Mortar Pit** | −10% cost, Smoke branch +25% duration | [L] |
| T4 | **Anti-Tank Gun Battery** — Sabot branch = **Semovente Tank Destroyer** | −10% cost, −8% damage, +12% rate of fire | [L] |
| T5 | **Breda 20mm AA Mount** *(new name; Italian list had no AA entry)* | −10% cost, +12% rate of fire | [L] |
| T6 | **Carro Armato Turret** | −12% cost, −10% damage, relocation cost only 12% | [L] |
| T7 | **Coastal Artillery Gun** — Coastal-pad variant = **Naval Patrol Cannon** (−20% cost on Coastal pads) | **+22% range**, +10% cost, −8% damage | [L] |
| T8 | **Guastatori Minefield** *(new name; Italian list had no minefield)* | −10% cost, Concussion branch +25% duration | [L] |
| T9 | **Recon Motorcycle Outpost** | −10% cost, +15% aura radius, −8% aura strength | [L] |
| **Signature** | **Bersaglieri Charge Post** | — | [L] |

**Signature: Bersaglieri Charge Post**

| Property | Value |
|---|---|
| Cost | 650 / 423 / 748 |
| Placement | Build pad, **1 per map** |
| Model | **Burst deployment on a short cooldown**, contrasting with the US Factory's steady trickle. Every 18s (14s/11s), deploys a squad of **4 fast infantry** simultaneously. |
| Unit behavior | Squad sprints up the path (2.5× normal infantry speed), throws grenades at the first cluster it reaches (35 HE each, 1.4-tile blast), then engages in melee/close combat, **applying Suppressed to everything within 1.5 tiles for as long as they survive**. Lifetime 20s, or until killed. Max 8 friendly units alive. |
| Damage role | Low sustained damage (~18 SA DPS each). Its output is **disruption and time**, not kills. |
| Upgrade path | L2: 5 per squad, grenades gain +1 target. L3: squad additionally applies **Spotted** to everything it engages, and reveals Concealed units along its route. |
| Counterplay | Squads die fast to Medium/Heavy Armor. Their Suppression is the real payload, which means Italy pairs them with artillery — an obvious, teachable combo. |
| Visual readability | Same friendly-unit language as the US Factory: white outline, flag pip, 85% scale, lower sort layer. The sprint is visually distinct (dust trail). Grenade arcs are drawn. |
| Balancing risks | **(a)** Its Suppression uptime, combined with Italy's +25% Suppression duration leaning, could stack into permanent slow — hard-capped by making Suppression non-refreshing beyond 4s total from any source. **(b)** Eight friendly sprites plus a wave of enemies plus grenades is the densest visual moment in the game; the 85%-scale-and-outline rule and a hard particle budget must be tested here specifically. **(c)** It is the signature most likely to feel weak on paper; the tooltip and the post-mortem panel must explicitly credit "enemy time stalled: 34s." |

**Doctrines:** *Alpini* (all towers gain +12% range on Elevated pads; ability: **Mountain Position**, 4 CP, one tower gains +40% range for 12s) · *Celere* (relocation is free and instant; ability: **Redeploy**, 3 CP, instantly move any tower to any empty pad) · *Regia Marina* (Coastal pads grant +18% damage; ability: **Naval Bombardment**, 6 CP, a heavy HE strike along a line from the nearest map edge).

---

#### 8.2.6 JAPAN — *"Prepare the ground"*

**Mechanical identity:**
1. **Route denial specialist.** Japan's minefield cap is **9** instead of 6, its fields carry **+2 charges**, and it regenerates charges **40% faster**. Japan is the only nation whose traps are a primary damage source rather than a supplement.
2. **Concealment and ambush.** Japanese towers deal **+30% damage on their first shot at any newly acquired target**. This rewards spread placement across many small engagements rather than one killzone — the mechanical opposite of Germany.
3. **Charge-based burst.** The Special Attack Airfield stores discrete strikes rather than providing sustained output; Japan front-loads preparation and spends it in decisive moments.

| Archetype | Japanese name | Leaning | Scope |
|---|---|---|---|
| T1 | **Type 92 Machine Gun Nest** | +10% range, −8% rate of fire | [L] |
| T2 | **Sniper Hideout** | +15% first-shot bonus (stacks to +45% total), −8% rate of fire | [L] |
| T3 | **Imperial Mortar Squad** | Baseline, −6% cost | [L] |
| T4 | **Type 1 47mm AT Gun** *(new name; Japanese list had no AT gun)* | −8% damage, +12% rate of fire | [L] |
| T5 | **Type 96 AA Mount** *(new name; Japanese list had no AA entry)* | Baseline | [L] |
| T6 | **Type 97 Chi-Ha Turret** | −10% cost, −12% damage, +10% rate of fire | [L] |
| T7 | **Coastal Naval Gun** | +18% range, +8% cost | [L] |
| T8 | **Bamboo Spike Trap** — AT branch = **Anti-Tank Minefield** | **+2 charges, +40% regen, cap 9** | [L] |
| T9 | **Artillery Observation Tower** | +2.0 range to indirect-fire towers in aura (unique aura leaning) | [L] |
| **Signature** | **Special Attack Airfield** | — | [L] |
| *Deferred [P1]* | Assault Infantry Post (reworked "Banzai Infantry Post" — friendly-unit spawner) · Zero Fighter Beacon (→ T10) | — | [P1] |

**On naming:** the brief flags "Kamikaze Airfield" and "Banzai Infantry Post" for reconsideration. **Recommendation: rename both.**

- **"Kamikaze Airfield" → "Special Attack Airfield."** This is the neutral, historically-used administrative term, it avoids the loaded loanword, and mechanically it is presented purely as *a stored, one-use precision air strike* — aircraft enter from the map edge, dive, and the strike resolves as an explosion. No pilot is depicted, no self-destruction is dramatized, no glorifying language appears anywhere in the tooltip or the audio. The tooltip reads: *"Stores aircraft for a single high-intensity strike against a designated ground target."*
- **"Banzai Infantry Post" → "Assault Infantry Post"** (and it is deferred to [P1] regardless). The word carries a caricature history in Western war media that the brief explicitly asks us to avoid, and its mechanical content ("fast infantry that charges") is fully expressible without it.

Both renames cost nothing mechanically and remove real risk.

**Signature: Special Attack Airfield**

| Property | Value |
|---|---|
| Cost | 650 / 423 / 748 |
| Placement | Build pad, **1 per map**. Requires a `Standard` pad (needs a runway footprint). |
| Model | **Stored discrete charges.** Accumulates 1 aircraft every 40s (32s/25s), maximum **2 stored** (3 at L3). Spending **3 CP** adds 12s of progress. |
| Activation | Player clicks the tower, then clicks a ground target or point. One aircraft is consumed. |
| Effect | After a 2.0s approach (aircraft visibly enters from the map edge and flies to the point), delivers **420 damage in a 2.6-tile radius**, split as 60% AP / 40% HE — which makes it the only player tool in the game that hits *very* hard against both Heavy Armor and clustered soft targets simultaneously. |
| Range | Whole map. Ground targets only. |
| Upgrade path | L2: −8s charge time, +60 damage. L3: 3 stored charges, +0.6 blast radius, and the strike applies **Spotted** to survivors for 8s. |
| Counterplay | Two charges is two moments. Against a sustained grind it does nothing. Against air waves it does nothing. It is a boss-and-spike answer, and the player who spends both charges on wave 9 has nothing for the finale. |
| Visual readability | Stored charges shown as 1–3 aircraft icons on the tower and in the HUD. Targeting shows the blast radius circle. The 2.0s approach is fully visible with a dashed approach line, and a ground marker flashes at the impact point — enemies that walk out of it genuinely escape, which is a real skill element. |
| Balancing risks | **(a)** 420 damage is the largest single number in the game and must be tuned against boss HP specifically — the design target is that two full charges deal roughly 35% of a boss's health, never more. **(b)** The tone risk is entirely in presentation, addressed above; the strike must read as an air raid, not a death. **(c)** Because it is stored and manual, it is the tower most likely to be *forgotten* by casual players — the HUD shows an amber pulse on the charge icons when at max, and the post-mortem panel calls out unspent charges. |

**Doctrines:** *Island Defense* (Minefield cap +3 and traps cost −20%; ability: **Prepared Ground**, 4 CP, instantly refill all minefield charges) · *Bushido Engineering* — rename to *Fortified Line* (Enclosed pads grant +15% rate of fire and Suppression immunity extends 2 tiles; ability: **Hold Fast**, 4 CP, +6 Defense Line Integrity, once per mission) · *Naval Support* (Coastal pads grant +25% range; ability: **Offshore Barrage**, 5 CP, HE strike along a line from the map edge).

### 8.3 Doctrines summary

18 doctrines, 3 per nation, chosen in the pre-mission loadout screen alongside the 6 tower slots. Each = one passive + one ability. Abilities draw on **six shared code behaviors**: point-blast, line-blast, aura-buff, spawn-friendly, instant-refund/utility, and status-application. Every one of the 18 is a data row parameterizing one of those six.

**Scope note:** if Milestone 6 runs long, ship **2 doctrines per nation (12)** and release the third set as the first free update. This is the designated pressure valve.

---

## 9. Country selection, campaign structure, and unlocks

### 9.1 Selection flow

**Alliance first, then nation.** The flow is:

`Main Menu → Campaign → [Allies | Axis] → [3 nation cards] → Confirm → Campaign map`

Rationale: the alliance choice is the *fiction* choice (which side's story am I in, who am I fighting) and the nation choice is the *toolkit* choice. Presenting them together as six equal cards buries the second decision. Two clear steps also lets the alliance screen carry the framing and tone-setting copy (Section 14).

### 9.2 One campaign spine, two framings

There is **one** 12-mission campaign structure, used by both alliances:

| Act | Missions | Maps | Theme | Teaching goal |
|---|---|---|---|---|
| **I — The Line Holds** | 1–4 | Bocage Crossroads, Desert Supply Route, River Crossing, Ruined Town | Defensive footing | Archetypes, armor classes, the first air wave, the first Siege unit |
| **II — Contested Ground** | 5–8 | Industrial Rail Yard, Snowy Forest Pass, Coastal Fortification, Bocage Crossroads (night variant) | Escalation, split lanes | Escort/Support units, Concealed units, multi-lane pressure, signature tower mastery |
| **III — The Last Fronts** | 9–12 | Island Jungle Route, Desert Supply Route (storm variant), Snowy Forest Pass (counterattack variant), Coastal Fortification (finale) | Full combined arms | Every archetype, every enemy, all four bosses |

Note that four of the twelve missions **reuse a map with a different route configuration, different weather/lighting, different build-pad availability, and an entirely different wave script**. This is the highest-value content lever available to a solo developer: a variant mission costs roughly 15% of a new map and reads as ~70% as fresh, provided the route config genuinely changes (an entry point closed, a bridge destroyed opening a second lane, pads flooded out).

**Mirroring:** the same twelve missions exist for both alliances. What changes:
- Mission briefing text (two variants per mission, ~120 words each — 24 short texts total)
- Enemy pool (the opposing alliance's three nations' visual variants)
- Objective framing (hold the crossroads vs. hold the crossroads — genuinely symmetrical; we are not writing two stories)
- Map lighting/dressing preset (subtle: different faction banners on the friendly objective)

**This is the single most important scope decision in the document.** It converts "six campaigns" into "twelve missions plus a text layer."

### 9.3 Enemy faction composition

Enemy nationality is **cosmetic only**. The wave data specifies archetypes; a per-mission `EnemyThemeWeighting` asset assigns which of the three opposing nations' visual variants each spawn uses.

Weighting is thematic per map:
- Desert Supply Route (Axis enemies) → 55% Italian, 35% German, 10% Japanese
- Island Jungle Route (Axis enemies) → 80% Japanese, 20% German
- Snowy Forest Pass (Allied enemies) → 70% Soviet, 20% US, 10% British

This produces recognizable theater flavor with zero mechanical divergence. A "Panzer IV Tank" and a "Sherman Tank Column" are the same Medium Armor archetype with different sprites, different engine sounds, and different names.

### 9.4 Map access

**All nations can play all maps.** No nation-locked content. A player who wants to run Japan through a snowy European forest can, and the campaign framing (fictionalized theaters, Section 14.2) is written specifically to make that unremarkable rather than absurd.

### 9.5 Unlocks

**All six nations are available from the first launch of the game.** No nation is gated.

Rationale: this is a premium single-player game. Gating a third of the content behind hours of play in order to manufacture a progression curve is a free-to-play instinct that actively harms a $15 product. The player paid; the toolkit is theirs. The *campaign* provides progression through mission difficulty and through the star system.

What **is** gated, lightly:

| Content | Gate |
|---|---|
| Signature tower for a nation | Complete **1 mission** with that nation (Mission 1 on any difficulty). Effectively a tutorial gate. |
| Doctrines 2 and 3 for a nation | Complete 2 and 4 missions with that nation |
| Skirmish mode | Complete campaign Mission 3 |
| Endless mode | Complete campaign Mission 8 |
| Veteran difficulty | Complete any mission on Regular |
| Elite difficulty | Complete any mission on Veteran |
| Challenge modifiers | Earned via stars (Section 11) |
| Maps 9–12 in Skirmish | Complete the corresponding campaign mission |

### 9.6 Random Nation mode

**Yes, include it — in Skirmish only.** [L]

It is a checkbox on the skirmish setup screen. It rolls nation + doctrine + a random 6-tower loadout from that nation's 10. It costs almost nothing to implement, it is genuinely fun as a mastery challenge, and it supports three achievements. It is **not** offered in Campaign, where loadout intent matters.

---

## 10. Enemy system

### 10.1 Principles

- Enemies are defined by **archetype**, never by nationality. A nation is a sprite set, a name, and an audio bank.
- Every archetype has one **special mechanic** and no more. Two mechanics on one enemy is unreadable at 3× speed.
- Every archetype has an **unmistakable silhouette** at 48px. Silhouette is tested by rendering the sprite as a black shape and confirming it is identifiable.
- Enemies are **introduced one at a time**, each with a pause-the-game "New Threat" card the first time it ever appears, showing its silhouette, armor class, special mechanic, and a one-line counter hint. Shown once per profile, ever.
- **No stereotyping.** Mechanics map to equipment classes and military roles. A "Japanese Infantry Platoon" and a "US Rifle Squad" are the same Basic Infantry archetype with identical stats. Fast Infantry is *fast* because it is a motorcycle scout or a light recon patrol, not because of any national characteristic. This rule is absolute and applies to art, name, audio, and codex text.

### 10.2 Launch enemy roster [L] — 12 archetypes

Baseline values are for Regular difficulty at wave 1 equivalence. Actual HP scales per Section 10.5.

---

**E1 — Basic Infantry** · Armor: **Soft** · Speed: 1.6 t/s · HP: 55 · Leak: 1
- *Silhouette:* upright rifle squad, 3 figures in a loose file, distinct helmet shape per nation.
- *Special:* none. This is the control group.
- *Counters:* Automatic Gun (ideal), Field Mortar, Minefield, Armored Emplacement coax.
- *Wave role:* filler and pressure. Present in every wave.
- *Variants:* US Rifle Squad · British Infantry Section · Soviet Conscripts · Wehrmacht Infantry · Italian Riflemen · Japanese Infantry Platoon
- **[VS]**

**E2 — Fast Infantry / Scout** · Armor: **Soft** · Speed: **3.4 t/s** · HP: 40 · Leak: 1
- *Silhouette:* two figures, forward lean, motorcycle or bicycle in some national variants, dust trail.
- *Special:* **Sprint** — after taking any damage, +40% speed for 2s. Punishes chip damage and mortar reliance.
- *Counters:* Automatic Gun (hitscan, so speed doesn't help), Minefield, Suppression from any source, Armored Emplacement coax. **Field Mortar and Heavy Artillery are bad against these** — the shell arrives where they were.
- *Wave role:* early leak pressure; teaches that indirect fire has a travel time.
- *Variants:* US Ranger Unit · British Commando Team · Soviet Shock Troops (visual only) · German Stormtroopers · Italian Bersaglieri Scouts · Japanese Recon Patrol
- **[VS]**

**E3 — Swarm Infantry** · Armor: **Soft** · Speed: 1.9 t/s · HP: 26 · Leak: 1
- *Silhouette:* six small figures in a tight blob, deliberately reading as "a lot."
- *Special:* **Cohesion** — spawns in groups of 8–12 and moves as a cluster; individual units that fall behind speed up to rejoin.
- *Counters:* Field Mortar (ideal), Heavy Artillery, Minefield HE, Automatic Gun with Sustained Fire.
- *Wave role:* the AoE check. Punishes an all-single-target board.
- **[L]** (Milestone 4)

**E4 — Armored Infantry** · Armor: **Hardened** · Speed: 1.4 t/s · HP: 130 · Leak: 1
- *Silhouette:* bulkier figures, visible body armor plating and a shield-shaped pack, slower gait.
- *Special:* **Dug In** — while Suppressed, takes 30% less damage. A deliberate anti-synergy that teaches the player that Suppression is not universally good.
- *Counters:* Marksman Post (ideal), Field Mortar HE, Anti-Tank Gun, Armored Emplacement.
- *Wave role:* the first real armor lesson, before vehicles arrive. Introduced wave 5–6 of Mission 1.
- **[VS]**

**E5 — Light Vehicle** · Armor: **Hardened** · Speed: 2.6 t/s · HP: 180 · Leak: 2
- *Silhouette:* small wheeled body, distinctly non-tracked, low profile.
- *Special:* **Evasive** — 25% chance to ignore any single instance of Small Arms damage. Makes MG walls unreliable against it specifically.
- *Counters:* Anti-Tank Gun, Field Mortar, Minefield, Marksman Post.
- *Wave role:* the bridge between infantry and armor. First appearance mission 1, wave 8.
- *Variants:* Allied Supply Convoy · Italian Armored Car · German Kübel Patrol · Soviet Scout Car
- **[VS]**

**E6 — Medium Armor** · Armor: **Armored** · Speed: 1.5 t/s · HP: 520 · Leak: 3
- *Silhouette:* the classic tank shape — hull, turret, tracks. The most recognizable object in the game.
- *Special:* **Frontal Plate** — takes 20% less damage from towers positioned in front of it along its path; full damage from the sides and rear. Makes *placement angle* matter, communicated by a subtle directional shading on the tank and a tooltip line. (If playtesting shows this is unreadable, cut it — flagged as the most cuttable mechanic in the game.)
- *Counters:* Anti-Tank Gun (ideal), Armored Emplacement, Heavy Artillery Precision branch, AT Minefield.
- *Wave role:* the armor gate. Mission 1 wave 10.
- *Variants:* Sherman Tank Column · T-34 Assault Tank · Panzer IV Tank · Carro Armato M13
- **[VS]**

**E7 — Heavy Armor** · Armor: **Heavy** · Speed: 1.0 t/s · HP: 1400 · Leak: **5**
- *Silhouette:* visibly larger than Medium Armor (1.4× scale), wider tracks, longer gun, deeper engine rumble.
- *Special:* **Suppression Immune.** Nothing slows it. It simply arrives.
- *Counters:* Anti-Tank Gun with Sabot, Armored Emplacement Heavy Turret, Heavy Artillery Precision, AT Minefield, Special Attack Airfield. Note that HE is 0.40× and SA is 0.08× — this enemy is the hard test of whether the player built AP.
- *Wave role:* the mid-mission spike. Never appears more than 3 at once outside Act III.
- *Variants:* Tiger Heavy Tank · IS Heavy Tank · Churchill Assault Tank
- **[L]** (Milestone 4)

**E8 — Air Unit** · Armor: **Soft (bomber) / Hardened (heavy bomber)** · Speed: 2.2 t/s · HP: 300 · Leak: 2
- *Silhouette:* aircraft from above, wings horizontal, a moving shadow on the ground beneath it (the shadow is the readability trick — it is how the player tracks air against a busy background).
- *Special:* **Air Corridor** — ignores the ground path entirely, flying a straight line from an air entry marker to the objective. This is the single biggest reason maps must be designed with air routes in mind.
- *Counters:* Flak Battery (only), RAF Scramble Command interception, Flak 88 Dual Purpose.
- *Wave role:* the hard gate. Always pre-announced (10.3).
- *Variants:* RAF Bomber Formation · Luftwaffe Bomber Squadron · US Bomber Wing
- **[L]** (Milestone 5)

**E9 — Support / Repair Vehicle** · Armor: **Hardened** · Speed: 1.5 t/s · HP: 260 · Leak: 2
- *Silhouette:* boxy truck/half-track with a visible crane or tool rack, and a **bright green tether beam** to whatever it is repairing.
- *Special:* **Field Repair** — restores 4% max HP/sec to one damaged vehicle within 4 tiles. The tether beam is always drawn and is the single most important readability element in the enemy roster.
- *Counters:* Marksman Post Overwatch branch (auto-targets it — this is the branch's whole purpose), Heavy Artillery, any tower with Strongest priority manually set, minefields placed behind the front.
- *Wave role:* the "kill the right thing" lesson. Introduced Act II.
- **[L]** (Milestone 5)

**E10 — Escort / Shield Vehicle** · Armor: **Hardened** · Speed: 1.4 t/s · HP: 320 · Leak: 3
- *Silhouette:* half-track with visible raised armor screens; projects a **translucent hexagonal bubble** over allies within 3.5 tiles.
- *Special:* **Cover Screen** — allies inside the bubble gain a shared shield pool of 400 HP that absorbs damage before their own HP. The bubble has its own visible bar. Killing the Escort pops the bubble instantly, dropping any remaining shield.
- *Counters:* Marksman Post, Special Attack Airfield, focused AP fire, Heavy Artillery (the shield pool is shared, so AoE drains it fast).
- *Wave role:* forces target prioritization and makes burst damage valuable. Introduced Act II.
- **[L]** (Milestone 5)

**E11 — Recon / Concealed Unit** · Armor: **Soft** · Speed: 2.8 t/s · HP: 90 · Leak: 1
- *Silhouette:* lone crouched figure or light motorcycle, rendered at **45% opacity with a dashed outline** when concealed.
- *Special:* **Concealed** — untargetable by towers unless within range of a Command Post, an active Spotted source, or a British Radar tower. Fully visible to the player at all times; just not shootable. Also **grants nearby enemies +15% speed** while alive (a scout leading a column).
- *Counters:* Command Post (the reason it exists), Marksman Overwatch, Minefields (traps do not require targeting — this is a genuinely satisfying discovery), any Spotted source.
- *Wave role:* the support-tower lesson. Introduced Act II. **The Minefield interaction is deliberate and should be discoverable, not tutorialized.**
- **[L]** (Milestone 5)

**E12 — Siege / Artillery Unit** · Armor: **Hardened** · Speed: 0.9 t/s · HP: 480 · Leak: 3
- *Silhouette:* towed gun or self-propelled artillery piece with a distinctly long barrel and a visible elevation arc.
- *Special:* **Bombard** — halts at 11 tiles from the nearest tower and fires; the shell **Suppresses that tower for 6s** (disabled, red hatching, audible warning). Every 8s while alive. Does not damage or destroy. Towers on `Enclosed` pads are immune.
- *Counters:* Heavy Artillery (outranges it at 20 tiles — the designed answer), Marksman Post (11 tiles, exactly matches, creating a genuine duel), Special Attack Airfield, RAF Scramble, minefields placed at its likely halt positions.
- *Wave role:* the range-and-reach lesson, and the reason Heavy Artillery exists. Introduced Act I mission 4.
- **[L]** (Milestone 4)

### 10.3 Bosses [L] — 4

Bosses are readable, mechanically fair, and **never** just large health pools. Each boss tests a specific competency, arrives with a 40-second build window and a dedicated musical cue, and is defeated by understanding rather than by raw DPS. **A boss reaching the objective is an instant loss.**

---

**B1 — Breakthrough Panzer** (end of Act I, mission 4) · Armor: **Heavy** · HP 6,000 · Speed 0.9
- *Mechanic:* **Armor Skirts.** Begins the fight with a 2,000-point skirt layer that reduces all incoming damage by 50%. The skirt has its own bar and **is destroyed by Explosive damage at 3× rate**. Once broken, the boss takes full damage but gains +30% speed.
- *Test:* can you sequence damage types? HE first, then AP.
- *Adds:* 2× Basic Infantry every 12s.
- **[VS]** (simplified: skirt + adds only)

**B2 — Armored Column Command** (end of Act II, mission 8) · Armor: **Armored** · HP 4,500 · Speed 1.2
- *Mechanic:* **Convoy.** Arrives as a command vehicle escorted by 4× Medium Armor in a fixed formation. The command vehicle projects **+40% damage resistance and Suppression immunity to all escorts.** Killing the command vehicle first is much harder (it is at the rear) but instantly collapses the escorts to 50% HP.
- *Test:* target prioritization under pressure, and whether you built reach.
- **[L]**

**B3 — Bomber Wing** (mission 10) · Armor: **Hardened (Air)** · HP 3,200 across 3 aircraft that must all be killed
- *Mechanic:* **Formation.** Three bombers fly in a V. While all three are alive they share a 30% damage reduction. Each destroyed bomber removes the bonus and **slows the survivors by 20%** (formation discipline breaking). They fly a wide air corridor that crosses most of the map, so a single Flak cluster cannot cover it — the player must spread AA or use mobile answers.
- *Test:* did you build AA in depth rather than in one spot?
- *Anti-frustration:* announced from mission start, not mid-mission. The mission briefing says so explicitly.
- **[L]**

**B4 — Fortress Assault Group** (mission 12, the finale) · Multi-phase, HP 9,000 total
- *Phase 1 (Heavy):* a Heavy Armor spearhead with two Escort Vehicles. Shield bubbles must be popped.
- *Phase 2 (at 60% HP):* the spearhead halts and becomes a **Siege platform**, suppressing towers on an 6s rotation while releasing waves of Swarm and Fast Infantry. The player must clear adds while dealing with rolling tower blackouts.
- *Phase 3 (at 25% HP):* Suppression immunity, +50% speed, direct sprint for the objective, accompanied by a simultaneous 3-bomber air element.
- *Test:* everything. This is the exam.
- *Fairness rules:* each phase transition has a 3-second visible telegraph (halt, animation, audio sting, HUD banner). No phase introduces a mechanic the player has not seen earlier in the campaign.
- **[L]**

**Elite variants [L]:** three enemy archetypes get an "Elite" flavor at higher difficulties — Elite Medium Armor (+50% HP, gains the Frontal Plate at 35%), Elite Swarm (12→16 count), Elite Siege (13-tile range). Pure data, no new code.

### 10.4 Wave composition data structure

A mission's waves are authored as data, edited in a custom editor plugin (Section 15.6).

```
MissionDefinition
├─ mapId, alliance-neutral
├─ startingSupply, defenseLineHP, buildTimeSeconds
├─ enemyThemeWeighting  (which opposing nations' skins, by percentage)
├─ briefingTextAllies, briefingTextAxis
├─ starObjectives[3]
└─ waves[16..24]
     WaveDefinition
     ├─ waveNumber
     ├─ previewTags[]          // icons shown in the preview strip
     ├─ isAirWave, isBossWave  // drives announcements and build time
     ├─ buildTimeOverride
     ├─ earlyCallBonusMultiplier
     └─ groups[1..6]
          SpawnGroup
          ├─ enemyArchetypeId
          ├─ count
          ├─ spawnPointId       // which map entry
          ├─ startDelaySeconds  // relative to wave start
          ├─ intervalSeconds    // between individual spawns
          ├─ pathId             // which route, if the map has splits
          ├─ eliteFlag
          └─ hpMultiplierOverride (default 1.0 — use sparingly)
```

Wave difficulty is measured by a computed **Threat Value** = Σ(effective HP × leak cost weight) + air/siege/support penalties. The editor displays Threat Value per wave and graphs the mission curve, which makes tuning a visual exercise rather than a guessing game.

### 10.5 Difficulty and scaling

**Within a mission** — enemy HP scales as `baseHP × (1 + 0.055 × (waveNumber − 1))`. At wave 18 that is 1.94×. Superlinear feel comes from *composition* (more armor, more support) rather than from an aggressive exponent, which keeps the numbers predictable and the player's mental model intact.

**Across the campaign** — each act applies a flat multiplier: Act I ×1.00, Act II ×1.35, Act III ×1.80. Applied on top of wave scaling.

**Across difficulties:**

| Difficulty | Enemy HP | Enemy speed | Income | Defense Line | Build time | Notes |
|---|---|---|---|---|---|---|
| Recruit | ×0.75 | ×0.95 | ×1.20 | 30 | 35s | New Threat cards stay open longer; counter hints shown on all enemies |
| Regular | ×1.00 | ×1.00 | ×1.00 | 20 | 25s | The intended experience |
| Veteran | ×1.35 | ×1.05 | ×0.90 | 12 | 20s | Elite variants begin appearing |
| Elite | ×1.75 | ×1.12 | ×0.82 | 8 | 15s | Elite variants common; +1 wave of pressure in each act |

**No difficulty changes enemy behavior or introduces new mechanics.** Elite is the same game, tighter. This preserves learning transfer.

### 10.6 Pacing rules (enforced in the wave editor as warnings)

- **No wave exceeds 75 seconds** from first spawn to last expected kill against a reference build. The editor computes this estimate and flags violations.
- **No cleanup tail.** The last spawn group of a wave must not be a single slow high-HP unit. If a wave ends with Heavy Armor, it ends with the Heavy Armor plus something else that dies later.
- **Recovery waves.** Every third wave after wave 6 is a deliberately lighter wave (Threat Value ≤70% of the previous) so the player can build, breathe, and reposition. Tension needs troughs.
- **Spike telegraphing.** Any wave with Threat Value >1.5× the previous shows an amber warning banner in the preview.
- **Air announcement.** Air waves are announced **three waves in advance** with a persistent HUD indicator, plus a klaxon and banner at the wave itself.
- **Boss announcement.** Bosses are named in the mission briefing and get a 40-second build window.

### 10.7 Wave preview UI

A horizontal strip above the build bar showing the **next three waves** as cards:

- **Wave N+1** (next): full detail — archetype icons with counts, armor class icons, special-threat badges (air/siege/support/concealed), Threat Value bar, entry point indicators.
- **Wave N+2:** archetype icons only, no counts.
- **Wave N+3:** threat badges only (so a player sees "air is coming" but not the exact composition).

This is precisely the information-vs-spoiler balance the brief asks for: the player can *plan* three waves out and *react* one wave out, but never simply reads a script.

### 10.8 Path selection

- Each map has 1–3 **entry points** and 1 objective.
- Maps with splits define **named paths**; a spawn group is assigned a `pathId` in data.
- Where a path forks mid-route, the fork is resolved by a **fixed per-spawn assignment made at spawn time** (alternating or weighted, defined in the map asset). **Never random.** Deterministic paths mean a player can learn a map, which is essential for a game built on replay.
- Air units use separate **air corridors** (straight lines from air entry markers to the objective) authored per map.

---

## 11. Map and mission design

### 11.1 Launch map roster [L] — 8 maps

Firm recommendation: **8 maps**, of which 4 also serve a variant mission (Section 9.2), producing 12 campaign missions and 8 skirmish/endless maps.

Eight is the number where the art budget stays commissionable, each map can be individually tuned and playtested to a high standard, and the campaign still feels varied. Ten would mean two maps that never got a proper balance pass.

---

**M1 — Bocage Crossroads** *(Hedgerow farmland)*
- **Terrain:** patchwork fields, thick hedgerow walls, sunken lanes, a stone farmhouse objective.
- **Topology:** two entries merging into one lane at the crossroads, then a short run to the objective. 22 build pads.
- **Build space:** dense — pads at nearly every hedge corner. Deliberately forgiving.
- **Gimmick:** **Hedgerow sightlines.** Hedges block nothing mechanically, but pads *behind* hedges are tagged `Enclosed` (Siege-immune) at the cost of being 1–2 tiles further from the road. A clean, cheap trade taught by geometry.
- **Suits:** all archetypes; the tutorial map.
- **Art:** greens, ochres, overcast light. Moderate prop count.
- **Scope:** **[VS]** — this is the vertical slice map, built at final quality.
- **Variant (Mission 8, "Night Crossing"):** night lighting, the western entry is closed and a third northern entry opens, half the crossroad pads are unavailable (rubble), Concealed enemies are weighted heavily.

**M2 — Desert Supply Route**
- **Terrain:** open sand, rocky outcrops, a wadi, a fuel depot objective.
- **Topology:** one long serpentine road with three hairpins. Very few natural chokes. 26 pads, many on outcrops.
- **Gimmick:** **Open ground and elevated rock.** Six `Elevated` pads on outcrops with commanding range. Long sightlines make Marksman and Heavy Artillery excellent and short-range towers poor. The map teaches range as a resource.
- **Suits:** Britain, Italy (Alpini elevation leaning), long-range builds. Hostile to Automatic Gun spam.
- **Art:** the cheapest map to produce (few props, flat terrain, one palette).
- **Scope:** **[L]** · **Variant (Mission 10, "Sandstorm"):** periodic dust reduces all tower range by 25% for 20s on a 60s cycle, telegraphed by a visible sweeping haze.

**M3 — River Crossing**
- **Terrain:** wide river, two bridges, a pontoon ford, riverside village objective.
- **Topology:** two parallel lanes (the two bridges) that never merge, each with its own objective damage. 24 pads, with a central "island" cluster of 5 pads that reaches both lanes at extended range.
- **Gimmick:** **Bridge bottlenecks.** The bridges are 1 tile wide, forcing single-file movement. Minefields and area damage are extraordinary here; the central island is the map's strategic prize.
- **Suits:** artillery, mines, Katyusha Storm. Punishes single-lane fortresses.
- **Scope:** **[L]**

**M4 — Ruined Town**
- **Terrain:** shelled European town, rubble streets, a cathedral square objective.
- **Topology:** a grid with three viable routes that split and re-merge twice. Enemies distribute across all three. 30 pads (the most), many in building shells.
- **Gimmick:** **Enclosed density.** 12 of the 30 pads are `Enclosed` (Siege-immune building interiors) but have restricted range arcs — a tower inside a building has its range clipped by wall geometry, shown as a pie-slice rather than a circle. This is the only range-shape variation in the game and is worth the implementation cost precisely once, on one map.
- **Suits:** Germany (fortress clustering), close-range archetypes.
- **Scope:** **[L]** — the most expensive map to produce. Schedule it late.

**M5 — Industrial Rail Yard**
- **Terrain:** marshalling yard, rolling stock, cranes, a locomotive works objective.
- **Topology:** rail lines and a service road; two ground entries plus a **third entry that opens at wave 10** (a rail gate). 25 pads.
- **Gimmick:** **The wave-10 gate.** A telegraphed mid-mission topology change. The HUD shows a countdown on the gate from wave 7. This teaches players to hold Supply in reserve and is the cheapest possible "second act" for a map.
- **Suits:** flexible builds; punishes spending everything early.
- **Scope:** **[L]**

**M6 — Snowy Forest Pass**
- **Terrain:** conifer forest, snowbound road, a frozen logging camp objective.
- **Topology:** a single winding road with heavy tree cover on both sides. 20 pads (the fewest) — this is the "tight budget" map.
- **Gimmick:** **Forest cover.** Sections of road are marked as under canopy. Enemies in canopy sections are **Concealed** unless a Command Post covers them. This makes the support archetype mandatory rather than optional, on exactly one map, which is a legitimate and interesting constraint.
- **Suits:** Command Post builds, Britain (double reveal radius), traps.
- **Scope:** **[L]** · **Variant (Mission 11, "Counterattack"):** the road reverses direction — enemies enter from the former objective side — forcing the player to rebuild their mental model of a map they know.

**M7 — Coastal Fortification**
- **Terrain:** cliffs, bunkers, beach approaches, a gun battery objective.
- **Topology:** two beach approaches converging on a cliff road. 28 pads, including 8 `Coastal` pads on the cliff edge.
- **Gimmick:** **Coastal pads and the tide line.** Coastal pads give the Naval Gun / Coastal Artillery variants a cost break and +range. Additionally, the lower beach path is **tidal**: on a 90-second cycle it floods, closing that route entirely and pushing all enemies onto the upper road. Fully telegraphed by a visible water line and a HUD timer.
- **Suits:** Italy, Japan (naval-leaning T7), long-range.
- **Scope:** **[L]** — also the Act III finale map (Mission 12), with all pads available and both routes permanently open.

**M8 — Island Jungle Route**
- **Terrain:** dense jungle, a single mud track, a river ford, an airstrip objective.
- **Topology:** one main track with two short bypass loops that open and close. 23 pads.
- **Gimmick:** **Mud.** Marked track sections slow **vehicles only** by 40% (infantry unaffected). This creates natural killzones for anti-armor towers and produces a genuinely different threat ordering — infantry arrives first, armor arrives late and slow.
- **Suits:** Japan (traps), anti-armor concentration, layered defense.
- **Scope:** **[L]**

**Deferred maps [P1]:** Mountain Pass, Airfield Perimeter, Fjord Landing, Steppe Rail Junction. Named here so the post-launch roadmap is concrete.

### 11.2 Map gimmick philosophy

Every gimmick above is: **one boolean or one timer**, authored per map, affecting either pad tags, path availability, or a single stat multiplier. None requires a simulation.

| Gimmick | Implementation cost |
|---|---|
| Enclosed pads (Siege immunity) | A tag on the pad asset |
| Elevated pads (+range) | A tag on the pad asset |
| Coastal pads (cost/range for one archetype) | A tag on the pad asset |
| Bridge bottleneck | Path geometry only. **Zero code.** |
| Wave-10 gate | Enable a spawn point at wave N. Three lines. |
| Tide cycle | Enable/disable a path on a timer. ~20 lines. |
| Sandstorm | A global range multiplier on a timer. ~15 lines. |
| Mud | A speed multiplier on a path segment, filtered by unit class. ~25 lines. |
| Canopy concealment | A Concealed flag on a path segment. Reuses E11's system entirely. |
| Clipped range arcs (Ruined Town only) | The one genuinely expensive gimmick: ~1 day of work for a pie-slice range shape and a line-of-sight check. Justified because it defines one map's whole identity. |

**Explicitly out of scope [X]:** destructible terrain, dynamic pathfinding, weather affecting projectile physics, fog of war, day/night simulation, terrain deformation.

### 11.3 Star objectives

Each mission has three stars:

1. **Complete the mission** (any difficulty)
2. **Complete with ≥75% Defense Line Integrity remaining**
3. **A mission-specific objective** — e.g. "Complete without building more than 8 towers", "Destroy the Breakthrough Panzer before it reaches the crossroads", "Complete using no Anti-Tank Guns", "Complete on Veteran or higher"

36 stars total. Stars unlock challenge modifiers (Section 12).

---

## 12. Progression, replayability, and modes

### 12.1 Modes at launch

| Mode | Description | Scope |
|---|---|---|
| **Campaign** | 12 missions, alliance-framed, star objectives | [L] |
| **Skirmish** | Any launch map, any nation, any difficulty, optional modifiers, 20 waves | [L] |
| **Endless** | Any launch map, infinite scaling waves, score-based | [L] |
| Tutorial | Integrated into Mission 1, not separate | [L] |
| Daily/weekly challenges | **[X]** — requires live ops, a server, and ongoing curation. Wrong shape for a solo premium release. | [X] |

### 12.2 The firm answer on persistent meta-upgrades

**No persistent meta stat upgrades at launch. Not now, not later.** [X]

Reasoning: meta-progression in a tower defense game solves a problem this game does not have. It exists to smooth difficulty for players who would otherwise churn out of a free-to-play funnel, and it does so by making the *player's account* stronger rather than the player. In a premium single-player game, it actively damages the core loop — it makes early missions trivially easy on replay (destroying the replay value the brief asks for), it makes balance a function of an unknown progression state, and it turns difficulty tuning into a two-variable problem for a solo developer who can barely afford one.

**What replaces it:** four difficulties, ten challenge modifiers, 36 stars, 18 doctrines, six nations, and a Faction Mastery track that awards **cosmetics and titles only**.

### 12.3 Faction Mastery

Each nation has a 10-rank mastery track filled by playing that nation (XP from mission completion, scaled by difficulty and stars). Rewards are strictly cosmetic:

- Ranks 1–3: alternate tower color schemes for that nation
- Ranks 4–6: profile banners and insignia (fictionalized unit markings, Section 14)
- Ranks 7–9: alternate map-table dressing (unit counter styles, pen colors)
- Rank 10: a nation-specific title and a "Veteran Command" table frame

Six tracks × 10 ranks = 60 cosmetic entries. This is a palette-swap job, not an art job.

### 12.4 Challenge modifiers [L] — 10

Toggleable in Skirmish and, once unlocked, in Campaign replays. Each awards a score multiplier and feeds achievements.

| Modifier | Effect | Mult |
|---|---|---|
| Iron Discipline | Cannot sell towers | ×1.2 |
| Rationing | −30% starting Supply | ×1.2 |
| Forced March | Enemy speed +25% | ×1.3 |
| Thin Line | Defense Line Integrity 5 | ×1.4 |
| No Retreat | No pausing | ×1.15 |
| Limited Command | Command Points do not regenerate | ×1.25 |
| Heavy Assault | Double the Heavy Armor count in all waves | ×1.35 |
| Air Superiority | Double the air waves | ×1.3 |
| Blackout | Wave preview shows threat badges only | ×1.25 |
| Improvised | Random 4-tower loadout, re-rolled every 5 waves | ×1.5 |

### 12.5 Achievements [L] — 40

Distribution: 12 campaign progress · 12 nation-specific (win a mission with each nation; win with each nation's signature as your only tower above L2) · 8 mastery/challenge · 5 discovery (kill a Concealed unit with a minefield; destroy a boss with a single Special Attack strike; win without losing a single Defense Line point) · 3 completionist.

**No grind achievements.** [X] Nothing asking for 10,000 kills.

### 12.6 Cosmetics

Faction Mastery unlocks only (12.3). **No cosmetic store, no cosmetic currency, no microtransactions.** [X]

### 12.7 Leaderboards [P1-adjacent]

Steam leaderboards on Endless mode score, per map. Implemented if Steamworks integration completes early; **not a launch blocker**. Skirmish and Campaign have no leaderboards (modifier combinations make them uncomparable).

### 12.8 Save system

- One campaign save per alliance (two total), auto-saved after each mission.
- Mid-mission saving: **yes**, a single suspend-and-resume slot per mode. Missions are 12 minutes but people have lives.
- Steam Cloud sync for saves, settings, and input bindings.
- Save format: JSON, versioned, with a migration hook from day one. This is cheap insurance and expensive to retrofit.

### 12.9 The post-mortem panel [VS]

On every victory *and* defeat, before the results screen, a panel shows:

- **Leaks:** which enemies got through, by archetype and count
- **Damage dealt by type:** a four-bar chart (SA / HE / AP / AA) versus **damage needed by armor class** across the mission — the single clearest possible statement of "you built the wrong thing"
- **Your least effective tower:** by damage-per-Supply, named
- **Your most effective tower:** by damage-per-Supply, named
- **Unspent resources:** Supply left on the table, unspent Command Points, unused signature charges
- **One actionable suggestion**, generated from a small rules table (e.g. `IF leaked_armor_share > 0.4 AND ap_damage_share < 0.2 THEN suggest_AP`)

This panel is a **vertical slice requirement**, not a polish item. It is the game's teaching system, it is what makes losses feel fair, and it is the difference between a player who quits at mission 3 and a player who tries again. It is also cheap: it reads from combat stats the game already tracks.

---

## 13. UI/UX specification

### 13.1 Menu flow

```
Splash / Content Notice (skippable after first run)
  └─ Main Menu
       ├─ Campaign
       │    └─ Alliance Select [Allies | Axis]
       │         └─ Nation Select (3 cards)
       │              └─ Campaign Map (12 mission nodes, 3 acts)
       │                   └─ Mission Briefing
       │                        └─ Loadout (6 towers + 1 doctrine + difficulty)
       │                             └─ MISSION
       ├─ Skirmish
       │    └─ Map → Nation → Doctrine → Difficulty → Modifiers → Loadout → MISSION
       ├─ Endless
       │    └─ Map → Nation → Doctrine → Loadout → MISSION
       ├─ Codex  (towers, enemies, damage table, nations)
       ├─ Settings
       └─ Quit
```

Total clicks from launch to playing: **6**. This is a target, not an estimate.

### 13.2 Nation select screen

Three large cards. Each shows: national color and insignia (fictionalized), a one-line identity statement ("Production wins wars"), three bullet mechanical principles, the signature tower's art and name, and a difficulty-to-learn pip (1–3). Hovering a card previews its tower silhouettes. This screen must make the player *want* to try a nation they didn't plan to.

### 13.3 Loadout screen

Left: the nation's 10 towers as cards; the player drags or clicks 6 into build slots. Right: three doctrine cards, pick one. Bottom: difficulty selector and (skirmish only) modifier toggles. A "Recommended Loadout" button fills the slots sensibly for the selected mission — critical for new players and for anyone who does not want to think about it. A **warning banner** appears if the loadout has no AP source or no AA source and the mission contains armor or air ("This mission includes air units. You have no anti-air tower selected."). This warning is not a block; it is information.

### 13.4 In-mission HUD

The map-table frame (Section 3.4) carries all UI:

- **Top-left:** Supply (with a per-wave income projection), Command Points, Defense Line Integrity bar.
- **Top-center:** wave counter, wave preview strip (three cards, Section 10.7), the air-warning indicator.
- **Top-right:** speed control lever (1×/2×/3×), pause, settings.
- **Bottom-center:** build bar — six tower buttons with cost, hotkeys Q/W/E/R/T/Y, greyed when unaffordable with the shortfall shown.
- **Bottom-right:** ability hotbar (1–4) with radial cooldowns and CP costs.
- **Bottom-left:** "Call Wave Early" button during build time, showing the exact bonus.
- **On-map:** build pads glow when the build bar is active; range circles preview on hover before placement; the selected tower shows its range, its current target with a thin line, and a floating panel.

### 13.5 Tower inspection panel

Opens on click, anchored to the tower, dismissible with Esc or by clicking elsewhere. Contains:

- Name, level (1–4 pips), current branch if chosen
- Live stats: DPS, range, rate of fire, damage type badge
- "Strong vs / Weak vs" icon rows
- **Lifetime damage dealt** and **damage per Supply invested** (this number teaches more than any tutorial)
- Targeting priority dropdown
- Upgrade button (cost, and a diff preview: `Damage 45 → 62`)
- At L2, two branch cards side by side with plain-language descriptions
- Sell button with refund amount

### 13.6 Enemy display

- Health bar above each unit, appearing only when damaged (reduces clutter enormously).
- **Armor class icon** rendered as a small glyph on the health bar's left cap — always present when the bar is visible.
- Status icons (Suppressed, Spotted, Shielded) as small badges to the right.
- Shield pools render as a distinct blue segment on top of the health bar.
- Concealed units render at 45% opacity with a dashed outline.
- Air units cast a ground shadow.
- Hovering any enemy opens its full card (armor, speed, special, counters).

### 13.7 Speed, pause, and results

- Speed: `Space` cycles 1×/2×/3×; `+`/`-` step. Speed persists across waves and is saved per-profile.
- Pause: `P`. **Building and upgrading remain available while paused.** A "PAUSED — planning mode" banner makes this explicit.
- Pause menu: Resume · Restart Mission · Settings · Abandon Mission (with confirmation) · Quit to Menu.
- Victory: post-mortem panel → results (stars earned, mastery XP, unlocks) → Next Mission / Replay / Menu.
- Defeat: post-mortem panel → Retry (same loadout) / Change Loadout / Menu. **"Retry" must be the default focused button and must be one click.** Friction on retry is the single biggest cause of abandonment in this genre.

### 13.8 Settings

**Video:** resolution, windowed/borderless/fullscreen, VSync, frame cap, UI scale (75%–150%).
**Audio:** master, music, SFX, UI, radio chatter — five sliders. Subtitles on/off, subtitle size.
**Gameplay:** default game speed, auto-pause on wave complete, confirm-before-sell, targeting priority defaults, tutorial hints on/off, damage numbers on/off.
**Controls:** full rebinding via Godot's built-in Input Map / `InputEventAction` system, with a reset-to-default and a visible conflict warning.
**Accessibility:** see below.

### 13.9 Accessibility [L]

Scoped to what a solo developer can deliver *well*. Doing five things properly beats listing fifteen.

| Feature | Specification |
|---|---|
| **Colorblind safety** | No mechanic is ever communicated by color alone. Damage types have distinct **glyphs** (bullet, burst, chevron, wing) used everywhere the color appears. Armor classes have distinct **shield glyph shapes**. Friendly/enemy distinction is by **outline weight and shape**, not red/blue. Three colorblind palettes (protanopia, deuteranopia, tritanopia) adjust the accent set, applied via a shared `Theme` resource swap. |
| **Faction color handling** | Nations use distinct hues *and* distinct insignia shapes. Enemies always share one enemy accent regardless of nation, so "is this mine or theirs" is never a hue question. |
| **UI scale** | 75%–150%, continuous, applied to all `Control`-node UI via the root viewport's content scale factor. Tested at 150% on 1080p. |
| **Screen shake** | Slider 0–100%, default 60%. Applies to all camera shake. |
| **Visual effects intensity** | Three levels (Full / Reduced / Minimal). Minimal removes screen flashes, reduces particle counts by ~70%, disables camera shake entirely, and simplifies the Katyusha barrage to fewer, larger impacts. **Also functions as the low-spec performance mode**, which is why it earns its place twice. |
| **Photosensitivity** | No flashing above 3Hz anywhere in the game. Verified on the Katyusha barrage, the Blitzkrieg activation, and all explosion effects. |
| **Remappable controls** | Full, including mouse buttons. Keyboard-only play is supported end-to-end (pad selection by Tab-cycling focus between `Control` nodes). |
| **Subtitles** | All radio chatter and all announcements are subtitled, with a speaker label and a size setting. On by default. |
| **Pause-while-building** | Listed here as well as in gameplay because it is the game's most significant motor-accessibility feature. |
| **No timed-input requirements** | Nothing in the game requires a reaction faster than the build timer. Manual signature activation is fully usable while paused. |

### 13.10 Learnability target

**A player learns the entire interface in Mission 1.** The tutorial is integrated, contextual, and consists of eight pause-the-game prompts (place a tower, upgrade it, choose a branch, read the wave preview, use an ability, respond to armor, respond to air, call a wave early), each dismissible and each replayable from the Codex.

---

## 14. Tone, historical handling, and content policy

### 14.1 The governing principle

The game is a **stylized strategy game about defending positions**, presented as counters on a commander's map table. It is recognizably WWII through **equipment, terrain, architecture, doctrine, and sound** — never through ideology, atrocity, or iconography of hatred.

### 14.2 The fictionalized theater framing

The campaign is set in **"the Fronts"** — an abstracted, unnamed set of theaters that evoke real geography without claiming to be it. Bocage Crossroads is not Normandy; it is farmland with hedgerows. Missions are framed as *operations*, not as historical battles, and briefings avoid dates, real place names, real commanders, and real unit designations.

This framing does three jobs:

1. It lets any nation play any map without absurdity or false historical claim.
2. It removes any implication that the game is depicting, and therefore taking a position on, specific real events.
3. It is honest with the player. The opening notice says so plainly.

### 14.3 Hard content rules (absolute)

**Never present, anywhere in the game, in any asset, in any language file:**

- Nazi iconography of any kind: swastikas, SS runes, Party symbols, Party salutes, Party slogans.
- Real fascist party symbols, slogans, or leaders of any nation (including Italian fascist iconography and Imperial Japanese ideological symbolism).
- Any named real political figure, any real leader's likeness, voice, or quotation.
- The Holocaust, camps, genocide, ethnic cleansing, or any reference to them. This subject matter has no place in a tower defense game and no respectful treatment within this format. Its total absence is the correct handling.
- Civilians as targets, as units, as collateral, or as a mechanic.
- Depictions of wounds, blood, corpses, or dying. Destroyed units resolve into smoke, a wrecked chassis, or a dispersing token.
- War crimes, atrocities, occupation, resistance, prisoners, or reprisals as content or as mechanics.
- "Nazi" as a faction name, a unit name, a branding element, or in marketing copy. The playable power is **Germany**; the alliance is **the Axis**; the forces are **German forces**.
- National caricature in art, naming, audio, or codex text. No nation's units are cowardly, fanatical, primitive, or comic.

**Insignia policy:** all national markings are **fictionalized**. Germany uses a plain dark cross with a distinct geometric border invented for this game. Japan uses an abstract solar disc variant with a different ray count and framing than any historical marking. The USSR uses an abstract red star with a distinct inner geometry. Italy, Britain, and the US use similarly abstracted roundels and stars. Each is instantly readable as "that nation's side" and is not a real emblem. A single art brief document specifies all six and is given to every commissioned artist.

**Audio policy:** radio chatter is short, generic, tactical, and **fully invented**. Lines are operational only ("Contact, north road." / "Armor spotted, request anti-tank." / "Position holding."). Non-English nations use a small set of short authentic-language operational phrases recorded by native speakers where budget allows; if not, non-verbal radio squelch and beeps are used instead. **No historical speech, no propaganda, no war cries, no slogans.** Everything is subtitled.

### 14.4 Storefront and opening framing

**Recommended Steam store copy paragraph (place near the top of the description):**

> *Fronts of War is a stylized tower defense game set in a fictionalized Second World War. It is presented as a strategy game on a commander's map table, using invented unit markings and abstracted battlefield pieces. It does not depict, endorse, or represent the ideologies, symbols, or atrocities of the period.*

**Recommended in-game opening notice (shown once, skippable thereafter, also available in the Codex):**

> *Fronts of War is a work of fiction inspired by the Second World War. Its theaters, operations, and unit markings are invented. It portrays the war as a strategy game of positions and equipment, and deliberately does not depict the ideologies, the political movements, or the human suffering of the period. Those subjects deserve better than a game like this one, and we have left them out on purpose.*

That last sentence is doing real work. It is honest, it is disarming, and it signals to a reviewer or a journalist in one line that the omissions are considered rather than careless.

**Steam maturity settings:** declare Violence (mild, non-graphic) and Historical/Wartime Themes. Do not declare Sexual Content, Gore, or Adult Content. Expect no restriction in any market other than possible additional review in Germany — the fictionalized insignia policy is specifically designed to clear German requirements without a separate build, but budget one week for an age-rating (USK/PEGI) submission if you intend to sell in Germany with confidence.

### 14.5 Preserving WWII flavor without offensive imagery

The recognizability comes entirely from these channels, and they are more than sufficient:

- **Silhouettes of equipment.** A Sherman, a T-34, a Tiger, and a Chi-Ha are instantly distinguishable from above. This alone carries most of the theming.
- **Architecture and terrain.** Hedgerows, a shelled cathedral square, a Pacific airstrip, a marshalling yard, a desert wadi. Place does enormous work.
- **Equipment class vocabulary.** Bofors, Pak, Katyusha, Bren, Breda, Type 92 — the *names of guns* are period-authentic, uncontroversial, and richly evocative.
- **Doctrine-inspired mechanics.** German cluster synergy, Soviet saturation artillery, American production, British reconnaissance, Italian mobility, Japanese prepared positions. These are readings of real military doctrine, expressed as verbs.
- **Sound.** An MG42's rate of fire, a Katyusha launch, a radial engine, a field telephone, a bolt-action report. Audio is the cheapest and most powerful authenticity channel in the entire project and should receive proportionally more budget than its line-item suggests.
- **Presentation furniture.** Teletype fonts, grease pencil, map pins, paper mission slips, a brass table lamp.

---

## 15. Technical architecture (Godot 4.x / C#)

### 15.1 Principles

1. **Data over code.** Every tower, enemy, wave, map, nation, and doctrine is a Godot `Resource`. Adding content never requires a new script.
2. **One tuning surface.** All global constants live in `GameBalanceConfig`, a singleton `Resource`. No magic numbers in behavior scripts, ever.
3. **Low coupling.** Systems communicate through a typed event bus (a C# singleton autoload) and through interfaces, not through direct references. A tower does not know what a wave is.
4. **Determinism where it matters.** Fixed-timestep simulation at 60Hz, decoupled from rendering (driven by our own `GameLoop` autoload, not `_physics_process` directly, so game-speed multipliers stay exact). No `GD.Randf`/`System.Random` without a seeded, per-mission RNG instance. Replays and reliable balance testing both depend on this.
5. **Pooling by default.** Every projectile, effect, damage number, and enemy comes from a pool of pre-instantiated scenes, never `PackedScene.Instantiate()` mid-wave.
6. **Small scripts.** Target: no gameplay file over 300 lines. If one grows past that, it is doing two jobs.
7. **Text-native, agent-editable content.** Because `.tscn`/`.tres`/`.cs`/`.gd` are all plain text, an AI coding agent should be able to author or modify any tower, enemy, map, or UI screen entirely through ordinary file edits. Live-editor MCP tooling (§3.2) is a convenience for inspection, screenshots, and headless test runs — never a hard dependency for making a change.

### 15.2 Project structure

```
/godot-project
  project.godot
  /addons
    /wave_editor          (EditorPlugin: wave timeline + Threat Value graph)
    /balance_dashboard     (EditorPlugin: DPS-per-Supply parity checker)
    /map_pad_tool          (EditorPlugin: build-pad placement/tagging, path splines)
    /data_validator        (EditorPlugin menu command: broken-reference scan)
  /assets
    /art            /sprites /vfx /ui /fonts
    /audio          /music /sfx /radio /banks
    /data
      /towers       (54 archetype variants + 6 signatures, foldered by nation, as .tres Resources)
      /enemies      (12 archetypes + national skin refs + 3 elites + 4 bosses)
      /nations      (6 NationProfile .tres assets)
      /doctrines    (18)
      /missions     (12 + skirmish presets)
      /maps         (8 MapDefinition .tres assets)
      /config       (GameBalanceConfig, DamageTable, DifficultyProfiles)
  /scenes         /towers /enemies /projectiles /vfx /ui
  /scenes_root    boot.tscn, main_menu.tscn, mission.tscn (one reusable mission scene)
  /src   (C#, one .csproj at project root; organized by namespace, not by asmdef)
    /Core         GameLoop.cs, TimeController.cs, EventBus.cs, ObjectPool.cs, SeededRandom.cs, SaveSystem.cs
    /Combat       DamageResolver.cs, ArmorTable.cs, StatusController.cs, TargetingService.cs, ProjectileSystem.cs
    /Towers       TowerController.cs, TowerUpgradeController.cs, TowerTargeting.cs, /Behaviors, /Signatures
    /Enemies      EnemyController.cs, PathFollower.cs, /EnemyAbility, BossPhaseController.cs
    /Waves        WaveRunner.cs, SpawnDirector.cs, ThreatValueCalculator.cs
    /Economy      SupplyLedger.cs, CommandPointLedger.cs, BountyTable.cs
    /Map          MapRuntime.cs, BuildPad.cs, PathNetwork.cs, /MapGimmick
    /Meta         ProgressionService.cs, UnlockService.cs, MasteryService.cs, AchievementService.cs
    /UI           /Hud /Menus /Panels /Tooltips
    /Platform     SteamService.cs (wrapped behind IPlatformService)
    /Debug        DebugConsole.cs, WaveSkipper.cs, StatOverlay.cs
  /tests          (GUT or GoDotTest suites, mirroring /src)
```

**Version control:** Git with Git LFS for all binary assets (`*.png`, `*.wav`, `*.ogg`, `*.psd`, `*.ttf`). Godot's own `.tscn`/`.tres`/`.import` files are plain text by default (Godot 4 does not require an explicit "force text serialization" setting the way Unity does — text is the native format), so LFS pressure is limited to genuinely binary assets. A `.gitignore` for `.godot/` (the local import cache), `export/`, and OS cruft. Commit `.import` files only where the project convention requires it (generally not — `.godot/` is regenerated).

**C# project layout note:** Godot 4's C# support centers on a single `.csproj` per project rather than Unity-style assembly definitions. Namespace-per-folder (`FrontsOfWar.Core`, `FrontsOfWar.Combat`, etc.) gives the same logical separation and low coupling without needing multiple compiled assemblies; if compile-time isolation between systems becomes valuable later, folders can be split into referenced class-library projects, but this is not a day-one requirement.

### 15.3 Core data models

Godot's `[GlobalClass]`-annotated `Resource` subclasses are the direct analog of Unity's `[CreateAssetMenu] ScriptableObject`: they appear in the editor's "Create New Resource" picker, serialize to `.tres`, and can be referenced by other resources or nodes via exported fields.

```csharp
// One archetype behavior + one data asset per national variant.
[GlobalClass] public partial class TowerDefinition : Resource {
    [Export] public string Id;                     // "us_t1_browning_mg"
    [Export] public TowerArchetype Archetype;       // enum: AutomaticGun, Marksman, ...
    [Export] public NationId Nation;
    [Export] public string DisplayNameKey;          // localization key
    [Export] public int BaseCost;
    [Export] public TowerStatBlock[] Levels;         // 4 entries (3 for signatures)
    [Export] public BranchDefinition BranchA, BranchB;
    [Export] public DamageType DamageType;
    [Export] public TargetingProfile DefaultTargeting;
    [Export] public TowerVisuals Visuals;            // sprite, muzzle vfx, projectile scene, sfx bank
    [Export] public PadTag[] AllowedPads;            // usually all
    [Export] public int MaxPerMap;                   // 0 = unlimited, 1 for signatures
}

[GlobalClass] public partial class EnemyDefinition : Resource {
    [Export] public string Id;                      // "e6_medium_armor"
    [Export] public EnemyArchetype Archetype;
    [Export] public ArmorClass ArmorClass;
    [Export] public bool IsAir;
    [Export] public float BaseHP, MoveSpeed;
    [Export] public int LeakCost, Bounty;
    [Export] public EnemyAbilityDefinition SpecialAbility;   // null for E1
    [Export] public NationalSkin[] Skins;            // 6 entries: sprite set + audio bank + name
}

[GlobalClass] public partial class NationProfile : Resource {
    [Export] public NationId Id;
    [Export] public AllianceId Alliance;
    [Export] public string IdentityLineKey;
    [Export] public StatLean[] Leanings;             // (archetype, stat, multiplier)
    [Export] public float SellRefundOverride;        // US = 0.85
    [Export] public float RelocationCostFraction;    // Italy = 0.20, others = -1 (disabled)
    [Export] public TowerDefinition[] Roster;         // 10
    [Export] public DoctrineDefinition[] Doctrines;   // 3
    [Export] public NationVisuals Visuals;
}
```

Damage resolution is a single pure static method with no side effects, which makes it unit-testable and engine-agnostic (it takes and returns plain values, no `Node` or `Resource` reference required):

```csharp
static float ResolveDamage(float baseDamage, DamageType type, ArmorClass armor,
                           bool isSpotted, DamageTable table)
    => baseDamage * table.Multiplier(type, armor) * (isSpotted ? 1.25f : 1f);
```

### 15.4 Simulation and performance

- **Fixed 60Hz simulation tick** driven by a single `GameLoop` autoload that owns the update order: Time → Spawns → Movement → Targeting → Firing → Projectiles → Damage → Status → Cleanup → UI. Deterministic order eliminates an entire category of "why did that behave differently" bugs. `GameLoop` runs off `_PhysicsProcess` (Godot's fixed-step callback) rather than `_Process`, so it stays decoupled from render framerate the same way the original Unity plan intended.
- Game speed multiplies **ticks per physics frame** (1/2/3), not Godot's global `Engine.TimeScale`. This keeps our own fixed-order simulation exact at all speeds and avoids `Engine.TimeScale` side effects on unrelated engine systems (audio pitch, physics substeps) that we do not want tied to gameplay speed.
- **Targeting is the hot path.** Use a uniform spatial grid (4-tile cells), rebuilt once per tick in plain C# (a `Dictionary<(int,int), List<EnemyController>>` or similar), and have towers query only the 9 cells within range. Never `Area2D`/`PhysicsServer2D` overlap queries per tower per frame — Godot's physics server is general-purpose and not tuned for hundreds of per-frame range queries; a custom grid is both faster and fully deterministic.
- **Performance budget:** 200 simultaneous enemies + 40 towers + 300 projectiles at a stable 60fps on a 2018-era integrated GPU laptop. Test on the worst machine you own, monthly.
- Sprite batching: all unit sprites on a small number of texture atlases (`AtlasTexture`/`SpriteFrames`), minimizing material/texture switches so Godot's 2D batcher can merge draw calls.

### 15.5 Steam integration

Wrapped behind `IPlatformService` with a `NullPlatformService` used in the editor and in early builds, so **Steam is never a dependency of the game running**. Implemented at Milestone 8, using **GodotSteam** (the actively maintained GDExtension/module wrapping the Steamworks SDK for Godot — the direct equivalent of Unity's Steamworks.NET in this stack):

- Achievements (40)
- Cloud saves
- Rich presence (optional)
- Leaderboards (Endless, optional)

### 15.6 Editor tooling (build these — they pay for themselves)

Godot's built-in 2D scene editor already covers a meaningful slice of what the original Unity plan needed custom `EditorWindow` tools for (dragging nodes, setting exported properties in the Inspector, previewing scenes) — because build pads, paths, and map dressing are just nodes in an ordinary scene. The custom tooling below is scoped to what genuinely has no native equivalent.

1. **Wave Editor plugin** (`EditorPlugin` with a `Control`-based dock; the timeline itself built on Godot's `GraphEdit`/`GraphNode` nodes, which are a strong native fit for "spawn groups as blocks on a time axis"). Shows the Threat Value graph, pacing-rule warnings, and a one-click "playtest from wave N." This tool is worth roughly three weeks of saved balancing time and should be built at Milestone 5, not later.
2. **Balance Dashboard plugin** (`EditorPlugin` dock). Reads all `TowerDefinition` resources and computes DPS-per-Supply per damage type per nation, asserts the ±3% national parity rule, and lists every violated hard floor (e.g. artillery fire delay). Run it before every content commit.
3. **Map Pad Tool.** A thin `EditorPlugin` (or, where sufficient, just well-authored `Marker2D`/`Area2D` scenes with an `@tool`-script gizmo) for placing build pads, tagging them, drawing paths as `Path2D` splines, and setting air corridors directly in the normal 2D viewport.
4. **Data Validator.** An `EditorPlugin` menu command — and, importantly, also a **headless CLI entry point** (`godot --headless --script res://tools/validate.cs` or a `Godot.SceneTree`-driven equivalent) — that walks all `Resource` assets and reports missing references, duplicate IDs, out-of-envelope stat leanings, and enemies with no listed counters. The headless path matters specifically because it lets a validation pass run in a GUI-less remote agent session or a CI job, not only inside an open editor window.

### 15.7 Testing

- **Unit tests** for: damage resolution, the armor table, bounty math, upgrade cost math, Threat Value calculation, save migration. These are pure C# functions with no `Node`/`Resource` dependency; run them with **GoDotTest** or **GUT (Godot Unit Testing)**, both of which support running headless from the command line (`godot --headless -s ...`) — again, no display required, which matters for CI and for remote agent sessions. There is no excuse not to test them.
- **Play mode smoke test:** a headless scene run via `godot --headless` that runs every mission at 20× with a scripted reference build and asserts completion without exceptions/errors in the log. Runs before every commit that touches combat.
- **Manual playtest cadence:** every mission, every difficulty, once per milestone. Log completion time and Defense Line remaining into a spreadsheet.

---

## 16. Art and audio production

### 16.1 Visual direction summary

Top-down 2D. Painted terrain. Clean, high-contrast sprite units viewed from directly above, free-rotating. Framed by a painted commander's map table.

### 16.2 Unit visual language

The rules that make 200 units on screen readable:

| Rule | Specification |
|---|---|
| **Size hierarchy** | Infantry 32px · Light Vehicle 44px · Medium Armor 56px · Heavy Armor 76px · Air 88px (plus shadow). Size is the primary threat cue and is respected absolutely. |
| **Silhouette test** | Every unit must be identifiable as a solid black shape at its native size. Tested for every sprite before acceptance. |
| **Friend/foe** | Player-side units (from signature towers) have a **2px white outline** and render at 85% scale on a lower `CanvasItem` z-index. Enemies have a **1px dark outline**. Never distinguished by color alone. |
| **Armor class cue** | Soft units read as small and loose. Hardened units have visible plating shapes. Armored units are tracked and turreted. Heavy units are visibly oversized with a longer gun. Shape carries armor class before any icon does. |
| **Nationality cue** | Palette (three-color ramp per nation) plus a small fictionalized insignia pip. Nationality is deliberately the *weakest* visual signal, because it is the least mechanically relevant. |
| **Motion** | Infantry: 4-frame walk cycle. Vehicles: rotating wheels/tracks (2-frame) plus body rotation. Air: none needed beyond a propeller blur. Total animation workload is very small — this is the top-down dividend. All driven by Godot's `AnimatedSprite2D`/`SpriteFrames`. |
| **Death** | Infantry disperse into a puff and a dropped token. Vehicles become a static wreck sprite that fades over 3s. No gore. Wrecks do not block. |

### 16.3 Tower visual states

Each tower needs **two art states**, not four: L1–L2 share a base sprite (with a small level pip); L3–L4 share a branch-specific sprite. Six nations × 9 archetypes × 2 branch sprites + 1 base = **~162 tower sprites**, which sounds large until you account for the fact that within a nation the base emplacement (sandbags, concrete, dugout) is shared and only the weapon changes. Realistic unique-art count: **~70 pieces plus recolors.**

### 16.4 Asset sourcing plan

| Category | Approach | Est. cost |
|---|---|---|
| Terrain tiles & props (8 maps) | Commission one artist for a consistent set; ~6 tilesets with shared props | $3,000–5,000 |
| Unit sprites (enemies, 12 archetypes × 6 nations, top-down) | Commission; heavy reuse via palette and part-swapping | $2,000–3,500 |
| Tower sprites (~70 unique + recolors) | Commission, same artist as units | $2,500–4,000 |
| UI / map-table frame | Commission a single cohesive UI kit | $1,000–1,800 |
| VFX | Asset store particle packs (Godot-compatible `GPUParticles2D` presets or portable sprite-sheet FX) + custom tuning | $150 |
| Music (8–10 tracks: menu, three act themes, boss, victory, defeat, ambient) | Commission or license from a stock library | $800–2,500 |
| SFX (weapons, engines, impacts, UI) | Licensed libraries (Soundly / A Sound Effect packs) | $300–600 |
| Radio chatter VO | Optional; non-verbal fallback is viable | $0–1,200 |
| **Total** | | **$9,750–18,750** |

**Prototype art strategy:** build the entire vertical slice with **colored geometric primitives plus text labels** (Godot `Polygon2D`/`ColorRect` placeholders plus a `Label`). Squares are infantry, hexagons are vehicles, size means armor class. This is genuinely playable, it forces the readability systems to work without art carrying them, and it makes the art swap a pure data change (every sprite reference lives in `TowerVisuals` / `NationalSkin`). Do not commission a single asset until the vertical slice is fun.

### 16.5 Audio direction

- **Music:** restrained, orchestral-with-percussion, low melodic profile so it survives 40 replays. Layered intensity: a base bed that adds a percussion layer during waves and a brass layer during boss phases. One track per act plus a boss track is enough; the layering does the rest. Implemented with Godot's `AudioStreamPlayer` bus layering (separate buses per layer, crossfaded via bus volume) — no third-party audio middleware needed.
- **SFX priority order** (where the authenticity budget goes): tower firing sounds (the player hears these ten thousand times — they must be distinct per archetype *and* per nation), impact/armor feedback (the ricochet ping for ineffective damage is a **gameplay system**, not a sound), vehicle engines, then everything else.
- **Ineffective-damage audio is a design feature.** A distinctly weak, high, metallic ping when Small Arms hits Armored is the fastest teaching signal in the entire game.
- **Mixing:** hard voice-count limits per sound category with distance-based culling, or wave 18 becomes noise. Budget: 4 concurrent instances per weapon type, ducked. Godot's audio bus system with per-bus limiter/ducking effects handles this natively.
- **Radio chatter:** 20–30 short lines per alliance, triggered on events (wave start, air incoming, boss phase, leak, low integrity, victory). Generic and operational only (14.3). Always subtitled.

---

## 17. Production plan

### 17.1 Milestones

| # | Milestone | Duration | Exit criteria |
|---|---|---|---|
| **M0** | Foundation | 2 wks | Project structure, Git+LFS, `GameLoop` autoload with fixed tick, `EventBus`, `ObjectPool`, `GameBalanceConfig`, `SeededRandom`, damage resolver with unit tests |
| **M1** | Core loop grey-box | 3 wks | One map, path following, build pads, T1 Automatic Gun, T4 Anti-Tank Gun, E1/E6 enemies, Supply economy, one wave. Primitives-only art. **It should already feel like a game.** |
| **M2** | Full slice systems | 4 wks | 4 archetypes, 4-level upgrades with branch fork, Command Points, one ability, 4 enemies, 12 waves, wave preview, speed controls, pause-with-build, post-mortem panel |
| **M3** | **Vertical slice** | 3 wks | Bocage Crossroads at final art quality, US nation, Arsenal of Democracy signature, B1 boss, victory/defeat, integrated tutorial. **External playtest gate: three strangers finish it and one asks to replay.** |
| **M4** | Content systems | 4 wks | Remaining 5 archetypes, E3/E7/E12, `NationProfile` system, all 6 nations' archetype variants as data, Wave Editor plugin |
| **M5** | Signatures & support enemies | 4 wks | All 6 signature towers, E8–E11, Flak/air systems, Heavy Artillery, Balance Dashboard plugin |
| **M6** | Doctrines, meta, maps | 6 wks | 18 doctrines, maps 2–5, mastery, unlocks, achievements plumbing, Codex |
| **M7** | Campaign content | 6 wks | Maps 6–8, all 12 missions authored, 4 bosses, 4 difficulties, variant missions, briefing text |
| **M8** | Modes & platform | 4 wks | Skirmish, Endless, challenge modifiers, GodotSteam integration, cloud saves, achievements live, settings and accessibility complete |
| **M9** | Balance & polish | 6 wks | Full balance pass across 6 nations × 8 maps × 4 difficulties. Audio pass. VFX pass. Performance pass. Bug burn-down. |
| **M10** | Launch prep | 3 wks | Store page, capsule art, trailer, demo build, press kit, age ratings, Steam Next Fest participation |
| | **Total** | **45 wks ≈ 11 months** | |

Add a **20% buffer** and plan for **13 months**. A solo schedule without buffer is a fiction.

### 17.2 Critical path and gates

- **The M3 gate is real.** If the vertical slice is not fun with five towers on one map, do not proceed. Iterate at M3 for as long as it takes. Every subsequent milestone multiplies whatever the slice is.
- **Art commissioning starts at M3**, not before, and runs in parallel from M4 onward. Artist lead times are 4–8 weeks; brief them at M3 so assets land at M6.
- **Steam page live at M6** at the latest, to accumulate wishlists for at least six months before launch. Wishlist count at launch is the single strongest predictor of first-week revenue.
- **Demo at M8**, targeting the next Steam Next Fest. The demo is Mission 1 + Mission 2 with two nations.

### 17.3 Ongoing practices

- Weekly build to a private Steam branch from M4.
- Balance dashboard run before every content commit.
- Playtest with a real human every two weeks from M3. Watch them, do not talk.
- Keep a `CUT.md` file. Every idea that arrives mid-production goes there, not into the project. Review it at M9 and ship nothing from it.

---

## 18. The anti-feature-creep contract

### 18.1 Explicitly out of scope [X]

Multiplayer or co-op · procedural map generation · destructible terrain · dynamic pathfinding or maze-building · fog of war · a strategic/campaign map layer with resource management · hero units with abilities and inventories · persistent meta stat upgrades · tower "research trees" between missions · unit micromanagement of any kind · crafting · loot or randomized tower stats · a cosmetic store or any monetization beyond the base price · daily/weekly live challenges · mod support at launch · console or mobile ports at launch · localization at launch · a custom engine · Unity · Unreal · full voice acting · cutscenes or animated story sequences · a branching narrative · naval or air-superiority sub-games · a level editor · Steam Workshop · replays or spectator features · seasonal events.

### 18.2 The one-in-one-out rule

After M3, no feature enters the project without an equal-sized feature leaving it. The `CUT.md` file records both sides of every trade. This rule has no exceptions and applies to ideas from playtesters, from reviewers, and especially from the developer at 1am.

### 18.3 Designated pressure valves

If the schedule slips, cut in this order. Each is pre-approved and pre-scoped so the decision is fast:

1. **Doctrines 3 of 3** for each nation (18 → 12). Saves ~1 week.
2. **Challenge modifiers 10 → 6.** Saves ~4 days.
3. **Endless mode.** Saves ~1.5 weeks. (Keep Skirmish; it is far more valuable.)
4. **The Ruined Town clipped-range-arc gimmick** — ship the map with normal circular ranges. Saves ~1 week.
5. **Elite enemy variants.** Saves ~3 days.
6. **Maps 8 → 7**, cutting Island Jungle Route and moving Mission 9 to a Coastal variant. Saves ~2.5 weeks. **This is the last resort.**

Never cut: the post-mortem panel, accessibility features, the wave preview, the tutorial, or the balance pass.

---

## 19. Claude Code implementation prompt ladder

Each item below is one focused Claude Code / Codex session with a stated acceptance check. Prompts assume the architecture in Section 15. Give the agent the relevant GDD section as context, not the whole document. Where a session needs current Godot API detail beyond training-data knowledge, it should consult the current Godot 4.x documentation (via WebFetch) rather than guessing.

**M0 — Foundation**
1. Create the Godot 4.x project structure per §15.2, `.gitignore`, Git LFS config, and the C# `.csproj` with namespace folders for Core/Combat/Towers/Enemies/Waves/UI. *Accept: project opens clean in a headless `godot --headless --check-only` pass, C# build succeeds.*
2. Implement `GameLoop` (autoload) with a fixed 60Hz tick driven from `_PhysicsProcess`, a defined system update order, and a `TimeController` supporting 1×/2×/3× as ticks-per-physics-frame plus pause. *Accept: a debug counter advances at exactly 60/120/180 ticks per second.*
3. Implement `EventBus` (typed, subscribe/unsubscribe, no allocation on publish) and `ObjectPool<T>` (pooling `PackedScene` instances). *Accept: unit tests for both, runnable headless via GoDotTest/GUT.*
4. Implement `GameBalanceConfig`, `DamageTable` (§5.4), `ArmorClass`/`DamageType` enums, and the pure `ResolveDamage` function. *Accept: headless unit tests covering all 16 table cells plus the Spotted modifier.*
5. Implement `SeededRandom` and wire a per-mission seed. *Accept: two runs with the same seed produce identical spawn ordering.*

**M1 — Core loop**
6. Implement `PathNetwork` (`Path2D`/`PathFollow2D`-backed) and `PathFollower` with speed multipliers and per-segment modifiers. *Accept: a primitive moves entry→objective at a configured speed.*
7. Implement `BuildPad` (a `Node2D` with tags, hover highlight via `Area2D` mouse-enter/exit, and click-to-open-build-menu). *Accept: pads visibly respond and report their tag.*
8. Implement `EnemyController` + `EnemyDefinition` resource, HP, armor class, leak handling, and the Defense Line ledger. *Accept: an enemy reaching the objective reduces integrity by its leak cost.*
9. Implement the uniform spatial grid and `TargetingService` with the five priority modes. *Accept: 40 towers querying 200 enemies costs under 1ms per tick in the profiler.*
10. Implement `TowerController` + `TowerDefinition` resource + hitscan firing for T1 Automatic Gun. *Accept: a placed tower kills an enemy; damage matches the table.*
11. Implement `ProjectileSystem` (pooled `Node2D` scenes, leading, arcing) and T4 Anti-Tank Gun. *Accept: projectiles lead moving targets and expire correctly.*
12. Implement `SupplyLedger`: starting supply, kill bounty, end-of-wave income, spend/refund. *Accept: numbers match §7.2 exactly in a scripted scenario.*
13. Implement `WaveRunner` + `WaveDefinition`/`SpawnGroup` resources, single-wave playback. *Accept: an authored wave spawns exactly as specified.*

**M2 — Slice systems**
14. Implement `TowerUpgradeController`: 4 levels, branch fork at L3, cost curve per §7.4, sell with the 4-second full-refund window. *Accept: upgrade and sell math verified by test.*
15. Implement T3 Field Mortar (indirect, min range, travel time, densest-cluster targeting) and T9 Command Post (aura, non-stacking, reveal, CP generation). *Accept: aura visibly applies and does not stack.*
16. Implement `StatusController` for Suppressed and Spotted, with the 4-second non-refreshing cap. *Accept: statuses apply, expire, and display.*
17. Implement `CommandPointLedger` and the three universal abilities with hotbar, cooldowns, and targeting. *Accept: each ability resolves correctly, including while paused.*
18. Build the in-mission HUD per §13.4 using `Control` nodes and split `CanvasLayer`s (static chrome / dynamic overlays / tooltips). *Accept: all readouts live-update; no unnecessary redraw storms in the profiler.*
19. Build the wave preview strip per §10.7 with three tiers of disclosure. *Accept: N+1/N+2/N+3 show the correct detail levels.*
20. Build the tower inspection panel per §13.5 including damage-per-Supply tracking. *Accept: the stat matches a manually computed value.*
21. Implement the post-mortem panel per §12.9, including the suggestion rules table. *Accept: a deliberately AP-less run produces the AP suggestion.*
22. Implement enemy health bars, armor glyphs, status badges, and the ineffective-damage ricochet feedback (visual + audio hook). *Accept: SA vs Armored is unmistakable without reading numbers.*

**M3 — Vertical slice**
23. Implement the Arsenal of Democracy signature: production timer, friendly unit spawning, backwards path traversal, soft-blocking with 3s release, the 5-unit cap, and friendly render rules. *Accept: friendlies stall enemies and never permanently lock a lane.*
24. Implement `BossPhaseController` and B1 Breakthrough Panzer (armor skirt, HE 3× rate, post-break speed, adds). *Accept: skirt breaks correctly and the transition is telegraphed.*
25. Implement mission flow: briefing → loadout → mission → post-mortem → results, using Godot's `SceneTree.ChangeSceneToFile`/packed-scene flow, with retry-in-one-click. *Accept: full loop, no orphaned nodes (verify with Godot's orphan-node debug report).*
26. Implement the integrated 8-prompt tutorial with pause-and-highlight. *Accept: a first-time player completes Mission 1 without external help.*

**M4 — Content systems**
27. Implement the remaining archetypes T2, T5, T6, T7, T8 with both branches each. *Accept: each behaves per §6 and passes the Balance Dashboard hard-floor checks.* (Split into 3–5 sessions, one or two archetypes each.)
28. Implement `NationProfile`, the stat-leaning application layer, and the ±15%/±3% validators. *Accept: switching nation changes stats without changing behavior code.*
29. Implement enemies E3, E7, E12 including Siege tower-suppression. *Accept: Enclosed pads are correctly immune.*
30. Build the Wave Editor plugin (`EditorPlugin` + `GraphEdit`-based dock) with the Threat Value graph and pacing warnings. *Accept: authoring a 20-wave mission takes under 30 minutes.*

**M5 — Signatures and air**
31–35. Implement the five remaining signature towers, one per session (RAF Scramble, Katyusha Storm, Blitzkrieg, Bersaglieri Charge, Special Attack Airfield), each per its §8.2 spec including readability requirements. *Accept per tower: activation, cooldown/charge, visual telegraph, and the named balancing mitigations all present.*
36. Implement air corridors, air targeting rules, `Air` class handling, and E8. *Accept: no ground tower ever acquires an air target; Flak works.*
37. Implement E9 Support (tether), E10 Escort (shield pool), E11 Recon (concealment). *Accept: the Minefield-vs-Concealed interaction works.*
38. Build the Balance Dashboard plugin. *Accept: it reports national parity within ±3% and flags any injected violation.*

**M6–M8 — Content, modes, platform**
39. Implement `DoctrineDefinition` and the six shared ability behaviors; author all 18 doctrines as data. *Accept: no doctrine requires bespoke code.*
40. Implement map gimmicks per §11.2 (one session per gimmick family).
41. Implement `ProgressionService`, `UnlockService`, `MasteryService`, stars, and the save system with versioned JSON migration. *Accept: a v1 save loads in a v2 build.*
42. Implement Skirmish and Endless mode flows plus the 10 challenge modifiers. *Accept: modifiers stack correctly and score multipliers apply.*
43. Implement `IPlatformService` + `SteamService` (GodotSteam-backed: achievements, cloud, optional leaderboards) with a working Null implementation. *Accept: the game runs identically with Steam absent.*
44. Implement the settings and accessibility systems per §13.8–13.9, including the three colorblind palettes (as `Theme` swaps) and the reduced-effects mode. *Accept: every mechanic is legible with all color removed.*
45. Build the Data Validator (`EditorPlugin` menu command plus a headless CLI entry point) and wire the headless form into a pre-commit check. *Accept: it catches a deliberately broken reference, both from inside the editor and from `godot --headless`.*

---

## 20. Risk register

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Six nations become six balance problems | Medium | High | The ±15% envelope, the ±3% parity validator in the Balance Dashboard, and the rule that only signatures carry unique code |
| The vertical slice is not fun | Medium | Critical | The M3 gate. Do not proceed. Iterate on the slice indefinitely rather than adding content to an unfun core |
| Art commissioning slips or quality is inconsistent | Medium | High | Single artist for units+towers; brief with a written visual-language doc; primitives-only prototype means art is never blocking |
| Signature towers are unbalanced against each other | High | Medium | Each spec above names its balancing risks explicitly; playtest each signature head-to-head on the same map at M5 |
| Friendly-unit signatures (US, Italy) create visual chaos | Medium | Medium | The 85%-scale, white-outline, lower-z-index rule; a specific stress-test scene at M5 |
| Scope creep from playtester feedback | High | High | `CUT.md`, the one-in-one-out rule, the pre-approved pressure-valve list |
| Tone/content complaint at launch | Low | High | §14 rules are absolute; fictionalized insignia; the opening notice; no ideology anywhere |
| German market age-rating friction | Low | Medium | Fictionalized insignia designed specifically to clear this without a separate build; budget one week for USK submission |
| Performance collapse in late waves | Medium | Medium | Spatial grid, pooling, voice limits, a hard 200-enemy budget, monthly testing on the weakest available machine |
| Godot C# ecosystem has thinner AI training-data coverage than Unity's | Medium | Medium | Agents consult current Godot docs via WebFetch when memorized idiom is uncertain; the Data Validator and headless test suite catch API-misuse regressions early; architecture is kept engine-idiom-light (plain C# where possible) to reduce surface area that depends on remembering Godot-specific APIs |
| Solo burnout | High | Critical | 20% schedule buffer, weekly builds for visible progress, external playtests for morale, and a strict "ship the scope" discipline |

---

## 21. Success criteria

**Vertical slice (M3):** three unfamiliar players complete Mission 1; at least one asks to play again immediately; all three can correctly explain why an enemy leaked.

**Launch quality bar:** a player can finish the 12-mission campaign on Regular in about 3 hours, replay it with a different nation and make genuinely different decisions, and reach roughly 12–15 hours of play before exhausting the content. Median Steam review sentiment cites *clarity* and *nation variety* as strengths.

**Commercial:** 8,000+ wishlists at launch, achieved via a Steam page live from M6 and a demo in one Steam Next Fest.

---

*End of document. Version 1.1 (Godot revision). Section 19 is the operational entry point — start at prompt 1.*
