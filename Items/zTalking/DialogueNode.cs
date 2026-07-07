using System;
using System.Collections.Generic;
using Terraria;

namespace SariaMod.Items.zTalking
{
    /// <summary>
    /// DIALOGUE NODE - One screen of dialogue
    /// </summary>
    public class DialogueNode
    {
        public string LocationID { get; set; }
        public string DialogueText { get; set; }
        public string Mood { get; set; } = "Normal";

        public string Button1Label { get; set; } = "";
        public string Button1Target { get; set; } = "";

        public string Button2Label { get; set; } = "";
        public string Button2Target { get; set; } = "";

        public string Button3Label { get; set; } = "";
        public string Button3Target { get; set; } = "";

        public bool DisableBackButton { get; set; } = false;
        public bool DisableExitButton { get; set; } = false;
        public string ActionOnEnter { get; set; } = "";

        // // Is this a panic/emergency node? (fast exit)
        public bool IsPanicNode { get; set; } = false;

        /// <summary>
        /// Optional special interaction token: "start-15", "continue-15", "complete-15".
        /// Used to track one-time interaction sequences per character.
        /// </summary>
        public string SequenceToken { get; set; } = string.Empty;

        // ============================================================
        // CUTSCENE PROPERTIES
        // ============================================================
        /// <summary>If true, this node is part of a cutscene (ignores held item, panic triggers)</summary>
        public bool IsCutscene { get; set; } = false;

        /// <summary>Cutscene priority. Higher priority cutscenes can override lower priority cutscenes.</summary>
        public int CutscenePriority { get; set; } = 0;

        /// <summary>Whether mouth should animate for this node.</summary>
        public bool AnimateMouth { get; set; } = true;

        /// <summary>Auto-advance to next node after this many frames (0 = manual advance only)</summary>
        public int AutoAdvanceFrames { get; set; } = 0;

        /// <summary>Target location when auto-advancing</summary>
        public string AutoAdvanceTarget { get; set; } = "";

        public string ExitTargetOverride { get; set; } = string.Empty;

        /// <summary>
        /// Optional named face set to use for this node (e.g. "Default", "Sad", "Angry").
        /// </summary>
        public string FaceSetName { get; set; } = "Default";

        public DialogueNode(string locationID, string dialogueText, string mood = "Normal")
        {
            LocationID = locationID;
            DialogueText = dialogueText;
            Mood = mood;
        }

        public DialogueNode()
        {
            LocationID = "";
            DialogueText = "";
            Mood = "Normal";
        }

        internal static string PickRandomTarget(string targets)
        {
            if (string.IsNullOrWhiteSpace(targets))
                return string.Empty;

            // Support: "Node-A , Node-B , Node-C"
            string[] parts = targets.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return targets.Trim();

            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim();

            if (parts.Length == 1)
                return parts[0];

            return parts[Main.rand.Next(parts.Length)];
        }
    }

    /// <summary>
    /// DIALOGUE DATABASE - Stores all dialogue nodes
    /// ============================================================
    /// TAGS YOU CAN USE IN DIALOGUE TEXT:
    /// ============================================================
    /// [color:ColorName]  - Change text color (White, Pink, LightBlue, Green, Yellow, Orange, Red, Purple, Cyan, Gold, Gray)
    /// [wait:X]           - Pause for X frames (60 = 1 second)
    /// [speed:X]          - Change text speed (1=fast, 4=slow)
    /// [silent]           - Start silent section (no mouth animation/sound)
    /// [/silent]          - End silent section
    /// [mouth]            - Explicitly enable mouth for following text (default)
    /// [/mouth]           - Disable mouth for following text (e.g. ellipses "..." or narration)
    ///
    /// Example: "Hello![wait:30] [/mouth]...\n[mouth] How are you?"
    /// ============================================================
    /// </summary>
    public static class DialogueDatabase
    {
        private static Dictionary<string, DialogueNode> _nodes = new Dictionary<string, DialogueNode>();

        public static IEnumerable<DialogueNode> BuiltInNodes => _nodes.Values;

