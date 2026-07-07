using System;
using Terraria;

namespace SariaMod.Items.zTalking
{
    internal static class DialogueCutsceneManager
    {
        private static bool _active;
        private static string _activeId = "";
        private static int _activePriority;

        public static bool IsActive => _active;
        public static string ActiveId => _activeId;
        public static int ActivePriority => _activePriority;

        public static bool TryStart(string id, int priority)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            if (_active)
            {
                if (priority <= _activePriority)
                    return false;
            }

            _active = true;
            _activeId = id;
            _activePriority = priority;
            return true;
        }

        public static void End(string id)
        {
            if (!_active)
                return;

            if (!string.Equals(_activeId, id, StringComparison.Ordinal))
                return;

            _active = false;
            _activeId = "";
            _activePriority = 0;
        }

        public static void Reset()
        {
            _active = false;
            _activeId = "";
            _activePriority = 0;
        }
    }
}
