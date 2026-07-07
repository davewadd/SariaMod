using System.Collections.Generic;

namespace SariaMod.Items.zTalking
{
    /// <summary>
    /// NEW DIALOGUE NODES - Work-in-progress nodes created/edited in the dialogue editor.
    /// </summary>
    public static class NewDialogueDatabase
    {
        private static readonly Dictionary<string, DialogueNode> _nodes = new();

        public static IEnumerable<DialogueNode> AllNodes => _nodes.Values;

        public static void RegisterOrReplace(DialogueNode node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.LocationID))
                return;

            _nodes[node.LocationID] = node;
        }

        public static bool TryGet(string id, out DialogueNode node) => _nodes.TryGetValue(id, out node);

        public static void Clear() => _nodes.Clear();
    }
}
