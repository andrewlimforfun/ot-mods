using System;
using System.Xml;
using Remind;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Remind.Core.Commands
{
    public class RemindGlobalAtCommand : IChatCommand
    {
        public const string CMD = "remindglobalat";
        public string Name => CMD;
        public string ShortName => "rga";
        public string Description => $"Send a global chat message at a specific time. Usage: /{CMD} [HH:mm] [message]";

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

            string timePart = args[0];
            string username = PlayerUtils.GetUserNameNoFormat();

            if (!DateTime.TryParse(timePart, out DateTime time))
            {
                ChatUtils.AddGlobalNotification("Invalid time format. Please use HH:mm.");
                return;
            }

            bool isLocal = false;
            DateTime utc = time.Kind == DateTimeKind.Utc ? time : time.ToUniversalTime();

            if (!RemindPlugin.ScheduledTaskManager.TryScheduleAt(timePart,
                () => ChatUtils.SendMessageAsync(RemindPlugin.ModName + username, message, isLocal), out ScheduledTask? task, out string error))
            {
                ChatUtils.AddGlobalNotification(error);
                return;
            }
            ChatUtils.AddGlobalNotification($"Message scheduled for {utc:HH:mm}UTC.");
            if (RemindPlugin.ChatConfirmation?.Value == true) {
                ChatUtils.SendMessageAsync(RemindPlugin.ModName + username, $"scheduled at {utc:HH:mm}UTC -> {message}", isLocal);
            }

        }
    }
}
