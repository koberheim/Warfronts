/**
 * JapanEnemy - Heavy Tank
 * Slow but heavily armored with high health
 */

import Enemy from '../../Enemy';
import { ScreenPosition } from '../../../types/GameTypes';
import { EnemyType, Nation } from '../../../types/Enums';

export default class JapanEnemy extends Enemy {
  constructor(scene: Phaser.Scene, x: number, y: number, pathScreenPositions: ScreenPosition[]) {
    super(scene, x, y, pathScreenPositions, EnemyType.JAPAN_HEAVYTANK, {
      nation: Nation.JAPAN,
      health: 200,
      speed: 25,
      armor: 10,
      reward: 40,
      isArmored: true,
      spriteKey: 'enemy_japan'
    });
  }
}
