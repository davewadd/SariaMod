using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using SariaMod.Dusts;
namespace SariaMod.Items.Emerald
{
    public class LaunchHitBox2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 200;
            base.Projectile.height = 250;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 70;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            base.Projectile.timeLeft = 10;
            Projectile.alpha = 300;
        }
        public override bool? CanCutTiles()
        {
            return true;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Vector2 direction = target.Center - player.Center;
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
            damage /= 2;
            knockback = 40;
            target.buffImmune[ModContent.BuffType<MeteorLaunchDebuff>()] = false;
            target.AddBuff(ModContent.BuffType<MeteorLaunchDebuff>(), 20);
            if (target.position.X + (float)(target.width / 2) > Projectile.position.X + (float)(Projectile.width / 2))
            {
                hitDirection = 1;
            }
            else
            {
                hitDirection = -1;
            }
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center.X, target.Center.Y, 0, 0, ModContent.ProjectileType<HitCheckSound>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            int myPlayer = Main.myPlayer;
            player.immuneTime = 30;
            player.immune = true;
            player.immuneNoBlink = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return true;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            int owner = player.whoAmI;
            int GiantMoth = ModContent.ProjectileType<Emeraldspike2>();
            for (int i = 0; i < 1000; i++)
            {
                {
                    if (Main.projectile[i].active && i != Projectile.whoAmI && ((Main.projectile[i].type == GiantMoth && Main.projectile[i].owner == owner)))
                    {
                        Vector2 SpikeHitBox = Main.projectile[i].Center;
                        SpikeHitBox.Y += 10;
                        { 
                            Projectile.Center = SpikeHitBox;
                        }
                    }
                }
            }
            if (player.velocity.Y > 0)
            {
                Projectile.Kill();
            }
        }
    }
}
