using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Strange;
using SariaMod.Buffs;
using SariaMod.Items.zDinner;
namespace SariaMod
{
	// This example show how to create new informational display (like Radar, Watches, etc.)
	// Take a look at the ExampleInfoDisplayPlayer at the end of the file to see how to use it
	class Servings : InfoDisplay
	{
		public override void SetStaticDefaults() {
			// This is the name that will show up when hovering over icon of this info display
			InfoName.SetDefault("Serving's Dealt");
		}
		// This dictates whether or not this info display should be active
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<StoredServingsDisplayPlayer>().Servingsv;
		}
		// Here we can change the value that will be displayed in the game
		public override string DisplayValue() {
            // Counting how many minions we have
            Player player = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            int servings = 0;
			servings = modPlayer.Serving;
				// This is the value that will show up when viewing this display in normal play, right next to the icon
			return servings > 0 ? $"{servings} Enemies Fed" : "As If";
		}
	}
	public class StoredServingsDisplayPlayer : ModPlayer
	{
		// Flag checking when information display should be activated
		public bool Servingsv;
		public override void ResetEffects() {
			Servingsv = false;
		}
		public override void UpdateEquips() {
			// The information display is only activated when a Radar is present
			Player player = Main.LocalPlayer;
			FairyPlayer modPlayer = player.Fairy();
			if (player.HeldItem.type == ModContent.ItemType<KingsDinner>())
			{
				Servingsv = true;
			}
		}
	}
}
