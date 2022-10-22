using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using SariaMod.Buffs;

using Terraria;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
using SariaMod.Items.zPearls;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amber
{
	public class GreenMothItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Green Moth Item");
			Tooltip.SetDefault("Use when amber Saria is present!\n Summons the Green Moth Goliath");
		}


		public override void SetDefaults()
		{
			
			base.item.width = 40;
			base.item.height = 40;
			base.item.maxStack = 999;
			item.value = Item.buyPrice(20, 0, 0, 0);

			item.rare = ItemRarityID.Expert;
			item.consumable = true;
			item.useTime = 36;
			item.useAnimation = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.UseSound = SoundID.Item45;
			item.autoReuse = false;
			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<GreenPoint>();

		}
		public override bool AltFunctionUse(Player player)
		{
			return false;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			
			Lighting.AddLight(item.Center, Color.OrangeRed.ToVector3() * 2f);
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{

			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(item.buffType, 2);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.


			position = Main.MouseWorld;


			return true;
		}
		public override bool CanUseItem(Player player)
		{ 
			if (player.altFunctionUse ==2)
            {
				item.consumable = false;
				return false;
            }
			if (player.altFunctionUse !=2)
            {
				item.consumable = true;
            }
			if (((player.ownedProjectileCounts[ModContent.ProjectileType<AmberSariaMinion>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<AMASariaMinion>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<DASariaMinion>()] <= 0f)) || player.ownedProjectileCounts[ModContent.ProjectileType<GreenPoint>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<AmberGreen>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<GreenMoth>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGiant>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath>()] > 0f)
			{
				return false;
			}
			else
			{
				return true;
			}
            }
		
	}
}
