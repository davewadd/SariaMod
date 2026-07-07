using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using SariaMod.Dusts;
namespace SariaMod.Items.Emerald
{
    public class BufferProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 500;
            base.Projectile.height = 150;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 70;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            base.Projectile.timeLeft = 70;
            Projectile.minion = false;
            Projectile.minionSlots = 0;
            Projectile.alpha = 300;
        }
        public override bool? CanHitNPC(NPC target)
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (player.velocity.Y > 0)
            {
                return target.CanBeChasedBy(Projectile);
            }
            else
            {
                return false;
            }
        }
        public override bool MinionContactDamage()
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
            target.buffImmune[ModContent.BuffType<MeteorSpikeDebuff>()] = false;
            target.AddBuff(ModContent.BuffType<MeteorSpikeDebuff>(), 40);
            damage = 1;
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center.X, target.Center.Y, 0, 0, ModContent.ProjectileType<HitCheckSound>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (Projectile.spriteDirection == -1)
            {
                Projectile.position.X = player.Center.X - 80;
            }
            if (Projectile.spriteDirection == 1)
            {
                Projectile.position.X = player.Center.X - 70;
            }
            Projectile.position.X = player.position.X - 240;
            Projectile.position.Y = player.Center.Y ;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && player.immune == false && Main.projectile[i].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
                {
                    Main.projectile[i].Kill();
                }
            }
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3>()] > 0f)
            {
                SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, base.Projectile.Center);
                Vector2 newmiddle = Projectile.Center;
                newmiddle.Y -= 150;
                for (int j = 0; j < 4; j++) //set to 2
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), newmiddle + Utils.RandomVector2(Main.rand, -200f, 200f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Crystalshard4>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
            }
        }
    }
}
