using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;
using SariaMod;
using SariaMod.Buffs;
using Terraria.ModLoader.IO;
using SariaMod.Gores;
using SariaMod.Items.zPearls;

namespace SariaMod
{
    public class SariaModSystem : ModSystem
    {
        public static bool CustomRainSoundIsPlaying = false;
        private static bool lastKnownFocusState = true;

        public override void Load()
        {
            // Hook into the game's activation/deactivation events
            // These fire when the window gains/loses focus, even when the game is paused
            Main.OnPostDraw += OnPostDraw_CheckFocus;
        }

        public override void Unload()
        {
            Main.OnPostDraw -= OnPostDraw_CheckFocus;
        }

        /// <summary>
        /// This runs after every draw call, which happens even when the game logic is paused.
        /// We use this to detect focus changes and manage rain sounds.
        /// </summary>
        private void OnPostDraw_CheckFocus(GameTime gameTime)
        {
            if (Main.dedServ || Main.gameMenu)
                return;

            // Check for focus changes
            bool currentFocus = Main.hasFocus;
            if (currentFocus != lastKnownFocusState)
            {
                lastKnownFocusState = currentFocus;
                
                // Notify the player's ModPlayer about focus change
                if (Main.LocalPlayer != null && Main.LocalPlayer.active)
                {
                    if (Main.LocalPlayer.TryGetModPlayer(out FairyPlayerMiscEffects modPlayer))
                    {
                        modPlayer.HandleFocusChange(currentFocus);
                    }
                }
            }
        }

        private bool _sandstormWasHappening = false;

        public override void PostUpdateWorld()
        {
            // Only the server (or single-player host) runs the repeat logic
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            bool happening = Terraria.GameContent.Events.Sandstorm.Happening;

            // Detect transition from happening → stopped
            if (_sandstormWasHappening && !happening && SariaMod.SandstormRepeatCount > 0)
            {
                SariaMod.SandstormRepeatCount--;

                // Restart the sandstorm
                Terraria.GameContent.Events.Sandstorm.StartSandstorm();
                NetMessage.SendData(MessageID.WorldData);

                // Broadcast updated repeat count to all clients
                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket p = SariaMod.Instance.GetPacket();
                    p.Write((byte)SariaMod.SoundMessageType.SyncSandstormRepeat);
                    p.Write(SariaMod.SandstormRepeatCount);
                    p.Send(-1, -1);
                }
            }

            _sandstormWasHappening = Terraria.GameContent.Events.Sandstorm.Happening;
        }

        public override void PostDrawTiles()
        {
            if (Main.gameMenu) return;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active) return;

            // Teleport destination sphere — drawn here so it stays visible even when
            // Saria is far off-screen and PostDraw never fires for her projectile.
            int sariaType = ModContent.ProjectileType<Items.Strange.Saria>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.type != sariaType)
                    continue;
                if (proj.ModProjectile is Items.Strange.Saria saria)
                    saria.DrawTeleportDestination(Main.spriteBatch);
                break;
            }

            // Find the RainOcarinaNote projectile owned by the local player
            int projectileTimeLeft = -1;
            int rainOcarinaType = ModContent.ProjectileType<RainOcarinaNote>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == rainOcarinaType && proj.owner == player.whoAmI)
                {
                    projectileTimeLeft = proj.timeLeft;
                    break;
                }
            }

            // Only draw vignette if the projectile exists
            if (projectileTimeLeft > 0)
            {
                FairyPlayerMiscEffects modPlayer = player.GetModPlayer<FairyPlayerMiscEffects>();
                if (modPlayer != null)
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    modPlayer.DrawRainOcarinaEffects(projectileTimeLeft);
                    Main.spriteBatch.End();
                }
            }

            // Find the SandstormOcarinaNote projectile owned by the local player
            int oasisProjectileTimeLeft = -1;
            int oasisOcarinaType = ModContent.ProjectileType<SandstormOcarinaNote>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == oasisOcarinaType && proj.owner == player.whoAmI)
                {
                    oasisProjectileTimeLeft = proj.timeLeft;
                    break;
                }
            }

            // Only draw effect if the projectile exists
            if (oasisProjectileTimeLeft > 0)
            {
                FairyPlayerMiscEffects modPlayer = player.GetModPlayer<FairyPlayerMiscEffects>();
                if (modPlayer != null)
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    modPlayer.DrawOasisOcarinaEffects(oasisProjectileTimeLeft);
                    Main.spriteBatch.End();
                }
            }
        }

        public override void OnWorldUnload()
        {
            if (Main.LocalPlayer.TryGetModPlayer(out FairyPlayerMiscEffects modPlayer))
            {
                modPlayer.StopAllLoopedSounds();
            }
            CustomRainSoundIsPlaying = false;
            lastKnownFocusState = true;
        }
    }
}