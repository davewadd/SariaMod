using System.Collections.Generic;

namespace SariaMod.Items.zTalking
{
    /// <summary>
    /// CREATED DIALOGUE - Runtime/edit generated nodes.
    /// Backwards-compatible shim: this now points at <see cref="NewDialogueDatabase"/>.
    /// </summary>
    public static class CreatedDialogueDatabase
    {
        public static IEnumerable<DialogueNode> AllNodes => NewDialogueDatabase.AllNodes;

        public static void RegisterOrReplace(DialogueNode node) => NewDialogueDatabase.RegisterOrReplace(node);

        public static bool TryGet(string id, out DialogueNode node) => NewDialogueDatabase.TryGet(id, out node);

        public static void Clear() => NewDialogueDatabase.Clear();
    }
}
