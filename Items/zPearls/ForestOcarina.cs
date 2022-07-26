using Microsoft.Xna.Framework;



using System;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zPearls
{


	public class ForestOcarina : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("ForestOcarina");
			Tooltip.SetDefault("Left click to choose song\n Right click to play song\nMay make Saria happy");
		}


		public override void SetDefaults()
		{
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 1;
			item.useTime = 36;
			item.useStyle = ItemUseStyleID.HoldingOut;
			base.item.value = 0;
			item.shoot = ModContent.ProjectileType<ForestNote>();
			item.UseSound = base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/SongCorrect");
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(item.Center, Color.Green.ToVector3() * 2f);
		}
		public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		public override bool CanUseItem(Player player)
		{

			

		


		 if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<ForestNote>()] == 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<NotePlay>()] == 0f))
			{
				item.UseSound = base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/SongCorrect");
				item.shoot = ModContent.ProjectileType<ForestNote>();
				return true;
			}
			
			if (player.altFunctionUse == 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<ForestNote>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<TimeNote>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<RainNote>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<OasisNote>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<BloodNote>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<EclipseNote>()] > 0f))
            {
				item.UseSound = SoundID.Item1;
				item.shoot = ModContent.ProjectileType<NotePlay>();
				return true;
			}
			else
			{
				
				return false;
			}
		}
		

public override void AddRecipes()
		{
			
		}
	}

}