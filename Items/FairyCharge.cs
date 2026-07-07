using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Items
{
    public class FairyCharge : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[base.Projectile.type] = 8;
        }
        public override void AI()
        {
            {
                base.Projectile.frameCounter++;
                if (base.Projectile.frameCounter > 6)
                {
                    base.Projectile.frame++;
                    base.Projectile.frameCounter = 0;
                }
                if (base.Projectile.frame >= 8)
                {
                    base.Projectile.frame = 0;
                }
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            if (base.Projectile.timeLeft < 85)
            {
                byte b2 = (byte)(base.Projectile.timeLeft * 3);
                byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
                return new Color(b2, b2, b2, a2);
            }
            return new Color(255, 255, 255, 100);
        }
    }
}
