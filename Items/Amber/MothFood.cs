using Microsoft.Xna.Framework;
using SariaMod.Items.zPearls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Amber
{
    public class MothFood : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient BloodMoth food");
            Tooltip.SetDefault("Food Ancient Moths cannot resist!\nConsumable");
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 100, 0);
            Item.rare = ItemRarityID.Red;
            Item.consumable = true;
            Item.useTime = 36;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item45;
            Item.autoReuse = false;
            // These below are needed for a minion weapon
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.shootSpeed = 1;
            // No buffTime because otherwise the item tooltip would say something like "1 minute duration"
            Item.shoot = ModContent.ProjectileType<Mothdust>();
        }
        public override bool AltFunctionUse(Player player)
        {
            return false;
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.OrangeRed.ToVector3() * 2f);
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.consumable = false;
                return false;
            }
            if (player.altFunctionUse != 2)
            {
                Item.consumable = true;
                return true;
            }
            else
            {
                return true;
            }
        }
        public override void AddRecipes()
        {
            {
                Recipe recipe = CreateRecipe(1);
                recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 2);
                recipe.AddIngredient(ItemID.TissueSample, 1);
                recipe.Register();
            }
            {
                Recipe recipe = CreateRecipe(1);
                recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 2);
                recipe.AddIngredient(ItemID.ShadowScale, 1);
                recipe.Register();
            }
        }
    }
}
