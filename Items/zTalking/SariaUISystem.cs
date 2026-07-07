using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using SariaMod.Items.Strange;

namespace SariaMod.Items.zTalking
{
    /// <summary>
    /// SARIA UI SYSTEM - Central manager for the dialogue UI
    /// ============================================================
    /// PUBLIC API:
    /// - DisplayDialogue(startID, sariaProj) - Normal dialogue
    /// - StartCutscene(cutsceneID, sariaProj) - Priority cutscene (ignores held item/panic)
    /// - CloseDialogue() - Force close
    /// - IsDialogueActive - Check if UI is open
    /// - IsCutsceneActive - Check if a cutscene is playing
    /// - TalkingTimer - Frames since dialogue started (this session)
    /// - TotalTalkingTime - Cumulative frames across all sessions
    /// - InteractionTimer - Cooldown frames before next interaction can trigger
    /// - CutsceneTimer - Cooldown frames before next cutscene can trigger
    /// ============================================================
    /// </summary>
    public class SariaUISystem : ModSystem
    {
        internal static DialogueUIState DialogueUI;
        private UserInterface _dialogueInterface;

        internal static MeterBarUIState MeterBarUI;
        private UserInterface _meterBarInterface;

        public static bool IsDialogueActive => DialogueUI?.IsActive ?? false;
        public static bool IsDialogueEnding => DialogueUI?.IsEnding ?? false;
        public static bool IsCutsceneActive => DialogueCutsceneManager.IsActive;
        public static string ActiveCutsceneID => DialogueCutsceneManager.ActiveId;
        
        /// <summary>Frames since current dialogue session started</summary>
        public static int TalkingTimer => DialogueUIState.TalkingTimer;

        /// <summary>Cumulative talking time across all sessions</summary>
        public static int TotalTalkingTime => DialogueUIState.TotalTalkingTime;

        /// <summary>Interaction cooldown timer — frames remaining before the next interaction can trigger</summary>
        public static int InteractionTimer => Main.LocalPlayer?.GetModPlayer<FairyPlayer>()?.totalTalkingTime ?? 0;

        /// <summary>Cutscene cooldown timer — frames remaining before the next cutscene can trigger</summary>
        public static int CutsceneTimer => Main.LocalPlayer?.GetModPlayer<FairyPlayer>()?.smallTalkingTime ?? 0;

        // ============================================================
        // LOAD
        // ============================================================
        public override void Load()
        {
            if (Main.dedServ)
                return;

            DialogueUI = new DialogueUIState();
            DialogueUI.Activate();

            _dialogueInterface = new UserInterface();
            _dialogueInterface.SetState(DialogueUI);

            MeterBarUI = new MeterBarUIState();
            MeterBarUI.Activate();

            _meterBarInterface = new UserInterface();
            _meterBarInterface.SetState(MeterBarUI);

            DialogueDatabase.InitializeDefaultDialogue();
        }

        // ============================================================
        // UNLOAD
        // ============================================================
        public override void Unload()
        {
            DialogueDatabase.Clear();
            DialogueCutsceneManager.Reset();
            DialogueUI = null;
            _dialogueInterface = null;
            MeterBarUI = null;
            _meterBarInterface = null;
        }

        // ============================================================
        // WORLD LOAD / UNLOAD — close any stale UI
        // ============================================================
        public override void OnWorldLoad()
        {
            if (DialogueUI?.IsActive == true)
                DialogueUI.CloseDialogue();
        }

        public override void OnWorldUnload()
        {
            if (DialogueUI?.IsActive == true)
                DialogueUI.CloseDialogue();
        }

        // ============================================================
        // PUBLIC API
        // ============================================================
        
