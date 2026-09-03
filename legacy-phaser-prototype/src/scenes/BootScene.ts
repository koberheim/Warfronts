/**
 * BootScene - Initialize game plugins and configurations
 * First scene that runs when the game starts
 */

import Phaser from 'phaser';
import { SceneKey } from '../types/Enums';

export default class BootScene extends Phaser.Scene {
  constructor() {
    super({ key: SceneKey.BOOT });
  }

  /**
   * Initialize scene
   */
  init(): void {
    console.log('BootScene: Initializing...');
  }

  /**
   * Preload any critical assets needed for the loading screen
   */
  preload(): void {
    // Set base path for assets
    this.load.setBaseURL(window.location.origin);

    // Load any loading screen assets here if needed
    // For now, we'll use Phaser's built-in graphics
  }

  /**
   * Create scene objects and transition to PreloadScene
   */
  create(): void {
    console.log('BootScene: Complete. Starting PreloadScene...');

    // Transition to PreloadScene
    this.scene.start(SceneKey.PRELOAD);
  }
}
