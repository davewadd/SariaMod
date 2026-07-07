using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zPearls
{
    public class BloodOcarina : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("BloodOcarina");
            Tooltip.SetDefault("Causes a Bloodmoon when used at night");
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 1;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            base.Item.value = 0;
            Item.shoot = ModContent.ProjectileType<Bloodmoon>();
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.Red.ToVector3() * 2f);
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            {
                return true;
            }
        }
        public override void AddRecipes()
        {
        }
    }
}