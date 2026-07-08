using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using SariaMod.Items.Strange;

namespace SariaMod.Items.zTalking
{
    /// <summary>
    /// ============================================================
    /// SARIA DIALOGUE UI STATE
    /// ============================================================
    /// Features:
    /// - Draggable UI (click and drag anywhere except buttons)
    /// - Position saved between dialogue sessions
    /// - Auto-repositions if buttons go off screen
    /// - Cutscene system (priority dialogue)
    /// - [silent] tag for non-speaking words
    /// - Mouth syncs to text speed
    /// ============================================================
    /// </summary>
    public partial class DialogueUIState : UIState
    {
        // ============================================================
        // TEXT AREA - EDIT THESE TO CHANGE WHERE TEXT APPEARS
        // ============================================================
        // // TextOffset: Where the text box starts (relative to panel center)
        // //   Positive X = right, Negative X = left
        // //   Positive Y = down, Negative Y = up
        // // TextMaxWidth: How wide the text can be before wrapping
        // // TextScale: Size of the text (0.8 = 80% of normal)
        // // LineHeightBase: Vertical spacing between lines
        internal static readonly Vector2 TextOffset = new Vector2(-75, -40); // ADJUSTED - moved down from -64
        internal const float TextMaxWidth = 235f;
        internal const float TextScale = 0.8f;
        internal const float LineHeightBase = 16f;
        // ============================================================

        // ============================================================
        // POSITION CALIBRATION - Panel and portrait positions
        // ============================================================
        // Background texture is 402x145, center is at (201, 72.5)
        // All offsets are relative to the background center
        // Positive X = right, Negative X = left
        // Positive Y = down, Negative Y = up
        // ============================================================
        
        private static readonly Vector2 DefaultPanelOffset = new Vector2(0, 176);
        
        // Background offset - aligns the background image with the logical panel center
        // Set to (0,0) so background center IS the panel center
        internal static readonly Vector2 BackgroundOffset = new Vector2(0, 0);

        // Portrait positions (relative to background center)
        // Eyes texture is 46x22 per frame, Mouth is 14x5 per frame
        // Portrait cutout on left side of background, approximately at X = -140 from center
        internal static readonly Vector2 EyesOffset = new Vector2(-143, -30);
        internal static readonly Vector2 Eyes7Offset = new Vector2(-147, -12); // Form 7 eyes — different size, adjust as needed
        internal static readonly Vector2 MouthOffset = new Vector2(-143, -2);

        // Greeting portrait offset (relative to background center, scales with UI)
        internal static readonly Vector2 GreetingPortraitOffset = new Vector2(-155, -21);

        // Sparks portrait overlay position (relative to background center, form 4 electric sparks)
        internal static readonly Vector2 SparksPortraitOffset = new Vector2(-151, -7);

        // Button positions (relative to background center)
        // Exit button (32x35): far left bottom
        // Back button (60x35): next to exit
        // SmallChoice buttons (102x47): three buttons across the bottom right area
        internal static readonly Vector2 ExitButtonOffset = new Vector2(-181, 43);
        internal static readonly Vector2 BackButtonOffset = new Vector2(-135, 43);
        internal static readonly Vector2 Button1Offset = new Vector2(-54, 49);
        internal static readonly Vector2 Button2Offset = new Vector2(48, 49);
        internal static readonly Vector2 Button3Offset = new Vector2(150, 49);

        // // Button sizes for hitbox detection (slightly smaller than visual)
        internal static readonly Vector2 SmallButtonSize = new Vector2(30, 32);
        internal static readonly Vector2 CustomButtonSize = new Vector2(98, 44);

        // // Button label settings
        private const float ButtonLabelMaxWidth = 80f;
        private const float ButtonLabelScale = 0.65f;
        private const float ButtonLabelMinScale = 0.45f;
        private const float ButtonLineHeight = 12f;

        // ============================================================
        // DRAGGING & POSITION PERSISTENCE
        // ============================================================
        private static Vector2 _savedPanelPosition = Vector2.Zero;
        private static bool _hasCustomPosition = false;

        private Vector2 _currentPanelOffset;
        private bool _isDragging = false;
        private Vector2 _dragStartMouse;
        private Vector2 _dragStartPanel;

        // // Panel size for drag detection and boundary checking
        internal const float PanelWidth = 380f;
        internal const float PanelHeight = 180f;

        // ============================================================
        // OPEN / CLOSE ANIMATION
        // ============================================================
        private enum PanelAnimState { Idle, Opening, Closing }
        private PanelAnimState _panelAnimState = PanelAnimState.Idle;
        private int  _panelAnimTimer  = 0;

        // Duration (frames) for each animation
        private const int OpenAnimFrames  = 13;
        private const int CloseAnimFrames = 10;

