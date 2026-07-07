using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace SariaMod.Items.zPearls
{
    public class OasisOcarina : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("OasisOcarina");
            Tooltip.SetDefault("Causes Sandstorms");
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 1;
            Item.useTime = 240;
            Item.useAnimation = 240;
            Item.useStyle = ItemUseStyleID.Shoot;
            base.Item.value = 0;
        }
        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/SongCorrect"));

            // IMPORTANT: Only the local player (the one using the item) should execute logic and send packets.
            // This prevents remote clients from "echoing" the command when they see the animation.
            if (player.whoAmI == Main.myPlayer)
            {
                // Spawn visual effect projectile on the local player first
                Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<SandstormOcarinaNote>(),
                    0,
                    0f,
                    player.whoAmI
                );

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    // In single player, directly toggle sandstorm
                    if (Terraria.GameContent.Events.Sandstorm.Happening)
                    {
                        Terraria.GameContent.Events.Sandstorm.StopSandstorm();
                        SariaMod.SandstormRepeatCount = 0;
                        Main.NewText("The sands settle...", 200, 170, 100);
                    }
                    else
                    {
                        // If repeats are active, cancel them (playing again is the off-switch)
                        if (SariaMod.SandstormRepeatCount > 0)
                        {
                            SariaMod.SandstormRepeatCount = 0;
                        }
                        else
                        {
                            SariaMod.SandstormRepeatCount = 4;
                        }
                        Terraria.GameContent.Events.Sandstorm.StartSandstorm();
                        Main.NewText("The deserts air begins to stir...", 200, 170, 100);
                    }
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Send packet to server to broadcast visual effect to other players
                    ModPacket effectPacket = SariaMod.Instance.GetPacket();
                    effectPacket.Write((byte)SariaMod.SoundMessageType.SandstormOcarinaEffect);
                    effectPacket.Send();

                    // Send packet to toggle sandstorm
                    ModPacket packet = SariaMod.Instance.GetPacket();
                    packet.Write((byte)SariaMod.SoundMessageType.StartSandstorm);
                    packet.Write(false); // This is a request, not a response
                    packet.Send();

                    // Send repeat counter update: 0 if already active (toggle off), else 2
                    int newRepeat = SariaMod.SandstormRepeatCount > 0 ? 0 : 4;
                    SariaMod.SandstormRepeatCount = newRepeat;
                    ModPacket repeatPacket = SariaMod.Instance.GetPacket();
                    repeatPacket.Write((byte)SariaMod.SoundMessageType.SyncSandstormRepeat);
                    repeatPacket.Write(newRepeat);
                    repeatPacket.Send();
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
        }
    }
}
