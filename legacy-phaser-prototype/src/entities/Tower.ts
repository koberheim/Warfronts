/**
 * Tower - Base class for all tower types
 * Handles targeting, firing, and upgrades
 */

import Phaser from 'phaser';
import { GridPosition } from '../types/GameTypes';
import { TowerType, Nation, ProjectileType } from '../types/Enums';
import Projectile from './Projectile';
import Enemy from './Enemy';

export default class Tower extends Phaser.GameObjects.Container {
  // Tower properties
  protected towerType: TowerType;
  protected nation: Nation;
  protected gridPosition: GridPosition;

  // Stats
  protected damage: number;
  protected range: number;
  protected fireRate: number; // Shots per second
  protected projectileSpeed: number;
  protected projectileType: ProjectileType;
  protected level: number;
  protected cost: number;

  // State
  protected target: Enemy | null;
  protected lastFireTime: number;
  protected rangeCircle?: Phaser.GameObjects.Graphics;
  protected towerSprite?: Phaser.GameObjects.Sprite;
  protected isSelected: boolean;

  /**
   * Create a new Tower
   */
  constructor(
    scene: Phaser.Scene,
    x: number,
    y: number,
    gridPosition: GridPosition,
    towerType: TowerType,
    config: {
      nation: Nation;
      damage: number;
      range: number;
      fireRate: number;
      projectileSpeed: number;
      projectileType: ProjectileType;
      cost: number;
      spriteKey?: string;
    }
  ) {
    super(scene, x, y);

    this.towerType = towerType;
    this.nation = config.nation;
    this.gridPosition = gridPosition;
    this.damage = config.damage;
    this.range = config.range;
    this.fireRate = config.fireRate;
    this.projectileSpeed = config.projectileSpeed;
    this.projectileType = config.projectileType;
    this.cost = config.cost;
    this.level = 1;
    this.target = null;
    this.lastFireTime = 0;
    this.isSelected = false;

    // Create visual representation
    this.createVisuals(config.spriteKey);

    // Add to scene
    scene.add.existing(this);

    // Set depth for proper layering
    this.setDepth(10);
  }

  /**
   * Create tower visuals
   */
  protected createVisuals(_spriteKey?: string): void {
    const color = this.getNationColor();

    // Create base platform
    const base = this.scene.add.ellipse(0, 5, 28, 16, 0x4a4a4a);
    this.add(base);

    // Create sandbag walls
    const sandbag1 = this.scene.add.ellipse(-10, 0, 12, 8, 0xb8a788);
    const sandbag2 = this.scene.add.ellipse(10, 0, 12, 8, 0xb8a788);
    const sandbag3 = this.scene.add.ellipse(0, -8, 12, 8, 0xb8a788);
    this.add([sandbag1, sandbag2, sandbag3]);

    // Create main tower body (colored based on nation)
    const body = this.scene.add.circle(0, -5, 12, color);
    this.add(body);

    // Add shading
    const shade = this.scene.add.circle(-3, -3, 12, 0x000000, 0.2);
    this.add(shade);

    // Create barrel/weapon
    const barrel = this.scene.add.rectangle(0, -15, 5, 18, 0x333333);
    this.add(barrel);

    // Add weapon tip
    const tip = this.scene.add.circle(0, -24, 3, 0x1a1a1a);
    this.add(tip);

    // Add British roundel (white-red-blue circles)
    const roundelOuter = this.scene.add.circle(0, -5, 6, 0xffffff);
    const roundelMiddle = this.scene.add.circle(0, -5, 4, 0xcc0000);
    const roundelInner = this.scene.add.circle(0, -5, 2, 0x0033cc);
    this.add([roundelOuter, roundelMiddle, roundelInner]);
  }

