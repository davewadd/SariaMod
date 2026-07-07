using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class SariaFirstFormNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Saria's First Form");
            Tooltip.SetDefault("~Psyshock will pelt your enemies!\n~Enemies hit will slowely rise!\n " + "\n " + SariaModUtilities.ColorMessage("Psychic powers keep you from taking fall damage!", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("~Space, Jungle, Glowshroom", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("~all evil biomes", new Color(135, 206, 180)));
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 999;
            Item.rare = ItemRarityID.Orange;
            base.Item.value = 0;
        }
        public override void AddRecipes()
        {
            {
                Recipe recipe = CreateRecipe(1);
                recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
                recipe.Register();
            }
        }
    }
}