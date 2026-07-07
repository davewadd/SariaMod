using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Topaz;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class Ztarget5 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        public int ChannelTimer;
        public int Stage;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChannelTimer);
            writer.Write(Stage);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ChannelTimer = (int)reader.ReadInt32();
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
            int owner = player.whoAmI;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Saria modProjectile && modProjectile.IsCharging >= 1 && modProjectile.Transform == 3 && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                {
                        Projectile.timeLeft = 10;
                    if (ChannelTimer <= 200 && Stage <= 0)
                    {
                        ChannelTimer++;
                    }
                }
            }
            if (ChannelTimer == 201 && Stage <= 0)
            {
                if (player.statMana >= player.statManaMax2 / 2)
                {
                    player.statMana -= player.statManaMax2 / 2;
                    player.manaRegenDelay = 30;
                    Stage = 1;
                }
            }
           /// Main.NewText(ChannelTimer);
            if (Stage == 1)
            {
                {
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<LightningCloud>()] <= 0f && Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<LightningCloud>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        player.AddBuff(ModContent.BuffType<ThunderCloudBuff>(), 4);
                        Stage = 2;
                    }
                    else if (player.ownedProjectileCounts[ModContent.ProjectileType<LightningCloud>()] > 0f)
                    {
                        int CloudStrife = ModContent.ProjectileType<LightningCloud>();
                        for (int l = 0; l < 1000; l++)
                        {
                            if (Main.projectile[l].active && l != base.Projectile.whoAmI && ((Main.projectile[l].type == CloudStrife && Main.projectile[l].owner == owner)))
                            {
                                {
                                    Main.projectile[l].Kill();
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<LightningCloud>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    player.AddBuff(ModContent.BuffType<ThunderCloudBuff>(), 4);
                                    Stage = 2;
                                }
                            }
                        }
                    }
                    Projectile.netUpdate = true;
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
