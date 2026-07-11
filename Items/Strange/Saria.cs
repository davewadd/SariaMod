using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using SariaMod.Items.zTalking;
using SariaMod.Items.Strange;
using Terraria.Localization;
using System;
using Terraria.Map;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using SariaMod.Diagnostics;
using SariaMod.Netcode.SariaSoundSync;
using ReLogic.Utilities;
namespace SariaMod.Items.Strange
{
    public enum MoodState { Normal = 0, Happy = 6, Sad = -6, Angry = -12, Cursed = -18 }

    public partial class Saria : ModProjectile
    {
        public const float DistanceToCheck = 1100f;
        public int Transform; //used for when saria changes forms
        public int Mood; // Saria's mood, can effect textures for her faces
        private int MoveTimer; // used to help calculate if she can move or not
        public int MoveTimerValue => MoveTimer;
        private int SleepHeal;
        public bool Sleep; // is Saria asleep? effects visuals and movement as well as enemy detection
        private bool XpTimer; // the short buff that shows the xp bar
        private bool Cursed; //is saria currently curse? effects buffs, visuals
        public bool Follow; // true when Cursed; drives the cursed movement logic
        private bool _linkCableFollow; // true when LinkCable is active and a marker is placed; mirrors Follow-path logic without cursed side-effects
        public int ChannelTime; //time the player actually channels
        public int BiomeTime; //short downtime from biome weakness reset
        public int ChannelState; //used for when she is actually using charge animation
        public int IsCharging; //tells when saria is actually in the charging animation state
        public int ChannelAttack;
        public int Eating; // is Saria Eating?
        public int ChangeForm; // is the Projectile UI that is used for changing forms
        private bool Holding; // checks whether the player is holding sarias confect or frozen yogurt
        public int CanMove; // value to help set whether saria can move or not;
        private int SpecialAnimate;//for things like electric Saria's Electric mask animation
        public int SpecialAnimateValue => SpecialAnimate;
        public int SoundTimer;
        public int SoundTimer2;
        public bool SelectSound; // sound that plays when cursor is over saria when selecting move
        private bool IsPlayerAsleep; // checks if the player is asleep to let saria know if she should sleep too
        public int CantAttackTimer;//used to time how long she cant attack for
        public int FlashCooldownTimer; // replaces FlashCooldown projectile (1800 ticks = 30s)
        public bool SariaTalking;//used to tell if Saria is Talking
        private bool CantAttack;// used for when she should not be able to attack between 0 and 1
        private bool Sneezing; // true while the sneeze animation (frames 25-35) is playing
        private bool BloodSneeze; // snapshot: true if StatLower was active when forced sneeze started
        public bool BloodSneezeValue => BloodSneeze;
        public bool CursedValue      => Cursed;
        private const int ShortChannelThreshold = 20;
        private const int BubbleFaceMaxDuration = 350;

        // Distance at which Saria enters/exits the cursed "separated" state.
        // Change this one value and both the AI threshold AND the debug ring update automatically.
        private const float CursedSeparationRadius = 120f;
        // Within this radius of the player, ground-probe corrections are active.
        // Outside it (non-cursed only) she floats freely through tiles.
        private const float TileCollisionRadius = 70f;
        public int frameToSync;
        public int directionToSync;
        public int syncedFrameCounter;
        public bool LegsIsCasual;
        public bool LegsGoingToCasual;
        public bool LegsIsProper;
        public bool LegsGoingToProper;
        public bool ArmsIsDown;
        public bool ArmsGoingUp;
        public bool ArmsIsUp;
        public bool ArmsGoingDown;
        public bool EyesLooking;
        public bool EyesBlinking;
        public bool EyesOpening;
        public int DisplayedMoodSync;
        private SariaCloseTracker _closeTracker = new SariaCloseTracker(); // Wall-scan / idle-offset tracker — see SariaCloseTracker.cs
        private int _moodOverrideTimer;
        private int _moodOverrideTarget;
        private int _moodPriority; // current mood priority; SetMoodFor calls with lower priority are blocked
        public int MoodPriority => _moodPriority;
        public int SicknessBar = 12000; // wellness bar: max 12000, drained 20% per negative biome Period