        // Visual-only offsets (never written to _currentPanelOffset / saved position)
        // Open:  slides from (-OpenSlideX, 0) → (0, 0),  alpha 0 → 1
        // Close: slides from (0, 0)           → (0, +CloseSlideY), alpha 1 → 0
        private const float OpenSlideX  = 30f;   // pixels (unscaled) to the left at start
        private const float CloseSlideY = 20f;   // pixels (unscaled) downward at end

        // Current per-frame visual delta (applied in Draw, never persisted)
        private Vector2 _animVisualOffset = Vector2.Zero;
        private float   _animAlpha        = 1f;

        // When true the UI is fully animated in and interactive
        public bool IsAnimationComplete => _panelAnimState == PanelAnimState.Idle;

        // ============================================================
        // TALKING TIMER - Global timer for cutscene triggers
        // ============================================================
        public static int TalkingTimer { get; private set; } = 0;
        /// <summary>Cumulative talking time across all sessions</summary>
        public static int TotalTalkingTime { get; private set; } = 0;

        public static void ResetTotalTalkingTime()
        {
            TotalTalkingTime = 0;
        }

        // ============================================================
        // CUTSCENE SYSTEM
        // ============================================================
        private static bool _isCutsceneActive = false;
        private static string _activeCutsceneID = "";
        private static int _cutsceneTimer = 0;
        private bool _isCutsceneMode = false;

        public static bool IsCutsceneActive => _isCutsceneActive;
        public static string ActiveCutsceneID => _activeCutsceneID;

        // ============================================================
        // COLOR DICTIONARY
        // ============================================================
        internal static readonly Dictionary<string, Color> NamedColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
        {
            { "White", Color.White },
            { "Pink", new Color(255, 182, 193) },
            { "LightBlue", new Color(173, 216, 230) },
            { "Green", new Color(144, 238, 144) },
            { "Yellow", new Color(255, 255, 150) },
            { "Orange", new Color(255, 200, 100) },
            { "Red", new Color(255, 100, 100) },
            { "Purple", new Color(200, 150, 255) },
            { "Cyan", Color.Cyan },
            { "Gold", new Color(255, 215, 0) },
            { "Gray", Color.LightGray },
        };

        // ============================================================
        // STATE VARIABLES
        // ============================================================
        private Projectile _sariaProjectile;
        private int _sariaTransform = 0;

        public int CurrentTransform => _sariaTransform;

        private Stack<string> _locationHistory = new Stack<string>();
        private DialogueNode _currentNode;
        private string _currentLocationID = "";


        // // Animations
        private int _eyeFrame = 0;
        private int _eyeAnimTimer = 0;
        private bool _isBlinking = false;
        private int _blinkFrameIndex = 0;
        private int _nextBlinkTime = 0;
        private int _mouthFrame = 0;
        private int _mouthAnimTimer = 0;

        // // Poe eye wave effect (form 7)
        private float _poeWavePhase = 0f;
        private float _poeWaveStrength = 0f; // 0 = solid, 1 = full wave active

        // // Button states
        private int _hoveredButton = -1;
        private bool[] _buttonEnabled = { true, true, true, true, true };

        // // UI state
        private bool _isActive = false;
        private int _soundCooldown = 0;
        private int _clickCooldown = 0;
        private bool _isEnding = false;
        private int _exitCountdown = 0;

        // // Panic system
        private const int HealthHistoryLength = 300;
        private int[] _healthHistory = new int[HealthHistoryLength];
        private int _healthHistoryIndex = 0;
        private bool _healthHistoryFilled = false;

        private const int PanicExitFrames = 60;
        private const int NormalExitFrames = 180;

        // // Mouse state - tracked once per frame
        private bool _wasMouseDown = false;
        private bool _mouseDownThisFrame = false;
        private bool _mouseReleasedThisFrame = false;


        // ============================================================
        // UI SCALE
        // ============================================================
        private float GetUIScale()
        {
            try { return FairyConfig.Instance?.DialogueUIScale ?? 1.5f; }
            catch { return 1.5f; }
        }

        public override void OnInitialize()
        {
            _nextBlinkTime = Main.rand.Next(120, 300);
            for (int i = 0; i < HealthHistoryLength; i++)
                _healthHistory[i] = -1;
        }

        // ============================================================
        // PUBLIC API
        // ============================================================
        private int _activeInteractionId = 0;

        private static bool TryParseSequenceToken(string token, out string kind, out int id)
        {
            kind = string.Empty;
            id = 0;
            if (string.IsNullOrWhiteSpace(token)) return false;

            var parts = token.Trim().Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;

            kind = parts[0].Trim().ToLowerInvariant();
            return int.TryParse(parts[1].Trim(), out id) && id > 0;
        }

