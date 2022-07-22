using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



using System;
using SariaMod.Buffs;

using Terraria;



using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class Rupee2 : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Living Green Shard");
			Tooltip.SetDefault("Can be sold for a decent price\nCan be used as a quick heal");
		}


		public override void SetDefaults()
		{
			
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 999;
			item.value = Item.buyPrice(2, 0, 0, 0);
			item.healLife = 10;
			item.rare = ItemRarityID.Lime;
			item.consumable = true;
			item.useTime = 36;
			item.useAnimation = 36;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.UseSound = SoundID.Item46;
			item.autoReuse = true;
			// These below are needed for a minion weapon
			item.noMelee = true;
			
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			
			Lighting.AddLight(item.Center, Color.LimeGreen.ToVector3() * 2f);
		}
	}
}
