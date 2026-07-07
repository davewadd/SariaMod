using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Strange;
using SariaMod.Buffs;
using SariaMod;
namespace SariaMod
{
	// This example show how to create new informational display (like Radar, Watches, etc.)
	// Take a look at the ExampleInfoDisplayPlayer at the end of the file to see how to use it
	class SariaInfoDisplay : InfoDisplay
	{
		public override void SetStaticDefaults() {
			// This is the name that will show up when hovering over icon of this info display
			InfoName.SetDefault("Saria's Base Damage");
		}
		// This dictates whether or not this info display should be active
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<SariaInfoDisplayPlayer>().SariaDamage;
		}
		// Here we can change the value that will be displayed in the game
		public override string DisplayValue() 
        {
            // Counting how many minions we have
            Player player = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            int DamageCount = 0;
            if (modPlayer.Sarialevel == 6)
            {
                DamageCount = 900 + (modPlayer.SariaXp / 20);
            }
            else if (modPlayer.Sarialevel == 5)
            {
                DamageCount = 200 + (modPlayer.SariaXp / 342);
            }
            else if (modPlayer.Sarialevel == 4)
            {
                DamageCount = 75 + (modPlayer.SariaXp / 640);
            }
            else if (modPlayer.Sarialevel == 3)
            {
                DamageCount = 50 + (modPlayer.SariaXp / 1600);
            }
            else if (modPlayer.Sarialevel == 2)
            {
                DamageCount = 26 + (modPlayer.SariaXp / 833);
            }
            else if (modPlayer.Sarialevel == 1)
            {
                DamageCount = 15 + (modPlayer.SariaXp / 818);
            }
            else
            {
                DamageCount = 10 + (modPlayer.SariaXp / 600);
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                DamageCount += (DamageCount) / 4;
            }
            else if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                DamageCount /= 2;
            }
            if (player.Fairy().PlayerisPsychic)
            {
                DamageCount -= (DamageCount) / 4;
            }
            if (player.Fairy().PlayerisWater)
            {
                DamageCount /= 4;
            }
            if (player.Fairy().PlayerisElectric)
            {
                if ((player.HasBuff(ModContent.BuffType<Overcharged>())))
                {
                    DamageCount /= 10;
                }
                else
                {
                    DamageCount /= 20;
                }
            }
            // This is the value that will show up when viewing this display in normal play, right next to the icon
            return DamageCount > 0 ? $"{DamageCount} BaseDamage" : "No Damage";
		}
	}
	public class SariaInfoDisplayPlayer : ModPlayer
	{
		// Flag checking when information display should be activated
		public bool SariaDamage;
		public override void ResetEffects() {
			SariaDamage = false;
		}
		public override void UpdateEquips() {
			// The information display is only activated when a Radar is present
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0)
				SariaDamage = true;
		}
	}
}
