/**
 * TerrainRenderer - Renders terrain tiles with visual variety
 * Creates procedural textures for different terrain types
 */

import Phaser from 'phaser';
import { GridPosition } from '../types/GameTypes';
import GridSystem from './GridSystem';
import { GAME_CONSTANTS } from '../config/Constants';

export enum TerrainType {
  GRASS = 'grass',
  DIRT = 'dirt',
  SAND = 'sand',
  STONE = 'stone',
  WATER = 'water',
  PATH = 'path'
}

export interface TerrainTile {
  gridX: number;
  gridY: number;
  type: TerrainType;
  variant?: number;
}

export interface Decoration {
  gridX: number;
  gridY: number;
  type: 'tree' | 'rock' | 'bush' | 'ruins' | 'sandbag' | 'crater';
  offsetX?: number;
  offsetY?: number;
}

export default class TerrainRenderer {
  private scene: Phaser.Scene;
  private gridSystem: GridSystem;
  private terrainLayer!: Phaser.GameObjects.Container;
  private decorationLayer!: Phaser.GameObjects.Container;

  constructor(scene: Phaser.Scene, gridSystem: GridSystem) {
    this.scene = scene;
    this.gridSystem = gridSystem;

    this.terrainLayer = scene.add.container(0, 0).setDepth(0);
    this.decorationLayer = scene.add.container(0, 0).setDepth(1);
  }

  /**
   * Render terrain based on map theme
   */
  public renderTerrain(
    mapTheme: 'normandy' | 'desert' | 'forest' | 'urban',
    path: GridPosition[],
    gridWidth: number,
    gridHeight: number
  ): void {
    // Create path set for quick lookup
    const pathSet = new Set(path.map(p => `${p.x},${p.y}`));

    // Render all tiles
    for (let y = 0; y < gridHeight; y++) {
      for (let x = 0; x < gridWidth; x++) {
        const isPath = pathSet.has(`${x},${y}`);
        const terrainType = this.getTerrainType(x, y, isPath, mapTheme);
        this.renderTile(x, y, terrainType);
      }
    }

    // Add decorations based on theme
    this.addDecorations(mapTheme, path, gridWidth, gridHeight);
  }

  /**
   * Get terrain type for a tile
   */
  private getTerrainType(
    _x: number,
    _y: number,
    isPath: boolean,
    theme: 'normandy' | 'desert' | 'forest' | 'urban'
  ): TerrainType {
    if (isPath) return TerrainType.PATH;

    switch (theme) {
      case 'normandy':
        return TerrainType.GRASS;
      case 'desert':
        return TerrainType.SAND;
      case 'forest':
        return TerrainType.GRASS;
      case 'urban':
        return TerrainType.STONE;
      default:
        return TerrainType.GRASS;
    }
  }

  /**
   * Render a single terrain tile
   */
  private renderTile(gridX: number, gridY: number, type: TerrainType): void {
    const pos = this.gridSystem.gridToScreen(gridX, gridY);
    const tileWidth = GAME_CONSTANTS.TILE_WIDTH;
    const tileHeight = GAME_CONSTANTS.TILE_HEIGHT;

    const graphics = this.scene.add.graphics();

    // Get colors based on terrain type
    const colors = this.getTerrainColors(type);

    // Draw isometric diamond with subtle variation
    const variant = (gridX * 7 + gridY * 13) % 3; // Pseudo-random variation
    const baseColor = colors.base;
    const shadeColor = colors.shade;
    const highlightColor = colors.highlight;

    // Draw base diamond
    graphics.fillStyle(baseColor, 1);
    graphics.beginPath();
    graphics.moveTo(pos.x, pos.y - tileHeight / 2);
    graphics.lineTo(pos.x + tileWidth / 2, pos.y);
    graphics.lineTo(pos.x, pos.y + tileHeight / 2);
    graphics.lineTo(pos.x - tileWidth / 2, pos.y);
    graphics.closePath();
    graphics.fillPath();

    // Add shading for depth
    graphics.fillStyle(shadeColor, 0.3);
    graphics.fillTriangle(
      pos.x, pos.y + tileHeight / 2,
      pos.x - tileWidth / 2, pos.y,
      pos.x, pos.y - tileHeight / 2
    );

    // Add highlight
    graphics.fillStyle(highlightColor, 0.2);
    graphics.fillTriangle(
      pos.x, pos.y - tileHeight / 2,
      pos.x + tileWidth / 2, pos.y,
      pos.x, pos.y + tileHeight / 2
    );

    // Add texture details based on type
    if (type === TerrainType.GRASS) {
      this.addGrassTexture(graphics, pos, variant);
    } else if (type === TerrainType.SAND) {
      this.addSandTexture(graphics, pos, variant);
    } else if (type === TerrainType.PATH) {
      this.addPathTexture(graphics, pos);
    } else if (type === TerrainType.STONE) {
      this.addStoneTexture(graphics, pos, variant);
    }

    // Subtle border
    graphics.lineStyle(1, 0x000000, 0.1);
    graphics.strokePath();

    this.terrainLayer.add(graphics);
  }

