using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class LocatorShard : ModProjectile
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
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 90;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.aiStyle = 1;
            base.Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            base.Projectile.timeLeft = 150;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            target.buffImmune[BuffID.CursedInferno] = false;
            target.buffImmune[BuffID.Confused] = false;
            target.buffImmune[BuffID.Slow] = false;
            target.buffImmune[BuffID.ShadowFlame] = false;
            target.buffImmune[BuffID.Ichor] = false;
            target.buffImmune[BuffID.OnFire] = false;
            target.buffImmune[BuffID.Frostburn] = false;
            target.buffImmune[BuffID.Poisoned] = false;
            target.buffImmune[BuffID.Venom] = false;
            target.buffImmune[BuffID.Electrified] = false;
            target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(BuffID.Slow, 300);
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.SariaXp++;
            knockback /= 2;
        }
        public override bool MinionContactDamage()
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.SariaBaseDamage();
            Projectile.damage /= 2;
            Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 2f);
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