  /**
   * Get nation color for placeholder graphics
   */
  protected getNationColor(): number {
    switch (this.nation) {
      case Nation.USA: return 0x3498db;      // Blue
      case Nation.BRITAIN: return 0xe74c3c;  // Red
      case Nation.FRANCE: return 0x9b59b6;   // Purple
      case Nation.CANADA: return 0xe67e22;   // Orange
      default: return 0x95a5a6;              // Gray
    }
  }

  /**
   * Update tower logic
   */
  public update(time: number, _delta: number): void {
    // Acquire target if we don't have one
    if (!this.target || !this.target.active || !this.isInRange(this.target)) {
      this.target = null;
    }

    // Rotate towards target
    if (this.target) {
      const angle = Phaser.Math.Angle.Between(
        this.x,
        this.y,
        this.target.x,
        this.target.y
      );
      this.rotation = angle + Math.PI / 2; // Adjust for sprite orientation

      // Try to fire
      this.tryFire(time);
    }
  }

  /**
   * Set the current target
   */
  public setTarget(enemy: Enemy | null): void {
    this.target = enemy;
  }

  /**
   * Get current target
   */
  public getTarget(): Enemy | null {
    return this.target;
  }

  /**
   * Check if enemy is in range
   */
  public isInRange(enemy: Enemy): boolean {
    const distance = Phaser.Math.Distance.Between(
      this.x,
      this.y,
      enemy.x,
      enemy.y
    );
    return distance <= this.range;
  }

  /**
   * Try to fire at target
   */
  protected tryFire(time: number): void {
    if (!this.target) return;

    const timeSinceLastFire = time - this.lastFireTime;
    const fireInterval = 1000 / this.fireRate; // Convert to milliseconds

    if (timeSinceLastFire >= fireInterval) {
      this.fire();
      this.lastFireTime = time;
    }
  }

  /**
   * Fire a projectile at the target
   */
  protected fire(): void {
    if (!this.target) return;

    const projectile = new Projectile(
      this.scene,
      this.x,
      this.y,
      this.target,
      {
        damage: this.damage,
        speed: this.projectileSpeed,
        type: this.projectileType
      }
    );

    // Emit event for projectile creation
    this.scene.events.emit('projectile-fired', projectile);
  }

  /**
   * Show range indicator
   */
  public showRange(): void {
    if (this.rangeCircle) {
      this.rangeCircle.destroy();
    }

    this.rangeCircle = this.scene.add.graphics();
    this.rangeCircle.lineStyle(2, 0xffff00, 0.5);
    this.rangeCircle.strokeCircle(this.x, this.y, this.range);
    this.rangeCircle.setDepth(5);
  }

  /**
   * Hide range indicator
   */
  public hideRange(): void {
    if (this.rangeCircle) {
      this.rangeCircle.destroy();
      this.rangeCircle = undefined;
    }
  }

  /**
   * Select this tower
   */
  public select(): void {
    this.isSelected = true;
    this.showRange();
  }

  /**
   * Deselect this tower
   */
  public deselect(): void {
    this.isSelected = false;
    this.hideRange();
  }

  /**
   * Upgrade tower to next level
   */
  public upgrade(): boolean {
    // Will be implemented with upgrade system
    this.level++;
    this.damage += 5;
    this.range += 10;
    return true;
  }

  /**
   * Get tower info
   */
  public getInfo(): {
    type: TowerType;
    nation: Nation;
    level: number;
    damage: number;
    range: number;
    fireRate: number;
    gridPosition: GridPosition;
  } {
    return {
      type: this.towerType,
      nation: this.nation,
      level: this.level,
      damage: this.damage,
      range: this.range,
      fireRate: this.fireRate,
      gridPosition: this.gridPosition
    };
  }

  /**
   * Get grid position
   */
  public getGridPosition(): GridPosition {
    return this.gridPosition;
  }

  /**
   * Get tower cost
   */
  public getCost(): number {
    return this.cost;
  }

  /**
   * Cleanup
   */
  public destroy(fromScene?: boolean): void {
    this.hideRange();
    super.destroy(fromScene);
  }
}
