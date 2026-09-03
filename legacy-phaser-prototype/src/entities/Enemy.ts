/**
 * Enemy - Base class for all enemy types
 * Handles movement along path, health, and damage
 */

import Phaser from 'phaser';
import { ScreenPosition } from '../types/GameTypes';
import { EnemyType, Nation } from '../types/Enums';

export default class Enemy extends Phaser.GameObjects.Container {
  // Enemy properties
  protected enemyType: EnemyType;
  protected nation: Nation;

  // Stats
  protected maxHealth: number;
  protected currentHealth: number;
  protected speed: number;
  protected armor: number;
  protected reward: number;
  protected isArmored: boolean;

  // Movement
  protected pathScreenPositions: ScreenPosition[];
  protected currentPathIndex: number;
  protected pathProgress: number;

  // Visual
  protected enemySprite?: Phaser.GameObjects.Sprite;
  protected healthBar?: Phaser.GameObjects.Graphics;
  protected healthBarBg?: Phaser.GameObjects.Graphics;

  /**
   * Create a new Enemy
   */
  constructor(
    scene: Phaser.Scene,
    x: number,
    y: number,
    pathScreenPositions: ScreenPosition[],
    enemyType: EnemyType,
    config: {
      nation: Nation;
      health: number;
      speed: number;
      armor: number;
      reward: number;
      isArmored: boolean;
      spriteKey?: string;
    }
  ) {
    super(scene, x, y);

    this.enemyType = enemyType;
    this.nation = config.nation;
    this.maxHealth = config.health;
    this.currentHealth = config.health;
    this.speed = config.speed;
    this.armor = config.armor;
    this.reward = config.reward;
    this.isArmored = config.isArmored;
    this.pathScreenPositions = pathScreenPositions;
    this.currentPathIndex = 0;
    this.pathProgress = 0;

    // Create visuals
    this.createVisuals(config.spriteKey);

    // Add to scene
    scene.add.existing(this);

    // Set depth
    this.setDepth(8);
  }

  /**
   * Create enemy visuals
   */
  protected createVisuals(_spriteKey?: string): void {
    const color = this.getNationColor();

    if (this.isArmored) {
      // Tank visual
      // Tracks
      const leftTrack = this.scene.add.rectangle(-8, 2, 4, 20, 0x2a2a2a);
      const rightTrack = this.scene.add.rectangle(8, 2, 4, 20, 0x2a2a2a);
      this.add([leftTrack, rightTrack]);

      // Hull
      const hull = this.scene.add.ellipse(0, 0, 18, 24, color);
      this.add(hull);

      // Turret
      const turret = this.scene.add.circle(0, -2, 10, Phaser.Display.Color.ValueToColor(color).darken(20).color);
      this.add(turret);

      // Barrel
      const barrel = this.scene.add.rectangle(0, -12, 4, 14, 0x3a3a3a);
      this.add(barrel);

      // Shading
      const shade = this.scene.add.ellipse(-4, 0, 18, 24, 0x000000, 0.2);
      this.add(shade);

      // Highlight
      const highlight = this.scene.add.circle(3, -4, 4, 0xffffff, 0.3);
      this.add(highlight);
    } else {
      // Infantry visual
      // Body
      const body = this.scene.add.ellipse(0, 0, 10, 14, color);
      this.add(body);

      // Head (helmet)
      const head = this.scene.add.circle(0, -8, 5, Phaser.Display.Color.ValueToColor(color).darken(10).color);
      this.add(head);

      // Rifle
      const rifle = this.scene.add.rectangle(3, 2, 2, 10, 0x4a3a2a);
      this.add(rifle);

      // Shading
      const shade = this.scene.add.ellipse(-2, 0, 10, 14, 0x000000, 0.2);
      this.add(shade);
    }

    // Create health bar background
    this.healthBarBg = this.scene.add.graphics();
    this.healthBarBg.fillStyle(0x000000, 0.7);
    this.healthBarBg.fillRect(-15, -22, 30, 3);
    this.add(this.healthBarBg);

    // Create health bar
    this.healthBar = this.scene.add.graphics();
    this.add(this.healthBar);
    this.updateHealthBar();
  }

