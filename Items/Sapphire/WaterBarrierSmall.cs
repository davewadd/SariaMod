using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Ruby;
using SariaMod.Items;
using SariaMod.Items.zPearls;
using System;
using SariaMod.Items.Bands;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using Terraria.ModLoader;
using System.IO;

namespace SariaMod.Items.Sapphire
{
	public class WaterBarrierSmall : ModProjectile
	{
		public override void SetStaticDefaults()
		{
						base.DisplayName.SetDefault("Saria");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 1;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		
		
		public override void SetDefaults()
		{
			base.Projectile.width = 500;
			base.Projectile.height = 500;
			
			base.Projectile.alpha = 300;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			
			base.Projectile.penetrate = -1;
			base.Projectile.timeLeft = 4;
			base.Projectile.ignoreWater = true;
			Projectile.timeLeft = 24;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 60;
		}


		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Player player2 = Main.LocalPlayer;
			FairyPlayer modPlayer = player.Fairy();
			Lighting.AddLight(base.Projectile.Center, 0f, 0.5f, 0f);

			if (Projectile.timeLeft ==24)
            {
				for (int i = 0; i < 50; i++)
				{
					Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
					Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BubbleDust2>(), speed * -23, Scale: 3.1f);
					float light = 0.15f * d.scale;
					Lighting.AddLight(d.position, light, light, light);
					d.noGravity = true;
				}
				SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Water2"), Projectile.Center);
			}
			if (modPlayer.Sarialevel == 6)
			{
				Projectile.damage = 900 + (modPlayer.SariaXp / 40);
			}
			else if (modPlayer.Sarialevel == 5)
			{
				Projectile.damage = 200 + (modPlayer.SariaXp / 342);
			}
			else if (modPlayer.Sarialevel == 4)
			{
				Projectile.damage = 75 + (modPlayer.SariaXp / 640);
			}
			else if (modPlayer.Sarialevel == 3)
			{
				Projectile.damage = 50 + (modPlayer.SariaXp / 1600);
			}
			else if (modPlayer.Sarialevel == 2)
			{
				Projectile.damage = 26 + (modPlayer.SariaXp / 833);
			}

			else if (modPlayer.Sarialevel == 1)
			{
				Projectile.damage = 15 + (modPlayer.SariaXp / 818);
			}
			else
			{
				Projectile.damage = 10 + (modPlayer.SariaXp / 600);
			}
			{
				
					if (player2.Hitbox.Intersects(Projectile.Hitbox) && (player2.team == player.team) && !player2.HasBuff(ModContent.BuffType<Healed>()))
				{
					{
						player2.AddBuff(ModContent.BuffType<Healed>(), 30);
						if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
						{
							for (int i = 0; i < 50; i++)
							{
								Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
								Dust d = Dust.NewDustPerfect(player2.Center, ModContent.DustType<Healdust3>(), speed * 2, Scale: 2.1f);
								d.noGravity = true;
							}
							SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
							player2.Heal((player.statLifeMax2/20));
						}
						if (player.HasBuff(ModContent.BuffType<Overcharged>()))
						{
							for (int i = 0; i < 50; i++)
							{
								Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
								Dust d = Dust.NewDustPerfect(player2.Center, ModContent.DustType<Healdust3>(), speed * 4, Scale: 3.1f);
								d.noGravity = true;
							}
							SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
							player2.Heal((player.statLifeMax2 / 16));
						}
					}
				}
			}

		}

	

		

		



	}
}
