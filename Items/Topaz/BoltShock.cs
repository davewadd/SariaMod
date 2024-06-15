
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
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
using SariaMod.Buffs;
using Terraria.DataStructures;

namespace SariaMod.Items.Topaz
{
	public class BoltShock : ModProjectile
	{
		public override void SetStaticDefaults()
		{
						base.DisplayName.SetDefault("Saria");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 30;
			base.Projectile.height = 30;
			
			base.Projectile.alpha = 300;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			base.Projectile.netImportant = true;

			base.Projectile.penetrate = 1;
			base.Projectile.timeLeft = 8000;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 4;
		}
	
		public override bool? CanHitNPC(NPC target)
        {
			return false;
		}
		
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			base.Projectile.rotation += 0.095f;
		
			
			int head = -1;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				if (Main.projectile[i].active && Main.projectile[i].owner == Main.myPlayer)
				{
					if (head == -1 && Main.projectile[i].type == ModContent.ProjectileType<MechwormHead>())
					{
						head = i;
					}
					
					if (head != -1)
					{
						break;
					}
				}
			}

			if (head == -1)
			{



				int tailIndex;
				{
					tailIndex = -1;
					if (Main.myPlayer != player.whoAmI)
						return;
					int curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MechwormHead>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					if (Main.projectile.IndexInRange(curr))
					curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MechwormBody>(), (int)(Projectile.damage), Projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
					if (Main.projectile.IndexInRange(curr))
						Main.projectile[curr].originalDamage = Projectile.damage;
					int prev = curr;
					for (int i = 0; i < 10; i++)
						curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MechwormBody>(), (int)(Projectile.damage), Projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
					if (Main.projectile.IndexInRange(curr))
						Main.projectile[curr].originalDamage = Projectile.damage;
					Main.projectile[prev].localAI[1] = curr;


					tailIndex = curr;
				}
			}
		}
		
		



	}
}
