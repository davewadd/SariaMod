using Microsoft.Xna.Framework;

using SariaMod.Items.Ruby;
using System;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using SariaMod.Items.LilHarpy;
using Terraria.ModLoader;

namespace SariaMod.Buffs
{
	

	public class WillOWispBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Will-O-Wisp");
			Description.SetDefault("The Will-O-Wisps will protect you from projectile attacks. Will burn any other enemies. Can stack up to 8!");
			Main.debuff[base.Type] = false;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = false;
			Main.buffNoTimeDisplay[base.Type] = true;
			Main.vanityPet[Type] = true;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			
			if ( ((player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp>()] > 0f)))
            {
				player.buffTime[buffIndex] = 2;
			}
			
		}

	}
}