  /**
   * Get nation color for placeholder graphics
   */
  protected getNationColor(): number {
    switch (this.nation) {
      case Nation.GERMANY: return 0x7f8c8d;  // Gray
      case Nation.ITALY: return 0x16a085;    // Teal
      case Nation.JAPAN: return 0xc0392b;    // Dark Red
      default: return 0x34495e;              // Dark Gray
    }
  }

  /**
   * Update enemy logic
   */
  public update(_time: number, delta: number): void {
    this.moveAlongPath(delta);
  }

  /**
   * Move along the path
   */
  protected moveAlongPath(delta: number): void {
    if (this.currentPathIndex >= this.pathScreenPositions.length - 1) {
      // Reached end of path
      this.reachedEnd();
      return;
    }

    // Get current and next waypoint
    const current = this.pathScreenPositions[this.currentPathIndex];
    const next = this.pathScreenPositions[this.currentPathIndex + 1];

    // Calculate progress along current segment
    const segmentSpeed = (this.speed * delta) / 1000; // pixels per frame
    this.pathProgress += segmentSpeed;

    const distance = Math.sqrt(
      Math.pow(next.x - current.x, 2) + Math.pow(next.y - current.y, 2)
    );

    if (this.pathProgress >= distance) {
      // Move to next waypoint
      this.currentPathIndex++;
      this.pathProgress = 0;
    } else {
      // Interpolate position
      const t = this.pathProgress / distance;
      this.x = Phaser.Math.Linear(current.x, next.x, t);
      this.y = Phaser.Math.Linear(current.y, next.y, t);

      // Update rotation to face movement direction
      const angle = Math.atan2(next.y - current.y, next.x - current.x);
      this.rotation = angle;
    }
  }

  /**
   * Take damage
   */
  public takeDamage(damage: number): void {
    // Apply armor reduction
    const actualDamage = Math.max(1, damage - this.armor);
    this.currentHealth -= actualDamage;

    // Update health bar
    this.updateHealthBar();

    // Flash red when hit
    this.scene.tweens.add({
      targets: this,
      alpha: 0.5,
      duration: 100,
      yoyo: true,
      onComplete: () => {
        this.alpha = 1;
      }
    });

    // Check if dead
    if (this.currentHealth <= 0) {
      this.die();
    }
  }

  /**
   * Update health bar display
   */
  protected updateHealthBar(): void {
    if (!this.healthBar) return;

    this.healthBar.clear();

    const healthPercent = this.currentHealth / this.maxHealth;
    const barWidth = 28;
    const barColor = healthPercent > 0.5 ? 0x27ae60 : healthPercent > 0.25 ? 0xf39c12 : 0xe74c3c;

    this.healthBar.fillStyle(barColor, 1);
    this.healthBar.fillRect(-14, -21, barWidth * healthPercent, 2);
  }

  /**
   * Enemy reached end of path
   */
  protected reachedEnd(): void {
    // Emit event for game manager
    this.scene.events.emit('enemy-reached-end', this);
    this.destroy();
  }

  /**
   * Enemy died
   */
  protected die(): void {
    // Emit event for game manager
    this.scene.events.emit('enemy-killed', this);

    // Play death animation
    this.scene.tweens.add({
      targets: this,
      alpha: 0,
      scale: 0.5,
      duration: 200,
      onComplete: () => {
        this.destroy();
      }
    });
  }

  /**
   * Get enemy info
   */
  public getInfo(): {
    type: EnemyType;
    nation: Nation;
    health: number;
    maxHealth: number;
    speed: number;
    armor: number;
    reward: number;
    isArmored: boolean;
  } {
    return {
      type: this.enemyType,
      nation: this.nation,
      health: this.currentHealth,
      maxHealth: this.maxHealth,
      speed: this.speed,
      armor: this.armor,
      reward: this.reward,
      isArmored: this.isArmored
    };
  }

  /**
   * Get reward money
   */
  public getReward(): number {
    return this.reward;
  }

  /**
   * Check if enemy is armored
   */
  public getIsArmored(): boolean {
    return this.isArmored;
  }

  /**
   * Get current health
   */
  public getHealth(): number {
    return this.currentHealth;
  }

  /**
   * Cleanup
   */
  public destroy(fromScene?: boolean): void {
    if (this.healthBar) {
      this.healthBar.destroy();
    }
    if (this.healthBarBg) {
      this.healthBarBg.destroy();
    }
    super.destroy(fromScene);
  }
}
