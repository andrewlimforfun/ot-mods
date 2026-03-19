using System;
using System.IO;
using Fomo;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Fomo.Core.Commands
{
    public class FomoPlayerIdCommand : IChatCommand
    {
        public const string CMD = "fomoplayerid";
        public string Name => CMD;
        public string ShortName => "fpi";
        public string Description => "Get the current player's ID. Usage: /fomogetplayerid";

        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            var playerId = PlayerUtils.GetPlayerId();
            if (playerId.HasValue)
            {
                ChatUtils.AddGlobalNotification($"Current player ID: {playerId.Value}");
            }

            var steamPlayerId = PlayerUtils.GetSteamPlayerIdString();
            if (!string.IsNullOrEmpty(steamPlayerId))
            {
                ChatUtils.AddGlobalNotification($"Current Steam player ID: {steamPlayerId}");
            }
        }
    }
}
