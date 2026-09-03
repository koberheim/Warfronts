/**
 * WW2 Tower Defense Game
 * Entry point
 */

import Phaser from 'phaser';
import { gameConfig } from './config/GameConfig';

// Initialize the game when DOM is ready
window.addEventListener('load', () => {
  const game = new Phaser.Game(gameConfig);

  // Expose game instance for debugging
  if (import.meta.env.DEV) {
    (window as any).game = game;
  }

  console.log('WW2 Tower Defense initialized');
  console.log(`Phaser version: ${Phaser.VERSION}`);
});
