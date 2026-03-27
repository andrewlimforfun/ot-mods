using System.Collections.Generic;
using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echowhois <query> — lists all players whose in-game name contains the query
    /// or whose Steam ID ends with the query (if all digits).
    /// Shows: in-game name (TMP stripped), Steam persona name, Steam ID.
    /// </summary>
    public class EchoWhoIsCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.EchoWhoIsCommand");

        public string Name => "echowhois";
        public string ShortName => "ewi";
        public string Description => "List matching players with their Steam persona and Steam ID. Usage: /echowhois <name|steamid_suffix>";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /echowhois <name|steamid_suffix>");
                return;
            }

            string query = string.Join(" ", args).Trim();
            bool allDigits = System.Text.RegularExpressions.Regex.IsMatch(query, @"^\d+$");

            List<PlayerDetail> matches = PlayerUtils.FindAllPlayers((steamId, _pid, _info, _transform) =>
            {
                if (allDigits && steamId.EndsWith(query))
                    return true;

                var detail = new PlayerDetail(steamId, _pid, _info, _transform);
                string cleanName = ChatUtils.CleanTMPTags(detail.UserName).Trim();
                return cleanName.Contains(query, System.StringComparison.OrdinalIgnoreCase);
            });

            if (matches.Count == 0)
            {
                ChatUtils.AddGlobalNotification($"No players found matching \"{query}\".");
                return;
            }

            ChatUtils.AddGlobalNotification($"Found {matches.Count} player(s) matching \"{query}\":");
            foreach (PlayerDetail p in matches)
            {
                string cleanName = ChatUtils.CleanTMPTags(p.UserName).Trim();
                string persona = p.SteamPersonaName ?? "?";
                Logger.LogInfo($" {cleanName} | Steam: {persona} | ID: {p.SteamID}");
                ChatUtils.AddGlobalNotification($" {cleanName} | Steam: {persona} | ID: {p.SteamID}");
            }
        }
    }
}