        public static void RegisterNode(DialogueNode node)
        {
            if (!string.IsNullOrEmpty(node.LocationID))
            {
                _nodes[node.LocationID] = node;
            }
        }

        public static DialogueNode GetNode(string locationID)
        {
            if (CompletedDialogueDatabase.TryGet(locationID, out var completed))
                return completed;

            if (NewDialogueDatabase.TryGet(locationID, out var created))
                return created;

            if (_nodes.TryGetValue(locationID, out DialogueNode node))
            {
                return node;
            }
            return null;
        }

        public static void Clear()
        {
            _nodes.Clear();
            NewDialogueDatabase.Clear();
            CompletedDialogueDatabase.Clear();
        }

        public static void InitializeDefaultDialogue()
        {
            // ============================================================
            // MAIN MENU (Entry Point)
            // ============================================================
            RegisterNode(new DialogueNode("start",
                "[color:LightBlue]Hey there![color:White][wait:20] How can I help you today?",
                "Normal")
            {
                Button1Label = "Chat",
                Button1Target = "chat_menu",
                Button2Label = "Skills",
                Button2Target = "skills_menu",
                Button3Label = "Status",
                Button3Target = "status_menu",
                DisableBackButton = true
            });

            // ============================================================
            // PENDING NODE (Waiting for cutscene)
            // ============================================================
            RegisterNode(new DialogueNode("Pending",
                "[speed:4]...[speed:2]", 
                "Normal")
            {
                AnimateMouth = false,
                Button1Label = "Chat",
                Button1Target = "chat_menu",
                Button2Label = "???", // Will be replaced dynamically
                Button2Target = "",
                Button3Label = "Status",
                Button3Target = "status_menu",
                DisableBackButton = true
            });

            // ============================================================
            // PANIC NODE (Emergency - player taking damage!)
            // ============================================================
            RegisterNode(new DialogueNode("panic",
                "[color:Red]Watch out![color:White][wait:10] You're taking damage! [color:Yellow]Focus on the fight![color:White]",
                "Shocked")
            {
                DisableBackButton = true,
                DisableExitButton = true,
                IsPanicNode = true
            });

            // ============================================================
            // CHAT MENU
            // ============================================================
            RegisterNode(new DialogueNode("chat_menu",
                "What would you like to talk about?[wait:15][speed:3] I'm all ears!",
                "Normal")
            {
                Button1Label = "Weather",
                Button1Target = "chat_weather",
                Button2Label = "Adventure",
                Button2Target = "chat_adventure",
                Button3Label = "Back",
                Button3Target = "start"
            });

            RegisterNode(new DialogueNode("chat_weather",
                "The weather today is [color:LightBlue]quite pleasant![color:White][wait:20][speed:2] Perfect for adventuring!",
                "Normal")
            {
                Button1Label = "Nice!",
                Button1Target = "chat_menu",
                Button2Label = "",
                Button3Label = ""
            });

            RegisterNode(new DialogueNode("chat_adventure",
                "Adventure is out there! [color:Green]Be careful[color:White] though, okay?",
                "Normal")
            {
                Button1Label = "I will.",
                Button1Target = "chat_menu",
                Button2Label = "",
                Button3Label = ""
            });

            // ============================================================
            // TEST CUTSCENES
            // ============================================================
            
            // Hallow Intro Cutscene
            RegisterNode(new DialogueNode("cutscene_hallow_intro", "The lights here are so pretty... It feels like a dream.", "Happy")
            {
                IsCutscene = true,
                CutscenePriority = 10,
                Button2Label = "It is beautiful.",
                Button2Target = "cutscene_hallow_2"
            });

            RegisterNode(new DialogueNode("cutscene_hallow_2", "I wonder if the fairies here are friendly?", "Normal")
            {
                IsCutscene = true,
                CutscenePriority = 10,
                Button2Label = "Maybe.",
                Button2Target = "" // End
            });

            // Zora (Transform 1) Intro Cutscene
            RegisterNode(new DialogueNode("cutscene_zora_intro", "That blue form... I felt so at home in the water.", "Happy")
            {
                IsCutscene = true,
                CutscenePriority = 10,
                Button2Label = "You looked natural.",
                Button2Target = "cutscene_zora_2"
            });

            RegisterNode(new DialogueNode("cutscene_zora_2", "I hope we can go swimming again soon!", "Happy")
            {
                IsCutscene = true,
                CutscenePriority = 10,
                Button2Label = "We will.",
                Button2Target = "" // End
            });

            // ============================================================
            // STATUS MENU
            // ============================================================
            RegisterNode(new DialogueNode("status_menu",
                "Let me check my current condition[silent]...[/silent][wait:40][speed:4] [color:Green]Everything looks good![color:White]",
                "Normal")
            {
                Button1Label = "Details",
                Button1Target = "status_details",
                Button2Label = "",
                Button3Label = ""
            });

            RegisterNode(new DialogueNode("status_details",
                "[color:Cyan]Current form[color:White] is stable.[wait:20] [color:Yellow]Energy levels[color:White] are optimal![wait:15][speed:2] [color:Green]Ready for anything![color:White]",
                "Normal")
            {
                Button1Label = "Good!",
                Button1Target = "status_menu",
                Button2Label = "",
                Button3Label = ""
            });

            // ============================================================
            // EXIT DIALOGUE
            // ============================================================
            RegisterNode(new DialogueNode("exit_happy",
                "[color:Pink]See you later![color:White][wait:20] Take care out there!",
                "Normal")
            {
                DisableBackButton = true,
                DisableExitButton = true
            });

            RegisterNode(new DialogueNode("exit_sad",
                "[color:LightBlue]Oh[silent]...[/silent][color:White][wait:30] okay then.[wait:20] [color:Gray]Goodbye[silent]...[/silent][color:White]",
                "Normal")
            {
                DisableBackButton = true,
                DisableExitButton = true
            });

            // ============================================================
            // EXAMPLE CUTSCENE - Triggered externally, ignores held item
            // ============================================================
            RegisterNode(new DialogueNode("cutscene_intro",
                "[color:Gold]~Important Message~[color:White][wait:30]\nSomething significant is happening[silent]...[/silent]",
                "Normal")
            {
                IsCutscene = true,
                DisableBackButton = true,
                DisableExitButton = true,
                Button1Label = "Continue",
                Button1Target = "cutscene_intro_2",
                Button2Label = "",
                Button3Label = ""
            });

            RegisterNode(new DialogueNode("cutscene_intro_2",
                "This is the second part of the cutscene.[wait:20] It will close when you click okay.",
                "Normal")
            {
                IsCutscene = true,
                DisableBackButton = true,
                Button1Label = "Okay",
                Button1Target = "", // Empty target = close dialogue
                Button2Label = "",
                Button3Label = ""
            });

            // ============================================================
            // INTERACTIVE TIER (Placeholders)
            // ============================================================
            // RegisterNode(new DialogueNode("interactive_greeting_1", "Interactive Dialogue 1", "Normal"));
            // RegisterNode(new DialogueNode("interactive_greeting_2", "Interactive Dialogue 2", "Normal"));
            // RegisterNode(new DialogueNode("interactive_greeting_3", "Interactive Dialogue 3", "Normal"));
            // RegisterNode(new DialogueNode("interactive_greeting_4", "Interactive Dialogue 4", "Normal"));

            // ============================================================
            // SUNFLOWER INTERACTION
            // ============================================================
            RegisterNode(new DialogueNode("forest_sunflower_interaction", 
                "...", 
                "Normal")
            {
                Button1Label = "Something up?",
                Button1Target = "forest_sunflower_interaction_2",
                DisableBackButton = true
            });

            RegisterNode(new DialogueNode("forest_sunflower_interaction_2", 
                "It's just... that [c/FFF014:sunflower] over there. It looks quite... happy. Don't you think?", 
                "Happy")
            {
                Button1Label = "Yeah.",
                Button1Target = "", // End
                Button2Label = "",
                Button3Label = ""
            });
        }
    }
}
