using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items
{
    public class PowerUp : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        private const int sphereRadius = 1;
        public override void SetDefaults()
        {
            base.Projectile.width = 20;
            base.Projectile.height = 20;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = false;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            Projectile.alpha = 300;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft = 100;
            base.Projectile.minion = true;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
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
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;
            target.AddBuff(ModContent.BuffType<Burning2>(), 200);
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float speed = 2;
            if (Main.rand.NextBool())
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y + 34) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Powerupdust>(), 0f, 0f, 0, default(Color), 1.5f);
            }//end of dust stuff
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            Vector2 idlePosition = player.Center;
            idlePosition.Y = 80f;
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();
            {
                // Minion has a target: attack (here, fly towards the enemy)
                Projectile.position.Y = player.position.Y + 10;
                Projectile.position.X = player.position.X;
            }
            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            Lighting.AddLight(Projectile.Center, Color.IndianRed.ToVector3() * 0.78f);
            // Default movement parameters (here for attacking)
        }
    }
}
