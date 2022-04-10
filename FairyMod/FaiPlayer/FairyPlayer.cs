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

namespace FairyMod.FaiPlayer
{
	public class FairyPlayer : ModPlayer
	{
		public bool Sapphire;


		
		public override void ResetEffects()
        {
			
				if (Sapphire)
			{
				base.player.statLifeMax2 += 20;
			}
		
		}
	}
}