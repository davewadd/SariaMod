using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Emerald;
using SariaMod.Items.Strange;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using SariaMod.Items.Sapphire;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;

namespace SariaMod
{
    /// <summary>
    /// Reusable helper for playing a looping sound with optional fade-in and fade-out.
    /// Call Play() to start (fades in if fadeIn=true), Stop() to begin fading out, Update() every tick.
    /// </summary>
    public class FadingSoundPlayer
    {
        private SoundEffectInstance _instance;
        private float _currentVolume;
        private readonly float _fadeSpeed;
        private bool _fadingOut;
        private bool _fadeIn;
        private string _soundPath;

        public bool IsPlaying => _instance != null && _instance.State != SoundState.Stopped;

        /// <summary>
        /// Scales the maximum volume the sound fades up to. Default 1.0. Change any time — takes effect next Update().
        /// </summary>
        public float TargetVolume { get; set; } = 1f;

        public FadingSoundPlayer(float fadeSpeed = 1f / 60f)
        {
            _fadeSpeed = fadeSpeed;
        }

        /// <summary>
        /// Starts playing the looping sound. Fades in from silence if fadeIn is true.
        /// Does nothing if already playing.
        /// </summary>
        public void Play(string soundPath, bool fadeIn = true)
        {
            if (IsPlaying) return;

            _soundPath = soundPath;
            _fadeIn = fadeIn;
            _fadingOut = false;

            Asset<SoundEffect> asset = ModContent.Request<SoundEffect>(soundPath);
            if (asset == null || !asset.IsLoaded) return;

            _instance = asset.Value.CreateInstance();
            if (_instance == null) return;

            _instance.IsLooped = true;
            _currentVolume = fadeIn ? 0f : TargetVolume;
            _instance.Volume = _currentVolume * Main.ambientVolume;
            _instance.Play();
        }

        /// <summary>
        /// Begins fading out the sound. It will stop automatically once silent.
        /// </summary>
        public void Stop()
        {
            if (_instance == null) return;
            _fadingOut = true;
        }

        /// <summary>
        /// Pauses the sound immediately (e.g. on focus loss).
        /// </summary>
        public void Pause()
        {
            if (_instance != null && _instance.State == SoundState.Playing)
                _instance.Pause();
        }

        /// <summary>
        /// Resumes the sound if it was paused.
        /// </summary>
        public void Resume()
        {
            if (_instance != null && _instance.State == SoundState.Paused)
                _instance.Resume();
        }

        /// <summary>
        /// Stops and discards the instance immediately with no fade.
        /// </summary>
        public void StopImmediate()
        {
            if (_instance == null) return;
            _instance.Stop(immediate: true);
            _instance = null;
            _currentVolume = 0f;
            _fadingOut = false;
        }

        /// <summary>
        /// Call every tick to drive volume fading.
        /// </summary>
        public void Update()
        {
            if (_instance == null) return;

            if (_instance.State == SoundState.Stopped)
            {
                _instance = null;
                return;
            }

            if (_fadingOut)
            {
                _currentVolume = Math.Max(_currentVolume - _fadeSpeed, 0f);
                _instance.Volume = _currentVolume * Main.ambientVolume;
                if (_currentVolume <= 0f)
                {
                    _instance.Stop(immediate: true);
                    _instance = null;
                }
            }
            else if (_fadeIn && _currentVolume < TargetVolume)
            {
                _currentVolume = Math.Min(_currentVolume + _fadeSpeed, TargetVolume);
                _instance.Volume = _currentVolume * Main.ambientVolume;
            }
            else
            {
                // Smoothly fade toward TargetVolume in case it changed while playing (e.g. going indoors)
                if (_currentVolume < TargetVolume)
                    _currentVolume = Math.Min(_currentVolume + _fadeSpeed, TargetVolume);
                else if (_currentVolume > TargetVolume)
                    _currentVolume = Math.Max(_currentVolume - _fadeSpeed, TargetVolume);
                _instance.Volume = _currentVolume * Main.ambientVolume;
            }
        }
    }
}
