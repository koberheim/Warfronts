/**
 * MainMenuScene - Main menu with map selection
 */

import Phaser from 'phaser';
import { SceneKey } from '../types/Enums';
import { GAME_CONSTANTS } from '../config/Constants';

export default class MainMenuScene extends Phaser.Scene {
  constructor() {
    super({ key: SceneKey.MAIN_MENU });
  }

  /**
   * Create menu
   */
  create(): void {
    // Background
    this.cameras.main.setBackgroundColor('#1a1a1a');

    // Title
    this.add.text(
      GAME_CONSTANTS.CANVAS_WIDTH / 2,
      150,
      'WW2 TOWER DEFENSE',
      {
        fontFamily: 'Arial',
        fontSize: '56px',
        color: '#ffffff',
        fontStyle: 'bold',
        stroke: '#000000',
        strokeThickness: 4
      }
    ).setOrigin(0.5);

    // Subtitle
    this.add.text(
      GAME_CONSTANTS.CANVAS_WIDTH / 2,
      220,
      'Command British Forces Against the Axis',
      {
        fontFamily: 'Arial',
        fontSize: '20px',
        color: '#95a5a6',
        fontStyle: 'italic'
      }
    ).setOrigin(0.5);

    // Play button
    const playButton = this.createButton(
      GAME_CONSTANTS.CANVAS_WIDTH / 2,
      350,
      'START GAME',
      0x27ae60
    );

    playButton.on('pointerdown', () => {
      this.scene.start(SceneKey.GAME);
    });

    // Info text
    this.add.text(
      GAME_CONSTANTS.CANVAS_WIDTH / 2,
      500,
      'Phase 4-5 Complete!\nFull tower defense gameplay with waves, economy, and combat',
      {
        fontFamily: 'Arial',
        fontSize: '16px',
        color: '#3498db',
        align: 'center'
      }
    ).setOrigin(0.5);

    // Controls info
    this.add.text(
      GAME_CONSTANTS.CANVAS_WIDTH / 2,
      GAME_CONSTANTS.CANVAS_HEIGHT - 60,
      'Controls: Number keys (1-5) to select towers | Click to place | SPACE to start waves',
      {
        fontFamily: 'Arial',
        fontSize: '14px',
        color: '#7f8c8d',
        align: 'center'
      }
    ).setOrigin(0.5);

    // Version
    this.add.text(
      20,
      GAME_CONSTANTS.CANVAS_HEIGHT - 30,
      'v0.1.0 - Phase 5',
      {
        fontFamily: 'Arial',
        fontSize: '12px',
        color: '#555555'
      }
    );
  }

  /**
   * Create a button
   */
  private createButton(x: number, y: number, text: string, color: number): Phaser.GameObjects.Text {
    const button = this.add.text(x, y, text, {
      fontFamily: 'Arial',
      fontSize: '32px',
      color: '#ffffff',
      backgroundColor: Phaser.Display.Color.ValueToColor(color).color.toString(16),
      padding: { x: 40, y: 15 }
    }).setOrigin(0.5).setInteractive();

    // Hover effects
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
