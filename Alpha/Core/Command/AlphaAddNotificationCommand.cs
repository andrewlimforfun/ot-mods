using System;
using System.Collections.Generic;
using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using UnityEngine;

namespace Alpha.Core.Commands
{
    /// <summary>
    /// /alphamyposition - shows your current position in the game world.
    /// </summary>
    public class AlphaAddNotificationCommand : IChatCommand
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Alpha.AANPC");

        public string Name => "alphaaddnotification";
        public string ShortName => "aan";
        public string Description => "Add a notification. Usage: /alphaaddnotification [message]";
        public string Namespace => "alpha";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /alphaaddnotification [message]");
                return;
            }

            string message = string.Join(" ", args).Trim();
            _log.LogInfo($"Adding notification: {message}");
            ChatUtils.AddGlobalNotification(message);
        }
    }
}
