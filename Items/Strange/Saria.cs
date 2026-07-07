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

    public class Saria : ModProjectile
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
        // How long (in ticks) the transformation animation lasts before the form actually changes.
        // 180 ticks = 3 seconds at 60 fps. Change this one constant to adjust the duration.
        public const int TransformDuration = 180;
        // The form that will be applied when TransformTimer reaches zero. -1 = no pending transform.
        public int PendingTransform = -1;
        // Counts down from TransformDuration to 0 while a transformation is in progress.
        public int TransformTimer;
        // True while a transformation is actively counting down.
        public bool IsTransforming => TransformTimer > 0;

        // --- Transform visual effect draw-side state ---
        private const int TransformGrowTicks      = 45;  // ticks for sphere to grow to full size
        private const int TransformPopTicks        = 20;  // ticks for the pop burst to play after timer = 0
        private float _transformSphereScale        = 0f;
        private float _transformPulsePhase         = 0f;
        private float _transformWavePhase          = 0f;  // drives the liquid-edge ripple
        private int   _transformPopCountdown       = 0;
        private int   _prevTransformTimer          = -1;  // -1 = uninitialized; prevents false pop on first frame
        private int   _prevTransformTimerRemote     = -1;  // non-owner edge detection for sounds
        private int   _prevTeleportTimerRemote      = -1;  // non-owner edge detection for teleport sounds/visuals
        private SlotId _transformLoopSlot;                  // tracks the currently playing TransformLoop instance
        private int   _transformLoopAge             = -1;  // ticks since the loop last started; -1 = not playing
        private struct TransformGlob { public float Angle, Distance, SpawnDistance, Speed, MaxSize; }
        private readonly List<TransformGlob> _transformGlobs = new List<TransformGlob>();
        private struct TransformPillar { public float Angle, RotSpeed, Length, MaxLength, Width, Life, MaxLife; }
        private readonly List<TransformPillar> _transformPillars = new List<TransformPillar>();

        // --- Teleport visual effect state (pink sphere at source + destination) ---
        private float  _tpSphereScale  = 0f;
        private float  _tpPulsePhase   = 0f;
        private float  _tpWavePhase    = 0f;
        private int    _tpActiveDuration = 0;                // duration the current teleport wind-up was started with
        private SlotId _tpLoopSlot;                          // currently playing TransformLoop instance for teleport wind-up (source)
        private SlotId _tpDestLoopSlot;                       // currently playing TransformLoop instance at the destination
        private readonly List<TransformGlob>   _tpSourceGlobs   = new List<TransformGlob>();
        private readonly List<TransformPillar> _tpSourcePillars = new List<TransformPillar>();
        private readonly List<TransformGlob>   _tpDestGlobs     = new List<TransformGlob>();
        private readonly List<TransformPillar> _tpDestPillars   = new List<TransformPillar>();
        // Cached world positions at the moment a teleport wind-up starts on a non-owner client.
        // Needed because by the time tpJustEnded fires, the position netUpdate has already snapped
        // Projectile.Center to the destination and _inWallEscapeTarget has been zeroed.
        private Vector2 _tpCachedSrc  = Vector2.Zero;
        private Vector2 _tpCachedDest = Vector2.Zero;
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

        // --- Saria's own biome zone state (owner-computed via SceneMetrics, synced to all clients) ---
        // Biomes
        public bool SariaZoneSnow;
        public bool SariaZoneJungle;
        public bool SariaZoneCorrupt;
        public bool SariaZoneCrimson;
        public bool SariaZoneHallow;
        public bool SariaZoneDesert;
        public bool SariaZoneBeach;        // near world edge + surface + enough sand tiles
        public bool SariaZoneDungeon;       // enough dungeon brick tiles nearby
        public bool SariaZoneSandstorm;     // sandstorm event is active
        public bool SariaZoneUndergroundDesert; // desert tiles at underground depth
        public bool SariaZoneGlowingMushroom;
        public bool SariaZoneGraveyard;
        public bool SariaZoneMeteor;
        public bool SariaZoneForest;        // true when no other dominant biome is active
        // Depth layers (Y-position based, mirrors player Zone depth bands)
        public bool SariaZoneSkyHeight;
        public bool SariaZoneSpace;         // above sky — mirrors player.InSpace()
        public bool SariaZoneOverworld;     // surface layer
        public bool SariaZoneUnderground;   // below surface, above dirt layer
        public bool SariaZoneDirtLayer;     // dirt layer depth band
        public bool SariaZoneRockLayer;     // rock layer depth band
        public bool SariaZoneUnderworld;    // underworld / hell depth
        public bool SariaZoneRain;
        // Environment / nearby objects
        public bool SariaHasCampfire;
        public bool SariaHasHeartLantern;
        public bool SariaHasStarInBottle;
        public bool SariaHasWaterCandle;   // net positive water candles nearby
        public bool SariaHasPeaceCandle;   // net positive peace candles nearby
        public bool SariaHasCalmMindCandle;  // Calming Candle tile nearby
        public bool SariaHasReajCandle;      // Reaj Candle tile nearby

        // --- Biome scan movement gate ---
        // Re-scans only when Saria has moved at least one tile from the last scan position.
        // When she is stationary the zone flags from the last scan are kept as-is.
        private Vector2 _lastBiomeScanPos;
        private const float BiomeScanMoveThreshold = 16f; // 1 tile in world units

        // Modded biomes active at Saria's location.
        // Populated each scan by temporarily relocating the owner to Saria's center and
        // calling ModBiome.IsBiomeActive(player) for every registered ModBiome.
        // Read by SariaSpawnSystem to make the Saria NPC-spawn pass biome-correct.
        private readonly HashSet<int> _sariaActiveModBiomeTypes = new();
        public IReadOnlyCollection<int> SariaActiveModBiomeTypes => _sariaActiveModBiomeTypes;

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
        private static readonly int[] SoundCooldownTicks = new int[]
        {
            0,   // 0 unused
            18,  // 1 Hover  (~0.30s loop spacing)
            24,  // 2 Fly    (~0.40s loop spacing)
            10,  // 3 Step1  (~0.17s footstep spacing)
            10,  // 4 Step2
            30,  // 5 IceBarrier1 (one-shot, generous floor)
            30,  // 6 IceBarrier2
        };
        // Last GameUpdateCount this projectile played each sound id locally / sent it
        // over the network. Tracked separately so we can keep local playback in sync
        // with what remote clients will hear.
        private readonly int[] _lastSoundTick = new int[SoundCooldownTicks.Length];

        public void SetMoodFor(MoodState state, int durationTicks, int priority = 0)
        {
            if (priority < _moodPriority) return;
            bool moodChanging = (int)state != Mood;
            Mood = (int)state;
            _moodOverrideTarget = (int)state;
            _moodOverrideTimer = durationTicks;
            _moodPriority = priority;
            // Automatically show the matching bubble face for the same duration.
            // Owner-only: ShowBubbleFace writes to a local draw-side dictionary.
            if (Main.myPlayer == Projectile.owner)
            {
                var face = state switch
                {
                    MoodState.Happy  => SariaExtensions1.BubbleFaceType.Smile,
                    MoodState.Sad    => SariaExtensions1.BubbleFaceType.Sad,
                    MoodState.Angry  => SariaExtensions1.BubbleFaceType.Anger,
                    MoodState.Cursed => SariaExtensions1.BubbleFaceType.Cursed,
                    _                => SariaExtensions1.BubbleFaceType.None, // Normal clears face
                };
                Projectile.ShowBubbleFace(face, Math.Min(durationTicks, BubbleFaceMaxDuration));
                // Play a sound only when the mood actually changes state.
                if (moodChanging && state != MoodState.Normal)
                {
                    if (state == MoodState.Happy)
                        SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
                    else if (state == MoodState.Sad)
                        SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);
                    else if (state == MoodState.Angry)
                        SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);
                    else if (state == MoodState.Cursed)
                        SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);
                }
            }
            Projectile.netUpdate = true;
        }

        public void Sigh()
        {
            // Play a sigh sound (using Step2 as placeholder if specific sigh sound doesn't exist, or just a generic sound)
            // Assuming "SariaMod/Sounds/Sigh" might exist or using a fallback.
            // The prompt implies I should implement the trigger, I'll use a sound that fits or just a visual if sound is unknown.
            // Using "SariaMod/Sounds/Step2" as a placeholder or standard sound engine if specific not known.
            // Actually, I'll check if I can use a standard sound or if I should define it.
            // I'll use SoundID.Item1 (generic) or similar if I don't have a specific one, but let's try to be specific if possible.
            // I'll use a safe fallback.
            SoundEngine.PlaySound(SoundID.MenuClose, Projectile.Center);

            // Visual effect for sighing (e.g. small dust or emote)
            CombatText.NewText(Projectile.getRect(), Color.LightGray, "*Sigh*", true);
        }

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
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Transform);
            writer.Write(Mood);
            writer.Write(MoveTimer);
            writer.Write(SleepHeal);
            writer.Write(Sleep);
            writer.Write(Cursed);
            writer.Write(Follow);
            writer.Write(ChannelTime);
            writer.Write(ChannelState);
            writer.Write(IsCharging);
            writer.Write(ChannelAttack);
            writer.Write(Eating);
            writer.Write(ChangeForm);
            writer.Write(Holding);
            writer.Write(CanMove);
            writer.Write(SpecialAnimate);
            writer.Write(SoundTimer);
            writer.Write(SoundTimer2);
            writer.Write(SelectSound);
            writer.Write(IsPlayerAsleep);
            writer.Write(CantAttackTimer);
            writer.Write(SariaTalking);
            writer.Write(CantAttack);
            writer.Write(Sneezing);
            writer.Write(BloodSneeze);
            writer.Write(frameToSync);
            writer.Write(directionToSync);
            writer.Write(syncedFrameCounter);
            writer.Write(LegsIsCasual);
            writer.Write(LegsGoingToCasual);
            writer.Write(LegsIsProper);
            writer.Write(LegsGoingToProper);
            writer.Write(ArmsIsDown);
            writer.Write(ArmsGoingUp);
            writer.Write(ArmsIsUp);
            writer.Write(ArmsGoingDown);
            writer.Write(EyesLooking);
            writer.Write(EyesBlinking);
            writer.Write(EyesOpening);
            writer.Write(DisplayedMoodSync);
            IdleAnimator.Write(writer);
            writer.Write(FlashCooldownTimer);
            writer.Write(_moodOverrideTimer);
            writer.Write(_moodOverrideTarget);
            writer.Write(_moodPriority);
            writer.Write(SicknessBar);
            writer.Write(_smileActive);
            writer.Write(_smileInteractionUsed);
            writer.Write(_smileInteractionActive);
            writer.Write(_smileLockedUntilRoamReset);
            writer.Write(_playerHasLookedAway);
            writer.Write(_smileAngerTimer);
            writer.Write(_wasEyeRoaming);
            writer.Write(_playerStandingTimer);
            writer.Write(PendingTransform);
            writer.Write(TransformTimer);
            // Saria's own biome zones + depth + environment — packed into two ushorts (4 bytes)
            var (biomes, depthEnv) = PackSariaZones();
            writer.Write(biomes);
            writer.Write(depthEnv);
            writer.Write(_followMarkedPosition.X);
            writer.Write(_followMarkedPosition.Y);
            // A* path to the marked location — sent as compact tile-origin coords.
            int pathCount = _followPath.Count;
            if (pathCount > 255) pathCount = 255;
            writer.Write((byte)pathCount);
            for (int i = 0; i < pathCount; i++)
            {
                Vector2 c = _followPath[i];
                short ox = (short)Math.Round(c.X / 16f - FollowPathFootprintWidth  * 0.5f);
                short oy = (short)Math.Round(c.Y / 16f - FollowPathFootprintHeight * 0.5f);
                writer.Write(ox);
                writer.Write(oy);
            }
            writer.Write((byte)Math.Min(_followPathIndex, 254));
            // Teleport phase sync
            writer.Write(_inWallTeleportTimer);
            writer.Write(_tpActiveDuration);
            writer.Write(_inWallEscapeTarget.X);
            writer.Write(_inWallEscapeTarget.Y);
            writer.Write(_linkCableFollow);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Transform = reader.ReadInt32();
            Mood = reader.ReadInt32();
            MoveTimer = reader.ReadInt32();
            SleepHeal = reader.ReadInt32();
            Sleep = reader.ReadBoolean();
            Cursed = reader.ReadBoolean();
            Follow = reader.ReadBoolean();
            ChannelTime = reader.ReadInt32();
            ChannelState = reader.ReadInt32();
            IsCharging = reader.ReadInt32();
            ChannelAttack = reader.ReadInt32();
            Eating = reader.ReadInt32();
            ChangeForm = reader.ReadInt32();
            Holding = reader.ReadBoolean();
            CanMove = reader.ReadInt32();
            SpecialAnimate = reader.ReadInt32();
            SoundTimer = reader.ReadInt32();
            SoundTimer2 = reader.ReadInt32();
            SelectSound = reader.ReadBoolean();
            IsPlayerAsleep = reader.ReadBoolean();
            CantAttackTimer = reader.ReadInt32();
            SariaTalking = reader.ReadBoolean();
            CantAttack = reader.ReadBoolean();
            Sneezing = reader.ReadBoolean();
            BloodSneeze = reader.ReadBoolean();
            frameToSync = reader.ReadInt32();
            directionToSync = reader.ReadInt32();
            syncedFrameCounter = reader.ReadInt32();
            Projectile.frame = frameToSync;
            Projectile.frameCounter = syncedFrameCounter;
            Projectile.spriteDirection = directionToSync;
            LegsIsCasual      = reader.ReadBoolean();
            LegsGoingToCasual = reader.ReadBoolean();
            LegsIsProper      = reader.ReadBoolean();
            LegsGoingToProper = reader.ReadBoolean();
            ArmsIsDown    = reader.ReadBoolean();
            ArmsGoingUp   = reader.ReadBoolean();
            ArmsIsUp      = reader.ReadBoolean();
            ArmsGoingDown = reader.ReadBoolean();
            EyesLooking  = reader.ReadBoolean();
            EyesBlinking = reader.ReadBoolean();
            EyesOpening  = reader.ReadBoolean();
            DisplayedMoodSync = reader.ReadInt32();
            IdleAnimator.Read(reader);
            FlashCooldownTimer = reader.ReadInt32();
            _moodOverrideTimer = reader.ReadInt32();
            _moodOverrideTarget = reader.ReadInt32();
            _moodPriority = reader.ReadInt32();
            SicknessBar = reader.ReadInt32();
            _smileActive = reader.ReadBoolean();
            _smileInteractionUsed = reader.ReadBoolean();
            _smileInteractionActive = reader.ReadBoolean();
            _smileLockedUntilRoamReset = reader.ReadBoolean();
            _playerHasLookedAway = reader.ReadBoolean();
            _smileAngerTimer = reader.ReadInt32();
            _wasEyeRoaming = reader.ReadBoolean();
            _playerStandingTimer = reader.ReadInt32();
            PendingTransform = reader.ReadInt32();
            TransformTimer = reader.ReadInt32();
            // Saria's own biome zones + depth + environment — packed into two ushorts (4 bytes)
            UnpackSariaZones(reader.ReadUInt16(), reader.ReadUInt16());
            _followMarkedPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            // A* path to the marked location — rebuilt from compact tile-origin coords.
            int pathCount = reader.ReadByte();
            _followPath.Clear();
            for (int i = 0; i < pathCount; i++)
            {
                short ox = reader.ReadInt16();
                short oy = reader.ReadInt16();
                _followPath.Add(new Vector2(
                    (ox + FollowPathFootprintWidth  * 0.5f) * 16f,
                    (oy + FollowPathFootprintHeight * 0.5f) * 16f));
            }
            int syncedIndex = reader.ReadByte();
            _followPathIndex = Math.Min(syncedIndex, Math.Max(0, _followPath.Count - 1));
            // Teleport phase sync
            _inWallTeleportTimer = reader.ReadInt32();
            _tpActiveDuration    = reader.ReadInt32();
            _inWallEscapeTarget  = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            _linkCableFollow     = reader.ReadBoolean();
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
        public override void AI()
        {
            {
                Player player = Main.player[Projectile.owner];
                Player player2 = Main.LocalPlayer;
                FairyPlayer modPlayer = player.Fairy();
                FairyProjectile modprojectile = Projectile.Fairy();

                Rectangle movehitbox = Projectile.Hitbox;
                int owner = player.whoAmI;
                ///recharge effect
                if (CantAttack && CantAttackTimer <= 0 && !IsTransforming)
                {
                    Vector2 dustPosition = (Projectile.spriteDirection == 1) ? Projectile.Right : Projectile.Center;
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 dustspeed5 = Main.rand.NextVector2CircularEdge(1f, 1f) * -5;
                        Dust d = Dust.NewDustPerfect(dustPosition, ModContent.DustType<AbsorbPsychic>(), dustspeed5, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
                    CantAttack = false;
                }
                ///
                //////////////Transformation Timer
                ///
                if (TransformTimer > 0)
                {
                    TransformTimer--;

                    // All clients play the loop — each manages their own SlotId instance
                    if (Main.netMode != NetmodeID.Server)
                    {
                        bool isFirstTick = TransformTimer == TransformDuration - 1;
                        bool isLoopTick  = !isFirstTick && (TransformTimer % 181 == 0);
                        if (isFirstTick)
                        {
                            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                            if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound old))
                                old.Stop();
                            _transformLoopSlot = SoundEngine.PlaySound(
                                new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                                Projectile.Center);
                            _transformLoopAge = 0;
                        }
                        else if (isLoopTick)
                        {
                            if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound prev))
                                prev.Stop();
                            _transformLoopSlot = SoundEngine.PlaySound(
                                new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                                Projectile.Center);
                        }

                        // Track Saria's position every tick so the sound follows her
                        if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound active))
                        {
                            active.Position = Projectile.Center;
                            // Ramp from 0.5 → 1.0 over TransformDuration; stays at 1.0 after that
                            if (_transformLoopAge >= 0)
                            {
                                active.Volume = 0.5f + 0.5f * Math.Clamp(_transformLoopAge / (float)TransformDuration, 0f, 1f);
                                _transformLoopAge++;
                            }
                        }
                    }

                    if (TransformTimer == 0 && PendingTransform >= 0)
                    {
                        Transform = PendingTransform;
                        PendingTransform = -1;
                        BiomeTime = 100;
                        Projectile.netUpdate = true;

                        // Stop the loop and play the completion sting
                        if (Main.netMode != NetmodeID.Server)
                        {
                            if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound done))
                                done.Stop();
                            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                            _transformLoopAge = -1;
                        }
                    }
                }
                ///
                Projectile.SariaBaseDamage();
                Projectile.SariaBiomeEffectivness((int)BiomeTime, (int)Transform);

                // Extinguished visuals/audio — runs on all clients since Extinguished buff is synced.
                // Mood is also set here so all clients see the sad face reaction.
                if (Transform == 2 && player.HasBuff(ModContent.BuffType<Buffs.Extinguished>()) && Main.netMode != NetmodeID.Server)
                {
                    Projectile.SneezeDust(ModContent.DustType<Dusts.SmokeDust3>(), 20, 6, -10, 3, -12);
                    if (SoundTimer2 <= 0)
                    {
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/mist"), Projectile.Center);
                        SoundTimer2 += 200;
                    }
                }
                Projectile.SariaBubbleFaceSpawner((bool)Sleep, (int)CanMove, (bool)Cursed, (int)Mood);
                Projectile.damage /= 2;
                Projectile.knockBack = 10;
                bool newXpTimer = player.HasBuff(ModContent.BuffType<XPBuff>());
                if (XpTimer != newXpTimer)
                {
                    XpTimer = newXpTimer;
                    Projectile.netUpdate = true;
                }
                ///Channeling
                bool NotActive = Eating <= 0 && !IsPlayerAsleep && !Sleep;
                bool HoldingHealBall = player.HeldItem.type == ModContent.ItemType<HealBall>();
                bool HoldingHealBallInInventory = player.HasItem(ModContent.ItemType<HealBall>());
                bool CanChanneltoBeginWith = (ChannelTime > 20 && Eating <= 0 && !IsPlayerAsleep && !Sleep && !SariaTalking); /// if you only want her to attack after certain frames after charging edit this to match what frames you want to look for
                bool playerischanneling = (player.channel == true && HoldingHealBall && ChangeForm <= 0 && !SariaTalking && Main.myPlayer == Projectile.owner && !Main.mouseRight && !player.noItems);
                bool notActive = Eating <= 0 && !IsPlayerAsleep && !Sleep && !SariaTalking;
                bool holdingHealBall = player.HeldItem.type == ModContent.ItemType<HealBall>();
                bool canChanneltoBeginWith = (ChannelTime > ShortChannelThreshold && notActive);
                bool playerIsChanneling = (player.channel && holdingHealBall && ChangeForm <= 0 && !SariaTalking && Main.myPlayer == Projectile.owner && !Main.mouseRight && !player.noItems);
                // 1. Handle Channeling and Time Progression
                if (playerIsChanneling)
                {
                    UpdateChannelTime(player, modPlayer);
                }
                // 2. Spawn the Transform UI
                if (playerIsChanneling && player.ownedProjectileCounts[ModContent.ProjectileType<Transform>()] <= 0f && canChanneltoBeginWith)
                {
                    SpawnTransformUI(player);
                }
                // 3. Handle Channel Release and Actions
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Transform>()] > 0f && !player.channel)
                {
                    HandleChannelRelease(player, NotActive);
                }

                // --- CUTSCENE TRIGGERS ---
                if (Main.myPlayer == Projectile.owner)
                {
                    var tracker = player.GetModPlayer<SariaInteractionTrackerPlayer>();

                    // Hallow Cutscene Trigger
                    if (player.ZoneHallow)
                    {
                        // Add pending cutscene: ID="HallowIntro", Target="cutscene_hallow_intro", Button="Talk", Duration=5min, Condition="InHallow"
                        tracker.AddPendingCutscene("HallowIntro", "cutscene_hallow_intro", "Talk", 5.0, "InHallow");
                    }

                    // Zora Form (Transform 1) Trigger
                    // Trigger when entering Transform 1
                    if (Transform == 1)
                    {
                        // Add pending cutscene: ID="ZoraIntro", Target="cutscene_zora_intro", Button="Talk", Duration=5min, Condition="NotForm_1"
                        // Note: Condition is NotForm_1, so it won't be available while in Transform 1.
                        tracker.AddPendingCutscene("ZoraIntro", "cutscene_zora_intro", "Talk", 5.0, "NotForm_1");
                    }

                    // Example of Dependent Cutscene (Commented out)
                    // This would only trigger if "ZoraIntro" has been completed.
                    /*
                    if (Transform == 2)
                    {
                         tracker.AddPendingCutscene("FireIntro", "cutscene_fire_intro", "Talk", 5.0, "Completed_ZoraIntro");
                    }
                    */

                    }

                // Owner-side scans — run once per second (every 60 ticks)
                if (Main.myPlayer == Projectile.owner && Main.GameUpdateCount % 60 == 0)
                {
                    UpdateSariaZones();
                    InteractionManager.UpdateProximityChecks(Projectile);
                }

                /// ChangeForm stuff
                /// 
                if (ChangeForm >= 1 && !SariaTalking && player.ownedProjectileCounts[ModContent.ProjectileType<FormChangeOverlay>()] <= 0f)
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<FormChangeOverlay>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                }
                if (player.HeldItem.type != ModContent.ItemType<HealBall>() && CantAttackTimer < 100)
                {
                    CantAttackTimer = 100;
                }
                if (ChannelTime > 20 && NotActive && player.channel == true && HoldingHealBall && CantAttackTimer <= 0 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()) && !Main.mouseRight && !IsTransforming)
                {
                    ChannelState++;
                }
                {
                    if (ChannelState > 20 && NotActive)
                    {
                        IsCharging = 1;
                    }
                    else
                    {
                        IsCharging = 0;
                    }
                }
                if (CantAttackTimer > 0)
                {
                    CantAttackTimer--;
                }
                if (BiomeTime > 0)
                {
                    BiomeTime--;
                }
                // --- Period timer + natural SicknessBar decay ---
                // Every 360 ticks: base -4; ×3 in bad biome; ×2 if Drained; +10 if Soothing (additive); +15 if Overcharged (additive).
                if (Main.myPlayer == Projectile.owner)
                {
                    bool isBadBiome   = player.HasBuff(ModContent.BuffType<StatLower>());
                    bool hasSoothing  = player.HasBuff(ModContent.BuffType<Soothing>());
                    bool hasOvercharged = player.HasBuff(ModContent.BuffType<Overcharged>());
                    bool hasDrained   = player.HasBuff(ModContent.BuffType<Drained>());

                    // ---- STEP 1: multiplicative modifiers (apply to loss only, never to gains) ----
                    int baseLoss = isBadBiome ? 12 : 4;
                    if (hasDrained) baseLoss *= 2;
                    // Add future *N loss debuffs here: baseLoss *= N;

                    // ---- STEP 2: start from the raw loss ----
                    int change = -baseLoss;

                    // ---- STEP 3: additive bonuses (always last — never doubled by step 1) ----
                    if (hasSoothing)    change += 10; // e.g. -12 + 10 = -2; -4 + 10 = +6
                    if (hasOvercharged) change += 15;
                    // Add future +N recovery bonuses here: change += N;

                    SicknessDecayChange = change;

                    _sicknessDecayTimer++;
                    if (_sicknessDecayTimer >= 360)
                    {
                        _sicknessDecayTimer = 0;
                        SicknessBar = Math.Clamp(SicknessBar + SicknessDecayChange, 0, SicknessBarMax);
                        Projectile.netUpdate = true;
                    }

                    // Period timer: pure countdown only. All mood triggers are in SariaBubbleFaceSpawner.
                    if (_periodTimer > 0)
                    {
                        _periodTimer--;
                        if (_periodTimer == 0)
                            Projectile.netUpdate = true;
                    }
                    if (_statRaiseSoundCooldown > 0) _statRaiseSoundCooldown--;
                    if (_statLowerSoundCooldown > 0) _statLowerSoundCooldown--;
                }
                // --- End Period timer ---
                if (FlashCooldownTimer > 0)
                {
                    FlashCooldownTimer--;
                    if (FlashCooldownTimer == 0)
                        Projectile.netUpdate = true;
                }
                if (_moodOverrideTimer > 0)
                {
                    _moodOverrideTimer--;
                    Mood = _moodOverrideTarget;
                    if (_moodOverrideTimer == 0)
                    {
                        Mood = 0;
                        _moodPriority = 0;
                        Projectile.netUpdate = true;
                    }
                }
                if (SicknessBar <= SicknessBarMax / 5)
                {
                    if (!player.HasBuff(ModContent.BuffType<Soothing>()) && !player.HasBuff(ModContent.BuffType<Sickness>()))
                    {
                        if (Main.myPlayer == Projectile.owner) player.AddBuff(ModContent.BuffType<Sickness>(), 30000);
                    }
                }
                if ((!(HoldingHealBall) && SariaTalking) || (!SariaUISystem.IsDialogueActive))
                {
                    SariaTalking = false;
                }
                if (SoundTimer2 > 0)
                {
                    SoundTimer2--;
                }
                if (ChangeForm <= 0)
                {
                    SelectSound = false;
                }
                bool newCursed = player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || player.HasBuff(ModContent.BuffType<EclipseBuff>()) || Mood == (int)MoodState.Cursed;
                if (Cursed != newCursed)
                {
                    Cursed = newCursed;
                    Projectile.netUpdate = true;
                }
                Follow = Cursed;

                // LinkCable follow: active when cable is on, Saria is not cursed, and a marker is placed.
                // Owner-only computation; synced to all clients via SendExtraAI so they enter the correct movement branch.
                if (Main.myPlayer == Projectile.owner)
                {
                    bool prevLinkCableFollow = _linkCableFollow;
                    _linkCableFollow = modPlayer.LinkCable && !Cursed && modPlayer.LinkCableTarget != Vector2.Zero;
                    if (_linkCableFollow != prevLinkCableFollow)
                        Projectile.netUpdate = true;
                    if (_linkCableFollow)
                    {
                        // Inject the cable marker directly as the marked position.
                        Vector2 prevMarked = _followMarkedPosition;
                        _followMarkedPosition = modPlayer.LinkCableTarget;
                        _followMarkedNumber   = int.MaxValue - 1; // distinct sentinel; not a trail dot
                        if (Vector2.DistanceSquared(prevMarked, _followMarkedPosition) > (8f * 8f))
                            Projectile.netUpdate = true;
                    }
                    else if (!Follow && _followMarkedPosition != Vector2.Zero && _followMarkedNumber == int.MaxValue - 1)
                    {
                        // Cable was just turned off — clear the injected marker.
                        _followMarkedPosition = Vector2.Zero;
                        _followMarkedNumber   = -1;
                        Projectile.netUpdate  = true;
                    }
                }

                // Follow trail: track the player's path when they are out of Saria's sight.
                // Owner-only; dots are purely local draw-side data.
                if (Main.myPlayer == Projectile.owner)
                {
                    if (!Follow && !_linkCableFollow)
                    {
                        // Clear trail when Follow is inactive; reset everything for next session.
                        if (_followTrailDots.Count > 0)
                        {
                            _followTrailDots.Clear();
                            _followTrailDistAccum = 0f;
                        }
                        _followTrailNextNumber = 1;
                        _playerVisibleToSaria  = false;
                        _followTrailLastPos    = player.Center;
                        if (_followMarkedPosition != Vector2.Zero)
                        {
                            _followMarkedPosition = Vector2.Zero;
                            _followMarkedNumber   = -1;
                            Projectile.netUpdate  = true;
                        }
                        if (_followPath.Count > 0)
                        {
                            _followPath.Clear();
                            _followPathLastGoal = Vector2.Zero;
                            Projectile.netUpdate = true;
                        }
                    }
                    else if (Follow)
                    {
                        // Sight check: player within 500 world units AND clear line to Saria.
                        bool currentlyVisible =
                            Vector2.Distance(player.Center, Projectile.Center) <= 500f &&
                            Collision.CanHitLine(player.Center, 1, 1, Projectile.Center, 1, 1);

                        if (currentlyVisible)
                        {
                            // Player is visible — no trail needed; wipe any existing dots.
                            if (_followTrailDots.Count > 0)
                            {
                                _followTrailDots.Clear();
                                _followTrailDistAccum = 0f;
                                _followTrailNextNumber = 1;
                            }
                            _followTrailLastPos = player.Center;
                        }
                        else
                        {
                            // Falling edge (visible → not visible): reset and immediately drop dot #1.
                            if (_playerVisibleToSaria)
                            {
                                _followTrailDots.Clear();
                                _followTrailDistAccum  = 0f;
                                _followTrailNextNumber = 1;
                                _followTrailDots.Add((player.Center, _followTrailNextNumber++));
                                _followTrailLastPos = player.Center;
                            }

                            // Regular distance accumulation and dot placement while out of sight.
                            if (_followTrailLastPos == Vector2.Zero)
                                _followTrailLastPos = player.Center;

                            float moved = Vector2.Distance(player.Center, _followTrailLastPos);
                            _followTrailLastPos    = player.Center;
                            _followTrailDistAccum += moved;

                            if (_followTrailDistAccum >= FollowTrailInterval)
                            {
                                // Use % to drain the full backlog in one step — prevents a fast
                                // jump from building up a remainder that fires a cluster of
                                // delayed dots over the following ticks.
                                _followTrailDistAccum %= FollowTrailInterval;
                                _followTrailDots.Add((player.Center, _followTrailNextNumber++));
                                if (_followTrailDots.Count > FollowTrailMaxDots)
                                    _followTrailDots.RemoveAt(0);
                            }
                        }

                        _playerVisibleToSaria = currentlyVisible;

                        // Marked location selection.
                        // When player is within FollowMarkerRange with clear LoS, path directly to them.
                        // Otherwise target the highest-numbered visible trail dot.
                        bool playerNearAndVisible =
                            Vector2.Distance(player.Center, Projectile.Center) <= FollowMarkerRange &&
                            Collision.CanHitLine(player.Center, 1, 1, Projectile.Center, 1, 1);

                        Vector2 newMarked       = Vector2.Zero;
                        int     newMarkedNumber = -1;
                        if (playerNearAndVisible)
                        {
                            // Direct sight to player — path straight to them for the final approach.
                            newMarked       = player.Center;
                            newMarkedNumber = int.MaxValue;
                        }
                        else if (_followTrailDots.Count == 0)
                        {
                            // No dots and player not visible — place one at player's position immediately.
                            int newNum = _followTrailNextNumber++;
                            _followTrailDots.Add((player.Center, newNum));
                            newMarked       = player.Center;
                            newMarkedNumber = newNum;
                            Projectile.netUpdate = true;
                        }
                        else if (_followTrailDots.Count > 0)
                        {
                            // Prefer highest-numbered dot within FollowMarkerRange with clear LoS from Saria.
                            int     bestNumber = -1;
                            Vector2 bestPos    = Vector2.Zero;
                            for (int di = 0; di < _followTrailDots.Count; di++)
                            {
                                var dot = _followTrailDots[di];
                                if (Vector2.Distance(Projectile.Center, dot.Position) <= FollowMarkerRange &&
                                    Collision.CanHitLine(Projectile.Center, 1, 1, dot.Position, 1, 1) &&
                                    dot.Number > bestNumber)
                                {
                                    bestNumber = dot.Number;
                                    bestPos    = dot.Position;
                                }
                            }
                            // Fall back to oldest dot if none are visible within range.
                            if (bestPos != Vector2.Zero)
                            {
                                newMarked       = bestPos;
                                newMarkedNumber = bestNumber;
                            }
                            else
                            {
                                newMarked       = _followTrailDots[0].Position;
                                newMarkedNumber = _followTrailDots[0].Number;
                            }

                            // Shortcut: scan within FollowShortcutRange for a higher-numbered dot
                            // with clear LoS. If found, skip ahead to it and prune all dots below it.
                            int     shortcutBestNumber = newMarkedNumber;
                            Vector2 shortcutBestPos    = Vector2.Zero;
                            for (int di = 0; di < _followTrailDots.Count; di++)
                            {
                                var dot = _followTrailDots[di];
                                if (dot.Number > shortcutBestNumber &&
                                    Vector2.Distance(Projectile.Center, dot.Position) <= FollowShortcutRange &&
                                    Collision.CanHitLine(Projectile.Center, 1, 1, dot.Position, 1, 1))
                                {
                                    shortcutBestNumber = dot.Number;
                                    shortcutBestPos    = dot.Position;
                                }
                            }
                            if (shortcutBestPos != Vector2.Zero)
                            {
                                // Prune all dots with a number lower than the shortcut target.
                                for (int di = _followTrailDots.Count - 1; di >= 0; di--)
                                {
                                    if (_followTrailDots[di].Number < shortcutBestNumber)
                                        _followTrailDots.RemoveAt(di);
                                }
                                newMarked       = shortcutBestPos;
                                newMarkedNumber = shortcutBestNumber;
                            }
                        }

                        bool markMoved   = Vector2.DistanceSquared(_followMarkedPosition, newMarked) > (8f * 8f);
                        bool numberShift = _followMarkedNumber != newMarkedNumber;
                        _followMarkedPosition = newMarked;
                        _followMarkedNumber   = newMarkedNumber;
                        if (markMoved || numberShift)
                            Projectile.netUpdate = true;

                        // A* path planning to the marked location (owner-only).
                        // Runs only when a marked dot exists; recomputes on goal change
                        // or on the FollowPathRecalcTicks throttle. The resulting tile
                        // trail is synced to all clients (path change => netUpdate).
                        if (_followMarkedPosition != Vector2.Zero)
                        {
                            if (_followPathTimer > 0)
                                _followPathTimer--;

                            bool goalChanged = Vector2.DistanceSquared(_followPathLastGoal, _followMarkedPosition) > (8f * 8f);
                            // Only run the periodic timer replan when she has no active path
                            // (empty or already at the last node) — avoids resetting mid-traversal.
                            bool pathActive = _followPath.Count > 0 &&
                                              _followPathIndex < _followPath.Count - 1;
                            // Wall-block exception: if a wall is blocking her direction toward the
                            // current waypoint while mid-path, allow an immediate replan so she can
                            // route around newly placed tiles.
                            bool wallBlockingPath = false;
                            if (pathActive && _followPathIndex < _followPath.Count)
                            {
                                Vector2 toNext = _followPath[_followPathIndex] - SariaNavRef;
                                if ((_wallPausedLeft  && toNext.X < 0f) ||
                                    (_wallPausedRight && toNext.X > 0f))
                                    wallBlockingPath = true;
                            }
                            if (goalChanged || wallBlockingPath || (!pathActive && _followPathTimer <= 0))
                            {
                                _followPathTimer    = FollowPathRecalcTicks;
                                _followPathLastGoal = _followMarkedPosition;

                                float pathAllowance = _followMarkedNumber == int.MaxValue
                                    ? FollowPathPlayerAllowance
                                    : FollowPathAllowance;
                                var newPath = SariaPathfinder.FindPath(
                                    SariaNavRef, _followMarkedPosition,
                                    FollowPathFootprintWidth, FollowPathFootprintHeight,
                                    pathAllowance, Transform == 1);

                                if (!FollowPathsEqual(_followPath, newPath))
                                {
                                    _followPath.Clear();
                                    if (newPath != null)
                                        _followPath.AddRange(newPath);
                                    _followPathIndex = 0;
                                    Projectile.netUpdate = true;
                                }

                                // A* failed — path is null or empty and we have no existing path.
                                // Trigger the path-blocked teleport if not already running.
                                bool pathFailed = (_followPath.Count == 0) && _pathTeleportTimer <= 0
                                                  && _inWallTeleportTimer <= 0;
                                if (pathFailed && _followMarkedPosition != Vector2.Zero)
                                {
                                    float distToGoal = Vector2.Distance(SariaNavRef, _followMarkedPosition);
                                    const float directThresholdPx = PathTeleportDirectTiles * 16f;

                                    if (distToGoal > directThresholdPx)
                                    {
                                        // Too far — teleport directly on top of the dot.
                                        _pathTeleportTarget = _followMarkedPosition;
                                    }
                                    else
                                    {
                                        // Nearby — run reverse A* from dot toward Saria for
                                        // the closest reachable landing spot.
                                        var reversePath = SariaPathfinder.FindPath(
                                            _followMarkedPosition, SariaNavRef,
                                            FollowPathFootprintWidth, FollowPathFootprintHeight,
                                            FollowPathAllowance, Transform == 1);
                                        _pathTeleportTarget = (reversePath != null && reversePath.Count > 0)
                                            ? reversePath[0]
                                            : _followMarkedPosition;
                                    }

                                    // Lock destination and start 5-second wind-up.
                                    _pathTeleportTimer = PathTeleportDuration;
                                    StartTeleportWindUp(_pathTeleportTarget, PathTeleportDuration);
                                }
                            }
                        }
                        else if (_followPath.Count > 0)
                        {
                            // Marked location cleared while following — drop the path.
                            _followPath.Clear();
                            _followPathLastGoal = Vector2.Zero;
                            Projectile.netUpdate = true;
                        }
                    }
                    else if (_linkCableFollow)
                    {
                        // LinkCable A* planning: same path machinery as Follow but no trail dots.
                        // _followMarkedPosition is already set to LinkCableTarget above.
                        if (_followMarkedPosition != Vector2.Zero)
                        {
                            if (_followPathTimer > 0)
                                _followPathTimer--;

                            bool goalChanged = Vector2.DistanceSquared(_followPathLastGoal, _followMarkedPosition) > (8f * 8f);
                            bool pathActive  = _followPath.Count > 0 && _followPathIndex < _followPath.Count - 1;
                            // Arrival deadzone: if she's resting near the marker with no active
                            // path, don't re-path. This lets the ground probes nudge her for
                            // clearance without A* yanking her back to the exact tile every tick.
                            bool restingAtMarker = !pathActive
                                && Vector2.Distance(SariaNavRef, _followMarkedPosition) <= LinkCableArrivalDeadzone;
                            bool wallBlockingPath = false;
                            if (pathActive && _followPathIndex < _followPath.Count)
                            {
                                Vector2 toNext = _followPath[_followPathIndex] - SariaNavRef;
                                if ((_wallPausedLeft  && toNext.X < 0f) ||
                                    (_wallPausedRight && toNext.X > 0f))
                                    wallBlockingPath = true;
                            }
                            if (!restingAtMarker && (goalChanged || wallBlockingPath || (!pathActive && _followPathTimer <= 0)))
                            {
                                _followPathTimer    = FollowPathRecalcTicks;
                                _followPathLastGoal = _followMarkedPosition;

                                var newPath = SariaPathfinder.FindPath(
                                    SariaNavRef, _followMarkedPosition,
                                    FollowPathFootprintWidth, FollowPathFootprintHeight,
                                    FollowPathPlayerAllowance, Transform == 1);

                                if (!FollowPathsEqual(_followPath, newPath))
                                {
                                    _followPath.Clear();
                                    if (newPath != null)
                                        _followPath.AddRange(newPath);
                                    _followPathIndex = 0;
                                    Projectile.netUpdate = true;
                                }

                                // Teleport fallback if A* fails.
                                bool pathFailed = (_followPath.Count == 0) && _pathTeleportTimer <= 0
                                                  && _inWallTeleportTimer <= 0;
                                if (pathFailed)
                                {
                                    float distToGoal = Vector2.Distance(SariaNavRef, _followMarkedPosition);
                                    const float directThresholdPx = PathTeleportDirectTiles * 16f;
                                    if (distToGoal > directThresholdPx)
                                    {
                                        _pathTeleportTarget = _followMarkedPosition;
                                    }
                                    else
                                    {
                                        var reversePath = SariaPathfinder.FindPath(
                                            _followMarkedPosition, SariaNavRef,
                                            FollowPathFootprintWidth, FollowPathFootprintHeight,
                                            FollowPathPlayerAllowance, Transform == 1);
                                        _pathTeleportTarget = (reversePath != null && reversePath.Count > 0)
                                            ? reversePath[0]
                                            : _followMarkedPosition;
                                    }
                                    _pathTeleportTimer = PathTeleportDuration;
                                    StartTeleportWindUp(_pathTeleportTarget, PathTeleportDuration);
                                }
                            }
                        }
                        else if (_followPath.Count > 0)
                        {
                            _followPath.Clear();
                            _followPathLastGoal = Vector2.Zero;
                            Projectile.netUpdate = true;
                        }
                    }
                }
                if (player.HasBuff(ModContent.BuffType<Soothing>()) && player.HasBuff(ModContent.BuffType<Sickness>()))
                {
                    player.ClearBuff(ModContent.BuffType<Sickness>());
                }
                /////////////// End of Transformation Timer
                ///
                int dustspeed = 40;
                if ((Projectile.frame >= 36 && Projectile.frame <= 42))
                {
                    dustspeed = 5;
                }
                if (Transform == 2)
                {
                    Projectile.SneezeDust(ModContent.DustType<FlameDustSaria>(), 30, 100, -10, 3, -12);
                }
                // Emit MediumXpPearl-like light when Transform == 1 and in water (non-lava)
                if (Transform == 1)
                {
                    if (Projectile.IsMostlyInNonLavaLiquid())
                    {
                        Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 1.2f);
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Main.player[Projectile.owner].AddBuff(BuffID.Gills, 2);
                        }
                    }
                }

                // Emit air bubbles when underwater using custom BubbleDust
                // Bubbles float up slowly, emit light, and despawn when they exit water
                if (Projectile.IsTopHalfMostlyInNonLavaLiquid() && Main.netMode != NetmodeID.Server)
                {
                    // Slower spawn rate: ~1 out of 25 ticks
                    if (Main.rand.NextBool(25))
                    {
                        // Calculate spawn position near Saria's mouth/face area
                        float sneezespot = (Projectile.spriteDirection > 0) ? 3f : -12f;
                        Vector2 spawnPos = new Vector2(
                            Projectile.Center.X + sneezespot + Main.rand.NextFloat(-2f, 2f),
                            Projectile.Center.Y - 10f + Main.rand.NextFloat(-3f, 3f)
                        );

                        // Create bubble with slow upward velocity
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.4f, -0.15f));
                        Dust bubble = Dust.NewDustPerfect(spawnPos, ModContent.DustType<BubbleDust>(), vel);
                        bubble.noGravity = true;
                    }
                }
                if (Transform == 6)
                {
                    Projectile.SneezeDust(ModContent.DustType<ShadowFlameDust>(), 30, 100, -10, 3, -12);
                }
                // Dark shadow aura when mood is Cursed
                if (Mood == (int)MoodState.Cursed && Main.netMode != NetmodeID.Server)
                {
                    Projectile.SneezeDust(ModContent.DustType<ShadowFlameDust>(), 60, 2, -10, 3, -12);
                    Lighting.AddLight(Projectile.Center, new Vector3(0.25f, 0f, 0.3f));
                }
                if (Projectile.frame == 22 && (Eating % 5 == 0) && (!Sleep) && !BloodSneeze && (!player.HasBuff(ModContent.BuffType<StatLower>()) && !player.HasBuff(ModContent.BuffType<Sickness>()) && !player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && !player.HasBuff(ModContent.BuffType<EclipseBuff>())))
                {
                    Projectile.SneezeDust(ModContent.DustType<Sneeze>(), 1, 1, -10, 3, -12);
                }
                if (Projectile.frame == 22 && (Eating % 5 == 0) && (!Sleep) && (BloodSneeze || player.HasBuff(ModContent.BuffType<StatLower>()) || player.HasBuff(ModContent.BuffType<Sickness>()) || player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || player.HasBuff(ModContent.BuffType<EclipseBuff>())))
                {
                    Projectile.SneezeDust(ModContent.DustType<Blood>(), 1, 1, -10, 3, -12);
                    Projectile.SneezeDust(ModContent.DustType<Blood>(), 30, 1, -10, 3, -12);
                }
                if ((player.active && Main.bloodMoon) && ((!player.HasBuff(ModContent.BuffType<Soothing>()))))
                {
                    player.AddBuff(ModContent.BuffType<BloodmoonBuff>(), 20);
                    Projectile.SneezeDust(ModContent.DustType<Blood>(), 30, 1, -10, 3, -12);
                    Projectile.SneezeDust(ModContent.DustType<BlackSmoke>(), 20, 6, -10, 3, -12);
                }
                Projectile.SneezeDust(ModContent.DustType<Psychic2>(), (int)dustspeed, 6, 34, 3, -12);

                // Fog breath - only when NOT underwater, using Saria's own synced zone flags
                bool isUnderwater = Projectile.IsTopHalfMostlyInNonLavaLiquid();
                if (!isUnderwater && (((SariaZoneSnow) && !(SariaExtensions1.IsLineSegmentPartiallyWalled(new Vector2(Projectile.Center.X, Projectile.position.Y), Projectile.Center, 0.75f) && SariaHasCampfire)) || (SariaZoneSpace) && !(SariaExtensions1.IsLineSegmentPartiallyWalled(new Vector2(Projectile.Center.X, Projectile.position.Y), Projectile.Center, 0.75f) && SariaHasCampfire) || (SariaZoneDesert && !Main.dayTime) && !(SariaExtensions1.IsLineSegmentPartiallyWalled(new Vector2(Projectile.Center.X, Projectile.position.Y), Projectile.Center, 0.75f) && SariaHasCampfire) || (SariaZoneRain && !SariaZoneJungle && !(SariaZoneDesert && Main.dayTime)) && !(SariaExtensions1.IsLineSegmentPartiallyWalled(new Vector2(Projectile.Center.X, Projectile.position.Y), Projectile.Center, 0.75f) && SariaHasCampfire)))
                {
                    if (Projectile.velocity.X <= 1)
                    {
                        Projectile.SneezeDust(ModContent.DustType<Fog>(), 50, 1, -10, 10, -17);
                    }
                    else if (Projectile.velocity.X > 1)
                    {
                        Projectile.SneezeDust(ModContent.DustType<Fog>(), 5, 1, -10, 10, -17);
                    }
                }//end of dust stuff
                if (Projectile.localAI[0] == 0f && Main.myPlayer == Projectile.owner)
                {
                    Projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = Projectile.damage;
                    modPlayer.smallTalkingTime = Main.rand.Next(30 * 60, 40 * 60 + 1);
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Ztarget>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<HealCursor>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                        CantAttackTimer = 120;
                    }
                    Projectile.localAI[0] = 1f;
                }
                ////Ztargets
                if (player.ownedProjectileCounts[ModContent.ProjectileType<HealCursor>()] <= 0f)
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<HealCursor>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                }
                Projectile.Ztargets((int)ChannelState, (int)Transform);
                ///
                if (player.dead)
                {
                    modPlayer.SariaXp /= 2;
                }
                if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
                {
                    Projectile.timeLeft = 10;
                }
                if (!player.HasBuff(ModContent.BuffType<SariaBuff>()) && Projectile.timeLeft == 1)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.Kill();
                    }
                }
                if ((!HoldingHealBallInInventory && !HoldingHealBall) && Projectile.timeLeft == 1)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.Kill();
                    }
                }
                /// AiStuff
                Vector2 targetCenter = Projectile.position;
                bool foundTarget = false;
                bool CanSee = false;

                // --- PRIORITY: ZtargetReal projectile (owned by same player) ---
                int ztargetRealType = ModContent.ProjectileType<ZtargetReal>();
                float bestZtargetDist = 2000f;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (!p.active)
                        continue;

                    if (p.type != ztargetRealType)
                        continue;

                    // only consider the one(s) owned by the same player as this Saria projectile
                    if (p.owner != Projectile.owner)
                        continue;

                    float between = Vector2.Distance(p.Center, Projectile.Center);
                    if (between >= bestZtargetDist)
                        continue;

                    bool canSeeIt = Collision.CanHitLine(Projectile.Center, 1, 1, p.position, p.width, p.height);

                    bestZtargetDist = between;
                    targetCenter = p.Center;
                    foundTarget = true;
                    CanSee = true;
                }

                // --- FALLBACK: your existing NPC targeting logic ---
                if (!foundTarget && player.HasMinionAttackTargetNPC && player.HeldItem.type == ModContent.ItemType<HealBall>())
                {
                    NPC npc = Main.npc[player.MinionAttackTargetNPC];
                    bool CanSeeit = Collision.CanHitLine(Projectile.Center, 1, 1, npc.position, npc.width, npc.height);
                    float between = Vector2.Distance(npc.Center, Projectile.Center);

                    if (between < 2000f)
                    {
                        targetCenter = npc.Center;
                        foundTarget = true;
                        if (CanSeeit)
                            CanSee = true;
                    }
                }

                if (!foundTarget && Main.myPlayer == Projectile.owner)
                {
                    Vector2 bestFrozenTarget = Vector2.Zero;
                    float bestFrozenDistance = -1f;
                    bool bestFrozenCanSee = false;

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.CanBeChasedBy())
                            continue;

                        float between = _linkCableFollow
                            ? Vector2.Distance(npc.Center, Projectile.Center)
                            : Vector2.Distance(npc.Center, player.Center);
                        bool closeThroughWall = between < 800f;
                        bool canSeeit = Collision.CanHitLine(Projectile.Center, 1, 1, npc.position, npc.width, npc.height);
                        if (!closeThroughWall)
                            continue;

                        if (Transform == 1)
                        {
                            int frozenBuffId = ModContent.BuffType<EnemyFrozen>();
                            bool isFrozen = npc.HasBuff(frozenBuffId);

                            if (!isFrozen)
                            {
                                if (!foundTarget || Vector2.Distance(player.Center, targetCenter) > between)
                                {
                                    targetCenter = npc.Center;
                                    foundTarget = true;
                                    CanSee = canSeeit;
                                }
                            }
                            else
                            {
                                if (bestFrozenDistance == -1f || bestFrozenDistance > between)
                                {
                                    bestFrozenTarget = npc.Center;
                                    bestFrozenDistance = between;
                                    bestFrozenCanSee = canSeeit;
                                }
                            }
                        }
                        else
                        {
                            bool closest = Vector2.Distance(player.Center, targetCenter) > between;
                            if (closest || !foundTarget)
                            {
                                targetCenter = npc.Center;
                                foundTarget = true;
                                CanSee = canSeeit;
                            }
                        }
                    }

                    if (Transform == 1 && !foundTarget && bestFrozenDistance != -1f)
                    {
                        targetCenter = bestFrozenTarget;
                        foundTarget = true;
                        CanSee = bestFrozenCanSee;
                    }
                }
                // Biome weakness forced sneeze — suppress targeting so she can't
                // start new attacks while the sneeze is pending or playing.
                // Current attacks finish naturally, but no new ones begin.
                if ((player.HasBuff(ModContent.BuffType<StatLower>()) || Cursed) && (IdleAnimator.IsSneezeQueued || Sneezing))
                {
                    foundTarget = false;
                }
                // Transformation in progress — suppress targeting so she can't enter
                // a new attack state while changing forms. Current attacks finish naturally.
                if (IsTransforming)
                {
                    foundTarget = false;
                }
                Projectile.SariaAI((int)Transform, (int)ChannelTime, (bool)NotActive, (bool)foundTarget, (bool)Sleep, (bool)HoldingHealBall, (int)CantAttackTimer, (int)ChannelState, (int)Eating, (bool)CanSee);

                if ((Main.rand.NextBool(550) || foundTarget) && SpecialAnimate <= 0)
                {
                    SpecialAnimate = 60;
                }
                if (SpecialAnimate > 0)
                {
                    SpecialAnimate--;
                }
                /////end
                //Flashupdate stuff
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    float between = Vector2.Distance(Main.projectile[i].Center, player.Center);
                    if (between <= 100)
                    {
                        if (Main.projectile[i].active && i != Projectile.whoAmI && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)) && Main.myPlayer == Projectile.owner)
                        {
                            if ((!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (!player.HasBuff(ModContent.BuffType<EclipseBuff>()) && FlashCooldownTimer <= 0))) && Main.myPlayer == Projectile.owner)
                            {
                                Projectile.TriggerFlash();
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                                    player.Center.X, player.Center.Y, 0, 0,
                                    ModContent.ProjectileType<FlashBarrier>(),
                                    (int)Projectile.damage, 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                                FlashCooldownTimer = 1800;
                                SoundEngine.PlaySound(SoundID.Item76, Projectile.Center);
                                for (int o = 0; o < 50; o++)
                                {
                                    Vector2 speed2 = Main.rand.NextVector2CircularEdge(1.1f, 1.1f);
                                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PsychicRingDust>(), speed2 * 15, Scale: 4f);
                                    d.noGravity = true;
                                }
                            }
                        }
                    }
                }
                if (FlashCooldownTimer > 0)
                {
                    Projectile.SneezeDust(ModContent.DustType<Psychic>(), 20, 6, -10, 3, -12);
                }
                if (CantAttackTimer > 0)
                {
                    CantAttack = true;
                }
                if (Projectile.frame >= 44 && Projectile.frame <= 55 && Transform == 1)
                {
                    Projectile.AttackDust(ModContent.DustType<BubbleDust>(), 8, 34);
                }
                if (Projectile.frame >= 44 && Projectile.frame <= 55 && Transform == 2)
                {
                    Projectile.AttackDust(ModContent.DustType<FlameDust>(), 8, 34);
                }
                if (Projectile.frame >= 44 && Projectile.frame <= 55 && Transform == 3)
                {
                    Projectile.AttackDust2();
                }
                if (Projectile.frame >= 44 && Projectile.frame <= 55 && Transform == 6)
                {
                    Projectile.AttackDust(ModContent.DustType<ShadowFlameDust>(), 1, 34);
                }
                Vector2 idlePosition = player.Center;
                float speed = 2;
                float Close = 60;
                if (Eating <= 0 && !Sleep)
                {
                    if (player.HeldItem.type == ModContent.ItemType<FrozenYogurt>() || player.HeldItem.type == ModContent.ItemType<SariasConfect>())
                    {
                        Close = 20;
                        if ((player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] <= 0f) && !Holding)
                        {
                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                            Holding = true;
                            MoveTimer = 0;
                             }
                        }
                        if (HoldingHealBall)
                    {
                        Close = 60;
                        if (Holding)
                        {
                            Holding = false;
                        }
                    }
                    if (player.HeldItem.type != ModContent.ItemType<FrozenYogurt>() && player.HeldItem.type != ModContent.ItemType<SariasConfect>() && player.HeldItem.type != ModContent.ItemType<HealBall>())
                    {
                        if (player.statLife >= (player.statLifeMax2 - player.statLifeMax2 / 12))
                        {
                            Close = 30;
                        }
                        else
                        {
                            Close = 60;
                        }
                        if (Holding)
                        {
                            Holding = false;
                        }
                    }
                    // Emeraldspike force: push physical close to 0 while spikes are active.
                    bool spikeActive = player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike>()] > 0f
                        || player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike2>()] > 0f
                        || player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3>()] > 0f;

                    _closeTracker.Update(Projectile, player, Close, spikeActive);
                }
                float SariaOffsetX = _closeTracker.GetOffsetX(Projectile, player, Close);
                idlePosition.Y -= 15f;
                idlePosition.X += SariaOffsetX;
                _debugIdlePosition = idlePosition;
                Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
                float distanceToIdlePosition = vectorToIdlePosition.Length();
                bool _oneWallGreen = _detectorResults[2].Green ^ _detectorResults[3].Green;
                if (_oneWallGreen && Vector2.Distance(Projectile.Center, player.Center) <= 19f && player.velocity.Length() < 1f && !Sleep && Eating <= 0)
                {
                    if (!_followSight)
                        _lockedIdlePosition = idlePosition; // snapshot position on rising edge
                    _followSight = true;
                }
                else if (Vector2.Distance(Projectile.Center, player.Center) > 20 || Sleep || Eating > 0)
                {
                    _followSight = false;
                }
                // While FollowSight is active, freeze the idle position so Saria
                // stops drifting and the direction vector stays at zero.
                // Also mirror the player's facing direction continuously while latched.
                if (_followSight)
                {
                    Projectile.spriteDirection = player.direction;
                    idlePosition = _lockedIdlePosition;
                    vectorToIdlePosition = idlePosition - Projectile.Center;
                    distanceToIdlePosition = vectorToIdlePosition.Length();
                    _debugIdlePosition = idlePosition;
                }
                if (player.HasBuff(ModContent.BuffType<Veil>()) && Transform == 1)
                {
                    player.AddBuff(ModContent.BuffType<Veil>(), 8800);
                }

                Vector2 direction = idlePosition - Projectile.Center;

                if (Follow || _linkCableFollow)
                {
                    // LinkCable mode: redirect idle position to the placed marker so all
                    // subsequent checks (LoS, direction, distance) operate on the cable target.
                    if (_linkCableFollow && _followMarkedPosition != Vector2.Zero)
                    {
                        idlePosition = _followMarkedPosition;
                        vectorToIdlePosition = idlePosition - Projectile.Center;
                        distanceToIdlePosition = vectorToIdlePosition.Length();
                    }

                    bool canSeeIdle = Collision.CanHitLine(Projectile.Center, 1, 1, idlePosition, 1, 1);
                    // Cursed Follow: player must be within FollowMarkerRange AND visible to skip A*.
                    // LinkCable: always use A* — never take the direct-steer shortcut.
                    bool playerDirectVisible = !_linkCableFollow && canSeeIdle &&
                        distanceToIdlePosition <= FollowMarkerRange;

                    if (playerDirectVisible)
                    {
                        // Direct line-of-sight — go straight, no A* needed.
                        if (_followPath.Count > 0)
                        {
                            _followPath.Clear();
                            _followPathLastGoal = Vector2.Zero;
                        }
                        direction = idlePosition - Projectile.Center;
                        _cursedSpeedScale = Math.Min(_cursedSpeedScale + 0.04f, 1f);
                        _cursedSeparated  = false;
                    }
                    else if (_followPath.Count > 0)
                    {
                        // Path exists — steer toward the current waypoint, bypassing
                        // the separation halt so she keeps walking even when far away.
                        // Clamp index in case the path shrank since last tick.
                        if (_followPathIndex >= _followPath.Count)
                            _followPathIndex = _followPath.Count - 1;

                        bool atLastNode = _followPathIndex == _followPath.Count - 1;

                        // Advance intermediate waypoints when close enough.
                        // Owner-only: each client's SariaNavRef drifts slightly between syncs,
                        // so letting all clients advance independently causes them to target
                        // different waypoints and produce opposing velocity vectors → jitter.
                        // The owner advances and immediately syncs the new index via netUpdate
                        // so clients converge to the same waypoint within a frame or two.
                        if (Main.myPlayer == Projectile.owner)
                        {
                            bool didAdvance = false;
                            while (!atLastNode &&
                                   Vector2.Distance(SariaNavRef, _followPath[_followPathIndex]) <= 28f)
                            {
                                _followPathIndex++;
                                atLastNode = _followPathIndex == _followPath.Count - 1;
                                didAdvance = true;
                            }
                            if (didAdvance)
                                Projectile.netUpdate = true;
                        }

                        Vector2 toWaypoint = _followPath[_followPathIndex] - SariaNavRef;
                        float dist = toWaypoint.Length();

                        if (atLastNode)
                        {
                            // Final node: arrived.
                            if (dist <= 16f)
                            {
                                // State mutations are owner-only so non-owner clients never
                                // clear the path prematurely on their locally extrapolated position.
                                if (Main.myPlayer == Projectile.owner)
                                {
                                    if (Follow)
                                    {
                                        // Cursed mode: consume the trail dot and clear the mark so
                                        // the next evaluation can pick the next dot.
                                        for (int di = _followTrailDots.Count - 1; di >= 0; di--)
                                        {
                                            if (_followTrailDots[di].Position == _followMarkedPosition)
                                            {
                                                _followTrailDots.RemoveAt(di);
                                                break;
                                            }
                                        }
                                        _followPath.Clear();
                                        _followPathLastGoal   = Vector2.Zero;
                                        _followMarkedPosition = Vector2.Zero;
                                        _followMarkedNumber   = -1;
                                        Projectile.netUpdate  = true;
                                    }
                                    else
                                    {
                                        // LinkCable mode: arrived at marker — hold position, clear path
                                        // but keep _followMarkedPosition so she stays put.
                                        _followPath.Clear();
                                        _followPathLastGoal = Vector2.Zero;
                                        Projectile.netUpdate = true;
                                    }
                                }
                                direction = Vector2.Zero;
                            }
                            else
                            {
                                // Approaching final node: scale force by distance so she
                                // decelerates smoothly rather than oscillating.
                                float scale = Math.Min(dist, 100f);
                                direction = dist > 0.01f ? toWaypoint / dist * scale : Vector2.Zero;
                            }
                        }
                        else
                        {
                            // Intermediate node: full saturated force.
                            direction = dist > 0.01f ? toWaypoint / dist * 100f : Vector2.Zero;
                        }

                        // Keep speed-scale at 1 while following path (no bleed-off).
                        _cursedSpeedScale   = Math.Min(_cursedSpeedScale + 0.04f, 1f);
                        _cursedSeparated    = false;
                    }
                    else
                    {
                        if (Follow)
                        {
                            // No path — original separation logic unchanged.
                            // Enter separated when distance > CursedSeparationRadius.
                            // Exit only when distance <= CursedSeparationRadius AND clear LOS.
                            bool wasSeparated = _cursedSeparated;
                            if (!_cursedSeparated)
                            {
                                if (distanceToIdlePosition > CursedSeparationRadius)
                                    _cursedSeparated = true;
                            }
                            else
                            {
                                if (distanceToIdlePosition <= CursedSeparationRadius && canSeeIdle)
                                    _cursedSeparated = false;
                            }
                            // Transition: separated → reunited — reset MoveTimer so she can move immediately.
                            if (wasSeparated && !_cursedSeparated)
                            {
                                MoveTimer = 0;
                            }

                            if (_cursedSeparated)
                            {
                                // Too far away — let momentum bleed off naturally; scale decays to 0.
                                direction = Vector2.Zero;
                                _cursedSpeedScale = Math.Max(_cursedSpeedScale - 0.025f, 0f);
                            }
                            else
                            {
                                // Close enough — follow idle position; ramp speed back up.
                                direction = idlePosition - Projectile.Center;
                                _cursedSpeedScale = Math.Min(_cursedSpeedScale + 0.04f, 1f);
                            }
                        }
                        else
                        {
                            // LinkCable mode: no path yet — hold position.
                            direction = Vector2.Zero;
                            _cursedSpeedScale = Math.Min(_cursedSpeedScale + 0.04f, 1f);
                        }
                    }
                }
                else
                {
                    _cursedSeparated  = false;
                    _cursedSpeedScale = 1f;

                    // Far-teleport: when neither Follow nor Cursed is active and the idle
                    // position is beyond IdleTeleportThreshold, teleport instead of flying.
                    if (Main.myPlayer == Projectile.owner
                        && distanceToIdlePosition > IdleTeleportThreshold
                        && _idleTeleportTimer  <= 0
                        && _inWallTeleportTimer <= 0
                        && _pathTeleportTimer  <= 0
                        && CanMove > 0)
                    {
                        // Lock the target and start the 2-second wind-up.
                        _idleTeleportTarget = idlePosition;
                        _idleTeleportTimer  = IdleTeleportDuration;
                        StartTeleportWindUp(idlePosition, IdleTeleportDuration);
                    }

                    // Suppress normal flying movement while the idle teleport wind-up is active.
                    if (_idleTeleportTimer > 0)
                        direction = Vector2.Zero;
                }
                if (foundTarget)
                {
                    {
                        speed = 2;
                        // GreenPause: if the inner Green line of a wall detector is solid,
                        // suppress direction.X toward that wall so the movement formula
                        // cannot drift Saria into the tile. Priority is unaffected.
                        Vector2 gatedDirection = direction;
                        if (_wallPausedLeft  && gatedDirection.X < 0f) gatedDirection.X = 0f;
                        if (_wallPausedRight && gatedDirection.X > 0f) gatedDirection.X = 0f;
                        // Owner-authoritative while following: see note on the main integration below.
                        if (Main.myPlayer == Projectile.owner || !(Follow || _linkCableFollow))
                            Projectile.velocity = (((Projectile.velocity * (13 - speed) + gatedDirection) / 20) * CanMove);
                    }
                }
                int newCanMove;
                if (Sleep || Eating == 3 || Eating == 4 || Eating == 5 || Sneezing || (ChannelState > 0 && (IsCharging <= 0 || Projectile.frame <= 8)))// if you want Saria to not move when charging, copy---- || ChannelState > 0  ----- and put it behind Eating == 4
                {
                    newCanMove = 0;
                }
                else if ((MoveTimer >= 275 && ((Projectile.frame >= 0) && (Projectile.frame <= 36)) && distanceToIdlePosition <= 180 && (Math.Abs(Projectile.velocity.X) <= .5) && (player.statLife >= player.statLifeMax2)))
                {
                    newCanMove = 0;
                }
                else
                {
                    newCanMove = 1;
                }
                // Override: keep CanMove = 1 when a food signal is active, the player is
                // holding the food item, and Saria is in idle frames so she walks to the player.
                bool foodSignalActive = Eating <= 0 && !Sleep && !Sneezing && Projectile.frame <= SariaIdleAnimator.IdleFrameMax && (player.ownedProjectileCounts[ModContent.ProjectileType<FrozenYogurtSignal>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<Competitivetime>()] > 0f);
                if (foodSignalActive)
                {
                    newCanMove = 1;
                }
                // Override: keep CanMove = 1 while actively following the A* path so the
                // idle-stop timer does not freeze her mid-walk.
                // Covers both intermediate nodes and the final approach (> 16px away).
                // Eating > 0 takes priority: she must not move while consuming food.
                if ((Follow || _linkCableFollow) && !Sneezing && Eating <= 0 && _followPath.Count > 0 &&
                    Vector2.Distance(SariaNavRef, _followPath[_followPath.Count - 1]) > 16f)
                {
                    newCanMove = 1;
                }
                // Override: stop Saria in place while the player is holding the FeelingRod.
                if (player.HeldItem.type == ModContent.ItemType<FeelingRod>())
                {
                    newCanMove = 0;
                }
                if (CanMove != newCanMove)
                {
                    CanMove = newCanMove;
                }
                if (Sleep && (distanceToIdlePosition > 280))
                {
                    MoveTimer = 0;
                }
                if (ChannelState > 0)
                {
                    MoveTimer = 0;
                }
                {
                    Vector2 gatedDir = direction;
                    if (_followPath.Count == 0)
                    {
                        if (_wallPausedLeft  && gatedDir.X < 0f) gatedDir.X = 0f;
                        if (_wallPausedRight && gatedDir.X > 0f) gatedDir.X = 0f;
                    }
                    // Owner-authoritative movement: only the owner integrates velocity while in
                    // Follow/LinkCable mode. A non-owner sits ~100px behind the synced position,
                    // so re-running this formula makes it compute full speed toward the marker
                    // while the owner decelerates — then netUpdate snaps it back, causing jitter.
                    // Non-owners coast on the synced velocity and let position updates land.
                    if (Main.myPlayer == Projectile.owner || !(Follow || _linkCableFollow))
                        Projectile.velocity = ((Projectile.velocity * (13 - speed) + gatedDir) / 20) * CanMove;
                }
                // Follow / LinkCable mode: cap speed on both axes, scaled by _cursedSpeedScale.
                // Decays to 0 when separated (natural halt), ramps up when rejoining player.
                if (Follow || _linkCableFollow)
                {
                    float maxSpeedX = (2.25f + 0.30f * MathF.Sin(Main.GameUpdateCount * (MathF.PI * 2f / 300f))) * _cursedSpeedScale;
                    float maxSpeedY = 4.5f * _cursedSpeedScale;
                    Projectile.velocity.X = Math.Clamp(Projectile.velocity.X, -maxSpeedX, maxSpeedX);
                    Projectile.velocity.Y = Math.Clamp(Projectile.velocity.Y, -maxSpeedY, maxSpeedY);
                    // Gentle bob: sine wave on Y so she floats rather than walking rigidly.
                    // Suppressed when a solid tile is within 1 tile above her head so she
                    // doesn't clip into ceilings in doorways or 3-tile-high hallways.
                    // Owner-only: GameUpdateCount is not synced, so a non-owner would bob at a
                    // different sine phase and fight the owner's netUpdate, causing jitter.
                    if (Main.myPlayer == Projectile.owner && Math.Abs(Projectile.velocity.X) > 0.1f)
                    {
                        bool lowCeiling = Collision.SolidCollision(
                            new Vector2(Projectile.position.X, Projectile.position.Y - 16f),
                            Projectile.width, 16);
                        // Walking a path on the ground: ground probes are disabled during
                        // path-follow and tileCollide is off, so the bob pumps her INTO the
                        // floor — then waypoint steering pulls her back up to the path line
                        // next ticks. That push-down/pull-up cycle reads as a small vertical
                        // stutter while she walks. Ground within 1.5 tiles (24px) underfoot
                        // (platforms/slopes included, same reach as the settle scan probe)
                        // → she is WALKING, not floating: skip the bob.
                        bool walkingOnGround = _followPath.Count > 0 && Collision.SolidCollision(
                            new Vector2(Projectile.position.X, Projectile.position.Y + Projectile.height),
                            Projectile.width, 24, true);
                        if (!lowCeiling && !walkingOnGround)
                            Projectile.velocity.Y += 0.35f * MathF.Sin(Main.GameUpdateCount * (MathF.PI * 2f / 100f));
                    }
                    // Force a denser sync while following so the client's velocity refreshes every
                    // ~2 ticks instead of every 3-4, keeping vertical motion smooth on remotes.
                    if (Main.myPlayer == Projectile.owner && _followPath.Count > 0 && Main.GameUpdateCount % 2 == 0)
                        Projectile.netUpdate = true;
                }
                // Ground-riding correction: nudge Saria's Y position so she rides just
                // above tile surfaces without clipping into them.
                // Probe 1 (hitbox): is her body overlapping a solid tile? → push up.
                // Probe 2 (ground line): 20px wide line directly under her feet — is it
                //   touching a tile? → she is properly grounded, do nothing.
                // Scan probe: 1.5 tiles (24px) tall below feet — is there a tile nearby?
                //   If ground line is clear but tile is close, and she's not sleeping → settle down.
                // Platforms, half-tiles, and slopes are included via acceptTopSurfaces=true.
                // X velocity is never touched. Settle-down is skipped while sleeping.
                {
                    // Ground-probe corrections only apply when Saria is within TileCollisionRadius
                    // of the player (non-Follow modes). Outside that range she floats freely.
                    // Hysteresis: probe activates at TileCollisionRadius, deactivates only at
                    // TileCollisionRadius + TileProbeHysteresis to prevent boundary jitter.
                    if (!Follow && !_cursedSeparated && !(_linkCableFollow && _followPath.Count > 0))
                    {
                        float distToPlayer = Vector2.Distance(Projectile.Center, player.Center);
                        // Fix 3: out-of-bounds recovery — disable probes immediately if Saria leaves the
                        // playable world, and only re-enable them once she has returned AND reached her
                        // idle position. This prevents her from re-entering the world still clipped into tiles.
                        bool outOfBounds = Projectile.Center.Y > (Main.maxTilesY - 10) * 16f
                                        || Projectile.Center.Y < 0f
                                        || Projectile.Center.X > (Main.maxTilesX - 10) * 16f
                                        || Projectile.Center.X < 0f;
                        _dbgOutOfBounds = outOfBounds;
                        bool nearIdle = distanceToIdlePosition <= 1f && !outOfBounds;
                        // Both wall probes fired last frame → Saria is wedged between two walls.
                        // Disable all detector corrections so she can phase through to idle position,
                        // mirroring the "too far from player" bypass. Re-enables naturally via nearIdle.
                        bool bothWallsWedged = _detectorResults[2].Pink && _detectorResults[3].Pink;
                        if ((outOfBounds && CanMove > 0) || bothWallsWedged)
                        {
                            ProbesActive = false;
                        }
                        else if (!ProbesActive)
                        {
                            // Re-enable only when within range AND at most one detector is active.
                            // Two or more active means she's still in cramped/solid geometry — stay off.
                            // LinkCable arrived: skip the player-distance gate entirely.
                            bool withinRange = _linkCableFollow
                                ? !outOfBounds
                                : distToPlayer <= TileCollisionRadius && !outOfBounds;
                            int activeCount =
                                (_detectorResults[0].IsActive ? 1 : 0) +
                                (_detectorResults[1].IsActive ? 1 : 0) +
                                (_detectorResults[2].Pink     ? 1 : 0) +
                                (_detectorResults[3].Pink     ? 1 : 0);
                            if (withinRange && activeCount <= 1 && !_inWall)
                                ProbesActive = true;
                        }
                        else if (ProbesActive && !_linkCableFollow && distToPlayer > TileCollisionRadius + TileProbeHysteresis && CanMove > 0)
                            ProbesActive = false;
                    }
                    else
                    {
                        // Following A* path: disable probes so detectors don't fight path movement.
                        if (_followPath.Count > 0)
                            ProbesActive = false;
                        else
                            // Cursed mode keeps probes on while not separated.
                            ProbesActive = !_cursedSeparated;
                    }
                    bool applyProbes = ProbesActive;

                    // InWall detection — uses the body-fit box and orange box to detect solid tile coverage.
                    {
                        Vector2 iwSpritePos = new Vector2(
                            (float)Math.Round(Projectile.position.X),
                            (float)Math.Round(Projectile.position.Y));

                        SariaDetector.GetFacingDir(_detectorConfigs[0].RotationDegrees, out int iw0x, out int iw0y);
                        SariaDetector.GetFacingDir(_detectorConfigs[2].RotationDegrees, out int iw2x, out int iw2y);
                        SariaDetector.GetFacingDir(_detectorConfigs[3].RotationDegrees, out int iw3x, out int iw3y);

                        SariaDetector.GetProbeRects(in _detectorConfigs[0], iwSpritePos, iw0x, iw0y,
                            out _, out Rectangle iwFeet, out _,
                            Projectile.width, Projectile.spriteDirection);
                        SariaDetector.GetProbeRects(in _detectorConfigs[2], iwSpritePos, iw2x, iw2y,
                            out Rectangle iwWallLPink, out Rectangle iwWallL, out _);
                        SariaDetector.GetProbeRects(in _detectorConfigs[3], iwSpritePos, iw3x, iw3y,
                            out Rectangle iwWallRPink, out Rectangle iwWallR, out _);

                        // Body-fit box scan (inner green faces + feet bottom).
                        // bBottom stops one pixel above the feet green bottom so the ground
                        // tile itself is never counted — prevents false positives when moving fast.
                        int bLeft   = iwWallL.Right;
                        int bRight  = iwWallR.Left - 2;
                        int bTop    = Math.Min(iwWallL.Y, iwWallR.Y);
                        int bBottom = iwFeet.Bottom - 2 - 3;

                        int tLeft   = bLeft  / 16;
                        int tRight  = (bRight  - 1) / 16;
                        int tTop    = bTop    / 16;
                        int tBottom = (bBottom - 1) / 16;

                        int total = 0, solid = 0;
                        for (int tx = tLeft; tx <= tRight; tx++)
                        {
                            for (int ty = tTop; ty <= tBottom; ty++)
                            {
                                total++;
                                if (tx < 0 || ty < 0 || tx >= Main.maxTilesX || ty >= Main.maxTilesY)
                                { solid++; continue; }
                                Tile t = Main.tile[tx, ty];
                                if (t.HasTile && Main.tileSolid[t.TileType]
                                    && !Main.tileSolidTop[t.TileType] && !t.IsActuated
                                    && t.Slope == SlopeType.Solid && !t.IsHalfBlock)
                                    solid++;
                            }
                        }
                        bool overallCoverage = total > 0 && (float)solid / total >= 0.25f;

                        int tCenterX = (bLeft + bRight) / 2 / 16;
                        bool spineSolid = true;
                        for (int ty = tTop; ty <= tBottom; ty++)
                        {
                            if (tCenterX < 0 || ty < 0 || tCenterX >= Main.maxTilesX || ty >= Main.maxTilesY)
                                continue;
                            Tile t = Main.tile[tCenterX, ty];
                            if (!(t.HasTile && Main.tileSolid[t.TileType]
                                  && !Main.tileSolidTop[t.TileType] && !t.IsActuated))
                            { spineSolid = false; break; }
                        }
                        if (tTop > tBottom) spineSolid = false;

                        // Orange box scan (wall probe pink rects) — 40% coverage triggers inwall.
                        int obLeft   = iwWallLPink.Left;
                        int obRight  = iwWallRPink.Right - 2;
                        int obTop    = Math.Min(iwWallLPink.Top,    iwWallRPink.Top);
                        int obBottom = Math.Max(iwWallLPink.Bottom, iwWallRPink.Bottom);

                        int obTLeft   = obLeft  / 16;
                        int obTRight  = (obRight  - 1) / 16;
                        int obTTop    = obTop    / 16;
                        int obTBottom = (obBottom - 1) / 16;

                        int obTotal = 0, obSolid = 0;
                        for (int tx = obTLeft; tx <= obTRight; tx++)
                        {
                            for (int ty = obTTop; ty <= obTBottom; ty++)
                            {
                                obTotal++;
                                if (tx < 0 || ty < 0 || tx >= Main.maxTilesX || ty >= Main.maxTilesY)
                                { obSolid++; continue; }
                                Tile t = Main.tile[tx, ty];
                                if (t.HasTile && Main.tileSolid[t.TileType]
                                    && !Main.tileSolidTop[t.TileType] && !t.IsActuated)
                                    obSolid++;
                            }
                        }
                        bool orangeCoverage = obTotal > 0 && (float)obSolid / obTotal >= 0.25f;

                        _inWall = overallCoverage || spineSolid || orangeCoverage;
                        _dbgOverallCoverage = total > 0 ? (float)solid / total : 0f;
                        _dbgOrangeCoverage  = obTotal > 0 ? (float)obSolid / obTotal : 0f;
                    }

                    // Escape-target search and teleport state machine — owner-only.
                    // Phase 1: _inWall true → _inWallStuckTimer counts up to InWallStuckThreshold.
                    // Phase 2: threshold reached → _inWallTeleportTimer set to InWallTeleportDuration;
                    //          target locked, cannot be changed, stuck timer halted.
                    // Phase 3: teleport timer reaches 0 → teleport executed, everything reset.
                    if (Main.myPlayer == Projectile.owner)
                    {
                        if (_inWallTeleportTimer > 0)
                        {
                            // ── Teleport phase: target is locked, count down ──
                            _inWallTeleportTimer--;

                            if (_inWallTeleportTimer == 0)
                            {
                                // Execute teleport.
                                if (_inWallEscapeTarget != Vector2.Zero)
                                {
                                    // Burst: source position (where she was).
                                    if (Main.netMode != NetmodeID.Server)
                                    {
                                        // Stop wind-up loop and play completion sting (mirrors transform end).
                                            if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound tpDone))
                                                tpDone.Stop();
                                            if (SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound tpDestDone))
                                                tpDestDone.Stop();
                                        SpawnTeleportBurst(Projectile.Center);
                                        SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                                    }

                                    // Align body-fit box bottom (spritePos.Y + 76) with the footprint bottom.
                                    const float bBottomOffset     = 76f;
                                    float footprintHalfPx = FollowPathFootprintHeight * 8f; // 3 * 8 = 24
                                    Projectile.position = new Vector2(
                                        _inWallEscapeTarget.X - Projectile.width  * 0.5f,
                                        _inWallEscapeTarget.Y + footprintHalfPx - bBottomOffset);
                                    Projectile.velocity  = Vector2.Zero;
                                    Projectile.netUpdate = true;

                                    // Burst: destination position (where she landed).
                                    if (Main.netMode != NetmodeID.Server)
                                    {
                                        SpawnTeleportBurst(Projectile.Center);
                                        // Play without position so it's always audible even when
                                        // the destination is far off-screen.
                                        SoundEngine.PlaySound(SoundID.Item4);
                                    }
                                }
                                // Reset all in-wall escape state.
                                bool wasInWallEscape = _pathTeleportTimer <= 0 && _idleTeleportTimer <= 0;
                                _inWallEscapeTarget = Vector2.Zero;
                                _inWallStuckTimer   = 0;
                                _tpActiveDuration   = 0;

                                // Re-enable probes after a pure in-wall escape teleport so she
                                // snaps back to normal collision handling at the new position.
                                if (wasInWallEscape)
                                    ProbesActive = true;

                                // If this was a path-teleport, clear the path so she replans from
                                // the new position rather than resuming the old broken path.
                                if (_pathTeleportTimer > 0)
                                {
                                    _pathTeleportTimer    = 0;
                                    _pathTeleportTarget   = Vector2.Zero;
                                    _followPath.Clear();
                                    _followPathLastGoal   = Vector2.Zero;
                                    Projectile.netUpdate  = true;
                                }

                                // If this was an idle far-teleport, clear its state.
                                if (_idleTeleportTimer > 0)
                                {
                                    _idleTeleportTimer  = 0;
                                    _idleTeleportTarget = Vector2.Zero;
                                }
                            }
                        }
                        else if (_inWall)
                        {
                            // ── Phase 1: accumulate stuck time ──
                            _inWallStuckTimer++;

                            // Search for escape target every tick while stuck so it stays fresh.
                            // Priority target depends on separation state:
                            //   _cursedSeparated true  → bias toward player (she wants to get back)
                            //   _cursedSeparated false → bias toward marked location (continue the path)
                            int iwOriginX = (int)Math.Floor(Projectile.Center.X / 16f) - FollowPathFootprintWidth  / 2;
                            int iwOriginY = (int)Math.Floor(Projectile.Center.Y / 16f) - FollowPathFootprintHeight / 2;
                            var iwOrigin = new Microsoft.Xna.Framework.Point(iwOriginX, iwOriginY);

                            Vector2 iwPriority = _cursedSeparated
                                ? (_followMarkedPosition != Vector2.Zero ? _followMarkedPosition : player.Center)
                                : player.Center;

                            var iwCandidate = SariaPathfinder.NudgeTowardTarget(
                                iwOrigin, FollowPathFootprintWidth, FollowPathFootprintHeight, iwPriority);

                            _inWallEscapeTarget = iwCandidate.X != int.MinValue
                                ? new Vector2(
                                    (iwCandidate.X + FollowPathFootprintWidth  * 0.5f) * 16f,
                                    (iwCandidate.Y + FollowPathFootprintHeight * 0.5f) * 16f)
                                : Vector2.Zero;

                            // Threshold reached → lock target and enter teleport phase.
                            if (_inWallStuckTimer >= InWallStuckThreshold && _inWallEscapeTarget != Vector2.Zero)
                            {
                                // Target is now frozen for the entire teleport wind-up.
                                StartTeleportWindUp(_inWallEscapeTarget, InWallTeleportDuration);
                            }
                        }
                        else
                        {
                            // Not stuck — reset both timers and clear target.
                            _inWallStuckTimer   = 0;
                            _inWallEscapeTarget = Vector2.Zero;
                            // Note: _inWallTeleportTimer is NOT reset here; once teleport is
                            // committed it runs to completion even if _inWall briefly clears.
                        }
                    }

                    SariaDetector.Apply(_detectorConfigs, _detectorResults, Projectile.position,
                        applyProbes, Sleep, ref Projectile.position, ref Projectile.velocity,
                        Projectile.width, Projectile.spriteDirection);

                    // Cache results for debug draw.
                    _dbgHitboxInTile   = _detectorResults[0].Pink;
                    _dbgGroundTouching = _detectorResults[0].Green;
                    _dbgTileBelow      = _detectorResults[0].Yellow;
                    _dbgWallLeft       = _detectorResults[2].Pink;
                    _dbgWallRight      = _detectorResults[3].Pink;
                    _wallPausedLeft    = _detectorResults[2].GreenPaused;
                    _wallPausedRight   = _detectorResults[3].GreenPaused;

                    // Pressure plate trigger — owner only.
                    // Scans the bottom pixel row of Saria's hitbox for any tile that
                    // vanilla marks as a player pressure plate. Fires HitSwitch once on
                    // the rising edge (first tick she overlaps it) and resets when she
                    // fully leaves, matching how vanilla handles player stepping on plates.
                    if (Main.myPlayer == Projectile.owner)
                    {
                        int ppTileY     = (int)((Projectile.position.Y + Projectile.height + -16) / 16f);
                        int ppTileXLeft = (int)(Projectile.position.X / 16f);
                        int ppTileXRight= (int)((Projectile.position.X + Projectile.width - 1) / 16f);

                        bool onPlateNow = false;
                        for (int tx = ppTileXLeft; tx <= ppTileXRight; tx++)
                        {
                            if (tx < 0 || ppTileY < 0 || tx >= Main.maxTilesX || ppTileY >= Main.maxTilesY)
                                continue;
                            Tile t = Main.tile[tx, ppTileY];
                            if (!t.HasTile) continue;
                            // All player-triggerable pressure plate tile types (vanilla IDs).
                            int tt = t.TileType;
                            bool isPlayerPlate = tt == 135  // Red
                                              || tt == 137  // Green
                                              || tt == 138  // Gray
                                              || tt == 262  // Lihzahrd
                                              || tt == 420; // Brown
                            if (!isPlayerPlate) continue;

                            onPlateNow = true;
                                if (!_wasOnPressurePlateLastFrame)
                                {
                                    if (Main.netMode == NetmodeID.MultiplayerClient)
                                        NetMessage.SendData(MessageID.HitSwitch, -1, -1, null, tx, ppTileY);
                                    else
                                        Wiring.HitSwitch(tx, ppTileY);
                                }
                        }
                        _wasOnPressurePlateLastFrame = onPlateNow;
                    }
                    _wasGroundedLastFrame = _dbgGroundTouching;

                    // Fix 2: if the physics detector confirms a wall on Saria's idle side,
                    // reinforce the hold timer so CachedClose doesn't ease outward into the tile.
                    float _distToPlayerForWall = Vector2.Distance(Projectile.Center, player.Center);
                    _closeTracker.ReinforceFromWall(_dbgWallLeft, _dbgWallRight, player.direction, _distToPlayerForWall);

                    // Fix 3: zero out X velocity toward the wall so Saria waits instead of jittering.
                    // Skipped while following A* path so walls on the route don't block her.
                    bool wallOnIdleSide = (player.direction > 0 && _dbgWallRight) || (player.direction < 0 && _dbgWallLeft);
                    if (wallOnIdleSide && _followPath.Count == 0)
                    {
                        float towardWall = player.direction; // +1 right, -1 left
                        if (Math.Sign(Projectile.velocity.X) == (int)towardWall)
                            Projectile.velocity.X = 0f;
                    }
                }

                // Door auto-open/close — only while she is actively traversing an A* trail.
                if (Main.myPlayer == Projectile.owner && _followPath.Count > 0)
                {
                    // Use the orange hitbox — the bounding rect of the two pink wall-probe
                    // rectangles (configs [2] left and [3] right), exactly as drawn by the
                    // debug overlay. This is the narrow center-body box, not the full hitbox.
                    Vector2 spritePos = new Vector2(
                        (float)Math.Round(Projectile.position.X),
                        (float)Math.Round(Projectile.position.Y));

                    SariaDetector.GetFacingDir(_detectorConfigs[2].RotationDegrees, out int owlx, out int owly);
                    SariaDetector.GetFacingDir(_detectorConfigs[3].RotationDegrees, out int owrx, out int owry);
                    SariaDetector.GetProbeRects(in _detectorConfigs[2], spritePos, owlx, owly,
                        out Rectangle orangeWallL, out _, out _);
                    SariaDetector.GetProbeRects(in _detectorConfigs[3], spritePos, owrx, owry,
                        out Rectangle orangeWallR, out _, out _);

                    // Bounding rect of both pink probes (matches the orange box in the debug overlay).
                    int orangeLeft   = orangeWallL.Left;
                    int orangeRight  = orangeWallR.Right;
                    int orangeTop    = Math.Min(orangeWallL.Top,    orangeWallR.Top);
                    int orangeBottom = Math.Max(orangeWallL.Bottom, orangeWallR.Bottom);

                    // Convert pixel rect to tile range, extended 1 tile out on each side in X.
                    int fpLeft   = (int)Math.Floor((float)orangeLeft  / 16f) - 1;
                    int fpRight  = (int)Math.Floor((float)orangeRight / 16f) + 1;
                    int fpTop    = (int)Math.Floor((float)orangeTop    / 16f);
                    int fpBottom = (int)Math.Floor((float)orangeBottom / 16f);

                    var touchingDoors = new System.Collections.Generic.HashSet<Point>();

                    for (int tx = fpLeft; tx <= fpRight; tx++)
                    {
                        for (int ty = fpTop; ty <= fpBottom; ty++)
                        {
                            if (tx < 0 || ty < 0 || tx >= Main.maxTilesX || ty >= Main.maxTilesY)
                                continue;
                            Tile t = Main.tile[tx, ty];
                            if (!t.HasTile) continue;

                            if (t.TileType == TileID.ClosedDoor || t.TileType == TileID.TallGateClosed)
                            {
                                // Normalize ty to the TOP tile of this door column so the stored key
                                // is the same no matter which tile in the 3-tall multi-tile the scan
                                // happens to hit. Without this, a single-pixel shift in Saria's
                                // vertical position changes fpTop/fpBottom, dropping the stored ty
                                // out of the scan range — the open-door tiles then add a different
                                // (tx, otherTy) to touchingDoors, the key isn't found, and the door
                                // is spuriously closed then immediately re-opened on the next tick.
                                int closedTopTy = ty;
                                while (closedTopTy > 0
                                       && Main.tile[tx, closedTopTy - 1].HasTile
                                       && Main.tile[tx, closedTopTy - 1].TileType == t.TileType)
                                    closedTopTy--;

                                // Try spriteDirection first; if blocked try the other side.
                                int dir     = Projectile.spriteDirection;
                                int usedDir = dir;
                                bool opened = WorldGen.OpenDoor(tx, ty, dir);
                                if (!opened)
                                {
                                    usedDir = -dir;
                                    opened  = WorldGen.OpenDoor(tx, ty, -dir);
                                }

                                if (opened)
                                {
                                    // Key is the HINGE column (original tx) at the normalized top tile.
                                    // After opening, the hinge tile keeps TileID.OpenDoor so the scan
                                    // always finds it at the same position, making the key stable.
                                    // Using the panel column (tx+usedDir) was wrong because the panel
                                    // often falls outside the narrow wall-probe scan range, so the key
                                    // was never matched → spurious close+reopen every tick.
                                    var openKey = new Point(tx, closedTopTy);
                                    _sariaOpenedDoors.Add(openKey);
                                    touchingDoors.Add(openKey);
                                    // Broadcast the open so all clients + server update. The owner is
                                    // usually a client, so gating on Server alone never synced. Action 0 = open.
                                    if (Main.netMode != NetmodeID.SinglePlayer)
                                        NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, tx, ty, usedDir);
                                }
                            }

                            if (t.TileType == TileID.OpenDoor || t.TileType == TileID.TallGateOpen)
                            {
                                // Normalize to the top tile of this open door column so the key here
                                // matches the key stored in _sariaOpenedDoors on the open tick.
                                int openTopTy = ty;
                                while (openTopTy > 0
                                       && Main.tile[tx, openTopTy - 1].HasTile
                                       && Main.tile[tx, openTopTy - 1].TileType == t.TileType)
                                    openTopTy--;
                                touchingDoors.Add(new Point(tx, openTopTy));
                            }
                        }
                    }

                    // Close doors she opened that she's no longer touching.
                    var toClose = new System.Collections.Generic.List<Point>();
                    foreach (Point dp in _sariaOpenedDoors)
                    {
                        if (!touchingDoors.Contains(dp))
                            toClose.Add(dp);
                    }
                    foreach (Point dp in toClose)
                    {
                        _sariaOpenedDoors.Remove(dp);
                        if (dp.X >= 0 && dp.Y >= 0 && dp.X < Main.maxTilesX && dp.Y < Main.maxTilesY)
                        {
                            bool closed = WorldGen.CloseDoor(dp.X, dp.Y, false);
                            // Broadcast the close so all clients + server update. Action 1 = close.
                            if (closed && Main.netMode != NetmodeID.SinglePlayer)
                                NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 1, dp.X, dp.Y, 0);
                        }
                    }
                }
                if (Eating <= 0 && !Sneezing && !Sleep && player.ownedProjectileCounts[ModContent.ProjectileType<FrozenYogurtSignal>()] > 0f && distanceToIdlePosition <= 20 && Projectile.frame < 36)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Healpulse"), player.Center);
                    player.AddBuff(ModContent.BuffType<Soothing>(), 18000);
                    SetMoodFor(MoodState.Happy, 600, priority: 1);
                    Eating = 3;
                    Projectile.frame = 0;
                }
                if (Eating <= 0 && !Sneezing && !Sleep && player.ownedProjectileCounts[ModContent.ProjectileType<Competitivetime>()] > 0f && distanceToIdlePosition <= 20 && Projectile.frame < 36)
                {
                    player.AddBuff(ModContent.BuffType<Overcharged>(), 45000);
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                    SetMoodFor(MoodState.Happy, 800, priority: 1);
                    Eating = 4;
                    Projectile.frame = 0;
                }
                if (Eating == 3 || Eating == 4 || Eating ==5)
                {
                    Projectile.spriteDirection = 1;
                }
                if (player.statLife < (player.statLifeMax2) / 4 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()) && !player.HasBuff(ModContent.BuffType<Sickness>()) && !player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && !player.HasBuff(ModContent.BuffType<EclipseBuff>()))
                {
                    SetMoodFor(MoodState.Sad, 420, priority: 1);
                    player.statLife += 500;
                    player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 3000);
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Heal>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                    }
                }
                if (Projectile.NewIdlePosition(51))
                {
                    if (Projectile.velocity.X >= 0.25)
                    {
                        Projectile.spriteDirection = 1;
                    }
                    if (Projectile.velocity.X <= -0.25)
                    {
                        Projectile.spriteDirection = -1;
                    }
                }
                else if (!Projectile.NewIdlePosition(51))
                {
                    if (Projectile.velocity.X >= 1.25)
                    {
                        Projectile.spriteDirection = 1;
                    }
                    if (Projectile.velocity.X <= -1.25)
                    {
                        Projectile.spriteDirection = -1;
                    }
                }

                if (Projectile.frame == 25 && Sleep && MoveTimer >= 550)
                {
                    Projectile.SneezeDust(ModContent.DustType<Z>(), 40, 1, -10, 3, -12);
                }
                ///Sleep Ai
                if ((Math.Abs(Projectile.velocity.X) >= 0.5f) || (Math.Abs(Projectile.velocity.Y) >= 0.5f))
                {
                    MoveTimer = 0;
                }
                if ((Math.Abs(Projectile.velocity.X) < 0.5f) && (Math.Abs(Projectile.velocity.Y) < 0.5f))
                {
                    int moveTimerRate = HoldingHealBall ? 1 : 2;
                    if (MoveTimer < 10000 && Sleep)
                    {
                        MoveTimer += 1;
                    }
                    else if (MoveTimer < 5000 && !Sleep)
                    {
                        MoveTimer += moveTimerRate;
                    }
                }
                if (SariaTalking && !Sleep)
                {
                    if (MoveTimer >= 277)
                    {
                        MoveTimer = 276;
                    }
                }
                if (MoveTimer == 0)
                {
                    Sleep = false;
                    SleepHeal = 0;
                    Projectile.netUpdate = true;
                }
                if (Sleep && MoveTimer >= 8000 && SleepHeal <= 0 && (Main.myPlayer == Projectile.owner))
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Healpulse"), player.Center);
                    player.AddBuff(ModContent.BuffType<Soothing>(), 44000);
                    SetMoodFor(MoodState.Normal, 180, priority: 0);
                    SleepHeal = 1;
                    if (player.HasBuff(ModContent.BuffType<Drained>()))
                    {
                        player.ClearBuff(ModContent.BuffType<Drained>());
                    }
                }
                if (Sleep && MoveTimer >= 10000 && (Main.myPlayer == Projectile.owner))
                {
                    if (player.HasBuff(ModContent.BuffType<Drained>()))
                    {
                        player.ClearBuff(ModContent.BuffType<Drained>());
                    }
                    if (MoveTimer >= 10000)
                    {
                        player.AddBuff(ModContent.BuffType<Overcharged>(), 30000);
                        SicknessBar = SicknessBarMax;
                        if (SoundTimer <= 0)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                            for (int j = 0; j < 1; j++) //set to 2
                            {
                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                            }
                            SoundTimer = 1;
                        }
                        SetMoodFor(MoodState.Happy, 180, priority: 5);
                        MoveTimer = 0;
                        SoundTimer = 0;
                        Projectile.netUpdate = true;
                    }
                }
                if (player.sleeping.isSleeping && Eating <= 0)
                {
                    if (MoveTimer <= 6000)
                    {
                        MoveTimer = 6000;
                    }
                    if (IsPlayerAsleep && !Sleep)
                    {
                        if (Projectile.frame < 14)
                        {
                            Projectile.frame = 14;
                        }
                    }
                    else if (!IsPlayerAsleep && !Sleep && ChannelState <= 0)
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                        IsPlayerAsleep = true;
                    }
                }
                if (!player.sleeping.isSleeping)
                {
                    IsPlayerAsleep = false;
                }
                if (MoveTimer >= (5000) && Projectile.frame == 19 && !foundTarget && Eating <= 0 && Mood != (int)MoodState.Cursed)
                {
                    Sleep = true;
                    Projectile.netUpdate = true;
                }
                ///eatingAI
                if (Projectile.frame == 25 && (Eating == 3 || Eating == 4))
                {
                    Projectile.SneezeDust(ModContent.DustType<Fog>(), 1, 1, -10, 10, -17);
                }
                if (Projectile.frame == 37 && (Eating == 3 || Eating == 4))
                {
                    Eating = 5;
                    Projectile.frame = 22;
                    Vector2 Throw = Projectile.Center;
                    Throw.Y += 0f;
                    Throw.X += 40f;
                    Vector2 ThrowToo = Projectile.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero);
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Throw, ThrowToo * 10, ModContent.ProjectileType<EmptyCup>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                }
                
                ///end of sleep ai
                // Update psychic eye overlay opacity (fade-in during charge/attack/flash cooldown, ~2s fade-out after)
                bool flashCooldownActive = FlashCooldownTimer > 0;
                SariaPsychicEyes.UpdateOpacity(Projectile, ChannelState, Transform, flashCooldownActive);
                // Visual-only eye-tracking tick — runs on EVERY client so remote
                // observers see Saria's eyes follow her owner just like the owner does.
                // Reads only synced state (_eyeFreeMode, _eyeLookingBack, projectile.owner)
                // so it cannot desync gameplay.
                IdleAnimator.UpdateEyeOffsetVisual(Projectile, (int)Transform);

                // Non-owner clients: force states directly from synced bools every tick
                // so the transition animations never flicker between packets.
                if (Projectile.owner != Main.myPlayer)
                {
                    //IdleAnimator.ApplySyncedLegState(LegsIsCasual, LegsGoingToCasual, LegsIsProper, LegsGoingToProper); // commented out to verify eye fix independently
                    IdleAnimator.ApplySyncedLegState(LegsIsCasual, LegsGoingToCasual, LegsIsProper, LegsGoingToProper);
                    IdleAnimator.ApplySyncedArmState(ArmsIsDown, ArmsGoingUp, ArmsIsUp, ArmsGoingDown);
                    IdleAnimator.ApplySyncedEyeState(EyesLooking, EyesBlinking, EyesOpening);
                    IdleAnimator.DisplayedMood = DisplayedMoodSync;

                    // Detect transform start/end from synced TransformTimer for non-owner sound playback
                    if (Main.netMode != NetmodeID.Server)
                    {
                        bool remoteJustStarted = _prevTransformTimerRemote <= 0 && TransformTimer > 0;
                        bool remoteJustEnded   = _prevTransformTimerRemote > 0  && TransformTimer == 0;
                        if (remoteJustStarted)
                        {
                            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                            if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound old))
                                old.Stop();
                            _transformLoopSlot = SoundEngine.PlaySound(
                                new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                                Projectile.Center);
                        }
                        else if (remoteJustEnded)
                        {
                            if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound done))
                                done.Stop();
                            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                        }

                        // Keep loop position tracking Saria every tick
                        if (TransformTimer > 0 && SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound active))
                            active.Position = Projectile.Center;

                        _prevTransformTimerRemote = TransformTimer;
                    }
                }

                // Teleport wind-up detection — outside the non-owner block so it runs for all clients.
                if (Main.netMode != NetmodeID.Server && Projectile.owner != Main.myPlayer)
                {
                    bool tpJustStarted = _prevTeleportTimerRemote <= 0 && _inWallTeleportTimer > 0;
                    bool tpJustEnded   = _prevTeleportTimerRemote > 0  && _inWallTeleportTimer == 0;

                    if (tpJustStarted)
                    {
                        // Cache both positions now — by the time tpJustEnded fires the
                        // netUpdate will have snapped Projectile.Center and zeroed _inWallEscapeTarget.
                        _tpCachedSrc  = Projectile.Center;
                        _tpCachedDest = _inWallEscapeTarget;

                        SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                        if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound tpOld))
                            tpOld.Stop();
                        _tpLoopSlot = SoundEngine.PlaySound(
                            new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                            Projectile.Center);
                        for (int _i = 0; _i < 20; _i++)
                        {
                            Vector2 _vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 4f);
                            Dust _d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), _vel, Scale: 1.4f);
                            _d.noGravity = true;
                        }
                    }
                    else if (tpJustEnded)
                    {
                        if (SoundEngine.TryGetActiveSound(_tpLoopSlot,     out ActiveSound src)) src.Stop();
                        if (SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound dst)) dst.Stop();
                        SoundEngine.PlaySound(SoundID.Item4, _tpCachedSrc != Vector2.Zero ? _tpCachedSrc : Projectile.Center);
                        SoundEngine.PlaySound(SoundID.Item4);
                        // Source burst — at Saria's position before the teleport.
                        SpawnTeleportBurst(_tpCachedSrc != Vector2.Zero ? _tpCachedSrc : Projectile.Center);
                        // Destination burst — at the locked teleport target.
                        if (_tpCachedDest != Vector2.Zero)
                        {
                            SpawnTeleportBurst(_tpCachedDest);
                        }
                        _tpCachedSrc  = Vector2.Zero;
                        _tpCachedDest = Vector2.Zero;
                    }

                    // Snapshot BEFORE the decrement so that on the tick after the countdown
                    // reaches 0, _prevTeleportTimerRemote is still positive and tpJustEnded fires.
                    _prevTeleportTimerRemote = _inWallTeleportTimer;

                    if (_inWallTeleportTimer > 0)
                    {
                        _inWallTeleportTimer--;
                        TickTeleportPhase();
                        if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound tpActive))
                            tpActive.Position = Projectile.Center;
                    }
                }
                if (Projectile.owner == Main.myPlayer)
                {
                    bool newLegsIsCasual      = IdleAnimator.CurrentLegState == SariaIdleAnimator.LegState.Casual;
                    bool newLegsGoingToCasual  = IdleAnimator.CurrentLegState == SariaIdleAnimator.LegState.GoingToCasual;
                    bool newLegsIsProper       = IdleAnimator.CurrentLegState == SariaIdleAnimator.LegState.Proper;
                    bool newLegsGoingToProper  = IdleAnimator.CurrentLegState == SariaIdleAnimator.LegState.GoingToProper;
                    bool newArmsIsDown         = IdleAnimator.CurrentArmState == SariaIdleAnimator.ArmState.Down;
                    bool newArmsGoingUp        = IdleAnimator.CurrentArmState == SariaIdleAnimator.ArmState.GoingUp;
                    bool newArmsIsUp           = IdleAnimator.CurrentArmState == SariaIdleAnimator.ArmState.Up;
                    bool newArmsGoingDown      = IdleAnimator.CurrentArmState == SariaIdleAnimator.ArmState.GoingDown;
                    bool newEyesLooking        = IdleAnimator.CurrentEyeState == SariaIdleAnimator.EyeState.Looking;
                    bool newEyesBlinking       = IdleAnimator.CurrentEyeState == SariaIdleAnimator.EyeState.Blinking;
                    bool newEyesOpening        = IdleAnimator.CurrentEyeState == SariaIdleAnimator.EyeState.Opening;
                    int newDisplayedMoodSync   = IdleAnimator.DisplayedMood;
                    if (Projectile.frame != frameToSync || Projectile.spriteDirection != directionToSync
                        || newLegsIsCasual != LegsIsCasual || newLegsGoingToCasual != LegsGoingToCasual
                        || newLegsIsProper != LegsIsProper || newLegsGoingToProper != LegsGoingToProper
                        || newArmsIsDown != ArmsIsDown || newArmsGoingUp != ArmsGoingUp
                        || newArmsIsUp != ArmsIsUp || newArmsGoingDown != ArmsGoingDown
                        || newEyesLooking != EyesLooking || newEyesBlinking != EyesBlinking
                        || newEyesOpening != EyesOpening || newDisplayedMoodSync != DisplayedMoodSync)
                    {
                        frameToSync = Projectile.frame;
                        syncedFrameCounter = Projectile.frameCounter;
                        directionToSync = Projectile.spriteDirection;
                        LegsIsCasual     = newLegsIsCasual;
                        LegsGoingToCasual = newLegsGoingToCasual;
                        LegsIsProper     = newLegsIsProper;
                        LegsGoingToProper = newLegsGoingToProper;
                        ArmsIsDown    = newArmsIsDown;
                        ArmsGoingUp   = newArmsGoingUp;
                        ArmsIsUp      = newArmsIsUp;
                        ArmsGoingDown = newArmsGoingDown;
                        EyesLooking  = newEyesLooking;
                        EyesBlinking = newEyesBlinking;
                        EyesOpening  = newEyesOpening;
                        DisplayedMoodSync = newDisplayedMoodSync;
                        Projectile.netUpdate = true;
                    }
                    int frameSpeed = 30; //reduced by half due to framecounter speedup
                    Projectile.frameCounter += 2;
                    if (Projectile.frameCounter >= frameSpeed)
                    {
                        Projectile.frameCounter = 0;
                        if (Projectile.frame >= Main.projFrames[ModContent.ProjectileType<Saria>()]) //error here! you had the wrong projectile id, so the animation did not use the right frames
                        {
                            Projectile.frame = 0;
                        }

                        if (Projectile.ai[0] == 0 || Projectile.ai[0] == 3 || Projectile.ai[0] == 4) //only run these animations if not attacking! no longer overrides
                        {
                            if ((Projectile.velocity.Y) > -1f && (Projectile.velocity.Y) < 1f && Math.Abs(Projectile.velocity.X) <= .25) //Idle animation, notice how I have (
                                                                                                                                         //
                                                                                                                                         //.Y greater than -3f and less than 4f. this DID conflict with the rising and Falling animations but this is how i fixed it.
                            { ////however you set up the attack animation, make sure that none of these other animations override it. 
                              //that's easy legit just
                                Projectile.frame++;
                                if (IdleAnimator.IsActive && Projectile.frame > SariaIdleAnimator.IdleFrameMax && !IsPlayerAsleep)
                                {
                                    Projectile.frame = 0;
                                }
                                if (Projectile.frameCounter <= 36)
                                {
                                    Projectile.frameCounter = 0;
                                }
                                if (Sleep && MoveTimer > 250)
                                {
                                    if (Projectile.frame == 20)
                                    {
                                        Projectile.frame = 22;
                                        PlaySyncedSariaSound(SariaSoundId.Hover);
                                    }
                                    if (Projectile.frame >= 26 && MoveTimer >= 550)
                                    {
                                        Projectile.frame = 22;
                                        PlaySyncedSariaSound(SariaSoundId.Hover);
                                }
                                }
                                ///Charging animation
                                if (NotActive && ChannelState > 0)
                                {
                                    if (Projectile.frame >= 36 || Projectile.frame < 4)
                                    {
                                        if (IsCharging <= 0)
                                        {
                                            Projectile.frame = 4;
                                        }
                                        else
                                        {
                                            Projectile.frame = 8;
                                        }
                                    }
                                    if (Projectile.frame >= 4 && Projectile.frame < 36 && IsCharging <= 0)
                                    {
                                        Projectile.frame = 4;
                                    }
                                    if (Projectile.frame >= 12 && Projectile.frame < 36)
                                    {
                                        Projectile.frame = 8;
                                    }
                                }
                                ////end of charging animation
                                // Timer doubling at frame 22 during sneeze animation
                                if (Projectile.frame == 22 && Sneezing)
                                {
                                    IdleAnimator.OnSneezeDust();
                                }
                                if (Projectile.frame == 26 && (player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] <= 0f) && Sleep)
                                {
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                                }
                                if (Projectile.frame == 23 && !Sleep)
                                {
                                    PlaySyncedSariaSound(SariaSoundId.Step2);
                                }
                                if (Projectile.frame >= 36 && Eating % 5 == 0)
                                {
                                    Projectile.frame = 0;
                                    Eating = 0;
                                    if (Sleep)
                                    {
                                        Sleep = false;
                                    }
                                    PlaySyncedSariaSound(SariaSoundId.Step1);
                                }
                                // Sneeze animation completion — frame 35 displays for one tick, wraps at 36
                                if (Sneezing && Projectile.frame > 36)
                                {
                                    Projectile.frame = 0;
                                    Sneezing = false;
                                    BloodSneeze = false;
                                }
                                ////this is the random heal timer for when player is standng still. this one will need to be reworked to be on a seperate timer during idle animation.
                                if (Projectile.frame == 18 && player.statLife < ((player.statLifeMax2) - (player.statLifeMax2 / 4)) && !player.HasBuff(ModContent.BuffType<Healpulse2Buff>()))
                                                {
                                                    SetMoodFor(MoodState.Sad, 420, priority: 1);
                                                    player.statLife += 500;
                                                    if (!player.HasBuff(ModContent.BuffType<Healpulse2Buff>()))
                                                    {
                                                        for (int j = 0; j < 1; j++) //set to 2
                                                        {
                                                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Heal>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                                                                }
                                            }
                                            player.AddBuff(ModContent.BuffType<Healpulse2Buff>(), 3000);
                                }
                            }
                            if ((Projectile.velocity.Y) < 4f && Math.Abs(Projectile.velocity.X) > 0.25f && Math.Abs(Projectile.velocity.X) < 4f) //walking animation and such
                            {
                                Projectile.frame++;
                                Projectile.frameCounter += 3;
                                if (Projectile.frame <= 40)
                                {
                                    Projectile.frameCounter = 0;
                                }
                                if (Projectile.frame >= 40)
                                {
                                    Projectile.frame = 36;
                                }
                                if (Projectile.frame < 36)
                                {
                                    Projectile.frame = 36;
                                }
                            }
                            if ((Projectile.velocity.Y) < 4f && Math.Abs(Projectile.velocity.X) >= 4f)//running or (floating) animation
                            {
                                Projectile.frame++;
                                if (Projectile.frameCounter < 43)
                                {
                                    Projectile.frameCounter = 0;
                                }
                                if (Projectile.frame >= 43)
                                {
                                    Projectile.frame = 40;
                                    PlaySyncedSariaSound(SariaSoundId.Hover);
                                }
                                if (Projectile.frame < 40)
                                {
                                    Projectile.frame = 40;
                                }
                            }
                            if ((Projectile.velocity.Y) < -1f) //rising animation
                            {
                                Projectile.frame++;
                                if (Projectile.frameCounter < 43)
                                {
                                    Projectile.frameCounter = 0;
                                }
                                if (Projectile.frame >= 43)
                                {
                                    Projectile.frame = 40;
                                }
                                if (Projectile.frame < 40)
                                {
                                    Projectile.frame = 40;
                                    PlaySyncedSariaSound(SariaSoundId.Fly);
                                }
                            }
                            if ((Projectile.velocity.Y > 4f && Math.Abs(Projectile.velocity.X) > 0.25f) || (Projectile.velocity.Y > 1f && Math.Abs(Projectile.velocity.X) < 0.25f)) //falling while nearly still
                            {
                                Projectile.frame++;
                                if (Projectile.frameCounter < 99)
                                {
                                    Projectile.frameCounter = 0;
                                }
                                if (Projectile.frame >= 99)
                                {
                                    Projectile.frame = 97;
                                }
                                if (Projectile.frame < 97)
                                {
                                    Projectile.frame = 97;
                                }
                            }
                        }
                        Projectile.SariaAttacks((int)Transform, (int)CantAttackTimer, (int)ChannelAttack, (bool)foundTarget, (Vector2)targetCenter);
                    }
                    // --- Sneeze Timer (ticks constantly, not just during idle) ---
                    // Biome rates with indoor/outdoor distinction
                    if (player.behindBackWall && player.HasBuff(BuffID.Campfire))
                    {
                        IdleAnimator.SneezeBiomeRate = 0.05f;
                    }
                    else
                    {
                        bool isIndoors = player.behindBackWall;
                        if (player.ZoneSnow)
                            IdleAnimator.SneezeBiomeRate = isIndoors ? 1.3f : 3.0f;
                        else if (player.ZoneJungle)
                            IdleAnimator.SneezeBiomeRate = isIndoors ? 1.0f : 3.0f;
                        else if (player.ZoneForest)
                            IdleAnimator.SneezeBiomeRate = isIndoors ? 1.0f : 2.0f;
                        else if (player.ZoneSkyHeight)
                            IdleAnimator.SneezeBiomeRate = 1.5f;
                        else if (player.ZoneDesert && !Main.dayTime)
                            IdleAnimator.SneezeBiomeRate = 1.5f;
                        else if (player.ZoneHallow)
                            IdleAnimator.SneezeBiomeRate = 0.2f;
                        else
                            IdleAnimator.SneezeBiomeRate = 1.0f;

                        // Rain bonus (outdoor only, not in Zora/water form)
                        if (player.ZoneRain && !isIndoors && Transform != 1)
                            IdleAnimator.SneezeBiomeRate = Math.Max(IdleAnimator.SneezeBiomeRate, 2.0f);

                        // Biome weakness bonus — StatLower means she's struggling,
                        // stacks +3 on top of whatever the biome already gives
                        if (player.HasBuff(ModContent.BuffType<StatLower>()))
                            IdleAnimator.SneezeBiomeRate += 3.0f;
                    }

                    // Cursed — flat addition like StatLower, stacks aggressively
                    if (Cursed)
                        IdleAnimator.SneezeBiomeRate += 4.0f;

                    // Standing still long enough to sleep — sneeze builds faster
                    // (outside the campfire if/else so it stacks even indoors)
                    if (MoveTimer >= 5000)
                        IdleAnimator.SneezeBiomeRate += HoldingHealBall ? 1.0f : 1.5f;

                    // Boss alive → suppress sneezing entirely (timer stays at max)
                    bool bossAlive = false;
                    for (int b = 0; b < Main.maxNPCs; b++)
                    {
                        if (Main.npc[b].active && Main.npc[b].boss) { bossAlive = true; break; }
                    }

                    if (bossAlive)
                        IdleAnimator.ResetSneezeTimer();
                    else if (!Sneezing)
                        IdleAnimator.TickSneezeTimer();

                    // --- Idle Animator (runs AFTER frame advance so Update sees the same
                    //     Projectile.frame that Draw will see — eliminates 1-tick desync
                    //     that caused flicker/jitter on state transitions) ---
                    bool isIdleForAnimator =
                        (Projectile.ai[0] == 0 || Projectile.ai[0] == 3 || Projectile.ai[0] == 4)
                        && Projectile.velocity.Y > -1f && Projectile.velocity.Y < 1f
                        && Math.Abs(Projectile.velocity.X) <= 0.25f
                        && !Sleep && !IsPlayerAsleep && Eating <= 0 && ChannelState <= 0;

                    if (isIdleForAnimator)
                    {
                        if (!IdleAnimator.IsActive)
                        {
                            // Failsafe: if idle conditions are met but frame is stuck
                            // between idle range and walking range (4-35), reset to 0.
                            // Skip if Sneezing — the sneeze animation uses frames 12-35.
                            if (Projectile.frame > SariaIdleAnimator.IdleFrameMax && Projectile.frame < 36 && !Sneezing)
                            {
                                Projectile.frame = 0;
                            }

                            // DON'T activate until frame naturally enters idle range (0-3).
                            if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax)
                            {
                                IdleAnimator.IsActive = true;
                                Projectile.frameCounter = 0; // clean timing on entry
                                Sneezing = false; // Clear sneeze flag when returning to normal idle
                                BloodSneeze = false;
                            }
                        }

                        // Only run idle logic once actually active
                        if (IdleAnimator.IsActive)
                        {
                            // Safety clamp — should rarely fire now that activation is delayed
                            if (Projectile.frame > SariaIdleAnimator.IdleFrameMax)
                            {
                                Projectile.frame = 0;
                            }

                            IdleAnimator.Update(Projectile, (int)Transform);

                            // Sneeze trigger — overlays aligned + warmup complete, start sneeze animation
                            if (IdleAnimator.IsSneezeReady)
                            {
                                Projectile.frame = 12;
                                IdleAnimator.OnSneezeStart();
                                IdleAnimator.Deactivate();
                                Sneezing = true;
                                BloodSneeze = player.HasBuff(ModContent.BuffType<StatLower>()) || Cursed;
                            }
                        }
                    }
                    else if (IdleAnimator.IsActive)
                    {
                        // Normal idle interrupted (movement, etc.) — timer keeps ticking naturally
                        IdleAnimator.Interrupt();
                    }
                    // Note: Sneezing is protected by CanMove=0, so isIdleForAnimator stays true
                    // and we never reach this branch during frames 12-35.
                    // --- End Idle Animator ---

                    // --- Forced Sneeze (biome weakness / StatLower debuff / Cursed) ---
                    // When weakened by biome disadvantage OR cursed, the sneeze fires as soon as
                    // the timer reaches 0 regardless of animation state (walking, flying, etc.).
                    // Only charging and active attack animations are excluded.
                    // CanMove is forced to 0 so she stops even if enemies are nearby.
                    if ((player.HasBuff(ModContent.BuffType<StatLower>()) || Cursed) && IdleAnimator.IsSneezeQueued
                        && !Sneezing && !Sleep && Eating <= 0 && ChannelState <= 0
                        && !(Projectile.frame >= 44 && Projectile.frame <= 55)
                        && Projectile.ai[0] != 1 && Projectile.ai[0] != 2)
                    {
                        Projectile.frame = 12;
                        Projectile.frameCounter = 0;
                        Sneezing = true;
                        BloodSneeze = true;
                        CanMove = 0;
                        IdleAnimator.OnSneezeStart();
                        if (IdleAnimator.IsActive)
                            IdleAnimator.Deactivate();
                    }
                    // --- End Forced Sneeze ---
                }
            }
            // Tick teleport phase every AI tick so it advances even when Saria is off-screen.
            if (Main.netMode != NetmodeID.Server)
                TickTeleportPhase();
        }
        // Bitmask layout for Send/ReceiveExtraAI
        // ushort 1: tile-count biomes
        private const ushort ZoneBit_Snow           = 1 << 0;
        private const ushort ZoneBit_Jungle         = 1 << 1;
        private const ushort ZoneBit_Corrupt        = 1 << 2;
        private const ushort ZoneBit_Crimson        = 1 << 3;
        private const ushort ZoneBit_Hallow         = 1 << 4;
        private const ushort ZoneBit_Desert         = 1 << 5;
        private const ushort ZoneBit_GlowingMushroom= 1 << 6;
        private const ushort ZoneBit_Graveyard      = 1 << 7;
        private const ushort ZoneBit_Meteor         = 1 << 8;
        private const ushort ZoneBit_Forest         = 1 << 9;
        private const ushort ZoneBit_Rain           = 1 << 10;
        private const ushort ZoneBit_Beach          = 1 << 11;
        private const ushort ZoneBit_Dungeon        = 1 << 12;
        private const ushort ZoneBit_Sandstorm      = 1 << 13;
        private const ushort ZoneBit_UndergroundDesert = 1 << 14;
        // ushort 2: depth layers + environment
        private const ushort DepthBit_SkyHeight     = 1 << 0;
        private const ushort DepthBit_Space         = 1 << 1;
        private const ushort DepthBit_Overworld     = 1 << 2;
        private const ushort DepthBit_Underground   = 1 << 3;
        private const ushort DepthBit_DirtLayer     = 1 << 4;
        private const ushort DepthBit_RockLayer     = 1 << 5;
        private const ushort DepthBit_Underworld    = 1 << 6;
        private const ushort EnvBit_Campfire        = 1 << 7;
        private const ushort EnvBit_HeartLantern    = 1 << 8;
        private const ushort EnvBit_StarInBottle    = 1 << 9;
        private const ushort EnvBit_WaterCandle     = 1 << 10;
        private const ushort EnvBit_PeaceCandle     = 1 << 11;
        private const ushort EnvBit_CalmMindCandle  = 1 << 12;
        private const ushort EnvBit_ReajCandle      = 1 << 13;

        private (ushort biomes, ushort depthEnv) PackSariaZones()
        {
            ushort biomes = 0;
            if (SariaZoneSnow)            biomes |= ZoneBit_Snow;
            if (SariaZoneJungle)          biomes |= ZoneBit_Jungle;
            if (SariaZoneCorrupt)         biomes |= ZoneBit_Corrupt;
            if (SariaZoneCrimson)         biomes |= ZoneBit_Crimson;
            if (SariaZoneHallow)          biomes |= ZoneBit_Hallow;
            if (SariaZoneDesert)          biomes |= ZoneBit_Desert;
            if (SariaZoneGlowingMushroom) biomes |= ZoneBit_GlowingMushroom;
            if (SariaZoneGraveyard)       biomes |= ZoneBit_Graveyard;
            if (SariaZoneMeteor)          biomes |= ZoneBit_Meteor;
            if (SariaZoneForest)          biomes |= ZoneBit_Forest;
            if (SariaZoneRain)            biomes |= ZoneBit_Rain;
            if (SariaZoneBeach)           biomes |= ZoneBit_Beach;
            if (SariaZoneDungeon)         biomes |= ZoneBit_Dungeon;
            if (SariaZoneSandstorm)       biomes |= ZoneBit_Sandstorm;
            if (SariaZoneUndergroundDesert) biomes |= ZoneBit_UndergroundDesert;

            ushort depthEnv = 0;
            if (SariaZoneSkyHeight)       depthEnv |= DepthBit_SkyHeight;
            if (SariaZoneSpace)           depthEnv |= DepthBit_Space;
            if (SariaZoneOverworld)       depthEnv |= DepthBit_Overworld;
            if (SariaZoneUnderground)     depthEnv |= DepthBit_Underground;
            if (SariaZoneDirtLayer)       depthEnv |= DepthBit_DirtLayer;
            if (SariaZoneRockLayer)       depthEnv |= DepthBit_RockLayer;
            if (SariaZoneUnderworld)      depthEnv |= DepthBit_Underworld;
            if (SariaHasCampfire)         depthEnv |= EnvBit_Campfire;
            if (SariaHasHeartLantern)     depthEnv |= EnvBit_HeartLantern;
            if (SariaHasStarInBottle)     depthEnv |= EnvBit_StarInBottle;
            if (SariaHasWaterCandle)      depthEnv |= EnvBit_WaterCandle;
            if (SariaHasPeaceCandle)      depthEnv |= EnvBit_PeaceCandle;
            if (SariaHasCalmMindCandle)   depthEnv |= EnvBit_CalmMindCandle;
            if (SariaHasReajCandle)       depthEnv |= EnvBit_ReajCandle;

            return (biomes, depthEnv);
        }

        private void UnpackSariaZones(ushort biomes, ushort depthEnv)
        {
            SariaZoneSnow            = (biomes & ZoneBit_Snow)            != 0;
            SariaZoneJungle          = (biomes & ZoneBit_Jungle)          != 0;
            SariaZoneCorrupt         = (biomes & ZoneBit_Corrupt)         != 0;
            SariaZoneCrimson         = (biomes & ZoneBit_Crimson)         != 0;
            SariaZoneHallow          = (biomes & ZoneBit_Hallow)          != 0;
            SariaZoneDesert          = (biomes & ZoneBit_Desert)          != 0;
            SariaZoneGlowingMushroom = (biomes & ZoneBit_GlowingMushroom) != 0;
            SariaZoneGraveyard       = (biomes & ZoneBit_Graveyard)       != 0;
            SariaZoneMeteor          = (biomes & ZoneBit_Meteor)          != 0;
            SariaZoneForest          = (biomes & ZoneBit_Forest)          != 0;
            SariaZoneRain            = (biomes & ZoneBit_Rain)            != 0;
            SariaZoneBeach           = (biomes & ZoneBit_Beach)           != 0;
            SariaZoneDungeon         = (biomes & ZoneBit_Dungeon)         != 0;
            SariaZoneSandstorm       = (biomes & ZoneBit_Sandstorm)       != 0;
            SariaZoneUndergroundDesert = (biomes & ZoneBit_UndergroundDesert) != 0;

            SariaZoneSkyHeight       = (depthEnv & DepthBit_SkyHeight)    != 0;
            SariaZoneSpace           = (depthEnv & DepthBit_Space)        != 0;
            SariaZoneOverworld       = (depthEnv & DepthBit_Overworld)    != 0;
            SariaZoneUnderground     = (depthEnv & DepthBit_Underground)  != 0;
            SariaZoneDirtLayer       = (depthEnv & DepthBit_DirtLayer)    != 0;
            SariaZoneRockLayer       = (depthEnv & DepthBit_RockLayer)    != 0;
            SariaZoneUnderworld      = (depthEnv & DepthBit_Underworld)   != 0;
            SariaHasCampfire         = (depthEnv & EnvBit_Campfire)       != 0;
            SariaHasHeartLantern     = (depthEnv & EnvBit_HeartLantern)   != 0;
            SariaHasStarInBottle     = (depthEnv & EnvBit_StarInBottle)   != 0;
            SariaHasWaterCandle      = (depthEnv & EnvBit_WaterCandle)    != 0;
            SariaHasPeaceCandle      = (depthEnv & EnvBit_PeaceCandle)    != 0;
            SariaHasCalmMindCandle   = (depthEnv & EnvBit_CalmMindCandle)  != 0;
            SariaHasReajCandle       = (depthEnv & EnvBit_ReajCandle)      != 0;
        }

        /// <summary>
        /// Samples SceneMetrics at Saria's world position to populate her own zone fields.
        /// Only runs on the owner's client — results are synced to other clients via SendExtraAI.
        /// </summary>
        private void UpdateSariaZones()
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            // Only re-scan when Saria has moved at least one tile from the last scan position.
            // When she is stationary the zone flags from the previous scan are preserved as-is,
            // matching the "last biome she landed on" behaviour the user requested.
            if (Vector2.DistanceSquared(Projectile.Center, _lastBiomeScanPos) < BiomeScanMoveThreshold * BiomeScanMoveThreshold)
                return;

            _lastBiomeScanPos = Projectile.Center;

            Point tilePos = new Point((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));
            SceneMetrics metrics = new SceneMetrics();
            SceneMetricsScanSettings settings = new SceneMetricsScanSettings
            {
                BiomeScanCenterPositionInWorld = Projectile.Center,
                ScanOreFinderData = false,
            };
            metrics.ScanAndExportToMain(settings);

            SariaZoneSnow            = metrics.EnoughTilesForSnow;
            SariaZoneJungle          = metrics.EnoughTilesForJungle;
            SariaZoneCorrupt         = metrics.EnoughTilesForCorruption;
            SariaZoneCrimson         = metrics.EnoughTilesForCrimson;
            SariaZoneHallow          = metrics.EnoughTilesForHallow;
            SariaZoneDesert          = metrics.EnoughTilesForDesert;
            // Beach: world-edge proximity + surface depth + at least 300 nearby sand tiles.
            // SceneMetrics deliberately excludes beach sand from SandTileCount (to avoid triggering
            // the desert biome), so we scan the tiles manually within a 60-tile radius instead.
            int beachMargin = (int)(Main.maxTilesX * 0.0905);
            bool nearEdge = (tilePos.X < beachMargin || tilePos.X > Main.maxTilesX - beachMargin)
                            && tilePos.Y < Main.worldSurface;
            int manualSandCount = 0;
            if (nearEdge)
            {
                const int BeachScanRadius = 60;
                int x0 = Math.Max(0, tilePos.X - BeachScanRadius);
                int x1 = Math.Min(Main.maxTilesX - 1, tilePos.X + BeachScanRadius);
                int y0 = Math.Max(0, tilePos.Y - BeachScanRadius);
                int y1 = Math.Min(Main.maxTilesY - 1, tilePos.Y + BeachScanRadius);
                for (int bx = x0; bx <= x1; bx++)
                    for (int by = y0; by <= y1; by++)
                    {
                        Tile t = Main.tile[bx, by];
                        if (t != null && t.HasTile && TileID.Sets.isDesertBiomeSand[t.TileType])
                            manualSandCount++;
                    }
            }
            SariaZoneBeach = nearEdge && manualSandCount >= 300;
            int sariaWall = Main.tile[tilePos.X, tilePos.Y].WallType;
            bool hasDungeonWall = sariaWall == WallID.BlueDungeonUnsafe
                                  || sariaWall == WallID.GreenDungeonUnsafe
                                  || sariaWall == WallID.PinkDungeonUnsafe;
            SariaZoneDungeon         = metrics.DungeonTileCount >= 250 && hasDungeonWall;
            SariaZoneSandstorm       = Terraria.GameContent.Events.Sandstorm.Happening && SariaZoneDesert;
            SariaZoneUndergroundDesert = metrics.EnoughTilesForDesert
                                       && (SariaZoneUnderground || SariaZoneDirtLayer || SariaZoneRockLayer);
            SariaZoneGlowingMushroom = metrics.EnoughTilesForGlowingMushroom;
            SariaZoneGraveyard       = metrics.EnoughTilesForGraveyard;
            SariaZoneMeteor          = metrics.EnoughTilesForMeteor;
            SariaZoneForest          = !SariaZoneJungle && !SariaZoneSnow && !SariaZoneDesert
                                       && !SariaZoneCorrupt && !SariaZoneCrimson && !SariaZoneHallow
                                       && !SariaZoneGlowingMushroom && !SariaZoneGraveyard;
            // Depth layers — matching vanilla player zone thresholds exactly.
            // worldSurface, rockLayer, UnderworldLayer are already in tile coordinates.
            // InSpace()   : tileY < worldSurface * 0.3
            // ZoneSkyHeight: tileY < worldSurface * 0.6  (overlaps Space; Space takes priority)
            // ZoneOverworldHeight: tileY >= worldSurface * 0.6 && < worldSurface
            // ZoneDirtLayerHeight: tileY >= worldSurface && < rockLayer
            // ZoneRockLayerHeight: tileY >= rockLayer && < UnderworldLayer
            // ZoneUnderworldHeight: tileY >= UnderworldLayer
            int y = tilePos.Y;
            float spaceX = (float)Main.maxTilesX / 4200f;
            spaceX *= spaceX;
            SariaZoneSpace = (float)((y - (50f + 10f * spaceX)) / (Main.worldSurface / 5.0)) < 1f;
            SariaZoneSkyHeight   = y < Main.worldSurface * 0.50 && !SariaZoneSpace;
            SariaZoneOverworld   = y >= Main.worldSurface * 0.5 && y < Main.worldSurface;
            SariaZoneUnderground = y >= Main.worldSurface;
            SariaZoneDirtLayer   = y >= Main.worldSurface       && y < Main.rockLayer;
            SariaZoneRockLayer   = y >= Main.rockLayer          && y < Main.UnderworldLayer;
            SariaZoneUnderworld  = y >= Main.UnderworldLayer;
            SariaZoneRain        = Main.raining && !SariaZoneSpace
                                   && !SariaZoneUnderground
                                   && !SariaZoneDirtLayer
                                   && !SariaZoneRockLayer
                                   && !SariaZoneUnderworld;
            // Environment
            SariaHasCampfire         = metrics.HasCampfire;
            SariaHasHeartLantern     = metrics.HasHeartLantern;
            SariaHasStarInBottle     = metrics.HasStarInBottle;
            SariaHasWaterCandle      = metrics.WaterCandleCount > 0;
            SariaHasPeaceCandle      = metrics.PeaceCandleCount > 0;
            RefreshCandleEnvironment();

            // --- Modded biome detection via temporary player relocation ---
            // Briefly move the owner to Saria's center (same trick used by SariaSpawnSystem for
            // NPC spawning) so that ModBiome.IsBiomeActive(player) evaluates Saria's tile environment.
            // SceneMetrics was already scanned at Saria's position above, so tile-count based
            // mod biomes will see the correct data.  Vanilla zone flags on the player are also
            // patched from those results so biomes that cross-check player.ZoneJungle etc. work too.
            Player owner = Main.player[Projectile.owner];
            Vector2 savedPos      = owner.position;
            bool savedJungle      = owner.ZoneJungle;
            bool savedSnow        = owner.ZoneSnow;
            bool savedCrimson     = owner.ZoneCrimson;
            bool savedCorrupt     = owner.ZoneCorrupt;
            bool savedHallow      = owner.ZoneHallow;
            bool savedDesert      = owner.ZoneDesert;
            bool savedMushroom    = owner.ZoneGlowshroom;
            bool savedGraveyard   = owner.ZoneGraveyard;
            bool savedMeteor      = owner.ZoneMeteor;
            bool savedBeach       = owner.ZoneBeach;
            bool savedDungeon     = owner.ZoneDungeon;
            bool savedSky         = owner.ZoneSkyHeight;
            bool savedOverworld   = owner.ZoneOverworldHeight;
            bool savedDirt        = owner.ZoneDirtLayerHeight;
            bool savedRock        = owner.ZoneRockLayerHeight;
            bool savedUnderworld  = owner.ZoneUnderworldHeight;

            try
            {
                // Relocate owner to Saria's center (no velocity change needed for a pure logic query).
                owner.position = Projectile.Center - new Vector2(owner.width * 0.5f, owner.height * 0.5f);

                // Apply Saria's vanilla zone flags so mod biomes that read player.ZoneJungle etc.
                // see the right area rather than the player's actual location.
                owner.ZoneJungle         = SariaZoneJungle;
                owner.ZoneSnow           = SariaZoneSnow;
                owner.ZoneCrimson        = SariaZoneCrimson;
                owner.ZoneCorrupt        = SariaZoneCorrupt;
                owner.ZoneHallow         = SariaZoneHallow;
                owner.ZoneDesert         = SariaZoneDesert;
                owner.ZoneGlowshroom      = SariaZoneGlowingMushroom;
                owner.ZoneGraveyard      = SariaZoneGraveyard;
                owner.ZoneMeteor         = SariaZoneMeteor;
                owner.ZoneBeach          = SariaZoneBeach;
                owner.ZoneDungeon        = SariaZoneDungeon;
                owner.ZoneSkyHeight      = SariaZoneSkyHeight;
                owner.ZoneOverworldHeight  = SariaZoneOverworld;
                owner.ZoneDirtLayerHeight  = SariaZoneDirtLayer;
                owner.ZoneRockLayerHeight  = SariaZoneRockLayer;
                owner.ZoneUnderworldHeight = SariaZoneUnderworld;

                _sariaActiveModBiomeTypes.Clear();
                foreach (ModBiome biome in ModContent.GetContent<ModBiome>())
                {
                    try
                    {
                        if (biome.IsBiomeActive(owner))
                            _sariaActiveModBiomeTypes.Add(biome.Type);
                    }
                    catch { /* defensive: guard against misbehaving mod biomes */ }
                }
            }
            finally
            {
                // Always restore the owner's real state before any rendering or net code runs.
                owner.position            = savedPos;
                owner.ZoneJungle          = savedJungle;
                owner.ZoneSnow            = savedSnow;
                owner.ZoneCrimson         = savedCrimson;
                owner.ZoneCorrupt         = savedCorrupt;
                owner.ZoneHallow          = savedHallow;
                owner.ZoneDesert          = savedDesert;
                owner.ZoneGlowshroom      = savedMushroom;
                owner.ZoneGraveyard       = savedGraveyard;
                owner.ZoneMeteor          = savedMeteor;
                owner.ZoneBeach           = savedBeach;
                owner.ZoneDungeon         = savedDungeon;
                owner.ZoneSkyHeight         = savedSky;
                owner.ZoneOverworldHeight   = savedOverworld;
                owner.ZoneDirtLayerHeight   = savedDirt;
                owner.ZoneRockLayerHeight   = savedRock;
                owner.ZoneUnderworldHeight  = savedUnderworld;
            }

            Projectile.netUpdate = true;
        }

        /// <summary>
        /// Rescans Saria's candle environment LIVE — no movement gating, no owner gating.
        /// The half-extents match the vanilla buff-scan area a real player standing at her
        /// position would get (falling back to the old 30-tile radius if the screen size
        /// is unavailable, e.g. dedicated servers), so the debug panel's detection range
        /// and the spawn system's application range always agree.
        /// </summary>
        public void RefreshCandleEnvironment()
        {
            int halfW = Math.Max(30, Main.buffScanAreaWidth / 2);
            int halfH = Math.Max(30, Main.buffScanAreaHeight / 2);
            int centerX = (int)(Projectile.Center.X / 16f);
            int centerY = (int)(Projectile.Center.Y / 16f);
            int calmType = ModContent.TileType<Tiles.CalmingCandleTile>();
            int reajType = ModContent.TileType<Tiles.ReajCandleTile>();
            bool calm = false, reaj = false;
            for (int x = centerX - halfW; x <= centerX + halfW && !(calm && reaj); x++)
            {
                for (int y = centerY - halfH; y <= centerY + halfH; y++)
                {
                    if (!WorldGen.InWorld(x, y))
                        continue;
                    Tile t = Main.tile[x, y];
                    if (t == null || !t.HasTile)
                        continue;
                    if (t.TileType == calmType) calm = true;
                    else if (t.TileType == reajType) reaj = true;
                    if (calm && reaj)
                        break;
                }
            }
            SariaHasCalmMindCandle = calm;
            SariaHasReajCandle = reaj;
        }

        /// <summary>
        /// Scans tiles within a square radius around Saria for a given tile type.
        /// Only valid on the owner's client — always returns false on non-owners.
        /// </summary>
        public bool ScanTilesInRadius(int tileType, int radius)
        {
            if (Main.myPlayer != Projectile.owner)
                return false;

            int centerX = (int)(Projectile.Center.X / 16f);
            int centerY = (int)(Projectile.Center.Y / 16f);

            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    if (WorldGen.InWorld(x, y) && Main.tile[x, y].HasTile && Main.tile[x, y].TileType == tileType)
                        return true;
                }
            }
            return false;
        }

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
        /// <summary>
        /// Updates teleport glow-sphere state. Scale grows 0→1 over the teleport wind-up duration.
        /// <summary>
        /// Starts the teleport wind-up to a specific world position.
        /// Safe to call from outside (e.g. FeelingRod). Owner-only — caller must guard with Main.myPlayer == Projectile.owner.
        /// Uses IdleTeleportDuration (2 seconds) as the wind-up time.
        /// </summary>
        public void StartForcedTeleport(Vector2 worldTarget)
        {
            if (_inWallTeleportTimer > 0 || _pathTeleportTimer > 0 || _idleTeleportTimer > 0)
                return; // already in a teleport wind-up

            _idleTeleportTarget  = worldTarget;
            _idleTeleportTimer   = IdleTeleportDuration;
            StartTeleportWindUp(worldTarget, IdleTeleportDuration);
        }

        /// <summary>
        /// Initiates the teleport wind-up: locks the escape target, starts the countdown timer,
        /// plays the sting + loop sound, and spawns the initial dust burst.
        /// Callers must set any scenario-specific fields (_pathTeleportTimer, _idleTeleportTimer, etc.)
        /// BEFORE calling this method.
        /// </summary>
        private void StartTeleportWindUp(Vector2 position, int duration)
        {
            _inWallEscapeTarget  = position;
            _inWallTeleportTimer = duration;
            _tpActiveDuration    = duration;
            Projectile.netUpdate = true;

            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound tpOld))
                    tpOld.Stop();
                _tpLoopSlot = SoundEngine.PlaySound(
                    new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                    Projectile.Center);
                for (int _i = 0; _i < 20; _i++)
                {
                    Vector2 _vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 4f);
                    Dust _d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), _vel, Scale: 1.4f);
                    _d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Spawns the teleport burst visual effect (large + small dust rings) at the given world position.
        /// </summary>
        private void SpawnTeleportBurst(Vector2 position)
        {
            if (Main.netMode == NetmodeID.Server) return;
            for (int _i = 0; _i < 70; _i++)
            {
                Vector2 _vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(4f, 12f);
                Dust _d = Dust.NewDustPerfect(position, ModContent.DustType<AbsorbPsychic>(), _vel, Scale: 1.8f);
                _d.noGravity = true;
            }
            for (int _i = 0; _i < 25; _i++)
            {
                Vector2 _vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 3f);
                Dust _d = Dust.NewDustPerfect(position, ModContent.DustType<AbsorbPsychic>(), _vel, Scale: 1.2f);
                _d.noGravity = true;
            }
        }

        /// <summary>
        /// Ages, rotates, and despawns expired pillars in the given list.
        /// Spawns new pillars to keep the count at 3 (non-server only).
        /// Shared by TickTransformPhase and TickTeleportPhase.
        /// </summary>
        private void TickPillarList(List<TransformPillar> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                TransformPillar p = list[i];
                p.Life  -= 1f;
                p.Angle += p.RotSpeed;
                list[i]  = p;
                if (p.Life <= 0f) list.RemoveAt(i);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                while (list.Count < 3)
                {
                    float sizeRoll = Main.rand.NextFloat(1f);
                    float maxLen   = sizeRoll < 0.60f
                        ? Main.rand.NextFloat(280f, 400f)
                        : Main.rand.NextFloat(150f, 250f);
                    float baseWidth = sizeRoll < 0.60f
                        ? Main.rand.NextFloat(10f, 18f)
                        : Main.rand.NextFloat(7f, 11f);
                    float life = Main.rand.NextFloat(90f, 160f);
                    list.Add(new TransformPillar
                    {
                        Angle     = Main.rand.NextFloat(MathHelper.TwoPi),
                        RotSpeed  = Main.rand.NextFloat(0.003f, 0.009f) * (Main.rand.NextBool() ? 1f : -1f),
                        Length    = 0f,
                        MaxLength = maxLen,
                        Width     = baseWidth,
                        Life      = life,
                        MaxLife   = life,
                    });
                }
            }
        }

        /// <summary>
        /// Drives both the source (Saria) and destination sphere via shared phase values.
        /// Called once per PostDraw before any teleport draw calls.
        /// </summary>
        private void TickTeleportPhase()
        {
            if (_inWallTeleportTimer <= 0)
            {
                // Not in teleport phase — clear everything.
                _tpSphereScale   = 0f;
                _tpPulsePhase    = 0f;
                _tpActiveDuration = 0;
                _tpSourceGlobs.Clear();
                _tpSourcePillars.Clear();
                _tpDestGlobs.Clear();
                _tpDestPillars.Clear();
                if (SoundEngine.TryGetActiveSound(_tpLoopSlot,     out ActiveSound orphan))  orphan.Stop();
                if (SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound orphan2)) orphan2.Stop();
                return;
            }

            // Scale: grows from 0 at timer start to 1 at timer = 0.
            // Use the stored active duration so both 30-tick (in-wall) and
            // 300-tick (path-blocked) wind-ups reach full scale at the end.
            int activeDur = _tpActiveDuration > 0 ? _tpActiveDuration : InWallTeleportDuration;
            float progress = Math.Clamp(
                (float)(activeDur - _inWallTeleportTimer) / activeDur,
                0f, 1f);
            _tpSphereScale  = progress;
            _tpPulsePhase  += 0.08f;
            _tpWavePhase   += 0.12f;

            if (Main.netMode == NetmodeID.Server) return;

            // Re-fire the source loop every 181 ticks so it covers the full wind-up duration.
            // On the very first tick (activeDur - timer == 0) the loop was already started
            // at the call site; subsequent re-fires keep it seamless.
            int elapsed = activeDur - _inWallTeleportTimer;
            bool isLoopRetrigger = elapsed > 0 && (elapsed % 181 == 0);
            if (isLoopRetrigger)
            {
                if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound prev)) prev.Stop();
                _tpLoopSlot = SoundEngine.PlaySound(
                    new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                    Projectile.Center);
            }

            // Keep source loop tethered to Saria's position.
            if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound tpActive))
                tpActive.Position = Projectile.Center;

            // Destination loop: play at the escape target so the player hears it near
            // their camera even when Saria is far away.
            // "Far" = escape target is more than one screen width from Saria.
            bool hasDest = _inWallEscapeTarget != Vector2.Zero;
            bool destFar = hasDest &&
                           Vector2.Distance(Projectile.Center, _inWallEscapeTarget) > Main.screenWidth;
            if (hasDest)
            {
                bool destLoopNeeded = !SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound destActive);
                if (destLoopNeeded || isLoopRetrigger)
                {
                    if (SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound prev2)) prev2.Stop();
                    // If far, play without position so attenuation doesn't kill it;
                    // if close, anchor it to the destination in world space.
                    _tpDestLoopSlot = destFar
                        ? SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.5f })
                        : SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.5f }, _inWallEscapeTarget);
                }
                else if (!destFar && SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound destPos))
                {
                    destPos.Position = _inWallEscapeTarget;
                }
            }
            else
            {
                // No destination — stop dest loop if it somehow lingered.
                if (SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound stale)) stale.Stop();
            }

            // Tick and spawn pillars for source.
            TickPillarList(_tpSourcePillars);
            // Tick and spawn pillars for destination.
            TickPillarList(_tpDestPillars);
        }

        /// <summary>
        /// Updates transform phase values for this frame (pop detection, scale/alpha/pulse).
        /// Must be called once per PostDraw before any transform draw calls.
        /// </summary>
        private void TickTransformPhase()
        {
            bool justPopped = (_prevTransformTimer > 0 && TransformTimer == 0);
            _prevTransformTimer = TransformTimer;

            if (justPopped)
                _transformPopCountdown = TransformPopTicks;

            bool isActive = IsTransforming || _transformPopCountdown > 0;
            if (!isActive)
            {
                _transformSphereScale = 0f;
                _transformPulsePhase  = 0f;
                _transformGlobs.Clear();
                _transformPillars.Clear();
                return;
            }

            if (IsTransforming)
            {
                float growProgress = Math.Clamp((TransformDuration - TransformTimer) / (float)TransformGrowTicks, 0f, 1f);
                _transformSphereScale = growProgress;
                _transformPulsePhase += 0.08f;
                _transformWavePhase  += 0.12f;  // inner ripple advances fastest

                TickPillarList(_transformPillars);
            }
            else if (_transformPopCountdown > 0)
            {
                _transformSphereScale = 0f;
                _transformPopCountdown--;

                // Burst dust on first pop frame only
                if (_transformPopCountdown == TransformPopTicks - 1 && Main.netMode != NetmodeID.Server)
                {
                    // Fast outward burst — 75% white, 25% pink (AbsorbPsychic)
                    for (int i = 0; i < 70; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(4f, 12f);
                        int dustType = Main.rand.NextBool(4) ? ModContent.DustType<AbsorbPsychic>() : DustID.Cloud;
                        Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, Scale: 1.8f);
                        d.noGravity = true;
                    }
                    // Slow inner burst — 75% white, 25% pink
                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 3f);
                        int dustType = Main.rand.NextBool(4) ? ModContent.DustType<AbsorbPsychic>() : DustID.Cloud;
                        Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, Scale: 1.2f);
                        d.noGravity = true;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the transformation glow sphere. Called absolutely last in PostDraw so it
        /// renders on top of every body layer, arm, face, and UI element.
        /// </summary>
        private void DrawTransformGlowSphere()
        {
            if (_transformSphereScale <= 0f)
            {
                _transformGlobs.Clear();
                return;
            }

            Vector2 screenPos = Projectile.Center - Main.screenPosition - new Vector2(0f, 8f);
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            const float baseRadius = 90f;
            float s = _transformSphereScale;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            // waveFreq    = number of ripple peaks around the edge
            // waveAmp     = max pixel distortion of the edge radius
            // phaseOffset = added to the wave argument — use radius-proportional values so crests appear to travel outward
            // edgeFalloff = exponent for the alpha fade: 2 = soft, higher = sharper edge
            // aspectX     = horizontal stretch multiplier (>1 = wider)
            // aspectTop   = vertical stretch upward  (<1 = less tall)
            // aspectBot   = vertical stretch downward (<1 = less tall)
            void DrawSoftCircle(Vector2 centre, float radius, float alphaScale,
                                float waveFreq = 0f, float waveAmp = 0f, float phaseOffset = 0f,
                                float edgeFalloff = 2f,
                                float aspectX = 1f, float aspectTop = 1.4f, float aspectBot = 1.6f)
            {
                const int step = 3;
                float radiusTop    = radius * aspectTop;
                float radiusBottom = radius * aspectBot;

                for (float y = -radiusTop; y <= radiusBottom; y += step)
                {
                    float normY  = y < 0f ? y / radiusTop : y / radiusBottom;
                    float sinArg = (float)Math.Atan2(y, radius);
                    float ripple = waveAmp > 0f
                        ? waveAmp * (float)Math.Sin(waveFreq * sinArg + _transformWavePhase + phaseOffset)
                        : 0f;
                    float radiusX = (radius + ripple) * aspectX;
                    float halfW   = radiusX * (float)Math.Sqrt(Math.Max(0f, 1f - normY * normY));
                    if (halfW < 1f) continue;
                    float t        = Math.Abs(normY);
                    float rowAlpha = (1f - (float)Math.Pow(t, edgeFalloff)) * alphaScale;
                    if (rowAlpha < 0.005f) continue;
                    Main.spriteBatch.Draw(pixel,
                        new Rectangle((int)(centre.X - halfW), (int)(centre.Y + y), (int)(halfW * 2f), step),
                        null, Color.White * rowAlpha);
                }
            }

            // All rings share the same waveFreq=6 so the shape is consistent.
            // phaseOffset increases with radius so crests appear to travel outward from the center.
            // Amplitude and alpha decrease with each ring — wave energy dissipates as it spreads.
            // The center ring always leads (offset 0); each outer ring is offset by +1.2 radians.
            const float freq       = 6f;
            const float offsetStep = 1.2f;  // radians between rings — tune to taste

            // Center (anchor) — full amplitude, stays fixed as the wave source
            DrawSoftCircle(screenPos, baseRadius * s * 0.40f, s * 1.2f,  waveFreq: freq, waveAmp: 7f,  phaseOffset: 0f,              edgeFalloff: 2f);
            // Mid ring — wider, less tall
            DrawSoftCircle(screenPos, baseRadius * s * 0.70f, s * 0.70f, waveFreq: freq, waveAmp: 5.5f, phaseOffset: offsetStep,      edgeFalloff: 2.5f, aspectX: 1.25f, aspectTop: 1.1f, aspectBot: 1.2f);
            // Outer ring — wider, less tall
            DrawSoftCircle(screenPos, baseRadius * s * 1.08f, s * 0.40f, waveFreq: freq, waveAmp: 4f,  phaseOffset: offsetStep * 2f,  edgeFalloff: 3.5f, aspectX: 1.35f, aspectTop: 1.0f, aspectBot: 1.1f);
            // Halo — wider, less tall, breathing edge
            float haloFalloff = 3.5f + (float)(Math.Sin(_transformPulsePhase) * 0.5 + 0.5) * 2.5f;
            DrawSoftCircle(screenPos, baseRadius * s * 1.32f, s * 0.32f, waveFreq: freq, waveAmp: 2.5f, phaseOffset: offsetStep * 3f, edgeFalloff: haloFalloff, aspectX: 1.45f, aspectTop: 0.9f, aspectBot: 1.0f);
            // Spike fringe — almost no wave energy left; high falloff so only faint spikes survive
            float spikeFalloff = 7.0f + (float)(Math.Sin(_transformPulsePhase * 0.6f) * 0.5 + 0.5) * 3.0f;
            DrawSoftCircle(screenPos, baseRadius * s * 1.52f, s * 0.18f, waveFreq: freq, waveAmp: 1.5f, phaseOffset: offsetStep * 4f, edgeFalloff: spikeFalloff);

            // --- Absorption globs: spawn at edge, drift inward, grow then vanish ---
            void DrawGlobSpot(Vector2 pos, float radius, float alpha)
            {
                if (radius < 0.5f || alpha < 0.005f) return;
                int r = Math.Max(1, (int)radius);
                for (int dy = -r; dy <= r; dy++)
                {
                    float normY    = r > 0 ? dy / (float)r : 0f;
                    float halfW    = (float)Math.Sqrt(Math.Max(0f, 1f - normY * normY)) * r;
                    if (halfW < 0.5f) continue;
                    float rowAlpha = (1f - normY * normY) * alpha;
                    if (rowAlpha < 0.005f) continue;
                    Main.spriteBatch.Draw(pixel,
                        new Rectangle((int)(pos.X - halfW), (int)(pos.Y + dy), Math.Max(1, (int)(halfW * 2f)), 1),
                        null, Color.White * rowAlpha);
                }
            }

            // Spawn new globs at the outer edge each frame
            if (Main.netMode != NetmodeID.Server && _transformGlobs.Count < 60)
            {
                int spawnCount = Main.rand.Next(1, 3);
                for (int i = 0; i < spawnCount; i++)
                {
                    float angle     = Main.rand.NextFloat(MathHelper.TwoPi);
                    float spawnDist = baseRadius * s * Main.rand.NextFloat(1.35f, 1.75f);
                    _transformGlobs.Add(new TransformGlob
                    {
                        Angle         = angle,
                        Distance      = spawnDist,
                        SpawnDistance = spawnDist,
                        Speed         = Main.rand.NextFloat(0.4f, 1.2f),
                        MaxSize       = Main.rand.NextFloat(3f, 9f) * s,
                    });
                }
            }

            // Move globs inward; bell-curve size grows then shrinks to zero (absorbed)
            for (int i = _transformGlobs.Count - 1; i >= 0; i--)
            {
                TransformGlob g = _transformGlobs[i];
                g.Distance     -= g.Speed;
                _transformGlobs[i] = g;

                if (g.Distance <= 3f)
                {
                    _transformGlobs.RemoveAt(i);
                    continue;
                }

                float progress  = Math.Clamp(1f - g.Distance / g.SpawnDistance, 0f, 1f);
                float sizeScale = 4f * progress * (1f - progress); // bell: 0 → peak at 50% → 0
                float drawSize  = g.MaxSize * sizeScale;
                float alpha     = sizeScale * 0.85f * s;

                // Squish Y slightly to follow the oval contour
                Vector2 globPos = screenPos + new Vector2(
                    (float)Math.Cos(g.Angle) * g.Distance,
                    (float)Math.Sin(g.Angle) * g.Distance * 0.75f);

                DrawGlobSpot(globPos, drawSize, alpha);
            }
            // --- End absorption globs ---

            // --- Light pillars ---
            foreach (TransformPillar p in _transformPillars)
            {
                float lifeT    = p.Life / p.MaxLife;                          // 1→0 over lifetime
                // Length: grows to full in first 25% of life, then holds at max
                float growT  = Math.Clamp((1f - lifeT) / 0.25f, 0f, 1f);         // 0→1 in first 25%
                float length = p.MaxLength * growT;
                // Alpha envelope: fade in during grow phase, hold, fade out in last 30%
                float envelope = lifeT < 0.3f ? lifeT / 0.3f : 1f;
                if (length < 2f) continue;

                // Wider and longer beams are more translucent — big beams are ghostly, thin beams are bright.
                // Width range ~7-18px → widthFactor ~0.35-0.75 (inverted: wider = lower alpha)
                float widthFactor  = Math.Clamp(1f - (p.Width - 7f) / 20f,  0.30f, 0.85f);
                // Length range ~150-400px → lengthFactor ~0.55-0.90 (inverted: longer = lower alpha)
                float lengthFactor = Math.Clamp(1f - (p.MaxLength - 150f) / 500f, 0.45f, 0.90f);
                float alpha = envelope * s * 0.75f * widthFactor * lengthFactor;
                if (alpha < 0.005f) continue;

                // Draw pillar as scanline slices along its axis.
                // Width flares linearly from 0 at the root to p.Width*3 at the tip.
                float cos = (float)Math.Cos(p.Angle);
                float sin = (float)Math.Sin(p.Angle);
                const int sliceStep = 2;
                for (float d = 0f; d < length; d += sliceStep)
                {
                    float t        = d / length;                            // 0 at root, 1 at tip
                    float halfW    = (p.Width * 0.5f + p.Width * 2.5f * t); // flares outward
                    // Fade only kicks in past 50% — thin beams stay full-bright near the root
                    float fadeFactor = t < 0.5f ? 1f : (float)Math.Pow(1f - ((t - 0.5f) / 0.5f), 2.5f);
                    float rowAlpha = alpha * fadeFactor;
                    if (rowAlpha < 0.005f) continue;

                    // Centre of this slice in screen space
                    float cx = screenPos.X + cos * d;
                    float cy = screenPos.Y + sin * d * 0.75f;               // squish Y to match oval

                    // Perpendicular to the pillar axis
                    float px = -sin;
                    float py =  cos * 0.75f;

                    int rx = (int)(cx - px * halfW);
                    int ry = (int)(cy - py * halfW);
                    int rw = Math.Max(1, (int)(px * halfW * 2f));
                    int rh = Math.Max(1, (int)(py * halfW * 2f));

                    // Normalise into a proper rectangle
                    if (rw < 0) { rx += rw; rw = -rw; }
                    if (rh < 0) { ry += rh; rh = -rh; }
                    if (rw == 0) rw = 1;
                    if (rh == 0) rh = 1;

                    Main.spriteBatch.Draw(pixel, new Rectangle(rx, ry, rw, rh), null, Color.White * rowAlpha);
                }
            }
            // --- End light pillars ---

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        /// <summary>
        /// Draws the teleport destination sphere in world space.
        /// Called from SariaModSystem.PostDrawTiles so it renders even when
        /// Saria herself is off-screen.
        /// </summary>
        public void DrawTeleportDestination(SpriteBatch spriteBatch)
        {
            if (_tpSphereScale <= 0f || _inWallEscapeTarget == Vector2.Zero) return;
            // PostDrawTiles runs with no active SpriteBatch — start one.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.Transform);
            DrawTeleportGlowSphere(_inWallEscapeTarget, _tpSphereScale, _tpDestGlobs, _tpDestPillars);
            // DrawTeleportGlowSphere ends in AlphaBlend — close that batch too.
            spriteBatch.End();
        }

        /// <summary>
        /// Draws the teleport source sphere. Called from PostDraw (Saria visible on screen).
        /// </summary>
        private void DrawTeleportSourceSphere()
        {
            if (_tpSphereScale <= 0f) return;
            DrawTeleportGlowSphere(Projectile.Center, _tpSphereScale, _tpSourceGlobs, _tpSourcePillars);
        }
        /// Pass the source or destination position and the corresponding glob/pillar lists.
        /// Must be called while the SpriteBatch is in its normal AlphaBlend state;
        /// this method switches to Additive internally and restores AlphaBlend before returning.
        /// </summary>
        private void DrawTeleportGlowSphere(Vector2 worldCenter,
                                            float s,
                                            List<TransformGlob>   globs,
                                            List<TransformPillar> pillars)
        {
            if (s <= 0f)
            {
                globs.Clear();
                return;
            }

            Color pink = new Color(255, 80, 200);
            Vector2 screenPos = worldCenter - Main.screenPosition - new Vector2(0f, 8f);
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            const float baseRadius = 90f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            void DrawSoftCircle(Vector2 centre, float radius, float alphaScale,
                                float waveFreq = 0f, float waveAmp = 0f, float phaseOffset = 0f,
                                float edgeFalloff = 2f,
                                float aspectX = 1f, float aspectTop = 1.4f, float aspectBot = 1.6f)
            {
                const int step = 3;
                float radiusTop    = radius * aspectTop;
                float radiusBottom = radius * aspectBot;
                for (float y = -radiusTop; y <= radiusBottom; y += step)
                {
                    float normY  = y < 0f ? y / radiusTop : y / radiusBottom;
                    float sinArg = (float)Math.Atan2(y, radius);
                    float ripple = waveAmp > 0f
                        ? waveAmp * (float)Math.Sin(waveFreq * sinArg + _tpWavePhase + phaseOffset)
                        : 0f;
                    float radiusX = (radius + ripple) * aspectX;
                    float halfW   = radiusX * (float)Math.Sqrt(Math.Max(0f, 1f - normY * normY));
                    if (halfW < 1f) continue;
                    float t        = Math.Abs(normY);
                    float rowAlpha = (1f - (float)Math.Pow(t, edgeFalloff)) * alphaScale;
                    if (rowAlpha < 0.005f) continue;
                    Main.spriteBatch.Draw(pixel,
                        new Rectangle((int)(centre.X - halfW), (int)(centre.Y + y), (int)(halfW * 2f), step),
                        null, pink * rowAlpha);
                }
            }

            const float freq       = 6f;
            const float offsetStep = 1.2f;

            DrawSoftCircle(screenPos, baseRadius * s * 0.40f, s * 1.2f,  waveFreq: freq, waveAmp: 7f,  phaseOffset: 0f,              edgeFalloff: 2f);
            DrawSoftCircle(screenPos, baseRadius * s * 0.70f, s * 0.70f, waveFreq: freq, waveAmp: 5.5f, phaseOffset: offsetStep,      edgeFalloff: 2.5f, aspectX: 1.25f, aspectTop: 1.1f, aspectBot: 1.2f);
            DrawSoftCircle(screenPos, baseRadius * s * 1.08f, s * 0.40f, waveFreq: freq, waveAmp: 4f,  phaseOffset: offsetStep * 2f,  edgeFalloff: 3.5f, aspectX: 1.35f, aspectTop: 1.0f, aspectBot: 1.1f);
            float haloFalloff  = 3.5f + (float)(Math.Sin(_tpPulsePhase) * 0.5 + 0.5) * 2.5f;
            DrawSoftCircle(screenPos, baseRadius * s * 1.32f, s * 0.32f, waveFreq: freq, waveAmp: 2.5f, phaseOffset: offsetStep * 3f, edgeFalloff: haloFalloff, aspectX: 1.45f, aspectTop: 0.9f, aspectBot: 1.0f);
            float spikeFalloff = 7.0f + (float)(Math.Sin(_tpPulsePhase * 0.6f) * 0.5 + 0.5) * 3.0f;
            DrawSoftCircle(screenPos, baseRadius * s * 1.52f, s * 0.18f, waveFreq: freq, waveAmp: 1.5f, phaseOffset: offsetStep * 4f, edgeFalloff: spikeFalloff);

            // Globs
            void DrawGlobSpot(Vector2 pos, float radius, float alpha)
            {
                if (radius < 0.5f || alpha < 0.005f) return;
                int r = Math.Max(1, (int)radius);
                for (int dy = -r; dy <= r; dy++)
                {
                    float normY = r > 0 ? dy / (float)r : 0f;
                    float halfW = (float)Math.Sqrt(Math.Max(0f, 1f - normY * normY)) * r;
                    if (halfW < 0.5f) continue;
                    float rowAlpha = (1f - normY * normY) * alpha;
                    if (rowAlpha < 0.005f) continue;
                    Main.spriteBatch.Draw(pixel,
                        new Rectangle((int)(pos.X - halfW), (int)(pos.Y + dy), Math.Max(1, (int)(halfW * 2f)), 1),
                        null, pink * rowAlpha);
                }
            }

            if (Main.netMode != NetmodeID.Server && globs.Count < 60)
            {
                int spawnCount = Main.rand.Next(1, 3);
                for (int i = 0; i < spawnCount; i++)
                {
                    float angle     = Main.rand.NextFloat(MathHelper.TwoPi);
                    float spawnDist = baseRadius * s * Main.rand.NextFloat(1.35f, 1.75f);
                    globs.Add(new TransformGlob
                    {
                        Angle         = angle,
                        Distance      = spawnDist,
                        SpawnDistance = spawnDist,
                        Speed         = Main.rand.NextFloat(0.4f, 1.2f),
                        MaxSize       = Main.rand.NextFloat(3f, 9f) * s,
                    });
                }
            }
            for (int i = globs.Count - 1; i >= 0; i--)
            {
                TransformGlob g = globs[i];
                g.Distance    -= g.Speed;
                globs[i]       = g;
                if (g.Distance <= 3f) { globs.RemoveAt(i); continue; }
                float progress  = Math.Clamp(1f - g.Distance / g.SpawnDistance, 0f, 1f);
                float sizeScale = 4f * progress * (1f - progress);
                float drawSize  = g.MaxSize * sizeScale;
                float alpha     = sizeScale * 0.85f * s;
                Vector2 globPos = screenPos + new Vector2(
                    (float)Math.Cos(g.Angle) * g.Distance,
                    (float)Math.Sin(g.Angle) * g.Distance * 0.75f);
                DrawGlobSpot(globPos, drawSize, alpha);
            }

            // Pillars
            foreach (TransformPillar p in pillars)
            {
                float lifeT    = p.Life / p.MaxLife;
                float growT    = Math.Clamp((1f - lifeT) / 0.25f, 0f, 1f);
                float length   = p.MaxLength * growT;
                float envelope = lifeT < 0.3f ? lifeT / 0.3f : 1f;
                if (length < 2f) continue;
                float widthFactor  = Math.Clamp(1f - (p.Width    - 7f)   / 20f,  0.30f, 0.85f);
                float lengthFactor = Math.Clamp(1f - (p.MaxLength - 150f) / 500f, 0.45f, 0.90f);
                float alpha = envelope * s * 0.75f * widthFactor * lengthFactor;
                if (alpha < 0.005f) continue;
                float cos = (float)Math.Cos(p.Angle);
                float sin = (float)Math.Sin(p.Angle);
                const int sliceStep = 2;
                for (float d = 0f; d < length; d += sliceStep)
                {
                    float t        = d / length;
                    float halfW    = p.Width * 0.5f + p.Width * 2.5f * t;
                    float fadeFactor = t < 0.5f ? 1f : (float)Math.Pow(1f - ((t - 0.5f) / 0.5f), 2.5f);
                    float rowAlpha = alpha * fadeFactor;
                    if (rowAlpha < 0.005f) continue;
                    float cx = screenPos.X + cos * d;
                    float cy = screenPos.Y + sin * d * 0.75f;
                    float px = -sin;
                    float py =  cos * 0.75f;
                    int rx = (int)(cx - px * halfW);
                    int ry = (int)(cy - py * halfW);
                    int rw = Math.Max(1, (int)(px * halfW * 2f));
                    int rh = Math.Max(1, (int)(py * halfW * 2f));
                    if (rw < 0) { rx += rw; rw = -rw; }
                    if (rh < 0) { ry += rh; rh = -rh; }
                    if (rw == 0) rw = 1;
                    if (rh == 0) rh = 1;
                    Main.spriteBatch.Draw(pixel, new Rectangle(rx, ry, rw, rh), null, pink * rowAlpha);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        public override void PostDraw(Color lightColor)
        {
            // Owner-only LinkCable destination marker. In the LinkCable dot-placement
            // mode only the owning player sees the marked location; the A* trail stays
            // hidden here (it is shown solely through the debug overlay). Drawn in
            // PostDraw so it is visible in normal gameplay without debug visuals on.
            if (_linkCableFollow && Main.myPlayer == Projectile.owner && _followMarkedPosition != Vector2.Zero)
            {
                Texture2D pixelMark = TextureAssets.MagicPixel.Value;
                Vector2 markScreen  = _followMarkedPosition - Main.screenPosition;

                // Dark red filled square (12×12) at the target position.
                Main.spriteBatch.Draw(pixelMark,
                    new Rectangle((int)markScreen.X - 6, (int)markScreen.Y - 6, 12, 12),
                    null, new Color(160, 20, 20, 220));

                // SariaIcon.png centered above the dot.
                Texture2D markIcon = ModContent.Request<Texture2D>("SariaMod/SariaIcon").Value;
                Vector2 markIconOrigin = new Vector2(markIcon.Width * 0.5f, markIcon.Height);
                Main.spriteBatch.Draw(markIcon,
                    new Vector2(markScreen.X, markScreen.Y - 8),
                    null, Color.White * 0.9f,
                    0f, markIconOrigin, 0.75f, SpriteEffects.None, 0f);
            }

            {
                Player player = Main.player[Projectile.owner];
                FairyPlayer modPlayer = player.Fairy();
                float sneezespot = 5;
                {
                    Vector2 drawPosition;
                    Vector2 mouse = Main.MouseWorld;
                    mouse.X += 10f;
                    mouse.Y -= 5f;
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Vector2 startPos2 = Projectile.Center;
                    float radius2 = ((Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle));
                    startPos2.Y = radius2;
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    startPos2.X += sneezespot;
                    float between = Vector2.Distance(mouse, startPos2);
                    bool Rightclick = (player.HeldItem.type == ModContent.ItemType<HealBall>() && Main.mouseLeft && (Main.myPlayer == Projectile.owner));
                    if (between > 30)
                    {
                        SelectSound = false;
                    }

                    // Check for pending cutscenes (Persistent Icon Logic)
                    var tracker = player.GetModPlayer<SariaInteractionTrackerPlayer>();
                    var bestPending = tracker.GetBestAvailableCutscene();
                    bool hasAny = tracker.HasAnyPendingCutscenes();

                    if (hasAny && !SariaUISystem.IsDialogueActive)
                    {
                        if (bestPending != null)
                        {
                            // Condition met -> Orange (Normal SariaTimed)
                            Projectile.SariaBubbleFaces(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/SariaTimed").Value), false, 60, 3, -50, lightColor);
                        }
                        else
                        {
                            // Condition NOT met -> Grey (SariaTimedGrey)
                            // Using SariaTimedGrey texture instead of tinting
                            Projectile.SariaBubbleFaces(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/SariaTimedGrey").Value), false, 60, 3, -50, lightColor);
                        }
                    }

                    if (ChangeForm > 0 && (Main.myPlayer == Projectile.owner))
                    {
                        if (between <= 30 && (Main.myPlayer == Projectile.owner))
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (SariaModUtilities.ColorMessage("Saria", new Color(135, 206, 180)));
                            {
                                // Check for pending cutscenes
                                // Reusing variables from above: 'tracker', 'bestPending', 'hasAny'
                                // var tracker2 = player.GetModPlayer<SariaInteractionTrackerPlayer>();
                                // var pending = tracker2.GetActivePendingCutscene(); 

                                bool showTimed = hasAny; // Show timed icon if any exist

                                if (showTimed)
                                {
                                    if (bestPending != null)
                                    {
                                        Projectile.SariaBubbleFaces(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/SariaTimed").Value), false, 60, 3, -50, lightColor);
                                    }
                                    else
                                    {
                                        Projectile.SariaBubbleFaces(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/SariaTimedGrey").Value), false, 60, 3, -50, lightColor);
                                    }
                                }
                                else
                                            // Only draw SariaTalk if NOT showing SariaTimed
                                            if (!hasAny)
                                {
                                    Projectile.SariaBubbleFaces(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/SariaTalk").Value), false, 60, 3, -50, lightColor);
                                }

                                if (!SelectSound)
                                {
                                    SoundEngine.PlaySound(SoundID.MenuTick);
                                    SelectSound = true;
                                }
                            }
                        }
                        if (between <= 30 && Rightclick && Eating <= 0)
                        {
                            if (!SariaUISystem.IsDialogueActive)
                            {
                                SariaTalking = true;
                                ChangeForm = 0; // Close the form change overlay when opening dialogue
                                Projectile.netUpdate = true;
                                SoundEngine.PlaySound(SoundID.MenuOpen);

                                // Check pending cutscene for interaction
                                if (bestPending != null)
                                {
                                    // Condition met: Go to Pending node
                                    SariaUISystem.DisplayDialogue("Pending", Projectile);
                                }
                                else if (InteractionManager.CanTriggerInteractive(modPlayer))
                                {
                                    string interactiveID = InteractionManager.GetRandomInteractiveDialogue();
                                    InteractionManager.RegisterInteractiveDialogue(interactiveID);
                                    InteractionManager.IsInteractiveSession = true;
                                    SariaUISystem.DisplayDialogue(interactiveID, Projectile);
                                }
                                else
                                {
                                    // Default behavior — use debug override if available
                                    string startNode = SariaDebugUISystem.DebugEnabled
                                        ? SariaDebugUISystem.DebugStartNodeOverride
                                        : "start";
                                    SariaUISystem.DisplayDialogue(startNode, Projectile);
                                }
                            }
                        }
                    }
                    // Tick transform phase once before all transform draws.
                    TickTransformPhase();

                    Projectile.SariaBubbleFaceLoader((int)ChangeForm, (int)Eating, lightColor);
                    Projectile.SariaFeetandArmDraw((int)Transform, (int)Eating, lightColor);

                    // Idle feet pass (bottommost — drawn before body so glow stays underneath)
                    if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                        IdleAnimator.DrawFeetPass(Projectile, lightColor);

                    // Body pass (behind faces) — body, eat, hair, scar, body masks only
                    Projectile.SariaBodyDraw((int)Transform, (int)Eating, (int)IsCharging, (int)ChannelState, (int)SpecialAnimate, lightColor, armsOnly: false);

                    // Idle legs pass (behind faces)
                    if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                        IdleAnimator.DrawLegsPass(Projectile, Transform, lightColor);

                    // Emit campfire-like light for the flaming hair when in Transform 2 and hair is visible.
                    if (Transform == 2)
                    {
                        Vector2 lightPos = Projectile.Center + new Vector2(0f, -20f);
                        Lighting.AddLight(lightPos, Color.Orange.ToVector3() * 1.2f);
                    }
                    Projectile.SariaHornDraw((int)Transform, lightColor);

                    // Owner drives the displayed mood through the blink gate.
                    // Non-owner clients get DisplayedMood pushed directly from the synced packet value.
                    if (Projectile.owner == Main.myPlayer)
                        IdleAnimator.UpdateDisplayedMood(Mood);

                    // Idle underlays — drawn BEFORE the face base layer
                    if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                    {
                        IdleAnimator.DrawAngryIdleUnderlay(Projectile, Transform, Mood, Cursed, lightColor);
                        IdleAnimator.DrawMouthIdleUnderlay(Projectile, Transform, Mood, Cursed, lightColor);
                    }

                    // Faces and chest pieces
                    Projectile.SariaSmallFacesOrWhencursed((int)Transform, (bool)Sleep, (int)Eating, (int)IsCharging, (bool)Cursed, (int)ChannelState, (int)Mood, lightColor, IdleAnimator);

                    // Idle eye overlays — drawn AFTER the face base layer
                    if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                    {
                        IdleAnimator.DrawIdleEyes(Projectile, Transform, Mood, Cursed, lightColor);
                        IdleAnimator.DrawHappyIdleEyes(Projectile, Transform, Mood, Cursed, lightColor);
                        IdleAnimator.DrawSadIdleEyes(Projectile, Transform, Mood, Cursed, lightColor);
                        IdleAnimator.DrawAngryIdleEyes(Projectile, Transform, Mood, Cursed, lightColor);
                    }

                    // Arms pass (over faces and chest) — direction arms + their masks
                    Projectile.SariaBodyDraw((int)Transform, (int)Eating, (int)IsCharging, (int)ChannelState, (int)SpecialAnimate, lightColor, armsOnly: true);

                    // Idle arms pass (over faces and chest)
                    if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                        IdleAnimator.DrawArmsPass(Projectile, Transform, lightColor);

                    // Cursed shadowy overlay — drawn over all body parts, arms, and legs
                    // so it covers every form's glows and all animation states.
                    if (Mood == (int)MoodState.Cursed)
                    {
                        Color cursedTint = Color.Lerp(lightColor, new Color(60, 0, 80, 200), 0.55f);
                        Projectile.SariaBodyDraw((int)Transform, (int)Eating, (int)IsCharging, (int)ChannelState, (int)SpecialAnimate, cursedTint, armsOnly: false);
                        Projectile.SariaBodyDraw((int)Transform, (int)Eating, (int)IsCharging, (int)ChannelState, (int)SpecialAnimate, cursedTint, armsOnly: true);
                        if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                        {
                            IdleAnimator.DrawFeetPass(Projectile, cursedTint);
                            IdleAnimator.DrawLegsPass(Projectile, Transform, cursedTint);
                            IdleAnimator.DrawArmsPass(Projectile, Transform, cursedTint);
                        }
                    }

                    // Attack arm top pass — drawn over direction arms and idle arms
                    Projectile.SariaAttackArmTopDraw(Transform, (int)IsCharging, (int)ChannelState, (int)Eating, Sleep, Cursed, lightColor);

                    Projectile.SariaChargingAnimation((int)Transform, (bool)Sleep, (int)Eating, (int)IsCharging, (bool)Cursed, (int)ChannelState, (int)Mood, lightColor);
                    Projectile.SariaEatDraw((int)Transform, (int)Eating, lightColor, IdleAnimator);
                    Projectile.SariaSleepDraw((int)Transform, (bool)Sleep, lightColor, IdleAnimator);

                    // Sparks overlay — drawn last so it appears over all body parts,
                    // masks, arms, faces, and other overlays.
                    if (Transform == 3 && SpecialAnimate > 0)
                    {
                        Projectile.SariaSparksDraw(TextureAssets.Projectile[ModContent.ProjectileType<SariaSparks>()].Value, lightColor);
                    }

                    if (XpTimer && Main.myPlayer == Projectile.owner)
                    {
                        Projectile.SariaDrawInterface(lightColor, SariaExtensions1.InterfaceType.XPBar);
                        Projectile.SariaDrawInterface(lightColor, SariaExtensions1.InterfaceType.NextBoss);
                    }

                    // Transformation glow sphere — drawn absolutely last so it overlays
                    // every body layer, arm, face, and UI element.
                    DrawTransformGlowSphere();

                    // Source teleport sphere — drawn on Saria when wind-up is active.
                    // The destination sphere is drawn by SariaModSystem.PostDrawTiles
                    // so it renders even when Saria is far off-screen.
                    DrawTeleportSourceSphere();

                    // Debug: per-Saria bandwidth label, visible on non-owner clients
                    // when the network profiler panel is open/enabled.
                    if (Diagnostics.NetworkProfilerUISystem.DebugEnabled && Main.myPlayer != Projectile.owner)
                    {
                        var (netBytes, netPkts) = Diagnostics.NetworkProfiler.GetSariaAggregate();
                        string label = $"Net {Diagnostics.NetworkProfiler.FormatBytes(netBytes)}/s  {netPkts}p/s";
                        Vector2 labelPos = Projectile.Top - Main.screenPosition - new Vector2(0, 14);
                        Vector2 labelSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(label) * 0.65f;
                        labelPos.X -= labelSize.X * 0.5f;
                        Color labelColor = netBytes > 1024 ? new Color(255, 160, 80) : new Color(120, 220, 120);
                        Utils.DrawBorderString(Main.spriteBatch, label, labelPos, labelColor, 0.65f);
                    }

                    }

                // Debug overlay is now drawn by SariaDebugUISystem.Hook_DrawProjectileHitboxes
                // via DrawDebugOverlay() so it stays visible when Saria is off-screen.
            }
        }

        /// <summary>
        /// Compares two A* paths for equality (same length and matching tile-center
        /// waypoints). Treats null and empty as equal so cleared paths don't spam sync.
        /// </summary>
        private static bool FollowPathsEqual(System.Collections.Generic.List<Vector2> a, System.Collections.Generic.List<Vector2> b)
        {
            int countA = a?.Count ?? 0;
            int countB = b?.Count ?? 0;
            if (countA != countB)
                return false;
            for (int i = 0; i < countA; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }

        /// <summary>
        /// Draws Saria's debug overlay: idle position dot, probe rects, tile-collision ring,
        /// cursed-separation ring, and Follow trail dots.
        /// Called from SariaDebugUISystem.Hook_DrawProjectileHitboxes so it is always visible
        /// even when Saria is off-screen and PostDraw is culled by Terraria.
        /// The SpriteBatch must already be active with Main.GameViewMatrix.TransformationMatrix
        /// applied, so all positions are passed as world coordinates.
        /// </summary>
        public void DrawDebugOverlay(SpriteBatch spriteBatch)
        {
            // Marked location — drawn for all clients so everyone sees the chosen target dot.
            if ((Follow || _linkCableFollow) && _followMarkedPosition != Vector2.Zero)
            {
                Texture2D pixelMark = TextureAssets.MagicPixel.Value;
                Vector2 markScreen  = _followMarkedPosition - Main.screenPosition;

                // Dark red filled square (12×12) at the target position.
                spriteBatch.Draw(pixelMark,
                    new Rectangle((int)markScreen.X - 6, (int)markScreen.Y - 6, 12, 12),
                    null, new Color(160, 20, 20, 220));

                // SariaIcon.png centered above the dot.
                Texture2D icon = ModContent.Request<Texture2D>("SariaMod/SariaIcon").Value;
                Vector2 iconOrigin = new Vector2(icon.Width * 0.5f, icon.Height);
                spriteBatch.Draw(icon,
                    new Vector2(markScreen.X, markScreen.Y - 8),
                    null, Color.White * 0.9f,
                    0f, iconOrigin, 0.75f, SpriteEffects.None, 0f);
            }

            // A* pink dotted trail — drawn for all clients (synced via Extra AI).
            // A pink dot sits on each tile node, joined by a pink dotted line.
            if ((Follow || _linkCableFollow) && _followPath.Count > 0)
            {
                Texture2D pixelPath = TextureAssets.MagicPixel.Value;
                Color pathColor = new Color(255, 105, 200, 230);

                // Dotted connector segments between consecutive nodes.
                const float dotSpacing = 6f;
                for (int i = 0; i < _followPath.Count - 1; i++)
                {
                    Vector2 a = _followPath[i]     - Main.screenPosition;
                    Vector2 b = _followPath[i + 1] - Main.screenPosition;
                    Vector2 seg = b - a;
                    float len = seg.Length();
                    if (len <= 0f)
                        continue;
                    Vector2 dir = seg / len;
                    for (float d = 0f; d < len; d += dotSpacing)
                    {
                        Vector2 p = a + dir * d;
                        spriteBatch.Draw(pixelPath,
                            new Rectangle((int)p.X - 1, (int)p.Y - 1, 2, 2),
                            null, pathColor);
                    }
                }

                // Pink node dot on each tile.
                for (int i = 0; i < _followPath.Count; i++)
                {
                    Vector2 nodeScreen = _followPath[i] - Main.screenPosition;
                    spriteBatch.Draw(pixelPath,
                        new Rectangle((int)nodeScreen.X - 3, (int)nodeScreen.Y - 3, 6, 6),
                        null, pathColor);
                }
            }

            // Everything below is owner-only debug visualisation.
            if (Main.myPlayer != Projectile.owner)
                return;

            Player player = Main.player[Projectile.owner];
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            // InWall escape target — pink dot with SariaIcon, shown when she is stuck in geometry.
            if (_inWallEscapeTarget != Vector2.Zero)
            {
                Texture2D pixelEscape = TextureAssets.MagicPixel.Value;
                Vector2 escScreen = _inWallEscapeTarget - Main.screenPosition;

                // During teleport wind-up the dot pulses; before that it is dim.
                float pulse = _inWallTeleportTimer > 0
                    ? 0.5f + 0.5f * (float)Math.Sin(Main.GameUpdateCount * 0.25f)
                    : 0.45f;
                Color dotColor  = new Color(255, 80, 200) * pulse;
                Color iconColor = new Color(255, 80, 200) * (pulse * 0.9f);

                spriteBatch.Draw(pixelEscape,
                    new Rectangle((int)escScreen.X - 5, (int)escScreen.Y - 5, 10, 10),
                    null, dotColor);

                Texture2D escIcon = ModContent.Request<Texture2D>("SariaMod/SariaIcon").Value;
                Vector2 escIconOrigin = new Vector2(escIcon.Width * 0.5f, escIcon.Height);
                spriteBatch.Draw(escIcon,
                    new Vector2(escScreen.X, escScreen.Y - 8),
                    null, iconColor,
                    0f, escIconOrigin, 0.75f, SpriteEffects.None, 0f);
            }

            // Convert world positions to screen positions once.
            Vector2 idleScreenPos  = _debugIdlePosition - Main.screenPosition;
            Vector2 playerScreenPos = player.Center     - Main.screenPosition;

            // Red dot: idle position.
            spriteBatch.Draw(pixel,
                new Rectangle((int)idleScreenPos.X - 4, (int)idleScreenPos.Y - 4, 8, 8),
                null, new Color(255, 0, 0, 200));

            // Ground-probe visualizer.
            // Hitbox probe  — pink/red outline: lit when embedded in a tile (push-up fires).
            // Ground line   — green outline:    lit when feet are touching a surface (stable).
            // Scan probe    — yellow outline:   lit when a tile is within 1.5 tiles below (settle-down eligible).
            void DrawOutline(Rectangle r, Color c)
            {
                Vector2 o = Main.screenPosition;
                spriteBatch.Draw(pixel, new Rectangle(r.X - (int)o.X, r.Y          - (int)o.Y, r.Width,  1),         c);
                spriteBatch.Draw(pixel, new Rectangle(r.X - (int)o.X, r.Bottom     - (int)o.Y, r.Width,  1),         c);
                spriteBatch.Draw(pixel, new Rectangle(r.X - (int)o.X, r.Y          - (int)o.Y, 1,        r.Height),  c);
                spriteBatch.Draw(pixel, new Rectangle(r.Right - (int)o.X, r.Y      - (int)o.Y, 1,        r.Height),  c);
            }

            {
                Vector2 dbgSpritePos = new Vector2(
                    (float)Math.Round(Projectile.position.X),
                    (float)Math.Round(Projectile.position.Y));

                for (int di = 0; di < _detectorConfigs.Length; di++)
                {
                    SariaDetectorConfig dcfg = _detectorConfigs[di];
                    SariaDetector.GetFacingDir(dcfg.RotationDegrees, out int difx, out int dify);
                    SariaDetector.GetProbeRects(in dcfg, dbgSpritePos, difx, dify,
                        out Rectangle pinkR, out Rectangle greenR, out Rectangle yellowR,
                        Projectile.width, Projectile.spriteDirection);

                    bool dp = _detectorResults[di].Pink;
                    bool dg = _detectorResults[di].Green;
                    bool dy = _detectorResults[di].Yellow;

                    Color pinkActive   = new Color(255,  60, 180, 230); Color pinkDim   = new Color(255,  60, 180,  60);
                    Color greenActive  = new Color( 60, 230,  80, 230); Color greenDim  = new Color( 60, 230,  80,  60);
                    Color yellowActive = new Color(255, 220,   0, 200); Color yellowDim = new Color(255, 220,   0,  50);

                    if (dcfg.PinkDepth > 0)
                        DrawOutline(pinkR,   dp ? pinkActive   : pinkDim);
                    if (dcfg.GreenDepth > 0)
                        DrawOutline(greenR,  dg ? greenActive  : greenDim);
                    if (dcfg.HasPullLine && dcfg.PullLength > 0)
                        DrawOutline(yellowR, dy ? yellowActive : yellowDim);
                }

                // Orange bounding box spanning both wall probe pink rects ([2] left, [3] right).
                {
                    SariaDetector.GetFacingDir(_detectorConfigs[2].RotationDegrees, out int owlx, out int owly);
                    SariaDetector.GetFacingDir(_detectorConfigs[3].RotationDegrees, out int owrx, out int owry);
                    SariaDetector.GetProbeRects(in _detectorConfigs[2], dbgSpritePos, owlx, owly,
                        out Rectangle orangeWallL, out _, out _);
                    SariaDetector.GetProbeRects(in _detectorConfigs[3], dbgSpritePos, owrx, owry,
                        out Rectangle orangeWallR, out _, out _);

                    int owLeft   = orangeWallL.Left;
                    int owRight  = orangeWallR.Right - 2;
                    int owTop    = Math.Min(orangeWallL.Top,    orangeWallR.Top);
                    int owBottom = Math.Max(orangeWallL.Bottom, orangeWallR.Bottom);
                    DrawOutline(
                        new Rectangle(owLeft, owTop, owRight - owLeft, owBottom - owTop),
                        new Color(255, 140, 0, 200));
                }

                // Body-fit box: outer left of left-wall green → outer right of right-wall green,
                // top Y of the side green rects → bottom of the feet green rect.
                // This represents the actual collision footprint.
                {
                    SariaDetector.GetFacingDir(_detectorConfigs[0].RotationDegrees, out int f0x, out int f0y);
                    SariaDetector.GetFacingDir(_detectorConfigs[2].RotationDegrees, out int f2x, out int f2y);
                    SariaDetector.GetFacingDir(_detectorConfigs[3].RotationDegrees, out int f3x, out int f3y);

                    SariaDetector.GetProbeRects(in _detectorConfigs[0], dbgSpritePos, f0x, f0y,
                        out _, out Rectangle greenFeet,  out _,
                        Projectile.width, Projectile.spriteDirection);
                    SariaDetector.GetProbeRects(in _detectorConfigs[2], dbgSpritePos, f2x, f2y,
                        out _, out Rectangle greenWallL, out _);
                    SariaDetector.GetProbeRects(in _detectorConfigs[3], dbgSpritePos, f3x, f3y,
                        out _, out Rectangle greenWallR, out _);

                    int boxLeft   = greenWallL.Right;
                    int boxRight  = greenWallR.Left - 2;
                    int boxTop    = Math.Min(greenWallL.Y, greenWallR.Y);
                    int boxBottom = greenFeet.Bottom - 2;

                    DrawOutline(
                        new Rectangle(boxLeft, boxTop, boxRight - boxLeft, boxBottom - boxTop),
                        new Color(255, 255, 255, 200));
                }
            }

            // White ring: radius = TileCollisionRadius — ground-probe active inside, disabled outside (non-cursed).
            {
                bool insideTileRadius = Mood != (int)MoodState.Cursed
                    && Vector2.Distance(Projectile.Center, player.Center) <= TileCollisionRadius;
                const int tileRingSegments = 36;
                Color tileRingColor = insideTileRadius
                    ? new Color(180, 220, 255, 180)  // light blue — probe active
                    : new Color(255, 255, 255, 100);  // white — probe disabled
                for (int seg = 0; seg < tileRingSegments; seg++)
                {
                    float a1 = seg       * MathHelper.TwoPi / tileRingSegments;
                    float a2 = (seg + 1) * MathHelper.TwoPi / tileRingSegments;
                    Vector2 rp1 = playerScreenPos + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * TileCollisionRadius;
                    Vector2 rp2 = playerScreenPos + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * TileCollisionRadius;
                    Vector2 rdiff = rp2 - rp1;
                    float   rlen  = rdiff.Length();
                    if (rlen > 0f)
                    {
                        float rangle = (float)Math.Atan2(rdiff.Y, rdiff.X);
                        spriteBatch.Draw(pixel, rp1, new Rectangle(0, 0, 1, 1), tileRingColor,
                            rangle, Vector2.Zero, new Vector2(rlen, 1.5f), SpriteEffects.None, 0f);
                    }
                }
            }

            // Blue/red ring: radius = CursedSeparationRadius — matches the AI separation threshold exactly.
            // Saria follows the trail while she is outside this ring.
            if (Mood == (int)MoodState.Cursed)
            {
                float ringRadius = CursedSeparationRadius;
                const int ringSegments = 48;
                Color ringColor = _cursedSeparated
                    ? new Color(255, 80, 80, 180)   // red — currently separated / following trail
                    : new Color(80, 160, 255, 180);  // blue — inside ring / following direct
                for (int seg = 0; seg < ringSegments; seg++)
                {
                    float a1 = seg       * MathHelper.TwoPi / ringSegments;
                    float a2 = (seg + 1) * MathHelper.TwoPi / ringSegments;
                    Vector2 rp1 = idleScreenPos + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * ringRadius;
                    Vector2 rp2 = idleScreenPos + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * ringRadius;
                    Vector2 rdiff = rp2 - rp1;
                    float   rlen  = rdiff.Length();
                    if (rlen > 0f)
                    {
                        float rangle = (float)Math.Atan2(rdiff.Y, rdiff.X);
                        spriteBatch.Draw(pixel, rp1, new Rectangle(0, 0, 1, 1), ringColor,
                            rangle, Vector2.Zero, new Vector2(rlen, 1.5f), SpriteEffects.None, 0f);
                    }
                }
            }

            // Follow trail dots — yellow markers with permanent number labels.
            if (Follow && _followTrailDots.Count > 0)
            {
                Color dotColor   = new Color(255, 220, 0, 220);
                Color labelColor = new Color(255, 255, 255, 230);
                for (int di = 0; di < _followTrailDots.Count; di++)
                {
                    Vector2 dotScreen = _followTrailDots[di].Position - Main.screenPosition;
                    spriteBatch.Draw(pixel,
                        new Rectangle((int)dotScreen.X - 3, (int)dotScreen.Y - 3, 6, 6),
                        null, dotColor);

                    string label = _followTrailDots[di].Number.ToString();
                    Vector2 labelSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(label) * 0.55f;
                    Utils.DrawBorderString(spriteBatch, label,
                        new Vector2(dotScreen.X - labelSize.X * 0.5f, dotScreen.Y - 3 - labelSize.Y),
                        labelColor, 0.55f);
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

        private void PlaySyncedSariaSound(SariaSoundId soundId)
        {
            // Per-sound rate limit. Animation triggers fire every tick the condition
            // is true — without this gate the same sound would queue dozens of times
            // per second, swamping the network with tiny packets and stacking the
            // local SoundEngine. See SoundCooldownTicks above.
            int sid = (int)soundId;
            if (sid > 0 && sid < SoundCooldownTicks.Length && SoundCooldownTicks[sid] > 0)
            {
                int now = (int)Main.GameUpdateCount;
                if (now - _lastSoundTick[sid] < SoundCooldownTicks[sid])
                    return;
                _lastSoundTick[sid] = now;
            }

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                SoundStyle style = soundId switch
                {
                    SariaSoundId.Hover => new SoundStyle("SariaMod/Sounds/Hover"),
                    SariaSoundId.Fly => new SoundStyle("SariaMod/Sounds/Fly"),
                    SariaSoundId.Step1 => new SoundStyle("SariaMod/Sounds/Step1"),
                    SariaSoundId.Step2 => new SoundStyle("SariaMod/Sounds/Step2"),
                    _ => default
                };

                if (style != default)
                    SoundEngine.PlaySound(style, Projectile.Center);

                return;
            }

            // In multiplayer, always let the local client hear the sound if this is their projectile.
            if (Main.myPlayer == Projectile.owner)
            {
                SoundStyle localStyle = soundId switch
                {
                    SariaSoundId.Hover => new SoundStyle("SariaMod/Sounds/Hover"),
                    SariaSoundId.Fly => new SoundStyle("SariaMod/Sounds/Fly"),
                    SariaSoundId.Step1 => new SoundStyle("SariaMod/Sounds/Step1"),
                    SariaSoundId.Step2 => new SoundStyle("SariaMod/Sounds/Step2"),
                    _ => default
                };

                if (localStyle != default)
                    SoundEngine.PlaySound(localStyle, Projectile.Center);
            }

            // Clients (including host-and-play) send to the server; the server then broadcasts.
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (Main.myPlayer != Projectile.owner)
                    return;

                ModPacket packet = Mod.GetPacket();
                SariaSoundSyncMessage.Write(packet, Projectile, soundId);
                packet.Send();
                return;
            }

            // Dedicated server: broadcast directly.
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = Mod.GetPacket();
                SariaSoundSyncMessage.Write(packet, Projectile, soundId);
                packet.Send();
            }
        }
    }
}
