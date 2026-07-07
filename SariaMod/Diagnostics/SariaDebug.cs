using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Diagnostics
{
    /// <summary>
    /// General-purpose debug logger for SariaMod.
    ///
    /// General log   -> debugsaria.txt         (all tags: Spawn, FrozenGore, etc.)
    /// Owner follow  -> debugsaria_owner.txt   (path-follow state when this client owns Saria)
    /// Client follow -> debugsaria_client.txt  (path-follow state on non-owner clients)
    ///
    /// All files are cleared on each mod Load().  Written to ModSources/SariaMod/.
    /// </summary>
    public static class SariaDebug
    {
        private static readonly string BaseDir =
            "C:\\Users\\david\\OneDrive\\Documents\\My Games\\Terraria\\tModLoader\\ModSources\\SariaMod";

        private static readonly string LogPath       = Path.Combine(BaseDir, "debugsaria.txt");
        private static readonly string OwnerLogPath  = Path.Combine(BaseDir, "debugsaria_owner.txt");
        private static readonly string ClientLogPath = Path.Combine(BaseDir, "debugsaria_client.txt");

        private static bool _initialized = false;

        /// <summary>Call once at mod Load() to clear all log files for the new session.</summary>
        public static void Initialize()
        {
            try
            {
                string ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                // On a multiplayer session two processes (listen server + client, or two
                // clients on one PC) share these exact paths. Only clear the files when we
                // can take an exclusive lock instantly — a second process skips the wipe
                // instead of truncating the first one's live log mid-session.
                TryInitFile(LogPath,       $"=== SariaMod general log -- {ts} ==={System.Environment.NewLine}");
                TryInitFile(OwnerLogPath,  $"=== SariaMod follow log [OWNER] -- {ts} ==={System.Environment.NewLine}");
                TryInitFile(ClientLogPath, $"=== SariaMod follow log [CLIENT] -- {ts} ==={System.Environment.NewLine}");
                _initialized = true;
            }
            catch { }
        }

        private static void TryInitFile(string path, string header)
        {
            try
            {
                using FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using StreamWriter w = new StreamWriter(fs);
                w.Write(header);
            }
            catch { /* another process holds it — keep appending to the existing session file */ }
        }

        /// <summary>
        /// Appends a line using a shared-write FileStream. File.AppendAllText opens the
        /// file EXCLUSIVELY — when the listen server and a client (two processes) log to
        /// this same path, whoever loses the race throws and its line is silently eaten by
        /// the catch. That made entire subsystems look dead in multiplayer when they were
        /// logging fine. FileShare.ReadWrite lets both processes interleave lines.
        /// </summary>
        private static void AppendShared(string path, string line)
        {
            try
            {
                using FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using StreamWriter w = new StreamWriter(fs);
                w.WriteLine(line);
            }
            catch { }
        }

        /// <summary>
        /// Short role prefix so interleaved multi-process logs are attributable:
        /// SP = singleplayer, MC = multiplayer client, SV = server (incl. listen server).
        /// </summary>
        private static string NetTag =>
            Main.netMode == NetmodeID.Server ? "SV" :
            Main.netMode == NetmodeID.MultiplayerClient ? "MC" : "SP";

        /// <summary>Write a timestamped message to debugsaria.txt and optionally in-game chat.</summary>
        public static void Log(string tag, string message, Color chatColor = default)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] [{NetTag}] [{tag}] {message}";
            if (_initialized)
            {
                AppendShared(LogPath, line);
            }
            if (chatColor != default && chatColor != Color.Transparent && !Main.dedServ)
                Main.NewText($"[{tag}] {message}", chatColor);
        }

        /// <summary>Write to debugsaria.txt only -- no chat output.</summary>
        public static void LogSilent(string tag, string message) => Log(tag, message, Color.Transparent);

        /// <summary>
        /// Write a path-follow state line to the appropriate role file.
        /// isOwner=true  -> debugsaria_owner.txt
        /// isOwner=false -> debugsaria_client.txt
        /// </summary>
        public static void LogFollow(bool isOwner, string message)
        {
            if (!_initialized) return;
            string path = isOwner ? OwnerLogPath : ClientLogPath;
            AppendShared(path, $"[{DateTime.Now:HH:mm:ss.fff}] [{NetTag}] {message}");
        }
    }
}
