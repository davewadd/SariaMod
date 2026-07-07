using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zPearls
{
    public class EmptyNote : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 30;
            base.Projectile.height = 30;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = 1;
            base.Projectile.timeLeft = 2;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            Lighting.AddLight(Projectile.Center, Color.LightGreen.ToVector3() * 1f);
            Projectile.position.X = player.position.X;
            Projectile.position.Y = player.position.Y;
            if (Projectile.timeLeft == 2 && Main.player[Main.myPlayer].active && Main.bloodMoon)
            {
                Item.NewItem(Projectile.GetSource_FromThis(), (int)(Projectile.position.X + 0), (int)(Projectile.position.Y + 0), 0, 0, ModContent.ItemType<BloodOcarina>());
            }
        }
    }
}
