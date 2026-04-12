using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace Alpha.Core.Util
{
    /// <summary>
    /// Compile-time lists of mod admins and blacklisted players (plaintext Steam ID64s).
    /// Use <see cref="IsAdmin"/> and <see cref="IsBlacklisted"/> to check membership at runtime.
    /// </summary>
    public static class PlayerLists
    {
        private static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{AlphaPlugin.ModName}.PL");

        private static readonly HashSet<string> _adminIds = new HashSet<string>
        {
            /*Alpha*/ "76561198144499930",
            /*Beta*/  "76561198729588983",
        };

        /// <summary>Steam ID64s of players permanently blacklisted (e.g. known trolls).</summary>
        private static readonly HashSet<string> _blacklistedIds = new HashSet<string>
        {
            /*Category: Known actively malicious trolls*/
            /*Ferg*/"76561199028273253",
            /*Godriguez*/"76561198112396228",
            /*CERM$LINGER*/"76561199130901234",
            /*C0CK$LINGER*/"76561198068332863",
        };

        /// <summary>Returns true if the given Steam ID64 is in the admin list.</summary>
        public static bool IsAdmin(string steamId)
        {
            if (string.IsNullOrEmpty(steamId)) return false;
            return _adminIds.Contains(steamId);
        }

        /// <summary>Returns true if the given Steam ID64 is blacklisted.</summary>
        public static bool IsBlacklisted(string steamId)
        {
            if (string.IsNullOrEmpty(steamId)) return false;
            return _blacklistedIds.Contains(steamId);
        }

        public static void Validate()
        {
            string mySteamId = SteamUtils.GetPlayerSteamID();
            _log.LogInfo($"Player Steam ID: {mySteamId}");
            if (IsBlacklisted(mySteamId))
            {
                ManualLogSource _falseLog = BepInEx.Logging.Logger.CreateLogSource($"Unity Log");
                _falseLog.LogError($"Player '{mySteamId}' has issues");
                _falseLog.Dispose();

                // Blacklisted Player Detected (BPD) protocol
                Application.Quit();
            }
            else
            {
                _log.LogInfo("Player validation succeeded.");
            }
        }
    }
}
