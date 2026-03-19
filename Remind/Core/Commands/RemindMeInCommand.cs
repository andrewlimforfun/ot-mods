using System;
using Remind;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Remind.Core.Commands
{
    public class RemindMeInCommand : IChatCommand
    {
        public const string CMD = "remindmein";
        public string Name => CMD;
        public string ShortName => "rmi";
        public string Description => $"Show a private notification after a delay. Usage: /{CMD} [hh:mm:ss] [message]";

        public string Namespace => "remind";
        public void Execute(string[] args)
        {
            if (RemindPlugin.ScheduledTaskManager == null)
            {
                ChatUtils.AddGlobalNotification("ScheduledTaskManager is not initialized.");
                return;
            }

            if (args.Length < 2)
            {
                ChatUtils.AddGlobalNotification($"Usage: /{CMD} [hh:mm:ss] [message]");
                return;
            }

            string message = string.Join(" ", args, 1, args.Length - 1);
            if (string.IsNullOrWhiteSpace(message))
            {
                ChatUtils.AddGlobalNotification("Please enter a valid message.");
                return;
            }

            string timePart = args[0];

            if (!RemindPlugin.ScheduledTaskManager.TryScheduleIn(timePart, () =>
                ChatUtils.AddGlobalNotification(message), out _, out string error))
            {
                ChatUtils.AddGlobalNotification(error);
                return;
            }
            ChatUtils.AddGlobalNotification($"[{CMD}] Reminder in {timePart}: \"{message}\"");
        }
    }
}
