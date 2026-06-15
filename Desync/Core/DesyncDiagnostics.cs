using System;
using System.Collections.Generic;
using System.Linq;
using PurrNet;
using Steamworks;

namespace Desync.Core
{
    /// <summary>
    /// A point-in-time snapshot of lobby/host health diagnostics.
    /// Designed to be testable without Unity dependencies (accepts raw values).
    /// </summary>
    public class DesyncDiagnostics
    {
        public bool LobbyValid { get; }
        public string LobbyOwnerSteamId { get; }
        public string LocalSteamId { get; }
        public List<string> PlayerSteamIds { get; }
        public int TotalTrackedPlayers { get; }
        public string HostPersonaName { get; }
        public string HostUserName { get; }
        public bool IsHost { get; }

        public DesyncDiagnostics(
            bool lobbyValid,
            string lobbyOwnerSteamId,
            string localSteamId,
            List<string> playerSteamIds,
            int totalTrackedPlayers,
            string hostPersonaName,
            string hostUserName,
            bool isHost)
        {
            LobbyValid = lobbyValid;
            LobbyOwnerSteamId = lobbyOwnerSteamId;
            LocalSteamId = localSteamId;
            PlayerSteamIds = playerSteamIds;
            TotalTrackedPlayers = totalTrackedPlayers;
            HostPersonaName = hostPersonaName;
            HostUserName = hostUserName;
            IsHost = isHost;
        }

        /// <summary>
        /// Captures live diagnostics from the running game. Returns null if game state is unavailable.
        /// </summary>
        public static DesyncDiagnostics Capture()
        {
            bool lobbyValid = false;
            string lobbyOwnerSteamId = "";
            string localSteamId = "";
            var playerSteamIds = new List<string>();
            int totalTrackedPlayers = 0;
            string hostPersonaName = "";
            string hostUserName = "";
            bool isHost = NetworkManager.main?.isHost ?? false;

            try
            {
                MultiplayerManager? mm = MonoSingleton<MultiplayerManager>.I;
                lobbyValid = mm?.LobbyStatus ?? false;
                lobbyOwnerSteamId = Alpha.Core.Util.SteamUtils.GetLobbyOwnerSteamID();
                localSteamId = Alpha.Core.Util.SteamUtils.GetPlayerSteamID();
            }
            catch { }

            try
            {
                PlayerPanelController? controller = NetworkSingleton<PlayerPanelController>.I;
                if (controller != null)
                {
                    totalTrackedPlayers = controller.PlayerIDs.Count;
                    playerSteamIds = new List<string>(controller.PlayerSteamIDs);
                }
            }
            catch { }

            try
            {
                if (!string.IsNullOrEmpty(lobbyOwnerSteamId) && lobbyOwnerSteamId != "0")
                {
                    hostPersonaName = SteamFriends.GetFriendPersonaName(
                        new CSteamID(ulong.Parse(lobbyOwnerSteamId)));
                    // Try to get user name from panel
                    PlayerPanelController? controller = NetworkSingleton<PlayerPanelController>.I;
                    if (controller != null)
                    {
                        int idx = controller.PlayerSteamIDs.IndexOf(lobbyOwnerSteamId);
                        if (idx >= 0)
                        {
                            hostUserName = controller.PlayerTransforms[idx]
                                .GetComponent<PlayerController>().PlayerNameText.text;
                        }
                    }
                }
            }
            catch { }

            return new DesyncDiagnostics(
                lobbyValid, lobbyOwnerSteamId, localSteamId, playerSteamIds,
                totalTrackedPlayers,
                hostPersonaName, hostUserName, isHost);
        }

        /// <summary>True if lobby owner SteamID appears invalid.</summary>
        public bool IsLobbyOwnerInvalid =>
            string.IsNullOrEmpty(LobbyOwnerSteamId) || LobbyOwnerSteamId == "0";

        /// <summary>True if local player is the lobby owner but NetworkManager does not recognize us as host.</summary>
        public bool IsServerIdentityLost =>
            !IsLobbyOwnerInvalid && LocalSteamId == LobbyOwnerSteamId && !IsHost;

        /// <summary>True if lobby owner is not in the tracked player SteamID list.</summary>
        public bool IsHostMissingFromList =>
            !IsLobbyOwnerInvalid && !PlayerSteamIds.Contains(LobbyOwnerSteamId);

        /// <summary>True if host persona name is empty but they should be resolvable.</summary>
        public bool IsPersonaCacheStale =>
            !IsLobbyOwnerInvalid && !IsHostMissingFromList && string.IsNullOrEmpty(HostPersonaName);
    }
}
