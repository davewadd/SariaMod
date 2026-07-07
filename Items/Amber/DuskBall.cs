using Microsoft.Xna.Framework;
using SariaMod.Items.zPearls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Amber
{
    public class DuskBall : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("DuskBall");
            Tooltip.SetDefault(SariaModUtilities.ColorMessage("Use to catch Saria's GreenMothGoliath", new Color(135, 206, 180)));
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
            Item.value = Item.sellPrice(0, 30, 0, 0);
            Item.rare = ItemRarityID.Expert;
            Item.UseSound = SoundID.Item46;
            Item.shootSpeed = 8;
            Item.noUseGraphic = true;
            // These below are needed for a minion weapon
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.consumable = true;
            // No buffTime because otherwise the item tooltip would say something like "1 minute duration"
            Item.shoot = ModContent.ProjectileType<DuskBallProjectile3>();
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.Green.ToVector3() * 2f);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Glass, 3);
            recipe.AddIngredient(ItemID.IronBar, 3);
            recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 3);
            recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
            recipe.Register();
        }
    }
}