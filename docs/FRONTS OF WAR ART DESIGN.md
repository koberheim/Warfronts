# FRONTS OF WAR
## Art Design Specification
### Version 1.0 — 80/20 Stylized Visual Direction

---

# 1. ART DIRECTION NORTH STAR

**Fronts of War** is a richly illustrated, highly readable WWII tower-defense game rendered as a living painted battlefield.

The visual formula is:

**80% stylized / 20% grounded**

Its visual DNA combines:

- the immediate readability and personality of a stylized tower-defense game;
- the recognizable equipment silhouettes and material language of grounded WWII strategy;
- richly authored environmental storytelling;
- painterly, storybook battlefield environments;
- exaggerated, satisfying combat effects;
- a restrained command-table presentation layer surrounding—but not replacing—the battlefield.

The goal is **not historical simulation** and **not cartoon parody**.

The battlefield should feel like an exceptionally polished illustrated WWII adventure: recognizable Shermans, halftracks, field guns, farmhouses, rail yards, jungle roads and bunkers, but interpreted through bold shapes, compressed detail, chunky proportions and painterly color.

The GDD already establishes readability as the game's primary quality bar: after a failure, the player should understand what leaked, why, and what counter was needed. Art therefore serves gameplay before authenticity or decorative detail.

### Art-direction sentence

> **A colorful, hand-painted WWII battlefield built from bold shapes, chunky recognizable equipment and carefully controlled environmental detail, designed to remain immediately readable during large-scale combat.**

### The five governing visual principles

**1. Silhouette before detail.**  
If the player cannot identify something by shape, additional texture will not save it.

**2. Exaggerate function.**  
Important guns are larger. Heavy tanks are broader. Mortar tubes are clearer. Radios have prominent antennas. Sandbags are chunky. Explosions are oversized.

**3. Simplify reality rather than replacing it.**  
A Sherman should still unmistakably be a Sherman-inspired vehicle. A Norman farmhouse should still look like a Norman farmhouse. Stylization compresses and emphasizes reality.

**4. Detail belongs where gameplay does not.**  
Roads, enemies, towers, build pads and important effects receive clean visual space. Peripheral areas carry environmental storytelling.

**5. Charm without comedy.**  
The world can be appealing, colorful and exaggerated. Soldiers and nations are never treated as caricatures.

---

# 2. VISUAL STYLE TARGET — 80/20

The selected 80/20 direction establishes the following baseline.

## 2.1 What gets stylized

Stylization should primarily affect:

- proportions;
- silhouettes;
- shape simplification;
- color separation;
- value grouping;
- edge treatment;
- texture density;
- weapon scale;
- environmental forms;
- foliage masses;
- VFX scale;
- animation timing;
- UI illustration.

Stylization should **not** primarily come from:

- giant cartoon eyes;
- extreme human anatomy;
- comedy facial expressions;
- toy-like vehicles;
- slapstick deaths;
- fantasy colors;
- deliberately inaccurate equipment;
- national caricatures.

## 2.2 Detail philosophy

Individual objects should generally contain **less detail than reality but stronger detail than reality**.

A tank does not need every hatch, bolt and weld.

It does need:

- unmistakable turret geometry;
- recognizable hull proportions;
- prominent tracks;
- an exaggerated main gun;
- several large equipment shapes;
- clear highlights and shadows;
- 2–4 strategically placed surface details.

At gameplay scale, twenty tiny accurate details become noise. Five exaggerated correct details become identity.

---

# 3. CAMERA AND PERSPECTIVE

The GDD's mechanical camera remains locked: orthographic top-down, with roads, tracks and fixed build pads remaining geometrically reliable.

Art uses an **overhead-with-cheat** approach.

## 3.1 Mechanical plane

Gameplay operates on a true top-down 2D plane.

Therefore:

- range circles remain circles;
- collision footprints correspond to ground footprints;
- routes remain unobscured;
- sprites can rotate freely;
- gameplay does not depend on visual perspective.

## 3.2 Artistic cheat

Objects may reveal enough side surface to communicate volume.

Examples:

- buildings expose portions of walls beneath roofs;
- tanks reveal some hull side;
- sandbag rings expose inner and outer faces;
- crates reveal a front face;
- trees show trunk beneath canopy;
- artillery pieces expose wheels and carriage;
- trenches expose inner earth walls.

