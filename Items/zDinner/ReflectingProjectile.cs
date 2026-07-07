using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.zDinner;
namespace SariaMod.Items.zDinner
{
    public class ReflectingProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reflecting Shield");
        }
        public override void SetDefaults()
        {
            Projectile.width = 48; // Adjust for a larger hitbox
            Projectile.height = 68;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 2; // Refreshed every frame
            Projectile.DamageType = DamageClass.Generic;
            Projectile.damage = 0; // No contact damage
            Projectile.ignoreWater = true;
            Projectile.alpha = 300;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            // Check if the player is actively using the KingsDinner item
            bool playerIsUsingItem = player.HeldItem.type == ModContent.ItemType<KingsDinner>() && player.itemAnimation > 0;
            // Kill the projectile if the player is no longer using the item
            if (!player.active || player.dead || !playerIsUsingItem)
            {
                Projectile.Kill();
                return;
            }
            // Keep the projectile's lifespan refreshed
            Projectile.timeLeft = 2;
            // Define the distance the projectile should hover in front of the player
            float distance = 25f;
            // Calculate the desired position based on the player's direction
            Vector2 desiredPosition = player.Center;
            desiredPosition.X += player.direction * distance;
            // Update the projectile's position
            Projectile.Center = desiredPosition;
        }
    }
}