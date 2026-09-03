/**
 * EconomyManager - Handles money, costs, and rewards
 */

import Phaser from 'phaser';

export default class EconomyManager {
  private scene: Phaser.Scene;
  private currentMoney: number;
  private startingMoney: number;

  /**
   * Create a new EconomyManager
   */
  constructor(scene: Phaser.Scene, startingMoney: number = 500) {
    this.scene = scene;
    this.startingMoney = startingMoney;
    this.currentMoney = startingMoney;
  }

  /**
   * Get current money
   */
  public getMoney(): number {
    return this.currentMoney;
  }

  /**
   * Add money
   */
  public addMoney(amount: number): void {
    this.currentMoney += amount;
    this.emitMoneyChanged();
  }

  /**
   * Spend money
   */
  public spend(amount: number): boolean {
    if (this.canAfford(amount)) {
      this.currentMoney -= amount;
      this.emitMoneyChanged();
      return true;
    }
    return false;
  }

  /**
   * Check if can afford something
   */
  public canAfford(amount: number): boolean {
    return this.currentMoney >= amount;
  }

  /**
   * Reset money to starting amount
   */
  public reset(): void {
    this.currentMoney = this.startingMoney;
    this.emitMoneyChanged();
  }

  /**
   * Emit money changed event
   */
  private emitMoneyChanged(): void {
    this.scene.events.emit('money-changed', this.currentMoney);
  }
}
