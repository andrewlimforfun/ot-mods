using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoChatLog.Commands
{
    public class FomoChatLogNotificationFormatCommand : IChatCommand
    {
        public const string CMD = "fomochatlognotificationformat";
        public string Name => CMD;
        public string ShortName => "fclnf";
        public string Description => "Set or get the chat log notification format.";
        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoChatLogPlugin.EnableFeature?.Value != true )
            {
                ChatUtils.AddGlobalNotification("Chat log feature is disabled.");
                return;
            }

            if (FomoChatLogPlugin.NotificationFormat == null)
            {
                ChatUtils.AddGlobalNotification("Fomo Chat Log Mod is not initialized yet.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"Chat log notification format: {FomoChatLogPlugin.NotificationFormat?.Value}");
                return;
            }

            if (args.Length > 0)
            {
                string newFormat = string.Join(" ", args);
                if (string.IsNullOrWhiteSpace(newFormat))
                {
                    ChatUtils.AddGlobalNotification("Please enter a valid chat log notification format.");
                    return;
                }
                FomoChatLogPlugin.NotificationFormat.Value = newFormat;
                ChatUtils.AddGlobalNotification($"Chat log notification format is now set to: {newFormat}.");
            }
        }
    }
}
