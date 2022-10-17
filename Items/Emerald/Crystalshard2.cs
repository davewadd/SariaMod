using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class Crystalshard2 : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.projectile.type] = 1;
			ProjectileID.Sets.MinionShot[base.projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 20;
			base.projectile.height = 20;
			base.projectile.netImportant = true;
			base.projectile.friendly = true;
			base.projectile.ignoreWater = true;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 7;
			base.projectile.minionSlots = 0f;
			base.projectile.extraUpdates = 1;
			base.projectile.aiStyle = 14;
			base.projectile.penetrate = -1;
			projectile.tileCollide = true;
			base.projectile.timeLeft = 500;
			
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Vector2 direction = target.Center - player.Center;
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
			target.AddBuff(BuffID.Electrified, 300);
			target.AddBuff(BuffID.Slow, 300);
			if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				if (Main.rand.NextBool(150))

				{
					{

						Item.NewItem(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<LivingPurpleShard>());
					}
				}
			}
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				if (Main.rand.NextBool(50))

				{
					Item.NewItem(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<LivingPurpleShard>());
				}
			}
			damage /= 2;
			knockback /= 2;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
        {
			projectile.velocity.Y = 1/4;
			projectile.velocity.X = 0;
			return false;

        }

        public override bool MinionContactDamage()
		{
		
			
				return true;
			
		}
		
		public override void AI()
		{
			Player player = Main.player[projectile.owner];


			
			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage


			Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * 2f);
			// Default movement parameters (here for attacking)

			






			
		}
		public override Color? GetAlpha(Color lightColor)
		{
			if (base.projectile.timeLeft < 85)
			{
				byte b2 = (byte)(base.projectile.timeLeft * 3);
				byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
				return new Color(b2, b2, b2, a2);
			}
			return new Color(255, 255, 255, 100);
		}
	}
}