The result should *suggest* roughly a 65–75° elevated viewing angle while preserving mechanically top-down footprints.

This is a visual illusion, not an actual perspective camera.

## 3.3 Forbidden perspective behavior

Avoid:

- strong vanishing points;
- dramatically foreshortened buildings;
- foreground objects becoming substantially larger;
- roads narrowing toward the top of the screen;
- objects visually leaning in inconsistent directions;
- tall decorative objects obscuring routes.

---

# 4. SHAPE LANGUAGE

Fronts of War uses **broad, confident, slightly irregular shapes**.

Avoid tiny geometry.

### Primary shapes

Large forms establish identity.

Examples:

- tank hull;
- turret;
- farmhouse roof;
- tree canopy;
- bunker;
- artillery emplacement.

### Secondary shapes

These establish function.

Examples:

- tank gun;
- sandbag ring;
- chimney;
- antenna;
- wheels;
- windows;
- ammunition crates.

### Tertiary shapes

These provide flavor.

Examples:

- straps;
- tools;
- bolts;
- scattered stones;
- individual leaves;
- small debris.

At normal gameplay zoom:

**Primary shapes must dominate. Secondary shapes should remain visible. Tertiary shapes may disappear.**

If tertiary detail competes with a primary silhouette, remove it.

---

# 5. PROPORTION LANGUAGE

## Infantry

Use moderately heroic/chunky proportions.

Target approximately:

**5.5–6 heads tall rather than realistic 7.5–8 heads.**

Slightly enlarge:

- helmet/head;
- shoulders;
- hands;
- boots;
- primary weapon;
- backpack/equipment.

Weapons should generally read **15–25% larger visually** than strict realistic scale.

The result should look like a stylized soldier, not a bobblehead.

## Vehicles

Vehicles should generally be:

- approximately 10–15% broader;
- slightly shorter longitudinally where useful;
- fitted with 15–25% exaggerated primary weapons;
- equipped with enlarged wheels/tracks;
- stripped of insignificant surface detail.

Heavy armor should exaggerate **mass**, not simply overall scale.

A heavy tank should feel:

**wide + low + thick + long-gunned.**

The existing GDD size hierarchy remains authoritative:

- Infantry — 32 px
- Light Vehicle — 44 px
- Medium Armor — 56 px
- Heavy Armor — 76 px
- Air — 88 px plus shadow

Size is an absolute threat cue.

## Towers

Tower silhouettes should exaggerate the part that communicates function.

Examples:

**Automatic Gun**
- oversized gun shield;
- clearly visible twin/barrel assembly;
- compact emplacement.

**Mortar**
- large upward-facing tube;
- ammunition nearby;
- circular sandbag position.

**Anti-Tank**
- extremely readable long barrel;
- broad carriage;
- angular gun shield.

**Command Post**
- antennas;
- radio equipment;
- maps/signals gear;
- comparatively little weaponry.

The player should identify tower **role before nation**.

---

# 6. SILHOUETTE STANDARD

Every gameplay unit must pass the GDD's black-silhouette test.

This rule expands to towers.

Every tower and enemy should remain identifiable when:

1. converted to solid black;
2. displayed at normal gameplay scale;
3. shown for approximately one second.

Where two silhouettes collide, modify:

1. overall footprint;
2. weapon direction/length;
3. turret/body geometry;
4. external equipment;
5. negative space.

Do **not** solve silhouette collisions primarily with paint schemes.

---

# 7. RENDERING LANGUAGE

## 7.1 Painterly storybook battlefield

Terrain and static scenery use hand-painted forms with visible but restrained brush character.

Surfaces should not appear:

- photorealistic;
- procedurally noisy;
- digitally airbrushed;
- heavily outlined;
- materially flat.

Use broad color masses first, texture second.

## 7.2 Edge treatment

Environment:

- mostly painted edges;
- minimal explicit outlines;
- darker local-value separation where forms overlap.

Gameplay units:

- stronger separation;
- deliberate outline treatment according to friend/foe rules.

The GDD specifies player-side units with a 2 px white outline at 85% scale and enemies with a 1 px dark outline. Friend/foe must never rely on color alone.

## 7.3 Texture

Texture should operate at three scales.

