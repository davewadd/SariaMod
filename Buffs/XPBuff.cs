using Microsoft.Xna.Framework; 




using System;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Buffs
{
	

	public class XPBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("XP Checker");
			Description.SetDefault("While this buff is active you can see Saria's XP level");
			Main.debuff[base.Type] = false;
			Main.pvpBuff[base.Type] = false;
			Main.buffNoSave[base.Type] = true;
			Main.buffNoTimeDisplay[base.Type] = true;
			longerExpertDebuff = false;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;

			}

		}
			
		

	}
}