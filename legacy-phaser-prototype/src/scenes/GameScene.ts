/**
 * GameScene - Main gameplay scene (Phase 4 - Full Version)
 * Integrates all managers, systems, and entities for complete gameplay
 */

import Phaser from 'phaser';
import { SceneKey, TowerType } from '../types/Enums';
import { GAME_CONSTANTS } from '../config/Constants';
import { MapDefinition, WaveDefinition } from '../types/GameTypes';
import GridSystem from '../systems/GridSystem';
import TerrainRenderer from '../systems/TerrainRenderer';
import GameManager from '../managers/GameManager';
import EconomyManager from '../managers/EconomyManager';
import TowerManager from '../managers/TowerManager';
import AudioManager from '../managers/AudioManager';
import WaveSystem from '../systems/WaveSystem';
import Enemy from '../entities/Enemy';
import Projectile from '../entities/Projectile';

export default class GameScene extends Phaser.Scene {
  // Systems
  private gridSystem!: GridSystem;
  private terrainRenderer!: TerrainRenderer;
  private waveSystem!: WaveSystem;

  // Managers
  private gameManager!: GameManager;
  private economyManager!: EconomyManager;
  private towerManager!: TowerManager;
  private audioManager!: AudioManager;

  // Game data
  private mapData!: MapDefinition;
  private waveData!: WaveDefinition[];

  // Entity collections
  private enemies: Enemy[] = [];
  private projectiles: Projectile[] = [];

  // UI state
  private selectedTowerType: TowerType | null = null;
  private towerPreview?: Phaser.GameObjects.Graphics;

  constructor() {
    super({ key: SceneKey.GAME });
  }

  /**
   * Initialize scene with map data
   */
  init(_data?: { mapId?: string }): void {
    console.log('GameScene: Initializing full gameplay...');

    // Load map data (for now, hardcoded map01)
    // In later phases, this will load from JSON based on data.mapId
    this.mapData = {
      id: 'normandy_beach',
      name: 'Normandy Beach',
      gridWidth: 20,
      gridHeight: 12,
      startingMoney: 500,
      startingLives: 20,
      path: [
        { x: 0, y: 6 },
        { x: 4, y: 6 },
        { x: 4, y: 3 },
        { x: 8, y: 3 },
        { x: 8, y: 8 },
        { x: 12, y: 8 },
        { x: 12, y: 5 },
        { x: 16, y: 5 },
        { x: 16, y: 9 },
        { x: 19, y: 9 }
      ]
    };

    // Load wave data (hardcoded for now)
    this.waveData = [
      {
        waveNumber: 1,
        spawns: [
          { enemyType: 'german_infantry' as any, count: 10, interval: 1000, delay: 500 }
        ],
        reward: 50
      },
      {
        waveNumber: 2,
        spawns: [
          { enemyType: 'german_infantry' as any, count: 15, interval: 800, delay: 500 }
        ],
        reward: 75
      },
      {
        waveNumber: 3,
        spawns: [
          { enemyType: 'german_infantry' as any, count: 8, interval: 1000, delay: 500 },
          { enemyType: 'italian_lighttank' as any, count: 3, interval: 2000, delay: 3000 }
        ],
        reward: 100
      }
    ];
  }

  /**
   * Create scene objects
   */
  create(): void {
    console.log('GameScene: Creating full gameplay scene...');

    // Set background based on map theme
    const bgColor = this.mapData.id === 'normandy_beach' ? '#3a5a2a' : '#d4a574';
    this.cameras.main.setBackgroundColor(bgColor);

    // Calculate grid offset to center it
    const offsetX = GAME_CONSTANTS.CANVAS_WIDTH / 2 - 100;
    const offsetY = 150;

    // Initialize GridSystem
    this.gridSystem = new GridSystem(
      this,
      this.mapData.gridWidth,
      this.mapData.gridHeight,
      offsetX,
      offsetY
    );

    // Initialize TerrainRenderer
    this.terrainRenderer = new TerrainRenderer(this, this.gridSystem);

    // Render terrain based on map theme
    const mapTheme = this.mapData.id === 'normandy_beach' ? 'normandy' : 'desert';
    this.terrainRenderer.renderTerrain(
      mapTheme,
      this.mapData.path,
      this.mapData.gridWidth,
      this.mapData.gridHeight
    );

    // Block path tiles
    this.mapData.path.forEach(waypoint => {
      this.gridSystem.blockTile(waypoint.x, waypoint.y);
    });

    // Initialize managers
    this.gameManager = new GameManager(this, this.mapData.startingLives);
    this.economyManager = new EconomyManager(this, this.mapData.startingMoney);
    this.towerManager = new TowerManager(this, this.gridSystem, this.economyManager);
    this.audioManager = new AudioManager(this);

    // Path manager not needed after initialization (path data used by wave system)

    // Initialize WaveSystem
    this.waveSystem = new WaveSystem(
      this,
      this.gridSystem,
      this.mapData.path,
      this.waveData
    );

    // Set up event listeners
    this.setupEventListeners();

    // Set up input
    this.setupInput();

    // Create UI
    this.createUI();

    // Emit scene ready
    this.events.emit('scene-ready');

    console.log('GameScene: Ready! Click "Start Wave" to begin.');
  }

