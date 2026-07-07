using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.LilHarpy
{
    public class BigEye : ModProjectile
    {
        public override void SetDefaults()
        {
            base.Projectile.width = 114;
            base.Projectile.height = 110;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.timeLeft = 200;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.minion = true;
            base.Projectile.localNPCHitCooldown = 15;
            base.Projectile.minionSlots = 0f;
            base.Projectile.netImportant = true;
            base.Projectile.usesLocalNPCImmunity = true;
        }
        private static int ETime;
        private static int ETimetoCharge;
        private static int Overtime;
        private static int EForm2;
        private static int EForm;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Overtime);
            writer.Write(ETime);
            writer.Write(ETimetoCharge);
            writer.Write(EForm2);
            writer.Write(EForm);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Overtime = (int)reader.ReadInt32();
            ETime = (int)reader.ReadInt32();
            ETimetoCharge = (int)reader.ReadInt32();
            EForm2 = (int)reader.ReadInt32();
            EForm = (int)reader.ReadInt32();
        }
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Psychic Turret");
            Main.projFrames[base.Projectile.type] = 6;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 20;
        }
        public override bool MinionContactDamage()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            NPC target = base.Projectile.Center.MinionHoming(500f, player);
            if (target != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (EForm <= 0)
            {
                EForm2++;
                Projectile.netUpdate = true;
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
            if (Overtime >= 1)
            {
                Overtime--;
                Projectile.netUpdate = true;
            }
            if (Overtime <= 0 && EForm > 1)
            {
                EForm = 0;
            }
            if (EForm2 >= 50)
            {
                EForm2 = 0;
                EForm++;
                Projectile.netUpdate = true;
            }
            if (EForm <= 1)
            {
                if (NPC.downedMoonlord)
                {
                    Projectile.damage = 500 + (player.statDefense / 2);
                }
                else if (NPC.downedPlantBoss)
                {
                    Projectile.damage = 200 + (player.statDefense / 2);
                }
                else if (Main.hardMode)
                {
                    Projectile.damage = 90 + (player.statDefense / 2);
                }
                else
                {
                    Projectile.damage = 10 + (player.statDefense / 2);
                }
                Projectile.netUpdate = true;
            }
            if (EForm >= 2)
            {
                if (NPC.downedMoonlord)
                {
                    Projectile.damage = ((int)((500 + (player.statDefense / 2)) * 1.5));
                }
                else if (NPC.downedPlantBoss)
                {
                    Projectile.damage = ((int)((200 + (player.statDefense / 2)) * 1.5));
                }
                else if (Main.hardMode)
                {
                    Projectile.damage = ((int)((90 + (player.statDefense / 2)) * 1.5));
                }
                else
                {
                    Projectile.damage = ((int)((10 + (player.statDefense / 2)) * 1.5));
                }
                Projectile.netUpdate = true;
            }
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<BigEyeBuff>());
                Projectile.Kill();
            }
            if (player.HasBuff(ModContent.BuffType<BigEyeBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            if (!player.HasBuff(ModContent.BuffType<BigEyeBuff>()))
            {
                Projectile.Kill();
            }
            float speed = 4f;
            float inertia = 20f;
            Vector2 idlePosition = player.Center;
            idlePosition.Y -= 148f; // Go up 48 coordinates (three tiles from the center of the player)
            // If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
            // The index is projectile.minionPos
            float minionPositionOffsetX = (20 + Projectile.minionPos * 40) * -player.direction;
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
            if (player.HasMinionAttackTargetNPC && EForm != 1)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);
                // Reasonable distance away so it doesn't target across multiple screens
                bool closeThroughWall = between < 2000f;
                if (between < 2000f && (closeThroughWall))
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }
            if (!foundTarget && EForm != 1)
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
                        bool closeThroughWall = between < 500f;
                        if (((closest && inRange) || !foundTarget) && (closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }
            {
                if (foundTarget && EForm != 1)
                {
                    {
                        Vector2 idlePosition3 = targetCenter;
                        idlePosition3.Y -= 130f;
                        idlePosition3.X -= 60;
                        speed = 30f;
                        inertia = 120f;
                        Vector2 vectorToIdlePosition3 = idlePosition3 - Projectile.Center;
                        float distanceToIdlePosition3 = vectorToIdlePosition3.Length();
                        Vector2 direction2 = idlePosition3 - Projectile.Center;
                        direction2.Normalize();
                        direction2 *= speed;
                        Projectile.velocity = (Projectile.velocity * (inertia - 8) + direction2) / inertia;
                        if (distanceToIdlePosition3 < 310)
                        {
                            ETimetoCharge++;
                        }
                    }
                    if (EForm > 0)
                    {
                        if (ETimetoCharge >= 30)
                        {
                            speed = 2110f;
                            inertia = 60f;
                            {
                                // The immediate range around the target (so it doesn't latch onto it when close)
                                Vector2 direction = targetCenter - Projectile.Center;
                                direction.Normalize();
                                direction *= speed;
                                Projectile.velocity = (Projectile.velocity * (inertia - 8) + direction) / inertia;
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Roar"), base.Projectile.Center);
                                ETimetoCharge = 0;
                            }
                        }
                    }
                    if (EForm <= 0)
                    {
                        if (ETimetoCharge >= 80)
                        {
                            speed = 2110f;
                            inertia = 60f;
                            {
                                // The immediate range around the target (so it doesn't latch onto it when close)
                                Vector2 direction = targetCenter - Projectile.Center;
                                direction.Normalize();
                                direction *= speed;
                                Projectile.velocity = (Projectile.velocity * (inertia - 8) + direction) / inertia;
                                SoundEngine.PlaySound(SoundID.Roar, base.Projectile.Center);
                                ETimetoCharge = 0;
                            }
                        }
                    }
                }
            }
            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            Vector2 idlePosition2 = player.Center;
            idlePosition2.Y -= 148f;
            idlePosition2.X += minionPositionOffsetX;
            // Default movement parameters (here for attacking)
            if (EForm != 1)
            {
                if (!foundTarget)
                {
                    Projectile.rotation = Projectile.AngleTo(player.Center) + (float)(base.Projectile.spriteDirection == 1).ToInt() * (float)Math.PI;
                }
                if (foundTarget)
                {
                    Projectile.rotation = Projectile.AngleTo(targetCenter) + (float)(base.Projectile.spriteDirection == 1).ToInt() * (float)Math.PI;
                }
                if (!foundTarget)
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
            }
            if (EForm == 1)
            {
                float intertia = .92f;
                Projectile.velocity = Projectile.velocity * intertia;
                if (Projectile.rotation <= 1)
                {
                    Projectile.rotation++;
                }
                else
                {
                    Projectile.rotation *= 1.012f;
                }
                ETime++;
            }
            if (ETime >= 400)
            {
                if (EForm < 2)
                {
                    EForm += 1;
                    ETime = 0;
                    Overtime = 3000;
                    Projectile.netUpdate = true;
                }
            }
            Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
            int frameSpeed = 10; //reduced by half due to framecounter speedup
            Projectile.frameCounter += 2;
            if (Projectile.frameCounter >= frameSpeed)
            {
                Projectile.frameCounter = 0;
                {
                    {
                        base.Projectile.frame++;
                        if (EForm <= 1)
                        {
                            if (base.Projectile.frameCounter >= 10)
                            {
                                base.Projectile.frameCounter = 0;
                            }
                            if (base.Projectile.frame >= 3)
                            {
                                base.Projectile.frame = 0;
                            }
                            Projectile.netUpdate = true;
                        }
                        if (EForm >= 2)
                        {
                            if (base.Projectile.frameCounter >= 10)
                            {
                                base.Projectile.frameCounter = 4;
                            }
                            if (base.Projectile.frame >= 6)
                            {
                                base.Projectile.frame = 4;
                            }
                            Projectile.netUpdate = true;
                        }
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Vector2 drawPosition;
            if (EForm <= 0)
            {
                for (int i = 1; i < 3; i++)
                {
                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<BigEye>()].Value;
                    Vector2 startPos = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
                    Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
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
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);
                }
            }
            if (EForm == 1)
            {
                for (int i = 1; i < 20; i++)
                {
                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<BigEye>()].Value;
                    Vector2 startPos = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
                    Color drawColor = Color.Lerp(lightColor, Color.PaleVioletRed, 20f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
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
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);
                }
            }
            if (EForm == 2)
            {
                for (int i = 1; i < 4; i++)
                {
                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<BigEye>()].Value;
                    Vector2 startPos = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
                    Color drawColor = Color.Lerp(lightColor, Color.DarkRed, 20f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
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
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);
                }
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<BigEye>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                int frameY = frameHeight * base.Projectile.frame;
                Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
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
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            return false;
        }
    }
}
