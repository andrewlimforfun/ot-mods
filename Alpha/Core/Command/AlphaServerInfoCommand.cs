using System;
using System.Collections.Generic;
using System.Linq;
using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using PurrNet;
using PurrLobby;
using UnityEngine;

namespace Alpha.Core.Commands
{
    /// <summary>
    /// /alphaserverinfo - shows server and lobby health diagnostics.
    /// </summary>
    public class AlphaServerInfoCommand : IChatCommand
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Alpha.ASIC");

        public string Name => "alphaserverinfo";
        public string ShortName => "asi";
        public string Description => "Show server information. Usage: /alphaserverinfo";
        public string Namespace => "alpha";

        public void Execute(string[] args)
        {
            var host = PlayerUtils.GetHost();

            string duration = "unknown";
            if (AlphaPlugin.ServerJoinTime.HasValue)
            {
                TimeSpan elapsed = DateTime.UtcNow - AlphaPlugin.ServerJoinTime.Value;
                duration = $"{(int)elapsed.TotalHours:D2}h {elapsed.Minutes:D2}m";
            }

            // Lobby health diagnostics
            string lobbyValid = "unknown";
            string lobbyOwnerSteamId = "unknown";
            int serverPlayerCount = 0;
            int totalTrackedPlayers = 0;
            try
            {
                MultiplayerManager? mm = MonoSingleton<MultiplayerManager>.I;
                lobbyValid = mm?.LobbyStatus.ToString() ?? "null";
                lobbyOwnerSteamId = SteamUtils.GetLobbyOwnerSteamID();
            }
            catch (Exception ex)
            {
                lobbyValid = $"error: {ex.Message}";
            }

            try
            {
                var controller = NetworkSingleton<PlayerPanelController>.I;
                if (controller != null)
                {
                    totalTrackedPlayers = controller.PlayerIDs.Count;
                    serverPlayerCount = controller.PlayerIDs.Count(pid => pid.isServer);
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning($"Error reading PlayerIDs: {ex.Message}");
            }

            string msg =
                $"Lobby Name: {PlayerUtils.GetLobbyName()}\n" +
                $"Lobby Code: {PlayerUtils.GetLobbyCode()}\n" +
                $"Player Count: {PlayerUtils.GetPlayerCount()}/{PlayerUtils.GetMaxPlayers()}\n" +
                $"Lobby Valid: {lobbyValid}\n" +
                $"isServer players: {serverPlayerCount}/{totalTrackedPlayers}\n" +
                $"Session Duration: {duration}\n" +
                $"Host SteamID (lobby owner): {lobbyOwnerSteamId}\n" +
                $"Host SteamID (panel match): {host?.SteamID}\n" +
                $"Host Steam: {host?.SteamPersonaName}\n" +
                $"Host Name: {host?.UserName}";

            _log.LogInfo(msg);
            ChatUtils.AddGlobalNotification(msg);
        }
    }
}