  /**
   * Set up event listeners
   */
  private setupEventListeners(): void {
    // Enemy events
    this.events.on('enemy-spawned', this.onEnemySpawned, this);
    this.events.on('enemy-killed', this.onEnemyKilled, this);

    // Projectile events
    this.events.on('projectile-fired', this.onProjectileFired, this);

    // Wave events
    this.events.on('wave-reward', this.onWaveReward, this);
    this.events.on('wave-started', this.onWaveStarted, this);
    this.events.on('wave-completed', this.onWaveCompleted, this);

    // Tower events
    this.events.on('tower-placed', this.onTowerPlaced, this);

    // Game events
    this.events.on('game-over', this.onGameOver, this);
  }

  /**
   * Set up input handlers
   */
  private setupInput(): void {
    // Mouse move for tower preview
    this.input.on('pointermove', this.onPointerMove, this);

    // Mouse click for tower placement
    this.input.on('pointerdown', this.onPointerDown, this);

    // Keyboard shortcuts
    const keyboard = this.input.keyboard;
    if (keyboard) {
      // Number keys to select towers
      keyboard.on('keydown-ONE', () => this.selectTowerType(TowerType.BRITISH_RIFLEMAN));
      keyboard.on('keydown-TWO', () => this.selectTowerType(TowerType.BRITISH_MACHINEGUN));
      keyboard.on('keydown-THREE', () => this.selectTowerType(TowerType.BRITISH_ANTITANK));
      keyboard.on('keydown-FOUR', () => this.selectTowerType(TowerType.BRITISH_MORTAR));
      keyboard.on('keydown-FIVE', () => this.selectTowerType(TowerType.BRITISH_ARTILLERY));

      // ESC to cancel selection
      keyboard.on('keydown-ESC', () => this.cancelTowerSelection());

      // SPACE to start wave
      keyboard.on('keydown-SPACE', () => this.startNextWave());

      // ESC to pause (or cancel tower selection)
      keyboard.on('keydown-ESC', () => {
        if (this.selectedTowerType) {
          this.cancelTowerSelection();
        } else {
          this.pauseGame();
        }
      });
    }
  }

  /**
   * Pause the game
   */
  private pauseGame(): void {
    this.scene.pause();
    this.scene.launch(SceneKey.PAUSE);
  }

