using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Strange;
namespace SariaMod.Items.zPearls
{
    public class TMProjectile : ModProjectile
    {
        public const float DistanceToCheck = 1100f;
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
                return false;
        }
        private const int sphereRadius3 = 1;
        private const int sphereRadius2 = 6;
        private const int sphereRadius4 = 32;
        private const int sphereRadius = 100;
        public override void SetDefaults()
        {
            base.Projectile.width = 96;
            base.Projectile.height = 78;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            Projectile.alpha = 300;
            base.Projectile.ignoreWater = false;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 50;
            base.Projectile.minionSlots = 0f;
            base.Projectile.timeLeft = 1800;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.minion = true;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            {
                modPlayer.SariaUnlockPsychic2 = false;
                modPlayer.SariaUnlockWater = false;
                modPlayer.SariaUnlockWater2 = false;
                modPlayer.SariaUnlockFire = false;
                modPlayer.SariaUnlockFire2 = false;
                modPlayer.SariaUnlockElectric = false;
                modPlayer.SariaUnlockElectric2 = false;
                modPlayer.SariaUnlockRock = false;
                modPlayer.SariaUnlockRock2 = false;
                modPlayer.SariaUnlockBug = false;
                modPlayer.SariaUnlockBug2 = false;
                modPlayer.SariaUnlockGhost = false;
                modPlayer.SariaUnlockGhost2 = false;
                modPlayer.TMPointsUsed = 0;
                int owner = player.whoAmI;
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is Saria modProjectile && U != Projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                    {
                        modProjectile.Transform = 0;
                    }
                }
                        Projectile.Kill();
            }
        }
    }
}