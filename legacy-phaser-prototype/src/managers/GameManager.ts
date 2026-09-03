/**
 * GameManager - Central coordinator for game state
 * Manages lives, waves, win/loss conditions
 */

import Phaser from 'phaser';
import { GameState } from '../types/Enums';

export default class GameManager {
  private scene: Phaser.Scene;
  private currentWave: number;
  private lives: number;
  private startingLives: number;
  private gameState: GameState;
  private isWaveActive: boolean;

  /**
   * Create a new GameManager
   */
  constructor(scene: Phaser.Scene, startingLives: number = 20) {
    this.scene = scene;
    this.startingLives = startingLives;
    this.lives = startingLives;
    this.currentWave = 0;
    this.gameState = GameState.PLAYING;
    this.isWaveActive = false;

    // Listen to game events
    this.setupEventListeners();
  }

  /**
   * Set up event listeners
   */
  private setupEventListeners(): void {
    // Enemy reached end - lose a life
    this.scene.events.on('enemy-reached-end', this.onEnemyReachedEnd, this);

    // Wave completed
    this.scene.events.on('wave-completed', this.onWaveCompleted, this);
  }

  /**
   * Start a new wave
   */
  public startWave(): void {
    if (this.isWaveActive) {
      console.warn('Wave already active');
      return;
    }

    this.currentWave++;
    this.isWaveActive = true;

    console.log(`Starting wave ${this.currentWave}`);
    this.scene.events.emit('wave-started', this.currentWave);
  }

  /**
   * Handle enemy reaching the end
   */
  private onEnemyReachedEnd(_enemy: any): void {
    this.loseLife();
  }

  /**
   * Lose a life
   */
  private loseLife(): void {
    this.lives--;
    this.scene.events.emit('lives-changed', this.lives);

    console.log(`Life lost! Remaining: ${this.lives}`);

    if (this.lives <= 0) {
      this.gameOver(false);
    }
  }

  /**
   * Handle wave completion
   */
  private onWaveCompleted(): void {
    this.isWaveActive = false;
    console.log(`Wave ${this.currentWave} completed!`);
  }

  /**
   * Game over
   */
  private gameOver(victory: boolean): void {
    this.gameState = victory ? GameState.VICTORY : GameState.DEFEAT;
    this.scene.events.emit('game-over', victory);

    console.log(victory ? 'VICTORY!' : 'DEFEAT!');
  }

  /**
   * Pause game
   */
  public pause(): void {
    if (this.gameState === GameState.PLAYING) {
      this.gameState = GameState.PAUSED;
      this.scene.events.emit('game-paused');
    }
  }

  /**
   * Resume game
   */
  public resume(): void {
    if (this.gameState === GameState.PAUSED) {
      this.gameState = GameState.PLAYING;
      this.scene.events.emit('game-resumed');
    }
  }

  /**
   * Trigger victory
   */
  public triggerVictory(): void {
    this.gameOver(true);
  }

  /**
   * Get current wave number
   */
  public getCurrentWave(): number {
    return this.currentWave;
  }

  /**
   * Get current lives
   */
  public getLives(): number {
    return this.lives;
  }

  /**
   * Get game state
   */
  public getGameState(): GameState {
    return this.gameState;
  }

  /**
   * Check if wave is active
   */
  public isWaveInProgress(): boolean {
    return this.isWaveActive;
  }

  /**
   * Reset game
   */
  public reset(): void {
    this.lives = this.startingLives;
    this.currentWave = 0;
    this.gameState = GameState.PLAYING;
    this.isWaveActive = false;

    this.scene.events.emit('game-reset');
  }

  /**
   * Cleanup
   */
  public destroy(): void {
    this.scene.events.off('enemy-reached-end', this.onEnemyReachedEnd, this);
    this.scene.events.off('wave-completed', this.onWaveCompleted, this);
  }
}
