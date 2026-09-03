# WW2 Tower Defense - Unity Transition Guide

This document summarizes the Phaser.js prototype and provides guidance for recreating it in Unity.

---

## ✅ What We Built in Phaser (Prototype Complete)

### Core Systems Implemented
- ✅ **Isometric grid system** with coordinate conversion
- ✅ **Tower placement** with validation and economy
- ✅ **Enemy pathfinding** along waypoints
- ✅ **Wave spawning** system with JSON configurations
- ✅ **Combat system** (towers shoot, enemies take damage)
- ✅ **Economy manager** (money, costs, rewards)
- ✅ **Game manager** (lives, waves, win/loss)
- ✅ **Audio system** (placeholder SFX)
- ✅ **Visual effects** (explosions, camera effects)
- ✅ **Terrain rendering** (themed maps with decorations)

### Content Created
- 🏰 **5 British Towers** (Rifleman, MG, Anti-Tank, Mortar, Artillery)
- 👾 **3 Axis Enemies** (German Infantry, Italian Light Tank, Japanese Heavy Tank)
- 🗺️ **2 Maps** (Normandy Beach, North Africa)
- 📊 **5 Wave configurations** per map
- 🎨 **Visual themes** (Normandy green, Desert tan)

### Reference Documents
- 📚 **COUNTRIES_AND_TOWERS.md** - Complete design for 6 nations
- 📚 **ENEMIES_REFERENCE.md** - Full enemy roster with special abilities
- 📚 **This file** - Transition guide

---

## 🎮 Unity Recreation Checklist

### Project Setup
- [ ] Create new Unity 2D project
- [ ] Set up isometric camera angle
- [ ] Configure layers (Background, Terrain, Path, Towers, Enemies, Projectiles, UI)
- [ ] Set up sorting layers for proper rendering

### Core Systems to Recreate

#### 1. Grid System
```csharp
// Key conversion formulas (from Phaser GridSystem.ts)
// gridToScreen: x = (gridX - gridY) * (tileWidth / 2), y = (gridX + gridY) * (tileHeight / 2)
// screenToGrid: Use inverse matrix transformation

public class GridSystem : MonoBehaviour
{
    public int gridWidth = 20;
    public int gridHeight = 12;
    public float tileWidth = 64f;
    public float tileHeight = 32f;

    public Vector3 GridToWorld(int gridX, int gridY)
    {
        float x = (gridX - gridY) * (tileWidth / 2f);
        float y = -(gridX + gridY) * (tileHeight / 2f); // Negative for Unity's Y-up
        return new Vector3(x, y, 0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int gridX = Mathf.FloorToInt((worldPos.x / (tileWidth/2f) + worldPos.y / -(tileHeight/2f)) / 2f);
        int gridY = Mathf.FloorToInt((worldPos.y / -(tileHeight/2f) - worldPos.x / (tileWidth/2f)) / 2f);
        return new Vector2Int(gridX, gridY);
    }
}
```

#### 2. Tower System
```csharp
public abstract class Tower : MonoBehaviour
{
    public TowerData data; // ScriptableObject with stats
    public float damage;
    public float range;
    public float fireRate;
    protected Enemy currentTarget;
    protected float lastFireTime;

    void Update()
    {
        AcquireTarget();
        if (currentTarget != null && Time.time - lastFireTime >= 1f / fireRate)
        {
            Fire();
            lastFireTime = Time.time;
        }
    }

    void AcquireTarget()
    {
        // Find enemies in range using Physics2D.OverlapCircle
    }

    void Fire()
    {
        // Instantiate projectile
    }
}
```

#### 3. Enemy System
```csharp
public class Enemy : MonoBehaviour
{
    public EnemyData data;
    public float currentHealth;
    public float speed;
    private Vector3[] path;
    private int currentWaypoint = 0;

    void Update()
    {
        FollowPath();
    }

    void FollowPath()
    {
        if (currentWaypoint >= path.Length) return;

        Vector3 target = path[currentWaypoint];
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
            currentWaypoint++;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }
}
```

#### 4. Wave System
```csharp
public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveData
    {
        public List<EnemySpawn> spawns;
    }

    [System.Serializable]
    public class EnemySpawn
    {
        public GameObject enemyPrefab;
        public int count;
        public float interval;
        public float delay;
    }

    public List<WaveData> waves;

    IEnumerator SpawnWave(WaveData wave)
    {
        foreach (var spawn in wave.spawns)
        {
            yield return new WaitForSeconds(spawn.delay);

            for (int i = 0; i < spawn.count; i++)
            {
                Instantiate(spawn.enemyPrefab, spawnPoint.position, Quaternion.identity);
                yield return new WaitForSeconds(spawn.interval);
            }
        }
    }
}
```

### ScriptableObjects to Create

