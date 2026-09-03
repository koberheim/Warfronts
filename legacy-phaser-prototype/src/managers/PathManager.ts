/**
 * PathManager - Handles enemy movement paths
 */

import Phaser from 'phaser';
import { GridPosition, ScreenPosition } from '../types/GameTypes';
import GridSystem from '../systems/GridSystem';

export default class PathManager {
  private scene: Phaser.Scene;
  private pathWaypoints: GridPosition[];
  private pathScreenPositions: ScreenPosition[];

  /**
   * Create a new PathManager
   */
  constructor(scene: Phaser.Scene, gridSystem: GridSystem, path: GridPosition[]) {
    this.scene = scene;
    this.pathWaypoints = path;

    // Convert grid positions to screen positions
    this.pathScreenPositions = path.map(waypoint =>
      gridSystem.gridToScreen(waypoint.x, waypoint.y)
    );
  }

  /**
   * Get path waypoints in grid coordinates
   */
  public getPathWaypoints(): GridPosition[] {
    return this.pathWaypoints;
  }

  /**
   * Get path in screen coordinates
   */
  public getPathScreenPositions(): ScreenPosition[] {
    return this.pathScreenPositions;
  }

  /**
   * Get spawn position (first waypoint)
   */
  public getSpawnPosition(): ScreenPosition {
    return this.pathScreenPositions[0];
  }

  /**
   * Get end position (last waypoint)
   */
  public getEndPosition(): ScreenPosition {
    return this.pathScreenPositions[this.pathScreenPositions.length - 1];
  }

  /**
   * Visualize the path (for debugging)
   */
  public visualizePath(color: number = 0x00ff00, alpha: number = 0.5): void {
    const graphics = this.scene.add.graphics();
    graphics.lineStyle(3, color, alpha);

    // Draw path
    for (let i = 0; i < this.pathScreenPositions.length - 1; i++) {
      const current = this.pathScreenPositions[i];
      const next = this.pathScreenPositions[i + 1];

      graphics.lineBetween(current.x, current.y, next.x, next.y);

      // Draw waypoint markers
      graphics.fillStyle(color, alpha);
      graphics.fillCircle(current.x, current.y, 5);
    }

    // Draw last waypoint
    const last = this.pathScreenPositions[this.pathScreenPositions.length - 1];
    graphics.fillCircle(last.x, last.y, 5);

    graphics.setDepth(2);
  }

  /**
   * Check if a grid position is on the path
   */
  public isOnPath(gridX: number, gridY: number): boolean {
    return this.pathWaypoints.some(waypoint =>
      waypoint.x === gridX && waypoint.y === gridY
    );
  }
}
