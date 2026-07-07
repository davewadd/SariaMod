using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Strange;
using SariaMod.Buffs;
using SariaMod.Items.zTalking;
using SariaMod;
namespace SariaMod
{
	// This example show how to create new informational display (like Radar, Watches, etc.)
	// Take a look at the ExampleInfoDisplayPlayer at the end of the file to see how to use it
	class TMPointsDisplay : InfoDisplay
	{
		public override void SetStaticDefaults() {
			// This is the name that will show up when hovering over icon of this info display
			InfoName.SetDefault("TM Points");
		}
		// This dictates whether or not this info display should be active
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<TMInfoDisplayPlayer>().TMDis;
		}
		// Here we can change the value that will be displayed in the game
		public override string DisplayValue() 
        {
            // Counting how many minions we have
            Player player = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
			// This is the value that will show up when viewing this display in normal play, right next to the icon
			return modPlayer.TMPoints > 0 ? $"{modPlayer.TMPoints} TM Points" : "No Points";
		}
	}
	public class TMInfoDisplayPlayer : ModPlayer
	{
		// Flag checking when information display should be activated
		public bool TMDis;
		public override void ResetEffects() {
			TMDis = false;
		}
		public override void UpdateEquips() {
			// The information display is activated when dialogue UI is active
			if (SariaUISystem.IsDialogueActive)
				TMDis = true;
		}
	}
}
