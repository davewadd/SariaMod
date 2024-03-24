using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;
using Terraria.Audio;




using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
{
	public class LightningStrike3 : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 40;
			base.Projectile.height = 400;
			
			base.Projectile.alpha = 100;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			
			base.Projectile.penetrate = -1;
			base.Projectile.timeLeft = 150;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 10;
		}
		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			if ((player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator2>()] <= 0f))
            {
				Projectile.Kill();
            }

				if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				
				Projectile.localNPCHitCooldown = 6;
			}
			for (int b = 0; b < Main.maxNPCs; b++)
			{
				NPC npc = Main.npc[b];

				
				if ( Main.npc[b].Hitbox.Intersects(base.Projectile.Hitbox) && Main.npc[b].active && Main.npc[b].friendly == false)
				{
					if ((player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrike2>()] <= 0f) && (!player.HasBuff(ModContent.BuffType<StatRaise>())) && (!player.HasBuff(ModContent.BuffType<Overcharged>())) && (!player.HasBuff(ModContent.BuffType<StatLower>())) && !Main.player[Main.myPlayer].ZoneSnow)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 20, Projectile.position.Y + 160, 0, 0, ModContent.ProjectileType<LightningStrike2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					}
					if ((player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikeRed>()] <= 0f) && (!player.HasBuff(ModContent.BuffType<StatRaise>())) && (!player.HasBuff(ModContent.BuffType<Overcharged>())) && (player.HasBuff(ModContent.BuffType<StatLower>())) && !Main.player[Main.myPlayer].ZoneSnow)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 20, Projectile.position.Y + 160, 0, 0, ModContent.ProjectileType<LightningStrikeRed>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					}
					if ((player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikeBlue>()] <= 0f) && (player.HasBuff(ModContent.BuffType<StatRaise>())) && (!player.HasBuff(ModContent.BuffType<Overcharged>())) && (!player.HasBuff(ModContent.BuffType<StatLower>())) && !Main.player[Main.myPlayer].ZoneSnow)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 20, Projectile.position.Y + 160, 0, 0, ModContent.ProjectileType<LightningStrikeBlue>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					}
					if ((player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikePurple>()] <= 0f) && (player.HasBuff(ModContent.BuffType<Overcharged>())) && !Main.player[Main.myPlayer].ZoneSnow)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 20, Projectile.position.Y + 160, 0, 0, ModContent.ProjectileType<LightningStrikePurple>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					}
					if ((player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikePink>()] <= 0f) && Main.player[Main.myPlayer].ZoneSnow)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 20, Projectile.position.Y + 160, 0, 0, ModContent.ProjectileType<LightningStrikePink>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					}
					Projectile.netUpdate = true;
				}



			}
			{



				
				
				
					
				
				base.Projectile.frameCounter++;
				if (base.Projectile.frameCounter >= 4)
				{
					base.Projectile.frame++;
					base.Projectile.frameCounter = 0;

				}
				if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
				{
					base.Projectile.frame = 3;
					
				}
			
			}
			
			
			
			
			
		}

		

		
		
		



	}
}
