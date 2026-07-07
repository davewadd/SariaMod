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
    public class ShadowClaw : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 48;
            base.Projectile.height = 52;
            base.Projectile.alpha = 0;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 500;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 400;
        }
        private const int sphereRadius = 40;
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            Projectile.velocity.X = 0;
            Projectile.velocity.Y = 0;
            Projectile.alpha += 1;
            if (Projectile.alpha == 300f)
            {
                Projectile.active = false;
            }
            Lighting.AddLight(Projectile.Center, Color.DarkViolet.ToVector3() * 2f);
            if (Main.rand.NextBool(30))
            {
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            int frameSpeed = 5;
            {
                base.Projectile.frameCounter++;
                if (Projectile.frameCounter >= frameSpeed)
                    if (base.Projectile.frameCounter > 4)
                    {
                        base.Projectile.frame++;
                        base.Projectile.frameCounter = 0;
                    }
                if (base.Projectile.frame >= 4)
                {
                    base.Projectile.frame = 3;
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            int noise = 0;
            Player player = Main.player[base.Projectile.owner];
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
            knockback *= 0;
            modPlayer.SariaXp++;
            if (noise == 0)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ShadowClaw"), base.Projectile.Center);
                noise++;
            }
        }
    }
}
