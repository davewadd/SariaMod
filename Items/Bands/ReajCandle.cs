using SariaMod.Tiles;
using SariaMod.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Bands
{
    public class ReajCandle : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Reaj Candle");
            base.Tooltip.SetDefault("A Candle that puts a curse on you and your allies\n Enemies will hunt you down!");
        }
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 20;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = 1;
            Item.consumable = true;
            Item.value = 500;
            Item.createTile = ModContent.TileType<Tiles.ReajCandleTile>();
            Item.holdStyle = 1;
        }
        public override void HoldItem(Player player)
        {
            player.Fairy().CorruptMind = true;
            if (Main.rand.Next(player.itemAnimation > 0 ? 10 : 20) == 0)
            {
                Dust.NewDust(new Vector2(player.itemLocation.X + 10f * player.direction, player.itemLocation.Y - 12f * player.gravDir), 4, 4, 62);
            }
            Player player2 = Main.LocalPlayer;
            if (player2 is null)
                return;
            float between = Vector2.Distance(player.Center, player2.Center);
            if (!player.dead && player.active && player2 != player && between <= 1200)
            {
                player2.AddBuff(ModContent.BuffType<CorruptMindBuff>(), 20);
            }
            if (!player.dead && player.active)
            {
                player.AddBuff(ModContent.BuffType<CorruptMindBuff>(), 20);
            }
            player.itemLocation.Y += 8;
            Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
            Lighting.AddLight(position, 0.55f, 0.85f, 1f);
        }
        public override void PostUpdate()
        {
            Lighting.AddLight((int)((Item.position.X + Item.width / 2) / 16f), (int)((Item.position.Y + Item.height / 2) / 16f), 1f, 0.55f, 1f);
        }
        public override void AddRecipes()
        {
        }
    }
}
