using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Dusts;
using System;
using Terraria.Audio;
namespace SariaMod.Items.Emerald
{
    public class HitCheckSound : ModProjectile
    {
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
            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/MeteorSmash"), Projectile.Center);
            player.immuneTime = 30;
            player.immune = true;
            player.immuneNoBlink = true;
            player.wingTime = player.wingTimeMax;
            for (int i = 0; i < 50; i++)
                {
                    Vector2 speed2 = Main.rand.NextVector2CircularEdge(.5f, .5f);
                    Dust d = Dust.NewDustPerfect(Projectile.Bottom, ModContent.DustType<MeteorSpike>(), speed2 * 6, Scale: 2.7f);
                    d.noGravity = true;
                }
        }
    }
}
