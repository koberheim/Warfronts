/**
 * GridSystem - Manages the isometric grid
 * Handles coordinate conversions, validation, and grid rendering
 */

import Phaser from 'phaser';
import { GridPosition, ScreenPosition } from '../types/GameTypes';
import { GAME_CONSTANTS, GRID_COLORS } from '../config/Constants';
import { gridToScreen, screenToGrid, gridDistance, gridEquals } from '../utils/IsoUtils';

export default class GridSystem {
  private scene: Phaser.Scene;
  private gridWidth: number;
  private gridHeight: number;
  private offsetX: number;
  private offsetY: number;
  private gridGraphics?: Phaser.GameObjects.Graphics;
  private highlightGraphics?: Phaser.GameObjects.Graphics;
  private blockedTiles: Set<string>;

  /**
   * Create a new GridSystem
   * @param scene - The Phaser scene
   * @param gridWidth - Grid width in tiles
   * @param gridHeight - Grid height in tiles
   * @param offsetX - X offset for centering the grid
   * @param offsetY - Y offset for centering the grid
   */
  constructor(
    scene: Phaser.Scene,
    gridWidth: number,
    gridHeight: number,
    offsetX: number = 0,
    offsetY: number = 0
  ) {
    this.scene = scene;
    this.gridWidth = gridWidth;
    this.gridHeight = gridHeight;
    this.offsetX = offsetX;
    this.offsetY = offsetY;
    this.blockedTiles = new Set();
  }

  /**
   * Convert grid coordinates to screen position (with offset)
   */
  public gridToScreen(gridX: number, gridY: number): ScreenPosition {
    const pos = gridToScreen(gridX, gridY);
    return {
      x: pos.x + this.offsetX,
      y: pos.y + this.offsetY
    };
  }

  /**
   * Convert screen position to grid coordinates (accounting for offset)
   */
  public screenToGrid(screenX: number, screenY: number): GridPosition {
    return screenToGrid(
      screenX - this.offsetX,
      screenY - this.offsetY
    );
  }

  /**
   * Check if a grid position is within valid bounds
   */
  public isValidGridPosition(gridX: number, gridY: number): boolean {
    return (
      gridX >= 0 &&
      gridX < this.gridWidth &&
      gridY >= 0 &&
      gridY < this.gridHeight
    );
  }

  /**
   * Check if a grid position is blocked
   */
  public isBlocked(gridX: number, gridY: number): boolean {
    const key = `${gridX},${gridY}`;
    return this.blockedTiles.has(key);
  }

  /**
   * Block a grid tile
   */
  public blockTile(gridX: number, gridY: number): void {
    const key = `${gridX},${gridY}`;
    this.blockedTiles.add(key);
  }

  /**
   * Unblock a grid tile
   */
  public unblockTile(gridX: number, gridY: number): void {
    const key = `${gridX},${gridY}`;
    this.blockedTiles.delete(key);
  }

  /**
   * Check if a tile can have a tower placed on it
   */
  public canPlaceTower(gridX: number, gridY: number): boolean {
    return this.isValidGridPosition(gridX, gridY) && !this.isBlocked(gridX, gridY);
  }

  /**
   * Render the grid (for debugging/visualization)
   */
  public renderGrid(visible: boolean = true): void {
    // Clean up existing graphics
    if (this.gridGraphics) {
      this.gridGraphics.destroy();
    }

    if (!visible) return;

    this.gridGraphics = this.scene.add.graphics();
    this.gridGraphics.lineStyle(1, 0x00ff00, 0.3);

    // Draw all grid tiles
    for (let y = 0; y < this.gridHeight; y++) {
      for (let x = 0; x < this.gridWidth; x++) {
        this.drawTile(this.gridGraphics, x, y, 0x00ff00, 0.1);
      }
    }

    // Draw blocked tiles in red
    this.gridGraphics.lineStyle(1, 0xff0000, 0.5);
    this.blockedTiles.forEach(key => {
      const [x, y] = key.split(',').map(Number);
      this.drawTile(this.gridGraphics!, x, y, 0xff0000, 0.3);
    });
  }

  /**
   * Draw a single tile diamond
   */
  private drawTile(
    graphics: Phaser.GameObjects.Graphics,
    gridX: number,
    gridY: number,
    color: number,
    alpha: number
  ): void {
    const center = this.gridToScreen(gridX, gridY);
    const halfWidth = GAME_CONSTANTS.TILE_WIDTH / 2;
    const halfHeight = GAME_CONSTANTS.TILE_HEIGHT / 2;

    graphics.fillStyle(color, alpha);
    graphics.beginPath();
    graphics.moveTo(center.x, center.y - halfHeight);
    graphics.lineTo(center.x + halfWidth, center.y);
    graphics.lineTo(center.x, center.y + halfHeight);
    graphics.lineTo(center.x - halfWidth, center.y);
    graphics.closePath();
    graphics.fillPath();
    graphics.strokePath();
  }

  /**
   * Highlight a specific tile
   */
  public highlightTile(gridX: number, gridY: number, isValid: boolean = true): void {
    if (!this.highlightGraphics) {
      this.highlightGraphics = this.scene.add.graphics();
    }

    this.highlightGraphics.clear();

    if (!this.isValidGridPosition(gridX, gridY)) return;

    const color = isValid ? GRID_COLORS.VALID : GRID_COLORS.INVALID;
    this.highlightGraphics.lineStyle(2, color, 1);
    this.drawTile(this.highlightGraphics, gridX, gridY, color, 0.3);
  }

  /**
   * Clear tile highlight
   */
  public clearHighlight(): void {
    if (this.highlightGraphics) {
      this.highlightGraphics.clear();
    }
  }

  /**
   * Get grid dimensions
   */
  public getGridDimensions(): { width: number; height: number } {
    return { width: this.gridWidth, height: this.gridHeight };
  }

  /**
   * Get grid offset
   */
  public getGridOffset(): { x: number; y: number } {
    return { x: this.offsetX, y: this.offsetY };
  }

  /**
   * Calculate distance between two grid positions
   */
  public getDistance(pos1: GridPosition, pos2: GridPosition): number {
    return gridDistance(pos1, pos2);
  }

  /**
   * Check if two grid positions are equal
   */
  public arePositionsEqual(pos1: GridPosition, pos2: GridPosition): boolean {
    return gridEquals(pos1, pos2);
  }

  /**
   * Destroy the grid system
   */
  public destroy(): void {
    if (this.gridGraphics) {
      this.gridGraphics.destroy();
    }
    if (this.highlightGraphics) {
      this.highlightGraphics.destroy();
    }
    this.blockedTiles.clear();
  }
}