        private const int SicknessBarMax = 12000;
        public int SicknessDecayChange;   // net change per 360 ticks; read by MeterBarUIState for tooltip
        private int _periodTimer = 0;     // ticks remaining on the current period countdown; 0 = dormant
        public int PeriodTimerValue { get => _periodTimer; set => _periodTimer = value; }
        public const int StatSoundCooldownMax = 7200; // 2 minutes at 60 fps
        private int _statRaiseSoundCooldown = 0; // ticks until StatRaise sound can play again
        private int _statLowerSoundCooldown = 0; // ticks until StatLower sound can play again
        public int StatRaiseSoundCooldown { get => _statRaiseSoundCooldown; set => _statRaiseSoundCooldown = value; }
        public int StatLowerSoundCooldown { get => _statLowerSoundCooldown; set => _statLowerSoundCooldown = value; }
        private int _sicknessDecayTimer;    // natural SicknessBar decay accumulator
        public SariaIdleAnimator IdleAnimator = new SariaIdleAnimator();

        // Cursed separation state.
        // _cursedSeparated    — true while Saria is outside CursedSeparationRadius or cannot see idle position.
        // _cursedSpeedScale   — [0, 1] multiplier; ramps up when not separated, decays to 0 when separated.
        private bool  _cursedSeparated  = false;
        private float _cursedSpeedScale = 1f;

        // Follow trail breadcrumb dots.
        // _followTrailDots       — world-space positions + permanent assigned number, dropped every 100 tiles; max 60.
        // _followTrailDistAccum  — running cumulative distance since the last dot was placed.
        // _followTrailLastPos    — player position from the previous tick, used to measure travel delta.
        // _followTrailNextNumber — next label number to assign; increments each dot, resets to 1 when Follow turns off.
        // _playerVisibleToSaria  — true when player is within 500 units AND has clear LoS to Saria; falling edge drops dot #1.
        // _followMarkedPosition  — world-space position of the currently targeted dot; Vector2.Zero = no target.
        private readonly System.Collections.Generic.List<(Vector2 Position, int Number)> _followTrailDots = new();
        private float   _followTrailDistAccum   = 0f;
        private Vector2 _followTrailLastPos     = Vector2.Zero;
        private int     _followTrailNextNumber   = 1;
        private bool    _playerVisibleToSaria   = false;
        public  Vector2 _followMarkedPosition   = Vector2.Zero;
        private int     _followMarkedNumber     = -1;           // dot number of the currently targeted dot; -1 = none
        private const float FollowTrailInterval = 100f * 16f; // 100 tiles in world units
        private const int   FollowTrailMaxDots  = 30;
        private const float FollowMarkerRange   = 450f;       // world units — dot scan + player-visible suppression radius
        private const float FollowShortcutRange = 100f * 16f; // 100 tiles — shortcut lookahead to skip ahead in the trail

        // A* path to the marked location.
        // _followPath          — computed tile-center waypoints (start -> goal); synced for all clients to draw.
        // _followPathLastGoal  — marked position the current path was computed for; triggers recompute on change.
        // _followPathTimer     — owner-side throttle so A* runs at most every FollowPathRecalcTicks.
        // FollowPathAllowance  — A* search budget in tiles (max path length explored).
        // FollowPathFootprint* — player-sized collision footprint in tiles (width x height).
        private readonly System.Collections.Generic.List<Vector2> _followPath = new();
        private Vector2 _followPathLastGoal = Vector2.Zero;
        private int     _followPathTimer    = 0;
        private int     _followPathIndex    = 0;  // current waypoint Saria is walking toward
        private const float FollowPathAllowance       = 150f; // tiles — dot-to-dot A* budget
        private const float FollowPathPlayerAllowance  = 450f; // tiles — budget when pathing directly to player
        private const int   FollowPathRecalcTicks     = 20;   // owner-side recompute throttle
        private const int   FollowPathFootprintWidth  = 2;    // ceil(player.width  / 16) = ceil(20/16)
        private const int   FollowPathFootprintHeight = 3;    // ceil(player.height / 16) = ceil(42/16)
        // LinkCable arrival deadzone: once within this radius of the marker she stops
        // re-pathing so the ground probes can settle her with clearance (wiggle room),
        // instead of A* yanking her back onto the exact tile every tick. She re-engages
        // if knocked beyond it.
        private const float LinkCableArrivalDeadzone  = 48f;  // world px — 3 tiles of slack

