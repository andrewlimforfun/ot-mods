using System;
using System.Collections.Generic;
using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using UnityEngine;

namespace Alpha.Core.Commands
{
    /// <summary>
    /// /alphawhois - shows information about a player.
    /// </summary>
    public class AlphaWhoIsCommand : IChatCommand
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Alpha.AWIC");

        public string Name => "alphawhois";
        public string ShortName => "awi";
        public string Description => "Show information about a player. Usage: /alphawhois [player]";
        public string Namespace => "alpha";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /alphawhois [player]");
                return;
            }

            string query = string.Join(" ", args).Trim();

            List<PlayerDetail> matches = PlayerUtils.FindPlayersByQuery(query);
            if (matches.Count == 0)
            {
                ChatUtils.AddGlobalNotification($"No players found matching \"{query}\".");
                return;
            }

            string header = $"Found {matches.Count} player(s) matching \"{query}\":";
            var sb = new System.Text.StringBuilder(header);
            foreach (PlayerDetail p in matches)
            {
                string cleanName = ChatUtils.CleanTMPTags(p.UserName).Trim();
                string persona = p.SteamPersonaName ?? "?";
                var myPosition = PlayerUtils.GetPlayerDetail()?.Position ?? Vector3.zero;
                var distance = (int)Vector3.Distance(p.Position, myPosition);

                sb.Append($"\n| {cleanName} | {persona} | {p.SteamID} | {p.Position} | {distance} |");
            }

            string msg = sb.ToString();
            _log.LogInfo(msg);
            ChatUtils.AddGlobalNotification(msg);
        }
    }
}
