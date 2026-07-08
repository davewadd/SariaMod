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
    internal sealed partial class DialogueEditorUIState : UIState
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
    }
}
