using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod
{
    /// <summary>
    /// While camera-on-Saria mode is active (holding Heal Ball with Link Cable engaged),
    /// makes the LOCAL player's perceived biome — zone flags, modded biomes, music and
    /// backgrounds — match Saria's actual location EXCLUSIVELY, replacing the player's
    /// real biome rather than blending with it.
    ///
    /// HOW (and why the old "mirror Saria's snapshot in PostUpdate" approach failed):
    /// In Terraria 1.4.3's frame order the three consumers of biome state sample it
    /// BEFORE ModPlayer.PostUpdate ever runs, and all lean on the player's REAL position:
    ///
    ///  1. Main.UpdateAudio() — start of frame. Vanilla gates most biome music by the
    ///     local player's real position.Y (underworld, underground, mushroom-depth
    ///     variants...), and tModLoader resolves ModSceneEffect music here. Mirrored
    ///     flags can never fix this: the position itself is what's read. This is exactly
    ///     why hell/mushroom backgrounds changed (screen-based) but music never did.
    ///
    ///  2. Main.SceneMetrics.ScanAndExportToMain() — the global per-frame tile scan is
    ///     centred on LocalPlayer.Center, NOT the screen. Tile-count zones (jungle,
    ///     crimson, hallow, snow, desert, mushroom...) and the surface background styles
    ///     derived from them therefore kept reflecting the player's real surroundings
    ///     even with the camera parked on Saria.
    ///
    ///  3. Player.UpdateBiomes() — mid Player.Update(). Computes every zone flag from
    ///     scratch: tile-count zones from Main.SceneMetrics (fixed by hook 2), but depth
    ///     layers, beach, dungeon walls, space and ModBiome.IsBiomeActive(player) all
    ///     from the player's REAL position. A later PostUpdate mirror was always too
    ///     late for audio, and stomped these live values with a stale movement-gated
    ///     snapshot.
    ///
    /// So instead of copying flags around, we redirect the global scan centre and
    /// briefly relocate the local player onto Saria around exactly those calls
    /// (try/finally restore, the same battle-tested pattern as SariaSpawnSystem).
    /// Vanilla then computes ALL of it natively at Saria's spot — depth music, zone
    /// flags, purity, and modded biomes via IsBiomeActive — which automatically yields
    /// the "exclusive" semantics wanted: hallow tiles near the real player simply don't
    /// exist near Saria, so ZoneHallow turns off and only Saria's biome remains. All
    /// hooked calls are synchronous, single-threaded and run no physics/netcode, so the
    /// swap is externally unobservable.
    /// </summary>
    public class SariaPerceptionSystem : ModSystem
    {
        public override void Load()
        {
            On.Terraria.Player.UpdateBiomes += Hook_UpdateBiomes;
            On.Terraria.Main.UpdateAudio += Hook_UpdateAudio;
            On.Terraria.SceneMetrics.ScanAndExportToMain += Hook_ScanAndExportToMain;
            On.Terraria.Player.AddBuff += Hook_AddBuff;
        }

        public override void Unload()
        {
            On.Terraria.Player.UpdateBiomes -= Hook_UpdateBiomes;
            On.Terraria.Main.UpdateAudio -= Hook_UpdateAudio;
            On.Terraria.SceneMetrics.ScanAndExportToMain -= Hook_ScanAndExportToMain;
            On.Terraria.Player.AddBuff -= Hook_AddBuff;
        }

        /// <summary>
        /// Universal buff isolation while camera-on-Saria mode is active.
        ///
        /// Because the perception hooks make the LOCAL player's zone flags, modded biome
        /// flags and Main.SceneMetrics all describe SARIA's location, every buff system
        /// that keys off them — vanilla placed-item buffs (campfire / heart lantern /
        /// water candle near Saria), ModBiome.OnInBiome biome buffs, and any mod's
        /// ModPlayer code applying debuffs from zone flags (e.g. "in hell => burning") —
        /// would wrongly buff/debuff the player for a place they are not standing in.
        ///
        /// Rather than special-casing individual buffs (explicitly not wanted), gate the
        /// single chokepoint ALL of them must pass through: Player.AddBuff. While the
        /// mode is active the local player cannot GAIN a buff they do not already have;
        /// buffs they already have refresh normally and are never removed. This is
        /// buff-agnostic, covers vanilla and every mod, deletes nothing, and ends the
        /// instant the mode is toggled off. The ONLY exemptions are StatLower and
        /// StatRaise: those are Saria's own state-driven buffs and are meant to keep
        /// working while spectating her.
        ///
        /// Known trade-off: genuinely new buffs from the player's REAL surroundings are
        /// also deferred while spectating (their ambient sources are being perceived at
        /// Saria anyway — SceneMetrics points there), and quick-buff potions won't apply
        /// until the mode is released. Buffs active when the mode starts keep running.
        /// </summary>
        private static void Hook_AddBuff(On.Terraria.Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
        {
            if (self.whoAmI == Main.myPlayer
                && !Main.gameMenu
                && type != ModContent.BuffType<Buffs.StatLower>()
                && type != ModContent.BuffType<Buffs.StatRaise>())
            {
                // Saria-context windows: the Saria spawn pass physically relocates the
                // owner onto her AND rescans SceneMetrics there, and the camera-mode
                // global scan is redirected to her centre — any tile NearbyEffects fired
                // inside those windows describes HER surroundings. Block even REFRESHES
                // here: a candle standing next to Saria used to re-add the player's
                // expiring CorruptMind buff every tick (the relocated player "stood"
                // next to it during the pass), making the buff unremovable. Nothing
                // legitimate refreshes player buffs inside these two windows.
                bool sariaContext = InSariaContextScan
                    || SariaSpawnSystem.CurrentPass == SariaSpawnSystem.SpawnPass.Saria;

                // Outside those windows, camera mode only blocks buffs the player does
                // not already have — existing buffs keep refreshing from real sources.
                if (sariaContext || (!self.HasBuff(type) && TryGetLocalCameraSaria(out _)))
                {
                    return;
                }
            }

            orig(self, type, timeToAdd, quiet, foodHack);
        }

        /// <summary>
        /// Redirects the GLOBAL per-frame biome tile scan (the one exported to
        /// Main.SceneMetrics, normally centred on the real player) to Saria's centre, so
        /// tile-count zones and surface background styles resolve exclusively from her
        /// surroundings. Private scratch SceneMetrics instances (e.g. Saria's own
        /// UpdateSariaZones scan) pass through untouched.
        /// </summary>
        private static void Hook_ScanAndExportToMain(On.Terraria.SceneMetrics.orig_ScanAndExportToMain orig, SceneMetrics self, SceneMetricsScanSettings settings)
        {
            if (self == Main.SceneMetrics
                && settings.BiomeScanCenterPositionInWorld.HasValue
                && TryGetLocalCameraSaria(out Projectile saria))
            {
                settings.BiomeScanCenterPositionInWorld = saria.Center;

                // Tile NearbyEffects fired inside this redirected scan describe SARIA's
                // surroundings; flag the window so Hook_AddBuff can suppress buffs (and
                // refreshes) they try to give the far-away real player.
                InSariaContextScan = true;
                try
                {
                    orig(self, settings);
                }
                finally
                {
                    InSariaContextScan = false;
                }
                return;
            }

            orig(self, settings);
        }

        /// <summary>
        /// Recomputes the local player's biome state at Saria's position. Depth layers,
        /// beach, dungeon, space and every ModBiome.IsBiomeActive check resolve there;
        /// tile-count zones come from Main.SceneMetrics, which the camera shift already
        /// centres on Saria. Flags persist after the call (nothing recomputes them later
        /// in the frame), so draw-time backgrounds and next frame's music see them too.
        /// </summary>
        private static void Hook_UpdateBiomes(On.Terraria.Player.orig_UpdateBiomes orig, Player self)
        {
            if (self.whoAmI != Main.myPlayer || !TryGetLocalCameraSaria(out Projectile saria))
            {
                orig(self);
                return;
            }

            Vector2 savedPosition = self.position;
            self.position = saria.Center - new Vector2(self.width * 0.5f, self.height * 0.5f);
            try
            {
                orig(self);
            }
            finally
            {
                self.position = savedPosition;
            }
        }

        /// <summary>
        /// Runs the whole music/ambience decision with the local player standing at
        /// Saria, so vanilla's position.Y-gated tracks (underworld, underground,
        /// mushroom-depth...) and tModLoader's scene-effect music all resolve against
        /// her location. UpdateAudio only reads state — no physics or netcode — so the
        /// temporary swap cannot leak or be observed outside the call.
        /// </summary>
        private static void Hook_UpdateAudio(On.Terraria.Main.orig_UpdateAudio orig, Main self)
        {
            if (Main.gameMenu || !TryGetLocalCameraSaria(out Projectile saria))
            {
                orig(self);
                return;
            }

            Player player = Main.player[Main.myPlayer];
            Vector2 savedPosition = player.position;
            player.position = saria.Center - new Vector2(player.width * 0.5f, player.height * 0.5f);
            try
            {
                orig(self);
            }
            finally
            {
                player.position = savedPosition;
            }
        }

        /// <summary>
        /// When true, every perception hook passes through untouched. Set by
        /// SariaSpawnSystem around its spawn passes so it can recompute the owner's
        /// REAL-location biome state (player pass) and Saria's live state (Saria pass)
        /// without this system redirecting those recomputations back onto Saria.
        /// </summary>
        internal static bool BypassPerception;

        /// <summary>
        /// True while a Saria-centred tile scan is executing (the camera-mode redirected
        /// global scan). Together with the Saria spawn pass, these are the windows in
        /// which tile NearbyEffects describe HER surroundings, so Hook_AddBuff blocks
        /// even buff refreshes for the local player during them.
        /// </summary>
        internal static bool InSariaContextScan;

        /// <summary>
        /// True when the LOCAL player has camera-on-Saria mode active: holding the Heal
        /// Ball with Link Cable engaged and an owned Saria projectile present. Mirrors
        /// FairyPlayer.TryGetCameraSaria so perception and camera use one condition.
        /// </summary>
        internal static bool TryGetLocalCameraSaria(out Projectile sariaProj)
        {
            sariaProj = null;

            if (BypassPerception)
                return false;

            if (Main.netMode == NetmodeID.Server)
                return false;

            Player player = Main.player[Main.myPlayer];
            if (player == null || !player.active || player.dead)
                return false;
            if (player.HeldItem.type != ModContent.ItemType<Items.Strange.HealBall>())
                return false;
            if (!player.TryGetModPlayer(out FairyPlayer fp) || !fp.LinkCable)
                return false;

            int sariaType = ModContent.ProjectileType<Items.Strange.Saria>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Main.myPlayer && p.type == sariaType)
                {
                    sariaProj = p;
                    return true;
                }
            }
            return false;
        }
    }
}
