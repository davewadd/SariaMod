using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class SariaSecondFormNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Saria's Sapphire Form");
            Tooltip.SetDefault("~ColdSoul will slowely attack your enemies!\n~Enemies hit will have some Hp stolen\n " + "\n " + SariaModUtilities.ColorMessage("Now allows you to walk and breath in water", new Color(0, 200, 250, 200)) + "\n " + SariaModUtilities.ColorMessage("Super effective in:", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("~Hell, Rain, Beach, Meteor, and by a WaterCandle", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage(" ~Desert, Jungle, and Glowshroom", new Color(135, 206, 180)));
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