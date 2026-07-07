using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Strange;
using SariaMod.Buffs;
namespace SariaMod
{
	// This example show how to create new informational display (like Radar, Watches, etc.)
	// Take a look at the ExampleInfoDisplayPlayer at the end of the file to see how to use it
	class XpDisplay : InfoDisplay
	{
		public override void SetStaticDefaults() {
			// This is the name that will show up when hovering over icon of this info display
			InfoName.SetDefault("Saria's Xp");
		}
		// This dictates whether or not this info display should be active
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<SariaXpDisplayPlayer>().SariaXpv;
		}
		// Here we can change the value that will be displayed in the game
		public override string DisplayValue() {
            // Counting how many minions we have
            Player player = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            int XpCount = 0;
			XpCount = modPlayer.SariaXp;
				// This is the value that will show up when viewing this display in normal play, right next to the icon
			return XpCount > 0 ? $"{XpCount} Xp" : "No Xp";
		}
	}
	public class SariaXpDisplayPlayer : ModPlayer
	{
		// Flag checking when information display should be activated
		public bool SariaXpv;
		public override void ResetEffects() {
			SariaXpv = false;
		}
		public override void UpdateEquips() {
			// The information display is only activated when a Radar is present
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0)
				SariaXpv = true;
		}
	}
}