  /**
   * Create UI elements
   */
  private createUI(): void {
    const uiY = 30;

    // Title
    this.add.text(GAME_CONSTANTS.CANVAS_WIDTH / 2, uiY, this.mapData.name, {
      fontFamily: 'Arial',
      fontSize: '24px',
      color: '#ffffff',
      fontStyle: 'bold'
    }).setOrigin(0.5).setDepth(100);

    // Money display
    const moneyText = this.add.text(20, uiY, '', {
      fontFamily: 'Arial',
      fontSize: '18px',
      color: '#f1c40f',
      backgroundColor: '#000000',
      padding: { x: 10, y: 5 }
    }).setDepth(100);

    // Lives display
    const livesText = this.add.text(20, uiY + 35, '', {
      fontFamily: 'Arial',
      fontSize: '18px',
      color: '#e74c3c',
      backgroundColor: '#000000',
      padding: { x: 10, y: 5 }
    }).setDepth(100);

    // Wave display
    const waveText = this.add.text(20, uiY + 70, '', {
      fontFamily: 'Arial',
      fontSize: '18px',
      color: '#3498db',
      backgroundColor: '#000000',
      padding: { x: 10, y: 5 }
    }).setDepth(100);

    // Update displays
    const updateUI = () => {
      moneyText.setText(`💰 Money: $${this.economyManager.getMoney()}`);
      livesText.setText(`❤️ Lives: ${this.gameManager.getLives()}`);
      waveText.setText(`🌊 Wave: ${this.gameManager.getCurrentWave()}/${this.waveSystem.getTotalWaves()}`);
    };

    updateUI();
    this.events.on('money-changed', updateUI);
    this.events.on('lives-changed', updateUI);
    this.events.on('wave-started', updateUI);

    // Tower selection buttons
    const buttonY = GAME_CONSTANTS.CANVAS_HEIGHT - 120;
    const buttonSpacing = 128;
    const startX = 30;

    this.createTowerButton(startX, buttonY, '1: Rifleman\n$100', TowerType.BRITISH_RIFLEMAN, 0x8b4513);
    this.createTowerButton(startX + buttonSpacing, buttonY, '2: MG Nest\n$150', TowerType.BRITISH_MACHINEGUN, 0xa0522d);
    this.createTowerButton(startX + buttonSpacing * 2, buttonY, '3: Anti-Tank\n$200', TowerType.BRITISH_ANTITANK, 0xcd853f);
    this.createTowerButton(startX + buttonSpacing * 3, buttonY, '4: Mortar\n$250', TowerType.BRITISH_MORTAR, 0xdaa520);
    this.createTowerButton(startX + buttonSpacing * 4, buttonY, '5: Artillery\n$300', TowerType.BRITISH_ARTILLERY, 0xb8860b);

    // Start Wave button
    const startWaveBtn = this.add.text(
      GAME_CONSTANTS.CANVAS_WIDTH - 150,
      GAME_CONSTANTS.CANVAS_HEIGHT - 60,
      'Start Wave\n(SPACE)',
      {
        fontFamily: 'Arial',
        fontSize: '16px',
        color: '#ffffff',
        backgroundColor: '#27ae60',
        padding: { x: 15, y: 10 },
        align: 'center'
      }
    ).setOrigin(0.5).setDepth(100).setInteractive();

    startWaveBtn.on('pointerdown', () => this.startNextWave());
    startWaveBtn.on('pointerover', () => startWaveBtn.setScale(1.05));
    startWaveBtn.on('pointerout', () => startWaveBtn.setScale(1));

    // Instructions
    this.add.text(
      GAME_CONSTANTS.CANVAS_WIDTH / 2,
      GAME_CONSTANTS.CANVAS_HEIGHT - 15,
      'Select tower (1-5) | Click to place | ESC to cancel | SPACE to start wave',
      {
        fontFamily: 'Arial',
        fontSize: '12px',
        color: '#95a5a6',
        align: 'center'
      }
    ).setOrigin(0.5).setDepth(100);
  }

  /**
   * Create tower selection button
   */
  private createTowerButton(
    x: number,
    y: number,
    label: string,
    towerType: TowerType,
    color: number
  ): void {
    const button = this.add.text(x, y, label, {
      fontFamily: 'Arial',
      fontSize: '14px',
      color: '#ffffff',
      backgroundColor: Phaser.Display.Color.ValueToColor(color).darken(30).color.toString(16),
      padding: { x: 10, y: 8 },
      align: 'center'
    }).setOrigin(0.5).setDepth(100).setInteractive();

    button.on('pointerdown', () => this.selectTowerType(towerType));
    button.on('pointerover', () => button.setScale(1.05));
    button.on('pointerout', () => button.setScale(1));
  }

  /**
   * Select tower type for placement
   */
  private selectTowerType(towerType: TowerType): void {
    this.selectedTowerType = towerType;
    console.log(`Selected tower: ${towerType}`);
  }

  /**
   * Cancel tower selection
   */
  private cancelTowerSelection(): void {
    this.selectedTowerType = null;
    if (this.towerPreview) {
      this.towerPreview.destroy();
      this.towerPreview = undefined;
    }
    this.gridSystem.clearHighlight();
  }

