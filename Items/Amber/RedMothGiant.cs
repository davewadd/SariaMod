using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Items.Strange;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Amber
{
    public class RedMothGiant : ModProjectile
    {
        public override void SetDefaults()
        {
            base.Projectile.width = 62;
            base.Projectile.height = 50;
            base.Projectile.ignoreWater = true;
            base.Projectile.friendly = true;
            base.Projectile.timeLeft = 2000;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.minion = true;
            base.Projectile.localNPCHitCooldown = 15;
            base.Projectile.minionSlots = 0f;
            base.Projectile.netImportant = true;
            base.Projectile.usesLocalNPCImmunity = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X <= (float)((.2)) && Projectile.velocity.X >= (float)(-.2) && Projectile.velocity.Y <= (float)((.2)) && Projectile.velocity.Y >= (float)((-.2)))
            {
                Projectile.frame = 1;
            }
            return false;
        }
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Psychic Turret");
            Main.projFrames[base.Projectile.type] = 3;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
        }
        public override bool MinionContactDamage()
        {
            return true;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile.SariaBaseDamage();
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<Mothdust>()] > 0f))
            {
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(0f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Mothdust2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
            }
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<Mothdust2>()] > 0f))
            {
                Projectile.timeLeft += 40;
            }
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
                    targetCenter.Y -= 0f;
                    targetCenter.X += 0f;
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
                        float between2 = Vector2.Distance(npc.Center, player.Center);
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                        // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                        // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                        bool closeThroughWall = between2 < 450f;
                        if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            targetCenter.Y -= 0f;
                            targetCenter.X += 0f;
                            foundTarget = true;
                        }
                    }
                }
            }
            if (Projectile.velocity.X <= (float)((.4)) && Projectile.velocity.X >= (float)(-.4))
            {
                Projectile.tileCollide = true;
            }
            if (Projectile.velocity.X > (float)((.4)) || Projectile.velocity.X < (float)(-.4))
            {
                Projectile.tileCollide = false;
            }
            if (player.dead || !player.active)
            {
                Projectile.Kill();
            }
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] <= 0f))
            {
                Projectile.Kill();
            }
            if (Projectile.timeLeft == 2000)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath46, base.Projectile.Center);
            }
            float nothing = 1;
            float speed = 10f;
            float inertia = 10f;
            Vector2 idlePosition = player.Center;
            idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)
            // If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
            // The index is projectile.minionPos
            float minionPositionOffsetX = (10 + Projectile.minionPos * 40) * -player.direction;
            idlePosition.X += minionPositionOffsetX; // Go behind the player
            // All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)
            // Teleport to player if distance is too big
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();
            if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
            {
                // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                // and then set netUpdate to true
                Projectile.position = idlePosition;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }
            // If your minion is flying, you want to do this independently of any conditions
            float overlapVelocity = 0.04f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                // Fix overlap with other minions
                Projectile other = Main.projectile[i];
                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                    else Projectile.velocity.X += overlapVelocity;
                    if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
                    else Projectile.velocity.Y += overlapVelocity;
                }
            }
            Vector2 idlePosition2 = player.Center;
            idlePosition2.Y = 0f;
            idlePosition2.X += minionPositionOffsetX;
            // Default movement parameters (here for attacking)
            Projectile.rotation = Projectile.velocity.X * 0.05f;
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<RedHealthBar>()] <= 0f))
            {
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RedHealthBar>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
            }
            if (foundTarget)
            {
                base.Projectile.tileCollide = false;
                speed = 50f;
                inertia = 60f;
                {
                    // The immediate range around the target (so it doesn't latch onto it when close)
                    Vector2 direction = targetCenter - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 8) + direction) / inertia;
                }
            }
            if (!foundTarget)
            {
                // Minion doesn't have a target: return to player and idle
                if (distanceToIdlePosition > 450f)
                {
                    // Speed up the minion if it's away from the player
                    speed = 30f;
                    inertia = 60f;
                }
                if (distanceToIdlePosition > 400f)
                {
                    // Speed up the minion if it's away from the player
                    speed = 25f;
                    inertia = 60f;
                }
                if (distanceToIdlePosition > 100)
                {
                    // Slow down the minion if closer to the player
                    speed = 20f;
                    inertia = 80f;
                }
                if (distanceToIdlePosition > 10f)
                {
                    // The immediate range around the player (when it passively floats about)
                    // This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
                    inertia = 60f;
                    Projectile.velocity = (Projectile.velocity * (inertia - 8) + vectorToIdlePosition) / inertia;
                    if (Projectile.velocity.X <= (float)((.5)) && Projectile.velocity.X >= (float)(-.5))
                    {
                        base.Projectile.velocity.Y = (float)((.5));
                    }
                }
            }
            if (Projectile.velocity.X >= 0)
            {
                Projectile.spriteDirection = -1;
            }
            if (Projectile.velocity.X < 0)
            {
                Projectile.spriteDirection = 1;
            }
            Lighting.AddLight(Projectile.Center, Color.MediumPurple.ToVector3() * 1f);
            int frameSpeed = 10; //reduced by half due to framecounter speedup
            Projectile.frameCounter += 2;
            if (Projectile.frameCounter >= frameSpeed)
            {
                Projectile.frameCounter = 0;
                {
                    base.Projectile.frame++;
                    if (base.Projectile.frameCounter >= 3)
                    {
                        base.Projectile.frameCounter = 0;
                    }
                    if (base.Projectile.frame >= 3)
                    {
                        base.Projectile.frame = 0;
                    }
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
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;
            target.AddBuff(ModContent.BuffType<Burning2>(), 200);
            Projectile.timeLeft += 150;
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                damage += (damage) / 4;
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                damage /= 7;
            }
            damage /= 5;
        }
    }
}
