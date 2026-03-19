using Remind;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Remind.Core.Commands
{
    public class RemindMeAtCommand : IChatCommand
    {
        public const string CMD = "remindmeat";
        public string Name => CMD;
        public string ShortName => "rma";
        public string Description => $"Show a private notification at a specific time. Usage: /{CMD} [HH:mm] [message]";

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
                ChatUtils.AddGlobalNotification($"Usage: /{CMD} [HH:mm] [message]");
                return;
            }

            string message = string.Join(" ", args, 1, args.Length - 1);
            if (string.IsNullOrWhiteSpace(message))
            {
                ChatUtils.AddGlobalNotification("Please enter a valid message.");
                return;
            }

            if (!RemindPlugin.ScheduledTaskManager.TryScheduleAt(args[0], () =>
                ChatUtils.AddGlobalNotification(message), out _, out string error))
            {
                ChatUtils.AddGlobalNotification(error);
                return;
            }
            ChatUtils.AddGlobalNotification($"[{CMD}] Reminder at {args[0]}: \"{message}\"");
        }
    }
}
