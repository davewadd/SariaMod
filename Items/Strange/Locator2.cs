using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public class Locator2 : ModProjectile
    {
        private class GreenLightTriangle
        {
            public Vector2 Position;
            public float Rotation;
            public float Size;
            public float Age;
            public float Lifetime;
        }

        private class PulsingSparkle
        {
            public Vector2 Position;
            public float Age;
            public float Lifetime;
            public float SpawnSize; // Original size it was created at
        }

        private List<GreenLightTriangle> greenTriangles = new List<GreenLightTriangle>();
        private List<PulsingSparkle> sparkles = new List<PulsingSparkle>();
        private int triangleSpawnTimer;
        private int sparkleSpawnTimer;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 100;
        }

        public override void SetDefaults()
        {
            base.Projectile.width = 100;
            base.Projectile.height = 100;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 100;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft = 500;
            base.Projectile.minion = true;
        }

        private int HitCount;

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(HitCount);
            writer.WriteVector2(new Vector2(Projectile.localAI[0], Projectile.localAI[1]));
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            HitCount = (int)reader.ReadInt32();
            Vector2 cachedCursor = reader.ReadVector2();
            Projectile.localAI[0] = cachedCursor.X;
            Projectile.localAI[1] = cachedCursor.Y;
        }

        public override bool MinionContactDamage()
        {
            return true;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return base.Projectile.timeLeft < 400 && target.CanBeChasedBy(base.Projectile);
        }

        private const int sphereRadius = 3;

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
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
            target.AddBuff(BuffID.Slow, 300);
            target.AddBuff(ModContent.BuffType<SariaCurse2>(), 50);
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<LocatorShard>()] < 60f)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), base.Projectile.Center + Utils.RandomVector2(Main.rand, 0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<LocatorShard>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    }
                }
                Projectile.Kill();
            }

            // Spawn hit VFX at target center
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity, ModContent.ProjectileType<LocatorHitVisual>(), 0, 0f, Projectile.owner);

            if (HitCount <= 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath, base.Projectile.Center);
            }
            if (HitCount == 1)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/AttackHit"), Projectile.Center);
            }
            if (HitCount >= 2)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/CriticalHit"), Projectile.Center);
            }
            if (player.HasBuff(ModContent.BuffType<Overcharged>()) && player.statMana >= 3)
            {
                player.statMana -= 3;
                player.manaRegenDelay = 30;
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), base.Projectile.Center + Utils.RandomVector2(Main.rand, 0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<LocatorSmall>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.SariaXp++;
            HitCount++;
            knockback /= 4;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();

            // Find Saria (mother) by type and owner instead of cached index
            Projectile mother = null;
            int ownerID = player.whoAmI;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Saria && Main.projectile[i].owner == ownerID)
                {
                    mother = Main.projectile[i];
                    break;
                }
            }

            // Kill safely if mother disappeared
            if (mother == null || !mother.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.knockBack = 10;

            // Calculate projectile speed for spawn rate scaling
            float speed = Projectile.velocity.Length();

            // Spawn colored dust triangles along trail - spawn less when moving slower
            triangleSpawnTimer++;
            int spawnThreshold = speed < 20f ? 8 : 5; // Slower = less frequent
            
            if (triangleSpawnTimer >= spawnThreshold && Projectile.oldPos.Length > 0)
            {
                triangleSpawnTimer = 0;
                
                // Pick random position along trail
                int randomTrailIndex = Main.rand.Next(Projectile.oldPos.Length);
                Vector2 trailPos = Projectile.oldPos[randomTrailIndex] + Projectile.Size * 0.5f;
                
                if (trailPos != Vector2.Zero)
                {
                    float randomSize = Main.rand.NextFloat(2f, 5f);
                    float randomRotation = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                    
                    // Randomly cycle between green, cyan, and purple
                    Color[] colors = { Color.LimeGreen, Color.Cyan, new Color(200, 100, 255) }; // purple
                    Color triangleColor = colors[Main.rand.Next(colors.Length)];

                    Vector2 point1 = new Vector2(
                        (float)Math.Cos(randomRotation) * randomSize,
                        (float)Math.Sin(randomRotation) * randomSize
                    );
                    Vector2 point2 = new Vector2(
                        (float)Math.Cos(randomRotation + MathHelper.TwoPi / 3f) * randomSize,
                        (float)Math.Sin(randomRotation + MathHelper.TwoPi / 3f) * randomSize
                    );
                    Vector2 point3 = new Vector2(
                        (float)Math.Cos(randomRotation + MathHelper.TwoPi * 2f / 3f) * randomSize,
                        (float)Math.Sin(randomRotation + MathHelper.TwoPi * 2f / 3f) * randomSize
                    );

                    for (int edge = 0; edge < 3; edge++)
                    {
                        float t = edge / 3f;

                        Vector2 edgePos1 = Vector2.Lerp(point1, point2, t);
                        Dust d1 = Dust.NewDustPerfect(trailPos + edgePos1, 267, Projectile.velocity * 0.15f, 0, triangleColor, 0.8f);
                        d1.noGravity = true;
                        d1.fadeIn = 1f;

                        Vector2 edgePos2 = Vector2.Lerp(point2, point3, t);
                        Dust d2 = Dust.NewDustPerfect(trailPos + edgePos2, 267, Projectile.velocity * 0.15f, 0, triangleColor, 0.8f);
                        d2.noGravity = true;
                        d2.fadeIn = 1f;

                        Vector2 edgePos3 = Vector2.Lerp(point3, point1, t);
                        Dust d3 = Dust.NewDustPerfect(trailPos + edgePos3, 267, Projectile.velocity * 0.15f, 0, triangleColor, 0.8f);
                        d3.noGravity = true;
                        d3.fadeIn = 1f;
                    }
                }
            }

            // Spawn white pulsing sparkles from outer hexagon
            sparkleSpawnTimer++;
            int sparkleSpawnChance = speed < 30f ? 10 : (speed < 50f ? 6 : 4);
            
            if (sparkleSpawnTimer >= sparkleSpawnChance)
            {
                sparkleSpawnTimer = 0;
                
                // Spawn from outer hexagon ring (radius 16)
                float hexRadius = 16f * Projectile.scale;
                float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Vector2 sparklePos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * hexRadius;
                
                sparkles.Add(new PulsingSparkle
                {
                    Position = sparklePos,
                    Age = 0f,
                    Lifetime = 2f, // 2 seconds for pulse cycle
                    SpawnSize = 0.8f // Tiny size
                });
            }

            // Update sparkles
            for (int i = sparkles.Count - 1; i >= 0; i--)
            {
                sparkles[i].Age += 1f / 60f; // Convert to seconds
                
                if (sparkles[i].Age >= sparkles[i].Lifetime)
                {
                    sparkles.RemoveAt(i);
                }
            }

            // Spawn Locator-style triangle dust only after leaving Saria's center (timeLeft <= 400)
            if (Projectile.timeLeft <= 400 && Main.rand.NextBool(8))
            {
                float randomSize = Main.rand.NextFloat(10f, 25f);
                float randomRotation = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Vector2 randomOffset = Main.rand.NextVector2Circular(15f, 15f);
                
                bool useCyan = Main.rand.NextBool(3);
                Color triangleColor1 = useCyan ? Color.DarkCyan : Color.DeepPink;
                Color triangleColor2 = useCyan ? Color.Cyan : Color.HotPink;
                
                Vector2 point1 = new Vector2(
                    (float)Math.Cos(randomRotation) * randomSize,
                    (float)Math.Sin(randomRotation) * randomSize
                );
                Vector2 point2 = new Vector2(
                    (float)Math.Cos(randomRotation + MathHelper.TwoPi / 3f) * randomSize,
                    (float)Math.Sin(randomRotation + MathHelper.TwoPi / 3f) * randomSize
                );
                Vector2 point3 = new Vector2(
                    (float)Math.Cos(randomRotation + MathHelper.TwoPi * 2f / 3f) * randomSize,
                    (float)Math.Sin(randomRotation + MathHelper.TwoPi * 2f / 3f) * randomSize
                );
                
                for (int edge = 0; edge < 10; edge++)
                {
                    float t = edge / 10f;
                    
                    Vector2 edgePos1 = Vector2.Lerp(point1, point2, t);
                    Dust d1 = Dust.NewDustPerfect(Projectile.Center + edgePos1 + randomOffset, 267, Projectile.velocity * 0.3f, 0, triangleColor1, 1.5f);
                    d1.noGravity = true;
                    d1.fadeIn = 2.0f;
                    d1.color = Color.Lerp(triangleColor1, triangleColor2, t);
                    
                    Vector2 edgePos2 = Vector2.Lerp(point2, point3, t);
                    Dust d2 = Dust.NewDustPerfect(Projectile.Center + edgePos2 + randomOffset, 267, Projectile.velocity * 0.3f, 0, triangleColor1, 1.5f);
                    d2.noGravity = true;
                    d2.fadeIn = 2.0f;
                    d2.color = Color.Lerp(triangleColor1, triangleColor2, t);
                    
                    Vector2 edgePos3 = Vector2.Lerp(point3, point1, t);
                    Dust d3 = Dust.NewDustPerfect(Projectile.Center + edgePos3 + randomOffset, 267, Projectile.velocity * 0.3f, 0, triangleColor1, 1.5f);
                    d3.noGravity = true;
                    d3.fadeIn = 2.0f;
                    d3.color = Color.Lerp(triangleColor1, triangleColor2, t);
                }
            }

            // Spawn Locator-style lightning dust only after leaving Saria's center (timeLeft <= 400)
            if (Projectile.timeLeft <= 400 && Main.rand.NextBool(12))
            {
                float lightningSize = Main.rand.NextFloat(8f, 18f);
                Vector2 startPos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                Vector2 currentPos = startPos;
                
                int segments = Main.rand.Next(4, 8);
                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.8f, 0.8f);
                    Vector2 nextPos = currentPos + new Vector2(
                        (float)Math.Cos(angle) * lightningSize,
                        (float)Math.Sin(angle) * lightningSize
                    );
                    
                    int points = 3;
                    for (int p = 0; p <= points; p++)
                    {
                        float t = p / (float)points;
                        Vector2 boltPos = Vector2.Lerp(currentPos, nextPos, t);
                        
                        float fade = 1f - (seg / (float)segments);
                        Color purpleColor = Color.Lerp(Color.DarkViolet, Color.MediumPurple, t);
                        
                        Dust lightning = Dust.NewDustPerfect(
                            boltPos,
                            267,
                            Projectile.velocity * 0.25f,
                            0,
                            purpleColor,
                            Main.rand.NextFloat(1.0f, 1.6f)
                        );
                        lightning.noGravity = true;
                        lightning.fadeIn = 1.5f;
                        lightning.color = purpleColor * fade;
                    }
                    
                    currentPos = nextPos;
                }
            }

            if (Projectile.timeLeft == 500)
            {
                SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, base.Projectile.Center);
            }
            Projectile.SariaBaseDamage();
            Projectile.damage /= 3;

            float baseSpeed = 70f;
            float inertia = 20f;

            // Orbit parameters
            float tOrbit = Main.GlobalTimeWrappedHourly;
            Vector2 orbitAnchor = mother.Center + new Vector2(0f, -80f);

            // index among Locator2 for spacing
            int myIndex = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == Projectile.type && i < Projectile.whoAmI)
                    myIndex++;
            }

            Vector2 separation = Vector2.Zero;
            const float sepRange = 64f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != Projectile.owner || p.type != Projectile.type || p.whoAmI == Projectile.whoAmI)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, p.Center);
                if (dist > 0.01f && dist < sepRange)
                    separation += (Projectile.Center - p.Center) * (0.8f / dist);
            }

            float phase = tOrbit * 2.2f + myIndex * 0.9f;
            Vector2 orbitOffset = new Vector2((float)Math.Cos(phase) * 70f, (float)Math.Sin(phase * 1.6f) * 26f);
            Vector2 desiredOrbitPos = orbitAnchor + orbitOffset;

            // "mouse" target used for straight shot
            Vector2 cursor = Main.MouseWorld;

            // Phase 1 (timeLeft > 400): circle above Saria instead of going to mouse
            if (Projectile.timeLeft > 400)
            {
                Vector2 to = (desiredOrbitPos + separation) - Projectile.Center;
                Vector2 desiredVel = to.SafeNormalize(Vector2.Zero) * 18f;
                Projectile.velocity = (Projectile.velocity * (14f - 1f) + desiredVel) / 14f;

                Projectile.rotation += 0.095f;
                return;
            }

            // On the frame we switch into the attack phase, dip to center then start straight shot
            if (Projectile.timeLeft == 400)
            {
                Projectile.Center = mother.Center;
                Projectile.velocity = Vector2.Zero;

                // Only the owner caches the aim target
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 aimTarget = cursor;

                    // In LinkCable mode with no ZtargetReal, aim at the closest enemy to Saria
                    if (mother.ModProjectile is Saria sariaMP && sariaMP.LinkCableFollowActive)
                    {
                        bool hasZtarget = false;
                        int ztargetType = ModContent.ProjectileType<ZtargetReal>();
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].type == ztargetType)
                            {
                                hasZtarget = true;
                                break;
                            }
                        }
                        if (!hasZtarget)
                        {
                            float closestDist = float.MaxValue;
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                NPC npc = Main.npc[i];
                                if (!npc.CanBeChasedBy()) continue;
                                float dist = Vector2.Distance(npc.Center, mother.Center);
                                if (dist < closestDist)
                                {
                                    closestDist = dist;
                                    aimTarget = npc.Center;
                                }
                            }
                        }
                    }

                    Projectile.localAI[0] = aimTarget.X;
                    Projectile.localAI[1] = aimTarget.Y;
                    Projectile.netUpdate = true;
                }
            }

            // Phase 2 (400..?): fire straight from Saria center toward cached cursor
            if (Projectile.timeLeft <= 400 && Projectile.timeLeft > 300)
            {
                Vector2 target = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
                Vector2 dir = (target - mother.Center).SafeNormalize(Vector2.UnitX);

                // Accelerate like normal locator
                float lerp = (400f - Projectile.timeLeft) / 100f;
                float s = MathHelper.Lerp(18f, baseSpeed, MathHelper.Clamp(lerp, 0f, 1f));
                Projectile.velocity = dir * s;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                return;
            }

            // Phase 3 (after the "fall" time): orbit above Saria again with separation
            {
                Vector2 to = (desiredOrbitPos + separation) - Projectile.Center;
                Vector2 desiredVel = to.SafeNormalize(Vector2.Zero) * 18f;
                Projectile.velocity = (Projectile.velocity * (14f - 1f) + desiredVel) / 14f;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                // still allow hitting only when in the normal attack window
                if (Projectile.timeLeft < 400)
                    Projectile.aiStyle = 1;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D trailTexture = TextureAssets.MagicPixel.Value;
            Vector2 centerOffset = Projectile.Size * 0.5f;

            if (Projectile.timeLeft >= 495)
            {
                DrawHexagonHeadWithSparks();
                return false;
            }

            // Draw trail
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 p0 = Projectile.oldPos[i - 1];
                Vector2 p1 = Projectile.oldPos[i];

                if (p0 == Vector2.Zero || p1 == Vector2.Zero)
                {
                    break;
                }

                float distFromCurrent = Vector2.Distance(p0 + centerOffset, Projectile.Center);
                if (distFromCurrent > 5000f)
                {
                    break;
                }

                p0 += centerOffset;
                p1 += centerOffset;

                Vector2 diff = p0 - p1;
                float len = diff.Length();
                if (len <= 0.001f)
                {
                    continue;
                }

                float t = i / (float)Projectile.oldPos.Length;
                float inverseLerp = 1f - t;
                float frontFlatten = (float)Math.Pow(inverseLerp, 2.5);

                float width = MathHelper.Lerp(12f, 2f, t) * Projectile.scale;
                width *= MathHelper.Lerp(frontFlatten, 1f, t * 0.15f);

                float alpha = MathHelper.Lerp(0.9f, 0f, t);

                Color c = Color.Lerp(Color.DeepPink, Color.HotPink, t * 0.5f);
                c = Color.Lerp(c, Color.LightPink, t);
                c = Color.Lerp(c, Color.Transparent, t);
                c *= alpha;

                float rotation = diff.ToRotation();
                Vector2 drawPos = (p1 + p0) * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Vector2 scale = new Vector2(len, width);

                Main.spriteBatch.Draw(
                    trailTexture,
                    drawPos,
                    new Rectangle(0, 0, 1, 1),
                    Projectile.GetAlpha(c),
                    rotation,
                    new Vector2(0.5f, 0.5f),
                    scale,
                    SpriteEffects.None,
                    0f);

                Lighting.AddLight((p1 + p0) * 0.5f, Color.DeepPink.ToVector3() * (0.4f * alpha));

                // Draw connecting segments to smooth out heavy rotation
                if (i > 1)
                {
                    Vector2 p_prev = Projectile.oldPos[i - 2] + centerOffset;
                    Vector2 connector = p1 - p_prev;
                    float connLen = connector.Length();
                    
                    if (connLen > 0.1f && connLen < 200f)
                    {
                        Vector2 connMid = (p_prev + p1) * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                        float connRot = connector.ToRotation();
                        float connWidth = width * 0.6f;
                        Color connColor = c * 0.7f;
                        
                        Main.spriteBatch.Draw(
                            trailTexture,
                            connMid,
                            new Rectangle(0, 0, 1, 1),
                            Projectile.GetAlpha(connColor),
                            connRot,
                            new Vector2(0.5f, 0.5f),
                            new Vector2(connLen, connWidth),
                            SpriteEffects.None,
                            0f);
                    }
                }
            }

            // Draw pulsing sparkles
            DrawPulsingSparkles(trailTexture);

            // Draw green light triangles
            DrawGreenLightTriangles(trailTexture);

            // Draw hexagon head with electric sparks
            DrawHexagonHeadWithSparks();

            return false;
        }

        private void DrawPulsingSparkles(Texture2D pixelTexture)
        {
            foreach (PulsingSparkle sparkle in sparkles)
            {
                float ageProgress = sparkle.Age / sparkle.Lifetime;
                
                // Pulse pattern: 0->1 (grow), 1->0 (shrink), repeat
                // First 0.5s: normal size, 0.5-1s: pulse up, 1-1.5s: pulse down, 1.5-2s: normal size
                float pulseCycle = ageProgress * 4f; // 4 cycles in 2 seconds (pulse every 0.5s after initial 0.5s)
                float pulse = 1f;
                
                if (pulseCycle < 2f) // First second: normal + 1 pulse cycle
                {
                    float localProgress = pulseCycle % 1f;
                    if (localProgress < 0.5f)
                    {
                        // Pulse in: 1 -> 1.3
                        pulse = 1f + (localProgress * 2f) * 0.3f;
                    }
                    else
                    {
                        // Pulse out: 1.3 -> 1
                        pulse = 1.3f - ((localProgress - 0.5f) * 2f) * 0.3f;
                    }
                }
                else
                {
                    // Second second: normal -> pulse -> normal
                    float localProgress = (pulseCycle - 2f) % 1f;
                    if (localProgress < 0.5f)
                    {
                        pulse = 1f + (localProgress * 2f) * 0.3f;
                    }
                    else
                    {
                        pulse = 1.3f - ((localProgress - 0.5f) * 2f) * 0.3f;
                    }
                }
                
                float size = sparkle.SpawnSize * pulse;
                
                // Pure white, visible in darkness
                Color sparkleColor = Color.White;
                Vector2 screenPos = sparkle.Position - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                
                // Draw as a tiny pixel
                Main.spriteBatch.Draw(
                    pixelTexture,
                    screenPos,
                    new Rectangle(0, 0, 1, 1),
                    sparkleColor,
                    0f,
                    new Vector2(0.5f, 0.5f),
                    size * 2f,
                    SpriteEffects.None,
                    0f);
                
                // Minimal light output
                Lighting.AddLight(sparkle.Position, Color.White.ToVector3() * 0.1f);
            }
        }

        private void DrawGreenLightTriangles(Texture2D pixelTexture)
        {
            foreach (GreenLightTriangle tri in greenTriangles)
            {
                float ageProgress = tri.Age / tri.Lifetime;
                float alpha = MathHelper.Lerp(1f, 0f, ageProgress);
                
                Vector2 screenPos = tri.Position - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                float size = tri.Size;
                float rot = tri.Rotation;
                
                // Calculate triangle points
                Vector2 p1 = screenPos + new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * size;
                Vector2 p2 = screenPos + new Vector2((float)Math.Cos(rot + MathHelper.TwoPi / 3f), (float)Math.Sin(rot + MathHelper.TwoPi / 3f)) * size;
                Vector2 p3 = screenPos + new Vector2((float)Math.Cos(rot + MathHelper.TwoPi * 2f / 3f), (float)Math.Sin(rot + MathHelper.TwoPi * 2f / 3f)) * size;
                
                Color greenColor = Color.LimeGreen * alpha;
                
                // Draw triangle edges
                DrawTriangleLine(p1, p2, greenColor, 3f, pixelTexture);
                DrawTriangleLine(p2, p3, greenColor, 3f, pixelTexture);
                DrawTriangleLine(p3, p1, greenColor, 3f, pixelTexture);
                
                Lighting.AddLight(tri.Position, greenColor.ToVector3() * 0.3f);
            }
        }

        // Separate draw function for triangles - positions already in screen space
        private void DrawTriangleLine(Vector2 a, Vector2 b, Color color, float width, Texture2D texture)
        {
            Vector2 diff = b - a;
            float len = diff.Length();
            if (len <= 0.001f)
                return;

            float rot = diff.ToRotation();
            Vector2 mid = (a + b) * 0.5f;

            Main.spriteBatch.Draw(
                texture,
                mid,
                new Rectangle(0, 0, 1, 1),
                color,
                rot,
                new Vector2(0.5f, 0.5f),
                new Vector2(len, width),
                SpriteEffects.None,
                0f);
        }

        private void DrawHexagonHeadWithSparks()
        {
            Texture2D trailTexture = TextureAssets.MagicPixel.Value;
            float t = Main.GlobalTimeWrappedHourly;

            // Hexagon head at projectile center
            Vector2 hexCenter = Projectile.Center;
            float hexRadius = 16f * Projectile.scale;
            float polyRotation = t * 4.5f;
            float lineWidth = 3.5f * Projectile.scale;

            // Color cycling: deep pink, hot pink, light pink
            Color hexColor = Color.Lerp(Color.DeepPink, Color.HotPink, (float)Math.Sin(t * 2.2f) * 0.5f + 0.5f);
            
            const int sides = 6;
            Span<Vector2> pts = stackalloc Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = polyRotation + MathHelper.TwoPi * (i / (float)sides);
                pts[i] = hexCenter + new Vector2(hexRadius, 0f).RotatedBy(a);
            }

            // Draw outer filled hexagon (solid purple)
            Color filledColor = Color.MediumPurple * 0.7f;
            
            for (int i = 1; i < sides - 1; i++)
            {
                DrawLightLine(hexCenter, pts[i], filledColor * 0.5f, lineWidth * 2f, trailTexture);
            }

            // Draw outer bright hexagon outline
            for (int i = 0; i < sides; i++)
            {
                Vector2 a = pts[i];
                Vector2 b = pts[(i + 1) % sides];
                DrawLightLine(a, b, hexColor, lineWidth, trailTexture);
            }

            // Draw inner subtle hexagon lines
            Color innerColor = Color.Lerp(hexColor, Color.White, 0.5f) * 0.9f;
            for (int i = 0; i < sides; i++)
            {
                Vector2 a = pts[i];
                Vector2 b = pts[(i + 1) % sides];
                DrawLightLine(a, b, innerColor, lineWidth * 0.45f, trailTexture);
            }

            Lighting.AddLight(hexCenter, hexColor.ToVector3() * 0.65f);

            // Draw expanding hexagon aura that grows and fades
            DrawExpandingHexagon(hexCenter, hexRadius, polyRotation, sides, trailTexture, lineWidth);
        }

        private void DrawExpandingHexagon(Vector2 center, float baseRadius, float rotation, int sides, Texture2D texture, float baseLineWidth)
        {
            float t = Main.GlobalTimeWrappedHourly;
            
            float animationTime = (500f - Projectile.timeLeft) * 0.01f;
            float expandProgress = animationTime % 1.5f;
            
            float expandingRadius = MathHelper.Lerp(baseRadius, baseRadius * 2.5f, expandProgress);
            float expandingAlpha = MathHelper.Clamp(1f - (expandProgress * 0.8f), 0f, 1f);
            
            Color expandingColor;
            if (expandProgress < 0.5f)
            {
                float colorT = expandProgress * 2f;
                Color startColor = Color.Lerp(Color.DeepPink, Color.HotPink, (float)Math.Sin(t * 2.2f) * 0.5f + 0.5f);
                expandingColor = Color.Lerp(startColor, Color.White, colorT);
            }
            else
            {
                float colorT = (expandProgress - 0.5f) * 2f;
                expandingColor = Color.Lerp(Color.White, Color.MediumPurple, colorT);
            }
            
            expandingColor *= expandingAlpha;
            
            Span<Vector2> expandPts = stackalloc Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = rotation + MathHelper.TwoPi * (i / (float)sides);
                expandPts[i] = center + new Vector2(expandingRadius, 0f).RotatedBy(a);
            }

            for (int i = 0; i < sides; i++)
            {
                Vector2 a = expandPts[i];
                Vector2 b = expandPts[(i + 1) % sides];
                DrawLightLine(a, b, expandingColor, baseLineWidth * 0.8f, texture);
            }

            Color innerExpanding = Color.Lerp(expandingColor, Color.White, 0.4f) * (expandingAlpha * 0.6f);
            for (int i = 0; i < sides; i++)
            {
                Vector2 a = expandPts[i];
                Vector2 b = expandPts[(i + 1) % sides];
                DrawLightLine(a, b, innerExpanding, baseLineWidth * 0.3f, texture);
            }

            Lighting.AddLight(center, expandingColor.ToVector3() * expandingAlpha * 0.4f);
        }

        private void DrawLightLine(Vector2 a, Vector2 b, Color color, float width, Texture2D texture)
        {
            Vector2 diff = b - a;
            float len = diff.Length();
            if (len <= 0.001f)
                return;

            float rot = diff.ToRotation();
            Vector2 mid = (a + b) * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            Main.spriteBatch.Draw(
                texture,
                mid,
                new Rectangle(0, 0, 1, 1),
                Projectile.GetAlpha(color),
                rot,
                new Vector2(0.5f, 0.5f),
                new Vector2(len, width),
                SpriteEffects.None,
                0f);
        }
    }
}
