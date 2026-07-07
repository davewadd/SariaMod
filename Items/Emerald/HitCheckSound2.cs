using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Dusts;
using System;
using Terraria.Audio;
namespace SariaMod.Items.Emerald
{
    public class HitCheckSound2 : ModProjectile
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
            for (int i = 0; i < 40; i++)
            {
                Vector2 speed = Utils.RandomVector2(Main.rand, -1f, 1f);
                speed.X = Main.rand.NextFloat(-.25f, .25f);
                speed.Y = Main.rand.NextFloat(1f, -1f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<MeteorSpike>(), speed * 30, Scale: 5.7f);
                d.noGravity = true;
            }
        }
    }
}
