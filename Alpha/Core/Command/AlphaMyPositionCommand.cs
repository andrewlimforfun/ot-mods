using System;
using System.Collections.Generic;
using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using UnityEngine;

namespace Alpha.Core.Commands
{
    /// <summary>
    /// /alphamyposition — shows your current position in the game world.
    /// </summary>
    public class AlphaMyPositionCommand : IChatCommand
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Alpha.AMPC");

        public string Name => "alphamyposition";
        public string ShortName => "amp";
        public string Description => "Show your current position. Usage: /alphamyposition";
        public string Namespace => "alpha";

        public void Execute(string[] args)
        {
            var myPosition = PlayerUtils.GetPlayerDetail()?.PlayerTransform.position ?? Vector3.zero;
            var positionStr = myPosition.ToString();
            ChatUtils.AddGlobalNotification($"Your current position: {positionStr}");
        }
    }
}