**Large texture:** broad mud, grass, sand, snow, stone.

**Medium texture:** wheel ruts, roof tiles, masonry variation, field rows.

**Fine texture:** scratches, grass blades, stones, chips.

Fine texture is heavily restricted near gameplay routes.

---

# 8. COLOR PHILOSOPHY

Color should be **richer and more organized than reality**.

Do not reproduce the desaturated grey-brown appearance associated with many realistic WWII games.

The battlefield should be attractive enough that players enjoy simply looking at it.

However, avoid candy saturation.

## Color hierarchy

### Environment
Moderately saturated, cohesive theater palette.

### Units
Slightly stronger local contrast than environment.

### Towers
Highest persistent gameplay-object contrast.

### VFX
Highest temporary contrast.

### UI
Distinct enough to remain separate from battlefield.

This creates:

**Environment < Units < Towers < Combat VFX**

in attention priority.

---

# 9. NATIONAL VISUAL IDENTITY

Nationality should be recognizable primarily through:

1. equipment silhouette;
2. construction language;
3. uniform/equipment design;
4. restrained palette;
5. fictionalized insignia.

Palette reinforces identity but does not create it.

The GDD intentionally makes nationality the weakest gameplay visual signal and requires fictionalized markings. 
### United States

Visual tendencies:

- olive drab;
- warm canvas;
- khaki;
- exposed steel;
- practical modular construction;
- timber/crate supply language.

Character: **industrial, practical, standardized.**

### Britain

Visual tendencies:

- muted olive;
- khaki;
- brown;
- canvas;
- slightly more improvised field construction.

Character: **practical, compact, field-adapted.**

### Soviet Union

Visual tendencies:

- earthy green;
- brown;
- dark steel;
- raw timber;
- rugged, simplified construction.

Character: **robust, blunt, functional.**

### Germany

Visual tendencies:

- field green;
- darker grey-green;
- muted tan;
- angular steel construction;
- precise equipment arrangements.

Character: **angular, engineered, compact.**

### Italy

Visual tendencies:

- warm olive;
- sand;
- faded green;
- canvas;
- lighter structural forms.

Character: **lightweight, mobile, Mediterranean.**

### Japan

Visual tendencies:

- khaki;
- olive-brown;
- warm canvas;
- dark metal;
- timber/bamboo field construction where appropriate.

Character: **compact, field-built, terrain-adapted.**

These are art-direction tendencies, **not team colors**.

---

# 10. HISTORICAL AND TONE GUARDRAILS

Fronts of War is fictionalized WWII strategy, not a depiction of WWII ideology or human suffering.

The GDD prohibits national caricature and requires equivalent archetypes across nations to retain identical underlying mechanics.

Art must therefore avoid:

- national stereotypes;
- ideological imagery;
- extremist symbols;
- political leaders;
- propaganda;
- civilians as gameplay objects;
- graphic injury;
- corpses;
- blood;
- atrocity imagery.

Destroyed vehicles become readable wrecks. Infantry resolve through non-graphic disappearance/dispersal effects.

WWII identity instead comes from equipment silhouettes, architecture, terrain, field construction, sound and environmental context.

---

# 11. ENVIRONMENT DESIGN SYSTEM

Environmental richness is a major part of the game's identity.

But environmental storytelling must be **authored rather than scattered randomly**.

Every map uses three classes of environmental assets.

## Tier 1 — Hero / Location Assets

Large map-defining objects.

Examples:

- Norman farmhouse;
- ruined church;
- railway station;
- factory;
- stone bridge;
- bunker complex;
- coastal battery;
- jungle hut group;
- windmill;
- depot;
- warehouse.

Target:

**4–8 hero assets per map.**

These establish location and provide screenshot identity.

They should preferably belong to reusable theater kits rather than being completely unique.

---

# 12. TIER 2 — STORYTELLING PROP CLUSTERS

Prop clusters are the principal tool for achieving Commandos-like environmental richness without making map assembly prohibitively expensive.

A cluster is an authored arrangement of several props saved and placed as a single composition.

Examples:

### Supply Dump
- crates;
- barrels;
- canvas stack;
- pallet;
- handcart.

### Abandoned Position
- sandbags;
- ammunition box;
- helmet;
- field telephone;
- spent equipment.