  /**
   * Get terrain colors
   */
  private getTerrainColors(type: TerrainType): {
    base: number;
    shade: number;
    highlight: number;
  } {
    switch (type) {
      case TerrainType.GRASS:
        return {
          base: 0x5a8c3e,
          shade: 0x4a7232,
          highlight: 0x7aa855
        };
      case TerrainType.SAND:
        return {
          base: 0xd4a574,
          shade: 0xb8895f,
          highlight: 0xe8c299
        };
      case TerrainType.DIRT:
        return {
          base: 0x8b6f47,
          shade: 0x6b5537,
          highlight: 0xa58a62
        };
      case TerrainType.STONE:
        return {
          base: 0x7f8c8d,
          shade: 0x5f6c6d,
          highlight: 0x9fa9aa
        };
      case TerrainType.PATH:
        return {
          base: 0x8b7355,
          shade: 0x6b5845,
          highlight: 0xa59070
        };
      default:
        return {
          base: 0x5a8c3e,
          shade: 0x4a7232,
          highlight: 0x7aa855
        };
    }
  }

  /**
   * Add grass texture details
   */
  private addGrassTexture(graphics: Phaser.GameObjects.Graphics, pos: { x: number; y: number }, variant: number): void {
    graphics.fillStyle(0x4a7232, 0.2);

    // Random grass patches
    for (let i = 0; i < 3 + variant; i++) {
      const offsetX = (i * 17 % 20) - 10;
      const offsetY = (i * 13 % 12) - 6;
      graphics.fillRect(pos.x + offsetX, pos.y + offsetY, 2, 2);
    }
  }

  /**
   * Add sand texture details
   */
  private addSandTexture(graphics: Phaser.GameObjects.Graphics, pos: { x: number; y: number }, variant: number): void {
    graphics.fillStyle(0xb8895f, 0.15);

    // Sand ripples
    for (let i = 0; i < 4; i++) {
      const offsetY = -8 + i * 4 + (variant * 2);
      graphics.fillRect(pos.x - 15, pos.y + offsetY, 30, 1);
    }
  }

  /**
   * Add path texture (looks like a dirt road)
   */
  private addPathTexture(graphics: Phaser.GameObjects.Graphics, pos: { x: number; y: number }): void {
    // Darker center
    graphics.fillStyle(0x6b5845, 0.3);
    graphics.fillEllipse(pos.x, pos.y, 20, 10);

    // Tire tracks
    graphics.lineStyle(1, 0x5a4835, 0.4);
    graphics.lineBetween(pos.x - 8, pos.y - 3, pos.x - 8, pos.y + 3);
    graphics.lineBetween(pos.x + 8, pos.y - 3, pos.x + 8, pos.y + 3);
  }

  /**
   * Add stone texture
   */
  private addStoneTexture(graphics: Phaser.GameObjects.Graphics, pos: { x: number; y: number }, variant: number): void {
    graphics.fillStyle(0x5f6c6d, 0.2);

    // Stone cracks
    for (let i = 0; i < 2 + variant; i++) {
      const offsetX = (i * 19 % 24) - 12;
      const offsetY = (i * 11 % 16) - 8;
      graphics.fillRect(pos.x + offsetX, pos.y + offsetY, 1 + variant, 4);
    }
  }

  /**
   * Add decorations to the map
   */
  private addDecorations(
    theme: 'normandy' | 'desert' | 'forest' | 'urban',
    path: GridPosition[],
    gridWidth: number,
    gridHeight: number
  ): void {
    const pathSet = new Set(path.map(p => `${p.x},${p.y}`));

    // Add decorations based on theme
    for (let y = 0; y < gridHeight; y++) {
      for (let x = 0; x < gridWidth; x++) {
        // Don't place on path
        if (pathSet.has(`${x},${y}`)) continue;

        // Random chance for decoration
        const random = (x * 17 + y * 23) % 100;

        if (random < 15) {
          const decorationType = this.getDecorationForTheme(theme, random);
          this.renderDecoration(x, y, decorationType);
        }
      }
    }
  }

