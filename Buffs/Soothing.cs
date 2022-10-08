using Microsoft.Xna.Framework;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using System;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Buffs
{
	

	public class Soothing : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Soothing");
			Description.SetDefault("This special frozen yogurt heals Saria's\nold wounds for now.");
			Main.debuff[base.Type] = true;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = false;
			Main.buffNoTimeDisplay[base.Type] = false;
			longerExpertDebuff = false;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
				player.buffTime[buffIndex] -=2;
			}
			

		}

	}
}