        private static bool TryParseCompleteTarget(string target, out int id)
        {
            id = 0;
            if (string.IsNullOrWhiteSpace(target))
                return false;

            string t = target.Trim();
            if (!t.StartsWith("complete-", StringComparison.OrdinalIgnoreCase))
                return false;

            return int.TryParse(t.Substring("complete-".Length).Trim(), out id) && id > 0;
        }

        private void ApplySequenceTokenOnEnter(DialogueNode node)
        {
            if (node == null) return;

            if (!TryParseSequenceToken(node.SequenceToken, out string kind, out int id))
                return;

            var tracker = GetInteractionTracker();

            // If already completed or lost, do not allow entering the sequence.
            if ((kind == "start" || kind == "final") && (tracker.IsCompleted(id) || tracker.IsLost(id)))
            {
                CloseDialogue();
                return;
            }

            if (kind == "start")
            {
                _activeInteractionId = id;
            }
            // final-N is informational (marks last node) but does not award; awarding happens via Complete-N target.
        }

        private bool TryHandleCompleteTarget(string target)
        {
            if (!TryParseCompleteTarget(target, out int id))
                return false;

            // If player isn't currently in that interaction, do nothing.
            if (_activeInteractionId != id)
                return true;

            GetInteractionTracker().TryMarkCompleted(id);
            _activeInteractionId = 0;

            CloseDialogue();
            return true;
        }

        private SariaInteractionTrackerPlayer GetInteractionTracker()
            => Main.LocalPlayer.GetModPlayer<SariaInteractionTrackerPlayer>();

        private void MarkActiveInteractionLostIfAny()
        {
            if (_activeInteractionId <= 0) return;
            GetInteractionTracker().TryMarkLost(_activeInteractionId);
            _activeInteractionId = 0;
        }

        public void DisplayDialogue(string startID, Projectile sariaProj)
        {
            DialogueNode startNode = DialogueDatabase.GetNode(startID);
            if (startNode == null)
            {
                Main.NewText($"[SariaUI] ERROR: Node '{startID}' not found! (called from DisplayDialogue)", Color.Red);
                return;
            }
            DisplayDialogue(startNode, sariaProj);
        }

        public void DisplayDialogue(DialogueNode startNode, Projectile sariaProj)
        {
            _sariaProjectile = sariaProj;
            if (sariaProj?.ModProjectile is Saria sariaModProj)
                _sariaTransform = sariaModProj.Transform;

            _locationHistory.Clear();
            _isEnding = false;
            _exitCountdown = 0;
            _clickCooldown = 0;
            _wasMouseDown = false;
            _isDragging = false;
            _isCutsceneMode = false;

            if (_hasCustomPosition)
                _currentPanelOffset = _savedPanelPosition;
            else
            {
                var fp = Main.LocalPlayer?.GetModPlayer<FairyPlayer>();
                if (fp != null)
                {
                    _savedPanelPosition = new Vector2(fp.DialoguePanelPosX, fp.DialoguePanelPosY);
                    _hasCustomPosition = true;
                }
                _currentPanelOffset = _hasCustomPosition ? _savedPanelPosition : DefaultPanelOffset;
            }

            _healthHistoryIndex = 0;
            _healthHistoryFilled = false;
            int currentHealth = Main.LocalPlayer.statLife;
            for (int i = 0; i < HealthHistoryLength; i++)
                _healthHistory[i] = currentHealth;

            // Reset talking timer for this session
            TalkingTimer = 0;

            // === BUTTON CLICK HANDLER ===
            ChangeLocation(startNode, addToHistory: false);
            _isActive = true;

            // Start open animation
            _panelAnimState  = PanelAnimState.Opening;
            _panelAnimTimer  = 0;
            _animVisualOffset = new Vector2(-OpenSlideX, 0f);
            _animAlpha        = 0f;

            // // Check boundaries on open
            ClampToScreenBounds();
        }

