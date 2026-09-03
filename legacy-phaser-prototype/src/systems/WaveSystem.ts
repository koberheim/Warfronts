/**
 * WaveSystem - Manages enemy wave spawning
 * Reads wave configurations and spawns enemies
 */

import Phaser from 'phaser';
import { WaveDefinition, GridPosition, ScreenPosition } from '../types/GameTypes';
import { EnemyType } from '../types/Enums';
import Enemy from '../entities/Enemy';
import GermanEnemy from '../entities/types/enemies/GermanEnemy';
import ItalianEnemy from '../entities/types/enemies/ItalianEnemy';
import JapanEnemy from '../entities/types/enemies/JapanEnemy';
import GridSystem from './GridSystem';

export default class WaveSystem {
  private scene: Phaser.Scene;
  private pathScreenPositions: ScreenPosition[];
  private waveDefinitions: WaveDefinition[];
  private currentWaveIndex: number;
  private isSpawning: boolean;
  private spawnQueue: Array<{ enemyType: EnemyType; delay: number }>;
  private spawnTimer: number;

  /**
   * Create a new WaveSystem
   */
  constructor(
    scene: Phaser.Scene,
    gridSystem: GridSystem,
    path: GridPosition[],
    waveDefinitions: WaveDefinition[]
  ) {
    this.scene = scene;
    this.waveDefinitions = waveDefinitions;
    this.currentWaveIndex = -1;
    this.isSpawning = false;
    this.spawnQueue = [];
    this.spawnTimer = 0;

    // Convert path to screen positions
    this.pathScreenPositions = path.map(waypoint =>
      gridSystem.gridToScreen(waypoint.x, waypoint.y)
    );
  }

  /**
   * Start the next wave
   */
  public startNextWave(): boolean {
    if (this.isSpawning) {
      console.warn('Wave already spawning');
      return false;
    }

    this.currentWaveIndex++;

    if (this.currentWaveIndex >= this.waveDefinitions.length) {
      console.log('All waves completed!');
      this.scene.events.emit('all-waves-completed');
      return false;
    }

    const wave = this.waveDefinitions[this.currentWaveIndex];
    console.log(`Starting wave ${wave.waveNumber}`);

    // Build spawn queue from wave definition
    this.buildSpawnQueue(wave);

    this.isSpawning = true;
    this.spawnTimer = 0;

    this.scene.events.emit('wave-started', wave.waveNumber);

    return true;
  }

  /**
   * Build spawn queue from wave definition
   */
  private buildSpawnQueue(wave: WaveDefinition): void {
    this.spawnQueue = [];

    wave.spawns.forEach(spawn => {
      let cumulativeDelay = spawn.delay;

      for (let i = 0; i < spawn.count; i++) {
        this.spawnQueue.push({
          enemyType: spawn.enemyType,
          delay: cumulativeDelay
        });

        cumulativeDelay += spawn.interval;
      }
    });

    // Sort by delay
    this.spawnQueue.sort((a, b) => a.delay - b.delay);
  }

  /**
   * Update wave system
   */
  public update(_time: number, delta: number, activeEnemies: Enemy[]): void {
    if (!this.isSpawning) return;

    this.spawnTimer += delta;

    // Spawn enemies from queue
    while (this.spawnQueue.length > 0 && this.spawnTimer >= this.spawnQueue[0].delay) {
      const spawn = this.spawnQueue.shift()!;
      this.spawnEnemy(spawn.enemyType);
    }

    // Check if wave is complete
    if (this.spawnQueue.length === 0 && activeEnemies.length === 0) {
      this.completeWave();
    }
  }

  /**
   * Spawn an enemy
   */
  private spawnEnemy(enemyType: EnemyType): void {
    const spawnPos = this.pathScreenPositions[0];

    let enemy: Enemy | null = null;

    switch (enemyType) {
      case EnemyType.GERMAN_INFANTRY:
        enemy = new GermanEnemy(this.scene, spawnPos.x, spawnPos.y, this.pathScreenPositions);
        break;

      case EnemyType.ITALIAN_LIGHTTANK:
        enemy = new ItalianEnemy(this.scene, spawnPos.x, spawnPos.y, this.pathScreenPositions);
        break;

      case EnemyType.JAPAN_HEAVYTANK:
        enemy = new JapanEnemy(this.scene, spawnPos.x, spawnPos.y, this.pathScreenPositions);
        break;

      default:
        console.error(`Unknown enemy type: ${enemyType}`);
        return;
    }

    if (enemy) {
      this.scene.events.emit('enemy-spawned', enemy);
      console.log(`Spawned ${enemyType}`);
    }
  }

  /**
   * Complete the current wave
   */
  private completeWave(): void {
    this.isSpawning = false;

    const wave = this.waveDefinitions[this.currentWaveIndex];
    console.log(`Wave ${wave.waveNumber} completed!`);

    // Award bonus money if defined
    if (wave.reward) {
      this.scene.events.emit('wave-reward', wave.reward);
    }

    this.scene.events.emit('wave-completed', wave.waveNumber);
  }

  /**
   * Get current wave number
   */
  public getCurrentWaveNumber(): number {
    return this.currentWaveIndex + 1;
  }

  /**
   * Get total wave count
   */
  public getTotalWaves(): number {
    return this.waveDefinitions.length;
  }

  /**
   * Check if spawning
   */
  public isWaveActive(): boolean {
    return this.isSpawning;
  }

  /**
   * Get remaining enemies in queue
   */
  public getRemainingSpawns(): number {
    return this.spawnQueue.length;
  }

  /**
   * Reset wave system
   */
  public reset(): void {
    this.currentWaveIndex = -1;
    this.isSpawning = false;
    this.spawnQueue = [];
    this.spawnTimer = 0;
  }
}
