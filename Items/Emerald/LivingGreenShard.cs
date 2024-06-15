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
			
			base.Item.width = 26;
			base.Item.height = 22;
			base.Item.maxStack = 999;
			Item.value = Item.buyPrice(0, 0, 100, 0);
			
			Item.rare = ItemRarityID.Lime;
			
			
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			
			Lighting.AddLight(Item.Center, Color.LimeGreen.ToVector3() * 2f);
		}
	}
}
