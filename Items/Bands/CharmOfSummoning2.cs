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
	public class CharmOfSummoning2 : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Charm of Summoning Upgrade1");
			base.Tooltip.SetDefault("Will increase the number of sentries and Minions\nGrows in power as Saria gets stronger\n\nIt seems slightly stronger.\n\n You can no longer get the Summoning buff!");
		}

		public override void SetDefaults()
		{
			base.item.width = 28;
			base.item.height = 20;
			base.item.value = Item.sellPrice(0, 0, 200);
			item.rare = ItemRarityID.Expert;
			base.item.accessory = true;
		}
		
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			FairyPlayer modPlayer = player.Fairy();
			if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
			{
				player.maxTurrets += 0;
				player.maxMinions += 2;
				
			}
			if (player.HasBuff(ModContent.BuffType<SapphireSariaBuff>()))
			{
				player.maxTurrets += 0;
				player.maxMinions += 3;
			}
			if (player.HasBuff(ModContent.BuffType<RubySariaBuff>()))
			{
				player.maxTurrets += 1;
				player.maxMinions += 3;
			}
			if (player.HasBuff(ModContent.BuffType<TopazSariaBuff>()))
			{
				player.maxTurrets += 1;
				player.maxMinions += 4;
			}
			if (player.HasBuff(ModContent.BuffType<EmeraldSariaBuff>()))
			{
				player.maxTurrets += 1;
				player.maxMinions += 4;
			}
			if (player.HasBuff(ModContent.BuffType<AmberSariaBuff>()))
			{
				player.maxTurrets += 1;
				player.maxMinions += 4;
			}
			if (player.HasBuff(ModContent.BuffType<AmethystSariaBuff>()))
			{
				player.maxTurrets += 2;
				player.maxMinions += 5;
			}
			if (player.HasBuff(ModContent.BuffType<DiamondSariaBuff>()))
			{
				player.maxTurrets += 3;
				player.maxMinions += 6;
			}
			
			else
            {
				player.maxTurrets += 0;
				player.maxMinions += 1;
            }
			player.ClearBuff(BuffID.Summoning);


		}
		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<CharmOfSummoning>(), 1);
				recipe.AddIngredient(ItemID.SummoningPotion, 1);
				recipe.AddIngredient(ModContent.ItemType<MediumXpPearl>(), 15);
				recipe.AddTile(ModContent.TileType<Tiles.SapphireBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}

		}
	}
}
