using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Bands
{
    /// <summary>
    /// A non-damaging laser projectile that traces from the player toward the cursor
    /// and snaps to the closest tile (including platforms). Modeled after the ZZBeam
    /// (Last Prism) pattern — uses Collision.LaserScan for tile detection and
    /// Utils.DrawLaser for beam rendering.
    /// Does NO damage and cannot hit anything.
    /// </summary>
    public class HookshotLaser : ModProjectile
    {
        private const float MaxBeamLengthDefault = 960f;
        private const float MaxBeamLengthLongshot = 1920f;
        private const int NumSamplePoints = 5;
        private const float BeamLengthChangeFactor = 0.75f;

        public override string Texture => "SariaMod/Items/Bands/HookshotDot";

        private float BeamLength
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        private bool _hitNPC;
        private bool _hitAnything;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hookshot Laser");
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.damage = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanHitNPC(NPC target) => false;
        public override bool CanHitPvp(Player target) => false;
        public override bool MinionContactDamage() => false;

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(BeamLength);
        public override void ReceiveExtraAI(BinaryReader reader) => BeamLength = reader.ReadSingle();

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Determine which hookshot the player is holding
            HookshotPlayer hookshotPlayer = player.GetModPlayer<HookshotPlayer>();
            bool holdingHookshot = player.HeldItem.type == ModContent.ItemType<HookShot>()
                || (hookshotPlayer.isForceHoldingHookshot && hookshotPlayer.forceHoldItemType == ModContent.ItemType<HookShot>());
            bool holdingLongshot = player.HeldItem.type == ModContent.ItemType<Longshot>()
                || (hookshotPlayer.isForceHoldingHookshot && hookshotPlayer.forceHoldItemType == ModContent.ItemType<Longshot>());

            if (!holdingHookshot && !holdingLongshot)
            {
                Projectile.Kill();
                return;
            }

            // Kill if an actual hook projectile is deployed
            bool hasActiveHook = player.ownedProjectileCounts[ModContent.ProjectileType<HookshotProjectile>()] > 0
                || player.ownedProjectileCounts[ModContent.ProjectileType<LongshotProjectile>()] > 0;
            if (hasActiveHook)
            {
                Projectile.Kill();
                return;
            }

            // Keep alive while conditions are met
            Projectile.timeLeft = 2;

            float maxRange = holdingLongshot ? MaxBeamLengthLongshot : MaxBeamLengthDefault;

            // Position at player center
            Projectile.Center = player.Center;

            // Aim toward mouse (only owner knows mouse position)
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 aim = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                if (aim.HasNaNs() || aim == Vector2.Zero)
                    aim = Vector2.UnitX * player.direction;

                if (aim != Projectile.velocity)
                {
                    Projectile.velocity = aim;
                    Projectile.netUpdate = true;
                }
            }

            if (Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = Vector2.UnitX * player.direction;
            }

            // Normalize velocity to unit length for direction
            Projectile.velocity = Vector2.Normalize(Projectile.velocity);
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Perform hitscan — LaserScan with width 0 for precise center-line tile detection
            float hitscanLength = PerformBeamHitscan(player, maxRange);
            BeamLength = MathHelper.Lerp(BeamLength, hitscanLength, BeamLengthChangeFactor);
        }

        private float PerformBeamHitscan(Player player, float maxRange)
        {
            Vector2 origin = player.Center;
            Vector2 dir = Projectile.velocity;

            // 1. Solid tile detection via LaserScan
            // Width 12 matches the hookshot's full 12px hitbox width.
            // 5 sample rays + min selection catches wall-top corners that
            // fewer/narrower rays would miss.
            float[] laserScanResults = new float[NumSamplePoints];
            Collision.LaserScan(origin, dir, 12f, maxRange, laserScanResults);

            // Take the minimum sample, but ignore hits closer than 20px —
            // those are the floor/wall tiles at the player's feet that the
            // hookshot clears on its first update (speed 28 × extraUpdates 2).
            const float MinHitThreshold = 20f;
            float tileHitDist = maxRange;
            for (int i = 0; i < laserScanResults.Length; i++)
            {
                if (laserScanResults[i] >= MinHitThreshold && laserScanResults[i] < tileHitDist)
                    tileHitDist = laserScanResults[i];
            }
            // If ALL samples were below the threshold, fall back to the
            // smallest one so the beam doesn't pass through a wall at
            // point-blank range.
            if (tileHitDist >= maxRange)
            {
                for (int i = 0; i < laserScanResults.Length; i++)
                {
                    if (laserScanResults[i] < tileHitDist)
                        tileHitDist = laserScanResults[i];
                }
            }

            // Forgiveness: the hookshot is a fast 12×12 projectile with extraUpdates=2,
            // so it can clip past tile edges the precise ray detects. Push the hit
            // point forward slightly to match actual hookshot behavior.
            tileHitDist = Math.Min(tileHitDist + 6f, maxRange);

            float bestDist = tileHitDist;
            _hitNPC = false;
            _hitAnything = tileHitDist < maxRange - 1f;

            // 2. Platform detection via ray stepping
            float platformDist = ScanForPlatforms(origin, dir, bestDist);
            if (platformDist < bestDist)
            {
                bestDist = platformDist;
                _hitAnything = true;
            }

            // 3. NPC hitbox detection via ray-AABB
            float npcDist = ScanForNPCs(origin, dir, bestDist);
            if (npcDist < bestDist)
            {
                bestDist = npcDist;
                _hitNPC = true;
                _hitAnything = true;
            }

            return bestDist;
        }

        private static float ScanForPlatforms(Vector2 origin, Vector2 dir, float maxDist)
        {
            // The hookshot grabs platforms from any direction (above, below,
            // or sideways), so scan unconditionally.

            // 16px step (one full tile) matches the hookshot's coarse collision
            // with extraUpdates=2 at speed 28. Finer steps cause false positives.
            const float step = 16f;
            int steps = (int)(maxDist / step);

            for (int i = 1; i <= steps; i++)
            {
                Vector2 pos = origin + dir * (i * step);

                int tileX = (int)(pos.X / 16f);
                int tileY = (int)(pos.Y / 16f);

                if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
                    continue;

                Tile tile = Main.tile[tileX, tileY];
                if (tile.HasTile && Main.tileSolidTop[tile.TileType])
                    return i * step;
            }

            return maxDist;
        }

        private static float ScanForNPCs(Vector2 origin, Vector2 dir, float maxDist)
        {
            float closest = maxDist;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float roughDist = Vector2.Distance(origin, npc.Center);
                if (roughDist > maxDist + 200f)
                    continue;

                float hitDist = RayAABBIntersect(origin, dir, npc.Hitbox);
                if (hitDist >= 0f && hitDist < closest)
                    closest = hitDist;
            }

            return closest;
        }

        private static float RayAABBIntersect(Vector2 origin, Vector2 dir, Rectangle aabb)
        {
            float tMin = float.NegativeInfinity;
            float tMax = float.PositiveInfinity;

            if (Math.Abs(dir.X) < 1e-8f)
            {
                if (origin.X < aabb.Left || origin.X > aabb.Right)
                    return -1f;
            }
            else
            {
                float invD = 1f / dir.X;
                float t1 = (aabb.Left - origin.X) * invD;
                float t2 = (aabb.Right - origin.X) * invD;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                if (tMin > tMax) return -1f;
            }

            if (Math.Abs(dir.Y) < 1e-8f)
            {
                if (origin.Y < aabb.Top || origin.Y > aabb.Bottom)
                    return -1f;
            }
            else
            {
                float invD = 1f / dir.Y;
                float t1 = (aabb.Top - origin.Y) * invD;
                float t2 = (aabb.Bottom - origin.Y) * invD;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                if (tMin > tMax) return -1f;
            }

            if (tMax < 0f) return -1f;
            return tMin >= 0f ? tMin : 0f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Only the owner sees the cursor indicator
            if (Main.myPlayer != Projectile.owner)
                return false;

            // Nothing hit means nothing to show at the cursor
            if (!_hitAnything)
                return false;

            // Draw the indicator at the cursor position — HookshotMark for NPCs, HookshotDot for tiles/platforms
            Vector2 drawScreen = Main.MouseScreen;
            string path = _hitNPC ? "SariaMod/Items/Bands/HookshotMark" : "SariaMod/Items/Bands/HookshotDot";
            Texture2D tex = ModContent.Request<Texture2D>(path).Value;
            if (tex != null)
            {
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
                Main.spriteBatch.Draw(tex, drawScreen, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
