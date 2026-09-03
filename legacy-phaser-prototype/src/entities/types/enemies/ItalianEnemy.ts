/**
 * ItalianEnemy - Light Tank
 * Faster armored unit with moderate health
 */

import Enemy from '../../Enemy';
import { ScreenPosition } from '../../../types/GameTypes';
import { EnemyType, Nation } from '../../../types/Enums';

export default class ItalianEnemy extends Enemy {
  constructor(scene: Phaser.Scene, x: number, y: number, pathScreenPositions: ScreenPosition[]) {
    super(scene, x, y, pathScreenPositions, EnemyType.ITALIAN_LIGHTTANK, {
      nation: Nation.ITALY,
      health: 100,
      speed: 40,
      armor: 5,
      reward: 20,
      isArmored: true,
      spriteKey: 'enemy_italian'
    });
  }
}
