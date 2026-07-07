using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zPearls
{
    public class CraftingRecipes : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(" New Recipes ");
            Tooltip.SetDefault("");
        }
        public override void AddRecipes()
        {
            {
                Recipe recipe = Recipe.Create(ItemID.LesserManaPotion, 10);
                recipe.AddIngredient(ItemID.BottledWater, 1);
                recipe.AddIngredient(ItemID.ManaCrystal, 1);
                recipe.AddTile(TileID.CookingPots);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.ManaPotion, 1);
                recipe.AddIngredient(ItemID.LesserManaPotion, 3);
                recipe.AddTile(TileID.CookingPots);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.SuperManaPotion, 1);
                recipe.AddIngredient(ItemID.ManaPotion, 3);
                recipe.AddTile(TileID.CookingPots);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.SlimeStaff, 1);
                recipe.AddIngredient(ItemID.Gel, 80);
                recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 3);
                recipe.AddTile(TileID.Bookcases);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.WrathPotion, 1);
                recipe.AddIngredient(ItemID.BottledWater, 1);
                recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
                recipe.AddTile(TileID.CookingPots);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.WormholePotion, 10);
                recipe.AddIngredient(ItemID.BottledWater, 10);
                recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
                recipe.AddTile(TileID.CookingPots);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.FrozenTurtleShell, 1);
                recipe.AddIngredient(ItemID.TurtleShell, 1);
                recipe.AddIngredient(ItemID.IceBlock, 50);
                recipe.AddTile(TileID.IceMachine);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.FeatherfallPotion, 3);
                recipe.AddIngredient(ItemID.BottledWater, 3);
                recipe.AddIngredient(ItemID.Feather, 1);
                recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
                recipe.AddTile(TileID.CookingPots);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.HunterPotion, 3);
                recipe.AddIngredient(ItemID.BottledWater, 3);
                recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
                recipe.AddTile(TileID.CookingPots);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.EndurancePotion, 10);
                recipe.AddIngredient(ItemID.BottledWater, 10);
                recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
                recipe.AddTile(TileID.CookingPots);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.HeartStatue, 1);
                recipe.AddIngredient(ItemID.StoneBlock, 20);
                recipe.AddIngredient(ItemID.LifeCrystal, 1);
                recipe.AddTile(TileID.WorkBenches);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.BattlePotion, 3);
                recipe.AddIngredient(ItemID.BottledWater, 3);
                recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
                recipe.AddTile(TileID.CookingPots);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.WaterCandle, 1);
                recipe.AddIngredient(ItemID.Torch, 1);
                recipe.AddIngredient(ItemID.Sapphire, 1);
                recipe.Register();
            }
            {
                Recipe recipe4 = Recipe.Create(ItemID.HealingPotion, 1);
                recipe4.AddIngredient(ItemID.LesserHealingPotion, 3);
                recipe4.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
                recipe4.AddTile(TileID.CookingPots);
                recipe4.Register();
            }
            {
                Recipe recipe5 = Recipe.Create(ItemID.GreaterHealingPotion, 1);
                recipe5.AddIngredient(ItemID.HealingPotion, 4);
                recipe5.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
                recipe5.AddTile(TileID.CookingPots);
                recipe5.Register();
            }
            {
                Recipe recipe6 = Recipe.Create(ItemID.SuperHealingPotion, 1);
                recipe6.AddIngredient(ItemID.GreaterHealingPotion, 2);
                recipe6.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
                recipe6.AddTile(TileID.CookingPots);
                recipe6.Register();
            }
            {
                Recipe recipe7 = Recipe.Create(ItemID.WaterBolt, 1);
                recipe7.AddIngredient(ItemID.Sapphire, 3);
                recipe7.AddIngredient(ItemID.Book, 1);
                recipe7.AddTile(TileID.Bookcases);
                recipe7.Register();
            }
            {
                Recipe recipe8 = Recipe.Create(ItemID.BorealWood, 1);
                recipe8.AddIngredient(ItemID.Wood, 1);
                recipe8.AddTile(TileID.WorkBenches);
                recipe8.Register();
            }
            {
                Recipe recipe8 = Recipe.Create(ItemID.Wood, 1);
                recipe8.AddIngredient(ItemID.BorealWood, 1);
                recipe8.AddTile(TileID.WorkBenches);
                recipe8.Register();
            }
            {
                Recipe recipe8 = Recipe.Create(ItemID.Ebonwood, 1);
                recipe8.AddIngredient(ItemID.BorealWood, 1);
                recipe8.AddTile(TileID.WorkBenches);
                recipe8.Register();
            }
            {
                Recipe recipe8 = Recipe.Create(ItemID.RichMahogany, 1);
                recipe8.AddIngredient(ItemID.BorealWood, 1);
                recipe8.AddTile(TileID.WorkBenches);
                recipe8.Register();
            }
            {
                Recipe recipe8 = Recipe.Create(ItemID.BorealWood, 1);
                recipe8.AddIngredient(ItemID.Mushroom, 1);
                recipe8.AddTile(TileID.WorkBenches);
                recipe8.Register();
            }
            {
                Recipe recipe8 = Recipe.Create(ItemID.BorealWood, 1);
                recipe8.AddIngredient(ItemID.GlowingMushroom, 1);
                recipe8.AddTile(TileID.WorkBenches);
                recipe8.Register();
            }
            {
                Recipe recipe8 = Recipe.Create(ItemID.SandBlock, 1);
                recipe8.AddIngredient(ItemID.DirtBlock, 1);
                recipe8.AddTile(TileID.Sand);
                recipe8.Register();
            }
            {
                Recipe recipe8 = Recipe.Create(ItemID.SnowBlock, 1);
                recipe8.AddIngredient(ItemID.DirtBlock, 1);
                recipe8.AddTile(TileID.SnowBlock);
                recipe8.Register();
            }
            {
                Recipe recipe8 = Recipe.Create(ItemID.IceBlock, 1);
                recipe8.AddIngredient(ItemID.SnowBlock, 1);
                recipe8.AddTile(TileID.SnowBlock);
                recipe8.Register();
            }
            {
                Recipe recipe8 = Recipe.Create(ItemID.Cloud, 1);
                recipe8.AddIngredient(ItemID.DirtBlock, 1);
                recipe8.AddTile(TileID.Cloud);
                recipe8.Register();
            }
        }
    }
}