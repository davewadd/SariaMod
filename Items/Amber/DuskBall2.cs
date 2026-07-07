using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Amber
{
    public class DuskBall2 : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("DuskBall (Goliath)");
            Tooltip.SetDefault(SariaModUtilities.ColorMessage("Calls on GreenMothGoliath", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("Only summons When Saria is active", new Color(50, 200, 250)));
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.knockBack = 13f;
            Item.mana = 1;
            Item.width = 32;
            Item.height = 32;
            base.Item.useTime = (base.Item.useAnimation = 10);
            Item.useStyle = 1;
            Item.value = Item.sellPrice(0, 15, 0, 0);
            Item.rare = ItemRarityID.Expert;
            Item.UseSound = SoundID.Item46;
            Item.shootSpeed = 8;
            Item.noUseGraphic = true;
            // These below are needed for a minion weapon
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            // No buffTime because otherwise the item tooltip would say something like "1 minute duration"
            Item.shoot = ModContent.ProjectileType<DuskBallReturn>();
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.Green.ToVector3() * 2f);
        }
        public override bool CanUseItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<DuskBallProjectile>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<DuskBallProjectile2>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<ReturnBallDusk>()] > 0f)
            {
                return false;
            }
            return true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
            if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath2>()] <= 0f))
            {
                {
                    Projectile.NewProjectile(Item.GetSource_FromThis(), position.X + 0, position.Y + 0, 0, 0, ModContent.ProjectileType<DuskBallProjectile>(), (int)(damage), 0f, player.whoAmI);
                }
            }
            else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath2>()] > 0f))
            {
                {
                    Projectile.NewProjectile(Item.GetSource_FromThis(), position.X + 0, position.Y + 0, 0, 0, ModContent.ProjectileType<DuskBallReturn>(), (int)(damage), 0f, player.whoAmI);
                }
            }
            return false;
        }
    }
}