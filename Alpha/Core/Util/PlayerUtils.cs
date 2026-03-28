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
        private static readonly Regex _digitsRegex = new Regex(@"^\d+$", RegexOptions.Compiled);

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

        public static List<PlayerDetail> FindPlayers(Func<string, PlayerID, PlayerIDInfo, NetworkTransform, bool> predicate)
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

        /// <summary>
        /// Resolves a free-form query to a player using the standard priority chain:
        /// Steam ID suffix (if all digits) → fuzzy display name → Steam persona name.
        /// </summary>
        public static PlayerDetail? FindPlayerByQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            if (query.Equals("_host", StringComparison.OrdinalIgnoreCase)) return GetHost();

            PlayerDetail? target = FindPlayer((_steamId, _pid, _info, _transform) =>
                QueryMatchesPlayer(_steamId, _pid, _info, _transform, query));
            return target;
        }

        public static List<PlayerDetail> FindPlayersByQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<PlayerDetail>();
            List<PlayerDetail> matches = FindPlayers((_steamId, _pid, _info, _transform) =>
                QueryMatchesPlayer(_steamId, _pid, _info, _transform, query));
            return matches;
        }

        private static bool QueryMatchesPlayer(string steamId, PlayerID id, PlayerIDInfo info, NetworkTransform transform, string query)
        {
            // If the query is all digits, check if it matches the end of the Steam ID
            bool allDigits = _digitsRegex.IsMatch(query);
            if (allDigits && steamId.EndsWith(query))
                return true;

            // Check if the query matches the player's display name (with TMP tags)
            string name = transform.GetComponent<PlayerController>().PlayerNameText.text;
            if (query.Contains("<") && name.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                return true;
            string cleanName = ChatUtils.CleanTMPTags(name).Trim();

            // Check if the query matches in the player's display name (without TMP tags)
            if (cleanName.Contains(query, StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(cleanName, query, RegexOptions.IgnoreCase))
                return true;

            // Check if the query matches the player's Steam persona name
            string? persona = SteamUtils.GetSteamPersonaName(steamId);
            if (persona != null && persona.Contains(query, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }


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
            return FindPlayers((_sid, _pid, _info, _t) => true);
        }

        public static string GetLobbyCode()
        {
            return MonoSingleton<MultiplayerManager>.I.LobbyCode;
        }

        public static string GetLobbyName()
        {
            return MonoSingleton<MultiplayerManager>.I.LobbyName;
        }

        public static int GetPlayerCount()
        {
            return NetworkSingleton<PlayerPanelController>.I?.IDInfos?.Count ?? 0;
        }

        public static int GetMaxPlayers()
        {
            return MonoSingleton<MultiplayerManager>.I._lobbyManager.CurrentLobby.MaxPlayers;
        }
    }
}
