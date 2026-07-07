using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Emerald
{
    public class Crystalshard4 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 20;
            base.Projectile.height = 20;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = false;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.aiStyle = 14;
            base.Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            base.Projectile.timeLeft = 200;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.Y = 1 / 4;
            Projectile.velocity.X = 0;
            return false;
        }
        public override bool MinionContactDamage()
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            Lighting.AddLight(Projectile.Center, Color.Gray.ToVector3() * 2f);
            // Default movement parameters (here for attacking)
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
