/**
 * BritishMachinegun - Vickers Machine Gun
 * Fast fire rate, moderate damage, good for swarms
 */

import Tower from '../../Tower';
import { GridPosition } from '../../../types/GameTypes';
import { TowerType, Nation, ProjectileType } from '../../../types/Enums';

export default class BritishMachinegun extends Tower {
  constructor(scene: Phaser.Scene, x: number, y: number, gridPosition: GridPosition) {
    super(scene, x, y, gridPosition, TowerType.BRITISH_MACHINEGUN, {
      nation: Nation.BRITAIN,
      damage: 6,
      range: 140,
      fireRate: 3.5,
      projectileSpeed: 480,
      projectileType: ProjectileType.BULLET,
      cost: 150,
      spriteKey: 'tower_british_mg'
    });
  }
}
