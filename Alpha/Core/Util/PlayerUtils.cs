using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using PurrNet;

using HarmonyLib;
using BepInEx.Logging;

using Alpha;

namespace Alpha.Core.Util
{
    public static class PlayerUtils
    {
        private static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{AlphaPlugin.ModName}.PU");
        public static string GetUserName()
        {
            string userName = NetworkSingleton<TextChannelManager>.I.UserName;
            // fallback
            if (string.IsNullOrEmpty(userName))
            {
                PlayerDetail? player = GetPlayerDetail();
                if (player != null)
                {
                    userName = player.UserName;
                }
            }
            return userName;
        }

        public static string GetUserNameNoFormat()
        {
            return ChatUtils.CleanTMPTags(GetUserName());
        }

        public static PlayerDetail? GetPlayerDetail()
        {
            try
            {
                PlayerDetail? player = null;

                // get local player ID from PurrNet and match to PlayerIDs list
                PlayerID? playerId = NetworkSingleton<TextChannelManager>.I?.localPlayer;
                if (playerId != null)
                {
                    player = FindPlayer((_sid, _playerId, _info, _transform) => _playerId == playerId);
                }

                // fallback: match via SteamID string
                if (player == null)
                {
                    string steamId = SteamUtils.GetPlayerSteamID();
                    player = FindPlayerBySteamID(steamId);
                }

                if (player == null)
                {
                    _log.LogWarning("Could not find player detail for local player.");
                }

                return player;

            }
            catch (Exception ex)
            {
                _log.LogError($"Error getting player ID: {ex}");
                return null;
            }
        }

        public static List<PlayerDetail> FindAllPlayers(Func<string, PlayerID, PlayerIDInfo, NetworkTransform, bool> predicate)
        {
            var controller = NetworkSingleton<PlayerPanelController>.I;
            if (controller == null) return new List<PlayerDetail>();

            var players = new List<PlayerDetail>();
            for (int i = 0; i < controller.PlayerIDs.Count; i++)
            {
                var steamId = controller.PlayerSteamIDs[i];
                var playerId = controller.PlayerIDs[i];
                var playerIdInfo = controller.IDInfos[i];
                var transform = controller.PlayerTransforms[i];
                if (predicate(steamId, playerId, playerIdInfo, transform))
                    players.Add(new PlayerDetail(steamId, playerId, playerIdInfo, transform));
            }
            return players;
        }

        public static PlayerDetail? FindPlayer(Func<string, PlayerID, PlayerIDInfo, NetworkTransform, bool> predicate)
        {
            var controller = NetworkSingleton<PlayerPanelController>.I;
            if (controller == null) return null;
            for (int i = 0; i < controller.PlayerIDs.Count; i++)
            {
                var steamId = controller.PlayerSteamIDs[i];
                var playerId = controller.PlayerIDs[i];
                var playerIdInfo = controller.IDInfos[i];
                var transform = controller.PlayerTransforms[i];
                if (predicate(steamId, playerId, playerIdInfo, transform))
                    return new PlayerDetail(steamId, playerId, playerIdInfo, transform);
            }
            return null;
        }

        public static PlayerDetail? FindPlayerBySteamID(string steamId) =>
            FindPlayer((_steamId, _pid, _info, _transform) => _steamId == steamId);

        public static PlayerDetail? FindPlayerBySteamIDSuffix(string suffix) =>
            FindPlayer((_steamId, _pid, _info, _transform) => _steamId.EndsWith(suffix));

        public static PlayerDetail? FindPlayerBySteamPersona(string persona) =>
            FindPlayer((_steamId, _pid, _info, _transform) => SteamUtils.GetSteamPersonaName(_steamId)?.Contains(persona) == true);

        public static PlayerDetail? GetHost()
        {
            // match via PurrNet server PlayerID
            PlayerDetail? host = FindPlayer((_sid, _playerId, _info, _transform) => _playerId.isServer);

            // fallback: match via Steam lobby owner, the way this game does it
            if (host == null)
            {
                string hostSteamId = SteamUtils.GetLobbyOwnerSteamID();
                host = FindPlayer((steamId, _pid, _info, _t) => steamId == hostSteamId);
            }

            if (host == null)
            {
                _log.LogWarning("Could not find host player in PlayerIDs.");
            }

            return host;
        }
        public static List<PlayerDetail> GetAllPlayers()
        {
            return FindAllPlayers((_sid, _pid, _info, _t) => true);
        }

        public static string GetLobbyCode()
        {
            return MonoSingleton<MultiplayerManager>.I.LobbyCode;
        }

        public static int GetPlayerCount()
        {
            return NetworkSingleton<PlayerPanelController>.I?.IDInfos?.Count ?? 0;
        }

        /// <summary>
        /// Fuzzy-finds a player by username. Matches are tried against the clean name (TMP tags stripped)
        /// using the following priority: exact → starts-with → contains (all case-insensitive).
        /// The query is also stripped of TMP tags before comparison.
        /// </summary>
        public static PlayerDetail? FuzzyFindPlayerByName(string query)
        {
            if (string.IsNullOrEmpty(query)) return null;

            string cleanQuery = ChatUtils.CleanTMPTags(query).Trim();
            if (string.IsNullOrEmpty(cleanQuery)) return null;

            var allPlayers = GetAllPlayers();

            // Priority 1: exact match (case-insensitive)
            foreach (var player in allPlayers)
            {
                string cleanName = ChatUtils.CleanTMPTags(player.UserName).Trim();
                if (string.Equals(cleanName, cleanQuery, StringComparison.OrdinalIgnoreCase))
                    return player;
            }

            // Priority 2: starts-with match (case-insensitive)
            foreach (var player in allPlayers)
            {
                string cleanName = ChatUtils.CleanTMPTags(player.UserName).Trim();
                if (cleanName.StartsWith(cleanQuery, StringComparison.OrdinalIgnoreCase))
                    return player;
            }

            // Priority 3: contains match (case-insensitive)
            foreach (var player in allPlayers)
            {
                string cleanName = ChatUtils.CleanTMPTags(player.UserName).Trim();
                if (cleanName.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase))
                    return player;
            }

            return null;
        }
    }
}