### Farmyard
- cart;
- barrels;
- fence;
- hay;
- tools.

### Rail Maintenance
- sleepers;
- rails;
- toolbox;
- oil drums;
- maintenance cart.

### Jungle Cache
- crates;
- tarpaulin;
- fuel drums;
- radio equipment;
- vegetation.

### Field Kitchen
- stove;
- tables;
- boxes;
- canvas shelter;
- utensils.

Clusters should normally contain roughly **4–12 component sprites**.

Variants can be generated by:

- rotation;
- mirroring;
- removing one element;
- exchanging one element;
- changing ground decal;
- changing foliage integration.

One prop family should create several clusters.

This creates visual richness without requiring hundreds of unrelated bespoke objects.

---

# 13. TIER 3 — MICRO-DETAIL OVERLAYS

Cheap environmental texture elements include:

- tire tracks;
- footprints;
- mud;
- shell marks;
- small rubble;
- fallen boards;
- weeds;
- leaf piles;
- snow disturbance;
- grass tufts;
- stones;
- oil stains;
- sand ripples;
- broken branches.

These are primarily decals/overlays.

They must never reduce route readability.

---

# 14. ENVIRONMENTAL DENSITY ZONES

Every map is deliberately divided into visual-density zones.

### Zone A — Gameplay Lane
**Density: LOW**

Road and immediate combat corridor.

Use:

- ground texture;
- occasional small debris;
- tire tracks;
- restrained decals.

Avoid large props.

### Zone B — Build-Pad Halo
**Density: LOW–MEDIUM**

Keep enough visual breathing room that tower silhouettes remain clean.

Decorative elements should visually frame pads rather than invade them.

### Zone C — General Terrain
**Density: MEDIUM**

Use:

- vegetation;
- fences;
- rocks;
- agricultural detail;
- small clusters.

### Zone D — Storytelling / Non-Playable Areas
**Density: HIGH**

Use:

- hero assets;
- dense prop clusters;
- environmental narratives;
- abandoned equipment;
- architectural detail;
- layered foliage.

The player's eye should naturally understand:

**quiet space = gameplay**

**rich space = worldbuilding**

---

# 15. PROP FAMILY PRODUCTION RULE

Never commission isolated decorative objects unless necessary.

Commission **families**.

Example — Supply Family:

- 4 crate variations;
- 3 barrel variations;
- 2 canvas stacks;
- 2 ammunition stacks;
- 1 handcart;
- 1 pallet;
- 5 preassembled clusters.

Those components can produce dozens of visually distinct arrangements.

This should become the standard environmental production unit.

---

# 16. FOLIAGE

Foliage should use bold clustered shapes rather than individually rendered leaves.

## Trees

Trees generally consist of:

- strong canopy silhouette;
- 3–7 major foliage masses;
- visible trunk where helpful;
- simplified internal texture;
- painted shadow.

Avoid perfectly circular "lollipop" trees.

## Bushes

Bushes should be irregular masses with limited internal detail.

## Grass

Grass primarily belongs in:

- clumps;
- borders;
- decals;
- field masses.

Avoid uniform procedural grass covering the map.

## Foliage gameplay rule

Foliage adjacent to roads should generally be darker or lower-contrast than units moving past it.

No foliage silhouette should resemble infantry at gameplay scale.

---

# 17. BUILDINGS

Buildings provide major environmental identity.

They should be slightly compressed vertically and widened where necessary to fit the overhead composition.

Prioritize:

1. roof silhouette;
2. building footprint;
3. architectural region;
4. major wall features;
5. damage state;
6. small decoration.

Buildings should generally expose enough wall to provide volume.

Roof texture should remain broad and readable.

Avoid hundreds of individually rendered roof tiles.

### Destruction

Destroyed buildings use:

- broken roof silhouettes;
- exposed rafters;
- rubble masses;
- darkened interiors;
- limited smoke/scorching.

Do not fill ruins with excessive fine debris.

---

# 18. ROADS AND PATHS

Routes are among the most important visual objects in the game.

The GDD uses roads, tracks and other visible routes as the foundation of path-based gameplay.

Routes must remain readable even when no units occupy them.

Roads should use:

- stronger value separation from surrounding terrain;
- clear shoulders;
- broad painted tire/wear patterns;
- environmental integration;
- distinctive intersections.