        /// <summary>
        /// Start a CUTSCENE - Priority dialogue that ignores held item and panic triggers
        /// Only one cutscene can play at a time. New cutscenes are blocked until current ends.
        /// </summary>
        public bool StartCutscene(string cutsceneID, Projectile sariaProj)
        {
        int priority = 0;
        DialogueNode requested = DialogueDatabase.GetNode(cutsceneID);
        if (requested != null)
            priority = requested.CutscenePriority;

        // Higher-priority cutscenes can override lower-priority cutscenes
        if (!DialogueCutsceneManager.TryStart(cutsceneID, priority))
            return false;

        // Close any existing dialogue first
        if (_isActive)
            CloseDialogue();

        _sariaProjectile = sariaProj;
        if (sariaProj?.ModProjectile is Saria sariaModProj)
            _sariaTransform = sariaModProj.Transform;

        _locationHistory.Clear();
        _isEnding = false;
        _exitCountdown = 0;
        _clickCooldown = 0;
        _wasMouseDown = false;
        _isDragging = false;

        // Restore saved position or use default
        if (_hasCustomPosition)
            _currentPanelOffset = _savedPanelPosition;
        else
        {
            var fp = Main.LocalPlayer?.GetModPlayer<FairyPlayer>();
            if (fp != null)
            {
                _savedPanelPosition = new Vector2(fp.DialoguePanelPosX, fp.DialoguePanelPosY);
                _hasCustomPosition = true;
            }
            _currentPanelOffset = _hasCustomPosition ? _savedPanelPosition : DefaultPanelOffset;
        }

        // Mark as cutscene
        _isCutsceneMode = true;
        _isCutsceneActive = true;
        _activeCutsceneID = cutsceneID;
        _cutsceneTimer = 0;

        TalkingTimer = 0;

        ChangeLocation(cutsceneID, addToHistory: false);
        _isActive = true;

        // Start open animation
        _panelAnimState  = PanelAnimState.Opening;
        _panelAnimTimer  = 0;
        _animVisualOffset = new Vector2(-OpenSlideX, 0f);
        _animAlpha        = 0f;

        // Check boundaries on open
        ClampToScreenBounds();
        return true;
    }

    public void CloseDialogue()
    {
        // If we're already closing, let the animation finish naturally
        if (_panelAnimState == PanelAnimState.Closing) return;

        // Trigger close animation; ExecuteClose() is called when it finishes
        _panelAnimState = PanelAnimState.Closing;
        _panelAnimTimer = 0;
        _animVisualOffset = Vector2.Zero;
        _animAlpha = 1f;
    }

    private void ExecuteClose()
    {
        // Check for early exit of sunflower interaction
        if (_currentLocationID == "forest_sunflower_interaction" && _sariaProjectile != null && _sariaProjectile.active && _sariaProjectile.ModProjectile is Saria saria)
        {
            saria.Sigh();
        }

        // If a one-time interaction was started but not completed, closing loses it permanently.
        MarkActiveInteractionLostIfAny();

        // // Save position for next time
        _savedPanelPosition = _currentPanelOffset;
        _hasCustomPosition = true;
        var fpClose = Main.LocalPlayer?.GetModPlayer<FairyPlayer>();
        if (fpClose != null)
        {
            fpClose.DialoguePanelPosX = _savedPanelPosition.X;
            fpClose.DialoguePanelPosY = _savedPanelPosition.Y;
        }

        // Add to cumulative talking time
        TotalTalkingTime += TalkingTimer;

        // Clear cutscene state
        if (_isCutsceneMode)
        {
            string endingId = _activeCutsceneID;
            _isCutsceneActive = false;
            _activeCutsceneID = "";
            _cutsceneTimer = 0;
            DialogueCutsceneManager.End(endingId);
        }

        _isActive = false;
        _isCutsceneMode = false;
        _panelAnimState = PanelAnimState.Idle;
        _currentNode = null;
        _locationHistory.Clear();
        _isEnding = false;
        _exitCountdown = 0;
        _isDragging = false;
        ResetTypewriter();

        // Notify Saria that talking stopped
        if (_sariaProjectile?.ModProjectile is Saria sariaModProj)
        {
            sariaModProj.SariaTalking = false;
        }
    }

    public void TriggerPanic()
    {
        // Cutscenes ignore panic!
        if (_isCutsceneMode) return;

        if (_isActive && !_isEnding)
        {
            _isEnding = true;
            ChangeLocation("panic", addToHistory: false);
            _exitCountdown = PanicExitFrames;
            for (int i = 0; i < _buttonEnabled.Length; i++)
                _buttonEnabled[i] = false;
        }
    }

    public void TriggerNormalExit()
    {
        // Cutscenes ignore normal exit triggers (like dropping item)!
        if (_isCutsceneMode) return;

        // Exiting early during an active interaction loses it.
        MarkActiveInteractionLostIfAny();

        if (_isActive && !_isEnding)
        {
            _isEnding = true;

            string overrideTarget = DialogueNode.PickRandomTarget(_currentNode?.ExitTargetOverride);
            string exitNode = !string.IsNullOrWhiteSpace(overrideTarget)
                ? overrideTarget
                : (Main.rand.NextBool(4) ? "exit_sad" : "exit_happy");

            ChangeLocation(exitNode, addToHistory: false);
            _exitCountdown = NormalExitFrames;
            for (int i = 0; i < _buttonEnabled.Length; i++)
                _buttonEnabled[i] = false;
        }
    }

    private bool IsTargetBlockedBySequence(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                return false;

            // Complete-N is always allowed to be pressed (it will no-op if not active)
            if (TryParseCompleteTarget(DialogueNode.PickRandomTarget(target), out _))
                return false;

            string picked = DialogueNode.PickRandomTarget(target);
            if (string.IsNullOrWhiteSpace(picked))
                return false;

            var node = DialogueDatabase.GetNode(picked);
            if (node == null)
                return false;

            if (!TryParseSequenceToken(node.SequenceToken, out string kind, out int id))
                return false;

            if (kind != "start")
                return false;

            var tracker = GetInteractionTracker();
            return tracker.IsCompleted(id) || tracker.IsLost(id);
        }

