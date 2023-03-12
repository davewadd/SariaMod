using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



using System;
using SariaMod.Buffs;

using Terraria;



using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class LivingGreenShard : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Living Green Shard");
			Tooltip.SetDefault("Can be sold for a decent price\nCan be used to craft an Emerald Gem Ball \n glass, 3\nIron Bar, 3\n Living Green Shard, 12\n LargeXpPearl, 1 \n at a Strange Bookcase!");
		}


		public override void SetDefaults()
		{
			
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 999;
			item.value = Item.buyPrice(2, 0, 0, 0);
			
			item.rare = ItemRarityID.Lime;
			
			
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			
			Lighting.AddLight(item.Center, Color.LimeGreen.ToVector3() * 2f);
		}
	}
}
