using Remind;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Remind.Core.Commands
{
    public class RemindChatConfirmationCommand : IChatCommand
    {
        public const string CMD = "remindchatconfirmation";
        public string Name => CMD;
        public string ShortName => "rcc";
        public string Description => "Toggle Remind chat confirmation on/off. Currently: " + (RemindPlugin.ChatConfirmation?.Value == true ? "ON" : "OFF");

        public string Namespace => "remind";
        public void Execute(string[] args)
        {
            if (RemindPlugin.ChatConfirmation == null)
            {
                return;
            }

            RemindPlugin.ChatConfirmation.Value = !RemindPlugin.ChatConfirmation.Value;
            ChatUtils.AddGlobalNotification($"Remind broadcast creation is now {(RemindPlugin.ChatConfirmation.Value ? "enabled" : "disabled")}.");
        }
    }
}