#### TowerData.cs
```csharp
[CreateAssetMenu(fileName = "TowerData", menuName = "TD/Tower")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public TowerType type;
    public Nation nation;
    public int cost;
    public float damage;
    public float range;
    public float fireRate;
    public ProjectileType projectileType;
    public Sprite icon;
    public GameObject prefab;
}
```

#### EnemyData.cs
```csharp
[CreateAssetMenu(fileName = "EnemyData", menuName = "TD/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Nation nation;
    public float health;
    public float speed;
    public float armor;
    public int reward;
    public bool isArmored;
    public Sprite sprite;
}
```

### UI System
Use Unity's UI Toolkit or Canvas system:
- **HUD** - Health, money, wave counter
- **Tower Selection Panel** - Buttons with costs and icons
- **Tower Info Panel** - Stats and upgrade options
- **Game Over Screen** - Victory/defeat with stats

### Visual Assets Needed

#### Sprites to Create
1. **Terrain Tiles** (64x32 isometric)
   - Grass tile
   - Sand tile
   - Dirt path tile
   - Stone tile

2. **Tower Sprites** (5 British towers)
   - Rifleman emplacement
   - Machine gun nest
   - Anti-tank gun
   - Mortar pit
   - Artillery battery

3. **Enemy Sprites**
   - German infantry
   - Italian light tank
   - Japanese heavy tank
   (+ future: all enemies from ENEMIES_REFERENCE.md)

4. **Decorations**
   - Trees
   - Rocks
   - Bushes
   - Craters
   - Ruins
   - Sandbags

5. **Projectiles**
   - Bullets (small)
   - Shells (medium)
   - Rockets (large with trail)

6. **Effects**
   - Explosion sprites/animations
   - Muzzle flash
   - Impact effect

### Audio Assets Needed
- **SFX:**
  - Tower placement
  - Rifle shot
  - Machine gun burst
  - Tank cannon
  - Artillery boom
  - Enemy death
  - Explosion
  - Wave start/complete
  - Button click

- **Music:**
  - Main menu theme
  - Battle theme (looping)
  - Victory fanfare
  - Defeat theme

---

## 📊 Data Files (JSON → ScriptableObjects)

### Tower Stats (from towers.json)
Create ScriptableObject instances for each:

| Tower | Cost | Damage | Range | Fire Rate | Type |
|-------|------|--------|-------|-----------|------|
| Rifleman | 100 | 12 | 160 | 0.9/s | Bullet |
| Machine Gun | 150 | 6 | 140 | 3.5/s | Bullet |
| Anti-Tank | 200 | 45 | 190 | 0.45/s | Shell |
| Mortar | 250 | 30 | 220 | 0.6/s | Shell |
| Artillery | 300 | 70 | 280 | 0.25/s | Shell |

### Enemy Stats (from enemies.json)

| Enemy | HP | Speed | Armor | Reward | Armored |
|-------|----|----|-------|--------|---------|
| German Infantry | 50 | 50 | 0 | $10 | No |
| Italian Light Tank | 100 | 40 | 5 | $20 | Yes |
| Japanese Heavy Tank | 200 | 25 | 10 | $40 | Yes |

### Map Data
Convert map JSONs to Unity Scenes:
- **Normandy Beach** - $500, 20 lives, grass theme
- **North Africa** - $450, 15 lives, desert theme

Path waypoints can be stored as Transform[] arrays.

---

## 🎯 Unity Advantages You'll Gain

### Built-in Features
- ✅ **Sprite Renderer** - Easy 2D graphics
- ✅ **Animator** - Tower and enemy animations
- ✅ **Particle System** - Explosions, smoke, muzzle flashes
- ✅ **Audio System** - Better sound management
- ✅ **Physics2D** - Circle cast for tower range detection
- ✅ **Tilemap** - Can use for terrain (optional)
- ✅ **Cinemachine** - Camera shake, zoom, effects
- ✅ **Timeline** - Cutscenes, intro/outro

### Asset Store Resources
- Isometric tile packs
- WW2 sprite assets
- UI kits
- Sound effect libraries
- Particle effect packs

### Steamworks Integration
- Unity has official Steamworks.NET plugin
- Easier achievement system
- Cloud saves
- Steam overlay
- Workshop support (future modding?)

---

## 🔄 Migration Priority

### Phase 1: Core Gameplay (Week 1-2)
1. Set up grid system
2. Implement tower placement
3. Enemy movement along path
4. Basic combat (shooting and damage)
5. Economy system

### Phase 2: Content (Week 3-4)
1. Create/import all tower sprites
2. Create/import all enemy sprites
3. Implement all 5 tower types
4. Implement all 3 enemy types
5. Set up both maps

### Phase 3: Polish (Week 5-6)
1. Add particle effects
2. Implement proper audio
3. Create UI screens
4. Add animations
5. Visual effects (explosions, impacts)

### Phase 4: Advanced Features (Week 7-8)
1. Tower upgrade system
2. More enemy types (flying, stealth, rush)
3. Additional maps
4. Country selection system
5. Save/load system