        // Navigation reference point: aligns Saria's box bottom with how dots were placed.
        // Dots are at player.Center = player.position.Y + player.height/2 (21px above ground).
        // Saria's feet box bottom  = Projectile.position.Y + 78 (feet anchor).
        // Saria's equivalent ref   = box bottom - player.height/2 = position.Y + 78 - 21 = position.Y + 57.
        // Projectile.Center.Y      = position.Y + 39  (half of sprite height 78).
        // So SariaNavRef.Y         = Center.Y + 18.
        private const float SariaNavRefOffsetY = 18f; // world pixels above Projectile.Center.Y
        private Vector2 SariaNavRef => new Vector2(Projectile.Center.X, Projectile.Center.Y + SariaNavRefOffsetY);

        // Hysteresis gate for ground/wall probe activation.
        // Probe turns ON  when distance to player <= TileCollisionRadius.
        // Probe turns OFF when distance > TileCollisionRadius + TileProbeHysteresis.
        // The dead-band prevents the rapid on/off toggle that caused boundary jitter.
        private const float TileProbeHysteresis = 20f;
        private bool ProbesActive = true;
        private bool    _inWall             = false;
        private float   _dbgOverallCoverage  = 0f;  // last body-fit coverage ratio (0-1)
        private float   _dbgOrangeCoverage   = 0f;  // last orange-box coverage ratio (0-1)
        private Vector2 _inWallEscapeTarget = Vector2.Zero; // nearest open footprint when InWall; Zero = none
        private int     _inWallStuckTimer   = 0;            // ticks _inWall has been continuously true; triggers teleport at threshold
        private int     _inWallTeleportTimer = 0;           // counts down 30→0; positive = teleport phase active (target locked)
        private const int InWallStuckThreshold  = 60;       // ticks stuck before teleport phase begins
        private const int InWallTeleportDuration = 30;      // ticks of teleport wind-up (~0.5 second)

        // Path-blocked teleport: fires when A* cannot find a path to the goal.
        private int     _pathTeleportTimer   = 0;           // counts down PathTeleportDuration→0; positive = path-teleport wind-up active
        private Vector2 _pathTeleportTarget  = Vector2.Zero;// destination found by reverse-A* from marked location; Zero = use dot directly
        private const int PathTeleportDuration  = 300;      // 5-second wind-up so player can see destination
        private const float PathTeleportDirectTiles = 250f; // beyond this tile distance, skip reverse A* and teleport directly

        // Idle far-teleport: fires when !Follow && !Cursed and idle position is > 2000 units away.
        private int     _idleTeleportTimer  = 0;            // counts down IdleTeleportDuration→0; positive = idle teleport wind-up active
        private Vector2 _idleTeleportTarget = Vector2.Zero; // idle position snapshot locked at wind-up start
        private const int   IdleTeleportDuration  = 120;   // 2-second wind-up
        private const float IdleTeleportThreshold = 2000f; // world units before teleport replaces flying

        private Vector2 _debugIdlePosition;
        private Vector2 _lockedIdlePosition; // frozen idle position while FollowSight is latched

