using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Alpha.Core.Util;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace Hush.Core
{
    /// <summary>
    /// Host-side player mute manager. Tracks permanently and temporarily muted players by Steam ID.
    /// Call <see cref="Tick"/> every frame from <c>Plugin.Update()</c> to prune expired timed mutes.
    /// </summary>
    public class PlayerMuteManager
    {
        private readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{HushPlugin.ModName}.PMM");
        private readonly HashSet<string> _permaMuted = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, DateTime> _timedMutes = new Dictionary<string, DateTime>(StringComparer.Ordinal);
        private readonly HashSet<string> _delegates = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>Permanently mutes a player. Removes any existing timed mute. Returns true if newly muted.</summary>
        public bool Mute(string steamId)
        {
            _timedMutes.Remove(steamId);
            if (!_permaMuted.Add(steamId)) return false;
            _log.LogInfo($"Permanently muted: {steamId}");
            return true;
        }

        /// <summary>Temporarily mutes a player for <paramref name="duration"/>. Removes any existing permanent mute. Returns true if applied.</summary>
        public bool MuteFor(string steamId, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero) return false;
            _permaMuted.Remove(steamId);
            _timedMutes[steamId] = DateTime.UtcNow + duration;
            _log.LogInfo($"Timed muted: {steamId} until {_timedMutes[steamId]:u}");
            return true;
        }

        /// <summary>Unmutes a player (permanent or timed). Returns true if they were previously muted.</summary>
        public bool Unmute(string steamId)
        {
            bool removed = _permaMuted.Remove(steamId) | _timedMutes.Remove(steamId);
            if (removed) _log.LogInfo($"Unmuted: {steamId}");
            return removed;
        }

        /// <summary>Returns true if the player is currently muted (permanent or active timed mute).</summary>
        public bool IsMuted(string steamId)
        {
            if (_permaMuted.Contains(steamId)) return true;
            if (_timedMutes.TryGetValue(steamId, out DateTime expiry))
                return expiry > DateTime.UtcNow;
            return false;
        }

        // ── Delegate management ──────────────────────────────────────────────────

        /// <summary>Returns true if the Steam ID is on the mute-delegate whitelist.</summary>
        public bool IsDelegate(string steamId) => _delegates.Contains(steamId);

        /// <summary>Adds a Steam ID to the delegate whitelist. Returns true if newly added.</summary>
        public bool AddDelegate(string steamId)
        {
            if (!_delegates.Add(steamId)) return false;
            _log.LogInfo($"Added mute delegate: {steamId}");
            return true;
        }

        /// <summary>Removes a Steam ID from the delegate whitelist. Returns true if it was present.</summary>
        public bool RemoveDelegate(string steamId)
        {
            if (!_delegates.Remove(steamId)) return false;
            _log.LogInfo($"Removed mute delegate: {steamId}");
            return true;
        }

        /// <summary>Returns a snapshot of the current delegate Steam IDs.</summary>
        public IReadOnlyCollection<string> GetDelegates() => _delegates;

        // ── Expiry / tick ─────────────────────────────────────────────────────────

        /// <summary>Prunes expired timed mutes. Must be called from <c>Plugin.Update()</c>.</summary>
        public void Tick()
        {
            if (_timedMutes.Count == 0) return;
            DateTime now = DateTime.UtcNow;
            var expired = _timedMutes.Where(kv => kv.Value <= now).Select(kv => kv.Key).ToList();
            foreach (string id in expired)
            {
                _timedMutes.Remove(id);
                // try to get player name for better UX in the notification, but fall back to Steam ID if not found
                PlayerDetail? player = PlayerUtils.FindPlayerBySteamID(id);
                string displayId = player != null ? $"{player.UserName} ({id})" : id;

                ChatUtils.AddGlobalNotification($"Timed mute expired for player {displayId}.");
                _log.LogInfo($"Timed mute expired: {player?.UserNameClean ?? id}");
            }
        }

        /// <summary>Returns a snapshot of all active mutes. Expiry is null for permanent mutes.</summary>
        public List<(string SteamId, DateTime? Expiry)> GetMutes()
        {
            var list = new List<(string, DateTime?)>();
            foreach (string id in _permaMuted)
                list.Add((id, null));
            DateTime now = DateTime.UtcNow;
            foreach (var kv in _timedMutes)
                if (kv.Value > now)
                    list.Add((kv.Key, kv.Value));
            return list;
        }

        /// <summary>Saves permanent and active timed mutes to a JSON file.</summary>
        public void Save(string filePath)
        {
            var dto = new MuteConfig
            {
                PermaMuted = new List<string>(_permaMuted),
                TimedMutes = _timedMutes
                    .Where(kv => kv.Value > DateTime.UtcNow)
                    .ToDictionary(kv => kv.Key, kv => kv.Value.ToString("o")),
                Delegates = new List<string>(_delegates),
            };
            string json = JsonConvert.SerializeObject(dto, Formatting.Indented);
            File.WriteAllText(filePath, json, Encoding.UTF8);
            _log.LogInfo($"Saved mute config to: {filePath}");
        }

        /// <summary>Loads mutes from a JSON file. Silently no-ops if the file is missing or corrupt.</summary>
        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _log.LogInfo("No mute config found, starting fresh.");
                return;
            }
            MuteConfig? dto;
            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                dto = JsonConvert.DeserializeObject<MuteConfig>(json);
            }
            catch (Exception ex) { _log.LogWarning($"Failed to parse mute config: {ex.Message}"); return; }
            if (dto == null) return;

            _permaMuted.Clear();
            _timedMutes.Clear();
            _delegates.Clear();

            if (dto.PermaMuted != null)
                foreach (string id in dto.PermaMuted)
                    if (!string.IsNullOrWhiteSpace(id))
                        _permaMuted.Add(id.Trim());

            if (dto.TimedMutes != null)
            {
                DateTime now = DateTime.UtcNow;
                foreach (var kv in dto.TimedMutes)
                    if (DateTime.TryParse(kv.Value, null, DateTimeStyles.RoundtripKind, out DateTime expiry) && expiry > now)
                        _timedMutes[kv.Key] = expiry;
            }

            if (dto.Delegates != null)
                foreach (string id in dto.Delegates)
                    if (!string.IsNullOrWhiteSpace(id))
                        _delegates.Add(id.Trim());

            _log.LogInfo($"Loaded mute config: {_permaMuted.Count} permanent, {_timedMutes.Count} timed, {_delegates.Count} delegates.");
        }

        private class MuteConfig
        {
            public List<string>? PermaMuted { get; set; }
            public Dictionary<string, string>? TimedMutes { get; set; }
            public List<string>? Delegates { get; set; }
        }
    }
}