        /// <summary>
        /// Display normal dialogue. Requires player to hold HealBall.
        /// Will be interrupted by panic (damage) or dropping the item.
        /// </summary>
        public static void DisplayDialogue(string startID, Projectile sariaProj)
        {
            if (startID == "Pending")
            {
                var player = Main.LocalPlayer.GetModPlayer<SariaInteractionTrackerPlayer>();
                var pending = player.GetBestAvailableCutscene();
                
                // Only show pending node if we have a valid pending cutscene
                if (pending != null)
                {
                    // Retrieve the configured "Pending" node from the database
                    var baseNode = DialogueDatabase.GetNode("Pending");
                    
                    // Fallback to "start" if "Pending" is missing (safety check)
                    if (baseNode == null) baseNode = DialogueDatabase.GetNode("start");

                    if (baseNode != null)
                    {
                        // Create a dynamic copy of the node to inject the cutscene button
                        var pendingNode = new DialogueNode(baseNode.LocationID, baseNode.DialogueText, baseNode.Mood)
                        {
                            AnimateMouth = baseNode.AnimateMouth,
                            
                            Button1Label = baseNode.Button1Label,
                            Button1Target = baseNode.Button1Target,
                            
                            // Inject pending cutscene data into Button 2
                            Button2Label = pending.ButtonText, 
                            Button2Target = pending.TargetNodeID,
                            
                            Button3Label = baseNode.Button3Label,
                            Button3Target = baseNode.Button3Target,
                            
                            DisableBackButton = baseNode.DisableBackButton,
                            DisableExitButton = baseNode.DisableExitButton,
                            ExitTargetOverride = baseNode.ExitTargetOverride,
                            SequenceToken = baseNode.SequenceToken,
                            FaceSetName = baseNode.FaceSetName
                        };

                        if (DialogueUI != null)
                        {
                            DialogueUI.DisplayDialogue(pendingNode, sariaProj);
                            return;
                        }
                    }
                }
            }

            if (DialogueUI != null)
            {
                DialogueUI.DisplayDialogue(startID, sariaProj);
            }
        }

        /// <summary>
        /// Start a CUTSCENE - Priority dialogue that:
        /// - Ignores held item requirement
        /// - Ignores panic triggers (player taking damage)
        /// - Blocks other cutscenes until finished
        /// Returns false if another cutscene is already active.
        /// </summary>
        public static bool StartCutscene(string cutsceneID, Projectile sariaProj)
        {
            if (DialogueUI != null)
            {
                return DialogueUI.StartCutscene(cutsceneID, sariaProj);
            }
            return false;
        }

        /// <summary>
        /// Force close the dialogue UI
        /// </summary>
        public static void CloseDialogue()
        {
            if (DialogueUI != null)
            {
                DialogueUI.CloseDialogue();
            }
        }

        public static void TriggerPanic()
        {
            if (DialogueUI != null)
            {
                DialogueUI.TriggerPanic();
            }
        }

        public static void TriggerNormalExit()
        {
            if (DialogueUI != null)
            {
                DialogueUI.TriggerNormalExit();
            }
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.gamePaused)
                return;

            if (_dialogueInterface?.CurrentState != null && DialogueUI?.IsActive == true)
            {
                _dialogueInterface.Update(gameTime);
            }

            bool sariaActive = IsSariaActiveForLocalPlayer();
            if (_meterBarInterface?.CurrentState != null && sariaActive && !Main.playerInventory && !Main.gameMenu)
            {
                _meterBarInterface.Update(gameTime);
            }
        }

        // ============================================================
        // INTERFACE LAYERS
        // ============================================================
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "SariaMod: Dialogue UI",
                    delegate
                    {
                        if (DialogueUI?.IsActive == true)
                        {
                            _dialogueInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI
                ));

                layers.Insert(mouseTextIndex + 1, new LegacyGameInterfaceLayer(
                    "SariaMod: Sneeze Meter Bar",
                    delegate
                    {
                        if (IsSariaActiveForLocalPlayer() && !Main.playerInventory && !Main.gameMenu && DialogueUI?.IsActive != true)
                        {
                            _meterBarInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }

        private static bool IsSariaActiveForLocalPlayer()
        {
            int sariaType = ModContent.ProjectileType<Saria>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                var p = Main.projectile[i];
                if (p.active && p.owner == Main.myPlayer && p.type == sariaType)
                    return true;
            }
            return false;
        }
    }
}