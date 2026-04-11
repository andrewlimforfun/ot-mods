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
            string display = DisplayId(steamId);
            bool hadTimed = _timedMutes.Remove(steamId);
            if (hadTimed && HushSettings.VerboseLogging) _log.LogDebug($"Mute: removed existing timed mute for {display}");
            if (!_permaMuted.Add(steamId))
            {
                if (HushSettings.VerboseLogging) _log.LogDebug($"Mute: {display} was already permanently muted (no-op)");
                return false;
            }
            _log.LogInfo($"Permanently muted: {display}");
            return true;
        }

        /// <summary>Temporarily mutes a player for <paramref name="duration"/>. Removes any existing permanent mute. Returns true if applied.</summary>
        public bool MuteFor(string steamId, TimeSpan duration)
        {
            string display = DisplayId(steamId);
            if (duration <= TimeSpan.Zero)
            {
                _log.LogWarning($"MuteFor: invalid duration {duration} for {display} — ignored");
                return false;
            }
            bool hadPerma = _permaMuted.Remove(steamId);
            if (hadPerma && HushSettings.VerboseLogging) _log.LogDebug($"MuteFor: removed existing permanent mute for {display}");
            bool hadTimed = _timedMutes.ContainsKey(steamId);
            _timedMutes[steamId] = DateTime.UtcNow + duration;
            if (hadTimed && HushSettings.VerboseLogging) _log.LogDebug($"MuteFor: extended/replaced existing timed mute for {display}");
            _log.LogInfo($"Timed muted: {display} until {_timedMutes[steamId]:u} ({duration.TotalSeconds:0}s)");
            return true;
        }

        /// <summary>Unmutes a player (permanent or timed). Returns true if they were previously muted.</summary>
        public bool Unmute(string steamId)
        {
            bool removedPerma = _permaMuted.Remove(steamId);
            bool removedTimed = _timedMutes.Remove(steamId);
            if (removedPerma || removedTimed)
            {
                _log.LogInfo($"Unmuted: {DisplayId(steamId)} (wasPerma={removedPerma}, wasTimed={removedTimed})");
                return true;
            }
            if (HushSettings.VerboseLogging) _log.LogDebug($"Unmute: {DisplayId(steamId)} was not muted (no-op)");
            return false;
        }

        /// <summary>Returns true if the player is currently muted (permanent or active timed mute).</summary>
        public bool IsMuted(string steamId)
        {
            if (_permaMuted.Contains(steamId))
            {
                if (HushSettings.VerboseLogging) _log.LogDebug($"IsMuted: {DisplayId(steamId)} → true (permanent)");
                return true;
            }
            else if (_timedMutes.TryGetValue(steamId, out DateTime expiry))
            {
                bool active = expiry > DateTime.UtcNow;
                if (HushSettings.VerboseLogging) _log.LogDebug($"IsMuted: {DisplayId(steamId)} → {active} (timed, expires {expiry:u}, remaining {(expiry - DateTime.UtcNow).TotalSeconds:0}s)");
                return active;
            } 
            if (HushSettings.VerboseLogging) _log.LogDebug($"IsMuted: {DisplayId(steamId)} → false (not in either list)");
            return false;
        }

        // -- Delegate management --------------------------------------------------

        /// <summary>Returns true if the Steam ID is on the mute-delegate whitelist.</summary>
        public bool IsDelegate(string steamId) => _delegates.Contains(steamId);

        /// <summary>Adds a Steam ID to the delegate whitelist. Returns true if newly added.</summary>
        public bool AddDelegate(string steamId)
        {
            if (!_delegates.Add(steamId)) return false;
            _log.LogInfo($"Added mute delegate: {DisplayId(steamId)}");
            return true;
        }

        /// <summary>Removes a Steam ID from the delegate whitelist. Returns true if it was present.</summary>
        public bool RemoveDelegate(string steamId)
        {
            if (!_delegates.Remove(steamId)) return false;
            _log.LogInfo($"Removed mute delegate: {DisplayId(steamId)}");
            return true;
        }

        /// <summary>Returns a snapshot of the current delegate Steam IDs.</summary>
        public IReadOnlyCollection<string> GetDelegates() => _delegates;

        // -- Expiry / tick ---------------------------------------------------------

        /// <summary>Prunes expired timed mutes. Must be called from <c>Plugin.Update()</c>.</summary>
        public void Tick()
        {
            if (_timedMutes.Count == 0) return;
            DateTime now = DateTime.UtcNow;
            var expired = _timedMutes.Where(kv => kv.Value <= now).Select(kv => kv.Key).ToList();
            foreach (string id in expired)
            {
                _timedMutes.Remove(id);
                string display = DisplayId(id);
                ChatUtils.AddGlobalNotification($"Timed mute expired for player {display}.");
                _log.LogInfo($"Timed mute expired: {display}");
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
            if (HushSettings.VerboseLogging) _log.LogInfo($"Saved mute config to: {filePath}");
        }

        /// <summary>Loads mutes from a JSON file. Silently no-ops if the file is missing or corrupt.</summary>
        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                if (HushSettings.VerboseLogging) _log.LogInfo("No mute config found, starting fresh.");
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

            if (HushSettings.VerboseLogging) _log.LogInfo($"Loaded mute config: {_permaMuted.Count} permanent, {_timedMutes.Count} timed, {_delegates.Count} delegates.");
        }

        private string DisplayId(string steamId)
        {
            try
            {
                PlayerDetail? player = PlayerUtils.FindPlayerBySteamID(steamId);
                return player != null ? $"{player.UserName} ({steamId})" : steamId;
            }
            catch { return steamId; }
        }

        private class MuteConfig
        {
            public List<string>? PermaMuted { get; set; }
            public Dictionary<string, string>? TimedMutes { get; set; }
            public List<string>? Delegates { get; set; }
        }
    }
}
