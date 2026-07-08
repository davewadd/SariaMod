using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using SariaMod.Diagnostics;
using SariaMod.Dusts;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Composited idle animation system for Saria.
    /// 
    /// Uses 4 torso frames (0-3) on the main sheet and overlays separate
    /// small spritesheets for legs, arms, and eyes.
    /// 
    /// BEHAVIORAL RULES:
    /// 1. WARMUP: Both legs and arms must complete WarmupCycles (default 3) full
    ///    loops in their base state before ANY transition is allowed.
    /// 2. CYCLE DURATIONS: Each looping state (Casual/Proper for legs, Down/Up
    ///    for arms) lasts a randomly chosen number of cycles:
    ///      Short  = 3 cycles
    ///      Medium = 4 cycles
    ///      Long   = 5 cycles
    ///    The duration is picked at random when entering the looping state.
    ///    Arms and legs pick independently.
    /// 3. SNEEZE (timer-based accumulator): SneezeBuildup accumulates each tick at
    ///    SneezeBiomeRate. When buildup >= SneezeThreshold AND legs are in
    ///    GoingToCasual AND arms are Down, the sneeze fires. Buildup resets on fire.
    ///    This value is also readable by Saria.cs for sleep integration
    ///    (high buildup = more restless/cold = sleepier).
    /// 4. EYES: 12-frame sheet (3 states × 4 frames). Looking loops for 4-5 cycles
    ///    then blinks (Blinking plays once → Opening plays once → back to Looking).
    /// 
    /// TUNABLE PARAMETERS (public properties — tweak freely):
    ///   WarmupCycles          (3)   Loops in base state before transitions allowed
    ///   SneezeThreshold       (600) Buildup value before sneeze queues
    ///   SneezeBiomeRate       (1.0) Per-tick accumulation — set by biome logic:
    ///       Snow/Rain = 1.5–2.0  (cold → sneezes sooner)
    ///       Jungle/Desert(day) = 0.3–0.5  (warm → sneezes less)
    ///       Normal/Underground = 1.0
    ///       Hallow = 0.2  (magical comfort)
    ///       Near campfire = 0  (pauses accumulation)
    /// 
    /// SPRITESHEET LAYOUT (16 frames each, 4 states × 4 frames, stacked vertically):
    ///   Legs:  0-3 Casual, 4-7 GoingToProper, 8-11 Proper, 12-15 GoingToCasual
    ///   Arms:  0-3 Down,   4-7 GoingUp,       8-11 Up,     12-15 GoingDown
    ///   All frames are same pixel dimensions as one main-sheet frame (48 × 78).
    /// 
    /// Expected texture paths (per-form, using string interpolation):
    ///   SariaMod/Items/Strange/{N}SariaAnimations/{N}SariaLegs
    ///   SariaMod/Items/Strange/{N}SariaAnimations/{N}SariaArmRight
    ///   SariaMod/Items/Strange/{N}SariaAnimations/{N}SariaArmLeft
    ///   where N = Transform + 1 (1 for form 1, 2 for form 2, etc.)
    /// 
    /// USAGE (when ready to hook into Saria.cs):
    ///   Field:    public SariaIdleAnimator IdleAnimator = new SariaIdleAnimator();
    ///   AI:       IdleAnimator.Update(Projectile);           // in idle block
    ///   PostDraw: IdleAnimator.Draw(Projectile, lightColor); // when frame 0-3
    ///   Net sync: IdleAnimator.Write/Read in SendExtraAI/ReceiveExtraAI
    ///   Sneeze:   if (IdleAnimator.IsSneezeReady) { /* play dust+sound */ IdleAnimator.ConsumeSneeze(); }
    ///   Biome:    IdleAnimator.SneezeBiomeRate = 2.0f; // in ZoneSnow, etc.
    /// </summary>
    public class SariaIdleAnimator
    {
        // =====================================================================
        //  CONSTANTS
        // =====================================================================

        /// <summary>Frames 0 through this value (inclusive) on the main sheet are idle torso frames.</summary>
        public const int IdleFrameMax = 3;

        /// <summary>Total frames per overlay spritesheet (4 states × 4 frames).</summary>
        private const int OverlayTotalFrames = 16;

        /// <summary>Total frames per eye overlay spritesheet (3 states × 4 frames).</summary>
        private const int EyeOverlayTotalFrames = 12;

        /// <summary>Frames per state (each state is a 4-frame set).</summary>
        private const int FramesPerState = 4;

        /// <summary>Trail length for feet idle overlay, matching normal SariaFeet (HowlongisTrail=25).</summary>
        private const int FeetTrailLength = 25;

        /// <summary>Trail decay timers for feet idle overlay, keyed by projectile.whoAmI.</summary>
        private static readonly Dictionary<int, float> _feetTrailDecayTimers = new Dictionary<int, float>();

        // =====================================================================
        //  TEXTURE PATHS
        // =====================================================================

        private const string FeetIdlePath = "SariaMod/Items/Strange/GlobalSariaAnimations/SariaFeetIdle";
        private const string NormalFaceIdlePath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaNormalFaceIdle";
        private const string NormalFaceIdleBackPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaNormalFaceIdleBack";
        private const string HappyFaceIdlePath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaHappyFaceIdle";
        private const string HappyFaceIdleBackPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaHappyFaceIdleBack";
        private const string SadFaceIdlePath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaSadFaceIdle";
        private const string SadFaceIdleBackPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaSadFaceIdleBack";
        private const string AngryFaceIdlePath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaAngryFaceIdle";
        private const string AngryFaceIdleUnderlayPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaAngryFaceIdleUnderlay";
        private const string AngryFaceIdleOverlayPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaAngryFaceIdleOverlay";
        private const string Form6FaceIdlePath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaNormalFaceIdle";
        private const string Form6FaceIdleBackPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaNormalFaceIdleBack";
        private const string Form6EyeBackgroundPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaIdleEyeBackground";
        private const string Form6HappyFaceIdlePath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaHappyFaceIdle";
        private const string Form6HappyFaceIdleBackPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaHappyFaceIdleBack";
        private const string Form6SadFaceIdlePath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaSadFaceIdle";
        private const string Form6SadFaceIdleBackPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaSadFaceIdleBack";
        private const string Form6AngryFaceIdlePath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaAngryFaceIdle";
        private const string Form6AngryFaceIdleBackPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaAngryFaceIdleBack";
        private const string Form7FaceIdlePath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaNormalFaceIdle";
        private const string Form7FaceIdleBackPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaNormalFaceIdleBack";
        private const string Form7EyeBackgroundPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaNormalFaceIdleBackground";
        private const string Form7HappyFaceIdlePath        = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaHappyFaceIdle";
        private const string Form7HappyEyeBackgroundPath   = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaHappyFaceIdleBackground";
        private const string Form7SadFaceIdlePath          = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaSadFaceIdle";
        private const string Form7SadEyeBackgroundPath     = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaSadFaceIdleBackground";
        private const string Form7AngryFaceIdlePath        = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaAngryFaceIdle";
        private const string Form7AngryEyeBackgroundPath   = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaAngryFaceIdleBackground";
        private const float Form7LookBackOffset = 1f;
        private const string MouthIdleUnderlayPath = "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaMouthIdleUnderlay";

        // =====================================================================
        //  ENUMS
        // =====================================================================

        public enum LegState
        {
            Casual = 0,          // frames 0-3,   loops
            GoingToProper = 1,   // frames 4-7,   plays once → Proper
            Proper = 2,          // frames 8-11,  loops
            GoingToCasual = 3    // frames 12-15, plays once → Casual
        }

        public enum ArmState
        {
            Down = 0,            // frames 0-3,   loops
            GoingUp = 1,         // frames 4-7,   plays once → Up
            Up = 2,              // frames 8-11,  loops
            GoingDown = 3        // frames 12-15, plays once → Down
        }

        public enum EyeState
        {
            Looking = 0,         // frames 0-3,   loops (base idle eyes)
            Blinking = 1,        // frames 4-7,   plays once → Opening
            Opening = 2          // frames 8-11,  plays once → Looking
        }

        // =====================================================================
        //  STATE FIELDS
        // =====================================================================

        public LegState CurrentLegState { get; set; } = LegState.Casual;
        public ArmState CurrentArmState { get; set; } = ArmState.Down;
        public EyeState CurrentEyeState { get; set; } = EyeState.Looking;

        // Tracks the previous Projectile.frame to detect when the base animation
        // completes a 4-frame cycle (local frame wraps from 3 back to 0).
        private int _prevProjectileFrame = -1;

        // Current local frame (0-3) within the 4-frame cycle, updated every tick.
        // Used by IsSneezeReady to ensure the sneeze fires only at frame 0.
        private int _localFrame;

        // Cycle counting for looping states (Short=3, Medium=4, Long=5)
        private int _legTargetCycles;
        private int _armTargetCycles;
        private int _legCyclesInState;
        private int _armCyclesInState;

        // Eye blink cycle counting — blinks every 3-4 cycles of Looking
        private int _eyeBlinkTarget;
        private int _eyeCyclesInState;

        // Eye sheet toggle — alternates between normal and looking-back sheets every 2-3 blinks
        private bool _eyeLookingBack;
        private int _eyeBlinksSinceSwap;
        private int _eyeSwapTarget;

        // Free eye mode — eyes smoothly track player position with max 2px offset
        private bool _eyeFreeMode;
        private Vector2 _eyeFreeOffset;
        private bool _eyeFreeFlipped;
        private int _eyeFreeCycles;
        private int _eyeFreeTarget;
        private bool _eyeFreeReturning;

        // --- Warmup tracking ---
        // Counts completed loop cycles in the initial base state.
        // No transitions until warmup is done.
        private int _legWarmupCycles;
        private int _armWarmupCycles;
        private bool _legWarmedUp;
        private bool _armWarmedUp;

        // --- Sneeze system (countdown timer) ---
        private float _sneezeTimer = 5400f; // counts down from SneezeThreshold
        private bool _sneezeQueued; // timer hit zero, waiting for overlay alignment
        private bool _sneezeTimerDoubled; // prevents double-fire of OnSneezeDust

        // --- Displayed mood buffering ---
        // _displayedMood holds the currently shown face expression.
        // Changes queue into _pendingMood and only flush at natural animation
        // break points (Opening→Looking transition or idle exit).
        public int DisplayedMood { get => _displayedMood; set => _displayedMood = value; }
        private int _displayedMood;
        private int _pendingMood;
        private bool _hasPendingMood;

        // Form 7 ghost face fade transition — replaces the blink-driven flush for this form.
        // Alpha fades 1→0 (fade-out, 1s), mood swaps at 0, then 0→1 (fade-in, 1s).
        // Row-by-row sine wave runs through the eye pixels continuously (always on).
        private float _form7EyeAlpha  = 1f;
        private bool  _form7FadingOut;
        private bool  _form7FadeActive;
        private float _form7WavePhase;   // advances 0.09f/tick, same as dialogue UI
        private int   _currentTransform; // cached from UpdateEyeOffsetVisual each tick

        /// <summary>Current wave phase for Form 7 ghost eye ripple. Read by non-idle draw calls.</summary>
        public float Form7WavePhase => _form7WavePhase;
        /// <summary>Current overall alpha for Form 7 ghost eyes [0,1]. Read by non-idle draw calls.</summary>
        public float Form7EyeAlpha  => _form7EyeAlpha;

        // =====================================================================
        //  TUNABLE CONFIGURATION (public properties)
        // =====================================================================

        /// <summary>Full loop cycles in base state before any transition is allowed.</summary>
        public int WarmupCycles { get; set; } = 3;

        /// <summary>
        /// Sneeze countdown threshold (~5400 ticks = 1.5 minutes at rate 1.0).
        /// Timer counts down from this value. When it reaches 0 the sneeze is queued
        /// and will fire at the next valid animation window.
        /// </summary>
        public float SneezeThreshold { get; set; } = 5400f;

        /// <summary>
        /// Per-tick accumulation rate for the sneeze timer.
        /// Set this externally based on biome + indoor/outdoor conditions:
        ///   Snow outdoor         → 3.0   (coldest, sneezes fastest)
        ///   Jungle outdoor       → 3.0   (humid, sneezes fastest)
        ///   Forest outdoor       → 2.0   (next most common)
        ///   Snow indoor          → 1.3   (slightly above normal)
        ///   Jungle/Forest indoor → 1.0   (normal)
        ///   Rain outdoor (!form1)→ max(current, 2.0)
        ///   Normal / Underground → 1.0
        ///   Hallow               → 0.2   (magical comfort)
        ///   Near Campfire+wall   → 0.05  (nearly pauses)
        /// Values &lt;= 0 pause buildup.
        /// </summary>
        public float SneezeBiomeRate { get; set; } = 1.0f;

        // =====================================================================
        //  READ-ONLY STATE (for Saria.cs integration)
        // =====================================================================

        /// <summary>
        /// Current sneeze countdown timer. Counts down from SneezeThreshold to 0.
        /// When it reaches 0, the sneeze is queued and overlays begin aligning.
        /// </summary>
        public float SneezeTimer => _sneezeTimer;

        /// <summary>Whether the sneeze is currently queued (waiting for overlay alignment).</summary>
        public bool IsSneezeQueued => _sneezeQueued;

        /// <summary>
        /// True when the sneeze is ready to fire: timer expired AND
        /// warmup complete AND legs in Casual AND arms in Down.
        /// Warmup requirement prevents nonstop sneezing after interrupts.
        /// Saria.cs should check this, set frame to 12, then call OnSneezeStart().
        /// </summary>
        public bool IsSneezeReady =>
            _sneezeQueued &&
            _legWarmedUp && _armWarmedUp &&
            CurrentLegState == LegState.Casual &&
            CurrentArmState == ArmState.Down &&
            CurrentEyeState == EyeState.Looking &&
            !_eyeFreeMode && !_eyeLookingBack &&
            _localFrame == 0;

        /// <summary>Whether this animator is currently active.</summary>
        public bool IsActive { get; set; }

        /// <summary>True while Saria's eyes are in free-roaming mode (smoothly tracking the player).</summary>
        public bool IsEyeFreeMode => _eyeFreeMode;

        // =====================================================================
        //  PUBLIC API
        // =====================================================================

        /// <summary>
        /// Notifies the animator of the current mood. When idle is active, the
        /// visible face expression only updates at the Opening→Looking eye
        /// transition or when leaving idle, so mid-idle mood changes feel natural.
        /// Ghost form is exempt (its draw path never consults _displayedMood).
        /// Safe to call every PostDraw frame.
        /// </summary>
        public void UpdateDisplayedMood(int newMood)
        {
            if (!IsActive)
            {
                _displayedMood = newMood;
                _hasPendingMood = false;
                return;
            }

            if (newMood != _displayedMood)
            {
                _pendingMood = newMood;
                _hasPendingMood = true;
                // Form 7 ghost: start a fade transition instead of waiting for a blink
                if (_currentTransform == 6 && !_form7FadeActive)
                {
                    _form7FadeActive = true;
                    _form7FadingOut  = true;
                }
            }
            else
            {
                _hasPendingMood = false;
            }
        }

        /// <summary>
        /// Forces CurrentLegState directly from the synced bools received from the owner's packet.
        /// Call every tick on non-owner clients to bypass the state machine entirely.
        /// </summary>
        public void ApplySyncedLegState(bool casual, bool goingToCasual, bool proper, bool goingToProper)
        {
            if (goingToProper)       CurrentLegState = LegState.GoingToProper;
            else if (proper)         CurrentLegState = LegState.Proper;
            else if (goingToCasual)  CurrentLegState = LegState.GoingToCasual;
            else                     CurrentLegState = LegState.Casual;
        }

        /// <summary>
        /// Forces CurrentArmState directly from the synced bools received from the owner's packet.
        /// Call every tick on non-owner clients to bypass the state machine entirely.
        /// </summary>
        public void ApplySyncedArmState(bool down, bool goingUp, bool up, bool goingDown)
        {
            if (goingUp)        CurrentArmState = ArmState.GoingUp;
            else if (up)        CurrentArmState = ArmState.Up;
            else if (goingDown) CurrentArmState = ArmState.GoingDown;
            else                CurrentArmState = ArmState.Down;
        }

        /// <summary>
        /// Forces CurrentEyeState directly from the synced bools received from the owner's packet.
        /// Call every tick on non-owner clients to bypass the state machine entirely.
        /// </summary>
        public void ApplySyncedEyeState(bool looking, bool blinking, bool opening)
        {
            if (blinking)       CurrentEyeState = EyeState.Blinking;
            else if (opening)   CurrentEyeState = EyeState.Opening;
            else                CurrentEyeState = EyeState.Looking;
        }

        /// <summary>
        /// Call every AI tick. Overlay frames are derived from projectile.frame,
        /// so legs and arms always stay perfectly in sync with the torso.
        /// State transitions happen at cycle boundaries (when local frame wraps 3→0).
        /// </summary>
        public void Update(Projectile projectile, int transform = 0)
        {
            if (!IsActive)
                return;

            int localFrame = projectile.frame % FramesPerState;
            _localFrame = localFrame;

            // Detect when the base animation completes a 4-frame cycle
            bool frameChanged = projectile.frame != _prevProjectileFrame;
            int prevLocal = _prevProjectileFrame >= 0 ? _prevProjectileFrame % FramesPerState : -1;
            bool cycleCompleted = frameChanged && prevLocal == FramesPerState - 1 && localFrame == 0;
            _prevProjectileFrame = projectile.frame;

            // Handle state transitions at cycle boundaries
            if (cycleCompleted)
            {
                HandleLegCycleComplete();
                HandleArmCycleComplete();
                HandleEyeCycleComplete();
            }

            // Eye offset is now driven by UpdateEyeOffsetVisual (called on every client
            // in Saria.AI) so remote clients see the eyes track the owner too.
        }

        /// <summary>
        /// Visual-only eye offset tick. Safe to call on EVERY client (owner and
        /// remotes) every AI tick — does not mutate any synced state, only the
        /// locally-computed _eyeFreeOffset / _eyeFreeFlipped that the draw layer
        /// reads. Driven by the already-synced _eyeFreeMode / _eyeLookingBack flags
        /// plus the synced projectile.owner so all clients converge on the same
        /// visual gaze direction.
        /// </summary>
        public void UpdateEyeOffsetVisual(Projectile projectile, int transform)
        {
            _currentTransform = transform;

            // Form 7 ghost: tick fade and update directional eye offset
            if (transform == 6)
            {
                // Wave phase always advances so the ripple never freezes
                _form7WavePhase += 0.09f;
                if (_form7WavePhase > MathF.PI * 20f)
                    _form7WavePhase -= MathF.PI * 20f;

                UpdateForm7EyeFade();
                UpdateForm7EyeOffset(projectile);
            }
            else if (_eyeFreeMode)
                UpdateFreeEyeOffset(projectile);
        }

        private void UpdateForm7EyeFade()
        {
            if (!_form7FadeActive) return;

            const float FadeSpeed = 1f / 60f; // 60 ticks per direction = 1 second each

            if (_form7FadingOut)
            {
                _form7EyeAlpha -= FadeSpeed;
                if (_form7EyeAlpha <= 0f)
                {
                    _form7EyeAlpha = 0f;
                    // Flush pending mood at the invisible midpoint (mirrors blink-open flush)
                    if (_hasPendingMood)
                    {
                        _displayedMood  = _pendingMood;
                        _hasPendingMood = false;
                    }
                    _form7FadingOut = false; // begin fade-in
                }
            }
            else
            {
                _form7EyeAlpha += FadeSpeed;
                if (_form7EyeAlpha >= 1f)
                {
                    _form7EyeAlpha  = 1f;
                    _form7FadeActive = false;
                }
            }
        }

        /// <summary>
        /// Call in PostDraw BEFORE faces. Draws feet and leg overlays + their masks.
        /// Guard: returns immediately if the form has no idle overlay textures.
        /// </summary>
        /// <summary>
        /// Draws only the SariaFeetIdle overlay. Call BEFORE SariaBodyDraw so the feet
        /// glow renders underneath the body.
        /// </summary>

        public void DrawFeetPass(Projectile projectile, Color lightColor)
        {
            int localFrame = projectile.frame % FramesPerState;
            int legFrame = GetAbsoluteFrame(CurrentLegState, localFrame);
            DrawFeetIdleOverlay(projectile, FeetIdlePath, legFrame, true, lightColor, 4f);
        }

        public void DrawLegsPass(Projectile projectile, int transform, Color lightColor)
        {
            int formNumber = transform + 1;
            string legsPath = $"SariaMod/Items/Strange/{formNumber}SariaAnimations/{formNumber}SariaLegs";

            if (!ModContent.HasAsset(legsPath))
                return;

            int localFrame = projectile.frame % FramesPerState;
            int legFrame = GetAbsoluteFrame(CurrentLegState, localFrame);
            bool form5Glow = transform == 4;
            bool form2WaterGlow = transform == 1 && projectile.IsMostlyInNonLavaLiquid();

            // Feet idle drawn separately in DrawFeetPass (before body), skip here

            // Legs overlay
            DrawOverlay(projectile, legsPath, legFrame, true, lightColor, 1f, form5Glow || form2WaterGlow);

            // Form 4 mask overlays for idle legs (same order as SariaBodyDraw body masks)
            if (transform == 3)
            {
                var legsTex = ModContent.Request<Texture2D>(legsPath, AssetRequestMode.ImmediateLoad).Value;
                var legsMask = SariaBodyMaskKey.GetBodyMask(legsTex);
                if (legsMask != null)
                    DrawOverlayDirect(projectile, legsMask, legFrame, true, lightColor, 1f);
                var legsMask2 = SariaBodyMaskKey.GetBodyMask2(legsTex);
                if (legsMask2 != null)
                    DrawOverlayElectric(projectile, legsMask2, legFrame, true, lightColor, 1f);
                if (Main.rand.NextBool(40))
                {
                    var legsMask3 = SariaBodyMaskKey.GetBodyMask3(legsTex);
                    if (legsMask3 != null)
                        DrawOverlayDirect(projectile, legsMask3, legFrame, true, lightColor, 1f);
                }
            }

            // Form 5 mask overlays for idle legs
            if (transform == 4)
            {
                var legsTex = ModContent.Request<Texture2D>(legsPath, AssetRequestMode.ImmediateLoad).Value;
                var legsM1 = SariaBodyMaskKey.GetForm5Mask1(legsTex);
                if (legsM1 != null)
                    DrawOverlay5Glow(projectile, legsM1, legFrame, true, lightColor, 1f, true, false);
                var legsM2 = SariaBodyMaskKey.GetForm5Mask2(legsTex);
                if (legsM2 != null)
                    DrawOverlay5Glow(projectile, legsM2, legFrame, true, lightColor, 1f, false, true);
            }
        }

        /// <summary>
        /// Call in PostDraw AFTER faces and eyes. Draws idle arm overlay + its masks,
        /// so arms always appear on top of the face and chest pieces for all forms.
        /// Guard: returns immediately if the form has no idle overlay textures.
        /// </summary>
        public void DrawArmsPass(Projectile projectile, int transform, Color lightColor)
        {
            int formNumber = transform + 1;
            bool isRight = projectile.spriteDirection == 1;
            string armsPath = isRight
                ? $"SariaMod/Items/Strange/{formNumber}SariaAnimations/{formNumber}SariaArmRight"
                : $"SariaMod/Items/Strange/{formNumber}SariaAnimations/{formNumber}SariaArmLeft";

            if (!ModContent.HasAsset(armsPath))
                return;

            int localFrame = projectile.frame % FramesPerState;
            int armFrame = GetAbsoluteFrame(CurrentArmState, localFrame);
            bool form5Glow = transform == 4;
            bool form2WaterGlow = transform == 1 && projectile.IsMostlyInNonLavaLiquid();

            // Arm base texture
            DrawOverlay(projectile, armsPath, armFrame, true, lightColor, 1f, form5Glow || form2WaterGlow);

            // Form 4 mask overlays for idle arms (Mask1 glow, Mask2 electric, Mask3 rare spark)
            if (transform == 3)
            {
                var armsTex = ModContent.Request<Texture2D>(armsPath, AssetRequestMode.ImmediateLoad).Value;
                var armsMask = SariaBodyMaskKey.GetBodyMask(armsTex);
                if (armsMask != null)
                    DrawOverlayDirect(projectile, armsMask, armFrame, true, lightColor, 1f);
                var armsMask2 = SariaBodyMaskKey.GetBodyMask2(armsTex);
                if (armsMask2 != null)
                    DrawOverlayElectric(projectile, armsMask2, armFrame, true, lightColor, 1f);
                if (Main.rand.NextBool(40))
                {
                    var armsMask3 = SariaBodyMaskKey.GetBodyMask3(armsTex);
                    if (armsMask3 != null)
                        DrawOverlayDirect(projectile, armsMask3, armFrame, true, lightColor, 1f);
                }
            }

            // Form 5 mask overlays for idle arms (pulsing pink/green glow)
            if (transform == 4)
            {
                var armsTex = ModContent.Request<Texture2D>(armsPath, AssetRequestMode.ImmediateLoad).Value;
                var armsM1 = SariaBodyMaskKey.GetForm5Mask1(armsTex);
                if (armsM1 != null)
                    DrawOverlay5Glow(projectile, armsM1, armFrame, true, lightColor, 1f, true, false);
                var armsM2 = SariaBodyMaskKey.GetForm5Mask2(armsTex);
                if (armsM2 != null)
                    DrawOverlay5Glow(projectile, armsM2, armFrame, true, lightColor, 1f, false, true);
            }
        }

        /// <summary>
        /// Resets all state to defaults. Call when leaving idle
        /// (e.g., starting to walk, attack, sleep, eat).
        /// </summary>
        public void Reset()
        {
            CurrentLegState = LegState.Casual;
            CurrentArmState = ArmState.Down;
            CurrentEyeState = EyeState.Looking;

            _prevProjectileFrame = -1;
            _localFrame = 0;

            _legTargetCycles = 0;
            _armTargetCycles = 0;
            _legCyclesInState = 0;
            _armCyclesInState = 0;

            _eyeBlinkTarget = 0;
            _eyeCyclesInState = 0;
            _eyeLookingBack = false;
            _eyeBlinksSinceSwap = 0;
            _eyeSwapTarget = 0;
            _eyeFreeMode = false;
            _eyeFreeOffset = Vector2.Zero;
            _eyeFreeFlipped = false;
            _eyeFreeCycles = 0;
            _eyeFreeTarget = 0;
            _eyeFreeReturning = false;

            _legWarmupCycles = 0;
            _armWarmupCycles = 0;
            _legWarmedUp = false;
            _armWarmedUp = false;

            _sneezeTimer = SneezeThreshold;
            _sneezeQueued = false;
            _sneezeTimerDoubled = false;

            _displayedMood = 0;
            _pendingMood = 0;
            _hasPendingMood = false;

            IsActive = false;
        }

        /// <summary>
        /// Soft deactivation — preserves leg/arm/sneeze state so she resumes
        /// her current pose when returning to idle. Use when she leaves idle
        /// briefly (walking, short movement). Call Reset() for hard resets
        /// (sleep, eat, major state changes).
        /// </summary>
        public void Deactivate()
        {
            if (_hasPendingMood) { _displayedMood = _pendingMood; _hasPendingMood = false; }
            IsActive = false;
            _prevProjectileFrame = -1;
        }

        /// <summary>
        /// Call from Saria.cs when the sneeze animation starts (frame 12).
        /// Sets the countdown timer to half threshold and clears the queue.
        /// </summary>
        public void OnSneezeStart()
        {
            _sneezeTimer = SneezeThreshold / 2f;
            _sneezeQueued = false;
            _sneezeTimerDoubled = false;
        }

        /// <summary>
        /// Call from Saria.cs when the sneeze dust appears (frame 22).
        /// Doubles the timer, restoring it to ~full threshold.
        /// Uses a flag to prevent multiple calls per sneeze cycle.
        /// </summary>
        public void OnSneezeDust()
        {
            if (_sneezeTimerDoubled) return;
            _sneezeTimer *= 2f;
            _sneezeTimerDoubled = true;
        }

        /// <summary>
        /// Resets the sneeze timer to full threshold and clears the queue.
        /// Call when a boss is alive to suppress sneezing entirely.
        /// </summary>
        public void ResetSneezeTimer()
        {
            _sneezeTimer = SneezeThreshold;
            _sneezeQueued = false;
        }

        /// <summary>
        /// Call when normal idle is interrupted (movement, state change).
        /// Does NOT modify the sneeze timer — timer ticks naturally at all times.
        /// Only frames 12-35 (sneeze animation) need protection from interruption;
        /// that is handled in Saria.cs via CanMove freeze.
        /// </summary>
        public void Interrupt()
        {
            // Timer is never modified here — it just keeps counting down.
            // Sneeze queue is preserved if already set.
            if (_hasPendingMood) { _displayedMood = _pendingMood; _hasPendingMood = false; }

            CurrentLegState = LegState.Casual;
            CurrentArmState = ArmState.Down;
            CurrentEyeState = EyeState.Looking;
            _prevProjectileFrame = -1;
            _localFrame = 0;
            _legTargetCycles = 0;
            _armTargetCycles = 0;
            _legCyclesInState = 0;
            _armCyclesInState = 0;
            _eyeBlinkTarget = 0;
            _eyeCyclesInState = 0;
            _eyeLookingBack = false;
            _eyeBlinksSinceSwap = 0;
            _eyeSwapTarget = 0;
            _eyeFreeMode = false;
            _eyeFreeOffset = Vector2.Zero;
            _eyeFreeFlipped = false;
            _eyeFreeCycles = 0;
            _eyeFreeTarget = 0;
            _eyeFreeReturning = false;
            _legWarmupCycles = 0;
            _armWarmupCycles = 0;
            _legWarmedUp = false;
            _armWarmedUp = false;
            _sneezeTimerDoubled = false;
            IsActive = false;
        }

        /// <summary>
        /// Ticks the sneeze countdown timer. Call every AI tick from Saria.cs
        /// (not just during idle). Timer counts down by SneezeBiomeRate each tick.
        /// When it reaches 0, the sneeze is queued.
        /// </summary>
        public void TickSneezeTimer()
        {
            if (_sneezeQueued) return;
            if (SneezeBiomeRate > 0f)
            {
                _sneezeTimer -= SneezeBiomeRate;
                if (_sneezeTimer <= 0f)
                {
                    _sneezeTimer = 0f;
                    _sneezeQueued = true;
                }
            }
        }

        // =====================================================================
        //  NET SYNC
        // =====================================================================

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write((int)CurrentLegState);
            writer.Write((int)CurrentArmState);
            writer.Write(_legWarmupCycles);
            writer.Write(_armWarmupCycles);
            writer.Write(_legWarmedUp);
            writer.Write(_armWarmedUp);
            writer.Write(_legTargetCycles);
            writer.Write(_armTargetCycles);
            writer.Write(_legCyclesInState);
            writer.Write(_armCyclesInState);
            writer.Write(_sneezeTimer);
            writer.Write(_sneezeQueued);
            writer.Write(_sneezeTimerDoubled);
            writer.Write((int)CurrentEyeState);
            writer.Write(_eyeBlinkTarget);
            writer.Write(_eyeCyclesInState);
            writer.Write(_eyeLookingBack);
            writer.Write(_eyeBlinksSinceSwap);
            writer.Write(_eyeSwapTarget);
            writer.Write(_eyeFreeMode);
            writer.Write(_eyeFreeFlipped);
            writer.Write(_eyeFreeCycles);
            writer.Write(_eyeFreeTarget);
            writer.Write(_eyeFreeReturning);
            writer.Write(_displayedMood);
            writer.Write(_pendingMood);
            writer.Write(_hasPendingMood);
        }

        public void Read(System.IO.BinaryReader reader)
        {
            CurrentLegState = (LegState)reader.ReadInt32();
            CurrentArmState = (ArmState)reader.ReadInt32();
            _legWarmupCycles = reader.ReadInt32();
            _armWarmupCycles = reader.ReadInt32();
            _legWarmedUp = reader.ReadBoolean();
            _armWarmedUp = reader.ReadBoolean();
            _legTargetCycles = reader.ReadInt32();
            _armTargetCycles = reader.ReadInt32();
            _legCyclesInState = reader.ReadInt32();
            _armCyclesInState = reader.ReadInt32();
            _sneezeTimer = reader.ReadSingle();
            _sneezeQueued = reader.ReadBoolean();
            _sneezeTimerDoubled = reader.ReadBoolean();
            CurrentEyeState = (EyeState)reader.ReadInt32();
            _eyeBlinkTarget = reader.ReadInt32();
            _eyeCyclesInState = reader.ReadInt32();
            _eyeLookingBack = reader.ReadBoolean();
            _eyeBlinksSinceSwap = reader.ReadInt32();
            _eyeSwapTarget = reader.ReadInt32();
            _eyeFreeMode = reader.ReadBoolean();
            _eyeFreeFlipped = reader.ReadBoolean();
            _eyeFreeCycles = reader.ReadInt32();
            _eyeFreeTarget = reader.ReadInt32();
            _eyeFreeReturning = reader.ReadBoolean();
            _displayedMood = reader.ReadInt32();
            _pendingMood = reader.ReadInt32();
            _hasPendingMood = reader.ReadBoolean();
        }

        // =====================================================================
        //  LEGS CYCLE COMPLETE
        // =====================================================================

        /// <summary>
        /// Called when the base Projectile.frame completes a 4-frame cycle (wraps 3→0).
        /// Handles warmup gating, cycle-counted transitions, and sneeze queue alignment for legs.
        /// Order: warmup first (always completes), then sneeze alignment, then normal cycling.
        /// When sneeze is queued: holds at Casual (target), skips from Proper, lets play-once finish.
        /// </summary>
        private void HandleLegCycleComplete()
        {
            bool isLooping = IsLoopingLegState(CurrentLegState);

            if (isLooping)
            {
                // WARMUP GATE: count cycles, block transitions until done
                if (!_legWarmedUp)
                {
                    _legWarmupCycles++;
                    if (_legWarmupCycles >= WarmupCycles)
                    {
                        _legWarmedUp = true;
                        _legTargetCycles = PickRandomDuration();
                        _legCyclesInState = 0;
                    }
                    return;
                }

                // SNEEZE QUEUE ALIGNMENT: override normal transitions after warmup
                if (_sneezeQueued)
                {
                    if (CurrentLegState == LegState.Casual)
                        return; // At target — hold here until sneeze fires
                    else
                        AdvanceLegState(); // Not at target (Proper) — advance immediately
                    return;
                }

                // COUNT CYCLES toward target
                _legCyclesInState++;

                if (_legCyclesInState >= _legTargetCycles)
                {
                    AdvanceLegState();
                }
            }
            else
            {
                // Play-once transition finished → advance to next looping state
                AdvanceLegState();
            }
        }

        // =====================================================================
        //  ARMS CYCLE COMPLETE
        // =====================================================================

        /// <summary>
        /// Called when the base Projectile.frame completes a 4-frame cycle (wraps 3→0).
        /// Handles warmup gating, cycle-counted transitions, and sneeze queue alignment for arms.
        /// When sneeze is queued: holds at Down (target), skips from Up, lets play-once finish.
        /// </summary>
        private void HandleArmCycleComplete()
        {
            bool isLooping = IsLoopingArmState(CurrentArmState);

            if (isLooping)
            {
                // WARMUP GATE: count cycles, block transitions until done
                if (!_armWarmedUp)
                {
                    _armWarmupCycles++;
                    if (_armWarmupCycles >= WarmupCycles)
                    {
                        _armWarmedUp = true;
                        _armTargetCycles = PickRandomDuration();
                        _armCyclesInState = 0;
                    }
                    return;
                }

                // SNEEZE QUEUE ALIGNMENT: override normal transitions after warmup
                if (_sneezeQueued)
                {
                    if (CurrentArmState == ArmState.Down)
                        return; // At target — hold here until sneeze fires
                    else
                        AdvanceArmState(); // Not at target (Up) — advance immediately
                    return;
                }

                // COUNT CYCLES toward target
                _armCyclesInState++;

                if (_armCyclesInState >= _armTargetCycles)
                {
                    AdvanceArmState();
                }
            }
            else
            {
                // Play-once transition finished → advance to next looping state
                AdvanceArmState();
            }
        }

        // =====================================================================
        //  EYES CYCLE COMPLETE
        // =====================================================================

        /// <summary>
        /// Called when the base Projectile.frame completes a 4-frame cycle (wraps 3→0).
        /// Eyes are simpler than legs/arms: Looking loops for 4-5 cycles then blinks.
        /// Blinking and Opening each play once before returning to Looking.
        /// </summary>
        private void HandleEyeCycleComplete()
        {
            if (CurrentEyeState == EyeState.Looking)
            {
                // SNEEZE QUEUE ALIGNMENT: lock to default texture but allow normal blinking
                if (_sneezeQueued)
                {
                    if (_eyeFreeMode)
                        _eyeFreeReturning = true;
                    _eyeLookingBack = false;
                }

                // If the displayed face doesn't match what it should be, blink immediately
                // to reach the Opening→Looking boundary where the face will swap.
                if (_hasPendingMood)
                {
                    AdvanceEyeState(); // Looking → Blinking (skip remaining cycle)
                    return;
                }

                // Initialize blink target on first entry
                if (_eyeBlinkTarget <= 0)
                    _eyeBlinkTarget = PickBlinkDuration();

                _eyeCyclesInState++;

                if (_eyeCyclesInState >= _eyeBlinkTarget)
                {
                    AdvanceEyeState(); // Looking → Blinking
                }
            }
            else
            {
                // Blinking and Opening play once then advance
                AdvanceEyeState();
            }
        }

        // =====================================================================
        //  STATE MACHINE TRANSITIONS
        // =====================================================================

        private void AdvanceLegState()
        {
            CurrentLegState = CurrentLegState switch
            {
                LegState.Casual => LegState.GoingToProper,
                LegState.GoingToProper => LegState.Proper,
                LegState.Proper => LegState.GoingToCasual,
                LegState.GoingToCasual => LegState.Casual,
                _ => LegState.Casual
            };

            // When entering a looping state, pick a new cycle target
            if (IsLoopingLegState(CurrentLegState))
            {
                _legTargetCycles = PickRandomDuration();
                _legCyclesInState = 0;
            }
        }

        private void AdvanceArmState()
        {
            CurrentArmState = CurrentArmState switch
            {
                ArmState.Down => ArmState.GoingUp,
                ArmState.GoingUp => ArmState.Up,
                ArmState.Up => ArmState.GoingDown,
                ArmState.GoingDown => ArmState.Down,
                _ => ArmState.Down
            };

            // When entering a looping state, pick a new cycle target
            if (IsLoopingArmState(CurrentArmState))
            {
                _armTargetCycles = PickRandomDuration();
                _armCyclesInState = 0;
            }
        }

        private void AdvanceEyeState()
        {
            CurrentEyeState = CurrentEyeState switch
            {
                EyeState.Looking => EyeState.Blinking,
                EyeState.Blinking => EyeState.Opening,
                EyeState.Opening => EyeState.Looking,
                _ => EyeState.Looking
            };

            // ALL eye mode transitions happen at the Opening boundary (after
            // Blinking → Opening). Free eye counting/deactivation runs even
            // during sneeze alignment so _eyeFreeMode can reach false and
            // unblock IsSneezeReady. New activations are blocked by sneeze.
            if (CurrentEyeState == EyeState.Opening)
            {
                if (_hasPendingMood) { _displayedMood = _pendingMood; _hasPendingMood = false; }

                if (_eyeFreeMode)
                {
                    // Count completed blinks toward free eye duration
                    if (!_eyeFreeReturning)
                    {
                        _eyeFreeCycles++;
                        if (_eyeFreeCycles >= _eyeFreeTarget)
                            _eyeFreeReturning = true;
                    }

                    // Deactivate free eye once offset has returned to zero
                    if (_eyeFreeReturning && _eyeFreeOffset == Vector2.Zero)
                    {
                        _eyeFreeMode = false;
                        _eyeFreeReturning = false;
                        _eyeFreeFlipped = false;
                    }
                }
                else if (!_sneezeQueued)
                {
                    // Looking-back toggle + new activations (blocked during sneeze)
                    if (_eyeLookingBack)
                    {
                        // Was looking back — always swap back to normal after one blink
                        _eyeLookingBack = false;
                        _eyeBlinksSinceSwap = 0;
                        _eyeSwapTarget = PickSwapTarget();
                    }
                    else
                    {
                        // On normal sheet — count blinks toward swap target
                        if (_eyeSwapTarget <= 0)
                            _eyeSwapTarget = PickSwapTarget();

                        _eyeBlinksSinceSwap++;

                        if (_eyeBlinksSinceSwap >= _eyeSwapTarget)
                        {
                            _eyeLookingBack = true;
                            _eyeBlinksSinceSwap = 0;
                        }
                    }

                    // Randomly activate free eye mode (~33% chance, lasts 4-5 blinks)
                    if (!_eyeFreeMode && Main.rand.Next(3) == 0)
                    {
                        _eyeFreeMode = true;
                        _eyeFreeCycles = 0;
                        _eyeFreeTarget = Main.rand.NextBool() ? 4 : 5;
                        _eyeFreeReturning = false;
                        _eyeFreeFlipped = false;
                    }
                }
            }

            // When returning to Looking, reset blink timing
            if (CurrentEyeState == EyeState.Looking)
            {
                _eyeBlinkTarget = PickBlinkDuration();
                _eyeCyclesInState = 0;
            }
        }

        // =====================================================================
        //  FREE EYE TRACKING
        // =====================================================================

        /// <summary>
        /// Updates the free eye offset each tick, smoothly tracking the player owner's
        /// position with a maximum 1-pixel horizontal / 1-pixel vertical offset. When returning to neutral, lerps
        /// back to zero. Actual mode deactivation happens at blink boundaries
        /// in AdvanceEyeState, matching the looking-back transition pattern.
        /// </summary>
        private void UpdateForm7EyeOffset(Projectile projectile)
        {
            if (_eyeFreeMode)
            {
                UpdateFreeEyeOffset(projectile);
                return;
            }

            Vector2 target = _eyeLookingBack
                ? new Vector2(projectile.spriteDirection == 1 ? -Form7LookBackOffset : Form7LookBackOffset, 0f)
                : Vector2.Zero;
            _eyeFreeOffset = Vector2.Lerp(_eyeFreeOffset, target, 0.1f);
            if (_eyeFreeOffset.Length() < 0.05f)
                _eyeFreeOffset = Vector2.Zero;
        }

        private void UpdateFreeEyeOffset(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];

            if (_eyeFreeReturning)
            {
                _eyeFreeOffset = Vector2.Lerp(_eyeFreeOffset, Vector2.Zero, 0.1f);
                if (_eyeFreeOffset.Length() < 0.05f)
                {
                    _eyeFreeOffset = Vector2.Zero;
                }
                return;
            }

            float sneezeX = projectile.spriteDirection > 0 ? 10f : -17f;
            Vector2 eyeOrigin = projectile.Center + new Vector2(sneezeX, -12f);

            Vector2 toPlayer = player.Center - eyeOrigin;
            if (toPlayer != Vector2.Zero)
                toPlayer.Normalize();

            bool playerBehind = (projectile.spriteDirection == 1 && player.Center.X < projectile.Center.X)
                             || (projectile.spriteDirection == -1 && player.Center.X > projectile.Center.X);
            _eyeFreeFlipped = playerBehind;

            Vector2 targetOffset = new Vector2(
                MathHelper.Clamp(toPlayer.X * 2.5f, -1f, 1f),
                MathHelper.Clamp(toPlayer.Y * 1f, -1f, 1f));
            _eyeFreeOffset = Vector2.Lerp(_eyeFreeOffset, targetOffset, 0.45f);
        }

        // =====================================================================
        //  HELPERS
        // =====================================================================

        private static bool IsLoopingLegState(LegState state) =>
            state == LegState.Casual || state == LegState.Proper;

        private static bool IsLoopingArmState(ArmState state) =>
            state == ArmState.Down || state == ArmState.Up;

        /// <summary>
        /// Picks a random cycle duration: Short (4), Medium (6), or Long (7).
        /// Called independently for legs and arms when entering a looping state.
        /// </summary>
        private static int PickRandomDuration()
        {
            return Main.rand.Next(3) switch
            {
                0 => 4, // Short
                1 => 6, // Medium
                _ => 7  // Long
            };
        }

        /// <summary>
        /// Picks a random blink interval: 3 or 4 cycles of Looking before blinking.
        /// </summary>
        private static int PickBlinkDuration()
        {
            return Main.rand.NextBool() ? 3 : 4;
        }

        /// <summary>
        /// Picks how many blinks before swapping eye sheets: 2 or 3.
        /// </summary>
        private static int PickSwapTarget()
        {
            return Main.rand.NextBool() ? 2 : 3;
        }

        /// <summary>
        /// Converts a state enum value + local frame (0-3) into an absolute frame index
        /// on the 16-frame overlay sheet.
        /// </summary>
        private static int GetAbsoluteFrame(Enum state, int localFrame)
        {
            int stateIndex = Convert.ToInt32(state);
            return (stateIndex * FramesPerState) + Math.Clamp(localFrame, 0, FramesPerState - 1);
        }

        /// <summary>
        /// Draws the feet idle overlay with trail and glow effects identical to the
        /// normal SariaFeet (SariaMaindraw with Glowinthedark=true, DoesitTrail=true,
        /// HowlongisTrail=25). Uses 16-frame overlay sheet instead of 99-frame main sheet.
        /// </summary>
        private static void DrawFeetIdleOverlay(Projectile projectile, string texturePath, int frameIndex,
                                                 bool shouldFlip, Color lightColor, float yOffset)
        {
            Texture2D texture = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;
            int frameHeight = texture.Height / OverlayTotalFrames;
            int frameY = frameHeight * Math.Clamp(frameIndex, 0, OverlayTotalFrames - 1);
            Rectangle sourceRect = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;

            Vector2 drawPos = projectile.Center - Main.screenPosition
                              + new Vector2(0f, projectile.gfxOffY);
            drawPos.Y += yOffset;

            SpriteEffects effects = SpriteEffects.None;
            if (shouldFlip && projectile.spriteDirection == -1)
                effects = SpriteEffects.FlipHorizontally;

            // Glow-in-dark: always bright (GhostWhite), matching normal SariaFeet
            Color drawColor = Color.Lerp(lightColor, Color.GhostWhite, 20f);

            // --- Trail effect (identical to SariaMaindraw trail logic) ---
            if (!_feetTrailDecayTimers.ContainsKey(projectile.whoAmI))
                _feetTrailDecayTimers[projectile.whoAmI] = 0f;

            if (projectile.velocity.Length() > 0.1f)
                _feetTrailDecayTimers[projectile.whoAmI] = 30f;
            else if (_feetTrailDecayTimers[projectile.whoAmI] > 0f)
                _feetTrailDecayTimers[projectile.whoAmI]--;

            float currentTimer = _feetTrailDecayTimers[projectile.whoAmI];

            if (currentTimer > 0f)
            {
                float trailFadeFactor = MathHelper.Clamp(currentTimer / 30f, 0f, 1f);

                for (int i = 1; i < FeetTrailLength; i++)
                {
                    if (projectile.oldPos[i] == Vector2.Zero)
                        continue;

                    Vector2 currentPos = projectile.oldPos[i];
                    Vector2 previousPos = (i > 0) ? projectile.oldPos[i - 1] : projectile.Center;

                    if (previousPos == Vector2.Zero)
                        previousPos = currentPos;

                    int interpolationSteps = 3;
                    for (int t = 0; t <= interpolationSteps; t++)
                    {
                        float lerpAmount = (float)t / interpolationSteps;
                        Vector2 interpolatedPos = Vector2.Lerp(previousPos, currentPos, lerpAmount);
                        float completionRatio = ((float)i - 1 + lerpAmount) / (float)FeetTrailLength;

                        Vector2 trailPos = interpolatedPos + projectile.Size * 0.5f - Main.screenPosition;
                        trailPos.Y += yOffset;

                        float trailScale = scale * MathHelper.Lerp(1f, 0.3f, completionRatio);

                        Color trailColor = Color.Lerp(drawColor, Color.DeepPink, completionRatio);
                        trailColor = Color.Lerp(trailColor, Color.Transparent, completionRatio);
                        trailColor = Color.Lerp(Color.Transparent, trailColor, trailFadeFactor);

                        Main.spriteBatch.Draw(texture, trailPos, sourceRect,
                            projectile.GetAlpha(trailColor), rotation, origin,
                            trailScale, effects, 0f);
                    }
                }
            }

            // Clean up timer for inactive projectiles
            if (!projectile.active && _feetTrailDecayTimers.ContainsKey(projectile.whoAmI))
                _feetTrailDecayTimers.Remove(projectile.whoAmI);

            // Draw main sprite on top of trail
            Main.spriteBatch.Draw(texture, drawPos, sourceRect,
                projectile.GetAlpha(drawColor), rotation, origin,
                scale, effects, 0f);
        }

        /// <summary>
        /// Draws a pre-built texture (e.g. a generated mask) at the projectile's
        /// position using 16-frame overlay framing, with glow always enabled.
        /// </summary>
        private static void DrawOverlayDirect(Projectile projectile, Texture2D texture, int frameIndex,
                                              bool shouldFlip, Color lightColor, float yOffset)
        {
            int frameHeight = texture.Height / OverlayTotalFrames;
            int frameY = frameHeight * Math.Clamp(frameIndex, 0, OverlayTotalFrames - 1);
            Rectangle sourceRect = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;

            Vector2 drawPos = projectile.Center - Main.screenPosition
                              + new Vector2(0f, projectile.gfxOffY);
            drawPos.Y += yOffset;

            SpriteEffects effects = SpriteEffects.None;
            if (shouldFlip && projectile.spriteDirection == -1)
                effects = SpriteEffects.FlipHorizontally;

            Color drawColor = projectile.GetAlpha(Color.Lerp(lightColor, Color.GhostWhite, 20f));

            Main.spriteBatch.Draw(texture, drawPos, sourceRect,
                drawColor, projectile.rotation, origin,
                projectile.scale, effects, 0f);
        }

        /// <summary>
        /// Draws a pre-built mask texture with an electrical effect using
        /// 16-frame overlay framing. Pulsates via alpha3, adds shimmer
        /// copies for arcing, and uses an additive glow pass.
        /// </summary>
        private static void DrawOverlayElectric(Projectile projectile, Texture2D texture, int frameIndex,
                                                bool shouldFlip, Color lightColor, float yOffset)
        {
            SariaDrawingExtensions.UpdateAlphaCounters();

            float intensity = SariaExtensions1.electricIntensity;
            if (intensity <= 0f) return; // off phase — skip everything

            int frameHeight = texture.Height / OverlayTotalFrames;
            int frameY = frameHeight * Math.Clamp(frameIndex, 0, OverlayTotalFrames - 1);
            Rectangle sourceRect = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;

            Vector2 drawPos = projectile.Center - Main.screenPosition
                              + new Vector2(0f, projectile.gfxOffY);
            drawPos.Y += yOffset;

            SpriteEffects effects = SpriteEffects.None;
            if (shouldFlip && projectile.spriteDirection == -1)
                effects = SpriteEffects.FlipHorizontally;

            float electricPulse = MathHelper.Lerp(0.3f, 1f, 1f - SariaExtensions1.alpha3) * intensity;

            // --- 1) Pulsating base draw (scaled by cycle intensity) ---
            Color baseColor = projectile.GetAlpha(Color.Lerp(lightColor, Color.GhostWhite, 20f)) * electricPulse;
            Main.spriteBatch.Draw(texture, drawPos, sourceRect,
                baseColor, projectile.rotation, origin,
                projectile.scale, effects, 0f);

            // --- 2) Shimmer copies: electrical arcing ---
            // Jitter magnitude scales with intensity so arcs slow down as it fades.
            ulong randSeed = (Main.GameUpdateCount / 3) ^ (ulong)projectile.whoAmI;
            for (int c = 0; c < 3; c++)
            {
                float shakeX = Utils.RandomInt(ref randSeed, -8, 9) * 0.12f * intensity;
                float shakeY = Utils.RandomInt(ref randSeed, -8, 9) * 0.12f * intensity;
                Vector2 shimmerPos = drawPos + new Vector2(shakeX, shakeY);
                Color shimmerColor = projectile.GetAlpha(new Color(30, 90, 130, 0) * electricPulse);
                Main.spriteBatch.Draw(texture, shimmerPos, sourceRect,
                    shimmerColor, projectile.rotation, origin,
                    projectile.scale, effects, 0f);
            }

            // --- 3) Additive glow pass ---
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float glowIntensity = MathHelper.Lerp(0.1f, 0.35f, 1f - SariaExtensions1.alpha3) * intensity;
            Color glowColor = projectile.GetAlpha(
                Color.Lerp(Color.DeepSkyBlue, Color.Cyan, 1f - SariaExtensions1.alpha2) * glowIntensity);
            Main.spriteBatch.Draw(texture, drawPos, sourceRect,
                glowColor, projectile.rotation, origin,
                projectile.scale * 1.02f, effects, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws a pre-built texture with Form 5 glow-in/out effect using 16-frame
        /// overlay framing. Mirrors <c>Saria5GlowMaskdraw</c> but adapted for idle overlays.
        /// </summary>
        private static void DrawOverlay5Glow(Projectile projectile, Texture2D texture, int frameIndex,
                                             bool shouldFlip, Color lightColor, float yOffset,
                                             bool counter1, bool counter2)
        {
            int frameHeight = texture.Height / OverlayTotalFrames;
            int frameY = frameHeight * Math.Clamp(frameIndex, 0, OverlayTotalFrames - 1);
            Rectangle sourceRect = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;

            Vector2 drawPos = projectile.Center - Main.screenPosition
                              + new Vector2(0f, projectile.gfxOffY);
            drawPos.Y += yOffset;

            SpriteEffects effects = SpriteEffects.None;
            if (shouldFlip && projectile.spriteDirection == -1)
                effects = SpriteEffects.FlipHorizontally;

            Lighting.AddLight(projectile.Center, Color.DeepPink.ToVector3() * (1f - SariaDrawingExtensions.alpha1));
            Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * (1f - SariaDrawingExtensions.alpha2));

            Color drawColor = Color.Lerp(lightColor, Color.FloralWhite, 30f);
            if (counter1)
            {
                drawColor = Color.Lerp(drawColor, Color.Transparent, SariaDrawingExtensions.alpha1);
                projectile.RockDustOnVisiblePixels(texture, ModContent.DustType<Dusts.RockSparkle>(), 20,
                    OverlayTotalFrames, Math.Clamp(frameIndex, 0, OverlayTotalFrames - 1), shouldFlip);
            }
            if (counter2)
                drawColor = Color.Lerp(drawColor, Color.Transparent, SariaDrawingExtensions.alpha2);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, drawPos, sourceRect,
                drawColor, projectile.rotation, origin,
                projectile.scale, effects, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// absolute frame index. Uses its own 16-frame count, independent of the
        /// main projectile's 99-frame sheet.
        /// </summary>
        private static void DrawOverlay(Projectile projectile, string texturePath, int frameIndex,
                                         bool shouldFlip, Color lightColor, float yOffset,
                                         bool glowInDark = false)
        {
            Texture2D texture = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;
            int frameHeight = texture.Height / OverlayTotalFrames;
            int frameY = frameHeight * Math.Clamp(frameIndex, 0, OverlayTotalFrames - 1);
            Rectangle sourceRect = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;

            Vector2 drawPos = projectile.Center - Main.screenPosition
                              + new Vector2(0f, projectile.gfxOffY);
            drawPos.Y += yOffset;

            SpriteEffects effects = SpriteEffects.None;
            if (shouldFlip && projectile.spriteDirection == -1)
                effects = SpriteEffects.FlipHorizontally;

            // Glow-in-dark matches normal SariaFeet: always bright (GhostWhite)
            Color drawColor = glowInDark
                ? projectile.GetAlpha(Color.Lerp(lightColor, Color.GhostWhite, 20f))
                : projectile.GetAlpha(lightColor);

            Main.spriteBatch.Draw(texture, drawPos, sourceRect,
                drawColor, projectile.rotation, origin,
                projectile.scale, effects, 0f);
        }

        /// <summary>
        /// Draws the mouth underlay BELOW all face and eye layers.
        /// Call from Saria.cs PostDraw BEFORE SariaSmallFacesOrWhencursed and DrawIdleEyes,
        /// only when the torso is in idle range (0-3) and Eating &lt;= 0.
        /// Visible for Happy, Sad, and Angry moods. Never appears for ghost form (Transform 6).
        /// Does NOT shift with free-roam eyes. Form 6 (bug) is included.
        /// Processed through SariaFaceColorKey for per-form color swaps.
        /// Uses the same 12-frame eye sheet framing as DrawIdleEyes.
        /// </summary>
        public void DrawMouthIdleUnderlay(Projectile projectile, int transform, int mood, bool cursed, Color lightColor)
        {
            // Ghost form: no mouth underlay
            if (transform == 6) return;

            // Only visible for happy, sad, or angry moods
            Player player = Main.player[projectile.owner];
            bool isHappy = _displayedMood == (int)MoodState.Happy;
            bool isSad = _displayedMood == (int)MoodState.Sad
                         || player.HasBuff(ModContent.BuffType<Buffs.Extinguished>());
            bool isAngry = _displayedMood == (int)MoodState.Angry;
            if (!isHappy && !isSad && !isAngry) return;

            if (!ModContent.HasAsset(MouthIdleUnderlayPath)) return;

            int localFrame = projectile.frame % FramesPerState;
            int eyeFrame = GetAbsoluteFrame(CurrentEyeState, localFrame);

            Texture2D originalTex = ModContent.Request<Texture2D>(MouthIdleUnderlayPath, AssetRequestMode.ImmediateLoad).Value;
            Texture2D tex = SariaFaceColorKey.GetProcessedFace(originalTex, transform, skipTransparentDest: true);

            // No free-eye offset — mouth does not move with eye tracking
            DrawEyeOverlay(projectile, tex, eyeFrame, true, lightColor, 1f,
                Vector2.Zero, false, pointSample: true);
        }

        /// <summary>
        /// Draws the idle eye overlay on top of the face layer.
        /// Call from Saria.cs PostDraw AFTER SariaSmallFacesOrWhencursed,
        /// only when the torso is in idle range (0-3) and Eating &lt;= 0.
        /// Draws for the normal face (default mood range, not cursed).
        /// Form 7 (Transform 6) uses its own idle face paths (7SariaNormalFaceIdle / Back).
        /// The 12-frame sheet has 3 states × 4 frames:
        ///   0-3 Looking, 4-7 Blinking, 8-11 Opening.
        /// Processed through SariaFaceColorKey for per-form color swaps.
        /// Form 5 (Transform 4) also gets a pulsing pink glow overlay.
        /// </summary>
        public void DrawIdleEyes(Projectile projectile, int transform, int mood, bool cursed, Color lightColor)
        {
            // Only draw for non-cursed, normal mood.
            if (cursed) return;

            // Normal mood range: not happy, sad, or angry
            Player player = Main.player[projectile.owner];
            bool isHappy = _displayedMood == (int)MoodState.Happy;
            bool isSad = _displayedMood == (int)MoodState.Sad
                          || player.HasBuff(ModContent.BuffType<Buffs.Extinguished>());
            bool isAngry = _displayedMood == (int)MoodState.Angry;
            if (isHappy || isSad || isAngry) return;

            int localFrame = projectile.frame % FramesPerState;

            // Form 7 ghost: 4-frame flat loop, row-wave background + offset pupils, early return
            if (transform == 6)
            {
                int eyeFrame7 = localFrame;
                Vector2 eyeOffset7 = _eyeFreeOffset + new Vector2(0f, 1f);
                Texture2D bg7 = ModContent.Request<Texture2D>(Form7EyeBackgroundPath, AssetRequestMode.ImmediateLoad).Value;
                DrawForm7EyeRowWave(projectile, bg7, eyeFrame7, 4, _form7WavePhase, _form7EyeAlpha, new Vector2(0f, 1f), true, lightColor);
                Texture2D pupil7Raw = ModContent.Request<Texture2D>(Form7FaceIdlePath, AssetRequestMode.ImmediateLoad).Value;
                Texture2D pupil7 = SariaFaceColorKey.GetProcessedFace(pupil7Raw, transform, skipTransparentDest: true);
                DrawForm7EyeRowWave(projectile, pupil7, eyeFrame7, 4, _form7WavePhase, _form7EyeAlpha, eyeOffset7, true, lightColor);
                SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, bg7, eyeFrame7, new Vector2(0f, 1f), transform, totalFrames: 4);
                SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, pupil7Raw, eyeFrame7, eyeOffset7, transform, totalFrames: 4);
                return;
            }

            int eyeFrame = GetAbsoluteFrame(CurrentEyeState, localFrame);

            string eyePath;
            if (transform == 5)
                eyePath = (_eyeLookingBack && !_eyeFreeMode) ? Form6FaceIdleBackPath : Form6FaceIdlePath;
            else
                eyePath = (_eyeLookingBack && !_eyeFreeMode) ? NormalFaceIdleBackPath : NormalFaceIdlePath;
            Texture2D originalTex = ModContent.Request<Texture2D>(eyePath, AssetRequestMode.ImmediateLoad).Value;
            Texture2D tex = SariaFaceColorKey.GetProcessedFace(originalTex, transform, skipTransparentDest: true);

            Vector2 eyeOffset = _eyeFreeMode ? _eyeFreeOffset : Vector2.Zero;

            // Form 6: draw static insect eye shell (whites/frame) first, then pupils on top
            Texture2D form6BgTex = null;
            if (transform == 5)
            {
                form6BgTex = ModContent.Request<Texture2D>(Form6EyeBackgroundPath, AssetRequestMode.ImmediateLoad).Value;
                DrawEyeOverlay(projectile, form6BgTex, eyeFrame, true, lightColor, 1f,
                    Vector2.Zero, false, pointSample: true);
            }

            // Pupils drawn on top of shell so they show through the eye openings
            DrawEyeOverlay(projectile, tex, eyeFrame, true, lightColor, 1f,
                eyeOffset,
                false, pointSample: true);

            // Form 5 pulsing glow overlay (matches SariaEyesGlowandFadedraw behavior)
            if (transform == 4)
            {
                Texture2D glowTex = SariaFaceColorKey.GetForm5GlowFace(originalTex);
                if (glowTex != null && glowTex != originalTex)
                {
                    float a3 = SariaDrawingExtensions.alpha3;
                    Lighting.AddLight(projectile.Center, Color.DeepPink.ToVector3() * (1f - a3));
                    Color glowColor = Color.Lerp(Color.White, Color.Transparent, a3);

                    int glowFrameH = glowTex.Height / EyeOverlayTotalFrames;
                    int glowFrameY = glowFrameH * Math.Clamp(eyeFrame, 0, EyeOverlayTotalFrames - 1);
                    Rectangle glowRect = new Rectangle(0, glowFrameY, glowTex.Width, glowFrameH);
                    Vector2 glowOrigin = glowRect.Size() / 2f;

                    Vector2 glowPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
                    glowPos.Y += 1f;
                    glowPos += eyeOffset;

                    SpriteEffects glowEffects = projectile.spriteDirection == -1
                        ? SpriteEffects.FlipHorizontally
                        : SpriteEffects.None;

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    Main.spriteBatch.Draw(glowTex, glowPos, glowRect, glowColor,
                        projectile.rotation, glowOrigin, projectile.scale, glowEffects, 0f);
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // Psychic pink eye overlay on top of idle eyes.
            // Form 6: shell psychic first, then pupils psychic on top — matches draw order above.
            if (form6BgTex != null)
                SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, form6BgTex, eyeFrame, Vector2.Zero, transform);
            SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, originalTex, eyeFrame, eyeOffset, transform);
        }

        /// <summary>
        /// Draws the happy idle eye overlay on top of the face layer.
        /// Call from Saria.cs PostDraw AFTER SariaSmallFacesOrWhencursed and DrawMouthIdleUnderlay,
        /// only when the torso is in idle range (0-3) and Eating &lt;= 0.
        /// Active for mood &gt;= 2400 (happy and formerly-pumped range). Forms 1-5 only (transform 0-4).
        /// Uses SariaHappyFaceIdle / SariaHappyFaceIdleBack sheets. Follows the same looking-back,
        /// free-eye, Form 5 glow, and psychic overlay rules as DrawIdleEyes.
        /// </summary>
        public void DrawHappyIdleEyes(Projectile projectile, int transform, int mood, bool cursed, Color lightColor)
        {
            if (cursed) return;

            // Forms 0-6 (includes Form 6 insect and Form 7 ghost)
            if (transform < 0 || transform > 6) return;

            // Only happy mood
            if (_displayedMood != (int)MoodState.Happy) return;

            int localFrame = projectile.frame % FramesPerState;

            // Form 7 (ghost): 4-frame flat loop, row-wave background + offset pupils, early return
            if (transform == 6)
            {
                int eyeFrame7 = localFrame;
                Vector2 eyeOffset7 = _eyeFreeOffset + new Vector2(0f, 1f);
                Texture2D bg7 = ModContent.Request<Texture2D>(Form7HappyEyeBackgroundPath, AssetRequestMode.ImmediateLoad).Value;
                DrawForm7EyeRowWave(projectile, bg7, eyeFrame7, 4, _form7WavePhase, _form7EyeAlpha, new Vector2(0f, 1f), true, lightColor);
                Texture2D pupil7Raw = ModContent.Request<Texture2D>(Form7HappyFaceIdlePath, AssetRequestMode.ImmediateLoad).Value;
                Texture2D pupil7 = SariaFaceColorKey.GetProcessedFace(pupil7Raw, transform, skipTransparentDest: true);
                DrawForm7EyeRowWave(projectile, pupil7, eyeFrame7, 4, _form7WavePhase, _form7EyeAlpha, eyeOffset7, true, lightColor);
                SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, bg7, eyeFrame7, new Vector2(0f, 1f), transform, totalFrames: 4);
                SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, pupil7Raw, eyeFrame7, eyeOffset7, transform, totalFrames: 4);
                return;
            }

            int eyeFrame = GetAbsoluteFrame(CurrentEyeState, localFrame);

            string eyePath = transform == 5
                ? ((_eyeLookingBack && !_eyeFreeMode) ? Form6HappyFaceIdleBackPath : Form6HappyFaceIdlePath)
                : ((_eyeLookingBack && !_eyeFreeMode) ? HappyFaceIdleBackPath : HappyFaceIdlePath);
            Texture2D originalTex = ModContent.Request<Texture2D>(eyePath, AssetRequestMode.ImmediateLoad).Value;
            Texture2D tex = SariaFaceColorKey.GetProcessedFace(originalTex, transform, skipTransparentDest: true);

            Vector2 eyeOffset = _eyeFreeMode ? _eyeFreeOffset : Vector2.Zero;

            // Form 6: draw static insect eye shell first, then pupils on top
            Texture2D form6BgTex = null;
            if (transform == 5)
            {
                form6BgTex = ModContent.Request<Texture2D>(Form6EyeBackgroundPath, AssetRequestMode.ImmediateLoad).Value;
                DrawEyeOverlay(projectile, form6BgTex, eyeFrame, true, lightColor, 1f,
                    Vector2.Zero, false, pointSample: true);
            }

            DrawEyeOverlay(projectile, tex, eyeFrame, true, lightColor, 1f,
                eyeOffset, false, pointSample: true);

            // Form 5 pulsing glow overlay
            if (transform == 4)
            {
                Texture2D glowTex = SariaFaceColorKey.GetForm5GlowFace(originalTex);
                if (glowTex != null && glowTex != originalTex)
                {
                    float a3 = SariaDrawingExtensions.alpha3;
                    Lighting.AddLight(projectile.Center, Color.DeepPink.ToVector3() * (1f - a3));
                    Color glowColor = Color.Lerp(Color.White, Color.Transparent, a3);

                    int glowFrameH = glowTex.Height / EyeOverlayTotalFrames;
                    int glowFrameY = glowFrameH * Math.Clamp(eyeFrame, 0, EyeOverlayTotalFrames - 1);
                    Rectangle glowRect = new Rectangle(0, glowFrameY, glowTex.Width, glowFrameH);
                    Vector2 glowOrigin = glowRect.Size() / 2f;

                    Vector2 glowPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
                    glowPos.Y += 1f;
                    glowPos += eyeOffset;

                    SpriteEffects glowEffects = projectile.spriteDirection == -1
                        ? SpriteEffects.FlipHorizontally
                        : SpriteEffects.None;

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    Main.spriteBatch.Draw(glowTex, glowPos, glowRect, glowColor,
                        projectile.rotation, glowOrigin, projectile.scale, glowEffects, 0f);
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // Psychic pink eye overlay on top (Form 6: shell first, then pupils)
            if (form6BgTex != null)
                SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, form6BgTex, eyeFrame, Vector2.Zero, transform);
            SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, originalTex, eyeFrame, eyeOffset, transform);
        }

        /// <summary>
        /// Draws the sad idle eye overlay
        /// Call from Saria.cs PostDraw AFTER SariaSmallFacesOrWhencursed and DrawMouthIdleUnderlay,
        /// only when the torso is in idle range (0-3) and Eating &lt;= 0.
        /// Active for sad mood range. Forms 1-5 only (transform 0-4).
        /// Uses SariaSadFaceIdle / SariaSadFaceIdleBack sheets. Follows the same looking-back,
        /// free-eye, Form 5 glow, and psychic overlay rules as DrawIdleEyes.
        /// </summary>
        public void DrawSadIdleEyes(Projectile projectile, int transform, int mood, bool cursed, Color lightColor)
        {
            if (cursed) return;

            // Forms 0-6 (includes Form 6 insect and Form 7 ghost)
            if (transform < 0 || transform > 6) return;

            // Only sad mood
            Player player = Main.player[projectile.owner];
            bool isSad = _displayedMood == (int)MoodState.Sad
                         || player.HasBuff(ModContent.BuffType<Buffs.Extinguished>());
            if (!isSad) return;

            int localFrame = projectile.frame % FramesPerState;

            // Form 7 (ghost): 4-frame flat loop, row-wave background + offset pupils, early return
            if (transform == 6)
            {
                int eyeFrame7 = localFrame;
                Vector2 eyeOffset7 = _eyeFreeOffset + new Vector2(0f, 1f);
                Texture2D bg7 = ModContent.Request<Texture2D>(Form7SadEyeBackgroundPath, AssetRequestMode.ImmediateLoad).Value;
                DrawForm7EyeRowWave(projectile, bg7, eyeFrame7, 4, _form7WavePhase, _form7EyeAlpha, new Vector2(0f, 1f), true, lightColor);
                Texture2D pupil7Raw = ModContent.Request<Texture2D>(Form7SadFaceIdlePath, AssetRequestMode.ImmediateLoad).Value;
                Texture2D pupil7 = SariaFaceColorKey.GetProcessedFace(pupil7Raw, transform, skipTransparentDest: true);
                DrawForm7EyeRowWave(projectile, pupil7, eyeFrame7, 4, _form7WavePhase, _form7EyeAlpha, eyeOffset7, true, lightColor);
                SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, bg7, eyeFrame7, new Vector2(0f, 1f), transform, totalFrames: 4);
                SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, pupil7Raw, eyeFrame7, eyeOffset7, transform, totalFrames: 4);
                return;
            }

            int eyeFrame = GetAbsoluteFrame(CurrentEyeState, localFrame);

            string eyePath = transform == 5
                ? ((_eyeLookingBack && !_eyeFreeMode) ? Form6SadFaceIdleBackPath : Form6SadFaceIdlePath)
                : ((_eyeLookingBack && !_eyeFreeMode) ? SadFaceIdleBackPath : SadFaceIdlePath);
            Texture2D originalTex = ModContent.Request<Texture2D>(eyePath, AssetRequestMode.ImmediateLoad).Value;
            Texture2D tex = SariaFaceColorKey.GetProcessedFace(originalTex, transform, skipTransparentDest: true);

            Vector2 eyeOffset = _eyeFreeMode ? _eyeFreeOffset : Vector2.Zero;

            // Form 6: draw static insect eye shell first, then pupils on top
            Texture2D form6BgTex = null;
            if (transform == 5)
            {
                form6BgTex = ModContent.Request<Texture2D>(Form6EyeBackgroundPath, AssetRequestMode.ImmediateLoad).Value;
                DrawEyeOverlay(projectile, form6BgTex, eyeFrame, true, lightColor, 1f,
                    Vector2.Zero, false, pointSample: true);
            }

            DrawEyeOverlay(projectile, tex, eyeFrame, true, lightColor, 1f,
                eyeOffset, false, pointSample: true);

            // Form 5 pulsing glow overlay
            if (transform == 4)
            {
                Texture2D glowTex = SariaFaceColorKey.GetForm5GlowFace(originalTex);
                if (glowTex != null && glowTex != originalTex)
                {
                    float a3 = SariaDrawingExtensions.alpha3;
                    Lighting.AddLight(projectile.Center, Color.DeepPink.ToVector3() * (1f - a3));
                    Color glowColor = Color.Lerp(Color.White, Color.Transparent, a3);

                    int glowFrameH = glowTex.Height / EyeOverlayTotalFrames;
                    int glowFrameY = glowFrameH * Math.Clamp(eyeFrame, 0, EyeOverlayTotalFrames - 1);
                    Rectangle glowRect = new Rectangle(0, glowFrameY, glowTex.Width, glowFrameH);
                    Vector2 glowOrigin = glowRect.Size() / 2f;

                    Vector2 glowPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
                    glowPos.Y += 1f;
                    glowPos += eyeOffset;

                    SpriteEffects glowEffects = projectile.spriteDirection == -1
                        ? SpriteEffects.FlipHorizontally
                        : SpriteEffects.None;

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    Main.spriteBatch.Draw(glowTex, glowPos, glowRect, glowColor,
                        projectile.rotation, glowOrigin, projectile.scale, glowEffects, 0f);
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // Psychic pink eye overlay on top (Form 6: shell first, then pupils)
            if (form6BgTex != null)
                SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, form6BgTex, eyeFrame, Vector2.Zero, transform);
            SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, originalTex, eyeFrame, eyeOffset, transform);
        }

        /// <summary>
        /// Draws the angry face underlay
        /// Call from Saria.cs PostDraw BEFORE SariaSmallFacesOrWhencursed, only during idle.
        /// Forms 1-5 only (transform 0-4). Color-swapped via SariaFaceColorKey. Never moves.
        /// </summary>
        public void DrawAngryIdleUnderlay(Projectile projectile, int transform, int mood, bool cursed, Color lightColor)
        {
            if (cursed) return;
            if (transform < 0 || transform > 4) return;

            Player player = Main.player[projectile.owner];
            bool isAngry = _displayedMood == (int)MoodState.Angry;
            if (!isAngry) return;

            if (!ModContent.HasAsset(AngryFaceIdleUnderlayPath)) return;

            int localFrame = projectile.frame % FramesPerState;
            int eyeFrame = GetAbsoluteFrame(CurrentEyeState, localFrame);

            Texture2D originalTex = ModContent.Request<Texture2D>(AngryFaceIdleUnderlayPath, AssetRequestMode.ImmediateLoad).Value;
            Texture2D tex = SariaFaceColorKey.GetProcessedFace(originalTex, transform, skipTransparentDest: true);

            DrawEyeOverlay(projectile, tex, eyeFrame, true, lightColor, 1f,
                Vector2.Zero, false, pointSample: true);
        }

        /// <summary>
        /// Draws the angry idle pupils and overlay on top of the face base layer.
        /// Call from Saria.cs PostDraw AFTER SariaSmallFacesOrWhencursed, only during idle.
        /// Pupils (SariaAngryFaceIdle): forms 1-5, color-swapped, moves with free-roam eyes.
        ///   No back sheet — looking-back shifts pupils 1px opposite to spriteDirection (ghost-style).
        /// Overlay (SariaAngryFaceIdleOverlay): all 7 forms, no color swap, never moves.
        /// Form 5 pulsing glow and psychic overlay applied to pupils.
        /// </summary>
        public void DrawAngryIdleEyes(Projectile projectile, int transform, int mood, bool cursed, Color lightColor)
        {
            if (cursed) return;

            Player player = Main.player[projectile.owner];
            bool isAngry = _displayedMood == (int)MoodState.Angry;
            if (!isAngry) return;

            int localFrame = projectile.frame % FramesPerState;
            int eyeFrame = GetAbsoluteFrame(CurrentEyeState, localFrame);

            // --- Pupils (forms 0-6) ---
            if (transform >= 0 && transform <= 6)
            {
                // Form 7 (ghost): 4-frame flat loop, row-wave background + offset pupils, early return
                if (transform == 6)
                {
                    int eyeFrame7 = localFrame;
                    Vector2 eyeOffset7 = _eyeFreeOffset + new Vector2(0f, 1f);
                    Texture2D bg7 = ModContent.Request<Texture2D>(Form7AngryEyeBackgroundPath, AssetRequestMode.ImmediateLoad).Value;
                    DrawForm7EyeRowWave(projectile, bg7, eyeFrame7, 4, _form7WavePhase, _form7EyeAlpha, new Vector2(0f, 1f), true, lightColor);
                    Texture2D pupil7Raw = ModContent.Request<Texture2D>(Form7AngryFaceIdlePath, AssetRequestMode.ImmediateLoad).Value;
                    Texture2D pupil7 = SariaFaceColorKey.GetProcessedFace(pupil7Raw, transform, skipTransparentDest: true);
                    DrawForm7EyeRowWave(projectile, pupil7, eyeFrame7, 4, _form7WavePhase, _form7EyeAlpha, eyeOffset7, true, lightColor);
                    SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, bg7, eyeFrame7, new Vector2(0f, 1f), transform, totalFrames: 4);
                    SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, pupil7Raw, eyeFrame7, eyeOffset7, transform, totalFrames: 4);
                    return;
                }

                    // Forms 0-5 only: standard pupil draw (Form 7 already handled above)
                    if (transform != 6)
                    {
                        string pupilPath;
                        Vector2 eyeOffset;

                        if (transform == 5)
                        {
                            // Form 6: use back sheet instead of pixel shift, same as normal/happy/sad
                            pupilPath = (_eyeLookingBack && !_eyeFreeMode) ? Form6AngryFaceIdleBackPath : Form6AngryFaceIdlePath;
                            eyeOffset = _eyeFreeMode ? _eyeFreeOffset : Vector2.Zero;
                        }
                        else
                        {
                            // Forms 0-4: no back sheet, looking-back shifts pupils 1px
                            pupilPath = AngryFaceIdlePath;
                            if (_eyeFreeMode)
                                eyeOffset = _eyeFreeOffset;
                            else if (_eyeLookingBack)
                                eyeOffset = new Vector2(projectile.spriteDirection == 1 ? -1f : 1f, 0f);
                            else
                                eyeOffset = Vector2.Zero;
                        }

                        // Form 6: draw static insect eye shell first, then pupils on top
                        Texture2D form6BgTex = null;
                        if (transform == 5)
                        {
                            form6BgTex = ModContent.Request<Texture2D>(Form6EyeBackgroundPath, AssetRequestMode.ImmediateLoad).Value;
                            DrawEyeOverlay(projectile, form6BgTex, eyeFrame, true, lightColor, 1f,
                                Vector2.Zero, false, pointSample: true);
                        }

                        Texture2D pupilOriginal = ModContent.Request<Texture2D>(pupilPath, AssetRequestMode.ImmediateLoad).Value;
                        Texture2D pupilTex = SariaFaceColorKey.GetProcessedFace(pupilOriginal, transform, skipTransparentDest: true);

                        DrawEyeOverlay(projectile, pupilTex, eyeFrame, true, lightColor, 1f,
                            eyeOffset, false, pointSample: true);

                        // Form 5 pulsing glow overlay
                        if (transform == 4)
                        {
                            Texture2D glowTex = SariaFaceColorKey.GetForm5GlowFace(pupilOriginal);
                            if (glowTex != null && glowTex != pupilOriginal)
                            {
                                float a3 = SariaDrawingExtensions.alpha3;
                                Lighting.AddLight(projectile.Center, Color.DeepPink.ToVector3() * (1f - a3));
                                Color glowColor = Color.Lerp(Color.White, Color.Transparent, a3);

                                int glowFrameH = glowTex.Height / EyeOverlayTotalFrames;
                                int glowFrameY = glowFrameH * Math.Clamp(eyeFrame, 0, EyeOverlayTotalFrames - 1);
                                Rectangle glowRect = new Rectangle(0, glowFrameY, glowTex.Width, glowFrameH);
                                Vector2 glowOrigin = glowRect.Size() / 2f;

                                Vector2 glowPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
                                glowPos.Y += 1f;
                                glowPos += eyeOffset;

                                SpriteEffects glowEffects = projectile.spriteDirection == -1
                                    ? SpriteEffects.FlipHorizontally
                                    : SpriteEffects.None;

                                Main.spriteBatch.End();
                                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                                Main.spriteBatch.Draw(glowTex, glowPos, glowRect, glowColor,
                                    projectile.rotation, glowOrigin, projectile.scale, glowEffects, 0f);
                                Main.spriteBatch.End();
                                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                            }
                        }

                        // Psychic pink eye overlay on top of pupils (Form 6: shell first, then pupils)
                        if (form6BgTex != null)
                            SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, form6BgTex, eyeFrame, Vector2.Zero, transform);
                        SariaPsychicEyes.DrawPsychicIdleEyeOverlay(projectile, pupilOriginal, eyeFrame, eyeOffset, transform);
                    }
            }

            // --- Overlay: all 7 forms, no color swap, no movement ---
            if (ModContent.HasAsset(AngryFaceIdleOverlayPath))
            {
                Texture2D overlayTex = ModContent.Request<Texture2D>(AngryFaceIdleOverlayPath, AssetRequestMode.ImmediateLoad).Value;
                DrawEyeOverlay(projectile, overlayTex, eyeFrame, true, lightColor, 1f,
                    Vector2.Zero, false, pointSample: true);
            }
        }

        /// <summary>
        /// Draws an eye overlay frame from a 12-frame sheet (3 states × 4 frames).
        /// Same positioning logic as DrawOverlay but uses EyeOverlayTotalFrames.
        /// </summary>
        /// <summary>
        /// Draws a Form 7 ghost eye texture with a per-row sine-wave alpha ripple.
        /// The wave moves through the pixels vertically (like the dialogue UI poe effect).
        /// overallAlpha multiplies every row's alpha, enabling a smooth fade-in/out.
        /// No horizontal jitter — eyes stay exactly in position.
        /// </summary>
        public static void DrawForm7EyeRowWave(
            Projectile projectile, Texture2D texture,
            int frameIndex, int totalFrames,
            float wavePhase, float overallAlpha,
            Vector2 extraOffset, bool shouldFlip, Color lightColor)
        {
            if (overallAlpha <= 0f) return;

            int frameHeight = texture.Height / totalFrames;
            int frameY = frameHeight * Math.Clamp(frameIndex, 0, totalFrames - 1);

            // Same anchor as DrawEyeOverlay: center of projectile + gfxOffY + extraOffset
            Vector2 center = projectile.Center - Main.screenPosition
                             + new Vector2(0f, projectile.gfxOffY)
                             + extraOffset;

            bool doFlip = shouldFlip && projectile.spriteDirection == -1;
            SpriteEffects effects = doFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Color baseColor = projectile.GetAlpha(Color.Lerp(lightColor, Color.GhostWhite, 20f));

            // Top-left of the frame (origin is centre, same as DrawEyeOverlay)
            float topLeftX = center.X - (texture.Width  * 0.5f) * projectile.scale;
            float topLeftY = center.Y - (frameHeight     * 0.5f) * projectile.scale;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            for (int row = 0; row < frameHeight; row++)
            {
                float t = 1f - (float)row / frameHeight; // 1 at top, 0 at bottom
                float wave    = MathF.Sin(wavePhase + t * MathF.PI * 5f);
                float rowAlpha = (1f - 0.45f * (wave * 0.5f + 0.5f)) * overallAlpha;

                Rectangle rowRect = new Rectangle(0, frameY + row, texture.Width, 1);
                Vector2   rowPos  = new Vector2(topLeftX, topLeftY + row * projectile.scale);
                Color     rowColor = baseColor * rowAlpha;

                Main.spriteBatch.Draw(texture, rowPos, rowRect, rowColor,
                    0f, Vector2.Zero, projectile.scale, effects, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private static void DrawEyeOverlay(Projectile projectile, Texture2D texture, int frameIndex,
                                            bool shouldFlip, Color lightColor, float yOffset,
                                            Vector2 extraOffset = default, bool invertFlip = false, bool pointSample = false,
                                            int totalFrames = EyeOverlayTotalFrames)
        {
            int frameHeight = texture.Height / totalFrames;
            int frameY = frameHeight * Math.Clamp(frameIndex, 0, totalFrames - 1);
            Rectangle sourceRect = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;

            Vector2 drawPos = projectile.Center - Main.screenPosition
                              + new Vector2(0f, projectile.gfxOffY);
            drawPos.Y += yOffset;
            drawPos += extraOffset;

            bool doFlip = shouldFlip && projectile.spriteDirection == -1;
            if (invertFlip) doFlip = !doFlip;
            SpriteEffects effects = doFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Color drawColor = projectile.GetAlpha(Color.Lerp(lightColor, Color.GhostWhite, 20f));

            if (pointSample)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Main.spriteBatch.Draw(texture, drawPos, sourceRect,
                drawColor, projectile.rotation, origin,
                projectile.scale, effects, 0f);
            if (pointSample)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
    }
}
