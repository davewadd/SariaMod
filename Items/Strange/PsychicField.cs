using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace SariaMod.Items.Strange
{
    public static class PsychicFieldSystem
    {
        public const int MaxOwnedFields = 3;
        public const float FieldRadius = 640f;
        public const float LinkRadius = 760f;
        public const float PelletDamageMultiplier = 0.2f;
        public const float PortalFallMaxSpeed = 50f;
        public const float PortalFallAcceleration = 0.65f;
        public const int EnemyBuffRefreshTime = 12;

        private static readonly List<Projectile> LinkedFields = new List<Projectile>();

        public static bool TrySummonFieldFromCharge(Projectile sariaProjectile, Projectile chargeProjectile)
        {
            if (Main.myPlayer != sariaProjectile.owner)
            {
                return false;
            }

            Player owner = Main.player[sariaProjectile.owner];
            if (owner.ownedProjectileCounts[ModContent.ProjectileType<PsychicFieldProjectile>()] >= MaxOwnedFields)
            {
                SoundEngine.PlaySound(SoundID.MenuClose, chargeProjectile.Center);
                return false;
            }

            int fieldIndex = Projectile.NewProjectile(
                sariaProjectile.GetSource_FromThis(),
                chargeProjectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<PsychicFieldProjectile>(),
                sariaProjectile.damage,
                0f,
                sariaProjectile.owner,
                sariaProjectile.whoAmI,
                chargeProjectile.whoAmI);

            if (Main.projectile.IndexInRange(fieldIndex))
            {
                Main.projectile[fieldIndex].originalDamage = sariaProjectile.damage;
            }

            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, chargeProjectile.Center);
            chargeProjectile.Kill();
            return true;
        }

        public static void SpawnPelletsForProjectileHit(Projectile sourceProjectile, NPC target, int damage)
        {
            if (!CanSourceSpawnPellets(sourceProjectile, damage))
            {
                return;
            }

            int fieldCount = GetLinkedFieldsTouching(target.Center, LinkedFields);
            if (fieldCount <= 0)
            {
                return;
            }

            int pelletDamage = Math.Max(1, (int)Math.Round(damage * PelletDamageMultiplier));
            for (int i = 0; i < fieldCount; i++)
            {
                Projectile field = LinkedFields[i];
                Vector2 spawnPosition = field.Center;
                Vector2 direction = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitY);
                Vector2 velocity = direction * 18f;

                int pelletIndex = Projectile.NewProjectile(
                    sourceProjectile.GetSource_FromThis(),
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<PsychicPellet>(),
                    pelletDamage,
                    0f,
                    sourceProjectile.owner,
                    target.whoAmI,
                    field.whoAmI);

                if (Main.projectile.IndexInRange(pelletIndex))
                {
                    Main.projectile[pelletIndex].originalDamage = pelletDamage;
                }
            }
        }

        public static int GetLinkedFieldsTouching(Vector2 worldPosition, List<Projectile> linkedFields)
        {
            linkedFields.Clear();

            bool[] visited = new bool[Main.maxProjectiles];
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile field = Main.projectile[i];
                if (!IsActiveField(field) || visited[i])
                {
                    continue;
                }

                if (Vector2.DistanceSquared(worldPosition, field.Center) <= FieldRadius * FieldRadius)
                {
                    AddLinkedCluster(i, visited, linkedFields);
                }
            }

            return linkedFields.Count;
        }

        public static bool IsActiveField(Projectile projectile)
        {
            return projectile.active && projectile.type == ModContent.ProjectileType<PsychicFieldProjectile>();
        }

        private static bool CanSourceSpawnPellets(Projectile sourceProjectile, int damage)
        {
            if (!sourceProjectile.active || damage <= 0 || !sourceProjectile.friendly || sourceProjectile.hostile)
            {
                return false;
            }

            if (sourceProjectile.owner < 0 || sourceProjectile.owner >= Main.maxPlayers || Main.myPlayer != sourceProjectile.owner)
            {
                return false;
            }

            if (sourceProjectile.type == ModContent.ProjectileType<PsychicPellet>()
                || sourceProjectile.type == ModContent.ProjectileType<PsychicFieldProjectile>())
            {
                return false;
            }

            return true;
        }

        private static void AddLinkedCluster(int startIndex, bool[] visited, List<Projectile> linkedFields)
        {
            Queue<int> queue = new Queue<int>();
            visited[startIndex] = true;
            queue.Enqueue(startIndex);

            while (queue.Count > 0)
            {
                int currentIndex = queue.Dequeue();
                Projectile currentField = Main.projectile[currentIndex];
                if (!IsActiveField(currentField))
                {
                    continue;
                }

                linkedFields.Add(currentField);

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (visited[i])
                    {
                        continue;
                    }

                    Projectile candidate = Main.projectile[i];
                    if (!IsActiveField(candidate))
                    {
                        continue;
                    }

                    if (Vector2.DistanceSquared(currentField.Center, candidate.Center) <= LinkRadius * LinkRadius)
                    {
                        visited[i] = true;
                        queue.Enqueue(i);
                    }
                }
            }
        }
    }

    public class PsychicFieldProjectile : ModProjectile
    {
        private const int RingSegments = 72;
        private readonly List<Projectile> linkedFields = new List<Projectile>();

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Psychic Field");
            ProjectileID.Sets.MinionShot[Projectile.type] = false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.netImportant = true;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanHitNPC(NPC target) => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 2;
            Projectile.rotation += 0.015f;

            Lighting.AddLight(Projectile.Center, Color.DeepPink.ToVector3() * 0.65f);
            ApplyPortalFallPhysics();
            RefreshEnemyBuffs();
            SpawnIdleDust();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float pulse = 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + Projectile.whoAmI);
            Color outer = Color.Lerp(Color.MediumPurple, Color.DeepPink, pulse) * 0.55f;
            Color inner = Color.Lerp(Color.Cyan, Color.White, pulse) * 0.35f;

            DrawRing(pixel, PsychicFieldSystem.FieldRadius, outer, 3f);
            DrawRing(pixel, PsychicFieldSystem.FieldRadius * 0.62f, inner, 2f);
            DrawCenterHex(pixel);
            DrawLinks(pixel);

            return false;
        }

        private void ApplyPortalFallPhysics()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || Vector2.DistanceSquared(player.Center, Projectile.Center) > PsychicFieldSystem.FieldRadius * PsychicFieldSystem.FieldRadius)
                {
                    continue;
                }

                player.noFallDmg = true;
                player.fallStart = (int)(player.position.Y / 16f);
                player.maxFallSpeed = Math.Max(player.maxFallSpeed, PsychicFieldSystem.PortalFallMaxSpeed);
                if (player.velocity.Y > 0f && player.velocity.Y < PsychicFieldSystem.PortalFallMaxSpeed)
                {
                    player.velocity.Y = Math.Min(PsychicFieldSystem.PortalFallMaxSpeed, player.velocity.Y + PsychicFieldSystem.PortalFallAcceleration);
                }
            }
        }

        private void RefreshEnemyBuffs()
        {
            int buffType = ModContent.BuffType<PsychicFieldDebuff>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                {
                    continue;
                }

                if (Vector2.DistanceSquared(npc.Center, Projectile.Center) <= PsychicFieldSystem.FieldRadius * PsychicFieldSystem.FieldRadius)
                {
                    npc.buffImmune[buffType] = false;
                    npc.AddBuff(buffType, PsychicFieldSystem.EnemyBuffRefreshTime);
                }
            }
        }

        private void SpawnIdleDust()
        {
            if (!Main.rand.NextBool(5))
            {
                return;
            }

            Vector2 edge = Projectile.Center + Main.rand.NextVector2CircularEdge(PsychicFieldSystem.FieldRadius, PsychicFieldSystem.FieldRadius);
            Dust dust = Dust.NewDustPerfect(edge, ModContent.DustType<AbsorbPsychic>(), (Projectile.Center - edge).SafeNormalize(Vector2.Zero) * 2f, Scale: Main.rand.NextFloat(1.1f, 1.7f));
            dust.noGravity = true;
        }

        private void DrawLinks(Texture2D pixel)
        {
            linkedFields.Clear();
            PsychicFieldSystem.GetLinkedFieldsTouching(Projectile.Center, linkedFields);

            for (int i = 0; i < linkedFields.Count; i++)
            {
                Projectile other = linkedFields[i];
                if (other.whoAmI == Projectile.whoAmI)
                {
                    continue;
                }

                DrawLine(pixel, Projectile.Center, other.Center, Color.DeepPink * 0.22f, 2f);
            }
        }

        private void DrawRing(Texture2D pixel, float radius, Color color, float width)
        {
            Vector2 previous = Projectile.Center + new Vector2(radius, 0f).RotatedBy(Projectile.rotation);
            for (int i = 1; i <= RingSegments; i++)
            {
                float angle = Projectile.rotation + MathHelper.TwoPi * i / RingSegments;
                Vector2 next = Projectile.Center + new Vector2(radius, 0f).RotatedBy(angle);
                DrawLine(pixel, previous, next, color, width);
                previous = next;
            }
        }

        private void DrawCenterHex(Texture2D pixel)
        {
            const int sides = 6;
            float radius = 34f + 4f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f);
            Vector2 previous = Projectile.Center + new Vector2(radius, 0f).RotatedBy(Projectile.rotation);
            for (int i = 1; i <= sides; i++)
            {
                float angle = Projectile.rotation + MathHelper.TwoPi * i / sides;
                Vector2 next = Projectile.Center + new Vector2(radius, 0f).RotatedBy(angle);
                DrawLine(pixel, previous, next, Color.HotPink * 0.9f, 3f);
                previous = next;
            }
        }

        private void DrawLine(Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 difference = end - start;
            float length = difference.Length();
            if (length <= 0.001f)
            {
                return;
            }

            Vector2 drawPosition = (start + end) * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Main.spriteBatch.Draw(
                pixel,
                drawPosition,
                new Rectangle(0, 0, 1, 1),
                Projectile.GetAlpha(color),
                difference.ToRotation(),
                new Vector2(0.5f, 0.5f),
                new Vector2(length, width),
                SpriteEffects.None,
                0f);
        }
    }

    public class PsychicPellet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Psychic Pellet");
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.extraUpdates = 1;
        }

        public override bool? CanCutTiles() => false;

        public override bool? CanHitNPC(NPC target)
        {
            int targetIndex = (int)Projectile.ai[0];
            return target.whoAmI == targetIndex && target.CanBeChasedBy(Projectile);
        }

        public override void AI()
        {
            int targetIndex = (int)Projectile.ai[0];
            if (targetIndex < 0 || targetIndex >= Main.maxNPCs || !Main.npc[targetIndex].active)
            {
                Projectile.Kill();
                return;
            }

            NPC target = Main.npc[targetIndex];
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * 18f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.18f);
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.45f);

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Psychic>(), -Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.5f, Scale: 1.1f);
                dust.noGravity = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            crit = false;
            knockback = 0f;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 4f);
                Dust dust = Dust.NewDustPerfect(target.Center, ModContent.DustType<PsychicRingDust>(), speed, Scale: 1.4f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 centerOffset = Projectile.Size * 0.5f;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 oldPosition = Projectile.oldPos[i];
                if (oldPosition == Vector2.Zero)
                {
                    continue;
                }

                Vector2 start = Projectile.oldPos[i - 1] + centerOffset;
                Vector2 end = oldPosition + centerOffset;
                Vector2 difference = start - end;
                float length = difference.Length();
                if (length <= 0.001f)
                {
                    continue;
                }

                float progress = i / (float)Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(Color.HotPink, Color.Transparent, progress) * 0.75f;
                Main.spriteBatch.Draw(
                    pixel,
                    (start + end) * 0.5f - Main.screenPosition,
                    new Rectangle(0, 0, 1, 1),
                    Projectile.GetAlpha(trailColor),
                    difference.ToRotation(),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(length, MathHelper.Lerp(5f, 1f, progress)),
                    SpriteEffects.None,
                    0f);
            }

            Texture2D glow = TextureAssets.Extra[98].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = glow.Size() * 0.5f;
            Main.spriteBatch.Draw(glow, drawPosition, null, Color.DeepPink * 0.6f, 0f, origin, 0.12f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, drawPosition, null, Color.White, 0f, origin, 0.045f, SpriteEffects.None, 0f);
            return false;
        }
    }

    public class PsychicFieldMapLayer : ModMapLayer
    {
        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            Texture2D icon = TextureAssets.Extra[98].Value;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile field = Main.projectile[i];
                if (!PsychicFieldSystem.IsActiveField(field))
                {
                    continue;
                }

                Color iconColor = field.owner == Main.myPlayer ? Color.DeepPink : Color.MediumPurple;
                var result = context.Draw(
                    icon,
                    field.Center / 16f,
                    iconColor,
                    new SpriteFrame(1, 1, 0, 0),
                    0.16f,
                    0.16f,
                    Alignment.Center);

                if (result.IsMouseOver)
                {
                    string ownerName = field.owner >= 0 && field.owner < Main.maxPlayers && Main.player[field.owner].active
                        ? Main.player[field.owner].name
                        : "unknown owner";
                    text = field.owner == Main.myPlayer
                        ? "Psychic Field (click to unsummon)"
                        : $"Psychic Field ({ownerName})";

                    if (field.owner == Main.myPlayer && Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        field.Kill();
                        Main.mouseLeftRelease = false;
                    }
                }
            }
        }
    }
}
