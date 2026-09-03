/**
 * AudioManager - Handles sound effects and music
 */

import Phaser from 'phaser';

export default class AudioManager {
  private scene: Phaser.Scene;
  private musicVolume: number;
  private sfxVolume: number;
  private currentMusic?: Phaser.Sound.BaseSound;
  private isMusicMuted: boolean;
  private isSfxMuted: boolean;

  /**
   * Create a new AudioManager
   */
  constructor(scene: Phaser.Scene) {
    this.scene = scene;
    this.musicVolume = 0.6;
    this.sfxVolume = 0.7;
    this.isMusicMuted = false;
    this.isSfxMuted = false;
  }

  /**
   * Play a sound effect
   */
  public playSfx(key: string, volume: number = 1.0): void {
    if (this.isSfxMuted) return;

    // For now, create placeholder sounds using Phaser's sound synthesis
    // In a full implementation, these would be actual audio files
    this.playPlaceholderSfx(key, volume);
  }

  /**
   * Play placeholder sound effect (since we don't have actual audio files yet)
   */
  private playPlaceholderSfx(key: string, volume: number): void {
    // Create different beep tones for different sound effects
    const frequencies: { [key: string]: number } = {
      'tower_place': 440,    // A note
      'tower_shoot': 523,    // C note
      'enemy_hit': 349,      // F note
      'enemy_death': 294,    // D note
      'projectile_fire': 659, // E note
      'wave_start': 392,     // G note
      'wave_complete': 523,  // C note
      'money_gain': 880,     // A high
      'game_over': 196       // G low
    };

    const frequency = frequencies[key] || 440;

    // Note: In production, this would load and play actual audio files
    // For now, we'll just log that the sound would play
    if (this.scene.game.config.physics.arcade?.debug) {
      console.log(`🔊 SFX: ${key} (${frequency}Hz) at volume ${volume * this.sfxVolume}`);
    }
  }

  /**
   * Play background music
   */
  public playMusic(key: string, loop: boolean = true): void {
    if (this.isMusicMuted) return;

    // Stop current music
    if (this.currentMusic) {
      this.currentMusic.stop();
    }

    // Note: In production, this would load and play actual music files
    console.log(`🎵 Music: ${key} (loop: ${loop})`);
  }

  /**
   * Stop current music
   */
  public stopMusic(): void {
    if (this.currentMusic) {
      this.currentMusic.stop();
      this.currentMusic = undefined;
    }
  }

  /**
   * Set music volume
   */
  public setMusicVolume(volume: number): void {
    this.musicVolume = Phaser.Math.Clamp(volume, 0, 1);
    // Note: Volume setting would be applied here when actual audio is implemented
  }

  /**
   * Set SFX volume
   */
  public setSfxVolume(volume: number): void {
    this.sfxVolume = Phaser.Math.Clamp(volume, 0, 1);
  }

  /**
   * Toggle music mute
   */
  public toggleMusicMute(): void {
    this.isMusicMuted = !this.isMusicMuted;
    if (this.isMusicMuted && this.currentMusic) {
      this.currentMusic.pause();
    } else if (!this.isMusicMuted && this.currentMusic) {
      this.currentMusic.resume();
    }
  }

  /**
   * Toggle SFX mute
   */
  public toggleSfxMute(): void {
    this.isSfxMuted = !this.isSfxMuted;
  }

  /**
   * Get music volume
   */
  public getMusicVolume(): number {
    return this.musicVolume;
  }

  /**
   * Get SFX volume
   */
  public getSfxVolume(): number {
    return this.sfxVolume;
  }

  /**
   * Check if music is muted
   */
  public isMusicMutedState(): boolean {
    return this.isMusicMuted;
  }

  /**
   * Check if SFX is muted
   */
  public isSfxMutedState(): boolean {
    return this.isSfxMuted;
  }

  /**
   * Cleanup
   */
  public destroy(): void {
    this.stopMusic();
  }
}