Avoid razor-clean paths.

Road edges should look natural but never ambiguous.

---

# 19. BUILD PADS

Build pads must look like believable prepared military positions **and** unmistakable interactive locations.

Possible visual language:

- cleared earth;
- sandbag corners;
- timber framing;
- equipment markers;
- painted/chalk boundary;
- subtle construction stakes.

Empty pads should not resemble finished towers.

Available, selected, unavailable and occupied states must differ by both shape/iconography and color.

---

# 20. THEATER VISUAL PILLARS

## Western Europe

Core palette:

- rich greens;
- ochre;
- damp earth;
- limestone grey;
- muted red/brown roofs.

Environmental vocabulary:

- bocage;
- sunken roads;
- wheat fields;
- orchards;
- stone walls;
- Norman farmhouses;
- drainage ditches;
- village structures;
- damaged civilian infrastructure without depicting civilians.

Bocage Crossroads specifically uses patchwork fields, thick hedgerows, sunken lanes and a stone farmhouse objective; the GDD calls for greens, ochres and overcast lighting.

## Mediterranean / North Africa

Core palette:

- warm sand;
- dusty ochre;
- pale limestone;
- faded olive;
- turquoise accents where coastal.

Vocabulary:

- rocky outcrops;
- wadis;
- desert roads;
- fuel dumps;
- stucco;
- stone villages;
- olive groves;
- vineyards;
- dry grasses;
- coastal terrain.

## Eastern Europe

Core palette:

- muted forest green;
- birch white;
- dark mud;
- cold blue-grey;
- snow white;
- industrial rust.

Vocabulary:

- birch forests;
- broad fields;
- muddy roads;
- wooden structures;
- rail lines;
- factories;
- snow;
- industrial infrastructure.

## Pacific

Core palette:

- deep jungle green;
- warm mud;
- volcanic grey;
- beach sand;
- turquoise water;
- weathered timber.

Vocabulary:

- layered jungle;
- palms;
- muddy tracks;
- volcanic rock;
- coral beaches;
- bunkers;
- airfield infrastructure;
- timber/bamboo structures;
- supply dumps.

---

# 21. GAMEPLAY UNIT READABILITY

The GDD expects potentially very large numbers of units on screen and makes size and silhouette primary recognition mechanisms.

Therefore units should generally contain:

**3–5 major color/value masses.**

Not fifteen.

Example infantry:

1. helmet/head;
2. torso;
3. weapon;
4. legs;
5. equipment pack.

Example tank:

1. hull;
2. tracks;
3. turret;
4. gun;
5. equipment/accent.

---

# 22. ARMOR-CLASS VISUAL LANGUAGE

Armor classification should be recognizable before the player reads an icon.

The GDD establishes this principle explicitly.

### Soft

- narrow;
- loose;
- exposed;
- small.

### Hardened

- bulkier;
- protected;
- visibly reinforced;
- more enclosed.

### Armored

- strong track/wheel mass;
- turret or armored body;
- broad footprint.

### Heavy

- oversized;
- wide;
- thick;
- long primary weapon;
- visually dominant.

---

# 23. TOWER VISUAL PROGRESSION

The production plan uses only two principal art states rather than four fully separate tower designs.

### Level 1

Base emplacement.

Simple and clean.

### Level 2

Same principal art family.

Progress communicated using small additions such as:

- level indicator;
- ammunition;
- equipment;
- reinforcement detail.

Do not require a new full tower illustration.

### Level 3

Major branch silhouette appears.

This is the important transformation.

The player should immediately recognize which branch was selected.

### Level 4

Build on the branch identity.

Add:

- equipment;
- stronger weapon treatment;
- reinforcement;
- branch-specific effects.

Do not completely redesign the tower.

### Upgrade rule

**Branch choice must change silhouette, not merely decoration.**

---

# 24. COMBAT VFX

Combat uses **readability-enhanced realism**.

Effects should feel approximately **20–30% larger, brighter and clearer than reality would suggest**.

## Gunfire

Use:

- bright muzzle flash;
- readable tracer;
- brief smoke;
- directional recoil.

## Machine guns

Tracers should form readable firing rhythms rather than continuous laser lines.

## Anti-tank

Emphasize:

