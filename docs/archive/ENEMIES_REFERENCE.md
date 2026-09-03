# WW2 Tower Defense - Enemy Reference

This document outlines all enemy types for both Allied and Axis forces. Enemies are selected based on the player's alliance choice - if you play as Allies, you face Axis enemies, and vice versa.

---

## 🎯 Enemy Design Philosophy

### Enemy Roles
- **Basic Infantry** - Low HP, moderate speed, swarm units
- **Elite Infantry** - Higher HP or special abilities
- **Armored Infantry** - Moderate HP with armor, resistant to bullets
- **Light Vehicles** - Fast movement, moderate HP
- **Heavy Tanks** - Very high HP, slow, heavily armored
- **Special Units** - Unique mechanics (stealth, flying, rushing)
- **Support Units** - High HP but non-threatening (convoys, supplies)

### Balance Considerations
- Each faction should have similar enemy variety
- Special abilities should create interesting tower placement decisions
- Flying enemies require specialized anti-air towers
- Swarm units punish slow-firing towers
- Stealth units require detection or fast-tracking capabilities

---

## ⚔️ Axis Enemies (When Player Chooses Allies)

These enemies appear when the player selects Britain, USA, or Soviet Union.

### Basic Infantry

#### **Wehrmacht Infantry**
- **Type:** Basic Infantry
- **HP:** 50
- **Speed:** 50
- **Armor:** 0
- **Reward:** $10
- **Description:** Standard German soldiers - backbone of the Wehrmacht
- **Current Status:** ✅ **Fully Implemented**

#### **Italian Riflemen**
- **Type:** Basic Infantry
- **HP:** 40
- **Speed:** 55
- **Armor:** 0
- **Reward:** $8
- **Description:** Italian infantry units - slightly weaker but faster
- **Special:** Move in larger groups

#### **Japanese Infantry Platoon**
- **Type:** Basic Infantry
- **HP:** 45
- **Speed:** 52
- **Armor:** 0
- **Reward:** $9
- **Description:** Japanese soldiers in platoon formation
- **Special:** Slight damage resistance when in groups

### Elite & Special Infantry

#### **Panzergrenadier Squad**
- **Type:** Armored Infantry
- **HP:** 80
- **Speed:** 45
- **Armor:** 5
- **Reward:** $15
- **Description:** Mechanized German infantry with better equipment
- **Special:** Resistant to small arms fire

#### **German Stormtroopers**
- **Type:** Elite Fast Attacker
- **HP:** 70
- **Speed:** 70
- **Armor:** 2
- **Reward:** $20
- **Description:** Elite assault troops that move quickly
- **Special:** Sprint ability - periodically moves at double speed

#### **Japanese Banzai Charge Unit** 🌟
- **Type:** Swarm Rush Unit
- **HP:** 25
- **Speed:** 85
- **Armor:** 0
- **Reward:** $5 each
- **Description:** Fanatical warriors that charge en masse
- **Special Mechanic:**
  - Extremely fast movement with low health
  - Appears in large numbers (15-20 at once)
  - Overwhelms slow-firing towers
  - Forces players to build rapid-fire defenses
- **Counter:** Machine guns, high fire rate towers

### Armored Units

#### **Italian Armored Car**
- **Type:** Fast Vehicle
- **HP:** 90
- **Speed:** 65
- **Armor:** 8
- **Reward:** $18
- **Description:** Light reconnaissance vehicle
- **Special:** Fast and maneuverable
- **Current Status:** Basis for **Italian Light Tank** ✅ **Implemented**

#### **Panzer IV Tank**
- **Type:** Medium Tank
- **HP:** 200
- **Speed:** 35
- **Armor:** 15
- **Reward:** $35
- **Description:** German medium tank - balanced armor and speed
- **Special:** Well-rounded threat

#### **Tiger Heavy Tank** 🌟
- **Type:** Heavy Tank Boss
- **HP:** 450
- **Speed:** 20
- **Armor:** 25
- **Reward:** $60
- **Description:** Heavily armored German super-tank
- **Special Mechanic:**
  - Massive HP pool
  - Very slow movement
  - Requires dedicated anti-tank towers
  - "Mini-boss" unit that anchors waves
