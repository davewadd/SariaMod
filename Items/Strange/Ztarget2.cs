using Microsoft.Xna.Framework;
using SariaMod.Dusts;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class Ztarget2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        public int ChannelTimer;
        private int SoundTimer;
        private int TimesRepeat;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChannelTimer);
            writer.Write(SoundTimer);
            writer.Write(TimesRepeat);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ChannelTimer = (int)reader.ReadInt32();
            SoundTimer = (int)reader.ReadInt32();
            TimesRepeat = (int)reader.ReadInt32();
        }
        private const int sphereRadius = 100;
        public override void SetDefaults()
        {
            base.Projectile.width = 86;
            base.Projectile.height = 86;
            base.Projectile.netImportant = true;
            base.Projectile.alpha = 0;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 401;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            ///Main.NewText(ChannelTimer);
            base.Projectile.rotation += (float)0.07;
            if (Projectile.timeLeft == 2)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetCancel"), Projectile.Center);
            }
            if (Projectile.timeLeft == 401)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetDeep"), Projectile.Center);
            }
            if (SoundTimer < 101)
            {
                SoundTimer++;
            }
            int owner = player.whoAmI;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Saria modProjectile && modProjectile.IsCharging >= 1 && modProjectile.Transform == 0 && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                {
                    {
                        Projectile.timeLeft = 300;
                        if (ChannelTimer <= 900)
                        {
                            ChannelTimer++;
                        }
                    }
                }
            }
            if (ChannelTimer > 200)
            {
                {
                    Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), 0, 0, ModContent.DustType<BurningPsychic>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (ChannelTimer > 550)
            {
                {
                    Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), 0, 0, ModContent.DustType<BurningPsychic3>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (ChannelTimer > 900)
            {
                {
                    Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), 0, 0, ModContent.DustType<BurningPsychic2Long>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (ChannelTimer > 200)
            {
                if (Main.rand.NextBool(8))
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(54 * 54));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    {
                        Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Psychic3>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
            }
            if (ChannelTimer > 200 && ChannelTimer <= 550 && SoundTimer >= 62 && player.statMana >= 5)
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<LocatorR>()] <= 0f)
                {
                    player.statMana -= 5;
                    player.manaRegenDelay = 30;
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 40, 4, 0, ModContent.ProjectileType<LocatorR>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), speed * -5, Scale: 2.5f);
                    d.noGravity = true;
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Absorb1"), Projectile.Center);
                }
                SoundTimer = 0;
            }
            if (ChannelTimer > 550 && ChannelTimer <= 900 && SoundTimer >= 28 && player.statMana >= 5)
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<LocatorR>()] <= 1f)
                {
                    player.statMana -= 5;
                    player.manaRegenDelay = 30;
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 40, 4, 0, ModContent.ProjectileType<LocatorR>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), speed * -7, Scale: 3.0f);
                    d.noGravity = true;
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Absorb2"), Projectile.Center);
                }
                SoundTimer = 0;
            }
            if (ChannelTimer > 900 && SoundTimer >= 18)
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), speed * -10, Scale: 3.5f);
                    d.noGravity = true;
                }
                if (TimesRepeat <= 13)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Absorb3"), Projectile.Center);
                }
                if (TimesRepeat == 14)
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, base.Projectile.Center);
                }
                TimesRepeat++;
                SoundTimer = 0;
            }
            if (ChannelTimer > 900)
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<LocatorR>()] <= 2f && player.statMana >= 5)
                {
                    player.statMana -= 5;
                    player.manaRegenDelay = 30;
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 40, 4, 0, ModContent.ProjectileType<LocatorR>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
            }
            FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
            {
                float distanceFromTarget = 10f;
                Vector2 targetCenter = Projectile.position;
                bool foundTarget = false;
                // This code is required if your minion weapon has the targeting feature
                Projectile.friendly = foundTarget;
                if (Projectile.alpha == 0)
                {
                    Lighting.AddLight(Projectile.Center, Color.Pink.ToVector3() * 1f);
                }
                // Default movement parameters (here for attacking)
                float inertia = 13f;
                Vector2 idlePosition = player.Center;
                float minionPositionOffsetX = ((60 + Projectile.minionPos / 80) * player.direction) - 15;
                idlePosition.Y -= 70f;
                idlePosition.X += minionPositionOffsetX;
                Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
                float distanceToIdlePosition = vectorToIdlePosition.Length();
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
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
            damage /= damage / 4;
        }
    }
}
