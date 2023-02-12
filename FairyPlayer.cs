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
using System.IO;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SariaMod
{
	public class FairyPlayer : ModPlayer
	{
		public bool BloodmoonBuff;
		public bool SariaCurseD;
		public bool externalColdImmunity;
		public bool Burning2;
		public bool Frostburn2;
		public bool Frostburn3;
		public bool EclipseBuff;
		public int Sarialevel;
		public int FreezingTemp;
		public int SariaXp;
		public int XPBarLevel;

		public override void ResetEffects()
		{
			SariaCurseD = false;
			BloodmoonBuff = false;
			Burning2 = false;
			Frostburn2 = false;
			Frostburn3 = false;
			EclipseBuff = false;
		}
		public override void UpdateDead()
		{
			SariaCurseD = false;
			Burning2 = false;
			BloodmoonBuff = false;
			Frostburn2 = false;
			Frostburn3 = false;
			EclipseBuff = false;
		}
		public override void UpdateBadLifeRegen()
		{
			if (Frostburn3)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (player.lifeRegen > 0)
				{
					player.lifeRegen = 0;
				}
				player.lifeRegenTime = 0;
				// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
				player.lifeRegen -= 30;

			}
			if (Frostburn2)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (player.lifeRegen > 0)
				{
					player.lifeRegen = 0;
				}
				player.lifeRegenTime = 0;
				// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
				player.lifeRegen -= 30;

			}
			if (Burning2)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (player.lifeRegen > 0)
				{
					player.lifeRegen = 0;
				}
				player.lifeRegenTime = 0;
				// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
				player.lifeRegen -= 32;

			}
			if (SariaCurseD)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (player.lifeRegen > 0)
				{
					player.lifeRegen = 0;
				}
				player.lifeRegenTime = 0;
				// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
				player.lifeRegen -= 16;

			}
			if (BloodmoonBuff)
            {
				if (player.statLife > ((player.statLifeMax2) / 3))
				{
					if (player.lifeRegen > 0)
					{
						player.lifeRegen = 0;
					}
					player.lifeRegenTime = 0;
					// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
					player.lifeRegen -= 16;
				}
			}
			if (EclipseBuff)
			{
				if (player.statLife > ((player.statLifeMax2) / 3))
				{
					if (player.lifeRegen > 0)
					{
						player.lifeRegen = 0;
					}
					player.lifeRegenTime = 0;
					// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
					player.lifeRegen -= 16;
				}
			}
		}
		public override TagCompound Save()
		{
			return new TagCompound
			{
			{ "Sarialevel", Sarialevel },
				{ "SariaXp", SariaXp },
				{ "FeezingTemp", FreezingTemp },
				{ "XPBarLevel", XPBarLevel },
			};
		}
		public override void Load(TagCompound tag)
		{
			Sarialevel = tag.GetInt("Sarialevel");
			SariaXp = tag.GetInt("SariaXp");
			FreezingTemp = tag.GetInt("FreezingTemp");
			XPBarLevel = tag.GetInt("XPBarLevel");
		}
		public override void LoadLegacy(BinaryReader reader)
		{
			int loadVersion = reader.ReadInt32();
			Sarialevel = reader.ReadInt32();
			FreezingTemp = reader.ReadInt32();
			SariaXp = reader.ReadInt32();
			XPBarLevel = reader.ReadInt32();
		}
		
		public override void PostUpdateMiscEffects()
		{
			FairyPlayerMiscEffects.FairyPostUpdateMiscEffects(base.player, base.mod);
		}

	}
}