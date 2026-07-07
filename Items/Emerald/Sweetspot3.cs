using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using SariaMod.Dusts;
namespace SariaMod.Items.Emerald
{
    public class Sweetspot3 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 100;
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
            base.Projectile.timeLeft = 500;
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
            target.buffImmune[ModContent.BuffType<MeteorSpikeDebuff>()] = false;
            target.AddBuff(ModContent.BuffType<MeteorSpikeDebuff>(), 40);
            damage /= 2;
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center.X, target.Center.Y, 0, 0, ModContent.ProjectileType<HitCheckSound>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
            Projectile.timeLeft = 200;
            int owner = player.whoAmI;
            int GiantMoth = ModContent.ProjectileType<Emeraldspike3>();
            for (int i = 0; i < 1000; i++)
            {
                {
                    if (Main.projectile[i].active && i != Projectile.whoAmI && ((Main.projectile[i].type == GiantMoth && Main.projectile[i].owner == owner)))
                    {
                        Vector2 SpikeHitBox = Main.projectile[i].Center;
                        SpikeHitBox.Y += 150;
                        {
                            Projectile.Center = SpikeHitBox;
                        }
                    }
                }
            }
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && player.immune == false && Main.projectile[i].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
                {
                    Main.projectile[i].Kill();
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.position.X + 0, player.position.Y + 0, 0, 0, ModContent.ProjectileType<HitCheck3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                }
            }
            Vector2 DotMatch2 = player.position;
            DotMatch2.Y += 1;
            bool CanSee2 = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, DotMatch2, 1, 1);
            if (player.velocity.Y <= 0 || player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3>()] < 1f)
            {
                if (Main.myPlayer == Projectile.owner && !CanSee2) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X, player.Center.Y + 20, 0, 0, ModContent.ProjectileType<LaunchHitBox>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                Projectile.Kill();
            }
            Vector2 DotMatch = Projectile.position;
            DotMatch.Y -= 100;
            bool CanSee = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, DotMatch, 1, 1);
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                {
                    if (npc.HasBuff(ModContent.BuffType<MeteorSpikeDebuff>()) && CanSee)
                    {
                        if (npc.velocity.Y < player.velocity.Y)
                        {
                            player.immuneTime = 30;
                            player.immune = true;
                            player.immuneNoBlink = true;
                            npc.position.Y = (player.position.Y + 50);
                        }
                    }
                }
            }
        }
    }
}