- **Counter:** Anti-tank guns, artillery, focus fire
- **Current Status:** Basis for **Japanese Heavy Tank** ✅ **Implemented**

### Flying Units

#### **Luftwaffe Bomber Squadron** 🌟
- **Type:** Flying Enemy Wave
- **HP:** 100 per bomber
- **Speed:** 60
- **Armor:** 5
- **Reward:** $25 each
- **Description:** German bomber formation flying over the battlefield
- **Special Mechanic:**
  - Flies above ground path (ignores ground obstacles)
  - Can only be hit by anti-air capable towers
  - Appears in formations of 3-5 planes
  - Forces AA tower placement
- **Counter:** Bofors AA, dedicated AA positions

---

## 🦅 Allied Enemies (When Player Chooses Axis)

These enemies appear when the player selects Germany, Italy, or Japan.

### Basic Infantry

#### **US Rifle Squad**
- **Type:** Basic Infantry
- **HP:** 60
- **Speed:** 48
- **Armor:** 0
- **Reward:** $12
- **Description:** American infantry with M1 Garands - reliable and numerous
- **Special:** Slightly better HP than basic Axis infantry

#### **British Infantry Section**
- **Type:** Basic Infantry
- **HP:** 55
- **Speed:** 50
- **Armor:** 2
- **Reward:** $11
- **Description:** British soldiers with disciplined training
- **Special:** Minor armor from equipment

#### **Soviet Conscripts**
- **Type:** Basic Infantry Swarm
- **HP:** 40
- **Speed:** 45
- **Armor:** 0
- **Reward:** $7
- **Description:** Numerous Soviet infantry - quantity over quality
- **Special:** Appear in larger groups

### Elite & Special Infantry

#### **US Ranger Unit**
- **Type:** Elite Infantry
- **HP:** 100
- **Speed:** 52
- **Armor:** 3
- **Reward:** $20
- **Description:** Elite American special forces
- **Special:** High HP and equipment quality

#### **British Commando Team** 🌟
- **Type:** Fast Stealth Unit
- **HP:** 75
- **Speed:** 70
- **Armor:** 0
- **Reward:** $25
- **Description:** SAS commandos with stealth capabilities
- **Special Mechanic:**
  - Periodically becomes invisible/transparent
  - While cloaked, towers cannot target them
  - Forces players to build detection towers or high fire-rate towers that can catch them
  - Reveals when taking damage
- **Counter:** Radar towers, area-denial weapons, prediction

#### **Soviet Shock Troops**
- **Type:** Armored Infantry
- **HP:** 110
- **Speed:** 42
- **Armor:** 8
- **Reward:** $18
- **Description:** Heavily equipped Soviet assault infantry
- **Special:** High armor value for infantry

### Armored Units

#### **Sherman Tank Column**
- **Type:** Medium Armored Unit
- **HP:** 220
- **Speed:** 38
- **Armor:** 12
- **Reward:** $32
- **Description:** American tank formation
- **Special:** Moderate stats, appears in groups

#### **T-34 Assault Tank**
- **Type:** Fast Armored Unit
- **HP:** 180
- **Speed:** 50
- **Armor:** 10
- **Reward:** $28
- **Description:** Soviet tank with excellent mobility
- **Special:** Faster than typical tanks

### Support & Special Units

