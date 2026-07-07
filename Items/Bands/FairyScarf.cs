using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Bands
{
    [AutoloadEquip(EquipType.Neck)]
    public class FairyScarf : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fairy Scarf");
            Tooltip.SetDefault(SariaText.Foresight( "A roughly woven scarf made with one hand.") + "\n" + SariaText.Rupee("Negates the stat lowered debuff when stepping into biomes she is weak to"));
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 26;
            Item.value = Item.sellPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FairyPlayer>().SoftStepShimmerImmune = true;
        }
    }
}
