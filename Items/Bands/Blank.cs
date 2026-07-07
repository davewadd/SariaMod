using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Items.zPearls;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Bands
{
    public class Blank : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blank");
            base.Tooltip.SetDefault("you shouldn't have this");
        }
        public override void SetDefaults()
        {
            base.Item.width = 28;
            base.Item.height = 20;
            base.Item.value = Item.sellPrice(0, 0, 100);
            Item.rare = ItemRarityID.Expert;
        }
    }
}