#### **Allied Supply Convoy** 🌟
- **Type:** High HP Support Unit
- **HP:** 300
- **Speed:** 25
- **Armor:** 5
- **Reward:** $40
- **Description:** Heavily armored supply trucks
- **Special Mechanic:**
  - Very high HP but slow
  - Non-threatening (doesn't reduce lives more than other enemies)
  - Acts as a "damage sponge" that protects other units
  - Rewards patient players who can focus fire
- **Counter:** Sustained fire, anti-tank weapons

#### **RAF Bomber Formation** 🌟
- **Type:** Flying Enemy Wave
- **HP:** 120 per bomber
- **Speed:** 55
- **Armor:** 8
- **Reward:** $30 each
- **Description:** British bomber squadrons
- **Special Mechanic:**
  - Similar to Luftwaffe, requires AA capability
  - Slightly tougher than Axis bombers
  - Appears in V-formations
- **Counter:** Anti-aircraft towers, focused AA positions

---

## 🎮 Implementation Status

### Currently Implemented (Phase 6)
- ✅ Wehrmacht Infantry (German)
- ✅ Italian Light Tank (basis for Italian Armored Car)
- ✅ Japanese Heavy Tank (basis for Tiger Tank)

### Priority for Next Phase
1. **Flying enemies** - Adds new dimension requiring AA towers
2. **Stealth units** - Creates detection tower requirement
3. **Rush/swarm units** - Tests rapid-fire tower placement
4. **Support units** - Adds strategic depth

---

## 🔧 Enemy Mechanics Reference

### Special Abilities

| Ability | Effect | Counter Strategy |
|---------|--------|------------------|
| **Stealth/Cloak** | Becomes untargetable periodically | Detection towers, area weapons |
| **Rush/Banzai** | Extreme speed, low HP, large numbers | Rapid-fire towers, overlapping coverage |
| **Flying** | Ignores ground path, immune to ground towers | Anti-aircraft towers |
| **Heavy Armor** | High armor value, slow | Anti-tank guns, focus fire |
| **Damage Sponge** | Very high HP, slow, protects others | Sustained DPS, armor piercing |
| **Sprint** | Periodic speed boost | Slowing towers, chokepoint defense |
| **Group Bonus** | Stronger when near allies | Area damage, separation |

### Enemy Spawn Patterns

#### **Wave Composition**
- **Early Waves (1-3):** Mostly basic infantry
- **Mid Waves (4-6):** Mix of infantry and light vehicles
- **Late Waves (7-10):** Heavy tanks, elite units, special mechanics
- **Boss Waves:** Tiger tanks, bomber formations, mass rushes

#### **Difficulty Scaling**
- Increase enemy count per wave
- Introduce special units progressively
- Mix unit types to require diverse tower strategies
- Add "boss" units that anchor dangerous waves

---

## 📊 Enemy Balance Guidelines

### HP Scaling
- **Basic Infantry:** 40-60 HP
- **Elite Infantry:** 70-110 HP
- **Light Vehicles:** 80-120 HP
- **Medium Tanks:** 180-250 HP
- **Heavy Tanks:** 350-500 HP
- **Flying Units:** 100-150 HP
- **Special Units:** Varies based on mechanic

### Speed Guidelines
- **Very Slow:** 20-30 (Heavy tanks)
- **Slow:** 31-45 (Armored infantry, medium tanks)
- **Normal:** 46-55 (Basic infantry, support)
- **Fast:** 56-70 (Light vehicles, elite units)
- **Very Fast:** 71-90 (Rush units, special mechanics)

### Armor Guidelines
- **No Armor:** 0 (Basic infantry)
- **Light Armor:** 1-5 (Elite infantry, light vehicles)
- **Medium Armor:** 6-15 (Medium tanks, armored infantry)
- **Heavy Armor:** 16-25 (Heavy tanks, fortified units)

### Reward Scaling
- Base reward = HP / 5
- +50% for special mechanics (stealth, flying)
- +25% for elite units
- +100% for boss units

---

## 🎯 Design Notes for Implementation

### When Adding New Enemies

1. **Balance HP vs Speed**
   - Faster units should have less HP
   - Slow units can have more HP

2. **Special Abilities Should...**
   - Create interesting tower placement decisions
   - Have clear counters
   - Not be frustrating to play against
   - Reward strategic planning

3. **Visual Clarity**
   - Different enemy types should be clearly distinguishable
   - Special units should have unique visual indicators
   - Flying units need altitude indication

4. **Sound Design**
   - Unique sounds for special abilities
   - Audio cues for dangerous units
   - Warning sounds for rush waves

### Future Enemy Ideas
- **Paratroopers** - Drop mid-path, skip early defenses
- **Sappers** - Can disable towers temporarily
- **Medical Units** - Heal nearby enemies
- **Artillery Spotters** - Call in off-map damage
- **Transport Planes** - Drop multiple enemies when destroyed

---

*This document will be updated as new enemy types are implemented.*

**Last Updated:** Phase 6 - 3 Axis enemy types implemented
**Next Update:** When flying, stealth, or rush units are added
