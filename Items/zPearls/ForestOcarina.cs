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
			Tooltip.SetDefault("Plays an old forgotten song that\nmay make Saria happier");
		}


		public override void SetDefaults()
		{
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 1;
			item.useTime = 400;
			item.useAnimation = 400;
			item.useStyle = ItemUseStyleID.HoldingOut;
			base.item.value = 0;
			item.shoot = ModContent.ProjectileType<SariasSong>();
			item.UseSound = base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/SariasSong");
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
			{
				
				return true;
			}
		}
		

public override void AddRecipes()
		{
			
		}
	}

}