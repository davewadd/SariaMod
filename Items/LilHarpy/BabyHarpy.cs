using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Items.Strange;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.LilHarpy
{
    public class BabyHarpy : ModProjectile
    {
        public override void SetDefaults()
        {
            base.Projectile.width = 42;
            base.Projectile.height = 40;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.timeLeft = 200;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.minion = true;
            base.Projectile.localNPCHitCooldown = 20;
            base.Projectile.minionSlots = 0f;
            base.Projectile.netImportant = true;
            base.Projectile.usesLocalNPCImmunity = true;
        }
        private int HTransform;
        private int HTimer;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(HTransform);
            writer.Write(HTimer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            HTransform = (int)reader.ReadInt32();
            HTimer = (int)reader.ReadInt32();
        }
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Psychic Turret");
            Main.projFrames[base.Projectile.type] = 10;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
        }
        public override bool MinionContactDamage()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            NPC target = base.Projectile.Center.MinionHoming(500f, player);
            if (target != null)
            {
                return false;
            }
            else
            {
                return false;
            }
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<BigEye>()] > 1f)
            {
                Projectile.Kill();
            }
            Projectile.SariaBaseDamage();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0f)
            {
                HTransform = 1;
                Projectile.netUpdate = true;
            }
            else
            {
                HTransform = 0;
                Projectile.netUpdate = true;
            }
            if (NPC.downedMoonlord)
            {
                Projectile.damage = 100 * player.maxMinions;
            }
            else if (NPC.downedPlantBoss)
            {
                Projectile.damage = 75 * player.maxMinions;
            }
            else if (Main.hardMode)
            {
                Projectile.damage = 40 * player.maxMinions;
            }
            else
            {
                Projectile.damage = 10 * player.maxMinions;
            }
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<BabyHarpyBuff>());
                Projectile.Kill();
            }
            if (player.HasBuff(ModContent.BuffType<BabyHarpyBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            if (!player.HasBuff(ModContent.BuffType<BabyHarpyBuff>()))
            {
                Projectile.Kill();
            }
            float speed = 8f;
            float inertia = 20f;
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
            // Starting search distance
            float distanceFromTarget = 10f;
            Vector2 targetCenter = Projectile.position;
            bool foundTarget = false;
            // This code is required if your minion weapon has the targeting feature
            if (player.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);
                bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                // Reasonable distance away so it doesn't target across multiple screens
                bool closeThroughWall = between < 320f;
                if (between < 2000f && (lineOfSight || closeThroughWall))
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
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
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                        // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                        // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                        bool closeThroughWall = between < 260f;
                        if (((between < 500) && (lineOfSight || closeThroughWall)))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }
            int attacktimer = 120;
            if (NPC.downedMoonlord)
            {
                attacktimer = 60;
            }
            else if (NPC.downedPlantBoss)
            {
                attacktimer = 80;
            }
            else if (Main.hardMode)
            {
                attacktimer = 95;
            }
            else if (NPC.downedBoss1)
            {
                attacktimer = 105;
            }
            HTimer++;
            if (HTimer >= attacktimer && foundTarget && (player.ownedProjectileCounts[ModContent.ProjectileType<Feather>()] <= 0f))
            {
                if (HTransform == 0 && Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Feather>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                else if (HTransform == 1 && Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Feather2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                HTimer = 0;
            }
            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            Vector2 idlePosition2 = player.Center;
            idlePosition2.Y -= 48f;
            idlePosition2.X += minionPositionOffsetX;
            // Default movement parameters (here for attacking)
            Projectile.rotation = Projectile.velocity.X * 0.05f;
            {
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
                        speed = 8f;
                        inertia = 60f;
                    }
                    else
                    {
                        // Slow down the minion if closer to the player
                        speed = 4f;
                        inertia = 80f;
                    }
                    if (distanceToIdlePosition > 20f)
                    {
                        // The immediate range around the player (when it passively floats about)
                        // This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
                        vectorToIdlePosition.Normalize();
                        vectorToIdlePosition *= speed;
                        Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
                    }
                    else if (Projectile.velocity == Vector2.Zero)
                    {
                        // If there is a case where it's not moving at all, give it a little "poke"
                        Projectile.velocity.X = -0.15f;
                        Projectile.velocity.Y = -0.15f;
                    }
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
            Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
            int frameSpeed = 10; //reduced by half due to framecounter speedup
            Projectile.frameCounter += 2;
            if (Projectile.frameCounter >= frameSpeed)
            {
                Projectile.frameCounter = 0;
                if (distanceToIdlePosition >= 400)
                {
                    Projectile.frame++;
                    if (base.Projectile.frameCounter < 10)
                    {
                        base.Projectile.frameCounter = 0;
                    }
                    if (base.Projectile.frame >= 10)
                    {
                        base.Projectile.frame = 7;
                    }
                    if (base.Projectile.frame < 7)
                    {
                        base.Projectile.frame = 7;
                    }
                }
                else if (distanceToIdlePosition < 400)
                {
                    base.Projectile.frame++;
                    if (base.Projectile.frameCounter >= 10)
                    {
                        base.Projectile.frameCounter = 0;
                    }
                    if (base.Projectile.frame >= 6)
                    {
                        base.Projectile.frame = 0;
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[base.Projectile.owner];
            {
                Vector2 drawPosition;
                if (HTransform == 0)
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/LilHarpy/BabyHarpy");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.Projectile.rotation;
                    float scale = base.Projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.Projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                }
                else if (HTransform == 1)
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/LilHarpy/BabyHarpy2");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.Projectile.rotation;
                    float scale = base.Projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.Projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                }
                return false;
            }
        }
    }
}
