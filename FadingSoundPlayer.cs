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
        private readonly float _fadeOutSpeed;
        private bool _fadingOut;
        private bool _fadeIn;
        private string _soundPath;
        private bool _hasPrimarySpatialSource;
        private bool _hasSecondarySpatialSource;
        private Vector2 _primarySpatialSource;
        private Vector2 _secondarySpatialSource;

        public bool IsPlaying => _instance != null && _instance.State != SoundState.Stopped;

        /// <summary>
        /// Scales the maximum volume the sound fades up to. Default 1.0. Change any time — takes effect next Update().
        /// </summary>
        public float TargetVolume { get; set; } = 1f;

        public FadingSoundPlayer(float fadeSpeed = 1f / 60f, float? fadeOutSpeed = null)
        {
            _fadeSpeed = fadeSpeed;
            _fadeOutSpeed = fadeOutSpeed ?? fadeSpeed;
        }

        /// <summary>
        /// Spatializes the loop around one world position. Volume reaches zero
        /// beyond roughly one and a half screen widths.
        /// </summary>
        public void SetSpatialSource(Vector2 position)
        {
            _hasPrimarySpatialSource = true;
            _hasSecondarySpatialSource = false;
            _primarySpatialSource = position;
        }

        /// <summary>
        /// Spatializes one loop around two world positions without playing a
        /// second copy. The nearer source controls attenuation while both
        /// sources contribute to stereo panning.
        /// </summary>
        public void SetSpatialSources(Vector2 primaryPosition, Vector2 secondaryPosition)
        {
            _hasPrimarySpatialSource = true;
            _hasSecondarySpatialSource = true;
            _primarySpatialSource = primaryPosition;
            _secondarySpatialSource = secondaryPosition;
        }

        public void ClearSpatialSources()
        {
            _hasPrimarySpatialSource = false;
            _hasSecondarySpatialSource = false;
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

            Asset<SoundEffect> asset = ModContent.Request<SoundEffect>(
                soundPath,
                AssetRequestMode.ImmediateLoad);
            if (asset == null) return;

            _instance = asset.Value.CreateInstance();
            if (_instance == null) return;

            _instance.IsLooped = true;
            _currentVolume = fadeIn ? 0f : TargetVolume;
            ApplyOutputVolume();
            _instance.Play();
        }

        /// <summary>
        /// Starts this loop and registers it with the shared lifecycle manager.
        /// Managed loops update automatically and are cleaned up on menu exit,
        /// world unload, and mod unload.
        /// </summary>
        public void PlayManaged(string soundPath, bool fadeIn = true)
        {
            Play(soundPath, fadeIn);
            if (IsPlaying)
                LoopingSoundManager.Register(this);
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
                _currentVolume = Math.Max(_currentVolume - _fadeOutSpeed, 0f);
                if (_currentVolume <= 0f)
                {
                    _instance.Stop(immediate: true);
                    _instance = null;
                    return;
                }
            }
            else if (_fadeIn && _currentVolume < TargetVolume)
            {
                _currentVolume = Math.Min(_currentVolume + _fadeSpeed, TargetVolume);
            }
            else
            {
                // Smoothly fade toward TargetVolume in case it changed while playing (e.g. going indoors)
                if (_currentVolume < TargetVolume)
                    _currentVolume = Math.Min(_currentVolume + _fadeSpeed, TargetVolume);
                else if (_currentVolume > TargetVolume)
                    _currentVolume = Math.Max(_currentVolume - _fadeSpeed, TargetVolume);
            }

            ApplyOutputVolume();
        }

        private void ApplyOutputVolume()
        {
            if (_instance == null)
                return;

            float spatialGain = 1f;
            float pan = 0f;
            if (_hasPrimarySpatialSource)
            {
                Vector2 listenerPosition = Main.screenPosition
                    + new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
                float primaryGain = GetSpatialGain(_primarySpatialSource, listenerPosition);
                float primaryPan = GetSpatialPan(_primarySpatialSource, listenerPosition);
                spatialGain = primaryGain;
                pan = primaryPan;

                if (_hasSecondarySpatialSource)
                {
                    float secondaryGain = GetSpatialGain(_secondarySpatialSource, listenerPosition);
                    float secondaryPan = GetSpatialPan(_secondarySpatialSource, listenerPosition);
                    float totalWeight = primaryGain + secondaryGain;
                    spatialGain = Math.Max(primaryGain, secondaryGain);
                    pan = totalWeight > 0.001f
                        ? (primaryPan * primaryGain + secondaryPan * secondaryGain) / totalWeight
                        : 0f;
                }
            }

            _instance.Pan = MathHelper.Clamp(pan, -1f, 1f);
            _instance.Volume = MathHelper.Clamp(
                _currentVolume * Main.ambientVolume * spatialGain,
                0f,
                1f);
        }

        private static float GetSpatialGain(Vector2 sourcePosition, Vector2 listenerPosition)
        {
            float audibleDistance = Math.Max(320f, Main.screenWidth * 1.5f);
            float distance = Vector2.Distance(sourcePosition, listenerPosition);
            return MathHelper.Clamp(1f - distance / audibleDistance, 0f, 1f);
        }

        private static float GetSpatialPan(Vector2 sourcePosition, Vector2 listenerPosition)
        {
            float halfScreenWidth = Math.Max(1f, Main.screenWidth * 0.5f);
            return MathHelper.Clamp(
                (sourcePosition.X - listenerPosition.X) / halfScreenWidth,
                -1f,
                1f);
        }
    }
}
