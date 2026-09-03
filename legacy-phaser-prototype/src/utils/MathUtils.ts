/**
 * Mathematical utility functions
 */

import { ScreenPosition } from '../types/GameTypes';

/**
 * Calculate distance between two screen positions
 */
export function distance(pos1: ScreenPosition, pos2: ScreenPosition): number {
  const dx = pos2.x - pos1.x;
  const dy = pos2.y - pos1.y;
  return Math.sqrt(dx * dx + dy * dy);
}

/**
 * Calculate angle between two screen positions in radians
 */
export function angleBetween(pos1: ScreenPosition, pos2: ScreenPosition): number {
  return Math.atan2(pos2.y - pos1.y, pos2.x - pos1.x);
}

/**
 * Clamp a value between min and max
 */
export function clamp(value: number, min: number, max: number): number {
  return Math.max(min, Math.min(max, value));
}

/**
 * Linear interpolation
 */
export function lerp(start: number, end: number, t: number): number {
  return start + (end - start) * t;
}

/**
 * Check if a point is within a circle
 */
export function isPointInCircle(
  point: ScreenPosition,
  center: ScreenPosition,
  radius: number
): boolean {
  return distance(point, center) <= radius;
}

/**
 * Convert degrees to radians
 */
export function degToRad(degrees: number): number {
  return degrees * (Math.PI / 180);
}

/**
 * Convert radians to degrees
 */
export function radToDeg(radians: number): number {
  return radians * (180 / Math.PI);
}
