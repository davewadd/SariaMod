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
    internal sealed class DialogueEditorUIState : UIState
    {
        private static readonly Vector2 DefaultPanelOffset = new(0, -40);

        private static readonly Vector2 FacePrevOffset = new(-170, -47);
        private static readonly Vector2 FaceNextOffset = new(-112, -47);

        private static readonly Vector2 TransformPrevOffset = new(-170, -65);
        private static readonly Vector2 TransformNextOffset = new(-112, -65);
        private const int MaxTransformIndex = 6;

        private static Vector2 _savedPanelPosition = Vector2.Zero;
        private static bool _hasCustomPosition;

        private Vector2 _currentPanelOffset;
        private bool _isDragging;
        private Vector2 _dragStartMouse;
        private Vector2 _dragStartPanel;

        private bool _isActive;

        private int _hoveredButton = -1;
        private bool _wasMouseDown;
        private bool _mouseDownThisFrame;
        private bool _mouseReleasedThisFrame;
        private int _clickCooldown;

        private int _faceIndex;
        private int _transformPreviewIndex;

        private Rectangle _facePrevHit;
        private Rectangle _faceNextHit;

        private Rectangle _transformPrevHit;
        private Rectangle _transformNextHit;

        // Hit rectangles for the per-button target boxes
        private Rectangle _exitBoxHit;
        private Rectangle _backBoxHit;
        private Rectangle _b1BoxHit;
        private Rectangle _b2BoxHit;
        private Rectangle _b3BoxHit;
        private Rectangle _autoBoxHit;

        // Button enable states
        private bool _enableBtn1 = true;
        private bool _enableBtn2 = true;
        private bool _enableBtn3 = true;
        private bool _enableBack = true;
        private bool _enableExit = true;

        // Target boxes beneath each button (comma-separated allowed)
        private string _btn1Targets = "";
        private string _btn2Targets = "";
        private string _btn3Targets = "";
        private string _backTargetsHint = "(back uses history)";
        private string _exitTargets = "";
        private string _autoAdvanceFrames = "0";
        private string _autoAdvanceTargets = "";
        private string _sequenceToken = "";

        private string _faceSetName = "Default";

        private readonly string[] _fieldNames = new[]
        {
            "Node ID",
            "Dialogue Text",
            "Face Set (Default/Sad/Angry/etc)",
            "Sequence Token (start-N/final-N)",
            "Exit Targets (csv, optional)",
            "Exit Enabled (true/false)",
            "AutoAdvance Frames (0=off)",
            "AutoAdvance Target (nodeId or Complete-N)",
            "Btn1 Label",
            "Btn1 Targets (csv)",
            "Btn1 Enabled (true/false)",
            "Btn2 Label",
            "Btn2 Targets (csv)",
            "Btn2 Enabled (true/false)",
            "Btn3 Label",
            "Btn3 Targets (csv)",
            "Btn3 Enabled (true/false)",
            "Back Enabled (true/false)",
            "Speed (default)",
            "Color (default)",
            "Animate Mouth (true/false)",
            "Priority (General/Cutscene)",
            "Cutscene Priority (number)"
        };

        private int _selectedFieldIndex;

        private string _nodeId = "created_node";
        private string _dialogueText = "Type dialogue here...";
        private string _b1Label = "";
        private string _b1Target = "";
        private string _b2Label = "";
        private string _b2Target = "";
        private string _b3Label = "";
        private string _b3Target = "";
        private string _defaultSpeed = "2";
        private string _defaultColor = "White";
        private string _animateMouth = "true";
        private string _timerFrames = "0";
        private string _priorityMode = "General";
        private string _cutscenePriority = "1";

        private string _statusLine = "";
        private int _statusTimer;

        // Inline section-based text editor panel (renders in dialogue text area)
        private EditorTextSectionPanel _textSectionPanel;

        private bool _isPreviewPlaying;
        private int _hoveredPreviewButton = -1;

        // Preview typewriter + animation state (mirrors DialogueUIState behavior)
        private readonly List<DialogueUIChar> _previewColoredText = new();
        private readonly List<List<DialogueUIChar>> _previewWrappedLines = new();
        private string _previewFullText = "";
        private int _previewCharIndex;
        private int _previewFrameCounter;
        private int _previewCurrentSpeed = 2;
        private int _previewWaitFrames;
        private bool _previewTextComplete;
        private int _previewCurrentLineIndex;
        private int _previewCurrentCharInLine;

        private bool _previewIsCurrentlySpeaking;
        private int _previewMouthFrame;
        private int _previewMouthAnimTimer;
        private int _previewEyeFrame;
        private int _previewEyeAnimTimer;
        private bool _previewIsBlinking;
        private int _previewBlinkFrameIndex;
        private int _previewNextBlinkTime;

        // Preview sound cooldown to prevent audio spam (scoped to Dialogue Creator)
        private int _previewSoundCooldown;

        // Poe eye wave effect (form 7) - preview version
        private float _previewPoeWavePhase = 0f;
        private float _previewPoeWaveStrength = 0f;

        private struct DialogueUIChar
        {
            public char Character;
            public Color TextColor;
            public bool IsSilent;
            public bool MouthEnabled;

            public DialogueUIChar(char c, Color color, bool silent, bool mouthEnabled)
            {
                Character = c;
                TextColor = color;
                IsSilent = silent;
                MouthEnabled = mouthEnabled;
            }
        }

        private static readonly Vector2 PreviewButtonOffset = new(181, -62);
        private static readonly Vector2 PreviewButtonSize = new(22, 22);

        private static readonly Vector2 AutoAdvanceBoxOffset = new(0, 126);
        private static readonly Vector2 AutoAdvanceBoxSize = new(240, 16);

        private static readonly Vector2 NodeFinderBoxOffset = new(0, 108);
        private static readonly Vector2 NodeFinderBoxSize = new(240, 16);

        private static readonly Vector2 SaveNewButtonOffset = new(-110, 72);
        private static readonly Vector2 SaveCompletedButtonOffset = new(110, 72);
        private static readonly Vector2 SaveOriginButtonOffset = new(0, 72);
        private static readonly Vector2 SaveButtonSize = new(100, 18);

        private Rectangle _nodeFinderHit;
        private Rectangle _saveNewHit;
        private Rectangle _saveCompletedHit;
        private Rectangle _saveOriginHit;

        private string _nodeFinderId = "";
        private FairyConfig.CreatedDialogueOutputMode? _loadedFrom;

        private bool _editingNodeFinder;

        // Bottom-docked button panel for Save/SaveAs/ExitCreator
        private EditorBottomButtonPanel _bottomButtonPanel;

        // Left-anchored button editor panel for configuring dialogue buttons
        private EditorButtonPanel _buttonEditorPanel;

        // Debug panel state - independent of main panel
        private static Vector2 _debugPanelOffset = new(250, -120);
        private static bool _hasDebugPanelPosition;
        private bool _isDraggingDebugPanel;
        private Vector2 _debugPanelDragStartMouse;
        private Vector2 _debugPanelDragStartOffset;
        private const float DebugPanelWidth = 320f;
        private const float DebugPanelHeight = 300f;
        private Rectangle _debugPanelHit;

        // Debug panel visibility toggle
        private bool _debugPanelVisible;
        private Rectangle _debugToggleButtonHit;
        private const float DebugToggleButtonWidth = 28f;
        private const float DebugToggleButtonHeight = 60f;

        private DialogueNode BuildNodeFromFields()
        {
            int.TryParse(_autoAdvanceFrames, out int timerFrames);
            int.TryParse(_cutscenePriority, out int csPriority);

            bool isCutscene = string.Equals(_priorityMode, "Cutscene", StringComparison.OrdinalIgnoreCase);
            bool animateMouth = !string.Equals(_animateMouth, "false", StringComparison.OrdinalIgnoreCase);

            bool enable1 = _enableBtn1;
            bool enable2 = _enableBtn2;
            bool enable3 = _enableBtn3;
            bool enableBack = _enableBack;

            var node = new DialogueNode(_nodeId?.Trim() ?? "", BuildDialogueFromSectionsOrRaw(), "Normal")
            {
                FaceSetName = string.IsNullOrWhiteSpace(_faceSetName) ? "Default" : _faceSetName.Trim(),
                SequenceToken = _sequenceToken ?? string.Empty,
                ExitTargetOverride = _exitTargets ?? string.Empty,
                DisableExitButton = !_enableExit,

                Button1Label = _b1Label ?? "",
                Button1Target = enable1 ? (_btn1Targets ?? "") : "",

                Button2Label = _b2Label ?? "",
                Button2Target = enable2 ? (_btn2Targets ?? "") : "",

                Button3Label = _b3Label ?? "",
                Button3Target = enable3 ? (_btn3Targets ?? "") : "",

                DisableBackButton = !enableBack,

                IsCutscene = isCutscene,
                CutscenePriority = isCutscene ? Math.Max(0, csPriority) : 0,
                AnimateMouth = animateMouth,

                AutoAdvanceFrames = Math.Max(0, timerFrames),
                AutoAdvanceTarget = _autoAdvanceTargets ?? string.Empty,
            };

            return node;
        }

        private string BuildDialogueFromSectionsOrRaw()
        {
            if (_textSectionPanel != null && _textSectionPanel.Sections.Count > 0)
                return _textSectionPanel.ComposeTaggedText();

            return ApplyDefaultTags(_dialogueText ?? "");
        }

        private string ApplyDefaultTags(string text)
        {
            var t = text ?? string.Empty;

            if (int.TryParse(_defaultSpeed, out int speed) && speed > 0)
                t = $"[speed:{speed}]" + t;

            if (!string.IsNullOrWhiteSpace(_defaultColor) && DialogueUIState.NamedColors.ContainsKey(_defaultColor.Trim()))
                t = $"[color:{_defaultColor.Trim()}]" + t;

            return t;
        }

        private float GetUIScale()
        {
            try { return FairyConfig.Instance?.DialogueUIScale ?? 1.5f; }
            catch { return 1.5f; }
        }

        public override void OnInitialize()
        {
            if (_hasCustomPosition)
                _currentPanelOffset = _savedPanelPosition;
            else
                _currentPanelOffset = DefaultPanelOffset;

            _previewNextBlinkTime = Main.rand.Next(120, 300);

            // Initialize inline section-based text editor
            _textSectionPanel = new EditorTextSectionPanel
            {
                OnStatusChanged = SetStatus
            };

            // Initialize bottom button panel
            _bottomButtonPanel = new EditorBottomButtonPanel
            {
                OnExitCreatorClicked = () =>
                {
                    Close();
                },
                OnSaveClicked = () => SaveCurrentNodeToOrigin(),
                OnSaveAsClicked = () => SaveCurrentNode(FairyConfig.CreatedDialogueOutputMode.CreatedDialogue)
            };

            // Initialize button editor panel with callbacks to editor state
            _buttonEditorPanel = new EditorButtonPanel
            {
                GetButtonEnabled = GetButtonEnabledByRow,
                SetButtonEnabled = SetButtonEnabledByRow,
                GetButtonTarget = GetButtonTargetByRow,
                SetButtonTarget = SetButtonTargetByRow,
                GetButtonLabel = GetButtonLabelByRow,
                SetButtonLabel = SetButtonLabelByRow,
                OnStatusChanged = SetStatus
            };
        }

        // Callback helpers for EditorButtonPanel
        private bool GetButtonEnabledByRow(int row)
        {
            return row switch
            {
                0 => _enableBtn1,
                1 => _enableBtn2,
                2 => _enableBtn3,
                3 => _enableBack,
                4 => _enableExit,
                _ => true
            };
        }

        private void SetButtonEnabledByRow(int row, bool enabled)
        {
            switch (row)
            {
                case 0: _enableBtn1 = enabled; break;
                case 1: _enableBtn2 = enabled; break;
                case 2: _enableBtn3 = enabled; break;
                case 3: _enableBack = enabled; break;
                case 4: _enableExit = enabled; break;
            }
        }

        private string GetButtonTargetByRow(int row)
        {
            return row switch
            {
                0 => _btn1Targets ?? "",
                1 => _btn2Targets ?? "",
                2 => _btn3Targets ?? "",
                3 => _backTargetsHint,
                4 => _exitTargets ?? "",
                _ => ""
            };
        }

        private void SetButtonTargetByRow(int row, string target)
        {
            switch (row)
            {
                case 0: _btn1Targets = target; break;
                case 1: _btn2Targets = target; break;
                case 2: _btn3Targets = target; break;
                // Back doesn't have editable target (uses history)
                case 4: _exitTargets = target; break;
            }
        }

        private string GetButtonLabelByRow(int row)
        {
            return row switch
            {
                0 => _b1Label ?? "",
                1 => _b2Label ?? "",
                2 => _b3Label ?? "",
                _ => ""
            };
        }

        private void SetButtonLabelByRow(int row, string label)
        {
            switch (row)
            {
                case 0: _b1Label = label; break;
                case 1: _b2Label = label; break;
                case 2: _b3Label = label; break;
            }
        }

        public void Open()
        {
            _isActive = true;
            if (_hasCustomPosition)
                _currentPanelOffset = _savedPanelPosition;
            else
                _currentPanelOffset = DefaultPanelOffset;

            // Start at form 1 (index 0) - transform is controlled by the editor's purple buttons
            _transformPreviewIndex = 0;

            // Defaults for newly created nodes
            if (string.IsNullOrWhiteSpace(_autoAdvanceFrames))
                _autoAdvanceFrames = "0";
            if (_autoAdvanceTargets == null)
                _autoAdvanceTargets = "";
            _enableExit = true;

            _clickCooldown = 0;
            _wasMouseDown = false;
            _isDragging = false;
            SetStatus("Editor opened. Click any field area to edit.");
        }

        public void Close()
        {
            _savedPanelPosition = _currentPanelOffset;
            _hasCustomPosition = true;
            _isActive = false;
        }

        public bool IsActive => _isActive;

        public override void Update(GameTime gameTime)
        {
            if (!_isActive)
                return;
            if (Main.gamePaused)
                return;

            base.Update(gameTime);
            Main.LocalPlayer.mouseInterface = true;

            // Block game movement/inventory keys while typing in the section editor
            if (_textSectionPanel?.IsTyping == true)
                Main.drawingPlayerChat = true;

            bool mouseDown = Main.mouseLeft;
            _mouseDownThisFrame = mouseDown && !_wasMouseDown;
            _mouseReleasedThisFrame = !mouseDown && _wasMouseDown;

            if (_clickCooldown > 0) _clickCooldown--;
            if (_statusTimer > 0) _statusTimer--;

            // Transform is now controlled by the editor's purple buttons, no auto-sync

            // Update bottom button panel layout and hover
            _bottomButtonPanel?.UpdateLayout();
            _bottomButtonPanel?.UpdateHover();

            // Update button editor panel and text section panel layout (positioned relative to main panel)
            if (!_isPreviewPlaying)
            {
                float scale = GetUIScale();
                Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
                Vector2 panelPos = screenCenter + _currentPanelOffset * scale;
                Vector2 adjustedPanelPos = panelPos + DialogueUIState.BackgroundOffset * scale;
                _buttonEditorPanel?.UpdateLayout(adjustedPanelPos, scale);
                _buttonEditorPanel?.UpdateHover();
                _textSectionPanel?.UpdateLayout(adjustedPanelPos, scale);
            }

            if (_isPreviewPlaying)
            {
                UpdatePreviewHover();
                UpdatePreviewClicks();
                UpdatePreviewTypewriter();
                UpdatePreviewEyeAnimation();
                UpdatePreviewMouthAnimation();

                _wasMouseDown = mouseDown;
                ClampToScreenBounds();
                return;
            }

            // Check bottom panel clicks first
            if (_mouseReleasedThisFrame && _clickCooldown <= 0 && _bottomButtonPanel?.ContainsMouse() == true)
            {
                if (_bottomButtonPanel.HandleClick())
                {
                    _clickCooldown = 15;
                    _wasMouseDown = mouseDown;
                    return;
                }
            }

            // Check button editor panel clicks
            if (_mouseReleasedThisFrame && _clickCooldown <= 0 && _buttonEditorPanel?.ContainsMouse() == true)
            {
                if (_buttonEditorPanel.HandleClick())
                {
                    _clickCooldown = 10;
                    _wasMouseDown = mouseDown;
                    return;
                }
            }

            // Check text section panel input (before hover/dragging so typing takes priority)
            if (_textSectionPanel != null)
            {
                if (_textSectionPanel.UpdateInput())
                {
                    _wasMouseDown = mouseDown;
                    ClampToScreenBounds();
                    return;
                }
            }

            UpdateHover();
            UpdateDragging(mouseDown);
            UpdateClicks();

            // Skip keyboard input when the text section panel is in typing mode
            if (_textSectionPanel?.IsTyping != true)
                UpdateKeyboardInput();

            _wasMouseDown = mouseDown;
            ClampToScreenBounds();
        }

        private void SetStatus(string msg)
        {
            _statusLine = msg;
            _statusTimer = 240;
        }

        private Rectangle CreateButtonRect(Vector2 panelPos, Vector2 offset, Vector2 size)
        {
            Vector2 pos = panelPos + offset;
            return new Rectangle((int)(pos.X - size.X / 2), (int)(pos.Y - size.Y / 2), (int)size.X, (int)size.Y);
        }

        private void ClampToScreenBounds()
        {
            float scale = GetUIScale();
            Vector2 screenCenter = new(Main.screenWidth, Main.screenHeight);
            screenCenter /= 2f;

            Vector2 panelPos = screenCenter + _currentPanelOffset * scale;

            float panelLeft = panelPos.X - (DialogueUIState.PanelWidth * scale / 2);
            float panelRight = panelPos.X + (DialogueUIState.PanelWidth * scale / 2);
            float panelTop = panelPos.Y - (DialogueUIState.PanelHeight * scale / 2);
            float panelBottom = panelPos.Y + (DialogueUIState.PanelHeight * scale / 2);

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

        private void UpdateHover()
        {
            if (_isDragging || _isDraggingDebugPanel)
            {
                _hoveredButton = -1;
                return;
            }

            // Skip if mouse is over bottom panel
            if (_bottomButtonPanel?.ContainsMouse() == true)
            {
                _hoveredButton = -1;
                return;
            }

            // Skip if mouse is over button editor panel
            if (_buttonEditorPanel?.ContainsMouse() == true)
            {
                _hoveredButton = -1;
                return;
            }

            float scale = GetUIScale();
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            Vector2 panelPos = screenCenter + _currentPanelOffset * scale;
            Vector2 adjustedPanelPos = panelPos + DialogueUIState.BackgroundOffset * scale;

            // Face set arrows just above the Greetings box area
            _facePrevHit = CreateButtonRect(adjustedPanelPos, FacePrevOffset * scale, new Vector2(22, 18) * scale);
            _faceNextHit = CreateButtonRect(adjustedPanelPos, FaceNextOffset * scale, new Vector2(22, 18) * scale);

            // Transform cycle arrows above the face arrows
            _transformPrevHit = CreateButtonRect(adjustedPanelPos, TransformPrevOffset * scale, new Vector2(22, 18) * scale);
            _transformNextHit = CreateButtonRect(adjustedPanelPos, TransformNextOffset * scale, new Vector2(22, 18) * scale);

            Rectangle autoBox = new Rectangle(
                (int)(adjustedPanelPos.X + AutoAdvanceBoxOffset.X * scale - (AutoAdvanceBoxSize.X * scale) / 2f),
                (int)(adjustedPanelPos.Y + AutoAdvanceBoxOffset.Y * scale - (AutoAdvanceBoxSize.Y * scale) / 2f),
                (int)(AutoAdvanceBoxSize.X * scale),
                (int)(AutoAdvanceBoxSize.Y * scale));
            _autoBoxHit = autoBox;

            Rectangle exitRect = CreateButtonRect(adjustedPanelPos, DialogueUIState.ExitButtonOffset * scale, DialogueUIState.SmallButtonSize * scale);
            Rectangle backRect = CreateButtonRect(adjustedPanelPos, DialogueUIState.BackButtonOffset * scale, DialogueUIState.SmallButtonSize * scale);
            Rectangle btn1Rect = CreateButtonRect(adjustedPanelPos, DialogueUIState.Button1Offset * scale, DialogueUIState.CustomButtonSize * scale);
            Rectangle btn2Rect = CreateButtonRect(adjustedPanelPos, DialogueUIState.Button2Offset * scale, DialogueUIState.CustomButtonSize * scale);
            Rectangle btn3Rect = CreateButtonRect(adjustedPanelPos, DialogueUIState.Button3Offset * scale, DialogueUIState.CustomButtonSize * scale);

            Rectangle nodeFinder = new Rectangle(
                (int)(adjustedPanelPos.X + NodeFinderBoxOffset.X * scale - (NodeFinderBoxSize.X * scale) / 2f),
                (int)(adjustedPanelPos.Y + NodeFinderBoxOffset.Y * scale - (NodeFinderBoxSize.Y * scale) / 2f),
                (int)(NodeFinderBoxSize.X * scale),
                (int)(NodeFinderBoxSize.Y * scale));
            _nodeFinderHit = nodeFinder;

            // Update debug toggle button hit rectangle (right edge, vertically centered)
            _debugToggleButtonHit = new Rectangle(
                (int)(Main.screenWidth - DebugToggleButtonWidth - 4),
                (int)(Main.screenHeight / 2f - DebugToggleButtonHeight / 2f),
                (int)DebugToggleButtonWidth,
                (int)DebugToggleButtonHeight);

            Point mousePos = new Point(Main.mouseX, Main.mouseY);
            int previousHover = _hoveredButton;
            _hoveredButton = -1;

            // Check debug toggle button first
            if (_debugToggleButtonHit.Contains(mousePos))
                _hoveredButton = 300;
            else if (exitRect.Contains(mousePos))
                _hoveredButton = 4;
            else if (backRect.Contains(mousePos))
                _hoveredButton = 3;
            else if (btn1Rect.Contains(mousePos))
                _hoveredButton = 0;
            else if (btn2Rect.Contains(mousePos))
                _hoveredButton = 1;
            else if (btn3Rect.Contains(mousePos))
                _hoveredButton = 2;
            else if (_transformPrevHit.Contains(mousePos))
                _hoveredButton = 104;
            else if (_transformNextHit.Contains(mousePos))
                _hoveredButton = 105;
            else if (_facePrevHit.Contains(mousePos))
                _hoveredButton = 100;
            else if (_faceNextHit.Contains(mousePos))
                _hoveredButton = 101;
            else if (_exitBoxHit.Contains(mousePos))
                _hoveredButton = 204;
            else if (_backBoxHit.Contains(mousePos))
                _hoveredButton = 203;
            else if (_b1BoxHit.Contains(mousePos))
                _hoveredButton = 200;
            else if (_b2BoxHit.Contains(mousePos))
                _hoveredButton = 201;
            else if (_b3BoxHit.Contains(mousePos))
                _hoveredButton = 202;
            else if (_autoBoxHit.Contains(mousePos))
                _hoveredButton = 205;
            else if (_nodeFinderHit.Contains(mousePos))
                _hoveredButton = 206;

            if (_hoveredButton != -1 && _hoveredButton != previousHover)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }

        private void UpdateDragging(bool mouseDown)
        {
            float scale = GetUIScale();
            Vector2 panelPos = new Vector2(Main.screenWidth, Main.screenHeight) / 2f + _currentPanelOffset * scale;
            Vector2 adjusted = panelPos + DialogueUIState.BackgroundOffset * scale;

            Rectangle panelRect = new(
                (int)(adjusted.X - DialogueUIState.PanelWidth * scale / 2),
                (int)(adjusted.Y - DialogueUIState.PanelHeight * scale / 2),
                (int)(DialogueUIState.PanelWidth * scale),
                (int)(DialogueUIState.PanelHeight * scale));

            Point mousePos = new(Main.mouseX, Main.mouseY);

            // Handle debug panel dragging first (takes priority if mouse is over it)
            UpdateDebugPanelDragging(mouseDown, mousePos, scale);

            // Skip main panel dragging if we're dragging the debug panel
            if (_isDraggingDebugPanel)
                return;

            if (_mouseDownThisFrame && _hoveredButton == -1 && panelRect.Contains(mousePos) && !_debugPanelHit.Contains(mousePos))
            {
                _isDragging = true;
                _dragStartMouse = new Vector2(Main.mouseX, Main.mouseY);
                _dragStartPanel = _currentPanelOffset;
            }

            if (_isDragging && mouseDown)
            {
                Vector2 currentMouse = new(Main.mouseX, Main.mouseY);
                Vector2 delta = (currentMouse - _dragStartMouse) / scale;
                _currentPanelOffset = _dragStartPanel + delta;
            }

            if (!mouseDown)
                _isDragging = false;
        }

        private void UpdateDebugPanelDragging(bool mouseDown, Point mousePos, float scale)
        {
            // Only allow dragging when panel is visible
            if (!_debugPanelVisible)
            {
                _isDraggingDebugPanel = false;
                return;
            }

            // Start dragging debug panel
            if (_mouseDownThisFrame && _debugPanelHit.Contains(mousePos) && !_isDraggingDebugPanel)
            {
                _isDraggingDebugPanel = true;
                _debugPanelDragStartMouse = new Vector2(Main.mouseX, Main.mouseY);
                _debugPanelDragStartOffset = _debugPanelOffset;
            }

            // Continue dragging debug panel
            if (_isDraggingDebugPanel && mouseDown)
            {
                Vector2 currentMouse = new(Main.mouseX, Main.mouseY);
                Vector2 delta = currentMouse - _debugPanelDragStartMouse;
                _debugPanelOffset = _debugPanelDragStartOffset + delta;
                _hasDebugPanelPosition = true;
            }

            // Stop dragging debug panel
            if (!mouseDown)
                _isDraggingDebugPanel = false;
        }

        private void UpdateClicks()
        {
            if (_clickCooldown > 0) return;
            if (_isDragging) return;

            float scale = GetUIScale();
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            Vector2 panelPos = screenCenter + _currentPanelOffset * scale;
            Vector2 adjustedPanelPos = panelPos + DialogueUIState.BackgroundOffset * scale;

            // Preview play button takes priority over editor fields.
            UpdateEditorPreviewButtonHover(adjustedPanelPos, scale);
            if (_hoveredPreviewButton == 0)
            {
                UpdateEditorPreviewButtonClick(adjustedPanelPos, scale);
                return;
            }

            if (_mouseReleasedThisFrame)
            {
                // Debug toggle button click
                if (_hoveredButton == 300)
                {
                    _debugPanelVisible = !_debugPanelVisible;
                    SetStatus(_debugPanelVisible ? "Debug panel shown" : "Debug panel hidden");
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    _clickCooldown = 10;
                    return;
                }

                if (_hoveredButton == 206)
                {
                    _editingNodeFinder = true;
                    SetStatus("Node Finder: type node id, Enter=load, Tab=exit");
                    _clickCooldown = 8;
                    return;
                }

                // Removed old save button handlers (210, 211, 212) - now handled by bottom panel

                if (_hoveredButton == 4)
                {
                    // This is the dialogue Exit button in the preview, NOT the Exit Creator button
                    // Just cycle field for editor purposes
                    _selectedFieldIndex = 4; // Exit Targets
                    SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]}");
                    _clickCooldown = 8;
                    return;
                }

                if (_hoveredButton == 204)
                {
                    if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
                        _selectedFieldIndex = 3; // Sequence Token
                    else if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
                        _selectedFieldIndex = 5; // Exit Enabled
                    else
                        _selectedFieldIndex = 4; // Exit Targets

                    SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]}");
                    _clickCooldown = 8;
                    return;
                }

                if (_hoveredButton == 203)
                {
                    _selectedFieldIndex = 17; // back enabled toggle
                    SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]} (Enter to toggle)");
                    _clickCooldown = 8;
                    return;
                }

                if (_hoveredButton == 200)
                {
                    _selectedFieldIndex = 9; // Btn1 targets
                    SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]}");
                    _clickCooldown = 8;
                    return;
                }

                if (_hoveredButton == 201)
                {
                    _selectedFieldIndex = 12; // Btn2 targets
                    SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]}");
                    _clickCooldown = 8;
                    return;
                }

                if (_hoveredButton == 202)
                {
                    _selectedFieldIndex = 15; // Btn3 targets
                    SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]}");
                    _clickCooldown = 8;
                    return;
                }

                // Back icon cycles to next field
                if (_hoveredButton == 3)
                {
                    _selectedFieldIndex = (_selectedFieldIndex + 1) % _fieldNames.Length;
                    SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]}");
                    _clickCooldown = 10;
                    return;
                }

                // Transform arrows
                if (_hoveredButton == 104)
                {
                    CycleTransform(-1);
                    _clickCooldown = 8;
                    return;
                }

                if (_hoveredButton == 105)
                {
                    CycleTransform(1);
                    _clickCooldown = 8;
                    return;
                }

                // Face arrows
                if (_hoveredButton == 100)
                {
                    CycleFaceSet(-1);
                    _clickCooldown = 8;
                    return;
                }

                if (_hoveredButton == 101)
                {
                    CycleFaceSet(1);
                    _clickCooldown = 8;
                    return;
                }

                // Clicking on any of the 3 buttons selects corresponding label field
                if (_hoveredButton == 0) _selectedFieldIndex = 8;
                if (_hoveredButton == 1) _selectedFieldIndex = 11;
                if (_hoveredButton == 2) _selectedFieldIndex = 14;

                if (_hoveredButton >= 0 && _hoveredButton <= 2)
                {
                    SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]} (type to edit)");
                    _clickCooldown = 8;
                    return;
                }

                // Main body click
                if (_hoveredButton == -1)
                {
                    _selectedFieldIndex = (_selectedFieldIndex + 1) % _fieldNames.Length;
                    SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]}");
                    _clickCooldown = 8;
                }
            }
        }

        private void UpdateKeyboardInput()
        {
            string typed = Main.GetInputText(string.Empty);
            if (string.IsNullOrEmpty(typed))
                return;

            // Handle button editor panel keyboard input first
            if (_buttonEditorPanel?.IsEditing == true)
            {
                if (_buttonEditorPanel.HandleKeyboardInput(typed))
                    return;
            }

            if (_editingNodeFinder)
            {
                if (typed.Equals("\t", StringComparison.Ordinal))
                {
                    _editingNodeFinder = false;
                    SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]}");
                    return;
                }

                if (typed.Equals("\r", StringComparison.Ordinal) || typed.Equals("\n", StringComparison.Ordinal))
                {
                    if (TryLoadNode(_nodeFinderId))
                        SetStatus($"Loaded node '{_nodeFinderId}'.");
                    else
                        SetStatus($"Node not found: '{_nodeFinderId}'.");
                    return;
                }

                if (typed.Equals("\b", StringComparison.Ordinal))
                {
                    if (!string.IsNullOrEmpty(_nodeFinderId))
                        _nodeFinderId = _nodeFinderId[..^1];
                    return;
                }

                _nodeFinderId += typed;
                return;
            }

            if (typed.Equals("\t", StringComparison.Ordinal))
            {
                _selectedFieldIndex = (_selectedFieldIndex + 1) % _fieldNames.Length;
                SetStatus($"Selected: {_fieldNames[_selectedFieldIndex]}");
                return;
            }

            if (typed.Equals("\r", StringComparison.Ordinal) || typed.Equals("\n", StringComparison.Ordinal))
            {
                // Toggle boolean fields with Enter
                if (_selectedFieldIndex == 5) { _enableExit = !_enableExit; SetStatus("Exit Enabled: " + _enableExit); return; }
                if (_selectedFieldIndex == 10) { _enableBtn1 = !_enableBtn1; SetStatus("Btn1 Enabled: " + _enableBtn1); return; }
                if (_selectedFieldIndex == 13) { _enableBtn2 = !_enableBtn2; SetStatus("Btn2 Enabled: " + _enableBtn2); return; }
                if (_selectedFieldIndex == 16) { _enableBtn3 = !_enableBtn3; SetStatus("Btn3 Enabled: " + _enableBtn3); return; }
                if (_selectedFieldIndex == 17) { _enableBack = !_enableBack; SetStatus("Back Enabled: " + _enableBack); return; }

                var node = BuildNodeFromFields();
                if (string.IsNullOrWhiteSpace(node.LocationID))
                {
                    SetStatus("Node ID is required.");
                    return;
                }

                // Enter key: save into NEW runtime store only
                NewDialogueDatabase.RegisterOrReplace(node);
                _loadedFrom = FairyConfig.CreatedDialogueOutputMode.CreatedDialogue;
                SetStatus($"Saved node '{node.LocationID}' (NEW runtime). Click 'Save New' to write file.");
                SoundEngine.PlaySound(SoundID.MenuOpen);
                return;
            }

            if (typed.Equals("\b", StringComparison.Ordinal))
            {
                Backspace();
                return;
            }

            AppendText(typed);
        }

        private void SaveCurrentNode(FairyConfig.CreatedDialogueOutputMode target)
        {
            var node = BuildNodeFromFields();
            if (string.IsNullOrWhiteSpace(node.LocationID))
            {
                SetStatus("Node ID is required.");
                return;
            }

            if (target == FairyConfig.CreatedDialogueOutputMode.CompletedConversationNodes)
                CompletedDialogueDatabase.RegisterOrReplace(node);
            else
                NewDialogueDatabase.RegisterOrReplace(node);

            try
            {
                var nodes = target == FairyConfig.CreatedDialogueOutputMode.CompletedConversationNodes
                    ? CompletedDialogueDatabase.AllNodes
                    : NewDialogueDatabase.AllNodes;

                CreatedDialogueIO.SaveToFile(nodes, target);
                SetStatus($"Saved '{node.LocationID}' -> {CreatedDialogueIO.GetOutputPathForMode(target)}");
                SoundEngine.PlaySound(SoundID.MenuOpen);
            }
            catch (Exception ex)
            {
                SetStatus("Save failed: " + ex.Message);
            }
        }

        private void SaveCurrentNodeToOrigin()
        {
            if (_loadedFrom == null)
            {
                SaveCurrentNode(FairyConfig.CreatedDialogueOutputMode.CreatedDialogue);
                return;
            }

            SaveCurrentNode(_loadedFrom.Value);
        }

        private bool TryLoadNode(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            id = id.Trim();

            if (CompletedDialogueDatabase.TryGet(id, out var completed))
            {
                LoadIntoFields(completed);
                _loadedFrom = FairyConfig.CreatedDialogueOutputMode.CompletedConversationNodes;
                return true;
            }

            if (NewDialogueDatabase.TryGet(id, out var created))
            {
                LoadIntoFields(created);
                _loadedFrom = FairyConfig.CreatedDialogueOutputMode.CreatedDialogue;
                return true;
            }

            // built-in fallback
            var builtIn = DialogueDatabase.GetNode(id);
            if (builtIn != null)
            {
                LoadIntoFields(builtIn);
                _loadedFrom = null;
                return true;
            }

            return false;
        }

        private void LoadIntoFields(DialogueNode node)
        {
            _nodeId = node.LocationID ?? "";
            _dialogueText = node.DialogueText ?? "";
            _faceSetName = string.IsNullOrWhiteSpace(node.FaceSetName) ? "Default" : node.FaceSetName;
            _sequenceToken = node.SequenceToken ?? string.Empty;
            _exitTargets = node.ExitTargetOverride ?? string.Empty;
            _enableExit = !node.DisableExitButton;

            _b1Label = node.Button1Label ?? "";
            _btn1Targets = node.Button1Target ?? "";
            _enableBtn1 = !string.IsNullOrWhiteSpace(_b1Label);

            _b2Label = node.Button2Label ?? "";
            _btn2Targets = node.Button2Target ?? "";
            _enableBtn2 = !string.IsNullOrWhiteSpace(_b2Label);

            _b3Label = node.Button3Label ?? "";
            _btn3Targets = node.Button3Target ?? "";
            _enableBtn3 = !string.IsNullOrWhiteSpace(_b3Label);

            _enableBack = !node.DisableBackButton;

            _animateMouth = node.AnimateMouth ? "true" : "false";
            _autoAdvanceFrames = node.AutoAdvanceFrames.ToString();
            _autoAdvanceTargets = node.AutoAdvanceTarget ?? string.Empty;
            _priorityMode = node.IsCutscene ? "Cutscene" : "General";
            _cutscenePriority = node.CutscenePriority.ToString();

            // Parse dialogue text into sections for the inline editor
            if (_textSectionPanel != null)
            {
                var sections = ParseDialogueIntoSections(node.DialogueText ?? "");
                _textSectionPanel.SetSections(sections);
            }
        }

        /// <summary>
        /// Parse a tagged dialogue string into EditorTextSection list.
        /// Recognizes [color:X], [speed:N], [/mouth], [mouth], [wait:N] tags.
        /// </summary>
        private static List<EditorTextSection> ParseDialogueIntoSections(string dialogueText)
        {
            var result = new List<EditorTextSection>();
            if (string.IsNullOrEmpty(dialogueText))
                return result;

            string currentColor = "White";
            int currentSpeed = 2;
            bool currentMouth = true;
            int currentWait = 0;
            string currentText = "";

            int i = 0;
            while (i < dialogueText.Length)
            {
                if (dialogueText[i] == '[')
                {
                    int tagEnd = dialogueText.IndexOf(']', i);
                    if (tagEnd > i)
                    {
                        string tag = dialogueText.Substring(i + 1, tagEnd - i - 1);

                        if (tag.StartsWith("color:", StringComparison.OrdinalIgnoreCase))
                        {
                            // Flush current text as a section before color change
                            if (!string.IsNullOrEmpty(currentText))
                            {
                                result.Add(new EditorTextSection
                                {
                                    Text = currentText,
                                    Color = currentColor,
                                    Speed = currentSpeed,
                                    Mouth = currentMouth,
                                    WaitFrames = currentWait
                                });
                                currentText = "";
                                currentWait = 0;
                            }
                            currentColor = tag.Substring(6);
                        }
                        else if (tag.StartsWith("speed:", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(currentText))
                            {
                                result.Add(new EditorTextSection
                                {
                                    Text = currentText,
                                    Color = currentColor,
                                    Speed = currentSpeed,
                                    Mouth = currentMouth,
                                    WaitFrames = currentWait
                                });
                                currentText = "";
                                currentWait = 0;
                            }
                            if (int.TryParse(tag.Substring(6), out int spd))
                                currentSpeed = spd;
                        }
                        else if (tag.Equals("/mouth", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(currentText))
                            {
                                result.Add(new EditorTextSection
                                {
                                    Text = currentText,
                                    Color = currentColor,
                                    Speed = currentSpeed,
                                    Mouth = currentMouth,
                                    WaitFrames = currentWait
                                });
                                currentText = "";
                                currentWait = 0;
                            }
                            currentMouth = false;
                        }
                        else if (tag.Equals("mouth", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(currentText))
                            {
                                result.Add(new EditorTextSection
                                {
                                    Text = currentText,
                                    Color = currentColor,
                                    Speed = currentSpeed,
                                    Mouth = currentMouth,
                                    WaitFrames = currentWait
                                });
                                currentText = "";
                                currentWait = 0;
                            }
                            currentMouth = true;
                        }
                        else if (tag.StartsWith("wait:", StringComparison.OrdinalIgnoreCase))
                        {
                            if (int.TryParse(tag.Substring(5), out int wait))
                                currentWait = wait;
                        }

                        i = tagEnd + 1;
                        continue;
                    }
                }

                currentText += dialogueText[i];
                i++;
            }

            // Flush remaining text
            if (!string.IsNullOrEmpty(currentText))
            {
                result.Add(new EditorTextSection
                {
                    Text = currentText,
                    Color = currentColor,
                    Speed = currentSpeed,
                    Mouth = currentMouth,
                    WaitFrames = currentWait
                });
            }

            return result;
        }

        private Rectangle GetPreviewButtonRect(Vector2 panelPos, float scale)
        {
            Vector2 pos = panelPos + PreviewButtonOffset * scale;
            return new Rectangle((int)(pos.X - PreviewButtonSize.X * scale / 2f), (int)(pos.Y - PreviewButtonSize.Y * scale / 2f), (int)(PreviewButtonSize.X * scale), (int)(PreviewButtonSize.Y * scale));
        }

        private void DrawBackground(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            string panelPath = "SariaMod/Items/zTalking/SariaPanel";
            Texture2D panelTexture;
            try { panelTexture = ModContent.Request<Texture2D>(panelPath).Value; }
            catch { return; }

            if (panelTexture != null)
            {
                Vector2 origin = new Vector2(panelTexture.Width, panelTexture.Height) / 2f;
                spriteBatch.Draw(panelTexture, panelPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            if (_isPreviewPlaying)
                return;

            Vector2 footerPos = panelPos + new Vector2(0, 82) * scale;
            Utils.DrawBorderString(spriteBatch, "EDITOR: Type to edit | Enter=save runtime | Bottom buttons to save/exit", footerPos, Color.LightGray, 0.55f * scale, 0.5f, 0.5f);
        }

        private void DrawEditorGreetingPortrait(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            string greetingPath = $"SariaMod/Items/zTalking/Greetings{_transformPreviewIndex + 1}";
            Texture2D greetingTexture;
            try { greetingTexture = ModContent.Request<Texture2D>(greetingPath).Value; }
            catch { greetingTexture = ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Greetings1").Value; }

            if (greetingTexture != null)
            {
                Vector2 greetingPos = panelPos + DialogueUIState.GreetingPortraitOffset * scale;
                Vector2 origin = new Vector2(greetingTexture.Width, greetingTexture.Height) / 2f;
                spriteBatch.Draw(greetingTexture, greetingPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        private void DrawEditorGreetingOverHead(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            if (_transformPreviewIndex == 1)
            {
                string overHeadPath = "SariaMod/Items/zTalking/Greetings2OverHead";
                Texture2D overHeadTexture;
                try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
                catch { return; }

                if (overHeadTexture != null)
                {
                    Vector2 overHeadPos = panelPos + DialogueUIState.GreetingPortraitOffset * scale;
                    Vector2 origin = new Vector2(overHeadTexture.Width, overHeadTexture.Height) / 2f;
                    spriteBatch.Draw(overHeadTexture, overHeadPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
                }
            }
            else if (_transformPreviewIndex == 2)
            {
                string overHeadPath = "SariaMod/Items/zTalking/Greetings3OverHead";
                Texture2D overHeadTexture;
                try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
                catch { return; }

                if (overHeadTexture != null)
                {
                    Vector2 overHeadPos = panelPos + DialogueUIState.GreetingPortraitOffset * scale;
                    Vector2 origin = new Vector2(overHeadTexture.Width, overHeadTexture.Height) / 2f;

                    ulong randShakeEffect = (Main.GameUpdateCount / 8) ^ (ulong)((long)overHeadPos.Y << 20 | (long)(uint)overHeadPos.X);
                    float shakeX = Utils.RandomInt(ref randShakeEffect, -4, -3) * 0.07f;
                    float shakeY = Utils.RandomInt(ref randShakeEffect, -4, 3) * 0.07f;
                    Vector2 shimmerPos = overHeadPos + new Vector2(shakeY, shakeX) * scale;

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                    spriteBatch.Draw(overHeadTexture, shimmerPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                }
            }
            else if (_transformPreviewIndex == 3)
            {
                bool sparksActive = false;
                int sariaType = ModContent.ProjectileType<Saria>();
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj != null && proj.active && proj.type == sariaType && proj.owner == Main.myPlayer && proj.ModProjectile is Saria sparksSaria)
                    {
                        sparksActive = sparksSaria.SpecialAnimateValue > 0;
                        break;
                    }
                }

                if (sparksActive)
                {
                    string sparksPath = "SariaMod/Items/zTalking/SariaSparksPortrait";
                    Texture2D sparksTexture;
                    try { sparksTexture = ModContent.Request<Texture2D>(sparksPath).Value; }
                    catch { sparksTexture = null; }

                    if (sparksTexture != null)
                    {
                        int sparksFrame = (int)Main.GameUpdateCount / 3 % 14;
                        Rectangle sparksRect = sparksTexture.Frame(verticalFrames: 14, frameY: sparksFrame);
                        Vector2 sparksOrigin = sparksRect.Size() / 2f;
                        Vector2 sparksPos = panelPos + DialogueUIState.SparksPortraitOffset * scale;

                        Color sparksColor = Color.Lerp(Color.White, Color.LightBlue, 2f);
                        sparksColor *= 0.85f;

                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                        spriteBatch.Draw(sparksTexture, sparksPos, sparksRect, sparksColor, 0f, sparksOrigin, scale, SpriteEffects.None, 0f);

                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                    }
                }
            }
            else if (_transformPreviewIndex == 4)
            {
                string overHeadPath = "SariaMod/Items/zTalking/Greetings5OverHead";
                Texture2D overHeadTexture;
                try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
                catch { overHeadTexture = null; }

                if (overHeadTexture != null)
                {
                    Vector2 overHeadPos = panelPos + DialogueUIState.GreetingPortraitOffset * scale;
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

                string overHeadPath2 = "SariaMod/Items/zTalking/Greetings5OverHead2";
                Texture2D overHeadTexture2;
                try { overHeadTexture2 = ModContent.Request<Texture2D>(overHeadPath2).Value; }
                catch { overHeadTexture2 = null; }

                if (overHeadTexture2 != null)
                {
                    Vector2 overHeadPos2 = panelPos + DialogueUIState.GreetingPortraitOffset * scale;
                    Vector2 origin2 = new Vector2(overHeadTexture2.Width, overHeadTexture2.Height) / 2f;

                    Color drawColor2 = Color.White;
                    drawColor2 = Color.Lerp(drawColor2, Color.FloralWhite, 30f);
                    drawColor2 = Color.Lerp(drawColor2, Color.Transparent, SariaModUtilities.alpha3);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                    spriteBatch.Draw(overHeadTexture2, overHeadPos2, null, drawColor2, 0f, origin2, scale, SpriteEffects.None, 0f);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                }
            }
        }

        private void DrawFieldOverlay(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            // Always draw the toggle button
            DrawDebugToggleButton(spriteBatch);

            // Only draw the debug panel if visible
            if (!_debugPanelVisible)
            {
                // Still draw status line even when panel is hidden
                if (_statusTimer > 0 && !string.IsNullOrEmpty(_statusLine))
                {
                    Vector2 pos = panelPos + new Vector2(0, -95) * scale;
                    Utils.DrawBorderString(spriteBatch, _statusLine, pos, Color.Cyan * 0.9f, 0.6f * scale, 0.5f, 0.5f);
                }
                return;
            }

            // Calculate debug panel position (independent of main panel)
            float debugPanelX = panelPos.X + _debugPanelOffset.X;
            float debugPanelY = panelPos.Y + _debugPanelOffset.Y;
            
            // Update hit rectangle for debug panel dragging
            _debugPanelHit = new Rectangle(
                (int)debugPanelX,
                (int)debugPanelY,
                (int)DebugPanelWidth,
                (int)DebugPanelHeight);

            // Draw translucent background panel
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color panelBgColor = new Color(0, 0, 0, 100); // Mostly translucent black
            Color panelBorderColor = new Color(80, 80, 80, 150);
            
            // Panel background
            spriteBatch.Draw(pixel, _debugPanelHit, panelBgColor);
            
            // Panel border
            spriteBatch.Draw(pixel, new Rectangle(_debugPanelHit.X, _debugPanelHit.Y, _debugPanelHit.Width, 2), panelBorderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugPanelHit.X, _debugPanelHit.Bottom - 2, _debugPanelHit.Width, 2), panelBorderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugPanelHit.X, _debugPanelHit.Y, 2, _debugPanelHit.Height), panelBorderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugPanelHit.Right - 2, _debugPanelHit.Y, 2, _debugPanelHit.Height), panelBorderColor);

            // Draw panel title/drag hint
            Color titleColor = _isDraggingDebugPanel ? Color.Yellow : Color.Gray;
            Utils.DrawBorderString(spriteBatch, "[ Node Fields - Drag to Move ]", 
                new Vector2(_debugPanelHit.Center.X, _debugPanelHit.Y + 10), 
                titleColor, 0.45f, 0.5f, 0f);

            // Position text inside the panel with padding
            const float paddingX = 8f;
            const float paddingY = 24f; // Extra top padding for title
            Vector2 textStart = new Vector2(debugPanelX + paddingX, debugPanelY + paddingY);
            
            float lineHeight = 11f;
            float txtScale = 0.5f;

            string dialoguePreview = (_selectedFieldIndex == 1 || (_textSectionPanel?.Sections.Count ?? 0) > 0)
                ? BuildDialogueFromSectionsOrRaw()
                : _dialogueText;

            string[] values = new[]
            {
                _nodeId,
                dialoguePreview,
                _faceSetName,
                _sequenceToken,
                _exitTargets,
                _enableExit.ToString(),
                _autoAdvanceFrames,
                _autoAdvanceTargets,
                _b1Label,
                _btn1Targets,
                _enableBtn1.ToString(),
                $"" ,
                _b2Label,
                _btn2Targets,
                _enableBtn2.ToString(),
                $"" ,
                _b3Label,
                _btn3Targets,
                _enableBtn3.ToString(),
                _enableBack.ToString(),
                _defaultSpeed,
                _defaultColor,
                _animateMouth,
                _priorityMode,
                _cutscenePriority
            };

            for (int i = 0; i < _fieldNames.Length; i++)
            {
                Color nameColor = i == _selectedFieldIndex ? Color.Yellow : Color.LightGray;
                string line = $"{_fieldNames[i]}: {values[i]}";

                if (i == 1 && line.Length > 50)
                    line = line.Substring(0, 50) + "...";
                else if (line.Length > 55)
                    line = line.Substring(0, 55) + "...";
                
                // Truncate long lines with ellipsis
                if (line.Length > 60)
                    line = line.Substring(0, 60) + "...";

                Utils.DrawBorderString(spriteBatch, line, textStart + new Vector2(0, i * lineHeight), nameColor, txtScale, 0f, 0f);
            }

            // Status line stays relative to main panel
            if (_statusTimer > 0 && !string.IsNullOrEmpty(_statusLine))
            {
                Vector2 pos = panelPos + new Vector2(0, -95) * scale;
                Utils.DrawBorderString(spriteBatch, _statusLine, pos, Color.Cyan * 0.9f, 0.6f * scale, 0.5f, 0.5f);
            }
        }

        private void DrawDebugToggleButton(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            // Button colors - blue theme
            Color bgColor = _debugPanelVisible ? new Color(40, 80, 140) : new Color(30, 60, 120);
            Color borderColor = new Color(60, 120, 180);
            
            // Highlight on hover
            if (_hoveredButton == 300)
            {
                bgColor = _debugPanelVisible ? new Color(60, 100, 160) : new Color(50, 80, 150);
                borderColor = new Color(100, 160, 220);
            }

            // Draw button background
            spriteBatch.Draw(pixel, _debugToggleButtonHit, bgColor);

            // Draw border
            spriteBatch.Draw(pixel, new Rectangle(_debugToggleButtonHit.X, _debugToggleButtonHit.Y, _debugToggleButtonHit.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugToggleButtonHit.X, _debugToggleButtonHit.Bottom - 2, _debugToggleButtonHit.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugToggleButtonHit.X, _debugToggleButtonHit.Y, 2, _debugToggleButtonHit.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugToggleButtonHit.Right - 2, _debugToggleButtonHit.Y, 2, _debugToggleButtonHit.Height), borderColor);

            // Draw arrow indicator (< when visible, > when hidden)
            string arrow = _debugPanelVisible ? ">" : "<";
            Vector2 arrowPos = new Vector2(_debugToggleButtonHit.Center.X, _debugToggleButtonHit.Center.Y);
            Utils.DrawBorderString(spriteBatch, arrow, arrowPos, Color.White, 0.8f, 0.5f, 0.5f);

            // Draw label vertically
            string label = "DBG";
            float labelY = _debugToggleButtonHit.Y + 8;
            for (int i = 0; i < label.Length; i++)
            {
                Utils.DrawBorderString(spriteBatch, label[i].ToString(), 
                    new Vector2(_debugToggleButtonHit.Center.X, labelY + i * 12), 
                    Color.LightBlue, 0.5f, 0.5f, 0f);
            }
        }

        private void UpdatePreviewHover()
        {
            float scale = GetUIScale();
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            Vector2 panelPos = screenCenter + _currentPanelOffset * scale;
            Vector2 adjustedPanelPos = panelPos + DialogueUIState.BackgroundOffset * scale;

            Rectangle playRect = GetPreviewButtonRect(adjustedPanelPos, scale);
            int prev = _hoveredPreviewButton;
            _hoveredPreviewButton = -1;

            if (playRect.Contains(new Point(Main.mouseX, Main.mouseY)))
                _hoveredPreviewButton = 0;

            if (_hoveredPreviewButton != -1 && _hoveredPreviewButton != prev)
                SoundEngine.PlaySound(SoundID.MenuTick);
        }

        private void UpdatePreviewClicks()
        {
            if (_clickCooldown > 0) return;
            if (!_mouseReleasedThisFrame) return;

            if (_hoveredPreviewButton == 0)
            {
                StopPreview();
                _clickCooldown = 10;
            }
        }

        private void UpdateEditorPreviewButtonHover(Vector2 panelPos, float scale)
        {
            Rectangle playRect = GetPreviewButtonRect(panelPos, scale);
            int prev = _hoveredPreviewButton;
            _hoveredPreviewButton = -1;

            if (playRect.Contains(new Point(Main.mouseX, Main.mouseY)))
                _hoveredPreviewButton = 0;

            if (_hoveredPreviewButton != -1 && _hoveredPreviewButton != prev)
                SoundEngine.PlaySound(SoundID.MenuTick);
        }

        private void UpdateEditorPreviewButtonClick(Vector2 panelPos, float scale)
        {
            if (_clickCooldown > 0) return;
            if (!_mouseReleasedThisFrame) return;

            Rectangle playRect = GetPreviewButtonRect(panelPos, scale);
            if (!playRect.Contains(new Point(Main.mouseX, Main.mouseY)))
                return;

            StartPreview();
            _clickCooldown = 10;
        }

        private void StartPreview()
        {
            _isPreviewPlaying = true;

            ResetPreviewTypewriter();
            _previewFullText = BuildDialogueFromSectionsOrRaw();
            PreprocessPreviewText();
        }

        private void StopPreview()
        {
            _isPreviewPlaying = false;
            _hoveredPreviewButton = -1;
            ResetPreviewTypewriter();
        }

        private void ResetPreviewTypewriter()
        {
            _previewColoredText.Clear();
            _previewWrappedLines.Clear();
            _previewFullText = "";
            _previewCharIndex = 0;
            _previewFrameCounter = 0;
            _previewCurrentSpeed = 2;
            _previewWaitFrames = 0;
            _previewTextComplete = false;
            _previewCurrentLineIndex = 0;
            _previewCurrentCharInLine = 0;
            _previewIsCurrentlySpeaking = false;
            _previewMouthFrame = 0;
            _previewMouthAnimTimer = 0;
            _previewEyeFrame = 0;
            _previewEyeAnimTimer = 0;
            _previewIsBlinking = false;
            _previewBlinkFrameIndex = 0;
            _previewNextBlinkTime = Main.rand.Next(120, 300);
            _previewSoundCooldown = 0;
        }

        private void PreprocessPreviewText()
        {
            _previewWrappedLines.Clear();
            if (string.IsNullOrEmpty(_previewFullText))
                return;

            float scale = GetUIScale() * DialogueUIState.TextScale;
            float maxWidth = GetTextMaxWidth() * GetUIScale();
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            List<DialogueUIChar> allChars = new();
            Color currentColor = Color.White;
            bool currentSilent = false;
            bool currentMouthEnabled = true;
            int i = 0;

            while (i < _previewFullText.Length)
            {
                char c = _previewFullText[i];
                if (c == '[')
                {
                    int tagEnd = _previewFullText.IndexOf(']', i);
                    if (tagEnd > i)
                    {
                        string tag = _previewFullText.Substring(i + 1, tagEnd - i - 1);
                        i = tagEnd + 1;

                        if (tag.StartsWith("color:", StringComparison.OrdinalIgnoreCase))
                        {
                            string colorName = tag.Substring(6);
                            if (DialogueUIState.NamedColors.TryGetValue(colorName, out Color newColor))
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

                allChars.Add(new DialogueUIChar(c, currentColor, currentSilent, currentMouthEnabled));
                i++;
            }

            List<DialogueUIChar> currentLine = new();
            List<DialogueUIChar> currentWord = new();
            float currentLineWidth = 0;

            float measureChar(char ch) => font.MeasureString(ch.ToString()).X * scale;

            float measureWord(List<DialogueUIChar> word)
            {
                float width = 0;
                foreach (var cc in word)
                    width += measureChar(cc.Character);
                return width;
            }

            foreach (var cc in allChars)
            {
                if (cc.Character == ' ')
                {
                    float wordWidth = measureWord(currentWord);
                    if (currentLineWidth + wordWidth > maxWidth && currentLine.Count > 0)
                    {
                        _previewWrappedLines.Add(new List<DialogueUIChar>(currentLine));
                        currentLine.Clear();
                        currentLineWidth = 0;
                    }

                    currentLine.AddRange(currentWord);
                    currentLineWidth += wordWidth;
                    currentLine.Add(cc);
                    currentLineWidth += measureChar(' ');
                    currentWord.Clear();
                }
                else if (cc.Character == '\n')
                {
                    currentLine.AddRange(currentWord);
                    _previewWrappedLines.Add(new List<DialogueUIChar>(currentLine));
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
                float wordWidth = measureWord(currentWord);
                if (currentLineWidth + wordWidth > maxWidth && currentLine.Count > 0)
                {
                    _previewWrappedLines.Add(new List<DialogueUIChar>(currentLine));
                    currentLine.Clear();
                }
                currentLine.AddRange(currentWord);
            }

            if (currentLine.Count > 0)
                _previewWrappedLines.Add(currentLine);
        }

        private void UpdatePreviewTypewriter()
        {
            if (_previewTextComplete || _previewWrappedLines.Count == 0)
                return;

            // Decrement sound cooldown
            if (_previewSoundCooldown > 0)
                _previewSoundCooldown--;

            if (_previewWaitFrames > 0)
            {
                _previewWaitFrames--;
                _previewIsCurrentlySpeaking = false;
                return;
            }

            _previewFrameCounter++;
            if (_previewFrameCounter < Math.Max(1, _previewCurrentSpeed))
                return;

            _previewFrameCounter = 0;

            if (_previewCurrentLineIndex < _previewWrappedLines.Count)
            {
                var line = _previewWrappedLines[_previewCurrentLineIndex];
                if (_previewCurrentCharInLine < line.Count)
                {
                    var cc = line[_previewCurrentCharInLine];
                    _previewColoredText.Add(cc);
                    _previewCurrentCharInLine++;

                    bool shouldSpeak = !cc.IsSilent && cc.MouthEnabled && cc.Character != ' ';
                    _previewIsCurrentlySpeaking = shouldSpeak;

                    // Play typewriter sound with smooth overlap and weighted pitch variance
                    if (shouldSpeak && _previewSoundCooldown <= 0)
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
                        _previewSoundCooldown = 3;
                    }
                }
                else
                {
                    _previewCurrentLineIndex++;
                    _previewCurrentCharInLine = 0;
                }
            }

            // Parse tags to control wait/speed during preview
            while (_previewCharIndex < _previewFullText.Length)
            {
                if (_previewFullText[_previewCharIndex] != '[') { _previewCharIndex++; break; }

                int tagEnd = _previewFullText.IndexOf(']', _previewCharIndex);
                if (tagEnd <= _previewCharIndex) break;

                string tag = _previewFullText.Substring(_previewCharIndex + 1, tagEnd - _previewCharIndex - 1);
                _previewCharIndex = tagEnd + 1;

                if (tag.StartsWith("wait:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(tag.Substring(5), out int waitTime))
                        _previewWaitFrames = waitTime;
                }
                else if (tag.StartsWith("speed:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(tag.Substring(6), out int newSpeed))
                        _previewCurrentSpeed = Math.Max(1, newSpeed);
                }
            }

            int totalChars = 0;
            foreach (var l in _previewWrappedLines) totalChars += l.Count;

            if (_previewColoredText.Count >= totalChars)
            {
                _previewTextComplete = true;
                _previewIsCurrentlySpeaking = false;
            }
        }

        private void UpdatePreviewEyeAnimation()
        {
            _previewEyeAnimTimer++;

            if (!_previewIsBlinking)
            {
                if (_previewEyeAnimTimer >= _previewNextBlinkTime)
                {
                    _previewIsBlinking = true;
                    _previewBlinkFrameIndex = 0;
                    _previewEyeAnimTimer = 0;
                }
            }
            else
            {
                if (_previewEyeAnimTimer >= 4)
                {
                    _previewEyeAnimTimer = 0;
                    _previewBlinkFrameIndex++;
                    if (_previewBlinkFrameIndex >= 4)
                    {
                        _previewIsBlinking = false;
                        _previewEyeFrame = 0;
                        _previewNextBlinkTime = Main.rand.Next(120, 300);
                    }
                    else
                    {
                        _previewEyeFrame = _previewBlinkFrameIndex;
                    }
                }
            }
        }

        private void UpdatePreviewMouthAnimation()
        {
            bool animateMouth = !string.Equals(_animateMouth, "false", StringComparison.OrdinalIgnoreCase);
            if (!animateMouth)
            {
                _previewMouthFrame = 0;
                _previewMouthAnimTimer = 0;
                return;
            }

            int baseSpeed = 6;
            int mouthSpeed = baseSpeed + Math.Max(0, (_previewCurrentSpeed - 2) * 2);

            bool shouldAnimate = !_previewTextComplete && _previewWaitFrames <= 0 && _previewIsCurrentlySpeaking;
            if (shouldAnimate)
            {
                _previewMouthAnimTimer++;
                if (_previewMouthAnimTimer >= mouthSpeed)
                {
                    _previewMouthAnimTimer = 0;
                    _previewMouthFrame = (_previewMouthFrame + 1) % 5;
                }
            }
            else
            {
                _previewMouthFrame = 0;
                _previewMouthAnimTimer = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_isActive)
                return;

            // Tick alpha pulse counters once per frame (frame-guarded, safe to call from anywhere)
            SariaModUtilities.UpdateAlphaCounters();

            // Switch to PointClamp for crisp pixel art rendering (all dialogue UI textures)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            float scale = GetUIScale();
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            Vector2 panelPos = screenCenter + _currentPanelOffset * scale;
            Vector2 adjustedPanelPos = panelPos + DialogueUIState.BackgroundOffset * scale;

            if (_isPreviewPlaying)
            {
                DrawPreview(spriteBatch, adjustedPanelPos, scale);

                // Restore default sampler state before returning to framework
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                base.Draw(spriteBatch);
                return;
            }

            // Layer 1: Background (SariaPanel)
            DrawBackground(spriteBatch, adjustedPanelPos, scale);

            // Layer 2: Greeting portrait
            DrawEditorGreetingPortrait(spriteBatch, adjustedPanelPos, scale);

            // Layer 3: Portrait (eyes, mouth, extra, sparks)
            DrawEditorPortrait(spriteBatch, adjustedPanelPos, scale);

            // Layer 4: Greeting overhead overlays (transform-specific effects)
            DrawEditorGreetingOverHead(spriteBatch, adjustedPanelPos, scale);

            // Layer 5: Editor overlays
            DrawTargetBoxes(spriteBatch, adjustedPanelPos, scale);
            DrawFieldOverlay(spriteBatch, adjustedPanelPos, scale);

            // Layer 5.5: Inline section text editor (renders in dialogue text area)
            _textSectionPanel?.Draw(spriteBatch, scale);

            // Layer 6: Dialogue buttons
            DrawButtons(spriteBatch, adjustedPanelPos, scale);

            // Layer 7: Face arrows
            DrawFaceArrows(spriteBatch, adjustedPanelPos, scale);

            // Layer 7.5: Transform arrows (above face arrows)
            DrawTransformArrows(spriteBatch, adjustedPanelPos, scale);

            // Layer 8: Preview toggle button
            DrawPreviewToggleButton(spriteBatch, adjustedPanelPos, scale);

            // Layer 9: Button editor panel
            _buttonEditorPanel?.Draw(spriteBatch, scale);

            // Layer 10: Bottom-docked buttons
            _bottomButtonPanel?.Draw(spriteBatch);

            // Restore default sampler state before returning to framework
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            base.Draw(spriteBatch);
        }

        private void DrawPreview(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            DrawBackground(spriteBatch, panelPos, scale);
            DrawEditorGreetingPortrait(spriteBatch, panelPos, scale);
            DrawPreviewPortrait(spriteBatch, panelPos, scale);
            DrawEditorGreetingOverHead(spriteBatch, panelPos, scale);
            DrawPreviewDialogueText(spriteBatch, panelPos, scale);
            DrawPreviewButtons(spriteBatch, panelPos, scale);
            DrawPreviewToggleButton(spriteBatch, panelPos, scale);
        }

        private void DrawPreviewButtons(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            DrawPreviewButton(spriteBatch, "SariaMod/Items/zTalking/ExitChoiceUI",
                panelPos + DialogueUIState.ExitButtonOffset * scale, "", _enableExit, 3, scale);

            DrawPreviewButton(spriteBatch, "SariaMod/Items/zTalking/BackChoiceUI",
                panelPos + DialogueUIState.BackButtonOffset * scale, "", _enableBack, 3, scale);

            DrawPreviewButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + DialogueUIState.Button1Offset * scale, _b1Label ?? "", _enableBtn1, 3, scale);

            DrawPreviewButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + DialogueUIState.Button2Offset * scale, _b2Label ?? "", _enableBtn2, 3, scale);

            DrawPreviewButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + DialogueUIState.Button3Offset * scale, _b3Label ?? "", _enableBtn3, 3, scale);
        }

        private void DrawPreviewButton(SpriteBatch spriteBatch, string texturePath, Vector2 position, String label, bool isEnabled, int numFrames, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
            if (texture == null) return;

            int frameHeight = texture.Height / numFrames;
            int frameIndex = isEnabled ? 0 : 2;

            Rectangle sourceRect = new(0, frameIndex * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new(sourceRect.Width, sourceRect.Height);
            origin /= 2f;
            Color buttonColor = isEnabled ? Color.White : Color.Gray * 0.6f;

            spriteBatch.Draw(texture, position, sourceRect, buttonColor, 0f, origin, scale, SpriteEffects.None, 0f);

            if (!string.IsNullOrEmpty(label))
            {
                Color labelColor = isEnabled ? Color.White : Color.Gray * 0.6f;
                Utils.DrawBorderString(spriteBatch, label, position, labelColor, 0.65f * scale, 0.5f, 0.5f);
            }
        }

        private void DrawButtons(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            DrawButton(spriteBatch, "SariaMod/Items/zTalking/ExitChoiceUI", panelPos + DialogueUIState.ExitButtonOffset * scale, "", _hoveredButton == 4, _enableExit, 3, scale);
            DrawButton(spriteBatch, "SariaMod/Items/zTalking/BackChoiceUI", panelPos + DialogueUIState.BackButtonOffset * scale, "", _hoveredButton == 3, _enableBack, 3, scale);

            DrawButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI", panelPos + DialogueUIState.Button1Offset * scale, _b1Label ?? "", _hoveredButton == 0, _enableBtn1, 3, scale);
            DrawButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI", panelPos + DialogueUIState.Button2Offset * scale, _b2Label ?? "", _hoveredButton == 1, _enableBtn2, 3, scale);
            DrawButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI", panelPos + DialogueUIState.Button3Offset * scale, _b3Label ?? "", _hoveredButton == 2, _enableBtn3, 3, scale);
        }

        private void DrawButton(SpriteBatch spriteBatch, string texturePath, Vector2 position, string label, bool isHovered, bool isEnabled, int numFrames, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
            int frameHeight = texture.Height / numFrames;
            int frameIndex = !isEnabled ? 2 : isHovered ? 1 : 0;

            Rectangle sourceRect = new(0, frameIndex * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new(sourceRect.Width, sourceRect.Height);
            origin /= 2f;
            Color buttonColor = isEnabled ? Color.White : Color.Gray * 0.6f;

            spriteBatch.Draw(texture, position, sourceRect, buttonColor, 0f, origin, scale, SpriteEffects.None, 0f);

            if (!string.IsNullOrEmpty(label))
                Utils.DrawBorderString(spriteBatch, label, position, Color.White, 0.65f * scale, 0.5f, 0.5f);
        }

        private void DrawFaceArrows(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color c = Color.LimeGreen * 0.9f;

            Vector2 prev = panelPos + FacePrevOffset * scale;
            Vector2 next = panelPos + FaceNextOffset * scale;

            DrawTriangle(spriteBatch, pixel, prev, 8 * scale, c, left: true);
            DrawTriangle(spriteBatch, pixel, next, 8 * scale, c, left: false);
        }

        private void DrawTransformArrows(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color c = new Color(160, 80, 220) * 0.9f;

            Vector2 prev = panelPos + TransformPrevOffset * scale;
            Vector2 next = panelPos + TransformNextOffset * scale;

            DrawSpiral(spriteBatch, pixel, prev, 8 * scale, c, left: true, _hoveredButton == 104);
            DrawSpiral(spriteBatch, pixel, next, 8 * scale, c, left: false, _hoveredButton == 105);

            // Draw form label between the two arrows
            Vector2 labelPos = panelPos + new Vector2((TransformPrevOffset.X + TransformNextOffset.X) / 2f, TransformPrevOffset.Y) * scale;
            Utils.DrawBorderString(spriteBatch, $"Form {_transformPreviewIndex + 1}", labelPos, new Color(200, 140, 255), 0.5f * scale, 0.5f, 0.5f);
        }

        private static void DrawTriangle(SpriteBatch sb, Texture2D pixel, Vector2 center, float size, Color color, bool left)
        {
            float dir = left ? -1f : 1f;
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-size * 0.5f)), (int)(center.Y - size * 0.5f), (int)(size * 0.75f), (int)(size * 0.2f)), color);
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-size * 0.25f)), (int)(center.Y - size * 0.15f), (int)(size * 0.9f), (int)(size * 0.2f)), color);
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (0)), (int)(center.Y + size * 0.2f), (int)(size * 0.75f), (int)(size * 0.2f)), color);
        }

        private static void DrawSpiral(SpriteBatch sb, Texture2D pixel, Vector2 center, float size, Color color, bool left, bool hovered)
        {
            Color drawColor = hovered ? Color.Lerp(color, Color.White, 0.35f) : color;
            float dir = left ? -1f : 1f;
            float s = size;

            // Outer arm
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-s * 0.5f)), (int)(center.Y - s * 0.55f), (int)(s * 0.85f), (int)Math.Max(1, s * 0.15f)), drawColor);
            // Curl down
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (s * 0.2f)), (int)(center.Y - s * 0.55f), (int)Math.Max(1, s * 0.15f), (int)(s * 0.5f)), drawColor);
            // Inner arm
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-s * 0.25f)), (int)(center.Y - s * 0.1f), (int)(s * 0.55f), (int)Math.Max(1, s * 0.15f)), drawColor);
            // Curl up to center
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-s * 0.25f)), (int)(center.Y - s * 0.1f), (int)Math.Max(1, s * 0.15f), (int)(s * 0.45f)), drawColor);
            // Center dot
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-s * 0.15f)), (int)(center.Y + s * 0.2f), (int)Math.Max(1, s * 0.2f), (int)Math.Max(1, s * 0.15f)), drawColor);
        }

        private static bool IsSariaActiveInWorld()
        {
            try
            {
                int sariaType = ModContent.ProjectileType<Saria>();
                if (sariaType <= 0)
                    return false;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj != null && proj.active && proj.type == sariaType && proj.owner == Main.myPlayer)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private void SyncTransformFromSaria()
        {
            try
            {
                int sariaType = ModContent.ProjectileType<Saria>();
                if (sariaType <= 0)
                    return;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj != null && proj.active && proj.type == sariaType && proj.owner == Main.myPlayer && proj.ModProjectile is Saria saria)
                    {
                        _transformPreviewIndex = saria.Transform;
                        return;
                    }
                }
            }
            catch
            {
            }
        }

        private Vector2 GetTextOffset()
        {
            try
            {
                return new Vector2(FairyConfig.Instance?.DialogueTextOffsetX ?? DialogueUIState.TextOffset.X,
                    FairyConfig.Instance?.DialogueTextOffsetY ?? DialogueUIState.TextOffset.Y);
            }
            catch
            {
                return DialogueUIState.TextOffset;
            }
        }

        private float GetTextMaxWidth()
        {
            try { return FairyConfig.Instance?.DialogueTextMaxWidth ?? DialogueUIState.TextMaxWidth; }
            catch { return DialogueUIState.TextMaxWidth; }
        }

        private float GetTextMaxHeight()
        {
            try { return FairyConfig.Instance?.DialogueTextMaxHeight ?? 48f; }
            catch { return 48f; }
        }

        private static float ComputeTextScaleToFit(float baseTextScale, float scaledLineHeight, int totalLines, float maxHeight)
        {
            if (totalLines <= 0)
                return baseTextScale;

            const float clampMin = 0.45f;

            float fitForLines(int allowedLines)
            {
                float requiredHeight = allowedLines * scaledLineHeight;
                if (requiredHeight <= 0)
                    return baseTextScale;

                float ratio = maxHeight / requiredHeight;
                return baseTextScale * MathHelper.Clamp(ratio, clampMin, 1f);
            }

            if (totalLines <= 3)
                return baseTextScale;

            float s3 = fitForLines(3);
            if (totalLines <= 4)
                return s3;

            float s4 = fitForLines(4);
            if (totalLines <= 5)
                return s4;

            return fitForLines(5);
        }

        private void CycleTransform(int delta)
        {
            _transformPreviewIndex = (_transformPreviewIndex + delta) % (MaxTransformIndex + 1);
            if (_transformPreviewIndex < 0) _transformPreviewIndex += MaxTransformIndex + 1;
            SetStatus($"Transform: Form {_transformPreviewIndex + 1}");
        }

        private void CycleFaceSet(int delta)
        {
            var names = DialogueFaceSetRegistry.RegisteredNamesInOrder;
            if (names == null || names.Count == 0)
            {
                _faceSetName = "Default";
                return;
            }

            string current = string.IsNullOrWhiteSpace(_faceSetName) ? "Default" : _faceSetName.Trim();

            int idx = -1;
            for (int i = 0; i < names.Count; i++)
            {
                if (string.Equals(names[i], current, StringComparison.OrdinalIgnoreCase))
                {
                    idx = i;
                    break;
                }
            }

            if (idx < 0)
                idx = 0;

            idx = (idx + delta) % names.Count;
            if (idx < 0) idx += names.Count;

            _faceSetName = names[idx];
            _selectedFieldIndex = 2;
            SetStatus($"Face Set: {_faceSetName}");
        }

        private void AppendText(string s)
        {
            switch (_selectedFieldIndex)
            {
                case 0: _nodeId = (_nodeId ?? string.Empty) + s; break;
                case 2: _faceSetName = (_faceSetName ?? string.Empty) + s; break;
                case 3: _sequenceToken = (_sequenceToken ?? string.Empty) + s; break;
                case 4: _exitTargets = (_exitTargets ?? string.Empty) + s; break;
                case 6: _autoAdvanceFrames = (_autoAdvanceFrames ?? string.Empty) + s; break;
                case 7: _autoAdvanceTargets = (_autoAdvanceTargets ?? string.Empty) + s; break;
                case 8: _b1Label = (_b1Label ?? string.Empty) + s; break;
                case 9: _btn1Targets = (_btn1Targets ?? string.Empty) + s; break;
                case 11: _b2Label = (_b2Label ?? string.Empty) + s; break;
                case 12: _btn2Targets = (_btn2Targets ?? string.Empty) + s; break;
                case 14: _b3Label = (_b3Label ?? string.Empty) + s; break;
                case 15: _btn3Targets = (_btn3Targets ?? string.Empty) + s; break;
                case 18: _defaultSpeed = (_defaultSpeed ?? string.Empty) + s; break;
                case 19: _defaultColor = (_defaultColor ?? string.Empty) + s; break;
                case 20: _animateMouth = (_animateMouth ?? string.Empty) + s; break;
                case 21: _priorityMode = (_priorityMode ?? string.Empty) + s; break;
                case 22: _cutscenePriority = (_cutscenePriority ?? string.Empty) + s; break;
                default: _dialogueText = (_dialogueText ?? string.Empty) + s; break;
            }
        }

        private void Backspace()
        {
            static string Pop(string v) => string.IsNullOrEmpty(v) ? v : v.Substring(0, v.Length - 1);

            switch (_selectedFieldIndex)
            {
                case 0: _nodeId = Pop(_nodeId); break;
                case 2: _faceSetName = Pop(_faceSetName); break;
                case 3: _sequenceToken = Pop(_sequenceToken); break;
                case 4: _exitTargets = Pop(_exitTargets); break;
                case 6: _autoAdvanceFrames = Pop(_autoAdvanceFrames); break;
                case 7: _autoAdvanceTargets = Pop(_autoAdvanceTargets); break;
                case 8: _b1Label = Pop(_b1Label); break;
                case 9: _btn1Targets = Pop(_btn1Targets); break;
                case 11: _b2Label = Pop(_b2Label); break;
                case 12: _btn2Targets = Pop(_btn2Targets); break;
                case 14: _b3Label = Pop(_b3Label); break;
                case 15: _btn3Targets = Pop(_btn3Targets); break;
                case 18: _defaultSpeed = Pop(_defaultSpeed); break;
                case 19: _defaultColor = Pop(_defaultColor); break;
                case 20: _animateMouth = Pop(_animateMouth); break;
                case 21: _priorityMode = Pop(_priorityMode); break;
                case 22: _cutscenePriority = Pop(_cutscenePriority); break;
                default: _dialogueText = Pop(_dialogueText); break;
            }
        }

        private static void DrawBox(SpriteBatch sb, Texture2D pixel, Rectangle r, Color border)
        {
            sb.Draw(pixel, r, Color.Black * 0.55f);
            sb.Draw(pixel, new Rectangle(r.X, r.Y, r.Width, 1), border * 0.9f);
            sb.Draw(pixel, new Rectangle(r.X, r.Bottom - 1, r.Width, 1), border * 0.9f);
            sb.Draw(pixel, new Rectangle(r.X, r.Y, 1, r.Height), border * 0.9f);
            sb.Draw(pixel, new Rectangle(r.Right - 1, r.Y, 1, r.Height), border * 0.9f);
        }

        private void DrawTargetBoxes(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Rectangle exitBox = new Rectangle((int)(panelPos.X + DialogueUIState.ExitButtonOffset.X * scale - 49 * scale), (int)(panelPos.Y + (DialogueUIState.ExitButtonOffset.Y + 30) * scale), (int)(98 * scale), (int)(14 * scale));
            Rectangle backBox = new Rectangle((int)(panelPos.X + DialogueUIState.BackButtonOffset.X * scale - 49 * scale), (int)(panelPos.Y + (DialogueUIState.BackButtonOffset.Y + 30) * scale), (int)(98 * scale), (int)(14 * scale));
            Rectangle b1Box = new Rectangle((int)(panelPos.X + DialogueUIState.Button1Offset.X * scale - 49 * scale), (int)(panelPos.Y + (DialogueUIState.Button1Offset.Y + 30) * scale), (int)(98 * scale), (int)(14 * scale));
            Rectangle b2Box = new Rectangle((int)(panelPos.X + DialogueUIState.Button2Offset.X * scale - 49 * scale), (int)(panelPos.Y + (DialogueUIState.Button2Offset.Y + 30) * scale), (int)(98 * scale), (int)(14 * scale));
            Rectangle b3Box = new Rectangle((int)(panelPos.X + DialogueUIState.Button3Offset.X * scale - 49 * scale), (int)(panelPos.Y + (DialogueUIState.Button3Offset.Y + 30) * scale), (int)(98 * scale), (int)(14 * scale));

            Rectangle autoBox = new Rectangle(
                (int)(panelPos.X + AutoAdvanceBoxOffset.X * scale - (AutoAdvanceBoxSize.X * scale) / 2f),
                (int)(panelPos.Y + AutoAdvanceBoxOffset.Y * scale - (AutoAdvanceBoxSize.Y * scale) / 2f),
                (int)(AutoAdvanceBoxSize.X * scale),
                (int)(AutoAdvanceBoxSize.Y * scale));

            Rectangle nodeFinder = new Rectangle(
                (int)(panelPos.X + NodeFinderBoxOffset.X * scale - (NodeFinderBoxSize.X * scale) / 2f),
                (int)(panelPos.Y + NodeFinderBoxOffset.Y * scale - (NodeFinderBoxSize.Y * scale) / 2f),
                (int)(NodeFinderBoxSize.X * scale),
                (int)(NodeFinderBoxSize.Y * scale));

            _exitBoxHit = exitBox;
            _backBoxHit = backBox;
            _b1BoxHit = b1Box;
            _b2BoxHit = b2Box;
            _b3BoxHit = b3Box;
            _autoBoxHit = autoBox;
            _nodeFinderHit = nodeFinder;

            DrawBox(spriteBatch, pixel, exitBox, Color.White);
            DrawBox(spriteBatch, pixel, backBox, Color.White);
            DrawBox(spriteBatch, pixel, b1Box, Color.White);
            DrawBox(spriteBatch, pixel, b2Box, Color.White);
            DrawBox(spriteBatch, pixel, b3Box, Color.White);
            DrawBox(spriteBatch, pixel, autoBox, Color.White);

            DrawBox(spriteBatch, pixel, nodeFinder, _editingNodeFinder ? Color.Yellow : (_hoveredButton == 206 ? Color.Cyan : Color.White));
            Utils.DrawBorderString(spriteBatch, "Find: " + (_nodeFinderId ?? "") + "  (Enter=load)", new Vector2(nodeFinder.X + 3, nodeFinder.Center.Y), Color.White, 0.45f * scale, 0f, 0.5f);

            string exitTxt = string.IsNullOrWhiteSpace(_exitTargets) ? "Exit: (default)" : _exitTargets;
            if (!_enableExit) exitTxt = "(OFF) " + exitTxt;
            if (!string.IsNullOrWhiteSpace(_sequenceToken))
                exitTxt = $"{exitTxt} | Seq: {_sequenceToken}";

            Utils.DrawBorderString(spriteBatch, exitTxt, new Vector2(exitBox.X + 3, exitBox.Center.Y), Color.White, 0.45f * scale, 0f, 0.5f);
            Utils.DrawBorderString(spriteBatch, "BACK", new Vector2(backBox.Center.X, backBox.Center.Y), _enableBack ? Color.LightGray : Color.Gray, 0.45f * scale, 0.5f, 0.5f);
            Utils.DrawBorderString(spriteBatch, (_enableBtn1 ? "" : "(OFF) ") + (_btn1Targets ?? ""), new Vector2(b1Box.X + 3, b1Box.Center.Y), Color.White, 0.45f * scale, 0f, 0.5f);
            Utils.DrawBorderString(spriteBatch, (_enableBtn2 ? "" : "(OFF) ") + (_btn2Targets ?? ""), new Vector2(b2Box.X + 3, b2Box.Center.Y), Color.White, 0.45f * scale, 0f, 0.5f);
            Utils.DrawBorderString(spriteBatch, (_enableBtn3 ? "" : "(OFF) ") + (_btn3Targets ?? ""), new Vector2(b3Box.X + 3, b3Box.Center.Y), Color.White, 0.45f * scale, 0f, 0.5f);
        }

        private void DrawPreviewPortrait(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            string faceSet = string.IsNullOrWhiteSpace(_faceSetName) ? "Default" : _faceSetName.Trim();
            Vector2 eyesPos = panelPos + (_transformPreviewIndex == 6 ? DialogueUIState.Eyes7Offset : DialogueUIState.EyesOffset) * scale;
            DrawEditorEyes(spriteBatch, eyesPos, scale, faceSet, _previewEyeFrame, _previewIsCurrentlySpeaking);
            DrawEditorMouth(spriteBatch, panelPos + DialogueUIState.MouthOffset * scale, scale, faceSet, _previewMouthFrame);

            var extra = DialogueFaceSetRegistry.TryResolveExtraTexture(faceSet, _transformPreviewIndex);
            if (extra != null)
            {
                Vector2 origin = new Vector2(extra.Width, extra.Height) / 2f;
                spriteBatch.Draw(extra, panelPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            DrawEditorSparks(spriteBatch, panelPos, scale);
        }

        private void DrawEditorPortrait(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            string faceSet = string.IsNullOrWhiteSpace(_faceSetName) ? "Default" : _faceSetName.Trim();
            Vector2 eyesPos = panelPos + (_transformPreviewIndex == 6 ? DialogueUIState.Eyes7Offset : DialogueUIState.EyesOffset) * scale;
            DrawEditorEyes(spriteBatch, eyesPos, scale, faceSet, 0, false);
            DrawEditorMouth(spriteBatch, panelPos + DialogueUIState.MouthOffset * scale, scale, faceSet, 0);

            var extra = DialogueFaceSetRegistry.TryResolveExtraTexture(faceSet, _transformPreviewIndex);
            if (extra != null)
            {
                Vector2 origin = new Vector2(extra.Width, extra.Height) / 2f;
                spriteBatch.Draw(extra, panelPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            DrawEditorSparks(spriteBatch, panelPos, scale);
        }

        private void DrawEditorEyes(SpriteBatch spriteBatch, Vector2 position, float scale, string faceSetName, int eyeFrame, bool isSpeaking)
        {
            Texture2D eyeTexture = DialogueFaceSetRegistry.TryResolveEyesTexture(faceSetName, _transformPreviewIndex);
            if (eyeTexture == null) return;

            int numFrames = 4;
            int frameHeight = eyeTexture.Height / numFrames;
            Rectangle sourceRect = new Rectangle(0, eyeFrame * frameHeight, eyeTexture.Width, frameHeight);
            Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;

            if (_transformPreviewIndex == 4)
            {
                var baseSet = DialogueFaceSetRegistry.Get(faceSetName);
                string basePath = $"SariaMod/Items/zTalking/{baseSet.EyesPrefix}1";
                if (ModContent.RequestIfExists(basePath, out ReLogic.Content.Asset<Texture2D> baseAsset))
                {
                    Texture2D baseEyes = baseAsset.Value;
                    int baseFrameH = baseEyes.Height / numFrames;
                    Rectangle baseRect = new Rectangle(0, eyeFrame * baseFrameH, baseEyes.Width, baseFrameH);
                    Vector2 baseOrigin = new Vector2(baseRect.Width, baseRect.Height) / 2f;
                    spriteBatch.Draw(baseEyes, position, baseRect, Color.White, 0f, baseOrigin, scale, SpriteEffects.None, 0f);
                }

                Color glowColor = Color.White;
                glowColor = Color.Lerp(glowColor, Color.FloralWhite, 30f);
                glowColor = Color.Lerp(glowColor, Color.Transparent, SariaModUtilities.alpha3);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                spriteBatch.Draw(eyeTexture, position, sourceRect, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }
            else if (_transformPreviewIndex == 2)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                spriteBatch.Draw(eyeTexture, position, sourceRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }
            else if (_transformPreviewIndex == 6)
            {
                Rectangle singleRect = new Rectangle(0, 0, eyeTexture.Width, eyeTexture.Height);
                Vector2 singleOrigin = new Vector2(singleRect.Width, singleRect.Height) / 2f;

                // Speaking intensity: drives blend toward new effect + jitter
                float jitterTarget = isSpeaking ? 1f : 0f;
                _previewPoeWaveStrength = MathHelper.Lerp(_previewPoeWaveStrength, jitterTarget, 0.05f);

                _previewPoeWavePhase += 0.09f;
                if (_previewPoeWavePhase > MathF.PI * 20f)
                    _previewPoeWavePhase -= MathF.PI * 20f;

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
                        float baseWave = MathF.Sin(_previewPoeWavePhase + t * MathF.PI * 5f);
                        float baseAlpha = 1f - 0.45f * (baseWave * 0.5f + 0.5f);

                        // Speaking glow: broad sweeping ripples
                        float speakWave = MathF.Sin(_previewPoeWavePhase + t * MathF.PI * 2.5f);
                        float speakAlpha = 1f - 0.6f * (speakWave * 0.5f + 0.5f);

                        // Blend: idle = old glow, speaking = new glow
                        float alpha = MathHelper.Lerp(baseAlpha, speakAlpha, _previewPoeWaveStrength);

                        // Horizontal jitter: only when speaking
                        float jitterAmount = _previewPoeWaveStrength * 0.6f;
                        ulong rowSeed = jitterSeed * 31ul + (ulong)row * 7ul;
                        float jitter = ((float)(rowSeed % 1000u) / 500f - 1f) * jitterAmount * scale;

                        Rectangle rowRect = new Rectangle(0, row, texW, 1);
                        Vector2 rowPos = new Vector2(topLeftX + jitter, topLeftY + row * scale);
                        Color rowColor = Color.White * alpha;

                        spriteBatch.Draw(eyeTexture, rowPos, rowRect, rowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }
                }
            }
            else
            {
                spriteBatch.Draw(eyeTexture, position, sourceRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        private void DrawEditorMouth(SpriteBatch spriteBatch, Vector2 position, float scale, string faceSetName, int mouthFrame)
        {
            if (_transformPreviewIndex == 6) return;

            Texture2D mouthTexture = DialogueFaceSetRegistry.TryResolveMouthTexture(faceSetName, _transformPreviewIndex);
            if (mouthTexture == null) return;

            int numFrames = 5;
            int frameHeight = mouthTexture.Height / numFrames;
            Rectangle sourceRect = new Rectangle(0, mouthFrame * frameHeight, mouthTexture.Width, frameHeight);
            Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;

            spriteBatch.Draw(mouthTexture, position, sourceRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        private void DrawEditorSparks(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            if (_transformPreviewIndex != 3) return;

            bool sparksActive = false;
            int sariaType = ModContent.ProjectileType<Saria>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj != null && proj.active && proj.type == sariaType && proj.owner == Main.myPlayer && proj.ModProjectile is Saria sparksSaria)
                {
                    sparksActive = sparksSaria.SpecialAnimateValue > 0;
                    break;
                }
            }

            if (!sparksActive) return;

            string sparksPath = "SariaMod/Items/zTalking/SariaSparksPortrait";
            Texture2D sparksTexture;
            try { sparksTexture = ModContent.Request<Texture2D>(sparksPath).Value; }
            catch { sparksTexture = null; }

            if (sparksTexture == null) return;

            int sparksFrame = (int)Main.GameUpdateCount / 3 % 14;
            Rectangle sparksRect = sparksTexture.Frame(verticalFrames: 14, frameY: sparksFrame);
            Vector2 sparksOrigin = sparksRect.Size() / 2f;
            Vector2 sparksPos = panelPos + DialogueUIState.SparksPortraitOffset * scale;

            Color sparksColor = Color.Lerp(Color.White, Color.LightBlue, 2f);
            sparksColor *= 0.85f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            spriteBatch.Draw(sparksTexture, sparksPos, sparksRect, sparksColor, 0f, sparksOrigin, scale, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
        }

        private void DrawPreviewDialogueText(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            if (_previewWrappedLines == null || _previewWrappedLines.Count == 0)
                return;

            Vector2 textStart = panelPos + GetTextOffset() * scale;

            float baseTextScale = DialogueUIState.TextScale * scale;
            float maxHeightPx = GetTextMaxHeight() * scale;

            int totalLines = _previewWrappedLines.Count;
            float lineHeightAtBase = DialogueUIState.LineHeightBase * scale;
            float textScale = ComputeTextScaleToFit(baseTextScale, lineHeightAtBase, totalLines, maxHeightPx);

            float lineHeight = DialogueUIState.LineHeightBase * (textScale / baseTextScale) * scale;
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            Vector2 currentPos = textStart;
            int charIndex = 0;

            foreach (var line in _previewWrappedLines)
            {
                float xOffset = 0;
                foreach (var cc in line)
                {
                    if (charIndex >= _previewColoredText.Count)
                        return;

                    string charStr = cc.Character.ToString();
                    Vector2 charSize = font.MeasureString(charStr) * textScale;

                    Utils.DrawBorderString(spriteBatch, charStr, currentPos + new Vector2(xOffset, 0), _previewColoredText[charIndex].TextColor, textScale, 0f, 0f);
                    xOffset += charSize.X;
                    charIndex++;
                }
                currentPos.Y += lineHeight;
            }
        }

        private void DrawPreviewToggleButton(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            Rectangle rect = GetPreviewButtonRect(panelPos, scale);
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Color bgColor = _isPreviewPlaying ? new Color(180, 60, 60) : new Color(60, 120, 60);
            if (_hoveredPreviewButton == 0)
                bgColor = bgColor * 1.3f;

            spriteBatch.Draw(pixel, rect, bgColor);

            Color borderColor = Color.White * 0.8f;
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), borderColor);

            string label = _isPreviewPlaying ? "Stop" : "Play";
            Vector2 textPos = new(rect.Center.X, rect.Center.Y);
            Utils.DrawBorderString(spriteBatch, label, textPos, Color.White, 0.6f * scale, 0.5f, 0.5f);
        }

        public void HandleHotkeys()
        {
            if (!_isActive) return;

            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F6))
            {
                SaveCurrentNodeToOrigin();
            }

            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F7))
            {
                try
                {
                    foreach (var n in NewDialogueDatabase.AllNodes)
                        CompletedDialogueDatabase.RegisterOrReplace(n);

                    CreatedDialogueIO.SaveToFile(CompletedDialogueDatabase.AllNodes, FairyConfig.CreatedDialogueOutputMode.CompletedConversationNodes);
                    SetStatus($"Migrated NEW -> COMPLETED");
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
                catch (Exception ex)
                {
                    SetStatus("Migration failed: " + ex.Message);
                }
            }

            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F8))
            {
                try
                {
                    foreach (var n in DialogueDatabase.BuiltInNodes)
                        CompletedDialogueDatabase.RegisterOrReplace(n);

                    CreatedDialogueIO.SaveToFile(CompletedDialogueDatabase.AllNodes, FairyConfig.CreatedDialogueOutputMode.CompletedConversationNodes);
                    SetStatus($"Exported built-in nodes -> COMPLETED");
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
                catch (Exception ex)
                {
                    SetStatus("Export failed: " + ex.Message);
                }
            }

            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F9))
            {
                try
                {
                    var mode = FairyConfig.CreatedDialogueOutputMode.CreatedDialogue;
                    try { mode = FairyConfig.Instance?.DialogueCreatorOutputFile ?? mode; }
                    catch { }

                    var nodes = mode == FairyConfig.CreatedDialogueOutputMode.CompletedConversationNodes
                        ? CompletedDialogueDatabase.AllNodes
                        : NewDialogueDatabase.AllNodes;

                    CreatedDialogueIO.SaveToFile(nodes, mode);
                    SetStatus($"Wrote: {CreatedDialogueIO.GetOutputPathForMode(mode)}");
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
                catch (Exception ex)
                {
                    SetStatus("Save failed: " + ex.Message);
                }
            }
        }
    }
}
