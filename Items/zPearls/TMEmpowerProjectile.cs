using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zPearls
{
    public class TMEmpowerProjectile : ModProjectile
    {
        public override string Texture => "SariaMod/Items/zPearls/TMProjectile";
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
                // Unlock every base ability (Psychic is always available) up through Saria's Amethyst (7th) Form.
                modPlayer.SariaUnlockWater = true;
                modPlayer.SariaUnlockFire = true;
                modPlayer.SariaUnlockElectric = true;
                modPlayer.SariaUnlockRock = true;
                modPlayer.SariaUnlockBug = true;
                modPlayer.SariaUnlockGhost = true;
                // Unlock every charged attack, including Psychic's, up through the Amethyst Form.
                modPlayer.SariaUnlockPsychic2 = true;
                modPlayer.SariaUnlockWater2 = true;
                modPlayer.SariaUnlockFire2 = true;
                modPlayer.SariaUnlockElectric2 = true;
                modPlayer.SariaUnlockRock2 = true;
                modPlayer.SariaUnlockBug2 = true;
                modPlayer.SariaUnlockGhost2 = true;
                // Raise Saria to the level tied to her 7th Form and grant the TM Points that were "spent" unlocking everything above.
                modPlayer.Sarialevel = 6;
                modPlayer.SariaXp = 0;
                modPlayer.TMPointsUsed = 13;
                Projectile.Kill();
            }
        }
    }
}
