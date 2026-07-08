using Microsoft.Xna.Framework;
using System;
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