        // Ground-probe debug state — cached each AI tick, read in PostDraw.
        private bool _dbgHitboxInTile;
        private bool _dbgGroundTouching;
        private bool _dbgTileBelow;
        private bool _dbgWallLeft;
        private bool _dbgWallRight;
        private bool _wallPausedLeft;   // GreenPause: inner Green line of left wall detector is solid
        private bool _wallPausedRight;  // GreenPause: inner Green line of right wall detector is solid
        private bool _wasGroundedLastFrame;
        private bool _wasOnPressurePlateLastFrame; // rising-edge guard, independent of solid-tile grounded state
        private readonly System.Collections.Generic.HashSet<Point> _sariaOpenedDoors = new(); // tiles Saria opened; auto-closed when she leaves
        private bool _dbgOutOfBounds;
        private bool _followSight;

        // Public accessors for the debug UI panel.
        // Each DbgDet* property reflects whether that detector's Pink zone is firing (detector is active).
        public bool DbgDetDown          => _detectorResults[0].Pink;   // [0] Feet / down
        public bool DbgDetDownGreen     => _detectorResults[0].Green;
        public bool DbgDetDownYellow    => _detectorResults[0].Yellow && !_detectorResults[0].YellowSuppressed;
        public bool DbgDetUp            => _detectorResults[1].Pink;   // [1] Head / up
        public bool DbgDetUpGreen       => _detectorResults[1].Green;
        public bool DbgDetLeft          => _detectorResults[2].Pink;   // [2] Wall left
        public bool DbgDetLeftGreen     => _detectorResults[2].Green;
        public bool DbgDetRight         => _detectorResults[3].Pink;   // [3] Wall right
        public bool DbgDetRightGreen    => _detectorResults[3].Green;
        public bool DbgProbesActive => ProbesActive;
        public bool  InWall               => _inWall;
        public float DbgOverallCoverage   => _dbgOverallCoverage;
        public float DbgOrangeCoverage    => _dbgOrangeCoverage;
        public bool DbgOutOfBounds       => _dbgOutOfBounds;
        public bool FollowSight          => _followSight;
        public bool CursedSeparated       => _cursedSeparated;
        public bool LinkCableFollowActive  => _linkCableFollow;
        public int  FollowTrailDotCount  => _followTrailDots.Count;

        // Reusable ground/wall detector system.
        // [0] Feet  — down,  priority 2, no group,      green+pink active, acceptTopSurfaces
        // [1] Head  — up,    priority 1, no group,      green+pink active
        // [2] WallL — left,  priority 4, cancelGroup 1, pink-only active (Green still corrects velocity but doesn't suppress others)
        // [3] WallR — right, priority 4, cancelGroup 1, pink-only active (Green still corrects velocity but doesn't suppress others)
        private static readonly SariaDetectorConfig[] _detectorConfigs = new SariaDetectorConfig[]
        {
            // [0] Feet  — down,  priority 2, no group,      green+pink active, yellow, acceptTopSurfaces, excludeFurnitureTops, yellowPriority 1
            new SariaDetectorConfig(new Vector2(20f, 78f),  0f,   14, 4, 8,  3, 0, true, 24, true, true, true, yellowPriority: 1, flipWithDirection: true),
            // [1] Head  — up,    priority 1, no group,      green+pink active, yellow
            new SariaDetectorConfig(new Vector2(20f,  10f),  180f, 14, 4, 8,  2, 0, false, 24, true, false, flipWithDirection: true),
            // [2] WallL — left,  priority 4, cancelGroup 1, pink-only active, greenPause
            new SariaDetectorConfig(new Vector2(9f, 36f),  270f, 10, 4, 11,  4, 1, false, 24, false, false, greenPause: true),
            // [3] WallR — right, priority 4, cancelGroup 1, pink-only active, greenPause
            new SariaDetectorConfig(new Vector2(39f, 36f),  90f,  10, 4, 11,  4, 1, false, 24, false, false, greenPause: true),
        };
        private readonly SariaDetectorResult[] _detectorResults = new SariaDetectorResult[4];

        private bool _smileActive;
        // Locked to true after the looking interaction fires; cleared only when she
        // actually moves (MoveTimer resets to 0). Prevents the interaction from
        // repeating while she is standing in the same spot.
        private bool _smileInteractionUsed;

