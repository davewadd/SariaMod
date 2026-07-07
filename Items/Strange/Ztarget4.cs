using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Ruby;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class Ztarget4 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        private int ChannelTimer;
        private int SoundTimer;
        private int SoundTimer2;
        private int HitMax;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChannelTimer);
            writer.Write(SoundTimer);
            writer.Write(SoundTimer2);
            writer.Write(HitMax);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ChannelTimer = (int)reader.ReadInt32();
            SoundTimer = (int)reader.ReadInt32();
            SoundTimer2 = (int)reader.ReadInt32();
            HitMax = (int)reader.ReadInt32();
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
            if (HitMax >= 1 && (player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp2>()] < 8))
            {
                HitMax = 0;
            }
            if (HitMax <= 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp2>()] >= 8))
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<ShadowFlameDustCharge>(), speed * 6, Scale: 8.5f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot, base.Projectile.Center);
                Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 4f);
                HitMax = 1;
            }
            if (Projectile.timeLeft == 2)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetCancel"), Projectile.Center);
            }
            if (Projectile.timeLeft == 401)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetDeep"), Projectile.Center);
            }
            Projectile.Center = player.Center;
            int owner = player.whoAmI;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Saria modProjectile && modProjectile.IsCharging >= 1 && modProjectile.Transform == 2 && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                {
                    {
                        Projectile.timeLeft = 20;
                        if (ChannelTimer <= 900)
                        {
                            ChannelTimer++;
                        }
                    }
                }
            }
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp2>()] >= 8))
            {
                ChannelTimer = 0;
            }
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp>()] <= 0))
            {
                if (ChannelTimer >= 90)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Ignite"), Projectile.Center);
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<WillOWisp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    player.AddBuff(ModContent.BuffType<WillOWispBuff>(), 2);
                    ChannelTimer = 0;
                }
            }
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp>()] >= 1))
            {
                if (ChannelTimer >= 50)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Ignite"), Projectile.Center);
                    ChannelTimer = 0;
                    for (int i = 0; i < 1000; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].ModProjectile is WillOWisp modProjectile && modProjectile.WispHits >= 1 && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                        {
                            {
                                Projectile.timeLeft = 20;
                                if (ChannelTimer <= 900)
                                {
                                    modProjectile.WispHits += 3;
                                    ChannelTimer++;
                                }
                            }
                        }
                    }
                }
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