  /**
   * Start next wave
   */
  private startNextWave(): void {
    if (this.waveSystem.isWaveActive()) {
      console.log('Wave already in progress');
      return;
    }

    this.waveSystem.startNextWave();
    this.gameManager.startWave();
  }

  /**
   * Handle pointer move
   */
  private onPointerMove(pointer: Phaser.Input.Pointer): void {
    if (!this.selectedTowerType) {
      this.gridSystem.clearHighlight();
      return;
    }

    const gridPos = this.gridSystem.screenToGrid(pointer.x, pointer.y);

    if (this.gridSystem.isValidGridPosition(gridPos.x, gridPos.y)) {
      const canPlace = this.towerManager.canPlaceTower(gridPos.x, gridPos.y);
      this.gridSystem.highlightTile(gridPos.x, gridPos.y, canPlace);
    } else {
      this.gridSystem.clearHighlight();
    }
  }

  /**
   * Handle pointer down (tower placement)
   */
  private onPointerDown(pointer: Phaser.Input.Pointer): void {
    if (!this.selectedTowerType) return;

    const gridPos = this.gridSystem.screenToGrid(pointer.x, pointer.y);

    if (this.towerManager.placeTower(gridPos.x, gridPos.y, this.selectedTowerType)) {
      console.log('Tower placed successfully!');
    }
  }

  /**
   * Handle enemy spawned
   */
  private onEnemySpawned(enemy: Enemy): void {
    this.enemies.push(enemy);
  }

  /**
   * Handle enemy killed
   */
  private onEnemyKilled(enemy: Enemy): void {
    // Remove from list
    const index = this.enemies.indexOf(enemy);
    if (index > -1) {
      this.enemies.splice(index, 1);
    }

    // Award money
    this.economyManager.addMoney(enemy.getReward());

    // Play sound
    this.audioManager.playSfx('enemy_death');

    // Create explosion effect
    this.createExplosion(enemy.x, enemy.y, 0xff6600);
  }

  /**
   * Handle projectile fired
   */
  private onProjectileFired(projectile: Projectile): void {
    this.projectiles.push(projectile);
    this.audioManager.playSfx('projectile_fire', 0.3);
  }

  /**
   * Handle tower placed
   */
  private onTowerPlaced(): void {
    this.audioManager.playSfx('tower_place');
  }

  /**
   * Handle wave started
   */
  private onWaveStarted(): void {
    this.audioManager.playSfx('wave_start');

    // Flash effect
    this.cameras.main.flash(200, 100, 150, 255);
  }

  /**
   * Handle wave completed
   */
  private onWaveCompleted(): void {
    this.audioManager.playSfx('wave_complete');

    // Flash effect
    this.cameras.main.flash(300, 50, 255, 50);
  }

  /**
   * Handle wave reward
   */
  private onWaveReward(reward: number): void {
    this.economyManager.addMoney(reward);
    console.log(`Wave bonus: $${reward}`);
  }

