using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Sapphire
{
    public class Bubble : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 34;
            base.Projectile.height = 34;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = false;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            Projectile.alpha = 100;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft = 500;
            Projectile.DamageType = DamageClass.Summon;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            NPC target = base.Projectile.Center.MinionHoming(500f, player);
            if (Projectile.frame <= 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private const int sphereRadius2 = 6;
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
            Player player2 = Main.LocalPlayer;
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
            target.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
            target.AddBuff(ModContent.BuffType<Frostburn2>(), 200);
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.SariaXp++;
            if (Main.rand.NextBool())
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Water>(), 0f, 0f, 0, default(Color), 1.5f);
            }//end of dust stuff
            knockback /= 100;
            if (Main.rand.NextBool(10))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                damage = (damage);
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                damage /= 4;
            }
            else
            {
                damage -= (damage) / 2;
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Player player2 = Main.LocalPlayer;
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            Projectile.SariaBaseDamage();
            if (Main.rand.NextBool(20))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((Projectile.Center.X) + radius * (float)Math.Cos(angle), (Projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Cold>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            if (Main.rand.NextBool(20))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((Projectile.Center.X) + radius * (float)Math.Cos(angle), (Projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            // If your minion is flying, you want to do this independently of any conditions
            if (Projectile.timeLeft <= 10)
            {
                Projectile.scale = 1.5f;
                if (Main.rand.NextBool())
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Water>(), 0f, 0f, 0, default(Color), 1.5f);
                }//end of dust stuff
                if (Main.rand.NextBool())
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }//end of dust stuff
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Bubblepop") with { Volume = 2f, Pitch = 1.3f });
            }
            bool foundTarget = true;
            Projectile.friendly = foundTarget;
            Lighting.AddLight(Projectile.Center, new Color(0, 0, Main.DiscoB).ToVector3() * 2f);
            // Default movement parameters (here for attacking)
            float speed = 13f;
            float inertia = 12f;
            if (Projectile.timeLeft == 3000)
            {
                SoundEngine.PlaySound(SoundID.Drown, base.Projectile.Center);
            }
        }
    }
}
