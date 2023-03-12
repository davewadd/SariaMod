using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using SariaMod.Buffs;
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
	public class ConnersReaj : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Connor's Reajing Workout Supplements");
			base.Tooltip.SetDefault("Greatly increases defense and slight boost to melee attacks\n Range, Summon, and magic damage\n become much weaker.\n " + "\n " + SariaModUtilities.ColorMessage("Smells like regular flour...", new Color(0, 200, 250, 200)));
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
			player.statDefense += (player.statDefense/2) ;
			player.rangedDamage -= (player.rangedDamage / 2);
			player.meleeDamage += (player.meleeDamage/ 9);
			player.magicDamage -= (player.magicDamage / 2);
			player.minionDamage -= (player.minionDamage / 2);
			if (player.statLife <= (player.statLifeMax2) / 4 && !player.HasBuff(ModContent.BuffType<ReajBuff>()))
            {
				player.statLife += player.statLifeMax2/3;
				player.AddBuff(ModContent.BuffType<ReajBuff>(), 8000);
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Healpulse"), player.Center);
			}
		}
		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.WrathPotion);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 3);
				recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}

		}
	}
}