### Phase 5: Steam Release (Week 9-10)
1. Steamworks integration
2. Achievements
3. Cloud saves
4. Build for Windows
5. Store page assets

---

## 💡 Unity-Specific Tips

### Isometric Setup
```csharp
// Camera setup for isometric view
Camera.main.orthographic = true;
Camera.main.orthographicSize = 5;
Camera.main.transform.rotation = Quaternion.Euler(30, 45, 0); // Optional 3D iso
// OR keep 2D with isometric sprite sorting
```

### Sorting Layers (Bottom to Top)
1. Background
2. Terrain
3. Decorations
4. Path
5. Towers
6. Enemies
7. Projectiles
8. Effects
9. UI

### Performance Tips
- Use **object pooling** for projectiles and enemies
- Batch sprite rendering with **sprite atlases**
- Use **culling** for off-screen objects
- Keep particle count reasonable (<200 active)

### Recommended Packages
- **Cinemachine** - Camera control
- **TextMeshPro** - Better text rendering
- **Post Processing** - Visual effects
- **DOTween** - Smooth animations
- **Steamworks.NET** - Steam integration

---

## 📋 Quick Reference: Key Algorithms

### Isometric Conversions
```
// Grid to Screen
screenX = (gridX - gridY) * (tileWidth / 2)
screenY = (gridX + gridY) * (tileHeight / 2)

// Screen to Grid
gridX = floor((screenX / (tileWidth/2) + screenY / (tileHeight/2)) / 2)
gridY = floor((screenY / (tileHeight/2) - screenX / (tileWidth/2)) / 2)
```

### Targeting Priority (from TargetingSystem.ts)
- **First**: Closest to path end
- **Last**: Furthest from path end
- **Closest**: Nearest to tower
- **Strongest**: Most HP
- **Weakest**: Least HP

### Wave Difficulty Scaling
- Early waves: Infantry only
- Mid waves: Mixed infantry + light armor
- Late waves: Heavy tanks + special units
- Boss waves: Mass rushes or heavy tanks

---

## 🎮 What's Already Designed (Just Needs Assets)

### Complete Design Documents
- ✅ **6 Countries** with unique tower rosters (see COUNTRIES_AND_TOWERS.md)
- ✅ **60+ Towers** across all nations (10 per nation)
- ✅ **20+ Enemy Types** for both alliances (see ENEMIES_REFERENCE.md)
- ✅ **Unique Tower Mechanics** for each nation
- ✅ **Special Enemy Abilities** (stealth, flying, rush, etc.)

### Balanced Gameplay
- ✅ Tower costs and stats
- ✅ Enemy HP and armor values
- ✅ Wave compositions
- ✅ Economy (starting money, rewards)
- ✅ Difficulty curve

You have a **complete game design** - just need to implement in Unity with proper assets!

---

## 📦 What to Keep from Phaser Project

### Keep These Files as Reference
- `COUNTRIES_AND_TOWERS.md` - Tower designs
- `ENEMIES_REFERENCE.md` - Enemy designs
- `src/data/towers.json` - Tower stats
- `src/data/enemies.json` - Enemy stats
- `src/data/maps/*.json` - Map layouts
- `src/data/waves/*.json` - Wave configurations

### Code Logic to Port
- Grid conversion math (GridSystem.ts)
- Targeting algorithms (TargetingSystem.ts)
- Wave spawning logic (WaveSystem.ts)
- Tower/Enemy base class structure

---

## 🚀 Getting Started in Unity

1. **Create Project**
   ```
   Unity Hub → New Project → 2D
   Project Name: WW2 Tower Defense
   ```

2. **First Steps**
   - Create GridSystem script
   - Test isometric coordinate conversion
   - Create a simple tower prefab
   - Test tower placement on grid
   - Add a simple enemy that follows a path
   - Implement basic shooting

3. **Asset Creation**
   - Start with placeholder sprites (colored shapes)
   - OR find WW2 isometric asset pack on Asset Store
   - Gradually replace with final art

4. **Reference This Prototype**
   - The Phaser version proves all systems work
   - Copy the logic, adapt to Unity's component system
   - Use the JSON data files for balancing

---

## ✨ Final Notes

This Phaser prototype successfully proved:
- ✅ The game concept is fun
- ✅ All core systems work together
- ✅ The isometric grid system functions correctly
- ✅ Tower defense mechanics are solid
- ✅ Visual style is appealing

Unity will give you:
- 🎨 Better visual fidelity
- 🔊 Proper audio system
- ⚡ Better performance
- 🎮 Easier Steam deployment
- 🛠️ Professional development tools

**Good luck with the Unity version! You have a solid foundation to build from.**

---

*Last Updated: Phase 7 Complete*
*Phaser Prototype: Fully Playable*
*Ready for Unity Migration*
