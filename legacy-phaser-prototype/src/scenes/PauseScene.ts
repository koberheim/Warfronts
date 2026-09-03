/**
 * PauseScene - Pause menu overlay
 */

import Phaser from 'phaser';
import { SceneKey } from '../types/Enums';
import { GAME_CONSTANTS } from '../config/Constants';

export default class PauseScene extends Phaser.Scene {
  constructor() {
    super({ key: SceneKey.PAUSE });
  }

  /**
   * Create pause menu
   */
  create(): void {
    // Semi-transparent overlay
    this.add.rectangle(
      0,
      0,
      GAME_CONSTANTS.CANVAS_WIDTH,
      GAME_CONSTANTS.CANVAS_HEIGHT,
      0x000000,
      0.7
    ).setOrigin(0).setDepth(200);

    // Pause title
    this.add.text(
      GAME_CONSTANTS.CANVAS_WIDTH / 2,
      200,
      'PAUSED',
      {
        fontFamily: 'Arial',
        fontSize: '48px',
        color: '#ffffff',
        fontStyle: 'bold'
      }
    ).setOrigin(0.5).setDepth(201);

    // Resume button
    const resumeButton = this.createButton(
      GAME_CONSTANTS.CANVAS_WIDTH / 2,
      320,
      'Resume',
      0x27ae60
    );

    resumeButton.on('pointerdown', () => {
      this.resumeGame();
    });

    // Main menu button
    const menuButton = this.createButton(
      GAME_CONSTANTS.CANVAS_WIDTH / 2,
      400,
      'Main Menu',
      0x3498db
    );

    menuButton.on('pointerdown', () => {
      this.scene.stop(SceneKey.GAME);
      this.scene.stop(SceneKey.PAUSE);
      this.scene.start(SceneKey.MAIN_MENU);
    });

    // ESC to resume
    const escKey = this.input.keyboard?.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
    escKey?.on('down', () => this.resumeGame());
  }

  /**
   * Resume the game
   */
  private resumeGame(): void {
    this.scene.resume(SceneKey.GAME);
    this.scene.stop();
  }

  /**
   * Create a button
   */
  private createButton(x: number, y: number, text: string, color: number): Phaser.GameObjects.Text {
    const button = this.add.text(x, y, text, {
      fontFamily: 'Arial',
      fontSize: '24px',
      color: '#ffffff',
      backgroundColor: Phaser.Display.Color.ValueToColor(color).color.toString(16),
      padding: { x: 30, y: 12 }
    }).setOrigin(0.5).setInteractive().setDepth(201);

    button.on('pointerover', () => {
      button.setScale(1.05);
      this.input.setDefaultCursor('pointer');
    });

    button.on('pointerout', () => {
      button.setScale(1);
      this.input.setDefaultCursor('default');
    });

    return button;
  }
}
