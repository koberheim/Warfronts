/**
 * BritishMortar - 3-inch Mortar
 * Moderate range, area damage potential, moderate fire rate
 */

import Tower from '../../Tower';
import { GridPosition } from '../../../types/GameTypes';
import { TowerType, Nation, ProjectileType } from '../../../types/Enums';

export default class BritishMortar extends Tower {
  constructor(scene: Phaser.Scene, x: number, y: number, gridPosition: GridPosition) {
    super(scene, x, y, gridPosition, TowerType.BRITISH_MORTAR, {
      nation: Nation.BRITAIN,
      damage: 30,
      range: 220,
      fireRate: 0.6,
      projectileSpeed: 280,
      projectileType: ProjectileType.SHELL,
      cost: 250,
      spriteKey: 'tower_british_mortar'
    });
  }
}
