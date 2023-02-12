using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SariaMod.Buffs;
using System;
using Terraria;
using SariaMod.Items.Sapphire;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amethyst
{
	public class Ghostsmoke : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.projectile.type] = 4;
			ProjectileID.Sets.MinionShot[base.projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 200;
			base.projectile.height = 200;
			base.projectile.netImportant = true;
			base.projectile.friendly = true;
			base.projectile.ignoreWater = true;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 800;
			base.projectile.minionSlots = 0f;
			base.projectile.extraUpdates = 1;
			projectile.alpha = 0;
			projectile.scale = .5f;
			projectile.velocity *= .4f;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.timeLeft = 2000;
			base.projectile.minion = true;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			target.buffImmune[BuffID.CursedInferno] = false;
			target.buffImmune[BuffID.Confused] = false;
			target.buffImmune[BuffID.Slow] = false;
			target.buffImmune[BuffID.ShadowFlame] = false;
			target.buffImmune[BuffID.Ichor] = false;
			target.buffImmune[BuffID.OnFire] = false;
			target.buffImmune[BuffID.Frostburn] = false;
			target.buffImmune[BuffID.Poisoned] = false;
			target.buffImmune[BuffID.Venom] = false;
			target.buffImmune[BuffID.Electrified] = false;
			target.buffImmune[ModContent.BuffType<SariaCurse>()] = false;
			target.AddBuff(ModContent.BuffType<SariaCurse>(), 2000);
			target.AddBuff(BuffID.Slow, 300);
			if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				target.AddBuff(ModContent.BuffType<SariaCurse>(), 2000);
			}
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				target.AddBuff(ModContent.BuffType<SariaCurse>(), 300000);
			}
			if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				damage *= 0;
			}
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
				{
					damage /= 4;
				}
			knockback = 0;

		}
		public override void AI()
		{

			Player player = Main.player[projectile.owner];
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			projectile.rotation += projectile.velocity.X * 0.01f;
			{
				
				
				projectile.scale *= 1.01f;
			projectile.alpha += 1;
			if (projectile.alpha == 300f)
			{
				projectile.active = false;
			}
			
			float light = 0.35f * projectile.scale;
			Lighting.AddLight(projectile.position, Color.DarkViolet.ToVector3() * 6f);
			
			
		}
			
			
		
			
			

			
			int frameSpeed = 15;
			{
				base.projectile.frameCounter++;
				if (projectile.frameCounter >= frameSpeed)
					
          
					if (base.projectile.frameCounter > 4)
					{
						base.projectile.frame++;
						base.projectile.frameCounter = 0;
					}
				if (base.projectile.frame >= 4)
				{
					base.projectile.frame = 3;
				}
								
			}
		}
		
	}
}