- powerful muzzle blast;
- heavy recoil;
- bright projectile/tracer cue;
- strong armor impact.

## Mortars / artillery

Projectile arc should remain understandable.

Impacts use:

- bright initial flash;
- chunky dirt/smoke mass;
- short debris burst;
- persistent but rapidly clearing smoke.

## Explosions

Explosion language:

**flash → expanding shape → smoke → rapid dissipation**

Avoid realistic lingering smoke obscuring gameplay.

---

# 25. VFX SHAPE LANGUAGE

Effects should use strong graphic shapes.

Examples:

- muzzle flash = star/wedge;
- explosion = rounded angular burst;
- AP impact = sharp directional sparks;
- ricochet = narrow spark fan;
- smoke = broad overlapping painted puffs.

Effects may have painterly edges but must remain crisp enough to read instantly.

---

# 26. STATUS EFFECTS

Existing GDD language remains authoritative:

**Suppressed**
- grey dust cloud;
- downward-arrow icon.

**Spotted**
- red crosshair reticle.

**Concealed**
- 45% opacity;
- dashed outline.



These should remain visually consistent across every nation and theater.

Gameplay status language overrides environmental art direction.

---

# 27. DEATH AND DESTRUCTION

Infantry:

- quick dust/smoke puff;
- disappearance/dispersal;
- dropped abstract token where required.

Vehicles:

- readable wreck;
- smoke;
- temporary scorch;
- fade after the gameplay-defined duration.

The GDD specifies static vehicle wrecks fading over three seconds and explicitly prohibits gore.

Destruction should feel satisfying without becoming graphic.

---

# 28. ANIMATION

Animation should emphasize clarity and personality rather than realism.

### Infantry

GDD baseline:

**4-frame walk cycle.**

Use:

- strong arm/leg poses;
- readable weapon position;
- slightly exaggerated stride.

### Vehicles

Use:

- body rotation;
- turret rotation where applicable;
- simple wheel/track animation;
- recoil;
- occasional suspension/body kick if economical.

### Towers

Prioritize functional animation:

- gun rotation;
- recoil;
- reload;
- muzzle effects;
- antenna movement;
- radar/search movement where applicable.

Idle animation should be restrained.

---

# 29. UI — HYBRID WAR TABLE

This specification deliberately refines the GDD's original war-table concept.

The **battlefield itself is not literally a board game or printed map**.

Instead:

> **The commander's map table is the interface framing device surrounding a living painted battlefield.**

The original GDD already calls for wooden edges, brass fittings, paper slips and grease-pencil language.

We retain that vocabulary but reduce its literal intrusion into gameplay.

Use war-table materials heavily for:

- main menus;
- mission briefing;
- campaign presentation;
- tower cards;
- tooltips;
- wave information;
- pause screen;
- mission results;
- codex.

Use them more lightly in the active gameplay HUD.

---

# 30. UI MATERIAL LANGUAGE

Primary materials:

- dark painted metal;
- worn paper;
- map paper;
- aged wood;
- brass;
- canvas;
- stamped ink;
- grease pencil.

Avoid making every UI element look physically handmade.

The UI still needs the precision and responsiveness of a modern PC strategy game.

Physical material is the skin.

Modern information hierarchy is the structure.

---

# 31. ICONOGRAPHY

Icons use:

- bold silhouettes;
- limited internal lines;
- thick negative spaces;
- consistent perspective;
- high contrast.

Icons should resemble illustrated military field-manual symbols without directly copying historical military documents.

No gameplay mechanic should depend on color alone, consistent with the GDD accessibility standard.

---

# 32. VFX ACCESSIBILITY

Every effect must support:

**Full**

Complete intended presentation.

**Reduced**

Reduced particles and screen effects.

**Minimal**

Gameplay-critical shapes only.

The GDD requires Minimal to remove flashes, reduce particle counts by approximately 70%, disable camera shake and simplify major barrages. No flashing may exceed 3 Hz.

Therefore no mechanic may require decorative particles to understand what happened.

---

# 33. MAP COMPOSITION

Maps should be composed using three simultaneous layers.

### Layer 1 — Gameplay Geometry

- routes;
- build pads;
- objectives;
- entry points;
- air corridors.

This layer is sacred.

### Layer 2 — Geographic Logic

