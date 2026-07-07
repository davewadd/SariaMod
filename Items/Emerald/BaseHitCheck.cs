using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Dusts;
using System;
using Terraria.Audio;
namespace SariaMod.Items.Emerald
{
    public abstract class BaseHitCheck : ModProjectile
    {
        protected virtual bool SpawnKillEffects => false;
        protected virtual float KillDustScale => 1.5f;
        protected virtual int PassiveProjectileType => ModContent.ProjectileType<RupeeXPassive>();
        protected virtual int PassiveDamageIncrement => 1;
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 10;
            base.Projectile.height = 10;
            Projectile.alpha = 300;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = false;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 1;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            int owner = player.whoAmI;
            if (SpawnKillEffects)
            {
                for (int b = 0; b < 50; b++)
                {
                    Vector2 dustspeed5 = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RupeePick>(), dustspeed5 * 10, Scale: KillDustScale);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
            }
            if (player.ownedProjectileCounts[PassiveProjectileType] >= 1f)
            {
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].type == PassiveProjectileType && U != Projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                    {
                        if (Main.projectile[U].ModProjectile is BaseRupeeXPassive modRupee)
                        {
                            modRupee.Damage += PassiveDamageIncrement;
                        }
                    }
                }
            }
        }
    }
}
