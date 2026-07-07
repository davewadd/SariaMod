using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Items.LilHarpy;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Bands
{
    public class SusEye : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip are automatically set from the .lang files, but below is how it is done normally.
            // DisplayName.SetDefault("Paper Airplane");
            base.Tooltip.SetDefault("Summons the eye of Cthulu!\nFor some reason this one feels tame");
            // Tooltip.SetDefault("Summons a Paper Airplane to follow aimlessly behind you");
        }
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish);
            Item.shoot = ModContent.ProjectileType<BigEye>();
            Item.buffType = ModContent.BuffType<BigEyeBuff>();
            Item.noMelee = true;
            Item.value = Item.sellPrice(0, 16, 0, 0);
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600, true);
            }
        }
    }
}