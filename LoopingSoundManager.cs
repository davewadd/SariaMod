using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod
{
    /// <summary>
    /// Shared lifecycle manager for looping FadingSoundPlayer instances.
    /// Start loops with FadingSoundPlayer.PlayManaged so they automatically
    /// update, pause with focus, and stop when leaving the world or mod.
    /// </summary>
    public static class LoopingSoundManager
    {
        private static readonly List<FadingSoundPlayer> ManagedSounds = new List<FadingSoundPlayer>();

        public static void Register(FadingSoundPlayer sound)
        {
            if (sound != null && !ManagedSounds.Contains(sound))
                ManagedSounds.Add(sound);
        }

        public static void SetFocus(bool hasFocus)
        {
            foreach (FadingSoundPlayer sound in ManagedSounds)
            {
                if (hasFocus)
                    sound.Resume();
                else
                    sound.Pause();
            }
        }

        public static void StopAllImmediate()
        {
            foreach (FadingSoundPlayer sound in ManagedSounds)
                sound.StopImmediate();

            ManagedSounds.Clear();
        }

        public static void Update()
        {
            // Gameplay updates stop while returning to the main menu, so a
            // normal fade can otherwise leave a SoundEffectInstance alive.
            if (Main.gameMenu)
            {
                StopAllImmediate();
                return;
            }

            for (int i = ManagedSounds.Count - 1; i >= 0; i--)
            {
                FadingSoundPlayer sound = ManagedSounds[i];
                if (Main.hasFocus)
                {
                    sound.Resume();
                    sound.Update();
                }
                else
                {
                    sound.Pause();
                }

                if (!sound.IsPlaying)
                    ManagedSounds.RemoveAt(i);
            }
        }
    }

    public class LoopingSoundSystem : ModSystem
    {
        public override void OnWorldUnload()
        {
            LoopingSoundManager.StopAllImmediate();
        }

        public override void Unload()
        {
            LoopingSoundManager.StopAllImmediate();
        }

        public override void PostUpdateEverything()
        {
            LoopingSoundManager.Update();
        }
    }
}