    public bool IsActive => _isActive;
    public bool IsEnding => _isEnding;
    public bool IsCutsceneMode => _isCutsceneMode;

    // ============================================================
    // SCREEN BOUNDARY CHECK - Keeps buttons visible
    // ============================================================
    private void ClampToScreenBounds()
    {
        float scale = GetUIScale();
        Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;

        Vector2 panelPos = screenCenter + _currentPanelOffset * scale;
        
        // Calculate panel bounds
        float panelLeft = panelPos.X - (PanelWidth * scale / 2);
        float panelRight = panelPos.X + (PanelWidth * scale / 2);
        float panelTop = panelPos.Y - (PanelHeight * scale / 2);
        float panelBottom = panelPos.Y + (PanelHeight * scale / 2);

        // Keep at least 50 pixels of panel visible on screen for dragging
        float minVisible = 50 * scale;

        if (panelRight < minVisible)
            _currentPanelOffset.X += (minVisible - panelRight) / scale;
        if (panelLeft > Main.screenWidth - minVisible)
            _currentPanelOffset.X -= (panelLeft - (Main.screenWidth - minVisible)) / scale;
        if (panelBottom < minVisible)
            _currentPanelOffset.Y += (minVisible - panelBottom) / scale;
        if (panelTop > Main.screenHeight - minVisible)
            _currentPanelOffset.Y -= (panelTop - (Main.screenHeight - minVisible)) / scale;
    }

    // ============================================================
    // CHANGE LOCATION
    // ============================================================
    private void ChangeLocation(string nextLocationID, bool addToHistory = true)
    {
        if (string.IsNullOrEmpty(nextLocationID))
        {
            // Empty target = close dialogue
            // If we're in an active interaction, ending early loses it.
            MarkActiveInteractionLostIfAny();
            CloseDialogue();
            return;
        }

        // Handle pseudo-target that completes an interaction and ends (or later can be extended to route).
        if (TryHandleCompleteTarget(nextLocationID))
            return;

        DialogueNode nextNode = DialogueDatabase.GetNode(nextLocationID);
        if (nextNode == null)
        {
            Main.NewText($"[SariaUI] ERROR: Node '{nextLocationID}' not found! (navigating from '{_currentLocationID}')", Color.Red);
            return;
        }
        
        ChangeLocation(nextNode, addToHistory);
    }

    private void ChangeLocation(DialogueNode nextNode, bool addToHistory = true)
    {
        // Block targeting a start node that is already completed/lost.
        if (TryParseSequenceToken(nextNode.SequenceToken, out string k, out int seqId) && k == "start")
        {
            var tracker = GetInteractionTracker();
            if (tracker.IsCompleted(seqId) || tracker.IsLost(seqId))
                return;
        }

        if (addToHistory && !string.IsNullOrEmpty(_currentLocationID))
            _locationHistory.Push(_currentLocationID);

        _currentLocationID = nextNode.LocationID;
        _currentNode = nextNode;

        // Apply sequence token side effects
        ApplySequenceTokenOnEnter(nextNode);

        ResetTypewriter();
        _fullText = nextNode.DialogueText;
        PreprocessText();

        // Reset per-node timer baseline
        if (_isCutsceneMode)
            _cutsceneTimer = 0;

        // // Update button enabled states (but always draw all buttons!)
        _buttonEnabled[0] = !string.IsNullOrEmpty(nextNode.Button1Label) && !string.IsNullOrEmpty(nextNode.Button1Target) && !IsTargetBlockedBySequence(nextNode.Button1Target);
        _buttonEnabled[1] = !string.IsNullOrEmpty(nextNode.Button2Label) && !string.IsNullOrEmpty(nextNode.Button2Target) && !IsTargetBlockedBySequence(nextNode.Button2Target);
        _buttonEnabled[2] = !string.IsNullOrEmpty(nextNode.Button3Label) && !string.IsNullOrEmpty(nextNode.Button3Target) && !IsTargetBlockedBySequence(nextNode.Button3Target);
        _buttonEnabled[3] = !nextNode.DisableBackButton && _locationHistory.Count > 0;
        _buttonEnabled[4] = !nextNode.DisableExitButton;

        SoundEngine.PlaySound(SoundID.MenuOpen);
    }



