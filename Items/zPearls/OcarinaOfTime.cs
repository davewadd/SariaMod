using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace SariaMod.Items.zPearls
{
    public class OcarinaOfTime : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ocarina of Time");
            Tooltip.SetDefault("Opens a clock to manipulate time");
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 22;
            Item.maxStack = 1;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
            Item.noUseGraphic = false;
        }

        public override bool? UseItem(Player player)
        {
            // Only the local player should spawn the UI
            if (player.whoAmI == Main.myPlayer)
            {
                // Check if there's already a clock UI active
                int clockUIType = ModContent.ProjectileType<OcarinaOfTimeUI>();
                bool hasActiveUI = false;
                
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && 
                        Main.projectile[i].type == clockUIType && 
                        Main.projectile[i].owner == player.whoAmI)
                    {
                        hasActiveUI = true;
                        break;
                    }
                }

                // Don't allow using if time is already transitioning
                if (OcarinaOfTimeUI.IsTimeTransitioning)
                {
                    Main.NewText("Time is already shifting...", 255, 200, 100);
                    return true;
                }

                if (!hasActiveUI)
                {
                    // Spawn the clock UI projectile
                    Projectile.NewProjectile(
                        player.GetSource_FromThis(),
                        player.Center,
                        Vector2.Zero,
                        clockUIType,
                        0,
                        0f,
                        player.whoAmI
                    );
                    
                    SoundEngine.PlaySound(SoundID.MenuOpen, player.Center);
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            // Add recipe if desired
        }
    }
}
