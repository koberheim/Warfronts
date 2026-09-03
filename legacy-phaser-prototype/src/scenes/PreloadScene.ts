/**
 * PreloadScene - Load all game assets
 * Shows loading progress bar while loading
 */

import Phaser from 'phaser';
import { SceneKey } from '../types/Enums';
import { GAME_CONSTANTS } from '../config/Constants';

export default class PreloadScene extends Phaser.Scene {
  private loadingBar!: Phaser.GameObjects.Graphics;
  private progressBar!: Phaser.GameObjects.Graphics;
  private loadingText!: Phaser.GameObjects.Text;
  private percentText!: Phaser.GameObjects.Text;

  constructor() {
    super({ key: SceneKey.PRELOAD });
  }

  /**
   * Initialize loading screen
   */
  init(): void {
    console.log('PreloadScene: Initializing...');
  }

  /**
   * Create loading screen UI
   */
  preload(): void {
    const centerX = GAME_CONSTANTS.CANVAS_WIDTH / 2;
    const centerY = GAME_CONSTANTS.CANVAS_HEIGHT / 2;

    // Create loading text
    this.loadingText = this.add.text(centerX, centerY - 50, 'Loading...', {
      fontFamily: 'Arial',
      fontSize: '32px',
      color: '#ffffff'
    }).setOrigin(0.5);

    // Create percent text
    this.percentText = this.add.text(centerX, centerY + 50, '0%', {
      fontFamily: 'Arial',
      fontSize: '24px',
      color: '#ffffff'
    }).setOrigin(0.5);

    // Create progress bar background
    this.loadingBar = this.add.graphics();
    this.loadingBar.fillStyle(0x222222, 0.8);
    this.loadingBar.fillRect(centerX - 200, centerY - 25, 400, 50);

    // Create progress bar fill
    this.progressBar = this.add.graphics();

    // Listen to loading events
    this.load.on('progress', this.onProgress, this);
    this.load.on('fileprogress', this.onFileProgress, this);
    this.load.on('complete', this.onComplete, this);

    // Load game assets
    this.loadAssets();
  }

  /**
   * Load all game assets
   */
  private loadAssets(): void {
    // For now, we'll create placeholder graphics
    // In later phases, we'll load actual sprites, audio, and data files

    // Example: Load JSON data files (will be created in Phase 2)
    // this.load.json('towers', 'src/data/towers.json');
    // this.load.json('enemies', 'src/data/enemies.json');

    console.log('PreloadScene: Loading assets...');
  }

  /**
   * Update progress bar as assets load
   */
  private onProgress(progress: number): void {
    const centerX = GAME_CONSTANTS.CANVAS_WIDTH / 2;
    const centerY = GAME_CONSTANTS.CANVAS_HEIGHT / 2;

    // Update progress bar
    this.progressBar.clear();
    this.progressBar.fillStyle(0x27ae60, 1);
    this.progressBar.fillRect(centerX - 195, centerY - 20, 390 * progress, 40);

    // Update percent text
    this.percentText.setText(Math.floor(progress * 100) + '%');
  }

  /**
   * Log individual file progress
   */
  private onFileProgress(file: Phaser.Loader.File): void {
    console.log(`Loading: ${file.key}`);
  }

  /**
   * All assets loaded, transition to next scene
   */
  private onComplete(): void {
    console.log('PreloadScene: All assets loaded');

    // Clean up loading screen
    this.progressBar.destroy();
    this.loadingBar.destroy();
    this.loadingText.destroy();
    this.percentText.destroy();

    // Transition to MainMenuScene
    console.log('PreloadScene: Transitioning to MainMenuScene...');
    this.scene.start(SceneKey.MAIN_MENU);
  }
}
