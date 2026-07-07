using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;

namespace SariaMod
{
    public class FairyConfig : ModConfig
    {
        public static FairyConfig Instance;

        public override ConfigScope Mode => ConfigScope.ClientSide;

        // // Existing setting
        public bool Afterimages { get; set; }

        // ============================================================
        // UI SCALE SLIDER
        // ============================================================
        // // This slider controls how big the dialogue UI appears.
        // // 1.0 = normal size, 2.0 = double size, etc.
        // // Range: 1.0 to 10.0
        [Header("Dialogue UI Settings")]
        [Label("UI Scale")]
        [Tooltip("Adjusts the size of Saria's dialogue UI. 1.2 = normal, 2.0 = double size.")]
        [Slider]
        [Range(1.2f, 2f)]
        [Increment(0.1f)]
        [DefaultValue(1.5f)]
        public float DialogueUIScale { get; set; } = 1.5f;

        [Header("Dialogue Creation Panel")]
        [Label("Active")]
        [Tooltip("When enabled, shows a small launcher box at the top-center of the screen. Clicking it opens the Dialogue Creator.")]
        [DefaultValue(false)]
        public bool DialogueCreationPanelActive { get; set; } = false;

        [Header("Dialogue Text Box")]
        [Label("Text Offset X")]
        [Tooltip("Horizontal offset for where dialogue text starts (relative to the dialogue panel center).")]
        [Range(-300f, 300f)]
        [DefaultValue(-75f)]
        public float DialogueTextOffsetX { get; set; } = -75f;

        [Label("Text Offset Y")]
        [Tooltip("Vertical offset for where dialogue text starts (relative to the dialogue panel center).")]
        [Range(-300f, 300f)]
        [DefaultValue(-40f)]
        public float DialogueTextOffsetY { get; set; } = -40f;

        [Label("Text Box Max Width")]
        [Tooltip("Maximum width (in pixels) used for word wrapping dialogue text.")]
        [Range(100f, 600f)]
        [DefaultValue(235f)]
        public float DialogueTextMaxWidth { get; set; } = 235f;

        [Label("Text Box Max Height")]
        [Tooltip("Maximum height (in pixels) available for dialogue text before it is scaled down.")]
        [Range(20f, 200f)]
        [DefaultValue(48f)]
        public float DialogueTextMaxHeight { get; set; } = 48f;

        public enum CreatedDialogueOutputMode
        {
            CreatedDialogue,
            CompletedConversationNodes
        }

        [Header("Dialogue Creator Output")]
        [Label("Output File")]
        [Tooltip("Choose which file F9 writes created dialogue nodes into.")]
        [DefaultValue(CreatedDialogueOutputMode.CreatedDialogue)]
        public CreatedDialogueOutputMode DialogueCreatorOutputFile { get; set; } = CreatedDialogueOutputMode.CreatedDialogue;

        [Label("Output Folder")]
        [Tooltip("Folder (relative to your Terraria save path) where the output .cs files are written.")]
        [DefaultValue("SariaMod")]
        public string DialogueCreatorOutputFolder { get; set; } = "SariaMod";

        [Label("Created File Name")]
        [Tooltip("File name used when Output File is set to CreatedDialogue.")]
        [DefaultValue("Created Dialogue.cs")]
        public string DialogueCreatorCreatedFileName { get; set; } = "Created Dialogue.cs";

        [Label("Completed File Name")]
        [Tooltip("File name used when Output File is set to CompletedConversationNodes.")]
        [DefaultValue("Completed Conversation Nodes.cs")]
        public string DialogueCreatorCompletedFileName { get; set; } = "Completed Conversation Nodes.cs";
    }
}
