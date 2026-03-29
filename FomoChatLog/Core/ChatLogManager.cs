using System;
using System.IO;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace FomoChatLog.Core
{
    /// <summary>
    /// Manages daily rotating chat log files: path resolution, directory setup, and pruning old logs.
    /// Config entries are owned by <see cref="FomoChatLogPlugin"/> and passed in via the constructor.
    /// </summary>
    public class ChatLogManager
    {
        private readonly ManualLogSource _log;
        private readonly ConfigEntry<bool> _enableFeature;
        private readonly ConfigEntry<string> _pathRaw;
        private readonly ConfigEntry<int> _maxLogDays;

        public ChatLogManager(
            ManualLogSource log,
            ConfigEntry<bool> enableFeature,
            ConfigEntry<string> pathRaw,
            ConfigEntry<int> maxLogDays)
        {
            _log = log;
            _enableFeature = enableFeature;
            _pathRaw = pathRaw;
            _maxLogDays = maxLogDays;
        }

        /// <summary>
        /// Creates the log directory and prunes old daily log files beyond the retention limit.
        /// Call once during plugin startup after config is bound.
        /// </summary>
        public void Initialize()
        {
            string todayPath = GetTodayPath();
            string dir = Path.GetDirectoryName(todayPath) ?? string.Empty;
            _log.LogInfo("Creating chat log directory if it doesn't exist: " + dir);
            Directory.CreateDirectory(dir);

            if (_enableFeature.Value)
                PruneOldLogs(dir);
        }

        /// <summary>
        /// Returns the log file path for today, e.g. …/on_together_chat_log_2026-03-29.txt
        /// </summary>
        public string GetTodayPath()
        {
            string expanded = ExpandRaw(_pathRaw.Value);
            string dir  = Path.GetDirectoryName(expanded) ?? string.Empty;
            string stem = Path.GetFileNameWithoutExtension(expanded);
            string ext  = Path.GetExtension(expanded);
            return Path.Combine(dir, $"{stem}_{DateTime.Now:yyyy-MM-dd}{ext}");
        }

        /// <summary>
        /// Updates the raw base path config value (date suffix is always appended automatically).
        /// </summary>
        public void SetBasePath(string newPath) => _pathRaw.Value = newPath;

        // -------------------------------------------------------------------------

        private void PruneOldLogs(string dir)
        {
            string expanded = ExpandRaw(_pathRaw.Value);
            string stem = Path.GetFileNameWithoutExtension(expanded);
            string ext  = Path.GetExtension(expanded);

            var files = Directory.GetFiles(dir, $"{stem}_????-??-??{ext}");
            Array.Sort(files); // lexicographic == chronological for yyyy-MM-dd names

            int toDelete = files.Length - _maxLogDays.Value;
            for (int i = 0; i < toDelete; i++)
            {
                _log.LogInfo("Pruning old chat log: " + files[i]);
                File.Delete(files[i]);
            }
        }

        private static string ExpandRaw(string raw)
        {
            raw = Environment.ExpandEnvironmentVariables(raw);
            if (raw.StartsWith("~"))
                raw = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), raw[1..]);
            return raw;
        }
    }
}
