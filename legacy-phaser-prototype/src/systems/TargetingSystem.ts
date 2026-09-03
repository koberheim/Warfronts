/**
 * TargetingSystem - Handles tower AI targeting logic
 * Provides different targeting strategies
 */

import Enemy from '../entities/Enemy';
import Tower from '../entities/Tower';

export enum TargetingStrategy {
  FIRST = 'first',       // Target enemy closest to end
  LAST = 'last',         // Target enemy farthest from end
  CLOSEST = 'closest',   // Target closest enemy
  STRONGEST = 'strongest', // Target enemy with most health
  WEAKEST = 'weakest'    // Target enemy with least health
}

export default class TargetingSystem {
  /**
   * Find target for a tower using specified strategy
   */
  public static findTarget(
    tower: Tower,
    enemies: Enemy[],
    strategy: TargetingStrategy = TargetingStrategy.FIRST
  ): Enemy | null {
    // Filter enemies in range
    const enemiesInRange = enemies.filter(enemy =>
      enemy.active && tower.isInRange(enemy)
    );

    if (enemiesInRange.length === 0) {
      return null;
    }

    switch (strategy) {
      case TargetingStrategy.FIRST:
        return this.findFirstEnemy(enemiesInRange);

      case TargetingStrategy.LAST:
        return this.findLastEnemy(enemiesInRange);

      case TargetingStrategy.CLOSEST:
        return this.findClosestEnemy(tower, enemiesInRange);

      case TargetingStrategy.STRONGEST:
        return this.findStrongestEnemy(enemiesInRange);

      case TargetingStrategy.WEAKEST:
        return this.findWeakestEnemy(enemiesInRange);

      default:
        return enemiesInRange[0];
    }
  }

  /**
   * Find enemy closest to end (furthest along path)
   */
  private static findFirstEnemy(enemies: Enemy[]): Enemy {
    // For now, just return first enemy
    // In full implementation, would track progress along path
    return enemies[0];
  }

  /**
   * Find enemy farthest from end (least progress along path)
   */
  private static findLastEnemy(enemies: Enemy[]): Enemy {
    return enemies[enemies.length - 1];
  }

  /**
   * Find closest enemy to tower
   */
  private static findClosestEnemy(tower: Tower, enemies: Enemy[]): Enemy {
    let closest = enemies[0];
    let minDistance = Phaser.Math.Distance.Between(
      tower.x,
      tower.y,
      closest.x,
      closest.y
    );

    for (let i = 1; i < enemies.length; i++) {
      const enemy = enemies[i];
      const distance = Phaser.Math.Distance.Between(
        tower.x,
        tower.y,
        enemy.x,
        enemy.y
      );

      if (distance < minDistance) {
        minDistance = distance;
        closest = enemy;
      }
    }

    return closest;
  }

  /**
   * Find enemy with most health
   */
  private static findStrongestEnemy(enemies: Enemy[]): Enemy {
    let strongest = enemies[0];
    let maxHealth = strongest.getHealth();

    for (let i = 1; i < enemies.length; i++) {
      const enemy = enemies[i];
      const health = enemy.getHealth();

      if (health > maxHealth) {
        maxHealth = health;
        strongest = enemy;
      }
    }

    return strongest;
  }

  /**
   * Find enemy with least health
   */
  private static findWeakestEnemy(enemies: Enemy[]): Enemy {
    let weakest = enemies[0];
    let minHealth = weakest.getHealth();

    for (let i = 1; i < enemies.length; i++) {
      const enemy = enemies[i];
      const health = enemy.getHealth();

      if (health < minHealth) {
        minHealth = health;
        weakest = enemy;
      }
    }

    return weakest;
  }

  /**
   * Check if target is still valid
   */
  public static isValidTarget(enemy: Enemy | null, tower: Tower): boolean {
    if (!enemy || !enemy.active) {
      return false;
    }

    return tower.isInRange(enemy);
  }
}
