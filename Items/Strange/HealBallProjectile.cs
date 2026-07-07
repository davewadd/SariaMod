using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class HealBallProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 16;
            base.Projectile.height = 16;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = false;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            Projectile.aiStyle = 14;
            base.Projectile.penetrate = 2;
            base.Projectile.tileCollide = true;
            base.Projectile.timeLeft = 300;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return true;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            {
                if ((Math.Abs(oldVelocity.X) > (Math.Abs(Projectile.velocity.X)) * 2))
                {
                    base.Projectile.velocity.X = -1 * (oldVelocity.X * .6f);
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Pokebounce"), Projectile.Center);
                }
                else
                {
                    base.Projectile.velocity.X *= .8f;
                }
            }
            {
                base.Projectile.velocity.Y = 0f - (oldVelocity.Y * .6f);
            }
            if (Math.Abs(Projectile.oldVelocity.Y) >= 1f)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Pokebounce"), Projectile.Center);
            }
            return false;
        }
        private const int sphereRadius = 3;
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Lighting.AddLight(Projectile.Center, Color.LightPink.ToVector3() * 1f);
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[base.Projectile.owner];
            bool HoldingHealBall = player.HeldItem.type == ModContent.ItemType<HealBall>();
            bool HoldingHealBallInInventory = player.HasItem(ModContent.ItemType<HealBall>());
            for (int j = 0; j < 72; j++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 113);
                dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                dust.noGravity = true;
                dust.scale *= 3.9f;
            }
            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Pokeball"), Projectile.Center);
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] <= 0f) && (player.maxMinions >= 3) && (HoldingHealBallInInventory || HoldingHealBall))
            {
                player.AddBuff(ModContent.BuffType<SariaBuff>(), 30000);
                player.AddBuff(ModContent.BuffType<XPBuff>(), 500);
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Saria>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<ReturnBall>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            else
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<ReturnBall>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
        }
    }
}
