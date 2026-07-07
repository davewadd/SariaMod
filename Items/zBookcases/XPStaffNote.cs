using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class XPStaffNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Staff Note");
            Tooltip.SetDefault("Craft an XP staff!\nIngredients include:\nWood, 5\nLifeCrystal\n at a strangebookcase");
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 999;
            Item.rare = ItemRarityID.White;
            base.Item.value = 0;
        }
        public override void AddRecipes()
        {
        }
    }
}