        // True while the idle "smile at player" face is being shown.
        // Replaces the SmileTime projectile that tracked the same condition.
        public bool SmileActive          { get => _smileActive;            set => _smileActive            = value; }
        public bool SmileInteractionUsed { get => _smileInteractionUsed;   set => _smileInteractionUsed   = value; }

        // ----- Smile interaction state machine -----
        // _smileInteractionActive: true from the moment the smile fires until it ends (mood expires or anger triggers)
        // _smileLockedUntilRoamReset: interaction already fired this roam session; no re-trigger until roam ends+restarts
        // _playerHasLookedAway: player faced same direction as Saria (turned away) at least once this roam session
        // _smileAngerTimer: ticks the player has been facing away while an interaction is active (90 = 1.5 seconds)
        // _wasEyeRoaming: previous-tick eye free mode state for edge detection
        // _playerStandingTimer: ticks the player has been holding no movement keys (must reach 60 before trigger fires)
        private bool _smileInteractionActive;
        private bool _smileLockedUntilRoamReset;
        private bool _playerHasLookedAway;
        private int  _smileAngerTimer;
        private bool _wasEyeRoaming;
        private int  _playerStandingTimer;

        public bool SmileInteractionActive      { get => _smileInteractionActive;     set => _smileInteractionActive     = value; }
        public bool SmileLockedUntilRoamReset   { get => _smileLockedUntilRoamReset;  set => _smileLockedUntilRoamReset  = value; }
        public bool PlayerHasLookedAway         { get => _playerHasLookedAway;        set => _playerHasLookedAway        = value; }
        public int  SmileAngerTimer             { get => _smileAngerTimer;            set => _smileAngerTimer            = value; }
        public bool WasEyeRoaming               { get => _wasEyeRoaming;             set => _wasEyeRoaming              = value; }
        public int  PlayerStandingTimer         { get => _playerStandingTimer;        set => _playerStandingTimer        = value; }

