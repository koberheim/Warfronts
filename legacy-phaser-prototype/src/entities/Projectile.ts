/**
 * Projectile - Bullets, shells, and rockets fired by towers
 * Handles movement and collision with enemies
 */

import Phaser from 'phaser';
import { ProjectileType } from '../types/Enums';
import Enemy from './Enemy';

export default class Projectile extends Phaser.GameObjects.Container {
  protected projectileType: ProjectileType;
  protected damage: number;
  protected speed: number;
  protected target: Enemy;
  protected hasHit: boolean;
  protected projectileSprite?: Phaser.GameObjects.Graphics;

  /**
   * Create a new Projectile
   */
  constructor(
    scene: Phaser.Scene,
    x: number,
    y: number,
    target: Enemy,
    config: {
      damage: number;
      speed: number;
      type: ProjectileType;
      spriteKey?: string;
    }
  ) {
    super(scene, x, y);

    this.projectileType = config.type;
    this.damage = config.damage;
    this.speed = config.speed;
    this.target = target;
    this.hasHit = false;

    // Create visuals
    this.createVisuals(config.spriteKey);

    // Add to scene
    scene.add.existing(this);

    // Set depth
    this.setDepth(9);
  }

  /**
   * Create projectile visuals
   */
  protected createVisuals(_spriteKey?: string): void {
    // For now, create simple shapes based on type
    this.projectileSprite = this.scene.add.graphics();

    switch (this.projectileType) {
      case ProjectileType.BULLET:
        // Small yellow circle
        this.projectileSprite.fillStyle(0xf1c40f, 1);
        this.projectileSprite.fillCircle(0, 0, 3);
        break;

      case ProjectileType.SHELL:
        // Larger orange circle
        this.projectileSprite.fillStyle(0xe67e22, 1);
        this.projectileSprite.fillCircle(0, 0, 5);
        break;

      case ProjectileType.ROCKET:
        // Red elongated shape
        this.projectileSprite.fillStyle(0xe74c3c, 1);
        this.projectileSprite.fillRect(-2, -6, 4, 12);
        break;
    }

    this.add(this.projectileSprite);
  }

  /**
   * Update projectile logic
   */
  public update(_time: number, delta: number): void {
    if (this.hasHit) return;

    // Check if target is still valid
    if (!this.target || !this.target.active) {
      this.destroy();
      return;
    }

    // Move towards target
    const distance = Phaser.Math.Distance.Between(
      this.x,
      this.y,
      this.target.x,
      this.target.y
    );

    // Check if we hit the target
    if (distance < 10) {
      this.hit();
      return;
    }

    // Calculate movement
    const moveDistance = (this.speed * delta) / 1000;
    const angle = Phaser.Math.Angle.Between(
      this.x,
      this.y,
      this.target.x,
      this.target.y
    );

    // Update position
    this.x += Math.cos(angle) * moveDistance;
    this.y += Math.sin(angle) * moveDistance;

    // Update rotation to face target
    this.rotation = angle + Math.PI / 2;
  }

  /**
   * Hit the target
   */
  protected hit(): void {
    if (this.hasHit) return;
    this.hasHit = true;

    // Deal damage to target
    if (this.target && this.target.active) {
      this.target.takeDamage(this.damage);
    }

    // Create impact effect
    this.createImpactEffect();

    // Destroy projectile
    this.destroy();
  }

  /**
   * Create impact effect
   */
  protected createImpactEffect(): void {
    const impact = this.scene.add.circle(this.x, this.y, 5, 0xffffff, 0.8);
    impact.setDepth(15);

    this.scene.tweens.add({
      targets: impact,
      scale: 2,
      alpha: 0,
      duration: 200,
      onComplete: () => {
        impact.destroy();
      }
    });
  }

  /**
   * Get damage value
   */
  public getDamage(): number {
    return this.damage;
  }

  /**
   * Cleanup
   */
  public destroy(fromScene?: boolean): void {
    if (this.projectileSprite) {
      this.projectileSprite.destroy();
    }
    super.destroy(fromScene);
  }
}
