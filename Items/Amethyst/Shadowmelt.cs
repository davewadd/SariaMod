using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Amethyst
{
    public class Shadowmelt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 10;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 30;
            base.Projectile.height = 30;
            base.Projectile.aiStyle = 1;
            base.Projectile.alpha = 0;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = 1;
            base.Projectile.timeLeft = 200;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }
        private const int sphereRadius = 3;
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Lighting.AddLight(base.Projectile.Center, 0f, 0.5f, 0f);
            FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
            if (Main.rand.NextBool())
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Shadow2>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            if (Projectile.timeLeft == 198)
            {
                SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost, base.Projectile.Center);
            }
            {
                float distanceFromTarget = 10f;
                Vector2 targetCenter = Projectile.position;
                bool foundTarget = false;
                // This code is required if your minion weapon has the targeting feature
                if (player.HasMinionAttackTargetNPC)
                {
                    NPC npc = Main.npc[player.MinionAttackTargetNPC];
                    float between = Vector2.Distance(npc.Center, Projectile.Center);
                    // Reasonable distance away so it doesn't target across multiple screens
                    if (between < 2000f)
                    {
                        distanceFromTarget = between;
                        targetCenter = npc.Center;
                        if (Projectile.timeLeft >= 90)
                        {
                            targetCenter.Y += 50;
                        }
                        foundTarget = true;
                    }
                }
                if (!foundTarget)
                {
                    // This code is required either way, used for finding a target
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.CanBeChasedBy())
                        {
                            float between = Vector2.Distance(npc.Center, player.Center);
                            bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                            bool inRange = between < distanceFromTarget;
                            // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                            // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                            bool closeThroughWall = between < 1000f;
                            if (((closest && inRange) || !foundTarget) && (closeThroughWall))
                            {
                                distanceFromTarget = between;
                                targetCenter = npc.Center;
                                if (Projectile.timeLeft >= 90)
                                {
                                    targetCenter.Y += 50;
                                }
                                foundTarget = true;
                            }
                        }
                    }
                }
                // friendly needs to be set to true so the minion can deal contact damage
                // friendly needs to be set to false so it doesn't damage things like target dummies while idling
                // Both things depend on if it has a target or not, so it's just one assignment here
                // You don't need this assignment if your minion is shooting things instead of dealing contact damage
                Projectile.friendly = foundTarget;
                float speed = 70f;
                float inertia = 20f;
                Lighting.AddLight(Projectile.Center, Color.LightPink.ToVector3() * 0.78f);
                // Default movement parameters (here for attacking)
                if (Projectile.timeLeft >= 120)
                {
                    speed = 10f;
                }
                if (Projectile.timeLeft < 120)
                {
                    speed = 70f;
                }
                if (distanceFromTarget > 40f && Projectile.timeLeft <= 400)
                {
                    // The immediate range around the target (so it doesn't latch onto it when close)
                    Vector2 direction = targetCenter - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction) / inertia;
                }
                base.Projectile.frameCounter++;
                if (base.Projectile.frameCounter > 3)
                {
                    base.Projectile.frame++;
                    base.Projectile.frameCounter = 0;
                }
                if (Math.Abs(Projectile.velocity.X) >= 3)
                {
                    if (base.Projectile.frame >= 8)
                    {
                        base.Projectile.frame = 0;
                    }
                }
                else if (Math.Abs(Projectile.velocity.X) < 3)
                {
                    if (base.Projectile.frame >= 10)
                    {
                        base.Projectile.frame = 8;
                    }
                    if (base.Projectile.frame < 8)
                    {
                        base.Projectile.frame = 8;
                    }
                }
            }
            if (Projectile.timeLeft >= 200)
            {
                SoundEngine.PlaySound(SoundID.Item69, base.Projectile.Center);
            }
            if (Projectile.timeLeft == 1)
            {
                for (int j = 0; j < 1; j++) //set to 2
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(30f, -80f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<ShadowSneak>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
            }
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
            target.buffImmune[ModContent.BuffType<SariaCurse>()] = false;
            target.AddBuff(ModContent.BuffType<SariaCurse>(), 2000);
            modPlayer.SariaXp++;
            {
                SoundEngine.PlaySound(SoundID.DD2_LightningBugDeath, base.Projectile.Center);
            }
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(4f, -100f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<ShadowSneak>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                damage += (damage) / 4;
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                damage /= 2;
            }
        }
    }
}
