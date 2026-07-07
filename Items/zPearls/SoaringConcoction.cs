using SariaMod.Buffs;
using SariaMod.Items.Emerald;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zPearls
{
    public class SoaringConcoction : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soaring Concoction");
            Tooltip.SetDefault("Frozen Yogurt mixed with Rare magic!\nheavily increases flight time\n-Don't fly too close to the sun");
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.DrinkLong;
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 999;
            base.Item.value = 0;
            base.Item.consumable = true;
            Item.rare = ItemRarityID.Expert;
            Item.holdStyle = ItemHoldStyleID.HoldFront;
            Item.UseSound = SoundID.Item3;
            Item.noMelee = true;
            Item.value = Item.sellPrice(50, 0, 0, 0);
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<AerialAceBuff>();
            Item.buffTime = 10000;
        }
        public override bool CanUseItem(Player player)
        {
            return true;
        }
        public override void AddRecipes()
        {
            {
                Recipe recipe = CreateRecipe(5);
                recipe.AddIngredient(ModContent.ItemType<RareXpPearl>(), 1);
                recipe.AddIngredient(ItemID.SuperManaPotion, 3);
                recipe.AddIngredient(ItemID.SnowBlock, 5);
                recipe.Register();
            }
            {
                Recipe recipe = CreateRecipe(5);
                recipe.AddIngredient(ModContent.ItemType<LivingSilverShard>(), 1);
                recipe.AddIngredient(ItemID.SuperManaPotion, 3);
                recipe.AddIngredient(ItemID.SnowBlock, 5);
                recipe.Register();
            }
        }
    }
}