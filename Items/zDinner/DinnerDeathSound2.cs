using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework; // Add this using directive for Vector2
using SariaMod.Items.zDinner; // Add this using directive for SplitDinner
using SariaMod.Gores;
namespace SariaMod.Items.zDinner
{
    public class DinnerDeathSound2 : ModProjectile
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
            base.Projectile.netImportant = true;
            base.Projectile.penetrate = 1;
            base.Projectile.timeLeft = 150;
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
            base.Projectile.rotation += 0.095f;
            if (Projectile.timeLeft == 150)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/LetsGetOutOfHere"), Projectile.Center);
                for (int i = 0; i < 20; i++)
                {
                    // Roll a random number to select one of the three gore types.
                    int randGore = Main.rand.Next(3); // 0, 1, or 2
                    int goreType;
                    if (randGore == 0)
                    {
                        goreType = ModContent.GoreType<MessyDinner1>();
                    }
                    else if (randGore == 1)
                    {
                        goreType = ModContent.GoreType<MessyDinner2>();
                    }
                    else // randGore == 2
                    {
                        goreType = ModContent.GoreType<MessyDinner3>();
                    }
                    // Spawn the gore at the player's last position with a random spread velocity.
                    Gore.NewGore(
                        player.GetSource_Death(),
                        player.Center,
                        new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)),
                        goreType
                    );
                }
            }
        }
    }
}