- fields;
- rivers;
- cliffs;
- villages;
- forest;
- rail lines.

This explains why the route exists.

### Layer 3 — Environmental Story

- abandoned positions;
- supplies;
- wrecks;
- damaged infrastructure;
- field camps;
- equipment.

This explains what happened here.

Never allow Layer 3 to damage Layer 1.

---

# 34. VISUAL STORYTELLING

Environmental scenes should imply military activity without requiring text.

Examples:

- hurriedly abandoned checkpoint;
- recently reinforced farmhouse;
- damaged bridge under repair;
- supply convoy unloading;
- artillery position recently vacated;
- rail yard being used as a logistics hub;
- jungle bunker connected to supply caches;
- snow-covered defensive position.

Avoid environmental narratives involving civilian suffering, occupation or atrocities, consistent with the GDD's tone policy.

---

# 35. MAP UNIQUENESS TEST

Every map should be identifiable from a screenshot showing approximately **25% of the battlefield with the HUD removed**.

If two maps cannot pass this test, their environmental identity is insufficiently differentiated.

Uniqueness should come from:

- terrain structure;
- palette;
- architecture;
- foliage;
- hero assets;
- weather;
- road construction;
- environmental storytelling.

Not simply a color filter.

---

# 36. LIGHTING

Lighting is largely baked into artwork rather than dynamically simulated.

Default approach:

- broad ambient illumination;
- soft directional shadows;
- readable local contrast;
- limited deep blacks.

Avoid dramatic realistic lighting that hides units.

Night maps should be:

**night-colored, not dark.**

Use cool ambient terrain plus warm localized lights while preserving gameplay contrast.

---

# 37. SHADOWS

Use painted/soft graphic shadows.

Gameplay units:

- compact shadow;
- consistent light direction;
- enough separation to anchor sprite to terrain.

Buildings and trees:

- broader soft shadow;
- carefully controlled opacity.

Aircraft require a separated ground shadow to communicate altitude.

---

# 38. WEATHER

Weather is primarily a visual/thematic layer unless explicitly tied to a gameplay mechanic.

Possible effects:

- light rain;
- drifting snow;
- sand haze;
- jungle mist;
- wind-driven leaves;
- dust.

Weather must never obscure:

- enemy silhouettes;
- build pads;
- routes;
- range indicators;
- status icons.

---

# 39. ASSET CONSTRUCTION PHILOSOPHY

Build the world as a **kit**, not as a collection of paintings.

Reusable asset categories:

1. ground materials;
2. road segments;
3. foliage families;
4. architecture families;
5. prop families;
6. prop clusters;
7. decals;
8. tower components;
9. unit components;
10. VFX libraries.

Maps should feel authored even though most components are reusable.

---

# 40. ENVIRONMENT KIT TARGET

A theater kit should aim to contain roughly:

### Terrain
- 4–6 ground materials;
- 3–5 road/track treatments;
- 10–15 transition/edge pieces.

### Vegetation
- 5–8 tree variants;
- 6–10 bushes;
- 8–12 grass/ground plants.

### Architecture
- 4–8 reusable structures;
- 2–4 hero structures;
- damage variants where valuable.

### Props
- 25–40 individual reusable props;
- 12–20 authored clusters.

### Decals
- 20–30 dirt/damage/storytelling overlays.

These are production targets, not new GDD content commitments.

---

# 41. BOCAGE CROSSROADS — ART-DIRECTION PROOF

Bocage Crossroads remains the visual vertical slice.

The GDD explicitly requires it to be hand-painted at final quality and serve as the art-direction proof.

Its purpose is therefore larger than simply producing Mission 1.

It must prove:

- camera treatment;
- 80/20 stylization;
- foliage;
- roads;
- buildings;
- environmental density;
- build-pad readability;
- unit readability;
- tower readability;
- national identity;
- VFX;
- HUD integration;
- performance.

**Do not commission the remaining map library until Bocage Crossroads establishes these standards successfully.**

---

# 42. BOCAGE CROSSROADS TARGET COMPOSITION

The map should feature:

- irregular patchwork farmland;
- thick sculpted hedgerows;
- sunken lanes;
- muddy road edges;
- Norman-style stone farmhouse;
- field boundaries;
- orchard/tree clusters;
- utility poles/fences;
- small military storytelling clusters;
- damaged equipment;
- restrained shell damage.

