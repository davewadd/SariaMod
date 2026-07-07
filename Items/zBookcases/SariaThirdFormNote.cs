using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class SariaThirdFormNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Saria's Ruby Form");
            Tooltip.SetDefault("~Eruption will Bomb your enemies!\n~Smaller flames will be generated after the explosion\n " + "\n " + SariaModUtilities.ColorMessage("Saria can protect you from immense heat!", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("~Snow, Glowshroom, Jungle, Dungeon, and Holy", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage(" ~Ocean, Rain, and Sandstorm", new Color(135, 206, 180)));
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