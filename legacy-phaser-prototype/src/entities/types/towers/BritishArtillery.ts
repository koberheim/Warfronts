/**
 * BritishArtillery - QF 25-pounder Field Gun
 * Long range, high damage, slow fire rate
 */

import Tower from '../../Tower';
import { GridPosition } from '../../../types/GameTypes';
import { TowerType, Nation, ProjectileType } from '../../../types/Enums';

export default class BritishArtillery extends Tower {
  constructor(scene: Phaser.Scene, x: number, y: number, gridPosition: GridPosition) {
    super(scene, x, y, gridPosition, TowerType.BRITISH_ARTILLERY, {
      nation: Nation.BRITAIN,
      damage: 70,
      range: 280,
      fireRate: 0.25,
      projectileSpeed: 320,
      projectileType: ProjectileType.SHELL,
      cost: 300,
      spriteKey: 'tower_british_artillery'
    });
  }
}