  /**
   * Get decoration type for theme
   */
  private getDecorationForTheme(
    theme: 'normandy' | 'desert' | 'forest' | 'urban',
    random: number
  ): Decoration['type'] {
    switch (theme) {
      case 'normandy':
        return random < 8 ? 'tree' : random < 12 ? 'bush' : 'crater';
      case 'desert':
        return random < 5 ? 'rock' : random < 10 ? 'bush' : 'sandbag';
      case 'forest':
        return random < 10 ? 'tree' : 'bush';
      case 'urban':
        return random < 8 ? 'ruins' : random < 12 ? 'rock' : 'crater';
      default:
        return 'bush';
    }
  }

  /**
   * Render a decoration
   */
  private renderDecoration(gridX: number, gridY: number, type: Decoration['type']): void {
    const pos = this.gridSystem.gridToScreen(gridX, gridY);
    const graphics = this.scene.add.graphics();

    switch (type) {
      case 'tree':
        this.drawTree(graphics, pos.x, pos.y);
        break;
      case 'rock':
        this.drawRock(graphics, pos.x, pos.y);
        break;
      case 'bush':
        this.drawBush(graphics, pos.x, pos.y);
        break;
      case 'ruins':
        this.drawRuins(graphics, pos.x, pos.y);
        break;
      case 'sandbag':
        this.drawSandbag(graphics, pos.x, pos.y);
        break;
      case 'crater':
        this.drawCrater(graphics, pos.x, pos.y);
        break;
    }

    this.decorationLayer.add(graphics);
  }

  /**
   * Draw a tree
   */
  private drawTree(graphics: Phaser.GameObjects.Graphics, x: number, y: number): void {
    // Trunk
    graphics.fillStyle(0x4a3728, 1);
    graphics.fillRect(x - 2, y - 5, 4, 10);

    // Foliage
    graphics.fillStyle(0x2d5016, 1);
    graphics.fillCircle(x, y - 12, 8);
    graphics.fillStyle(0x3a6620, 1);
    graphics.fillCircle(x - 4, y - 10, 6);
    graphics.fillCircle(x + 4, y - 10, 6);
  }

  /**
   * Draw a rock
   */
  private drawRock(graphics: Phaser.GameObjects.Graphics, x: number, y: number): void {
    graphics.fillStyle(0x6b6b6b, 1);
    graphics.fillEllipse(x, y, 12, 8);
    graphics.fillStyle(0x8b8b8b, 0.5);
    graphics.fillEllipse(x - 2, y - 2, 8, 5);
  }

  /**
   * Draw a bush
   */
  private drawBush(graphics: Phaser.GameObjects.Graphics, x: number, y: number): void {
    graphics.fillStyle(0x3a6620, 1);
    graphics.fillCircle(x - 3, y, 5);
    graphics.fillCircle(x + 3, y, 5);
    graphics.fillCircle(x, y - 2, 6);
  }

  /**
   * Draw ruins
   */
  private drawRuins(graphics: Phaser.GameObjects.Graphics, x: number, y: number): void {
    graphics.fillStyle(0x8b7355, 1);
    graphics.fillRect(x - 6, y - 3, 5, 8);
    graphics.fillRect(x + 2, y, 4, 5);
    graphics.fillStyle(0x6b5845, 0.5);
    graphics.fillRect(x - 4, y - 5, 3, 4);
  }

  /**
   * Draw sandbags
   */
  private drawSandbag(graphics: Phaser.GameObjects.Graphics, x: number, y: number): void {
    graphics.fillStyle(0xb8a788, 1);
    graphics.fillRoundedRect(x - 8, y - 2, 7, 4, 2);
    graphics.fillRoundedRect(x + 1, y - 2, 7, 4, 2);
    graphics.fillRoundedRect(x - 4, y - 6, 7, 4, 2);
  }

  /**
   * Draw crater
   */
  private drawCrater(graphics: Phaser.GameObjects.Graphics, x: number, y: number): void {
    graphics.fillStyle(0x3a3020, 1);
    graphics.fillEllipse(x, y, 14, 8);
    graphics.fillStyle(0x2a2010, 0.5);
    graphics.fillEllipse(x - 2, y + 1, 8, 5);
  }

  /**
   * Cleanup
   */
  public destroy(): void {
    this.terrainLayer.destroy();
    this.decorationLayer.destroy();
  }
}
