using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Tracks Saria's "close" distance to the player and handles all wall-scan /
    /// idle-offset logic that was previously scattered through <see cref="Saria.AI"/>.
    ///
    /// <para>
    /// Two pieces of state are managed here:
    /// <list type="bullet">
    ///   <item><description><see cref="CachedClose"/> — the physical pixel distance used to
    ///     position Saria beside the player.  Starts at <c>60f</c> and is clamped inward
    ///     whenever a tile wall is detected.</description></item>
    ///   <item><description><see cref="StabilizeTimer"/> — hold-off counter (in ticks) that
    ///     prevents <see cref="CachedClose"/> from easing outward too quickly after a
    ///     wall-snap.</description></item>
    /// </list>
    /// </para>
    /// <para>The offset always uses <c>player.direction</c> directly every frame — no
    /// direction caching — so turning around immediately moves Saria's target to the
    /// correct side.</para>
    /// </summary>
    public struct SariaCloseTracker
    {
        // ── Persisted state ──────────────────────────────────────────────────────

        /// <summary>Physical pixel distance from the player used for the idle X offset.</summary>
        public float CachedClose;

        /// <summary>Ticks remaining on the outward-ease hold-off after a wall snap.</summary>
        public int StabilizeTimer;

        // Keep CachedDir field so existing SendExtraAI/ReceiveExtraAI callers compile,
        // but it is no longer used by any logic here.
        /// <summary>Unused — retained for binary compatibility only.</summary>
        public int CachedDir;

        // ── Constructor ──────────────────────────────────────────────────────────

        /// <summary>Initialises the tracker with its default values.</summary>
        public SariaCloseTracker()
        {
            CachedClose    = 60f;
            StabilizeTimer = 0;
            CachedDir      = 1;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Runs the wall-scan logic for this tick and returns the X offset to add to
        /// <c>idlePosition</c>.
        ///
        /// <para>Call once per AI tick, <em>inside</em> the
        /// <c>if (Eating &lt;= 0 &amp;&amp; !Sleep)</c> guard, after <c>Close</c> has been
        /// set for the current item/health state and after the
        /// <c>spikeActive</c> flag has been evaluated.</para>
        /// </summary>
        /// <param name="projectile">Saria's projectile (used for minionPos and CalcWallSafeClose).</param>
        /// <param name="player">Saria's owner player.</param>
        /// <param name="close">The logical close distance determined by the current item / health state.</param>
        /// <param name="spikeActive">True while any Emerald spike projectile is active — forces CachedClose to 0.</param>
        /// <remarks>After calling this, use <see cref="GetOffsetX"/> to obtain the idle X offset.</remarks>
        public void Update(Projectile projectile, Player player, float close, bool spikeActive)
        {
            // ── 1. Spike override ────────────────────────────────────────────────
            if (spikeActive)
            {
                CachedClose    = 0f;
                StabilizeTimer = 50;
                CachedDir      = player.direction;
            }
            else
            {
                // ── 2. Direction change — immediately use full close on new side ─
                // CachedDir tracks the last known direction solely to detect flips.
                if (player.direction != CachedDir)
                {
                    CachedClose    = close;
                    StabilizeTimer = 0;
                    CachedDir      = player.direction;
                }
                else
                {
                    // Always tile-scan every tick so idle position never enters geometry,
                    // regardless of whether the player is moving or standing still.
                    float wallSafe = projectile.CalcWallSafeClose(close);

                    if (wallSafe < CachedClose)
                    {
                        CachedClose    = wallSafe;
                        StabilizeTimer = 50;
                    }
                    else if (wallSafe >= close)
                    {
                        // No tile obstruction in range — snap to full close immediately.
                        // Covers item-switch (Close increases) and open-space cases.
                        CachedClose    = close;
                        StabilizeTimer = 0;
                    }
                    else if (wallSafe > CachedClose && StabilizeTimer <= 0)
                    {
                        CachedClose = MathHelper.Lerp(CachedClose, wallSafe, 0.06f);
                    }
                }
            }

            // ── 3. Decrement hold timer every tick ───────────────────────────────
            if (StabilizeTimer > 0)
                StabilizeTimer--;

            // ── 4. Physical close never exceeds logical close ────────────────────
            CachedClose = Math.Min(CachedClose, close);
        }

            /// <summary>
            /// Returns the X pixel offset to add to the idle position this tick.
            /// Call this <em>after</em> <see cref="Update"/> so the state is already correct.
            /// </summary>
            public float GetOffsetX(Projectile projectile, Player player, float close)
            {
                return ComputeOffsetX(projectile, player, close);
            }

        /// <summary>
        /// Reinforces the hold timer after <see cref="SariaDetector.Apply"/> has run.
        ///
        /// <para>If the physics detector confirms a wall on Saria's idle side, the hold
        /// timer is bumped up so <see cref="CachedClose"/> doesn't ease outward into the
        /// tile on the next tick.</para>
        ///
        /// <para>Call once per AI tick, immediately after <c>SariaDetector.Apply</c> and
        /// the debug-cache lines that read <c>_dbgWallLeft</c> / <c>_dbgWallRight</c>.</para>
        /// </summary>
        /// <param name="wallLeft">True if the left wall detector is firing (Pink zone active).</param>
        /// <param name="wallRight">True if the right wall detector is firing (Pink zone active).</param>
        /// <param name="playerDir">Current <c>player.direction</c> (+1 = right, -1 = left).</param>
        public void ReinforceFromWall(bool wallLeft, bool wallRight, int playerDir, float actualDistToPlayer)
        {
            bool wallOnIdleSide = (playerDir > 0 && wallRight) || (playerDir < 0 && wallLeft);
            if (wallOnIdleSide)
            {
                // Physical contact confirmed — clamp CachedClose to actual separation so the
                // idle target never gets placed inside the tile (catches single-tile door frames
                // that the tile-scan 2-tile check misses).
                if (actualDistToPlayer < CachedClose)
                    CachedClose = actualDistToPlayer;
                if (StabilizeTimer < 30)
                    StabilizeTimer = 30;
            }
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Computes the X pixel offset that positions Saria beside the player this tick.
        ///
        /// <para>Accounts for the wall-constraint frozen-behind-wall case: while Saria
        /// is tucked against a wall, the player is standing still, and the player has
        /// turned in place (but not physically moved), the offset is kept on the original
        /// constrained side so Saria doesn't jump through the wall.</para>
        /// </summary>
        private float ComputeOffsetX(Projectile projectile, Player player, float close)
        {
            float directionBias = player.direction == -1 ? 4f : 0f;
            return ((CachedClose + projectile.minionPos / 80) * player.direction) + directionBias;
        }
    }
}
