/**
 * Game-wide enumerations
 */

export enum Nation {
  USA = 'USA',
  BRITAIN = 'BRITAIN',
  FRANCE = 'FRANCE',
  CANADA = 'CANADA',
  GERMANY = 'GERMANY',
  ITALY = 'ITALY',
  JAPAN = 'JAPAN'
}

export enum TowerType {
  BRITISH_RIFLEMAN = 'british_rifleman',
  BRITISH_MACHINEGUN = 'british_machinegun',
  BRITISH_ANTITANK = 'british_antitank',
  BRITISH_ARTILLERY = 'british_artillery',
  BRITISH_MORTAR = 'british_mortar'
}

export enum EnemyType {
  GERMAN_INFANTRY = 'german_infantry',
  ITALIAN_LIGHTTANK = 'italian_lighttank',
  JAPAN_HEAVYTANK = 'japan_heavytank'
}

export enum ProjectileType {
  BULLET = 'bullet',
  SHELL = 'shell',
  ROCKET = 'rocket'
}

export enum GameState {
  MENU = 'MENU',
  PLAYING = 'PLAYING',
  PAUSED = 'PAUSED',
  VICTORY = 'VICTORY',
  DEFEAT = 'DEFEAT'
}

export enum SceneKey {
  BOOT = 'BootScene',
  PRELOAD = 'PreloadScene',
  MAIN_MENU = 'MainMenuScene',
  GAME = 'GameScene',
  UI = 'UIScene',
  PAUSE = 'PauseScene'
}
