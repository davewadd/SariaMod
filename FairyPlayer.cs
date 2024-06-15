using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using SariaMod.Buffs;
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
		public bool Statlowered;
		public bool Statrisen;
		public bool Sickness;
		public bool externalColdImmunity;
		public bool Burning2;
		public bool PassiveHealing;
		public bool Frostburn2;
		public bool Frostburn3;
		public bool EclipseBuff;
		public int Sarialevel;
		public int FreezingTemp;
		public int SariaXp;
		public int Timer;
		public int XPBarLevel;
		public int FairyBreak;

		public override void ResetEffects()
		{
			SariaCurseD = false;
			Statrisen = false;
			Statlowered = false;
			Sickness = false;
			externalColdImmunity = false;
			BloodmoonBuff = false;
			PassiveHealing = false;
			Burning2 = false;
			Frostburn2 = false;
			Frostburn3 = false;
			EclipseBuff = false;
		}
		public override void UpdateDead()
		{
			Statrisen = false;
			Statlowered = false;
			SariaCurseD = false;
			Sickness = false;
			Burning2 = false;
			PassiveHealing = false;
			externalColdImmunity = false;
			BloodmoonBuff = false;
			Frostburn2 = false;
			Frostburn3 = false;
			EclipseBuff = false;
		}
		public override void UpdateBadLifeRegen()
		{
			if (Timer < 30)
            {
				Timer++;
            }
			if (Frostburn3)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (Player.lifeRegen > 0)
				{
					Player.lifeRegen = 0;
				}
				Player.lifeRegenTime = 0;
				// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
				Player.lifeRegen -= 30;

			}
			if (Frostburn2)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (Player.lifeRegen > 0)
				{
					Player.lifeRegen = 0;
				}
				Player.lifeRegenTime = 0;
				// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
				Player.lifeRegen -= 30;

			}
			if (Sickness)
			{

				{
					Player.statDefense = 1;
					if (Player.statLife > ((Player.statLifeMax2) / 3))
					{
						if (Player.lifeRegen > 0)
						{
							Player.lifeRegen = 0;
						}
						Player.lifeRegenTime = 0;
						// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
						Player.lifeRegen -= 16;
					}
				}
			}
			if (PassiveHealing)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (Timer >= 30)
				{
					Player.Heal((1));
					Timer = 0;
				}
			}
			if (Burning2)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (Player.lifeRegen > 0)
				{
					Player.lifeRegen = 0;
				}
				Player.lifeRegenTime = 0;
				// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
				Player.lifeRegen -= 32;

			}
			if (SariaCurseD)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (Player.lifeRegen > 0)
				{
					Player.lifeRegen = 0;
				}
				Player.lifeRegenTime = 0;
				// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
				Player.lifeRegen -= 16;

			}
			if (Statlowered)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				Player.statDefense -= (Player.statDefense / 4) * 3;
				Player.statLifeMax2 -= 50;

			}
			if (Statrisen)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				Player.statDefense += Player.statDefense / 4;

			}
			if (BloodmoonBuff)
			{
				if (Player.statLife > ((Player.statLifeMax2) / 3))
				{
					if (Player.lifeRegen > 0)
					{
						Player.lifeRegen = 0;
					}
					Player.lifeRegenTime = 0;
					// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
					Player.lifeRegen -= 16;
				}
			}
			if (EclipseBuff)
			{
				if (Player.statLife > ((Player.statLifeMax2) / 3))
				{
					if (Player.lifeRegen > 0)
					{
						Player.lifeRegen = 0;
					}
					Player.lifeRegenTime = 0;
					// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
					Player.lifeRegen -= 16;
				}
			}
		}
		internal void StandardSync()
		{
			SyncSarialevel(false);
			SyncXpBarLevel(false);
		}
		private void SyncSarialevel(bool server)
		{
			
			ModPacket packet = Mod.GetPacket(256);
			packet.Write((byte)FairyModMessageType.LevelSync);
			packet.Write(Player.whoAmI);
			packet.Write(Sarialevel);
			Player.SendPacket(packet, server);
		}
	
		private void SyncXpBarLevel(bool server)
		{
			Player player = Main.LocalPlayer;
			ModPacket packet = Mod.GetPacket(256);
			packet.Write((byte)FairyModMessageType.XpBarSync);
			packet.Write(Player.whoAmI);
			packet.Write(XPBarLevel);
			Player.SendPacket(packet, server);
		}
		public override void SaveData(TagCompound tag)
		{
			tag["Sarialevel"] = Sarialevel;
			tag["SariaXp"] = SariaXp;
			tag["FreezingTemp"] = FreezingTemp;
			tag["XPBarLevel"] = XPBarLevel;
			tag["FairyBreak"] = FairyBreak;
		}
		public override void LoadData(TagCompound tag)
		{
			Sarialevel = tag.GetInt("Sarialevel");
			FairyBreak = tag.GetInt("FairyBreak");
			SariaXp = tag.GetInt("SariaXp");
			FreezingTemp = tag.GetInt("FreezingTemp");
			XPBarLevel = tag.GetInt("XPBarLevel");
		}

		internal void HandleSariaLevel(BinaryReader reader)
		{
			Sarialevel = reader.ReadInt32();
			if (Main.netMode == NetmodeID.Server)
			{
				SyncSarialevel(true);
			}
		}
		internal void HandleXpBarLevel(BinaryReader reader)
		{
			XPBarLevel = reader.ReadInt32();
			if (Main.netMode == NetmodeID.Server)
			{
				SyncXpBarLevel(true);
			}
		}

		public override void PostUpdateMiscEffects()
		{
			FairyPlayerMiscEffects.FairyPostUpdateMiscEffects(base.Player, base.Mod);
		}

	}

}