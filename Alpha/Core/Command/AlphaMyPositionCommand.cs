using Alpha.Core.Command;
using Alpha.Core.Util;
using PurrNet;
using UnityEngine;

namespace Alpha.Core.Commands
{
    /// <summary>
    /// /alphamyposition — shows your current position in the game world.
    /// </summary>
    public class AlphaMyPositionCommand : IChatCommand
    {
        public string Name => "alphamyposition";
        public string ShortName => "amp";
        public string Description => "Show your current position. Usage: /alphamyposition";
        public string Namespace => "alpha";

        public void Execute(string[] args)
        {
            // NetworkTransform.position is only valid for remote players; use MainPlayer.position for local
            Vector3 myPosition = PlayerUtils.GetPlayerDetail()?.Position ?? Vector3.zero;
            ChatUtils.AddGlobalNotification($"Your current position: {myPosition}");
        }
    }
}
