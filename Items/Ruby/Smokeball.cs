using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Ruby
{
    public class Smokeball : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 4;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 200;
            base.Projectile.height = 200;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 800;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
            Projectile.scale = .5f;
            Projectile.velocity *= .4f;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft = 2000;
            base.Projectile.minion = true;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
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
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;
            target.AddBuff(ModContent.BuffType<Burning2>(), 200);
            if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
            {
                damage = 1;
            }
            if (player.HasBuff(ModContent.BuffType<Overcharged>()))
            {
                damage /= 6;
            }
            knockback = 0;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            Projectile.rotation += Projectile.velocity.X * 0.01f;
            {
                Projectile.scale *= 1.01f;
                Projectile.alpha += 1;
                if (Projectile.alpha == 300f)
                {
                    Projectile.active = false;
                }
                float light = 0.35f * Projectile.scale;
                Lighting.AddLight(Projectile.position, Color.OrangeRed.ToVector3() * 6f);
            }
            int frameSpeed = 15;
            {
                base.Projectile.frameCounter++;
                if (Projectile.frameCounter >= frameSpeed)
                    if (base.Projectile.frameCounter > 4)
                    {
                        base.Projectile.frame++;
                        base.Projectile.frameCounter = 0;
                    }
                if (base.Projectile.frame >= 4)
                {
                    base.Projectile.frame = 3;
                }
            }
        }
    }
}
