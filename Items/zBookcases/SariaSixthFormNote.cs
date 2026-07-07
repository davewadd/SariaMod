using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class SariaSixthFormNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Saria's Amber Form");
            Tooltip.SetDefault("~AttackOrder will cause Saria to swarm enemies in bloodmoths stuck in amber!\n~Only two black moths can be active at once\n~Rare Red and Purple moths my also spawn\nfeed them to make them grow!\nThe Green moth is the strongest of its kind!\nCraft a DuskBall to Catch the Goliath!\n " + "\n " + SariaModUtilities.ColorMessage("Super effective in:", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("No Biomes", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("No Biomes", new Color(135, 206, 180)));
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