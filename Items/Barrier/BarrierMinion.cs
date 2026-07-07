using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Barrier
{
    public class BarrierMinion : ModProjectile
    {
        public override void SetDefaults()
        {
            base.Projectile.width = 42;
            base.Projectile.height = 350;
            base.Projectile.friendly = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.ignoreWater = true;
            Main.projFrames[base.Projectile.type] = 16;
            base.Projectile.timeLeft = 3000;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.minion = true;
            base.Projectile.minionSlots = 0f;
        }
        public override bool MinionContactDamage()
        {
            return true;
        }
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Barrier");
            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage /= 4;
            if (target.type == 68 || target.type == 325 || target.type == 327 || target.type == 325 || target.type == 344 || target.type == 345 || target.type == 346 || target.type == NPCID.Mothron || target.type == 82 || target.type == 87 || target.type == 83 || target.type == 253 || target.type == 467 || target.type == 473 || target.type == 474 || target.type == 475 || target.type == 476)
            {
                target.noTileCollide = false;
            }
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (base.Projectile.localAI[0] == 0f)
            {
                base.Projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = base.Projectile.damage / 4;
            }
            Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
            base.Projectile.frameCounter++;
            if (base.Projectile.frameCounter > 5)
            {
                base.Projectile.frame++;
                base.Projectile.frameCounter = 0;
            }
            if (base.Projectile.frame >= 16)
            {
                base.Projectile.frame = 0;
            }
        }
    }
}
