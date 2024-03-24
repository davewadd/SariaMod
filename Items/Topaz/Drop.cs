using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;




using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
{
	public class Drop : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.Projectile.type] = 4;
			ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 20;
			base.Projectile.height = 20;
			base.Projectile.netImportant = true;
			base.Projectile.friendly = true;
			base.Projectile.ignoreWater = true;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 7;
			base.Projectile.minionSlots = 0f;
			base.Projectile.extraUpdates = 1;
			Projectile.alpha = 0;
			Projectile.aiStyle = 14;
			base.Projectile.tileCollide = true;
			base.Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			base.Projectile.timeLeft = 1500;
			
		}
		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
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
			target.buffImmune[ModContent.BuffType<Burning2>()] = false;
			target.AddBuff(ModContent.BuffType<Burning2>(), 200);
			target.AddBuff(BuffID.Electrified, 300);
			damage /= 100;
			knockback /= 1000;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<StaticFloor>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
				Projectile.Kill();
			}


			return false;
		}

		public override bool MinionContactDamage()
		{
		
			
				return false;
			
		}
		
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			
			
			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage


			Lighting.AddLight(Projectile.Center, Color.LightYellow.ToVector3() * 1f);
			// Default movement parameters (here for attacking)
			
			






			
		}
		public override Color? GetAlpha(Color lightColor)
		{
			if (base.Projectile.timeLeft < 85)
			{
				byte b2 = (byte)(base.Projectile.timeLeft * 3);
				byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
				return new Color(b2, b2, b2, a2);
			}
			return new Color(255, 255, 255, 100);
		}
	}
}

