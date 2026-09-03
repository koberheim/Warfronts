/**
 * Phaser game configuration
 */

import Phaser from 'phaser';
import { GAME_CONSTANTS, PHYSICS_CONFIG } from './Constants';
import BootScene from '../scenes/BootScene';
import PreloadScene from '../scenes/PreloadScene';
import MainMenuScene from '../scenes/MainMenuScene';
import GameScene from '../scenes/GameScene';
import PauseScene from '../scenes/PauseScene';

export const gameConfig: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  width: GAME_CONSTANTS.CANVAS_WIDTH,
  height: GAME_CONSTANTS.CANVAS_HEIGHT,
  parent: 'game-container',
  backgroundColor: '#2d4a2b',
  scale: {
    mode: Phaser.Scale.FIT,
    autoCenter: Phaser.Scale.CENTER_BOTH
  },
  physics: {
    default: 'arcade',
    arcade: {
      ...PHYSICS_CONFIG
    }
  },
  render: {
    pixelArt: true,
    antialias: false,
    roundPixels: true
  },
  scene: [
    BootScene,
    PreloadScene,
    MainMenuScene,
    GameScene,
    PauseScene
  ]
};
