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
using SariaMod.Netcode;

namespace SariaMod.Items.Strange
{
    public static class PsychicFieldSystem
    {
        /// <summary>
        /// Maximum fields one player can own after Psychic Upg 1.
        /// </summary>
        public const int MaxOwnedFields = 3;
        public const int DefaultOwnedFields = 1;
        public const int MaxSariasPerTeam = 2;
        public const int MaxFieldsPerTeam = 6;
        private const int DefaultChargeTicks = 60 * 3;
        private static int nextFieldSpawnOrder = 1;

        /// <summary>
        /// Base radius (world pixels) of each psychic field.
        /// </summary>
        public const float FieldRadius = 640f;

        /// <summary>
        /// Pellet damage = sourceDamage * PelletDamageMultiplier.
        /// </summary>
        public const float PelletDamageMultiplier = 0.2f;

        /// <summary>
        /// Extra downward velocity per tick applied to players inside a field (portal gun counter).
        /// </summary>
        public const float PortalFallAcceleration = 0.6f;

        /// <summary>
        /// Maximum fall speed cap when inside a field.
        /// </summary>
        public const float PortalFallMaxSpeed = 10f * 3.5f;

        public const float PelletLaunchSpeed = 6f;
        public const int PelletOutwardDuration = 40;
        public const float PelletHomingRange = 400f;
        internal const int PelletSearchDuration = 600;
        internal const int PelletFadeDuration = 180;

        /// <summary>
        /// Boss multiplier divisor - bosses receive 1/N of the cluster multiplier (min 1).
        /// </summary>
        public const int BossMultiplierDivisor = 2;

        public static bool TrySummonFieldFromCharge(Projectile sariaProjectile, Projectile chargeProjectile)
        {
            if (Main.myPlayer != sariaProjectile.owner)
            {
                return false;
            }

            Vector2 fieldPosition = Main.MouseWorld;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                PsychicFieldNetworking.RequestFieldSummon(sariaProjectile, chargeProjectile, fieldPosition);
                chargeProjectile.Kill();
                return true;
            }

            return TrySummonFieldAuthoritatively(sariaProjectile, chargeProjectile, fieldPosition);
        }

        internal static bool TrySummonFieldFromNetwork(
            int requester,
            int sariaIdentity,
            int chargeIdentity,
            Vector2 fieldPosition)
        {
            if (Main.netMode != NetmodeID.Server || requester < 0 || requester >= Main.maxPlayers)
            {
                return false;
            }

            Player owner = Main.player[requester];
            if (!owner.active || owner.dead || !IsFiniteWorldPosition(fieldPosition))
            {
                return false;
            }

            Projectile sariaProjectile = FindOwnedProjectile(requester, ModContent.ProjectileType<Saria>(), sariaIdentity);
            Projectile chargeProjectile = FindOwnedProjectile(requester, ModContent.ProjectileType<Ztarget2>(), chargeIdentity);
            if (sariaProjectile == null || chargeProjectile == null
                || !(chargeProjectile.ModProjectile is Ztarget2 psychicCharge)
                || psychicCharge.ChannelTimer < GetRequiredChargeTicks(owner))
            {
                return false;
            }

            return TrySummonFieldAuthoritatively(sariaProjectile, chargeProjectile, fieldPosition);
        }

