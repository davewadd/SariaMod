using SariaMod.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using SariaMod.Items.Bands;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using SariaMod;
namespace SariaMod.Tiles
{
	public class ReajCandleTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			SariaModUtilities.SetUpCandle(this);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Reaj Candle");
			AddMapEntry(new Color(238, 145, 105), name);
		}
		 public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<Items.Bands.ReajCandle>();
        }
		public override void NearbyEffects(int i, int j, bool closer)
		{
			// NearbyEffects fires from ANY SceneMetrics scan that covers this tile —
			// including Saria's zone scans and the camera-on-Saria perception scan,
			// which are centred on HER position, not the player's. Only grant the buff
			// when the LOCAL player is genuinely inside the vanilla buff-scan box
			// around this candle, so a candle near a far-away Saria never buffs the
			// player (Saria reads the candle herself via SariaHasReajCandle).
			Player player = Main.LocalPlayer;
			if (player == null || player.dead || !player.active)
				return;
			Point playerTile = player.Center.ToTileCoordinates();
			if (System.Math.Abs(playerTile.X - i) > Main.buffScanAreaWidth / 2
				|| System.Math.Abs(playerTile.Y - j) > Main.buffScanAreaHeight / 2)
				return;
			player.AddBuff(ModContent.BuffType<CorruptMindBuff>(), 20);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			if (Main.tile[i, j].TileFrameX < 18)
			{
				r = 1f;
				g = 0.55f;
				b = 1f;
			}
			else
			{
				r = 0f;
				g = 0f;
				b = 0f;
			}
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<Items.Bands.ReajCandle>());
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			if (Main.tile[i, j].TileFrameX < 18)
				SariaModUtilities.DrawFlameSparks(62, 5, i, j);
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			SariaModUtilities.DrawFlameEffect(ModContent.Request<Texture2D>("SariaMod/Tiles/ReajCandleTileFlame").Value, i, j);
		}
		public override bool RightClick(int i, int j)
		{
			SariaModUtilities.LightHitWire(Type, i, j, 1, 1);
			return true;
		}
	}
}