    // ============================================================
    // UPDATE
    // ============================================================
    public override void Update(GameTime gameTime)
    {
        if (!_isActive || _currentNode == null) return;
        if (Main.gamePaused) return;

        // ── Animation tick ─────────────────────────────────────────────────
        if (_panelAnimState == PanelAnimState.Opening)
        {
            _panelAnimTimer++;
            float t = Math.Clamp(_panelAnimTimer / (float)OpenAnimFrames, 0f, 1f);
            // Ease-out cubic
            float ease = 1f - (1f - t) * (1f - t) * (1f - t);
            _animVisualOffset = new Vector2(-OpenSlideX * (1f - ease), 0f);
            _animAlpha = ease;
            if (_panelAnimTimer >= OpenAnimFrames)
            {
                _panelAnimState   = PanelAnimState.Idle;
                _animVisualOffset = Vector2.Zero;
                _animAlpha        = 1f;
            }
            // Block all interaction until open anim finishes
            _wasMouseDown = Main.mouseLeft;
            return;
        }

        if (_panelAnimState == PanelAnimState.Closing)
        {
            _panelAnimTimer++;
            float t = Math.Clamp(_panelAnimTimer / (float)CloseAnimFrames, 0f, 1f);
            // Ease-in cubic
            float ease = t * t * t;
            _animVisualOffset = new Vector2(0f, CloseSlideY * ease);
            _animAlpha = 1f - ease;
            if (_panelAnimTimer >= CloseAnimFrames)
            {
                ExecuteClose();
            }
            // Block interaction during close anim too
            _wasMouseDown = Main.mouseLeft;
            return;
        }
        // ───────────────────────────────────────────────────────────────────

        // Force-stop cutscene on player death
        if (_isCutsceneMode && Main.LocalPlayer.dead)
        {
            CloseDialogue();
            return;
        }

        base.Update(gameTime);

        Main.LocalPlayer.mouseInterface = true;

        // Track mouse state ONCE at the start of the frame
        bool mouseDown = Main.mouseLeft;
        _mouseDownThisFrame = mouseDown && !_wasMouseDown;  // Just pressed
        _mouseReleasedThisFrame = !mouseDown && _wasMouseDown;  // Just released

        // Increment talking timers
        TalkingTimer++;
        if (_isCutsceneMode)
            _cutsceneTimer++;

        if (_sariaProjectile != null && _sariaProjectile.active && _sariaProjectile.ModProjectile is Saria sariaModProj)
            _sariaTransform = sariaModProj.Transform;

        if (_soundCooldown > 0) _soundCooldown--;
        if (_clickCooldown > 0) _clickCooldown--;

        // Only non-cutscenes are allowed to auto-exit via healball/panic
        if (!_isCutsceneMode)
        {
            CheckHealBallHeld();

            if (!_isEnding)
                CheckForPanic();
        }

        // Auto-advance timer: only when not ending and once text is complete
        if (!_isEnding && _currentNode.AutoAdvanceFrames > 0)
        {
            if (_cutsceneTimer >= _currentNode.AutoAdvanceFrames && _isTextComplete)
            {
                string target = DialogueNode.PickRandomTarget(_currentNode.AutoAdvanceTarget);
                if (!string.IsNullOrWhiteSpace(target))
                    ChangeLocation(target);
                else
                    CloseDialogue();
                return;
            }
        }

        if (_isEnding && _isTextComplete)
        {
            _exitCountdown--;
            if (_exitCountdown <= 0)
            {
                CloseDialogue();
                return;
            }
        }

        // Reset speaking flag before typewriter update
        _isSpeakingThisFrame = false;

        UpdateTypewriter();
        UpdateEyeAnimation();
        UpdateMouthAnimation();

        if (!_isEnding)
        {
            UpdateButtonHover();
            UpdateDragging(mouseDown);
            UpdateButtonClicks();
        }

        _wasMouseDown = mouseDown;
        ClampToScreenBounds();
    }

    // ============================================================
    // BUTTON HOVER DETECTION (separate from click handling)
    // ============================================================
    private void UpdateButtonHover()
    {
        if (_isDragging)
        {
            _hoveredButton = -1;
            return;
        }

        float scale = GetUIScale();
        Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
        Vector2 panelPos = screenCenter + _currentPanelOffset * scale;
        
        // Apply background offset so hitboxes match visuals
        Vector2 adjustedPanelPos = panelPos + BackgroundOffset * scale;

        Rectangle exitRect = CreateButtonRect(adjustedPanelPos, ExitButtonOffset * scale, SmallButtonSize * scale);
        Rectangle backRect = CreateButtonRect(adjustedPanelPos, BackButtonOffset * scale, SmallButtonSize * scale);
        Rectangle btn1Rect = CreateButtonRect(adjustedPanelPos, Button1Offset * scale, CustomButtonSize * scale);
        Rectangle btn2Rect = CreateButtonRect(adjustedPanelPos, Button2Offset * scale, CustomButtonSize * scale);
        Rectangle btn3Rect = CreateButtonRect(adjustedPanelPos, Button3Offset * scale, CustomButtonSize * scale);

        Point mousePos = new Point(Main.mouseX, Main.mouseY);
        int previousHover = _hoveredButton;
        _hoveredButton = -1;

        // Check buttons in order of priority (smaller buttons first to prevent overlap issues)
        // Exit and Back buttons have priority over custom buttons
        if (_buttonEnabled[4] && exitRect.Contains(mousePos) && !_isEnding)
            _hoveredButton = 4;
        else if (_buttonEnabled[3] && backRect.Contains(mousePos))
            _hoveredButton = 3;
        else if (_buttonEnabled[0] && btn1Rect.Contains(mousePos))
            _hoveredButton = 0;
        else if (_buttonEnabled[1] && btn2Rect.Contains(mousePos))
            _hoveredButton = 1;
        else if (_buttonEnabled[2] && btn3Rect.Contains(mousePos))
            _hoveredButton = 2;

        if (_hoveredButton != -1 && _hoveredButton != previousHover && _buttonEnabled[_hoveredButton])
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }

    // ============================================================
    // DRAGGING SYSTEM
    // ============================================================
    private void UpdateDragging(bool mouseDown)
    {
        float scale = GetUIScale();
        Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
        Vector2 panelPos = screenCenter + _currentPanelOffset * scale;
        
        // Apply background offset so drag area matches visuals
        Vector2 adjustedPanelPos = panelPos + BackgroundOffset * scale;

        Rectangle panelRect = new Rectangle(
            (int)(adjustedPanelPos.X - PanelWidth * scale / 2),
            (int)(adjustedPanelPos.Y - PanelHeight * scale / 2),
            (int)(PanelWidth * scale),
            (int)(PanelHeight * scale)
        );

        Point mousePos = new Point(Main.mouseX, Main.mouseY);

        // Start dragging: mouse just pressed, not on a button, on the panel
        if (_mouseDownThisFrame && _hoveredButton == -1 && panelRect.Contains(mousePos))
        {
            _isDragging = true;
            _dragStartMouse = new Vector2(Main.mouseX, Main.mouseY);
            _dragStartPanel = _currentPanelOffset;
        }

        // Continue dragging
        if (_isDragging && mouseDown)
        {
            Vector2 currentMouse = new Vector2(Main.mouseX, Main.mouseY);
            Vector2 delta = (currentMouse - _dragStartMouse) / scale;
            _currentPanelOffset = _dragStartPanel + delta;
        }

        // Stop dragging when mouse released
        if (!mouseDown)
        {
            if (_isDragging)
            {
                // Snap: if more than 50% of the panel is off any screen edge, clamp it back
                float halfW = PanelWidth  * scale / 2f;
                float halfH = PanelHeight * scale / 2f;
                // Panel center in screen space
                Vector2 center = screenCenter + _currentPanelOffset * scale;
                // Clamp so at most 50% hangs off each edge (i.e. centre stays within half-panel of screen edge)
                center.X = Math.Clamp(center.X, halfW / 2f, Main.screenWidth  - halfW / 2f);
                center.Y = Math.Clamp(center.Y, halfH / 2f, Main.screenHeight - halfH / 2f);
                _currentPanelOffset = (center - screenCenter) / scale;

                // Persist position to player so it survives game close
                _savedPanelPosition = _currentPanelOffset;
                _hasCustomPosition = true;
                var fp = Main.LocalPlayer?.GetModPlayer<FairyPlayer>();
                if (fp != null)
                {
                    fp.DialoguePanelPosX = _savedPanelPosition.X;
                    fp.DialoguePanelPosY = _savedPanelPosition.Y;
                }
            }
            _isDragging = false;
        }
    }

    // ============================================================
    // BUTTON CLICK HANDLING
    // ============================================================
    private void UpdateButtonClicks()
    {
        if (_clickCooldown > 0) return;
        if (_isDragging) return;

        // Only process click on mouse release
        if (_mouseReleasedThisFrame && _hoveredButton != -1 && _buttonEnabled[_hoveredButton])
        {
            OnButtonClicked(_hoveredButton);
            _clickCooldown = 15;
        }
    }

    private void CheckHealBallHeld()
    {
        if (_isEnding) return;
        bool holdingHealBall = Main.LocalPlayer.HeldItem.type == ModContent.ItemType<HealBall>();
        if (!holdingHealBall)
            TriggerNormalExit();
    }

    private void CheckForPanic()
    {
        int currentHealth = Main.LocalPlayer.statLife;
        int maxHealth = Main.LocalPlayer.statLifeMax2;

        _healthHistory[_healthHistoryIndex] = currentHealth;
        _healthHistoryIndex = (_healthHistoryIndex + 1) % HealthHistoryLength;
        if (_healthHistoryIndex == 0) _healthHistoryFilled = true;

        int oldestIndex = _healthHistoryFilled ? _healthHistoryIndex : 0;
        int oldestHealth = _healthHistory[oldestIndex];

        if (oldestHealth > 0)
        {
            int healthLost = oldestHealth - currentHealth;
            float percentLost = (float)healthLost / maxHealth;
            if (percentLost >= 0.10f)
                TriggerPanic();
        }
    }