        private static bool TrySummonFieldAuthoritatively(
            Projectile sariaProjectile,
            Projectile chargeProjectile,
            Vector2 fieldPosition)
        {
            Player owner = Main.player[sariaProjectile.owner];
            int maxFields = GetMaxOwnedFields(owner);
            int ownedFieldCount = CountOwnedFields(owner.whoAmI);
            bool replacesOwnedField = ownedFieldCount >= maxFields;

            if (maxFields <= 0 || !CanOwnerUsePsychicFields(owner))
            {
                RejectCharge(chargeProjectile);
                return false;
            }

            if (owner.team > 0
                && CountFieldsForTeam(owner.team) >= MaxFieldsPerTeam
                && !replacesOwnedField)
            {
                RejectCharge(chargeProjectile);
                return false;
            }

            if (replacesOwnedField)
            {
                while (CountOwnedFields(owner.whoAmI) >= maxFields)
                {
                    RemoveOldestOwnedField(owner.whoAmI);
                }
            }

            int fieldType = ModContent.ProjectileType<PsychicFieldProjectile>();
            int fieldIndex = Projectile.NewProjectile(
                sariaProjectile.GetSource_FromThis(),
                fieldPosition,
                Vector2.Zero,
                fieldType,
                sariaProjectile.damage,
                0f,
                sariaProjectile.owner,
                sariaProjectile.whoAmI,
                chargeProjectile.whoAmI);

            if (Main.projectile.IndexInRange(fieldIndex))
            {
                Projectile field = Main.projectile[fieldIndex];
                field.originalDamage = sariaProjectile.damage;
                field.localAI[0] = nextFieldSpawnOrder++;
                field.netUpdate = true;
            }

            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, fieldPosition);
            chargeProjectile.Kill();
            return true;
        }

        internal static int GetRequiredChargeTicks(Player owner)
        {
            return DefaultChargeTicks;
        }

        private static int GetMaxOwnedFields(Player owner)
        {
            return owner.Fairy().SariaUpgrade4 ? MaxOwnedFields : DefaultOwnedFields;
        }

        internal static bool CanSummonSaria(Player player)
        {
            if (player == null || !player.active || player.dead || player.team <= 0)
            {
                return player != null && player.active && !player.dead;
            }

            return CountSariaOwnersOnTeam(player.team, player.whoAmI) < MaxSariasPerTeam;
        }

        internal static bool IsSariaSpawnWithinTeamCap(int ownerIndex)
        {
            if (ownerIndex < 0 || ownerIndex >= Main.maxPlayers)
            {
                return false;
            }

            Player player = Main.player[ownerIndex];
            if (!player.active || player.dead || player.team <= 0)
            {
                return player.active && !player.dead;
            }

            return CountSariaOwnersOnTeam(player.team, ownerIndex) < MaxSariasPerTeam;
        }

        private static bool CanOwnerUsePsychicFields(Player owner)
        {
            if (owner.team <= 0)
            {
                return true;
            }

            return CountSariaOwnersOnTeam(owner.team, owner.whoAmI) < MaxSariasPerTeam;
        }

        private static int CountOwnedFields(int ownerIndex)
        {
            int fieldType = ModContent.ProjectileType<PsychicFieldProjectile>();
            int count = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile field = Main.projectile[i];
                if (field.active && field.type == fieldType && field.owner == ownerIndex)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountFieldsForTeam(int team)
        {
            if (team <= 0)
            {
                return 0;
            }

            int fieldType = ModContent.ProjectileType<PsychicFieldProjectile>();
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile field = Main.projectile[i];
                if (!field.active || field.type != fieldType || field.owner < 0 || field.owner >= Main.maxPlayers)
                {
                    continue;
                }

                Player owner = Main.player[field.owner];
                if (owner.active && !owner.dead && owner.team == team)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountSariaOwnersOnTeam(int team, int excludedOwner)
        {
            if (team <= 0)
            {
                return 0;
            }

            int sariaType = ModContent.ProjectileType<Saria>();
            bool[] countedOwners = new bool[Main.maxPlayers];
            int count = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile saria = Main.projectile[i];
                if (!saria.active || saria.type != sariaType || saria.owner < 0 || saria.owner >= Main.maxPlayers
                    || saria.owner == excludedOwner || countedOwners[saria.owner])
                {
                    continue;
                }

                Player owner = Main.player[saria.owner];
                if (owner.active && !owner.dead && owner.team == team)
                {
                    countedOwners[saria.owner] = true;
                    count++;
                }
            }

            return count;
        }

        private static Projectile FindOwnedProjectile(int owner, int type, int identity)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == owner && projectile.type == type && projectile.identity == identity)
                {
                    return projectile;
                }
            }

            return null;
        }

        private static bool IsFiniteWorldPosition(Vector2 position)
        {
            return !float.IsNaN(position.X) && !float.IsInfinity(position.X)
                && !float.IsNaN(position.Y) && !float.IsInfinity(position.Y)
                && position.X >= 0f && position.Y >= 0f
                && position.X < Main.maxTilesX * 16f && position.Y < Main.maxTilesY * 16f;
        }

        private static void RejectCharge(Projectile chargeProjectile)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.MenuClose, chargeProjectile.Center);
            }

            chargeProjectile.Kill();
        }

        private static void RemoveOldestOwnedField(int owner)
        {
            int oldestFieldIndex = -1;
            float oldestSpawnOrder = float.MaxValue;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile field = Main.projectile[i];
                if (!IsActiveField(field) || field.owner != owner)
                {
                    continue;
                }

                if (field.localAI[0] < oldestSpawnOrder)
                {
                    oldestSpawnOrder = field.localAI[0];
                    oldestFieldIndex = i;
                }
            }

            if (oldestFieldIndex >= 0)
            {
                Main.projectile[oldestFieldIndex].Kill();
            }
        }

        public static void SpawnPelletsForProjectileHit(Projectile sourceProjectile, NPC target, int damage)
        {
            if (!CanSourceSpawnPellets(sourceProjectile, damage))
            {
                return;
            }

            int currentFieldCount = GetClusterMultiplierAtPosition(target.Center);
            if (target.boss && currentFieldCount > 0)
            {
                currentFieldCount = Math.Max(1, currentFieldCount / BossMultiplierDivisor);
            }

            int lingeringFieldCount = target.GetGlobalNPC<FairyGlobalNPC>().psychicFieldMultiplier;
            int fieldCount = Math.Max(currentFieldCount, lingeringFieldCount);
            if (fieldCount <= 0)
            {
                return;
            }

            int pelletDamage = Math.Max(1, (int)Math.Round(damage * PelletDamageMultiplier));
            for (int i = 0; i < fieldCount; i++)
            {
                Vector2 spawnPosition = target.Center + Main.rand.NextVector2Circular(target.width * 0.3f, target.height * 0.3f);
                Vector2 outwardDirection = (spawnPosition - target.Center).SafeNormalize(Vector2.Zero);
                if (outwardDirection == Vector2.Zero)
                {
                    outwardDirection = Main.rand.NextVector2CircularEdge(1f, 1f);
                }

                Vector2 velocity = outwardDirection * PelletLaunchSpeed * (0.8f + 0.4f * Main.rand.NextFloat());

                int pelletIndex = Projectile.NewProjectile(
                    sourceProjectile.GetSource_FromThis(),
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<PsychicPellet>(),
                    pelletDamage,
                    0f,
                    sourceProjectile.owner);

                if (Main.projectile.IndexInRange(pelletIndex))
                {
                    Main.projectile[pelletIndex].originalDamage = pelletDamage;
                }
            }
        }

        /// <summary>
        /// Returns the number of connected fields in the cluster touching <paramref name="worldPosition"/>.
        /// Uses range-overlap connection: fields connect when their radii overlap (distance <= 2 * FieldRadius).
        /// BFS over the projectile array, restricted to one owner or one shared team.
        /// </summary>
        public static int GetClusterMultiplierAtPosition(Vector2 worldPosition)
        {
            bool[] visited = new bool[Main.maxProjectiles];
            int maxCount = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile field = Main.projectile[i];
                if (!IsActiveField(field) || visited[i])
                {
                    continue;
                }

                // Check if this field touches the position
                if (Vector2.DistanceSquared(worldPosition, field.Center) <= FieldRadius * FieldRadius)
                {
                    // Found a touching field — BFS its whole cluster and count size
                    int clusterSize = AddLinkedClusterCounting(i, visited);
                    if (clusterSize > maxCount)
                    {
                        maxCount = clusterSize;
                    }
                }
            }

            return maxCount;
        }

        public static bool IsActiveField(Projectile projectile)
        {
            return projectile.active && projectile.type == ModContent.ProjectileType<PsychicFieldProjectile>();
        }

        /// <summary>
        /// No-fall-damage setup + maxFallSpeed cap. Called from PostUpdateMiscEffects.
        /// Does NOT modify velocity — that happens in a later hook after jump/gravity settle.
        /// Returns true if the player is inside any field.
        /// </summary>
        public static bool TryApplyPortalFallSetup(Player player)
        {
            if (!PlayerIsInsideField(player))
            {
                return false;
            }
            
            player.noFallDmg = true;
            player.fallStart = (int)(player.position.Y / 16f);
            player.fallStart2 = player.fallStart;
            player.maxFallSpeed = Math.Max(player.maxFallSpeed, PortalFallMaxSpeed);
            return true;
        }
        
        /// <summary>
        /// Actively pulls the player downward while inside a field.
        /// Called from PostUpdateRunSpeeds — runs after all jump/gravity/movement processing is final.
        /// Only amplifies when the player is actually falling (velocity.Y > 0f), so jumping works normally.
        /// </summary>
        public static void ApplyPortalFallAmplification(Player player)
        {
            if (!PlayerIsInsideField(player))
            {
                return;
            }
            
            if (player.velocity.Y > 0f)
            {
                player.velocity.Y += PortalFallAcceleration;
                if (player.velocity.Y > PortalFallMaxSpeed)
                {
                    player.velocity.Y = PortalFallMaxSpeed;
                }
            }
        }


        private static bool PlayerIsInsideField(Player player)
        {
            if (!player.active)
            {
                return false;
            }

            float radiusSquared = FieldRadius * FieldRadius;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile field = Main.projectile[i];
                if (IsActiveField(field) && Vector2.DistanceSquared(player.Center, field.Center) <= radiusSquared)
                {
                    return true;
                }
            }

            return false;
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

            // Prevent infinite loops: pellets and fields themselves do not trigger pellets
            if (sourceProjectile.type == ModContent.ProjectileType<PsychicPellet>()
                || sourceProjectile.type == ModContent.ProjectileType<PsychicFieldProjectile>())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// BFS from <paramref name="startIndex"/> over overlapping fields (range-overlap).
        /// Returns the cluster size without mutating the caller's list.
        /// </summary>
        private static int AddLinkedClusterCounting(int startIndex, bool[] visited)
        {
            int count = 0;
            float overlapSquared = (FieldRadius * 2f) * (FieldRadius * 2f);
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

                count++;

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

                    // Range-overlap connection: fields connect when their radii overlap
                    if (Vector2.DistanceSquared(currentField.Center, candidate.Center) <= overlapSquared
                        && CanFieldsFuse(currentField, candidate))
                    {
                        visited[i] = true;
                        queue.Enqueue(i);
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Fills the connected cluster containing <paramref name="sourceField"/> for rendering.
        /// </summary>
        public static int GetLinkedFieldsTouching(Projectile sourceField, List<Projectile> linkedFields)
        {
            linkedFields.Clear();

            if (!IsActiveField(sourceField))
            {
                return 0;
            }

            bool[] visited = new bool[Main.maxProjectiles];
            AddLinkedCluster(sourceField.whoAmI, visited, linkedFields);

            return linkedFields.Count;
        }

        private static void AddLinkedCluster(int startIndex, bool[] visited, List<Projectile> linkedFields)
        {
            float overlapSquared = (FieldRadius * 2f) * (FieldRadius * 2f);
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

                    // Range-overlap connection
                    if (Vector2.DistanceSquared(currentField.Center, candidate.Center) <= overlapSquared
                        && CanFieldsFuse(currentField, candidate))
                    {
                        visited[i] = true;
                        queue.Enqueue(i);
                    }
                }
            }
        }

        private static bool CanFieldsFuse(Projectile firstField, Projectile secondField)
        {
            if (!TryGetActiveFieldOwner(firstField, out Player firstOwner)
                || !TryGetActiveFieldOwner(secondField, out Player secondOwner))
            {
                return false;
            }

            if (firstField.owner == secondField.owner)
            {
                return true;
            }

            if (firstOwner.team <= 0 || firstOwner.team != secondOwner.team)
            {
                return false;
            }

            return CountSariaOwnersOnTeam(firstOwner.team, -1) <= MaxSariasPerTeam;
        }

        private static bool TryGetActiveFieldOwner(Projectile field, out Player owner)
        {
            owner = null;
            if (!IsActiveField(field) || field.owner < 0 || field.owner >= Main.maxPlayers)
            {
                return false;
            }

            owner = Main.player[field.owner];
            return owner.active && !owner.dead;
        }
    }

    public class PsychicFieldProjectile : ModProjectile
    {
        private const int RingSegments = 72;
        private readonly List<Projectile> linkedFields = new List<Projectile>();
        public override string Texture => "SariaMod/Items/Strange/Ztarget2";

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
                    // Inside this field — calculate the cluster multiplier and store it
                    npc.buffImmune[buffType] = false;
                    int clusterSize = PsychicFieldSystem.GetClusterMultiplierAtPosition(npc.Center);

                    // Bosses get reduced multiplier
                    if (npc.boss)
                    {
                        clusterSize = Math.Max(1, clusterSize / PsychicFieldSystem.BossMultiplierDivisor);
                    }

                    // Store on the NPC's global data
                    FairyGlobalNPC globalNpc = npc.GetGlobalNPC<FairyGlobalNPC>();
                    globalNpc.psychicFieldMultiplier = clusterSize;

                    // Apply with linger duration so the buff persists after leaving the field
                    npc.AddBuff(buffType, PsychicFieldDebuff.LingerDuration);
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
            // Add a secondary bright dust at the edge for visibility
            if (Main.rand.NextBool(3))
            {
                Dust brightDust = Dust.NewDustPerfect(edge, ModContent.DustType<Psychic3>(), Vector2.Zero, Scale: 0.8f);
                brightDust.noGravity = true;
            }
        }

        private void DrawLinks(Texture2D pixel)
        {
            linkedFields.Clear();
            PsychicFieldSystem.GetLinkedFieldsTouching(Projectile, linkedFields);

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
        public override string Texture => "SariaMod/Items/Strange/LocatorSmall";

        private int Age => (int)Projectile.ai[0];
        private float FadeProgress
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        private float FadeRatio => MathHelper.Clamp(FadeProgress / PsychicFieldSystem.PelletFadeDuration, 0f, 1f);
        private bool IsHomingPhase => Age > PsychicFieldSystem.PelletOutwardDuration;

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
            Projectile.scale = 1.5f;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 2;
            Projectile.extraUpdates = 1;
        }

        public override bool? CanCutTiles() => false;

        public override bool? CanHitNPC(NPC target)
        {
            if (!IsHomingPhase)
            {
                return false;
            }

            return IsValidPelletTarget(target);
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.timeLeft = 2;

            if (IsHomingPhase)
            {
                NPC nearest = FindNearestEnemy();
                if (nearest != null)
                {
                    if (FadeProgress > 0f || Age > PsychicFieldSystem.PelletSearchDuration)
                    {
                        Projectile.ai[0] = PsychicFieldSystem.PelletOutwardDuration + 1;
                    }

                    FadeProgress = 0f;

                    int homingAge = Age - PsychicFieldSystem.PelletOutwardDuration;
                    float ramp = MathHelper.Clamp(homingAge / 60f, 0f, 1f);
                    float homingSpeed = MathHelper.Lerp(5f, 18f, ramp);
                    float homingStrength = MathHelper.Lerp(0.05f, 0.25f, ramp);
                    Vector2 desiredVelocity = Projectile.DirectionTo(nearest.Center) * homingSpeed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
                }
                else
                {
                    Projectile.velocity *= 0.96f;
                    if (Age > PsychicFieldSystem.PelletSearchDuration)
                    {
                        FadeProgress++;
                        if (FadeProgress >= PsychicFieldSystem.PelletFadeDuration)
                        {
                            Projectile.Kill();
                            return;
                        }
                    }
                }
            }
            else
            {
                Projectile.velocity *= 0.94f;
            }

            // Clear stale custom-trail history once the pellet is effectively stopped.
            if (Projectile.velocity.Length() < 0.5f)
            {
                for (int i = 1; i < Projectile.oldPos.Length; i++)
                {
                    Projectile.oldPos[i] = Vector2.Zero;
                }

                if (!IsHomingPhase && Age >= PsychicFieldSystem.PelletOutwardDuration - 4)
                {
                    Projectile.velocity = Vector2.Zero;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            float drawAlpha = GetDrawAlpha();
            Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * (0.45f * drawAlpha));
            SpawnMovementDust(drawAlpha);
        }

        private NPC FindNearestEnemy()
        {
            NPC nearest = null;
            float nearestDistSq = PsychicFieldSystem.PelletHomingRange * PsychicFieldSystem.PelletHomingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!IsValidPelletTarget(npc))
                {
                    continue;
                }

                float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distSq < nearestDistSq)
                {
                    nearestDistSq = distSq;
                    nearest = npc;
                }
            }

            return nearest;
        }

        private bool IsValidPelletTarget(NPC npc)
        {
            if (!npc.active)
            {
                return false;
            }

            if (npc.type == NPCID.TargetDummy)
            {
                return true;
            }

            return npc.CanBeChasedBy(Projectile);
        }

        private float GetDrawAlpha()
        {
            if (FadeProgress <= 0f)
            {
                return 1f;
            }

            float fadeRatio = FadeRatio;
            float flicker = 0.55f + 0.45f * (0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 36f + Projectile.whoAmI));
            return MathHelper.Clamp((1f - fadeRatio) * flicker, 0f, 1f);
        }

        private void SpawnMovementDust(float drawAlpha)
        {
            if (drawAlpha <= 0f || Projectile.velocity.Length() < 1f)
            {
                return;
            }

            Vector2 reverseVelocity = -Projectile.velocity.SafeNormalize(Vector2.Zero);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Psychic>(), reverseVelocity * 1.7f, Scale: 1.2f);
                dust.noGravity = true;
            }

            if (Main.rand.NextBool(4))
            {
                Dust flash = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 8f, ModContent.DustType<Psychic3>(), reverseVelocity * 0.8f, Scale: 0.85f);
                flash.noGravity = true;
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
            float drawAlpha = GetDrawAlpha();

            float speed = Projectile.velocity.Length();
            if (speed >= 0.5f)
            {
                float trailIntensity = MathHelper.Clamp(speed / 5f, 0f, 1f);
                float targetDepth = trailIntensity * Projectile.oldPos.Length;

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
                    float segmentFade = MathHelper.Clamp(targetDepth - i + 1f, 0f, 1f);
                    float baseWidth = MathHelper.Lerp(5f, 1f, progress);
                    Color trailColor = Color.Lerp(Color.HotPink, Color.Transparent, progress) * 0.75f * segmentFade * drawAlpha;
                    Main.spriteBatch.Draw(
                        pixel,
                        (start + end) * 0.5f - Main.screenPosition,
                        new Rectangle(0, 0, 1, 1),
                        Projectile.GetAlpha(trailColor),
                        difference.ToRotation(),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(length, baseWidth * trailIntensity),
                        SpriteEffects.None,
                        0f);
                }
            }

            Texture2D glow = TextureAssets.Extra[98].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = glow.Size() * 0.5f;
            Main.spriteBatch.Draw(glow, drawPosition, null, Color.DeepPink * (0.6f * drawAlpha), 0f, origin, 0.12f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, drawPosition, null, Color.White * drawAlpha, 0f, origin, 0.045f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(
                pixel,
                drawPosition,
                new Rectangle(0, 0, 1, 1),
                Projectile.GetAlpha(Color.White * drawAlpha),
                Projectile.rotation,
                new Vector2(0.5f, 0.5f),
                new Vector2(3f, 3f) * Projectile.scale,
                SpriteEffects.None,
                0f);
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
