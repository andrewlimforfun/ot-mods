using System;
using System.Collections.Generic;
using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using PurrLobby;
using UnityEngine;

namespace Alpha.Core.Commands
{
    /// <summary>
    /// /alphamyposition - shows your current position in the game world.
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
            ChatUtils.AddGlobalNotification(
                $"Lobby Name: {PlayerUtils.GetLobbyName()}\n" +
                $"Lobby Code: {PlayerUtils.GetLobbyCode()}\n" +
                $"Player Count: {PlayerUtils.GetPlayerCount()}/{PlayerUtils.GetMaxPlayers()}\n" +
                $"Host: {host?.UserName} (Steam: {host?.SteamPersonaName})");
        }
    }
}
