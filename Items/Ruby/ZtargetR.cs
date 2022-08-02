using Microsoft.Xna.Framework;

using Terraria;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    public class ZtargetR : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Blade");
            ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
            Main.projFrames[base.projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            base.projectile.width = 80;
            base.projectile.height = 80;

            base.projectile.alpha = 0;
            base.projectile.friendly = true;
            base.projectile.tileCollide = false;

            base.projectile.penetrate = 1;
            base.projectile.timeLeft = 500;
            base.projectile.ignoreWater = true;

            base.projectile.usesLocalNPCImmunity = true;
            base.projectile.localNPCHitCooldown = 4;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[base.projectile.owner];
            Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
            projectile.scale = (float)0.7;
            base.projectile.rotation += (float)0.07;

            FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
            {
                float distanceFromTarget = 10f;
                Vector2 targetCenter = projectile.position;
                bool foundTarget = false;

                // This code is required if your minion weapon has the targeting feature
                if (player.HasMinionAttackTargetNPC)
                {
                    NPC npc = Main.npc[player.MinionAttackTargetNPC];
                    float between = Vector2.Distance(npc.Center, projectile.Center);
                    // Reasonable distance away so it doesn't target across multiple screens
                    if (between < 2000f)
                    {
                        distanceFromTarget = between;
                        targetCenter = npc.Center;
                        targetCenter.Y -= 0f;
                        targetCenter.X += 0f;
                        foundTarget = true;
                    }
                }

                if (!mother.active || mother.type != ModContent.ProjectileType<RubySariaMinion>())
                {

                    base.projectile.Kill();
                    return;
                }
                if (mother.active && projectile.timeLeft <= 10)
                {
                    projectile.timeLeft = 20;
                }

                if (player.HasMinionAttackTargetNPC && projectile.alpha >= 200)
                {
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/ZtargetEnemy"), base.projectile.Center);
                    projectile.alpha = 0;
                    projectile.scale = (float).7;
                }
                if (!player.HasMinionAttackTargetNPC && projectile.alpha <= 100 )
                {
                    if (projectile.timeLeft <= 490)
                    {
                        Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/ZtargetCancel"), base.projectile.Center);
                    }
                    projectile.alpha = 300;
                }




                projectile.friendly = foundTarget;
                Lighting.AddLight(projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 1f);
                // Default movement parameters (here for attacking)

                float inertia = 13f;
                Vector2 idlePosition = player.Center;
                float minionPositionOffsetX = ((60 + projectile.minionPos / 80) * player.direction) - 15;
                idlePosition.Y -= 70f;
                idlePosition.X += minionPositionOffsetX;
                Vector2 vectorToIdlePosition = idlePosition - projectile.Center;

                float distanceToIdlePosition = vectorToIdlePosition.Length();
                if (player.HasMinionAttackTargetNPC)
                {
                    // The immediate range around the target (so it doesn't latch onto it when close)

                    projectile.Center = targetCenter;

                }
                if (!foundTarget)
                {

                    if ((distanceToIdlePosition >= 2000))
                    {
                        projectile.position = mother.Center;
                    }


                    {

                        inertia = 10;
                        Vector2 direction2 = idlePosition - projectile.Center;

                        projectile.velocity = (projectile.velocity * (inertia - 8) + direction2) / 20;
                    }
                }
                else if (projectile.velocity == Vector2.Zero)
                {
                    // If there is a case where it's not moving at all, give it a little "poke"
                    projectile.velocity.X = -0.15f;
                    projectile.velocity.Y = -0.05f;
                }

            }

        }





        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.projectile.owner];
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
            target.AddBuff(BuffID.OnFire, 300);
            target.AddBuff(BuffID.Slow, 300);

            damage /= damage / 4;

        }



    }
}