        // ----- Sound throttling -----
        // Animation conditions in AI fire on every tick the condition holds true,
        // not just on transitions. Without throttling, that produced ~1000+ packets/s
        // for movement sounds (Step/Hover/Fly). These per-SariaSoundId cooldowns cap
        // the rate to roughly the natural sound length so remote clients still hear
        // a continuous loop while bandwidth stays sane. Indexed by (int)SariaSoundId.
        // 0 = no cooldown (unused slot). Values are in game ticks (60 ticks = 1s).
        // Last GameUpdateCount this projectile played each sound id locally / sent it
        // over the network. Tracked separately so we can keep local playback in sync
        // with what remote clients will hear.

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mother");
            Main.projFrames[Projectile.type] = 99;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
            ProjectileID.Sets.MinionShot[Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode == NetmodeID.Server
                && !PsychicFieldSystem.IsSariaSpawnWithinTeamCap(Projectile.owner))
            {
                Projectile.Kill();
            }
        }

        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return target.CanBeChasedBy(Projectile);
        }
        public override bool MinionContactDamage()
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            NPC target = Projectile.Center.MinionHoming(500f, player);
            if (target != null && !Sleep)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        { // Caching the player and modPlayer can make the code slightly cleaner.
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            // The logic to set hitDirection is separate from buffs and can be handled first.
            // Use a ternary operator for a more compact way to write this if/else.
            hitDirection = (player.position.X + (float)(player.width / 2) < Projectile.position.X + (float)(Projectile.width / 2)) ? 1 : -1;
            // Apply standard debuff immunities.
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
            modPlayer.SariaXp += 2;
            // Use a switch statement on the 'Transform' variable for clarity.
            switch (Transform)
            {
                case 0:
                    target.AddBuff(ModContent.BuffType<SariaCurse2>(), 200);
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity, ModContent.ProjectileType<LocatorHitVisual>(), 0, 0f, Projectile.owner);
                    break;
                case 1:
                    target.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
                    target.AddBuff(ModContent.BuffType<Frostburn2>(), 200);
                    break;
                case 2:
                    target.buffImmune[ModContent.BuffType<Burning2>()] = false;
                    target.AddBuff(ModContent.BuffType<Burning2>(), 200);
                    break;
                // Cases 3 and 4 are identical, so they can be combined.
                case 3:
                case 4:
                    target.AddBuff(BuffID.Electrified, 300);
                    break;
                case 5:
                    target.AddBuff(BuffID.Venom, 300);
                    target.AddBuff(BuffID.Poisoned, 300);
                    break;
                case 6:
                    target.buffImmune[ModContent.BuffType<SariaCurse>()] = false;
                    target.AddBuff(ModContent.BuffType<SariaCurse>(), 2000);
                    if (!player.HasBuff(ModContent.BuffType<StatLower>()))
                    {
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.position.X + 10, target.position.Y + 2, 0, 0, ModContent.ProjectileType<ShadowClaw>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                        }
                    }
                    break;
                // Default case is optional but good practice to handle unexpected values.
                default:
                    break;
            }
        }
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 78;
            Projectile.hide = false;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 50;
            Projectile.minionSlots = 0f;
            Projectile.timeLeft = 10;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.minion = true;
        }
        private const int sphereRadius3 = 1;
        private const int sphereRadius2 = 6;
        private const int sphereRadius4 = 32;
        private const int sphereRadius = 100;


        private void UpdateChannelTime(Player player, FairyPlayer modPlayer)
        {
            // Note: The logic `ChannelTime < 18` is less than the `ShortChannelThreshold` of 20,
            // so this condition will only be true for a small initial window. This behavior
            // is preserved from your original code.
            if (!modPlayer.PlayercanCharge && ChannelTime < 18)
            {
                ChannelTime++;
            }
            else if (modPlayer.PlayercanCharge)
            {
                ChannelTime++;
                Projectile.netUpdate = true;
            }
        }
        private void SpawnTransformUI(Player player)
        {
            // Ensure only the owner of the projectile can spawn the UI
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.position.X,
                    Projectile.position.Y - 24,
                    0, 0,
                    ModContent.ProjectileType<Transform>(),
                    (int)Projectile.damage,
                    0f,
                    Projectile.owner,
                    player.whoAmI,
                    Projectile.whoAmI
                );
            }
        }
        private void HandleChannelRelease(Player player, bool NotActive)
        {
            int veilBubbleType = ModContent.ProjectileType<Transform>();
            // This loop iterates through all projectiles to find the Transform UI.
            // This is kept identical to your original code to preserve functionality.
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                if (Main.projectile[i].active && i != Projectile.whoAmI && ChannelTime > 0 && (Main.projectile[i].type == veilBubbleType && Main.projectile[i].owner == Projectile.owner))
                {
                    if (ChannelTime <= ShortChannelThreshold)
                    {
                        // Handle early channel release (form change)
                        if (CantAttackTimer > 0 && Projectile.frame >= 44 && Projectile.frame < 56)
                        {
                            Projectile.frame = 56;
                        }
                        else if (ChangeForm <= 0)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/OptionsUp"), player.Center);
                            ChangeForm = 1;
                        }
                    }
                    else if (ChannelTime > ShortChannelThreshold && NotActive)
                    {
                        // Handle charged attack and cooldown
                        ChannelState = 0;
                        // Condition derived from original `if (CanChanneltoBeginWith)` check
                        if (ChannelTime > ShortChannelThreshold && NotActive && !SariaTalking && !IsTransforming)
                        {
                            ChannelAttack = 1;
                            Projectile.ai[0] = 1;
                        }
                    }
                    else
                    {
                        // Handle other release scenarios (e.g., in a non-active state)
                        ChannelState = 0;
                    }
                    // Actions common to all release scenarios
                    Main.projectile[i].Kill();
                    Projectile.netUpdate = true;
                    ChannelTime = 0;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile.BlueRingofdust(72);
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 15, Projectile.position.Y + 30, 0, 0, ModContent.ProjectileType<HealBallProjectile2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
        }
    }
}
