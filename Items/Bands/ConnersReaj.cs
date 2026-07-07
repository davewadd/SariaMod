using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Items.zPearls;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Bands
{
    public class ConnersReaj : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Connor's Reajing Workout Supplements");
            base.Tooltip.SetDefault("Greatly increases defense and slight boost to melee attacks\n Range, Summon, and magic damage\n become much weaker.\n " + "\n " + SariaModUtilities.ColorMessage("Smells like regular flour...", new Color(0, 200, 250, 200)));
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
            player.statDefense += (player.statDefense / 2);
            if (player.statLife <= (player.statLifeMax2) / 4 && !player.HasBuff(ModContent.BuffType<ReajBuff>()))
            {
                player.statLife += player.statLifeMax2 / 3;
                player.AddBuff(ModContent.BuffType<ReajBuff>(), 8000);
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Healpulse"), player.Center);
            }
        }
        public override void AddRecipes()
        {
            {
                Recipe recipe = CreateRecipe();
                recipe.AddIngredient(ItemID.WrathPotion);
                recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 3);
                recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
                recipe.Register();
            }
        }
    }
}
