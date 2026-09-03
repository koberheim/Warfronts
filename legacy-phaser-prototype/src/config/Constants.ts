/**
 * Game constants and configuration values
 */

import { GameConstants } from '../types/GameTypes';

export const GAME_CONSTANTS: GameConstants = {
  // Isometric tile dimensions (2:1 ratio)
  TILE_WIDTH: 64,
  TILE_HEIGHT: 32,

  // Canvas dimensions
  CANVAS_WIDTH: 1280,
  CANVAS_HEIGHT: 720,

  // Gameplay constants
  MAX_TOWER_LEVEL: 3
};

// Physics settings
export const PHYSICS_CONFIG = {
  gravity: { x: 0, y: 0 },
  debug: false
};

// Asset paths
export const ASSET_PATHS = {
  SPRITES: 'assets/sprites/',
  MAPS: 'assets/maps/',
  UI: 'assets/ui/',
  AUDIO_SFX: 'assets/audio/sfx/',
  AUDIO_MUSIC: 'assets/audio/music/',
  FONTS: 'assets/fonts/'
};

// Data file paths
export const DATA_PATHS = {
  TOWERS: 'src/data/towers.json',
  ENEMIES: 'src/data/enemies.json',
  MAPS: 'src/data/maps/',
  WAVES: 'src/data/waves/'
};

// UI colors
export const UI_COLORS = {
  PRIMARY: 0x4a4a4a,
  SECONDARY: 0x2c3e50,
  SUCCESS: 0x27ae60,
  DANGER: 0xe74c3c,
  WARNING: 0xf39c12,
  INFO: 0x3498db,
  TEXT: 0xffffff,
  TEXT_DARK: 0x2c3e50
};

// Grid colors
export const GRID_COLORS = {
  VALID: 0x00ff00,
  INVALID: 0xff0000,
  HOVER: 0xffff00,
  PATH: 0x0000ff
};
