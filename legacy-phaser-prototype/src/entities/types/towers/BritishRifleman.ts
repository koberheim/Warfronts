/**
 * BritishRifleman - Lee-Enfield Infantry
 * Standard infantry unit with good all-around stats
 */

import Tower from '../../Tower';
import { GridPosition } from '../../../types/GameTypes';
import { TowerType, Nation, ProjectileType } from '../../../types/Enums';

export default class BritishRifleman extends Tower {
  constructor(scene: Phaser.Scene, x: number, y: number, gridPosition: GridPosition) {
    super(scene, x, y, gridPosition, TowerType.BRITISH_RIFLEMAN, {
      nation: Nation.BRITAIN,
      damage: 12,
      range: 160,
      fireRate: 0.9,
      projectileSpeed: 420,
      projectileType: ProjectileType.BULLET,
      cost: 100,
      spriteKey: 'tower_british_rifleman'
    });
  }
}
