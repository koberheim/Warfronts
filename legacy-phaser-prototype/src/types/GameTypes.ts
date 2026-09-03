/**
 * TypeScript interfaces and types for the game
 */

import { Nation, EnemyType, ProjectileType } from './Enums';

/**
 * Grid position in isometric coordinates
 */
export interface GridPosition {
  x: number;
  y: number;
}

/**
 * Screen position in pixels
 */
export interface ScreenPosition {
  x: number;
  y: number;
}

/**
 * Tower upgrade definition
 */
export interface TowerUpgrade {
  level: number;
  cost: number;
  damageBonus?: number;
  rangeBonus?: number;
  fireRateBonus?: number;
  description?: string;
}

/**
 * Tower definition from JSON
 */
export interface TowerDefinition {
  id: string;
  name: string;
  nation: Nation;
  description: string;
  cost: number;
  damage: number;
  range: number;
  fireRate: number;
  projectileSpeed: number;
  projectileType: ProjectileType;
  spriteKey: string;
  upgrades?: TowerUpgrade[];
}

/**
 * Enemy definition from JSON
 */
export interface EnemyDefinition {
  id: string;
  name: string;
  nation: Nation;
  description: string;
  health: number;
  speed: number;
  armor: number;
  reward: number;
  isArmored: boolean;
  spriteKey: string;
}

/**
 * Wave spawn configuration
 */
export interface WaveSpawn {
  enemyType: EnemyType;
  count: number;
  interval: number;  // milliseconds between spawns
  delay: number;     // milliseconds before first spawn
}

/**
 * Wave definition
 */
export interface WaveDefinition {
  waveNumber: number;
  spawns: WaveSpawn[];
  reward?: number;  // Bonus money for completing wave
}

/**
 * Map path waypoint
 */
export interface PathWaypoint {
  x: number;
  y: number;
}

/**
 * Map definition from JSON
 */
export interface MapDefinition {
  id: string;
  name: string;
  gridWidth: number;
  gridHeight: number;
  startingMoney: number;
  startingLives: number;
  path: PathWaypoint[];
  blockedTiles?: GridPosition[];
  decorations?: {
    spriteKey: string;
    position: GridPosition;
  }[];
}

/**
 * Game configuration constants
 */
export interface GameConstants {
  TILE_WIDTH: number;
  TILE_HEIGHT: number;
  CANVAS_WIDTH: number;
  CANVAS_HEIGHT: number;
  MAX_TOWER_LEVEL: number;
}
