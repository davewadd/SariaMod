using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using FairyMod.FaiPlayer;
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
			base.Tooltip.SetDefault("Increases max number of minions and sentries!\nGrows in power as Saria gets stronger");
		}

		public override void SetDefaults()
		{
			base.item.width = 28;
			base.item.height = 20;
			base.item.value = Item.sellPrice(0, 0, 100);
			base.item.rare = 2;
			base.item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			FairyPlayer modPlayer = player.Fairy();
			if (player.ownedProjectileCounts[ModContent.ProjectileType<SariaMinion>()] > 0f)
			{
				player.maxTurrets += 0;
				player.maxMinions += 1;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<SapphireSariaMinion>()] > 0f)
			{
				player.maxTurrets += 0;
				player.maxMinions += 2;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<RubySariaMinion>()] > 0f)
			{
				player.maxTurrets += 1;
				player.maxMinions += 2;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<TopazSariaMinion>()] > 0f)
			{
				player.maxTurrets += 1;
				player.maxMinions += 3;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<EmeraldSariaMinion>()] > 0f)
			{
				player.maxTurrets += 1;
				player.maxMinions += 3;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<AmberSariaMinion>()] > 0f)
			{
				player.maxTurrets += 1;
				player.maxMinions += 3;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<AmethystSariaMinion>()] > 0f)
			{
				player.maxTurrets += 1;
				player.maxMinions += 3;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<DiamondSariaMinion>()] > 0f)
			{
				player.maxTurrets += 2;
				player.maxMinions += 4;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<PlatinumSariaMinion>()] > 0f)
			{
				player.maxTurrets += 3;
				player.maxMinions += 4;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<PlatinumBlueSariaMinion>()] > 0f)
			{
				player.maxTurrets += 11;
				player.maxMinions += 10;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<PlatinumPurpleSariaMinion>()] > 0f)
			{
				player.maxTurrets += 11;
				player.maxMinions += 10;
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
				recipe.AddIngredient(ItemID.ManaCrystal, 5);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 15);
				recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}

		}
	}
}
