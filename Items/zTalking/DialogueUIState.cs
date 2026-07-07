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
    public class DialogueUIState : UIState
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

        // // Typewriter
        private string _fullText = "";
        private int _charIndex = 0;
        private int _frameCounter = 0;
        private int _currentSpeed = 2;
        private int _waitFrames = 0;
        private bool _isTextComplete = false;
        private List<ColoredChar> _coloredText = new List<ColoredChar>();
        private List<List<ColoredChar>> _wrappedLines = new List<List<ColoredChar>>();
        private int _currentLineIndex = 0;
        private int _currentCharInLine = 0;

        // // MOUTH ANIMATION
        private bool _isSpeakingThisFrame = false;
        private bool _isSilentMode = false;
        private int _baseMouthSpeed = 6;
        private int _currentMouthSpeed = 6;
        private bool _isCurrentlySpeaking = false; // Tracks if we're in an active speaking section

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

        private struct ColoredChar
        {
            public char Character;
            public Color TextColor;
            public bool IsSilent;
            public bool MouthEnabled;
            public ColoredChar(char c, Color color, bool silent = false, bool mouthEnabled = true)
            {
                Character = c;
                TextColor = color;
                IsSilent = silent;
                MouthEnabled = mouthEnabled;
            }
        }

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

    private void ResetTypewriter()
    {
        _coloredText.Clear();
        _wrappedLines.Clear();
        _charIndex = 0;
        _frameCounter = 0;
        _currentSpeed = 2;
        _waitFrames = 0;
        _isTextComplete = false;
        _currentLineIndex = 0;
        _currentCharInLine = 0;
        _isSilentMode = false;
        _isSpeakingThisFrame = false;
        _isCurrentlySpeaking = false;
        _mouthFrame = 0;
        _mouthAnimTimer = 0;
        _currentMouthSpeed = _baseMouthSpeed;
    }

    // ============================================================
    // WORD WRAP
    // ============================================================
    private void PreprocessText()
    {
        _wrappedLines.Clear();
        if (string.IsNullOrEmpty(_fullText)) return;

        float scale = GetUIScale() * TextScale;
        float maxWidth = GetTextMaxWidth() * GetUIScale();
        DynamicSpriteFont font = FontAssets.MouseText.Value;

        List<ColoredChar> allChars = new List<ColoredChar>();
        Color currentColor = Color.White;
        bool currentSilent = false;
        bool currentMouthEnabled = true;
        int i = 0;

        while (i < _fullText.Length)
        {
            char c = _fullText[i];
            if (c == '[')
            {
                int tagEnd = _fullText.IndexOf(']', i);
                if (tagEnd > i)
                {
                    string tag = _fullText.Substring(i + 1, tagEnd - i - 1);
                    i = tagEnd + 1;

                    if (tag.StartsWith("color:", StringComparison.OrdinalIgnoreCase))
                    {
                        string colorName = tag.Substring(6);
                        if (NamedColors.TryGetValue(colorName, out Color newColor))
                            currentColor = newColor;
                    }
                    else if (tag.Equals("silent", StringComparison.OrdinalIgnoreCase))
                    {
                        currentSilent = true;
                    }
                    else if (tag.Equals("/silent", StringComparison.OrdinalIgnoreCase))
                    {
                        currentSilent = false;
                    }
                    else if (tag.Equals("mouth", StringComparison.OrdinalIgnoreCase))
                    {
                        currentMouthEnabled = true;
                    }
                    else if (tag.Equals("/mouth", StringComparison.OrdinalIgnoreCase))
                    {
                        currentMouthEnabled = false;
                    }
                    continue;
                }
            }

            allChars.Add(new ColoredChar(c, currentColor, currentSilent, currentMouthEnabled));
            i++;
        }

        List<ColoredChar> currentLine = new List<ColoredChar>();
        List<ColoredChar> currentWord = new List<ColoredChar>();
        float currentLineWidth = 0;

        foreach (var cc in allChars)
        {
            if (cc.Character == ' ')
            {
                float wordWidth = MeasureWord(font, currentWord, scale);
                if (currentLineWidth + wordWidth > maxWidth && currentLine.Count > 0)
                {
                    _wrappedLines.Add(new List<ColoredChar>(currentLine));
                    currentLine.Clear();
                    currentLineWidth = 0;
                }
                currentLine.AddRange(currentWord);
                currentLineWidth += wordWidth;
                currentLine.Add(cc);
                currentLineWidth += font.MeasureString(" ").X * scale;
                currentWord.Clear();
            }
            else if (cc.Character == '\n')
            {
                currentLine.AddRange(currentWord);
                _wrappedLines.Add(new List<ColoredChar>(currentLine));
                currentLine.Clear();
                currentWord.Clear();
                currentLineWidth = 0;
            }
            else
            {
                currentWord.Add(cc);
            }
        }

        if (currentWord.Count > 0)
        {
            float wordWidth = MeasureWord(font, currentWord, scale);
            if (currentLineWidth + wordWidth > maxWidth && currentLine.Count > 0)
            {
                _wrappedLines.Add(new List<ColoredChar>(currentLine));
                currentLine.Clear();
            }
            currentLine.AddRange(currentWord);
        }
        if (currentLine.Count > 0)
            _wrappedLines.Add(currentLine);
    }

    private float MeasureWord(DynamicSpriteFont font, List<ColoredChar> word, float scale)
    {
        float width = 0;
        foreach (var cc in word)
            width += font.MeasureString(cc.Character.ToString()).X * scale;
        return width;
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
    // TYPEWRITER
    // ============================================================
    private void UpdateTypewriter()
    {
        if (Main.gamePaused) return;
        if (_isTextComplete || _wrappedLines.Count == 0) return;

        if (_waitFrames > 0)
        {
            _waitFrames--;
            _isSpeakingThisFrame = false;
            _isCurrentlySpeaking = false;
            return;
        }

        _frameCounter++;
        if (_frameCounter < _currentSpeed)
        {
            return;
        }
        _frameCounter = 0;

        if (_currentLineIndex < _wrappedLines.Count)
        {
            var line = _wrappedLines[_currentLineIndex];
            if (_currentCharInLine < line.Count)
            {
                var cc = line[_currentCharInLine];
                _coloredText.Add(cc);
                _currentCharInLine++;

                bool shouldSpeak = !cc.IsSilent && cc.MouthEnabled && cc.Character != ' ';

                if (shouldSpeak)
                {
                    _isSpeakingThisFrame = true;
                    _isCurrentlySpeaking = true;

                    // Play typewriter sound with smooth overlap and weighted pitch variance
                    if (_soundCooldown <= 0)
                    {
                        // Weighted randomization: 80% default pitch, 20% lower pitch
                        float pitchVariance = 0f;
                        float volumeLevel = 1f;
                        int roll = Main.rand.Next(100);
                        if (roll >= 80)
                        {
                            // Lower pitch: random between -0.05f and -0.15f
                            pitchVariance = -0.05f - (Main.rand.NextFloat() * 0.10f);
                            // Reduce volume slightly for lowered pitch (more natural feel)
                            volumeLevel = 0.9f;
                        }

                        // MaxInstances = 3 allows sound tails to overlap for smoother transitions
                        // SoundLimitBehavior.ReplaceOldest ensures older sounds fade out gracefully
                        SoundStyle talkingSound = new SoundStyle("SariaMod/Sounds/SariaTalking")
                        {
                            Pitch = pitchVariance,
                            Volume = volumeLevel,
                            MaxInstances = 3,
                            SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
                        };
                        SoundEngine.PlaySound(talkingSound, Main.LocalPlayer.Center);
                        _soundCooldown = 3;
                    }
                }
                else
                {
                    _isSpeakingThisFrame = false;
                    if (cc.IsSilent || !cc.MouthEnabled)
                        _isCurrentlySpeaking = false;
                }
            }
            else
            {
                _currentLineIndex++;
                _currentCharInLine = 0;
            }
        }

        CheckForTagsAtPosition();

        _currentMouthSpeed = _baseMouthSpeed + Math.Max(0, (_currentSpeed - 2) * 2);

        int totalChars = 0;
        foreach (var line in _wrappedLines) totalChars += line.Count;
        if (_coloredText.Count >= totalChars)
        {
            _isTextComplete = true;
            _isCurrentlySpeaking = false;
        }
    }

    private void CheckForTagsAtPosition()
    {
        while (_charIndex < _fullText.Length)
        {
            if (_fullText[_charIndex] != '[') { _charIndex++; break; }

            int tagEnd = _fullText.IndexOf(']', _charIndex);
            if (tagEnd <= _charIndex) break;

            string tag = _fullText.Substring(_charIndex + 1, tagEnd - _charIndex - 1);
            _charIndex = tagEnd + 1;

            if (tag.StartsWith("wait:", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(tag.Substring(5), out int waitTime))
                    _waitFrames = waitTime;
            }
            else if (tag.StartsWith("speed:", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(tag.Substring(6), out int newSpeed))
                    _currentSpeed = Math.Max(1, newSpeed);
            }
            // Mouth on/off is handled in PreprocessText by per-character flags.
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

    // ============================================================
    // DRAWING
    // ============================================================
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!_isActive || _currentNode == null) return;
        // Also draw during close animation even after _isActive will be cleared
        if (_panelAnimState == PanelAnimState.Closing && _animAlpha <= 0f) return;

        // Tick alpha pulse counters once per frame (frame-guarded, safe to call from anywhere)
        SariaModUtilities.UpdateAlphaCounters();

        // Switch to PointClamp for crisp pixel art rendering (all dialogue UI textures)
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

        float scale = GetUIScale();
        float a = _animAlpha; // 0..1 overall panel alpha from animation
        Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
        Vector2 panelPos = screenCenter + _currentPanelOffset * scale;

        // Apply visual-only animation offset (never persisted) + background offset
        Vector2 adjustedPanelPos = panelPos + BackgroundOffset * scale + _animVisualOffset * scale;

        DrawBackground(spriteBatch, adjustedPanelPos, scale, a);
        DrawGreetingPortrait(spriteBatch, adjustedPanelPos, scale, a);
        DrawPortrait(spriteBatch, adjustedPanelPos, scale, a);
        DrawGreetingOverHead(spriteBatch, adjustedPanelPos, scale, a);
        DrawDialogueText(spriteBatch, adjustedPanelPos, scale, a);
        // Hide button labels and text while opening; show blank button shells only
        bool labelsVisible = _panelAnimState == PanelAnimState.Idle;
        DrawButtons(spriteBatch, adjustedPanelPos, scale, a, labelsVisible);

        // Show cutscene indicator (only when fully open)
        if (_isCutsceneMode && labelsVisible)
        {
            Vector2 cutscenePos = adjustedPanelPos + new Vector2(0, -100) * scale;
            Utils.DrawBorderString(spriteBatch, "~ CUTSCENE ~", cutscenePos, Color.Gold * 0.8f * a, 0.7f * scale, 0.5f, 0.5f);
        }

        if (_isEnding && _isTextComplete && labelsVisible)
        {
            float secondsLeft = _exitCountdown / 60f;
            string exitText = $"Closing in {secondsLeft:F1}s...";
            Vector2 exitPos = adjustedPanelPos + new Vector2(0, 70) * scale;
            Utils.DrawBorderString(spriteBatch, exitText, exitPos, Color.Gray * 0.7f * a, 0.8f * scale, 0.5f, 0.5f);
        }

        // Restore default sampler state before returning to framework
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
        base.Draw(spriteBatch);
    }

    private void DrawBackground(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha)
    {
        string panelPath = "SariaMod/Items/zTalking/SariaPanel";
        Texture2D panelTexture;
        try { panelTexture = ModContent.Request<Texture2D>(panelPath).Value; }
        catch { return; }

        if (panelTexture != null)
        {
            Vector2 origin = new Vector2(panelTexture.Width, panelTexture.Height) / 2f;
            spriteBatch.Draw(panelTexture, panelPos, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawGreetingPortrait(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha)
    {
        string greetingPath = $"SariaMod/Items/zTalking/Greetings{_sariaTransform + 1}";
        Texture2D greetingTexture;
        try { greetingTexture = ModContent.Request<Texture2D>(greetingPath).Value; }
        catch { greetingTexture = ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Greetings1").Value; }

        if (greetingTexture != null)
        {
            Vector2 greetingPos = panelPos + GreetingPortraitOffset * scale;
            Vector2 origin = new Vector2(greetingTexture.Width, greetingTexture.Height) / 2f;
            spriteBatch.Draw(greetingTexture, greetingPos, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawGreetingOverHead(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha)
    {
        if (_sariaTransform == 1)
        {
            // Greetings2OverHead appears on the second transform (Zora form)
            string overHeadPath = "SariaMod/Items/zTalking/Greetings2OverHead";
            Texture2D overHeadTexture;
            try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
            catch { return; }

            if (overHeadTexture != null)
            {
                Vector2 overHeadPos = panelPos + GreetingPortraitOffset * scale;
                Vector2 origin = new Vector2(overHeadTexture.Width, overHeadTexture.Height) / 2f;
                spriteBatch.Draw(overHeadTexture, overHeadPos, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }
        else if (_sariaTransform == 2)
        {
            // Greetings3OverHead appears on the third transform (Gerudo form) - Twinrova fire hair
            string overHeadPath = "SariaMod/Items/zTalking/Greetings3OverHead";
            Texture2D overHeadTexture;
            try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
            catch { return; }

            if (overHeadTexture != null)
            {
                Vector2 overHeadPos = panelPos + GreetingPortraitOffset * scale;
                Vector2 origin = new Vector2(overHeadTexture.Width, overHeadTexture.Height) / 2f;

                // Flame shimmer effect
                ulong randShakeEffect = (Main.GameUpdateCount / 8) ^ (ulong)((long)overHeadPos.Y << 20 | (long)(uint)overHeadPos.X);
                float shakeX = Utils.RandomInt(ref randShakeEffect, -4, -3) * 0.07f;
                float shakeY = Utils.RandomInt(ref randShakeEffect, -4, 3) * 0.07f;
                Vector2 shimmerPos = overHeadPos + new Vector2(shakeY, shakeX) * scale;

                // Swap to NonPremultiplied so the texture's straight alpha renders with exact original colors
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                spriteBatch.Draw(overHeadTexture, shimmerPos, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);

                // Restore original blend state for remaining UI draws
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }
        }
        else if (_sariaTransform == 3)
        {
            // SariaSparksPortrait overlay for electric form — only when sparks are active on Saria
            bool sparksActive = false;
            if (_sariaProjectile != null && _sariaProjectile.active && _sariaProjectile.ModProjectile is Saria sparksSaria)
                sparksActive = sparksSaria.SpecialAnimateValue > 0;

            if (sparksActive)
            {
                string sparksPath = "SariaMod/Items/zTalking/SariaSparksPortrait";
                Texture2D sparksTexture;
                try { sparksTexture = ModContent.Request<Texture2D>(sparksPath).Value; }
                catch { sparksTexture = null; }

                if (sparksTexture != null)
                {
                    // 14 frames, advance every 3 game ticks (matches SariaSparksDraw)
                    int sparksFrame = (int)Main.GameUpdateCount / 3 % 14;
                    Rectangle sparksRect = sparksTexture.Frame(verticalFrames: 14, frameY: sparksFrame);
                    Vector2 sparksOrigin = sparksRect.Size() / 2f;
                    Vector2 sparksPos = panelPos + SparksPortraitOffset * scale;

                    Color sparksColor = Color.Lerp(Color.White, Color.LightBlue, 2f);
                    sparksColor *= 0.85f * alpha;

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                    spriteBatch.Draw(sparksTexture, sparksPos, sparksRect, sparksColor, 0f, sparksOrigin, scale, SpriteEffects.None, 0f);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                }
            }
        }
        else if (_sariaTransform == 4)
        {
            // Greetings5OverHead with DialogueUIMaskdraw-style alpha2 fade
            string overHeadPath = "SariaMod/Items/zTalking/Greetings5OverHead";
            Texture2D overHeadTexture;
            try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
            catch { overHeadTexture = null; }

            if (overHeadTexture != null)
            {
                Vector2 overHeadPos = panelPos + GreetingPortraitOffset * scale;
                Vector2 origin = new Vector2(overHeadTexture.Width, overHeadTexture.Height) / 2f;

                Color drawColor = Color.White;
                drawColor = Color.Lerp(drawColor, Color.FloralWhite, 30f);
                drawColor = Color.Lerp(drawColor, Color.Transparent, SariaModUtilities.alpha2);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                spriteBatch.Draw(overHeadTexture, overHeadPos, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }

            // Greetings5OverHead2
            string overHeadPath2 = "SariaMod/Items/zTalking/Greetings5OverHead2";
            Texture2D overHeadTexture2;
            try { overHeadTexture2 = ModContent.Request<Texture2D>(overHeadPath2).Value; }
            catch { overHeadTexture2 = null; }

            if (overHeadTexture2 != null)
            {
                Vector2 overHeadPos2 = panelPos + GreetingPortraitOffset * scale;
                Vector2 origin2 = new Vector2(overHeadTexture2.Width, overHeadTexture2.Height) / 2f;

                Color drawColor2 = Color.White;
                drawColor2 = Color.Lerp(drawColor2, Color.FloralWhite, 30f);
                drawColor2 = Color.Lerp(drawColor2, Color.Transparent, SariaModUtilities.alpha3);
                drawColor2 *= alpha;

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                spriteBatch.Draw(overHeadTexture2, overHeadPos2, null, drawColor2, 0f, origin2, scale, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }
        }
    }

    private void DrawPortrait(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha)
    {
        string faceSet = _currentNode?.FaceSetName;
        Vector2 eyesPos = panelPos + (_sariaTransform == 6 ? Eyes7Offset : EyesOffset) * scale;
        DrawEyes(spriteBatch, eyesPos, scale, faceSet, alpha);
        DrawMouth(spriteBatch, panelPos + MouthOffset * scale, scale, faceSet, alpha);

        var extra = DialogueFaceSetRegistry.TryResolveExtraTexture(faceSet, _sariaTransform);
        if (extra != null)
        {
            Vector2 origin = new Vector2(extra.Width, extra.Height) / 2f;
            spriteBatch.Draw(extra, panelPos, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        // SariaSparksPortrait overlay for electric form (transform 3) — synced to in-world sparks
        if (_sariaTransform == 3)
        {
            bool sparksActive = false;
            if (_sariaProjectile != null && _sariaProjectile.active && _sariaProjectile.ModProjectile is Saria sparksSaria)
                sparksActive = sparksSaria.SpecialAnimateValue > 0;

            if (sparksActive)
            {
                string sparksPath = "SariaMod/Items/zTalking/SariaSparksPortrait";
                Texture2D sparksTexture;
                try { sparksTexture = ModContent.Request<Texture2D>(sparksPath).Value; }
                catch { sparksTexture = null; }

                if (sparksTexture != null)
                {
                    // 14 frames, advance every 3 game ticks (matches SariaSparksDraw)
                    int sparksFrame = (int)Main.GameUpdateCount / 3 % 14;
                    Rectangle sparksRect = sparksTexture.Frame(verticalFrames: 14, frameY: sparksFrame);
                    Vector2 sparksOrigin = sparksRect.Size() / 2f;
                    Vector2 sparksPos = panelPos + SparksPortraitOffset * scale;

                    Color sparksColor = Color.Lerp(Color.White, Color.LightBlue, 2f);
                    sparksColor *= 0.85f * alpha;

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                    spriteBatch.Draw(sparksTexture, sparksPos, sparksRect, sparksColor, 0f, sparksOrigin, scale, SpriteEffects.None, 0f);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                }
            }
        }
    }

    private void DrawEyes(SpriteBatch spriteBatch, Vector2 position, float scale, string faceSetName, float alpha)
    {
        Texture2D eyeTexture = DialogueFaceSetRegistry.TryResolveEyesTexture(faceSetName, _sariaTransform);
        if (eyeTexture == null) return;

        int numFrames = 4;
        int frameHeight = eyeTexture.Height / numFrames;
        Rectangle sourceRect = new Rectangle(0, _eyeFrame * frameHeight, eyeTexture.Width, frameHeight);
        Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;

        if (_sariaTransform == 4)
        {
            // Form 5: draw base eyes underneath first (uses current face set)
            var baseSet = DialogueFaceSetRegistry.Get(faceSetName);
            string basePath = $"SariaMod/Items/zTalking/{baseSet.EyesPrefix}1";
            if (ModContent.RequestIfExists(basePath, out ReLogic.Content.Asset<Texture2D> baseAsset))
            {
                Texture2D baseEyes = baseAsset.Value;
                int baseFrameH = baseEyes.Height / numFrames;
                Rectangle baseRect = new Rectangle(0, _eyeFrame * baseFrameH, baseEyes.Width, baseFrameH);
                Vector2 baseOrigin = new Vector2(baseRect.Width, baseRect.Height) / 2f;
                spriteBatch.Draw(baseEyes, position, baseRect, Color.White * alpha, 0f, baseOrigin, scale, SpriteEffects.None, 0f);
            }

            // Then draw face-set-specific Eyes5 glow overlay with alpha3 fade
            // Uses Additive blending so the glow brightens the base eyes without darkening white areas
            // Uses alpha3 to match SariaEyesGlowandFadedraw
            Color glowColor = Color.White;
            glowColor = Color.Lerp(glowColor, Color.FloralWhite, 30f);
            glowColor = Color.Lerp(glowColor, Color.Transparent, SariaModUtilities.alpha3);
            glowColor *= alpha;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            spriteBatch.Draw(eyeTexture, position, sourceRect, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
        }
        else if (_sariaTransform == 2)
        {
            // Form 3: NonPremultiplied for correct transparent pixels on Default-Eyes3
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            spriteBatch.Draw(eyeTexture, position, sourceRect, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
        }
        else if (_sariaTransform == 6)
        {
            // Form 7: old-style broad glow (constant) + new static ripples & jitter (speaking)
            Rectangle singleRect = new Rectangle(0, 0, eyeTexture.Width, eyeTexture.Height);
            Vector2 singleOrigin = new Vector2(singleRect.Width, singleRect.Height) / 2f;

            // Speaking intensity: drives blend toward new effect + jitter
            float jitterTarget = _isSpeakingThisFrame ? 1f : 0f;
            _poeWaveStrength = MathHelper.Lerp(_poeWaveStrength, jitterTarget, 0.05f);

            // Phase always advances so the glow never freezes
            _poeWavePhase += 0.09f;
            if (_poeWavePhase > MathF.PI * 20f)
                _poeWavePhase -= MathF.PI * 20f;

            {
                int texH = eyeTexture.Height;
                int texW = eyeTexture.Width;
                float topLeftX = position.X - singleOrigin.X * scale;
                float topLeftY = position.Y - singleOrigin.Y * scale;

                ulong jitterSeed = Main.GameUpdateCount;

                for (int row = 0; row < texH; row++)
                {
                    float t = 1f - (float)row / texH;

                    // Base glow: dense static ripples, always on
                    float baseWave = MathF.Sin(_poeWavePhase + t * MathF.PI * 5f);
                    float baseAlpha = 1f - 0.45f * (baseWave * 0.5f + 0.5f);

                    // Speaking glow: broad sweeping ripples
                    float speakWave = MathF.Sin(_poeWavePhase + t * MathF.PI * 2.5f);
                    float speakAlpha = 1f - 0.6f * (speakWave * 0.5f + 0.5f);

                    // Blend: idle = old glow, speaking = new glow
                    float rowAlpha = MathHelper.Lerp(baseAlpha, speakAlpha, _poeWaveStrength);

                    // Horizontal jitter: only when speaking
                    float jitterAmount = _poeWaveStrength * 0.6f;
                    ulong rowSeed = jitterSeed * 31ul + (ulong)row * 7ul;
                    float jitter = ((float)(rowSeed % 1000u) / 500f - 1f) * jitterAmount * scale;

                    Rectangle rowRect = new Rectangle(0, row, texW, 1);
                    Vector2 rowPos = new Vector2(topLeftX + jitter, topLeftY + row * scale);
                    Color rowColor = Color.White * (rowAlpha * alpha);

                    spriteBatch.Draw(eyeTexture, rowPos, rowRect, rowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
        }
        else
        {
            spriteBatch.Draw(eyeTexture, position, sourceRect, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawMouth(SpriteBatch spriteBatch, Vector2 position, float scale, string faceSetName, float alpha)
    {
            // Seventh form (transform 6) has no mouth
            if (_sariaTransform == 6) return;

        Texture2D mouthTexture = DialogueFaceSetRegistry.TryResolveMouthTexture(faceSetName, _sariaTransform);
        if (mouthTexture == null) return;

        int numFrames = 5;
        int frameHeight = mouthTexture.Height / numFrames;
        Rectangle sourceRect = new Rectangle(0, _mouthFrame * frameHeight, mouthTexture.Width, frameHeight);
            Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;

        spriteBatch.Draw(mouthTexture, position, sourceRect, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    private void DrawDialogueText(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha)
    {
        if (_coloredText.Count == 0) return;

        Vector2 textStart = panelPos + GetTextOffset() * scale;

        float baseTextScale = TextScale * scale;

        float maxHeightPx = GetTextMaxHeight() * scale;

        int totalLines = _wrappedLines?.Count ?? 0;
        float lineHeightAtBase = LineHeightBase * scale;
        float textScale = ComputeTextScaleToFit(baseTextScale, lineHeightAtBase, totalLines, maxHeightPx);

        float lineHeight = LineHeightBase * (textScale / baseTextScale) * scale;
        DynamicSpriteFont font = FontAssets.MouseText.Value;

        Vector2 currentPos = textStart;
        int charIndex = 0;

        foreach (var line in _wrappedLines)
        {
            float xOffset = 0;
            foreach (var cc in line)
            {
                if (charIndex >= _coloredText.Count) return;
                string charStr = cc.Character.ToString();
                Vector2 charSize = font.MeasureString(charStr) * textScale;

                Utils.DrawBorderString(spriteBatch, charStr, currentPos + new Vector2(xOffset, 0), _coloredText[charIndex].TextColor * alpha, textScale, 0f, 0f);
                xOffset += charSize.X;
                charIndex++;
            }
            currentPos.Y += lineHeight;
        }
    }

    private void DrawButtons(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha, bool labelsVisible)
    {
            // Always draw the exit button; disable/grey it if not enabled.
            DrawButton(spriteBatch, "SariaMod/Items/zTalking/ExitChoiceUI",
                panelPos + ExitButtonOffset * scale, labelsVisible ? "" : null, _hoveredButton == 4, _buttonEnabled[4] && !_isEnding, 3, scale, alpha);

            DrawButton(spriteBatch, "SariaMod/Items/zTalking/BackChoiceUI",
                panelPos + BackButtonOffset * scale, labelsVisible ? "" : null, _hoveredButton == 3, _buttonEnabled[3], 3, scale, alpha);

            DrawButtonWithWrappedText(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + Button1Offset * scale,
                labelsVisible ? (_currentNode?.Button1Label ?? "") : "",
                _hoveredButton == 0, _buttonEnabled[0], 3, scale, alpha, labelsVisible);

            DrawButtonWithWrappedText(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + Button2Offset * scale,
                labelsVisible ? (_currentNode?.Button2Label ?? "") : "",
                _hoveredButton == 1, _buttonEnabled[1], 3, scale, alpha, labelsVisible);

            DrawButtonWithWrappedText(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + Button3Offset * scale,
                labelsVisible ? (_currentNode?.Button3Label ?? "") : "",
                _hoveredButton == 2, _buttonEnabled[2], 3, scale, alpha, labelsVisible);
    }

    private void DrawButton(SpriteBatch spriteBatch, string texturePath, Vector2 position, string label, bool isHovered, bool isEnabled, int numFrames, float scale, float alpha)
    {
        Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
        if (texture == null) return;

        int frameHeight = texture.Height / numFrames;
        int frameIndex;
        if (!isEnabled)
            frameIndex = 2;
        else if (isHovered)
            frameIndex = 1;
        else
            frameIndex = 0;

        Rectangle sourceRect = new Rectangle(0, frameIndex * frameHeight, texture.Width, frameHeight);
        Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;
        Color buttonColor = (isEnabled ? Color.White : Color.Gray * 0.6f) * alpha;

        spriteBatch.Draw(texture, position, sourceRect, buttonColor, 0f, origin, scale, SpriteEffects.None, 0f);

        if (!string.IsNullOrEmpty(label))
        {
            Vector2 labelPos = position;
            if (isHovered && isEnabled)
            {
                labelPos.X += 2 * scale;
                labelPos.Y += 2 * scale;
            }
            Color labelColor = (isEnabled ? Color.White : Color.Gray * 0.6f) * alpha;
            Utils.DrawBorderString(spriteBatch, label, labelPos, labelColor, 0.75f * scale, 0.5f, 0.5f);
        }
    }

    private void DrawButtonWithWrappedText(SpriteBatch spriteBatch, string texturePath, Vector2 position, string label, bool isHovered, bool isEnabled, int numFrames, float scale, float alpha, bool showLabel)
    {
        Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
        if (texture == null) return;

        int frameHeight = texture.Height / numFrames;
        int frameIndex;
        if (!isEnabled)
            frameIndex = 2;
        else if (isHovered)
            frameIndex = 1;
        else
            frameIndex = 0;

        Rectangle sourceRect = new Rectangle(0, frameIndex * frameHeight, texture.Width, frameHeight);
        Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;
        Color buttonColor = (isEnabled ? Color.White : Color.Gray * 0.6f) * alpha;

        spriteBatch.Draw(texture, position, sourceRect, buttonColor, 0f, origin, scale, SpriteEffects.None, 0f);

        if (!showLabel || string.IsNullOrEmpty(label)) return;

        Vector2 labelPos = position;
        if (isHovered && isEnabled)
        {
            labelPos.X += 2 * scale;
            labelPos.Y += 2 * scale;
        }

        Color labelColor = (isEnabled ? Color.White : Color.Gray * 0.6f) * alpha;
        DynamicSpriteFont font = FontAssets.MouseText.Value;

        float maxWidth = ButtonLabelMaxWidth * scale;
        float maxHeight = 26 * scale;

        // Desired layout:
        // - 1 word: centered
        // - 2 words: stack unless they fit side-by-side
        // - 3+ words: normal word wrap
        string[] words = label.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        float baseScale = ButtonLabelScale * scale;
        float finalScale = baseScale;

        List<string> lines = new List<string>();

        if (words.Length == 1)
        {
            lines.Add(words[0]);
        }
        else if (words.Length == 2)
        {
            string sideBySide = words[0] + " " + words[1];
            if (font.MeasureString(sideBySide).X * baseScale <= maxWidth)
            {
                lines.Add(sideBySide);
            }
            else
            {
                lines.Add(words[0]);
                lines.Add(words[1]);
            }
        }
        else
        {
            string current = "";
            foreach (var w in words)
            {
                string test = string.IsNullOrEmpty(current) ? w : current + " " + w;
                if (font.MeasureString(test).X * baseScale <= maxWidth)
                {
                    current = test;
                }
                else
                {
                    if (!string.IsNullOrEmpty(current))
                        lines.Add(current);
                    current = w;
                }
            }
            if (!string.IsNullOrEmpty(current))
                lines.Add(current);

            if (lines.Count > 5)
            {
                // Hard limit: collapse remainder into last line
                lines = lines.GetRange(0, 5);
            }
        }

        // Shrink to fit width/height as a centered block
        int attempts = 0;
        while (attempts < 8)
        {
            float lineHeight = ButtonLineHeight * scale * (finalScale / baseScale);
            float totalHeight = lines.Count * lineHeight;

            bool tooWide = false;
            foreach (var l in lines)
            {
                if (font.MeasureString(l).X * finalScale > maxWidth)
                {
                    tooWide = true;
                    break;
                }
            }

            if (!tooWide && totalHeight <= maxHeight)
                break;

            finalScale *= 0.9f;
            if (finalScale < ButtonLabelMinScale * scale)
            {
                finalScale = ButtonLabelMinScale * scale;
                break;
            }

            attempts++;
        }

        float actualLineHeight2 = ButtonLineHeight * scale * (finalScale / baseScale);
        float totalTextHeight = lines.Count * actualLineHeight2;
        float startY = labelPos.Y - (totalTextHeight / 2f) + (actualLineHeight2 / 2f);

        for (int i = 0; i < lines.Count; i++)
        {
            Vector2 linePos = new Vector2(labelPos.X, startY + i * actualLineHeight2);
            Utils.DrawBorderString(spriteBatch, lines[i], linePos, labelColor, finalScale, 0.5f, 0.5f);
        }
    }

    private Vector2 GetTextOffset()
    {
        try
        {
            return new Vector2(FairyConfig.Instance?.DialogueTextOffsetX ?? TextOffset.X,
                FairyConfig.Instance?.DialogueTextOffsetY ?? TextOffset.Y);
        }
        catch
        {
            return TextOffset;
        }
    }

    private float GetTextMaxWidth()
    {
        try { return FairyConfig.Instance?.DialogueTextMaxWidth ?? TextMaxWidth; }
        catch { return TextMaxWidth; }
    }

    private float GetTextMaxHeight()
    {
        try { return FairyConfig.Instance?.DialogueTextMaxHeight ?? 48f; }
        catch { return 48f; }
    }

    private float ComputeTextScaleToFit(float baseTextScale, float scaledLineHeight, int totalLines, float maxHeight)
    {
        if (totalLines <= 0)
            return baseTextScale;

        float clampMin = 0.45f;

        float fitForLines(int allowedLines)
        {
            float requiredHeight = allowedLines * scaledLineHeight;
            if (requiredHeight <= 0)
                return baseTextScale;

            float ratio = maxHeight / requiredHeight;
            return baseTextScale * MathHelper.Clamp(ratio, clampMin, 1f);
        }

        // Stage 1: if it already fits within 3 lines at base scale, keep it.
        if (totalLines <= 3)
            return baseTextScale;

        // Stage 2: shrink to fit within the configured height for 3 lines.
        float s3 = fitForLines(3);
        if (totalLines <= 4)
            return s3;

        // Stage 3: once text is small enough, allow an extra line if needed.
        float s4 = fitForLines(4);
        if (totalLines <= 5)
            return s4;

        // Stage 4: last resort allow 5 lines.
        return fitForLines(5);
    }
}

}
