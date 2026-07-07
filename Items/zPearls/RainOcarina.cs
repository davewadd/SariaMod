using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace SariaMod.Items.zPearls
{
    public class RainOcarina : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("RainOcarina");
            Tooltip.SetDefault("Causes Rain");
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
            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/SongOfStorms"));

            // IMPORTANT: Only the local player (the one using the item) should execute logic and send packets.
            // This prevents remote clients from "echoing" the command when they see the animation.
            if (player.whoAmI == Main.myPlayer)
            {
                // Spawn visual effect projectile on the local player first
                Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<RainOcarinaNote>(),
                    0,
                    0f,
                    player.whoAmI
                );

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    // In single player, directly toggle rain
                    if (Main.raining)
                    {
                        Main.StopRain();
                        Main.NewText("The storm passes for now.", 50, 100, 150);
                    }
                    else
                    {
                        Main.StartRain();
                        Main.NewText("Another Storm! You played the Ocarina again didn't you?", 50, 100, 150);
                    }
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Send packet to server to broadcast visual effect to other players
                    ModPacket effectPacket = SariaMod.Instance.GetPacket();
                    effectPacket.Write((byte)SariaMod.SoundMessageType.RainOcarinaEffect);
                    effectPacket.Send();

                    // Send packet to toggle rain
                    ModPacket packet = SariaMod.Instance.GetPacket();
                    packet.Write((byte)SariaMod.SoundMessageType.StartRain);
                    packet.Write(false); // This is a request, not a response
                    packet.Send();
                }
            }
            return true;
        }


        public override void AddRecipes()
        {
        }
    }
}