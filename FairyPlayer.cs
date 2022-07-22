using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using SariaMod;
using SariaMod.Items.Bands;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SariaMod
{
	public class FairyPlayer : ModPlayer
	{
		public bool Sapphire;

		public bool BabyHarpy;

		public override void ResetEffects()
		{

			if (Sapphire)
			{
				base.player.statLifeMax2 += 20;
			}

		}
		private static void MiscEffects(Player player, FairyPlayer modPlayer, Mod mod)
		{
			if (!player.behindBackWall && Main.raining && player.ZoneSnow && !player.HasBuff(BuffID.Warmth) && !player.HasBuff(BuffID.Campfire) && player.ZoneOverworldHeight)
			{
				
				player.buffImmune[BuffID.Frostburn] = false;
				player.AddBuff(BuffID.Frostburn, 2);
			}
		}
	}
}