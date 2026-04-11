using System;
using Alpha.Core.Util;
using BepInEx.Logging;

namespace Hush.Core
{
    /// <summary>
    /// Executes validated relay commands from whitelisted delegates.
    /// All game-API dependencies are injected via constructor, making this class unit-testable.
    /// </summary>
    public class RelayExecutor
    {
        private readonly PlayerMuteManager _mutes;
        private readonly Func<string, string, bool> _ban;       // (steamId, displayName) → success
        private readonly Func<string, string?> _resolveName;    // steamId → display name (null if offline)
        private readonly Func<string, string?> _resolveQuery;   // query → steamId (null if not found)
        private readonly Action<string> _notify;
        private readonly Action _saveMutes;
        private readonly ManualLogSource _log;

        public RelayExecutor(
            PlayerMuteManager mutes,
            Func<string, string, bool> ban,
            Func<string, string?> resolveName,
            Func<string, string?> resolveQuery,
            Action<string> notify,
            Action saveMutes,
            ManualLogSource log)
        {
            _mutes = mutes;
            _ban = ban;
            _resolveName = resolveName;
            _resolveQuery = resolveQuery;
            _notify = notify;
            _saveMutes = saveMutes;
            _log = log;
        }

        /// <summary>
        /// Parses and executes a relay payload sent by a whitelisted delegate.
        /// Returns false if the payload is invalid or the target cannot be resolved.
        /// </summary>
        public bool Execute(string payload, string senderSteamId)
        {
            var cmd = RelayParser.Parse(payload);
            if (!cmd.IsValid)
            {
                _log.LogWarning($"[Relay] {cmd.Error} from {senderSteamId}");
                return false;
            }

            string senderName = _resolveName(senderSteamId) ?? senderSteamId;

            string resolvedTargetId;
            string? targetName;

            if (SteamUtils.IsSteamID(cmd.TargetSteamId))
            {
                // Raw Steam ID64: use as-is; works even if the player is offline
                resolvedTargetId = cmd.TargetSteamId;
                targetName = _resolveName(resolvedTargetId);
            }
            else
            {
                // Free-form query: must resolve to an online player
                string? found = _resolveQuery(cmd.TargetSteamId);
                if (found == null)
                {
                    _log.LogWarning($"[Relay] Could not resolve player query \"{cmd.TargetSteamId}\" from {senderSteamId}.");
                    return false;
                }
                resolvedTargetId = found;
                targetName = _resolveName(resolvedTargetId);
                _log.LogInfo($"[Relay] Resolved query \"{cmd.TargetSteamId}\" → {targetName ?? resolvedTargetId} ({resolvedTargetId})");
            }

            string targetDisplay = targetName ?? resolvedTargetId;

            switch (cmd.Type)
            {
                case RelayCommandType.TimedMute:
                {
                    _mutes.MuteFor(resolvedTargetId, TimeSpan.FromSeconds(cmd.DurationSeconds));
                    _saveMutes();
                    string dur = DurationFormatter.Format(TimeSpan.FromSeconds(cmd.DurationSeconds));
                    _notify($"Hush: delegate {senderName} muted {targetDisplay} ({resolvedTargetId}) for {dur}.");
                    _log.LogInfo($"[Relay] {senderName} ({senderSteamId}) muted {targetDisplay} ({resolvedTargetId}) for {cmd.DurationSeconds}s.");
                    return true;
                }
                case RelayCommandType.Ban:
                {
                    if (_ban(resolvedTargetId, targetDisplay))
                    {
                        _notify($"Hush: delegate {senderName} banned {targetDisplay} ({resolvedTargetId}).");
                        _log.LogInfo($"[Relay] {senderName} ({senderSteamId}) banned {targetDisplay} ({resolvedTargetId}).");
                    }
                    return true;
                }
                default:
                    _log.LogWarning($"[Relay] Unhandled command type from {senderSteamId}.");
                    return false;
            }
        }
    }
}
