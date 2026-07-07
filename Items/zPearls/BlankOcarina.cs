using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zPearls
{
    public class BlankOcarina : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blank Ocarina");
            Tooltip.SetDefault("Try using it durring a bloodmoon");
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 1;
            Item.useTime = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            base.Item.consumable = true;
            Item.shoot = ModContent.ProjectileType<EmptyNote>();
            Item.UseSound = new SoundStyle($"{nameof(SariaMod)}/Sounds/SongCorrect");
        }
        public override bool CanUseItem(Player player)
        {
            if (!Main.dayTime && Main.bloodMoon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override void AddRecipes()
        {
            {
                Recipe recipe = CreateRecipe(1);
                recipe.AddIngredient(ItemID.Wood, 12);
                recipe.AddIngredient(ItemID.ManaCrystal, 1);
                recipe.Register();
            }
        }
    }
}