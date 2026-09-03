/**
 * GermanEnemy - Wehrmacht Infantry
 * Standard infantry unit
 */

import Enemy from '../../Enemy';
import { ScreenPosition } from '../../../types/GameTypes';
import { EnemyType, Nation } from '../../../types/Enums';

export default class GermanEnemy extends Enemy {
  constructor(scene: Phaser.Scene, x: number, y: number, pathScreenPositions: ScreenPosition[]) {
    super(scene, x, y, pathScreenPositions, EnemyType.GERMAN_INFANTRY, {
      nation: Nation.GERMANY,
      health: 50,
      speed: 50,
      armor: 0,
      reward: 10,
      isArmored: false,
      spriteKey: 'enemy_german'
    });
  }
}
