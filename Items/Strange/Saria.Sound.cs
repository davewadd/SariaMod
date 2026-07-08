using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using SariaMod.Items.zTalking;
using SariaMod.Items.Strange;
using Terraria.Localization;
using System;
using Terraria.Map;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using SariaMod.Diagnostics;
using SariaMod.Netcode.SariaSoundSync;
using ReLogic.Utilities;
namespace SariaMod.Items.Strange
{
    public partial class Saria
    {
        private static readonly int[] SoundCooldownTicks = new int[]
        {
            0,   // 0 unused
            18,  // 1 Hover  (~0.30s loop spacing)
            24,  // 2 Fly    (~0.40s loop spacing)
            10,  // 3 Step1  (~0.17s footstep spacing)
            10,  // 4 Step2
            30,  // 5 IceBarrier1 (one-shot, generous floor)
            30,  // 6 IceBarrier2
        };

        private readonly int[] _lastSoundTick = new int[SoundCooldownTicks.Length];


        public void SetMoodFor(MoodState state, int durationTicks, int priority = 0)
        {
            if (priority < _moodPriority) return;
            bool moodChanging = (int)state != Mood;
            Mood = (int)state;
            _moodOverrideTarget = (int)state;
            _moodOverrideTimer = durationTicks;
            _moodPriority = priority;
            // Automatically show the matching bubble face for the same duration.
            // Owner-only: ShowBubbleFace writes to a local draw-side dictionary.
            if (Main.myPlayer == Projectile.owner)
            {
                var face = state switch
                {
                    MoodState.Happy  => SariaExtensions1.BubbleFaceType.Smile,
                    MoodState.Sad    => SariaExtensions1.BubbleFaceType.Sad,
                    MoodState.Angry  => SariaExtensions1.BubbleFaceType.Anger,
                    MoodState.Cursed => SariaExtensions1.BubbleFaceType.Cursed,
                    _                => SariaExtensions1.BubbleFaceType.None, // Normal clears face
                };
                Projectile.ShowBubbleFace(face, Math.Min(durationTicks, BubbleFaceMaxDuration));
                // Play a sound only when the mood actually changes state.
                if (moodChanging && state != MoodState.Normal)
                {
                    if (state == MoodState.Happy)
                        SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
                    else if (state == MoodState.Sad)
                        SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);
                    else if (state == MoodState.Angry)
                        SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);
                    else if (state == MoodState.Cursed)
                        SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);
                }
            }
            Projectile.netUpdate = true;
        }


        public void Sigh()
        {
            // Play a sigh sound (using Step2 as placeholder if specific sigh sound doesn't exist, or just a generic sound)
            // Assuming "SariaMod/Sounds/Sigh" might exist or using a fallback.
            // The prompt implies I should implement the trigger, I'll use a sound that fits or just a visual if sound is unknown.
            // Using "SariaMod/Sounds/Step2" as a placeholder or standard sound engine if specific not known.
            // Actually, I'll check if I can use a standard sound or if I should define it.
            // I'll use SoundID.Item1 (generic) or similar if I don't have a specific one, but let's try to be specific if possible.
            // I'll use a safe fallback.
            SoundEngine.PlaySound(SoundID.MenuClose, Projectile.Center);

            // Visual effect for sighing (e.g. small dust or emote)
            CombatText.NewText(Projectile.getRect(), Color.LightGray, "*Sigh*", true);
        }


        private void PlaySyncedSariaSound(SariaSoundId soundId)
        {
            // Per-sound rate limit. Animation triggers fire every tick the condition
            // is true — without this gate the same sound would queue dozens of times
            // per second, swamping the network with tiny packets and stacking the
            // local SoundEngine. See SoundCooldownTicks above.
            int sid = (int)soundId;
            if (sid > 0 && sid < SoundCooldownTicks.Length && SoundCooldownTicks[sid] > 0)
            {
                int now = (int)Main.GameUpdateCount;
                if (now - _lastSoundTick[sid] < SoundCooldownTicks[sid])
                    return;
                _lastSoundTick[sid] = now;
            }

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                SoundStyle style = soundId switch
                {
                    SariaSoundId.Hover => new SoundStyle("SariaMod/Sounds/Hover"),
                    SariaSoundId.Fly => new SoundStyle("SariaMod/Sounds/Fly"),
                    SariaSoundId.Step1 => new SoundStyle("SariaMod/Sounds/Step1"),
                    SariaSoundId.Step2 => new SoundStyle("SariaMod/Sounds/Step2"),
                    _ => default
                };

                if (style != default)
                    SoundEngine.PlaySound(style, Projectile.Center);

                return;
            }

            // In multiplayer, always let the local client hear the sound if this is their projectile.
            if (Main.myPlayer == Projectile.owner)
            {
                SoundStyle localStyle = soundId switch
                {
                    SariaSoundId.Hover => new SoundStyle("SariaMod/Sounds/Hover"),
                    SariaSoundId.Fly => new SoundStyle("SariaMod/Sounds/Fly"),
                    SariaSoundId.Step1 => new SoundStyle("SariaMod/Sounds/Step1"),
                    SariaSoundId.Step2 => new SoundStyle("SariaMod/Sounds/Step2"),
                    _ => default
                };

                if (localStyle != default)
                    SoundEngine.PlaySound(localStyle, Projectile.Center);
            }

            // Clients (including host-and-play) send to the server; the server then broadcasts.
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (Main.myPlayer != Projectile.owner)
                    return;

                ModPacket packet = Mod.GetPacket();
                SariaSoundSyncMessage.Write(packet, Projectile, soundId);
                packet.Send();
                return;
            }

            // Dedicated server: broadcast directly.
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = Mod.GetPacket();
                SariaSoundSyncMessage.Write(packet, Projectile, soundId);
                packet.Send();
            }
        }
    }
}
