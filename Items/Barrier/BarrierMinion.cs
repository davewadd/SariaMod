using System;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Barrier
{
	public class BarrierMinion : ModProjectile
	{
		public override void SetDefaults()
		{
			
			base.projectile.width = 42;
			base.projectile.height = 350;
			base.projectile.friendly = true;
			base.projectile.usesLocalNPCImmunity = true; 
			base.projectile.ignoreWater = true;
			Main.projFrames[base.projectile.type] = 16;
			base.projectile.timeLeft = 3000;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.minion = true;
			base.projectile.minionSlots = 0f;
		}
		public override bool MinionContactDamage()
		{
			return true;
		}
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Barrier");
			 ProjectileID.Sets.MinionSacrificable[base.projectile.type] = false;
		}
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {

			damage /= 4;
             hitDirection = -1;
			if (target.type == 68 || target.type == 325 || target.type == 327 || target.type == 325 || target.type == 344 || target.type == 345 || target.type == 346 || target.type == NPCID.Mothron || target.type == 82 || target.type == 87 || target.type == 83 || target.type == 253 || target.type == 467 || target.type == 473 || target.type == 474 || target.type == 475 || target.type == 476)
			{
				target.noTileCollide = false;


			}
		}

        public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			
			if (base.projectile.localAI[0] == 0f)
			{
				base.projectile.Fairy().spawnedPlayerMinionDamageValue = player.MinionDamage();
				base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = base.projectile.damage/4;
				
			
			}
			if (player.MinionDamage() != base.projectile.Fairy().spawnedPlayerMinionDamageValue)
			{
				int trueDamage = (int)((float)base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue / base.projectile.Fairy().spawnedPlayerMinionDamageValue * player.MinionDamage());
				base.projectile.damage = trueDamage/4;
			}

			Lighting.AddLight(projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
			base.projectile.frameCounter++;
			if (base.projectile.frameCounter > 5)
			{
				base.projectile.frame++;
				base.projectile.frameCounter = 0;
			}
			if (base.projectile.frame >= 16)
			{
				base.projectile.frame = 0;
			}
		}

	}
}
