using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.zTalking
{
    internal static class CreatedDialogueIO
    {
        private const string DefaultFolderName = "SariaMod";
        private const string DefaultCreatedFileName = "Created Dialogue.cs";
        private const string DefaultCompletedFileName = "Completed Conversation Nodes.cs";

        public static string GetOutputPath()
            => GetOutputPathForMode(GetConfiguredMode());

        public static string GetOutputPathForMode(FairyConfig.CreatedDialogueOutputMode mode)
        {
            string baseDir = Main.SavePath;

            string folder = DefaultFolderName;
            string createdName = DefaultCreatedFileName;
            string completedName = DefaultCompletedFileName;

            try
            {
                folder = FairyConfig.Instance?.DialogueCreatorOutputFolder ?? folder;
                createdName = FairyConfig.Instance?.DialogueCreatorCreatedFileName ?? createdName;
                completedName = FairyConfig.Instance?.DialogueCreatorCompletedFileName ?? completedName;
            }
            catch
            {
                // ignored; fall back to defaults
            }

            if (string.IsNullOrWhiteSpace(folder))
                folder = DefaultFolderName;

            string safeFile = mode == FairyConfig.CreatedDialogueOutputMode.CompletedConversationNodes
                ? (string.IsNullOrWhiteSpace(completedName) ? DefaultCompletedFileName : completedName)
                : (string.IsNullOrWhiteSpace(createdName) ? DefaultCreatedFileName : createdName);

            string outFolder = Path.Combine(baseDir, folder);
            Directory.CreateDirectory(outFolder);
            return Path.Combine(outFolder, safeFile);
        }

        public static void SaveToFile(IEnumerable<DialogueNode> nodes)
            => SaveToFile(nodes, GetConfiguredMode());

        public static void SaveToFile(IEnumerable<DialogueNode> nodes, FairyConfig.CreatedDialogueOutputMode mode)
        {
            string path = GetOutputPathForMode(mode);
            File.WriteAllText(path, GenerateSource(nodes, mode), Encoding.UTF8);
        }

        private static FairyConfig.CreatedDialogueOutputMode GetConfiguredMode()
        {
            try { return FairyConfig.Instance?.DialogueCreatorOutputFile ?? FairyConfig.CreatedDialogueOutputMode.CreatedDialogue; }
            catch { return FairyConfig.CreatedDialogueOutputMode.CreatedDialogue; }
        }

        private static string Escape(string s) => (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string GenerateSource(IEnumerable<DialogueNode> nodes, FairyConfig.CreatedDialogueOutputMode mode)
        {
            string className = mode == FairyConfig.CreatedDialogueOutputMode.CompletedConversationNodes
                ? "CompletedConversationNodes"
                : "CreatedDialogue";

            string registerCall = mode == FairyConfig.CreatedDialogueOutputMode.CompletedConversationNodes
                ? "CompletedDialogueDatabase.RegisterOrReplace"
                : "NewDialogueDatabase.RegisterOrReplace";

            var sb = new StringBuilder();
            sb.AppendLine("using SariaMod.Items.zTalking;\n");
            sb.AppendLine("namespace SariaMod.Items.zTalking\n{");
            sb.AppendLine($"    public static class {className}\n    {{");
            sb.AppendLine("        public static void RegisterAll()\n        {");

            foreach (var n in nodes)
            {
                if (n == null || string.IsNullOrWhiteSpace(n.LocationID))
                    continue;

                sb.AppendLine($"            {registerCall}(new DialogueNode(\"{Escape(n.LocationID)}\", \"{Escape(n.DialogueText)}\", \"{Escape(n.Mood)}\")");
                sb.AppendLine("            {");
                sb.AppendLine($"                Button1Label = \"{Escape(n.Button1Label)}\",");
                sb.AppendLine($"                Button1Target = \"{Escape(n.Button1Target)}\",");
                sb.AppendLine($"                Button2Label = \"{Escape(n.Button2Label)}\",");
                sb.AppendLine($"                Button2Target = \"{Escape(n.Button2Target)}\",");
                sb.AppendLine($"                Button3Label = \"{Escape(n.Button3Label)}\",");
                sb.AppendLine($"                Button3Target = \"{Escape(n.Button3Target)}\",");
                sb.AppendLine($"                DisableBackButton = {n.DisableBackButton.ToString().ToLowerInvariant()},");
                sb.AppendLine($"                DisableExitButton = {n.DisableExitButton.ToString().ToLowerInvariant()},");
                sb.AppendLine($"                ActionOnEnter = \"{Escape(n.ActionOnEnter)}\",");
                sb.AppendLine($"                IsPanicNode = {n.IsPanicNode.ToString().ToLowerInvariant()},");
                sb.AppendLine($"                IsCutscene = {n.IsCutscene.ToString().ToLowerInvariant()},");
                sb.AppendLine($"                CutscenePriority = {n.CutscenePriority},");
                sb.AppendLine($"                AnimateMouth = {n.AnimateMouth.ToString().ToLowerInvariant()},");
                sb.AppendLine($"                AutoAdvanceFrames = {n.AutoAdvanceFrames},");
                sb.AppendLine($"                AutoAdvanceTarget = \"{Escape(n.AutoAdvanceTarget)}\"");
                sb.AppendLine("            });");
            }

            sb.AppendLine("        }\n    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
