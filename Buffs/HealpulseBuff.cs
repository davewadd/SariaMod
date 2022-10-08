using Microsoft.Xna.Framework; 




using System;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Buffs
{
	

	public class HealpulseBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Healpulse");
			Description.SetDefault("Saria thought you could use some healing\nYou will have to wait for her power to recharge");
			Main.debuff[base.Type] = true;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = true;
			Main.buffNoTimeDisplay[base.Type] = false;
			longerExpertDebuff = false;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			


		}
			
		

	}
}