  /**
   * Handle game over
   */
  private onGameOver(victory: boolean): void {
    const message = victory ? 'VICTORY!' : 'DEFEAT!';
    const color = victory ? '#27ae60' : '#e74c3c';

    // Play sound
    this.audioManager.playSfx('game_over');

    // Camera effect
    if (victory) {
      this.cameras.main.flash(500, 50, 255, 50);
    } else {
      this.cameras.main.shake(500, 0.01);
      this.cameras.main.fade(500, 100, 0, 0);
    }

    // Create overlay
    this.time.delayedCall(300, () => {
      this.add.rectangle(
        GAME_CONSTANTS.CANVAS_WIDTH / 2,
        GAME_CONSTANTS.CANVAS_HEIGHT / 2,
        GAME_CONSTANTS.CANVAS_WIDTH,
        GAME_CONSTANTS.CANVAS_HEIGHT,
        0x000000,
        0.8
      ).setDepth(199);

      const titleText = this.add.text(
        GAME_CONSTANTS.CANVAS_WIDTH / 2,
        GAME_CONSTANTS.CANVAS_HEIGHT / 2 - 60,
        message,
        {
          fontFamily: 'Arial',
          fontSize: '72px',
          color: color,
          fontStyle: 'bold',
          stroke: '#000000',
          strokeThickness: 8
        }
      ).setOrigin(0.5).setDepth(200).setAlpha(0);

      // Animate title
      this.tweens.add({
        targets: titleText,
        alpha: 1,
        scale: { from: 0.5, to: 1 },
        duration: 500,
        ease: 'Back.out'
      });

      // Add stats
      const statsText = this.add.text(
        GAME_CONSTANTS.CANVAS_WIDTH / 2,
        GAME_CONSTANTS.CANVAS_HEIGHT / 2 + 40,
        `Wave ${this.gameManager.getCurrentWave()}\n` +
        `Money: $${this.economyManager.getMoney()}\n` +
        `Lives: ${this.gameManager.getLives()}`,
        {
          fontFamily: 'Arial',
          fontSize: '24px',
          color: '#ffffff',
          align: 'center'
        }
      ).setOrigin(0.5).setDepth(200).setAlpha(0);

      this.tweens.add({
        targets: statsText,
        alpha: 1,
        y: statsText.y + 20,
        duration: 400,
        delay: 300
      });

      // Return to menu button
      const menuButton = this.add.text(
        GAME_CONSTANTS.CANVAS_WIDTH / 2,
        GAME_CONSTANTS.CANVAS_HEIGHT - 100,
        'Return to Menu',
        {
          fontFamily: 'Arial',
          fontSize: '24px',
          color: '#ffffff',
          backgroundColor: '#3498db',
          padding: { x: 20, y: 10 }
        }
      ).setOrigin(0.5).setDepth(200).setInteractive().setAlpha(0);

      menuButton.on('pointerdown', () => {
        this.scene.stop();
        this.scene.start(SceneKey.MAIN_MENU);
      });

      menuButton.on('pointerover', () => {
        menuButton.setScale(1.1);
      });

      menuButton.on('pointerout', () => {
        menuButton.setScale(1);
      });

      this.tweens.add({
        targets: menuButton,
        alpha: 1,
        duration: 400,
        delay: 600
      });
    });

    // Stop the game
    this.scene.pause();
  }

  /**
   * Create explosion visual effect
   */
  private createExplosion(x: number, y: number, color: number): void {
    // Create particle burst
    const particles: Phaser.GameObjects.Arc[] = [];
    const particleCount = 12;

    for (let i = 0; i < particleCount; i++) {
      const angle = (Math.PI * 2 * i) / particleCount;
      const speed = 50 + Math.random() * 50;
      const particle = this.add.circle(x, y, 3, color).setDepth(50);
      particles.push(particle);

      this.tweens.add({
        targets: particle,
        x: x + Math.cos(angle) * speed,
        y: y + Math.sin(angle) * speed,
        alpha: 0,
        scale: 0,
        duration: 300 + Math.random() * 200,
        onComplete: () => particle.destroy()
      });
    }

    // Create flash
    const flash = this.add.circle(x, y, 5, 0xffffff, 0.8).setDepth(51);
    this.tweens.add({
      targets: flash,
      scale: 3,
      alpha: 0,
      duration: 200,
      onComplete: () => flash.destroy()
    });
  }

  /**
   * Update loop
   */
  update(time: number, delta: number): void {
    // Update wave system
    this.waveSystem.update(time, delta, this.enemies);

    // Update towers
    this.towerManager.update(time, delta, this.enemies);

    // Update enemies
    this.enemies.forEach(enemy => {
      if (enemy.active) {
        enemy.update(time, delta);
      }
    });

    // Update projectiles
    this.projectiles.forEach(projectile => {
      if (projectile.active) {
        projectile.update(time, delta);
      }
    });

    // Clean up destroyed projectiles
    this.projectiles = this.projectiles.filter(p => p.active);
  }

  /**
   * Cleanup
   */
  shutdown(): void {
    // Clean up managers
    if (this.gameManager) {
      this.gameManager.destroy();
    }

    // Clean up systems
    if (this.gridSystem) {
      this.gridSystem.destroy();
    }
    if (this.terrainRenderer) {
      this.terrainRenderer.destroy();
    }

    // Clear arrays
    this.enemies = [];
    this.projectiles = [];

    // Remove event listeners
    this.events.off('enemy-spawned', this.onEnemySpawned, this);
    this.events.off('enemy-killed', this.onEnemyKilled, this);
    this.events.off('projectile-fired', this.onProjectileFired, this);
    this.events.off('wave-reward', this.onWaveReward, this);
    this.events.off('game-over', this.onGameOver, this);
  }
}
