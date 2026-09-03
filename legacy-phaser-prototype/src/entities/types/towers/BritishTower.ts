/**
 * BritishAntitank - QF 6-pounder Anti-Tank Gun
 * Powerful against armored enemies, slow fire rate
 */

import Tower from '../../Tower';
import { GridPosition } from '../../../types/GameTypes';
import { TowerType, Nation, ProjectileType } from '../../../types/Enums';

export default class BritishAntitank extends Tower {
  constructor(scene: Phaser.Scene, x: number, y: number, gridPosition: GridPosition) {
    super(scene, x, y, gridPosition, TowerType.BRITISH_ANTITANK, {
      nation: Nation.BRITAIN,
      damage: 45,
      range: 190,
      fireRate: 0.45,
      projectileSpeed: 520,
      projectileType: ProjectileType.SHELL,
      cost: 200,
      spriteKey: 'tower_british_at'
    });
  }
}