Gameplay routes should remain cleaner than surrounding fields.

Hedgerows become major compositional walls that frame combat.

---

# 43. BOCAGE PROP FAMILIES

Recommended first production families:

### Farm
- wooden fence;
- stone wall;
- cart;
- hay;
- trough;
- barrels;
- tools;
- gates.

### Road
- signs;
- milestones;
- utility poles;
- drainage;
- broken fencing;
- road debris.

### Military Supply
- crates;
- barrels;
- ammunition;
- tarps;
- pallets;
- handcart.

### Defensive
- sandbags;
- stakes;
- camouflage net;
- ammunition;
- field radio;
- barriers.

### Damage
- shell craters;
- rubble;
- scorch marks;
- broken timber;
- wreck fragments.

These five families should provide enough components to create most of the map's storytelling clusters.

---

# 44. VERTICAL-SLICE UNIT/TOWER ART

The GDD defines the vertical slice around the United States, four enemy archetypes and the Breakthrough Panzer boss.

Art development should therefore prioritize:

### US tower language
- Automatic Gun / Browning-style emplacement;
- Field Mortar;
- anti-armor tower;
- Command Post/recon position;
- Arsenal of Democracy signature.

### Enemies
- Basic Infantry;
- Fast Infantry;
- Light Vehicle;
- Medium Armor;
- Breakthrough Panzer.

These assets should establish the production templates used by later nations.

---

# 45. ART ACCEPTANCE CHECKLIST

No gameplay asset is approved until it passes the following.

### Silhouette
Can it be recognized in solid black?

### Native Scale
Can it be identified at intended gameplay size?

### Function
Can the player infer what it does?

### Contrast
Does it separate from likely backgrounds?

### Rotation
Does it remain readable through 360° where rotation applies?

### Nationality
Is national identity present but subordinate to function?

### Style
Does it match the 80/20 stylization target?

### Detail
Are there unnecessary small details?

### Tone
Does it avoid caricature or prohibited imagery?

### VFX
Does its attack communicate direction and impact clearly?

If an asset fails one of the first four categories, it is **not production-ready regardless of illustration quality**.

---

# 46. SCREENSHOT TEST

Every major art milestone should be evaluated using three screenshots:

### Clean
No combat.

Tests environment and composition.

### Typical
Normal mid-wave combat.

Tests normal readability.

### Stress
Maximum expected enemy density + towers firing + statuses + abilities.

Tests whether the art direction survives the actual game.

An environment that looks beautiful only in the Clean screenshot has failed.

---

# 47. BLUR TEST

Reduce a gameplay screenshot to approximately **25% size** or apply strong blur.

The following should still separate:

- roads;
- enemy flow;
- towers;
- major buildings;
- terrain zones;
- explosions.

If everything becomes a uniform textured field, value grouping needs revision.

---

# 48. GRAYSCALE TEST

Review gameplay screenshots without color.

Routes, units, towers and major effects should remain readable.

This catches designs that rely too heavily on hue.

---

# 49. PRODUCTION PRIORITY

When art time or money becomes constrained, preserve quality in this order:

1. gameplay-unit silhouettes;
2. towers;
3. VFX readability;
4. routes/build pads;
5. hero environmental assets;
6. major terrain;
7. prop clusters;
8. micro-detail.

**Never sacrifice gameplay clarity to preserve decorative density.**

---

# 50. FINAL VISUAL TARGET

A finished Fronts of War screenshot should immediately communicate:

**WWII**

through recognizable equipment, architecture and battlefield geography.

It should communicate:

**stylized strategy game**

through proportions, color, shape simplification and exaggerated effects.

It should communicate:

**premium illustrated world**

through painterly terrain, environmental storytelling and cohesive art direction.

And it should communicate:

**tower defense**

before any of those things, through unmistakable routes, towers, enemies, build locations and combat information.

The final visual equation is:

> **Kingdom Rush-level readability and exaggeration  
> + grounded WWII equipment identity  
> + Commandos-like environmental storytelling  
> + painterly storybook terrain  
> + restrained war-table interface framing  
> = Fronts of War**

The controlling rule for every artist, asset and map is:

> **Make reality simpler, stronger and more readable—not merely more cartoonish.**