using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Items
{
    public class BlueCharge : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 8;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 52;
            base.Projectile.height = 48;
            Projectile.timeLeft = 2000;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return true;
        }
        public override void AI()
        {
            int frameSpeed = 20; //reduced by half due to framecounter speedup
            Projectile.frameCounter += 2;
            if (Projectile.frameCounter >= frameSpeed)
            {
                base.Projectile.frameCounter++;
                if (base.Projectile.frameCounter > 2)
                {
                    base.Projectile.frame++;
                    base.Projectile.frameCounter = 0;
                }
                if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
                {
                    base.Projectile.frame = 0;
                }
            }
        }
    }
}
