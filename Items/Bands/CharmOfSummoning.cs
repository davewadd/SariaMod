using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using SariaMod.Items.zPearls;

namespace SariaMod.Items.Bands
{
	public class CharmOfSummoning : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.Tooltip.SetDefault("Will increase the number of sentries and Minions\nGrows in power as Saria gets stronger\n\nWithout Saria the stone seems inactive");
		}

		public override void SetDefaults()
		{
			base.item.width = 28;
			base.item.height = 20;
			base.item.value = Item.sellPrice(0, 0, 100);
			item.rare = ItemRarityID.Expert;
			base.item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			FairyPlayer modPlayer = player.Fairy();
			if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
			{
				player.maxTurrets += 0;
				player.maxMinions += 1;
				
			}
			if (player.HasBuff(ModContent.BuffType<SapphireSariaBuff>()))
			{
				player.maxTurrets += 0;
				player.maxMinions += 2;
			}
			if (player.HasBuff(ModContent.BuffType<RubySariaBuff>()))
			{
				player.maxTurrets += 1;
				player.maxMinions += 2;
			}
			if (player.HasBuff(ModContent.BuffType<TopazSariaBuff>()))
			{
				player.maxTurrets += 1;
				player.maxMinions += 3;
			}
			if (player.HasBuff(ModContent.BuffType<EmeraldSariaBuff>()))
			{
				player.maxTurrets += 1;
				player.maxMinions += 3;
			}
			if (player.HasBuff(ModContent.BuffType<AmberSariaBuff>()))
			{
				player.maxTurrets += 1;
				player.maxMinions += 3;
			}
			if (player.HasBuff(ModContent.BuffType<AmethystSariaBuff>()))
			{
				player.maxTurrets += 2;
				player.maxMinions += 4;
			}
			if (player.HasBuff(ModContent.BuffType<DiamondSariaBuff>()))
			{
				player.maxTurrets += 3;
				player.maxMinions += 5;
			}
			
			else
            {
				player.maxTurrets += 0;
				player.maxMinions += 0;
            }
		}
		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.JungleSpores, 3);
				recipe.AddIngredient(ItemID.Ruby, 1);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 15);
				recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}

		}
	}
}
