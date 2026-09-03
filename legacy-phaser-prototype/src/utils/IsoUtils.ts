/**
 * Isometric utility functions
 */

import { GridPosition, ScreenPosition } from '../types/GameTypes';
import { GAME_CONSTANTS } from '../config/Constants';

/**
 * Convert grid coordinates to screen position
 * Uses 2:1 isometric projection
 */
export function gridToScreen(gridX: number, gridY: number): ScreenPosition {
  const x = (gridX - gridY) * (GAME_CONSTANTS.TILE_WIDTH / 2);
  const y = (gridX + gridY) * (GAME_CONSTANTS.TILE_HEIGHT / 2);

  return { x, y };
}

/**
 * Convert screen position to grid coordinates
 * Inverse of gridToScreen
 */
export function screenToGrid(screenX: number, screenY: number): GridPosition {
  const tileWidth = GAME_CONSTANTS.TILE_WIDTH;
  const tileHeight = GAME_CONSTANTS.TILE_HEIGHT;

  const gridX = Math.floor((screenX / (tileWidth / 2) + screenY / (tileHeight / 2)) / 2);
  const gridY = Math.floor((screenY / (tileHeight / 2) - screenX / (tileWidth / 2)) / 2);

  return { x: gridX, y: gridY };
}

/**
 * Calculate distance between two grid positions
 */
export function gridDistance(pos1: GridPosition, pos2: GridPosition): number {
  const dx = pos2.x - pos1.x;
  const dy = pos2.y - pos1.y;
  return Math.sqrt(dx * dx + dy * dy);
}

/**
 * Check if two grid positions are equal
 */
export function gridEquals(pos1: GridPosition, pos2: GridPosition): boolean {
  return pos1.x === pos2.x && pos1.y === pos2.y;
}
