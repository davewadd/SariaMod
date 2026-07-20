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
        /// Matches the minimum gravity multiplier Terraria uses in space.
        /// </summary>
        public const float SpaceGravityMultiplier = 0.25f;

        /// <summary>
        /// Airborne movement-speed bonus, expressed as 25 percentage points.
        /// </summary>
        public const float AirborneMoveSpeedBonus = 0.25f;

        /// <summary>
        /// Existing movement bonuses above 50% prevent the field bonus.
        /// </summary>
        public const float AirborneMoveSpeedBonusCutoff = 1.5f;

        /// <summary>
        /// Extra downward velocity per tick applied while holding down inside a field.
        /// </summary>
        public const float PortalFallAcceleration = 0.6f;

        /// <summary>
        /// Maximum fast-fall speed while holding down inside a field.
        /// </summary>
        public const float PortalFallMaxSpeed = 10f * 3.5f;

        public const float PelletLaunchSpeed = 6f;

        // Reused once-per-update cache shared by gameplay checks and field-link drawing.
        private static readonly List<int> activeFieldIndices = new List<int>();
        private static readonly List<int> componentFieldIndices = new List<int>();
        private static readonly Queue<int> fieldSearchQueue = new Queue<int>();
        private static readonly bool[] visitedFieldIndices = new bool[Main.maxProjectiles];
        private static readonly int[] clusterIdByFieldIndex = new int[Main.maxProjectiles];
        private static readonly int[] clusterSizeByFieldIndex = new int[Main.maxProjectiles];
        private static ulong cachedFieldUpdate = ulong.MaxValue;
        private static ulong refreshedEnemyBuffsUpdate = ulong.MaxValue;

        public static bool TrySummonFieldFromCharge(Projectile sariaProjectile, Projectile chargeProjectile)
        {
            if (Main.myPlayer != sariaProjectile.owner)
            {
                return false;
            }

            Vector2 fieldPosition = chargeProjectile.Center;
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

            return TrySummonFieldAuthoritatively(sariaProjectile, chargeProjectile, chargeProjectile.Center);
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

        public static void SpawnPelletsForProjectileHit(Projectile sourceProjectile, NPC target)
        {
            int sourceDamage = sourceProjectile.damage;
            if (!CanSourceSpawnPellets(sourceProjectile, sourceDamage))
            {
                return;
            }

            if (!target.HasBuff(ModContent.BuffType<PsychicFieldDebuff>()))
            {
                return;
            }

            int pelletCount = target.GetGlobalNPC<FairyGlobalNPC>().psychicFieldPelletCount;
            if (pelletCount <= 0)
            {
                return;
            }

            int pelletDamage = Math.Max(1, (int)Math.Round(sourceDamage * PelletDamageMultiplier));
            for (int i = 0; i < pelletCount; i++)
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
                    ModContent.ProjectileType<LocatorPellet>(),
                    pelletDamage,
                    0f,
                    sourceProjectile.owner);

                if (Main.projectile.IndexInRange(pelletIndex))
                {
                    Main.projectile[pelletIndex].originalDamage = pelletDamage;
                }
            }
        }

        public static bool IsActiveField(Projectile projectile)
        {
            return projectile.active && projectile.type == ModContent.ProjectileType<PsychicFieldProjectile>();
        }

        /// <summary>
        /// No-fall-damage setup. Called from PostUpdateMiscEffects.
        /// Gravity and fast-fall movement are applied in a later hook after movement effects settle.
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
            return true;
        }
        
        /// <summary>
        /// Applies space gravity while holding up inside a field, with the existing fast fall while holding down.
        /// Called from PostUpdateRunSpeeds after other movement effects have settled.
        /// </summary>
        public static void ApplyPortalFallAmplification(Player player)
        {
            if (!PlayerIsInsideField(player))
            {
                return;
            }

            if (player.controlUp)
            {
                player.gravity = Math.Min(player.gravity, Player.defaultGravity * SpaceGravityMultiplier);
            }

            if (player.controlDown && player.velocity.Y > 0f)
            {
                player.maxFallSpeed = Math.Max(player.maxFallSpeed, PortalFallMaxSpeed);
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
            if (sourceProjectile.type == ModContent.ProjectileType<LocatorPellet>()
                || sourceProjectile.type == ModContent.ProjectileType<PsychicFieldProjectile>())
            {
                return false;
            }

            return true;
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

            EnsureFieldClusterCache();
            int clusterId = clusterIdByFieldIndex[sourceField.whoAmI];
            if (clusterId <= 0)
            {
                return 0;
            }

            for (int i = 0; i < activeFieldIndices.Count; i++)
            {
                int fieldIndex = activeFieldIndices[i];
                if (clusterIdByFieldIndex[fieldIndex] == clusterId)
                {
                    linkedFields.Add(Main.projectile[fieldIndex]);
                }
            }

            return linkedFields.Count;
        }

        public static void RefreshEnemyBuffs()
        {
            if (refreshedEnemyBuffsUpdate == Main.GameUpdateCount)
            {
                return;
            }

            refreshedEnemyBuffsUpdate = Main.GameUpdateCount;
            EnsureFieldClusterCache();
            if (activeFieldIndices.Count == 0)
            {
                return;
            }

            int buffType = ModContent.BuffType<PsychicFieldDebuff>();
            float radiusSquared = FieldRadius * FieldRadius;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                {
                    continue;
                }

                int pelletCount = 0;
                for (int j = 0; j < activeFieldIndices.Count; j++)
                {
                    int fieldIndex = activeFieldIndices[j];
                    Projectile field = Main.projectile[fieldIndex];
                    if (Vector2.DistanceSquared(npc.Center, field.Center) <= radiusSquared)
                    {
                        pelletCount = Math.Max(pelletCount, clusterSizeByFieldIndex[fieldIndex]);
                    }
                }

                if (pelletCount <= 0)
                {
                    continue;
                }

                npc.buffImmune[buffType] = false;
                // This value remains on the NPC while the refreshed buff lingers outside the fields.
                npc.GetGlobalNPC<FairyGlobalNPC>().psychicFieldPelletCount = pelletCount;
                npc.AddBuff(buffType, PsychicFieldDebuff.LingerDuration);
            }
        }

        private static void EnsureFieldClusterCache()
        {
            if (cachedFieldUpdate == Main.GameUpdateCount)
            {
                return;
            }

            cachedFieldUpdate = Main.GameUpdateCount;
            activeFieldIndices.Clear();
            componentFieldIndices.Clear();
            fieldSearchQueue.Clear();
            Array.Clear(visitedFieldIndices, 0, visitedFieldIndices.Length);
            Array.Clear(clusterIdByFieldIndex, 0, clusterIdByFieldIndex.Length);
            Array.Clear(clusterSizeByFieldIndex, 0, clusterSizeByFieldIndex.Length);

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile field = Main.projectile[i];
                if (IsActiveField(field) && TryGetActiveFieldOwner(field, out _))
                {
                    activeFieldIndices.Add(i);
                }
            }

            float overlapSquared = (FieldRadius * 2f) * (FieldRadius * 2f);
            int nextClusterId = 1;
            for (int i = 0; i < activeFieldIndices.Count; i++)
            {
                int startIndex = activeFieldIndices[i];
                if (visitedFieldIndices[startIndex])
                {
                    continue;
                }

                componentFieldIndices.Clear();
                fieldSearchQueue.Clear();
                visitedFieldIndices[startIndex] = true;
                fieldSearchQueue.Enqueue(startIndex);

                while (fieldSearchQueue.Count > 0)
                {
                    int currentIndex = fieldSearchQueue.Dequeue();
                    Projectile currentField = Main.projectile[currentIndex];
                    if (!IsActiveField(currentField))
                    {
                        continue;
                    }

                    componentFieldIndices.Add(currentIndex);

                    for (int j = 0; j < activeFieldIndices.Count; j++)
                    {
                        int candidateIndex = activeFieldIndices[j];
                        if (visitedFieldIndices[candidateIndex])
                        {
                            continue;
                        }

                        Projectile candidate = Main.projectile[candidateIndex];
                        if (!IsActiveField(candidate))
                        {
                            continue;
                        }

                        if (Vector2.DistanceSquared(currentField.Center, candidate.Center) <= overlapSquared
                            && CanFieldsFuse(currentField, candidate))
                        {
                            visitedFieldIndices[candidateIndex] = true;
                            fieldSearchQueue.Enqueue(candidateIndex);
                        }
                    }
                }

                int clusterSize = componentFieldIndices.Count;
                for (int j = 0; j < componentFieldIndices.Count; j++)
                {
                    int fieldIndex = componentFieldIndices[j];
                    clusterIdByFieldIndex[fieldIndex] = nextClusterId;
                    clusterSizeByFieldIndex[fieldIndex] = clusterSize;
                }

                nextClusterId++;
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
            PsychicFieldSystem.RefreshEnemyBuffs();
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

        private void SpawnIdleDust()
        {
            if (!Main.rand.NextBool(5))
            {
                return;
            }

            if (!VisualDustLimiter.TryReserveHalfCapacitySlot())
            {
                return;
            }

            Vector2 edge = Projectile.Center + Main.rand.NextVector2CircularEdge(PsychicFieldSystem.FieldRadius, PsychicFieldSystem.FieldRadius);
            Dust dust = Dust.NewDustPerfect(edge, ModContent.DustType<AbsorbPsychic>(), (Projectile.Center - edge).SafeNormalize(Vector2.Zero) * 2f, Scale: Main.rand.NextFloat(1.1f, 1.7f));
            dust.noGravity = true;
            // Add a secondary bright dust at the edge for visibility
            if (Main.rand.NextBool(3) && VisualDustLimiter.TryReserveHalfCapacitySlot())
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
