/**
 * TowerManager - Manages tower placement, upgrades, and targeting
 */

import Phaser from 'phaser';
import Tower from '../entities/Tower';
import Enemy from '../entities/Enemy';
import { GridPosition } from '../types/GameTypes';
import { TowerType } from '../types/Enums';
import GridSystem from '../systems/GridSystem';
import EconomyManager from './EconomyManager';
import BritishRifleman from '../entities/types/towers/BritishRifleman';
import BritishMachinegun from '../entities/types/towers/BritishMachinegun';
import BritishAntitank from '../entities/types/towers/BritishTower';
import BritishArtillery from '../entities/types/towers/BritishArtillery';
import BritishMortar from '../entities/types/towers/BritishMortar';

export default class TowerManager {
  private scene: Phaser.Scene;
  private gridSystem: GridSystem;
  private economyManager: EconomyManager;
  private towers: Tower[];
  private selectedTower: Tower | null;

  /**
   * Create a new TowerManager
   */
  constructor(
    scene: Phaser.Scene,
    gridSystem: GridSystem,
    economyManager: EconomyManager
  ) {
    this.scene = scene;
    this.gridSystem = gridSystem;
    this.economyManager = economyManager;
    this.towers = [];
    this.selectedTower = null;
  }

  /**
   * Attempt to place a tower
   */
  public placeTower(gridX: number, gridY: number, towerType: TowerType): boolean {
    // Validate position
    if (!this.canPlaceTower(gridX, gridY)) {
      console.log('Cannot place tower: invalid position');
      return false;
    }

    // Get tower cost
    const cost = this.getTowerCost(towerType);

    // Check affordability
    if (!this.economyManager.canAfford(cost)) {
      console.log('Cannot place tower: insufficient funds');
      this.scene.events.emit('tower-placement-failed', 'Not enough money');
      return false;
    }

    // Get screen position
    const screenPos = this.gridSystem.gridToScreen(gridX, gridY);

    // Create tower
    const tower = this.createTower(screenPos.x, screenPos.y, { x: gridX, y: gridY }, towerType);

    if (tower) {
      // Spend money
      this.economyManager.spend(cost);

      // Add to towers list
      this.towers.push(tower);

      // Block the grid tile
      this.gridSystem.blockTile(gridX, gridY);

      // Emit event
      this.scene.events.emit('tower-placed', tower);

      console.log(`Placed ${towerType} at (${gridX}, ${gridY})`);
      return true;
    }

    return false;
  }

  /**
   * Create a tower of the specified type
   */
  private createTower(
    x: number,
    y: number,
    gridPosition: GridPosition,
    towerType: TowerType
  ): Tower | null {
    switch (towerType) {
      case TowerType.BRITISH_RIFLEMAN:
        return new BritishRifleman(this.scene, x, y, gridPosition);

      case TowerType.BRITISH_MACHINEGUN:
        return new BritishMachinegun(this.scene, x, y, gridPosition);

      case TowerType.BRITISH_ANTITANK:
        return new BritishAntitank(this.scene, x, y, gridPosition);

      case TowerType.BRITISH_ARTILLERY:
        return new BritishArtillery(this.scene, x, y, gridPosition);

      case TowerType.BRITISH_MORTAR:
        return new BritishMortar(this.scene, x, y, gridPosition);

      default:
        console.error(`Unknown tower type: ${towerType}`);
        return null;
    }
  }

  /**
   * Check if a tower can be placed at the position
   */
  public canPlaceTower(gridX: number, gridY: number): boolean {
    return this.gridSystem.canPlaceTower(gridX, gridY);
  }

  /**
   * Get tower cost
   */
  public getTowerCost(towerType: TowerType): number {
    // Tower costs from the specific tower classes
    switch (towerType) {
      case TowerType.BRITISH_RIFLEMAN: return 100;
      case TowerType.BRITISH_MACHINEGUN: return 150;
      case TowerType.BRITISH_ANTITANK: return 200;
      case TowerType.BRITISH_MORTAR: return 250;
      case TowerType.BRITISH_ARTILLERY: return 300;
      default: return 0;
    }
  }

  /**
   * Update all towers
   */
  public update(time: number, delta: number, enemies: Enemy[]): void {
    this.towers.forEach(tower => {
      // Update tower
      tower.update(time, delta);

      // If tower doesn't have a target, try to acquire one
      if (!tower.getTarget()) {
        const target = this.findTargetForTower(tower, enemies);
        if (target) {
          tower.setTarget(target);
        }
      }
    });
  }

  /**
   * Find a target for a tower
   */
  private findTargetForTower(tower: Tower, enemies: Enemy[]): Enemy | null {
    // Find first enemy in range
    for (const enemy of enemies) {
      if (enemy.active && tower.isInRange(enemy)) {
        return enemy;
      }
    }
    return null;
  }

  /**
   * Select a tower
   */
  public selectTower(tower: Tower): void {
    // Deselect previous tower
    if (this.selectedTower) {
      this.selectedTower.deselect();
    }

    // Select new tower
    this.selectedTower = tower;
    tower.select();

    // Emit event
    this.scene.events.emit('tower-selected', tower);
  }

  /**
   * Deselect current tower
   */
  public deselectTower(): void {
    if (this.selectedTower) {
      this.selectedTower.deselect();
      this.selectedTower = null;
    }
  }

  /**
   * Get tower at grid position
   */
  public getTowerAtPosition(gridX: number, gridY: number): Tower | null {
    return this.towers.find(tower => {
      const pos = tower.getGridPosition();
      return pos.x === gridX && pos.y === gridY;
    }) || null;
  }

  /**
   * Get all towers
   */
  public getTowers(): Tower[] {
    return this.towers;
  }

  /**
   * Get selected tower
   */
  public getSelectedTower(): Tower | null {
    return this.selectedTower;
  }

  /**
   * Remove a tower
   */
  public removeTower(tower: Tower): void {
    const index = this.towers.indexOf(tower);
    if (index > -1) {
      this.towers.splice(index, 1);

      // Unblock grid tile
      const pos = tower.getGridPosition();
      this.gridSystem.unblockTile(pos.x, pos.y);

      // Destroy tower
      tower.destroy();
    }
  }

  /**
   * Clear all towers
   */
  public clear(): void {
    this.towers.forEach(tower => tower.destroy());
    this.towers = [];
    this.selectedTower = null;
  }
}
