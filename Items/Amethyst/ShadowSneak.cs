using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Amethyst
{
    public class ShadowSneak : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 15;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 100;
            base.Projectile.height = 270;
            base.Projectile.alpha = 0;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 200;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 20;
        }
        private const int sphereRadius = 3;
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
            Lighting.AddLight(Projectile.Center, Color.DarkViolet.ToVector3() * 12f);
            if (Projectile.frame >= 5)
            {
                if (Main.rand.NextBool())
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 130) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Shadow2>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Projectile.frame <= 2)
            {
                for (int j = 0; j < 10; j++) //set to 2
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(20f, 130f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Ghostsmoke>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
            }
            {
                Projectile.knockBack = 5;
                if (Projectile.timeLeft >= 190)
                {
                    base.Projectile.velocity.X = (float)((.001) * player.direction);
                }
                base.Projectile.velocity.Y = 0;
                base.Projectile.frameCounter++;
                int frameSpeed = 15;
                {
                    base.Projectile.frameCounter++;
                    if (Projectile.frameCounter >= frameSpeed)
                        if (base.Projectile.frameCounter > 14)
                        {
                            base.Projectile.frame++;
                            base.Projectile.frameCounter = 0;
                        }
                    if (base.Projectile.frame >= 14)
                    {
                        base.Projectile.frame = 14;
                        Projectile.timeLeft -= 30;
                    }
                }
            }
            if (Projectile.timeLeft >= 200)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath59, base.Projectile.Center);
            }
            if (Projectile.timeLeft == 90)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Spiritcrawl"), base.Projectile.Center);
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Poe"), base.Projectile.Center);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            FairyProjectile.DrawCenteredAndAfterimage(base.Projectile, lightColor, ProjectileID.Sets.TrailingMode[base.Projectile.type]);
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.Projectile.owner];
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
            target.buffImmune[ModContent.BuffType<SariaCurse>()] = false;
            target.AddBuff(ModContent.BuffType<SariaCurse>(), 2000);
            modPlayer.SariaXp++;
            if (target.defense > 200)
            {
                target.defense = 200;
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                damage += (damage) / 4;
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                damage /= 2;
            }
        }
    }
}