    // ============================================================
    // ANIMATIONS
    // ============================================================
    private void UpdateEyeAnimation()
    {
        if (Main.gamePaused) return;
        _eyeAnimTimer++;

        if (!_isBlinking)
        {
            if (_eyeAnimTimer >= _nextBlinkTime)
            {
                _isBlinking = true;
                _blinkFrameIndex = 0;
                _eyeAnimTimer = 0;
            }
        }
        else
        {
            if (_eyeAnimTimer >= 4)
            {
                _eyeAnimTimer = 0;
                _blinkFrameIndex++;
                if (_blinkFrameIndex >= 4)
                {
                    _isBlinking = false;
                    _eyeFrame = 0;
                    _nextBlinkTime = Main.rand.Next(120, 300);
                }
                else
                {
                    _eyeFrame = _blinkFrameIndex;
                }
            }
        }
    }

    /// <summary>
    /// Mouth animation - Animates while text is actively being revealed
    /// Stops on pauses, waits, and [silent] sections
    /// </summary>
    private void UpdateMouthAnimation()
    {
        if (Main.gamePaused) return;

        if (_currentNode != null && !_currentNode.AnimateMouth)
        {
            _mouthFrame = 0;
            _mouthAnimTimer = 0;
            return;
        }

        // Animate mouth while we're actively speaking (text being revealed, not waiting, not complete)
        bool shouldAnimate = !_isTextComplete && _waitFrames <= 0 && _isCurrentlySpeaking;

        if (shouldAnimate)
        {
            _mouthAnimTimer++;
            if (_mouthAnimTimer >= _currentMouthSpeed)
            {
                _mouthAnimTimer = 0;
                _mouthFrame = (_mouthFrame + 1) % 5;
            }
        }
        else
        {
            // Return to default frame (closed mouth) when not speaking
            _mouthFrame = 0;
            _mouthAnimTimer = 0;
        }
    }

    private Rectangle CreateButtonRect(Vector2 panelPos, Vector2 offset, Vector2 size)
    {
        Vector2 pos = panelPos + offset;
        return new Rectangle(
            (int)(pos.X - size.X / 2),
            (int)(pos.Y - size.Y / 2),
            (int)size.X,
            (int)size.Y
        );
    }

    private void OnButtonClicked(int buttonIndex)
    {
        if (_currentNode == null) return;

        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/OptionSelect"), Main.LocalPlayer.Center);

        // Special handling for Pending node: Clear the pending cutscene when the button is clicked
        if (_currentNode.LocationID == "Pending" && buttonIndex == 1)
        {
            var tracker = GetInteractionTracker();
            // We need to complete the specific cutscene that was presented.
            // Since the UI is modal and pauses game logic (mostly), the "best" cutscene should still be the same one.
            var pending = tracker.GetBestAvailableCutscene();
            if (pending != null)
            {
                tracker.CompletePendingCutscene(pending.ID);
            }
        }

        switch (buttonIndex)
        {
            case 0:
                {
                    string target = DialogueNode.PickRandomTarget(_currentNode.Button1Target);
                    if (!string.IsNullOrEmpty(target))
                    {
                        if (!TryHandleCompleteTarget(target))
                            ChangeLocation(target);
                    }
                    else
                    {
                        MarkActiveInteractionLostIfAny();
                        CloseDialogue();
                    }
                    break;
                }
            case 1:
                {
                    string target = DialogueNode.PickRandomTarget(_currentNode.Button2Target);
                    if (!string.IsNullOrEmpty(target))
                    {
                        if (!TryHandleCompleteTarget(target))
                            ChangeLocation(target);
                    }
                    else
                    {
                        MarkActiveInteractionLostIfAny();
                        CloseDialogue();
                    }
                    break;
                }
            case 2:
                {
                    string target = DialogueNode.PickRandomTarget(_currentNode.Button3Target);
                    if (!string.IsNullOrEmpty(target))
                    {
                        if (!TryHandleCompleteTarget(target))
                            ChangeLocation(target);
                    }
                    else
                    {
                        MarkActiveInteractionLostIfAny();
                        CloseDialogue();
                    }
                    break;
                }
            case 3:
                if (_locationHistory.Count > 0)
                {
                    string previous = _locationHistory.Pop();
                    ChangeLocation(previous, addToHistory: false);
                }
                break;
            case 4:
                MarkActiveInteractionLostIfAny();
                TriggerNormalExit();
                break;
        }
    }

}

}
