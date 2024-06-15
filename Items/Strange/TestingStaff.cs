using Microsoft.Xna.Framework; 


using System;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
 
using SariaMod.Items.Platinum;
using SariaMod.Items.zPearls;
using System.Linq;
using SariaMod.Items.zBookcases;
using SariaMod.Items.Strange;
using Terraria;
using SariaMod.Buffs;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{


	public class TestingStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("XPStaff");
			Tooltip.SetDefault(SariaModUtilities.ColorMessage("Shows the Level of XP Saria has when used", new Color(0, 200, 250, 200)));
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			
			Item.knockBack = 13f;
			Item.mana = 1;
			Item.width = 32;
			Item.height = 32;
			base.Item.useTime = (base.Item.useAnimation = 10);
			Item.useStyle = 1;
			Item.value = Item.buyPrice(0, 30, 0, 0);
			Item.rare = ItemRarityID.Cyan;
			Item.UseSound = SoundID.Item49;
			Item.autoReuse = true;
			// These below are needed for a minion weapon
			Item.noMelee = true;
            Item.damage = 80;
			Item.DamageType = DamageClass.Summon;
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration
			Item.shootSpeed = 0;
			Item.shoot = ModContent.ProjectileType<LightningCloud>();
			Item.buffType = ModContent.BuffType<WillOWispBuff>();
			Item.buffTime = 2;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(Item.Center, Color.SeaShell.ToVector3() * 2f);
		}

       

      

       









        
	}

}