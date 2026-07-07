using Microsoft.Xna.Framework;
using SariaMod.Items.zPearls;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod;
using SariaMod.Items.zTalking;
using SariaMod.Buffs;
namespace SariaMod.Items.Emerald
{
    public class LivingSilverFragment : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Living Silver fragment");
            Tooltip.SetDefault("Can be used to make Living Silver Shards");
        }
        public override void SetDefaults()
        {
            base.Item.width = 20;
            base.Item.height = 20;
            base.Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 156, 0);
            Item.consumable = false;
            Item.noUseGraphic = true;
            // These below are needed for a minion weapon
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.rare = ItemRarityID.Expert;
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.Silver.ToVector3() * 1f);
        }
        public override bool CanUseItem(Player player)
        {
            return false;
        }
    }
}
