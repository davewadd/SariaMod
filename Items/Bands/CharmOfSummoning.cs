using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Bands
{
    public class CharmOfSummoning : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Charm of Summoning");
            base.Tooltip.SetDefault("Gives the summon slots Saria Requires as she levels up!\n\nWithout Saria the stone gives 2 Summon slots");
        }
        public override void SetDefaults()
        {
            base.Item.width = 28;
            base.Item.height = 20;
            base.Item.value = Item.sellPrice(0, 0, 100);
            Item.rare = ItemRarityID.Expert;
            base.Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            FairyPlayer modPlayer = player.Fairy();
            if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
            {
                if (modPlayer.Sarialevel == 6)
                {
                    player.maxTurrets += 0;
                    player.maxMinions += 3;
                }
                else if (modPlayer.Sarialevel == 5)
                {
                    player.maxTurrets += 0;
                    player.maxMinions += 0;
                }
                else if (modPlayer.Sarialevel == 4)
                {
                    player.maxTurrets += 2;
                    player.maxMinions += 2;
                }
                else if (modPlayer.Sarialevel == 3)
                {
                    player.maxTurrets += 2;
                    player.maxMinions += 2;
                }
                else if (modPlayer.Sarialevel == 2)
                {
                    player.maxTurrets += 1;
                    player.maxMinions += 2;
                }
                else if (modPlayer.Sarialevel == 1)
                {
                    player.maxTurrets += 2;
                    player.maxMinions += 1;
                }
                else
                {
                    player.maxTurrets += 1;
                    player.maxMinions += 1;
                }
            }
            else
            {
                player.maxTurrets += 0;
                player.maxMinions += 3;
            }
        }
    }
}
