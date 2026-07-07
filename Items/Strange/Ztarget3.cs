using Microsoft.Xna.Framework;
using SariaMod.Dusts;
using SariaMod.Items.Sapphire;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class Ztarget3 : ModProjectile
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
        private int SoundTimer2;
        public int Stage;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChannelTimer);
            writer.Write(SoundTimer);
            writer.Write(SoundTimer2);
            writer.Write(Stage);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ChannelTimer = (int)reader.ReadInt32();
            SoundTimer = (int)reader.ReadInt32();
            SoundTimer2 = (int)reader.ReadInt32();
            Stage = (int)reader.ReadInt32();
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
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Saria modProjectile && modProjectile.IsCharging >= 1 && modProjectile.Transform == 1 && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                {
                    Projectile.timeLeft = 300;
                    if (ChannelTimer <= 200 && Stage <= 0)
                    {
                        ChannelTimer++;
                    }
                    if (ChannelTimer <= 550 && Stage == 1)
                    {
                        ChannelTimer++;
                    }
                    if (ChannelTimer <= 899 && Stage == 2)
                    {
                        ChannelTimer++;
                    }
                    if (Stage == 3)
                    {
                        ChannelTimer = 901;
                    }
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<WaterSummon>()] > 0f && ChannelTimer < 899)
            {
                ChannelTimer = 899;
                Stage = 2;
                SoundTimer2 = 200;
            }
            SoundTimer2++;
            if (SoundTimer2 > 200 && ChannelTimer > 200)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/WaterFormLoop"), Projectile.Center);
                SoundTimer2 = 0;
            }
            if (ChannelTimer == 201)
            {
                if (player.statMana >= player.statManaMax2 / 8)
                {
                    player.statMana -= player.statManaMax2 / 8;
                    player.manaRegenDelay = 350;
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Switch1"), Projectile.Center);
                    Stage = 1;
                }
            }
            if (ChannelTimer == 551)
            {
                if (player.statMana >= player.statManaMax2 / 4)
                {
                    player.statMana -= player.statManaMax2 / 4;
                    player.manaRegenDelay = 350;
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Switch2"), Projectile.Center);
                    Stage = 2;
                }
            }
            if (ChannelTimer == 900)
            {
                if (player.statMana >= player.statManaMax2 / 2)
                {
                    player.statMana -= player.statManaMax2 / 2;
                    player.manaRegenDelay = 350;
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Switch3"), Projectile.Center);
                    Stage = 3;
                }
            }
            if (ChannelTimer > 200)
            {
                if (Main.rand.NextBool(8))
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    {
                        Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust3>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
            }
            if (ChannelTimer > 600)
            {
                if (Main.rand.NextBool(8))
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(100 * 100));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    {
                        Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust3>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
            }
            if (ChannelTimer > 200 && ChannelTimer <= 550 && SoundTimer >= 62)
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BubbleDust2>(), speed * -7, Scale: 2.7f);
                    d.noGravity = true;
                }
                SoundTimer = 0;
            }
            if (ChannelTimer > 550 && ChannelTimer <= 900 && SoundTimer >= 28)
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BubbleDust2>(), speed * -9, Scale: 3.0f);
                    d.noGravity = true;
                }
                SoundTimer = 0;
            }
            if (Stage >= 3)
            {
                if (SoundTimer >= 18)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BubbleDust2>(), speed * -11, Scale: 3.5f);
                        d.noGravity = true;
                    }
                    SoundTimer = 0;
                }
                int VeilBubble = ModContent.ProjectileType<WaterCheck>();
                for (int b = 0; b < 1000; b++)
                {
                    if (Main.projectile[b].active && b != base.Projectile.whoAmI && ((Main.projectile[b].type == VeilBubble && Main.projectile[b].owner == owner)))
                    {
                        {
                            if (player.ownedProjectileCounts[ModContent.ProjectileType<WaterSummon>()] <= 0f && Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<WaterSummon>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                            else if (player.ownedProjectileCounts[ModContent.ProjectileType<WaterSummon>()] > 0f)
                            {
                                int VeilBubble2 = ModContent.ProjectileType<WaterSummon>();
                                for (int l = 0; l < 1000; l++)
                                {
                                    if (Main.projectile[l].active && l != base.Projectile.whoAmI && ((Main.projectile[l].type == VeilBubble2 && Main.projectile[l].owner == owner)))
                                    {
                                        {
                                            Main.projectile[l].Kill();
                                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<WaterSummon>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                }
                            }
                            Main.projectile[b].Kill();
                            Projectile.netUpdate = true;
                        }
                    }
                }
            }
            FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
            {
                float distanceFromTarget = 10f;
                Vector2 targetCenter = Projectile.position;
                bool foundTarget = false;
                Projectile.friendly = foundTarget;
                if (Projectile.alpha == 0)
                {
                    Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * 1f);
                }
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
