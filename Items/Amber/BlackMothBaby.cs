using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Amber
{
    public class BlackMothBaby : ModProjectile
    {
        public override void SetDefaults()
        {
            base.Projectile.width = 42;
            base.Projectile.height = 40;
            base.Projectile.ignoreWater = true;
            base.Projectile.friendly = true;
            base.Projectile.timeLeft = 700;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.minion = false;
            base.Projectile.localNPCHitCooldown = 15;
            base.Projectile.minionSlots = 0f;
            base.Projectile.netImportant = true;
            base.Projectile.usesLocalNPCImmunity = true;
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
                        bool closeThroughWall = between2 < 400f;
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
            if (player.dead || !player.active)
            {
                Projectile.Kill();
            }
            float speed = 30f;
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
            idlePosition2.Y -= 48f;
            idlePosition2.X += minionPositionOffsetX;
            // Default movement parameters (here for attacking)
            Projectile.rotation = Projectile.velocity.X * 0.05f;
            if (foundTarget)
            {
                speed = 30f;
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
                    speed = 40f;
                    inertia = 60f;
                }
                else if (distanceToIdlePosition > 50)
                {
                    // Slow down the minion if closer to the player
                    speed = 30f;
                    inertia = 80f;
                }
                else
                {
                    // Slow down the minion if closer to the player
                    speed = 4f;
                    inertia = 80f;
                }
                if (distanceToIdlePosition > 30f)
                {
                    // The immediate range around the player (when it passively floats about)
                    // This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 8) + vectorToIdlePosition) / inertia;
                }
                if (Projectile.velocity == Vector2.Zero)
                {
                    // If there is a case where it's not moving at all, give it a little "poke"
                    Projectile.velocity.X = -0.15f;
                    Projectile.velocity.Y = -0.15f;
                }
            }
            if (Projectile.velocity.X >= 0)
            {
                Projectile.spriteDirection = -1;
            }
            if (Projectile.velocity.X <= -0)
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
            target.AddBuff(BuffID.Ichor, 300);
            target.AddBuff(BuffID.Slow, 300);
            Projectile.timeLeft += 35;
            if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
            {
                if (Main.rand.NextBool(40))
                {
                    {
                        Item.NewItem(player.GetSource_FromThis(), (int)(player.position.X + 0), (int)(player.position.Y + 0), 0, 0, ModContent.ItemType<MothFood>());
                    }
                }
            }
            if (player.HasBuff(ModContent.BuffType<Overcharged>()))
            {
                if (Main.rand.NextBool(30))
                {
                    Item.NewItem(player.GetSource_FromThis(), (int)(player.position.X + 0), (int)(player.position.Y + 0), 0, 0, ModContent.ItemType<MothFood>());
                }
            }
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
