using System.Linq;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoTelegram.Commands
{
    public class FomoTelegramNotificationFormatCommand : IChatCommand
    {
        public const string CMD = "fomotelegramnotificationformat";
        public string Name => CMD;
        public string ShortName => "ftnf";
        public string Description => "Set or get the Telegram notification format.";
        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoTelegramPlugin.EnableFeature?.Value != true )
            {
                ChatUtils.AddGlobalNotification("Telegram feature is disabled.");
                return;
            }

            if (FomoTelegramPlugin.NotificationFormat == null)
            {
                ChatUtils.AddGlobalNotification("Fomo Telegram Mod is not initialized yet.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"Telegram notification format: {FomoTelegramPlugin.NotificationFormat?.Value}");
                return;
            }

            if (args.Length > 0)
            {
                string newFormat = string.Join(" ", args);
                if (string.IsNullOrWhiteSpace(newFormat))
                {
                    ChatUtils.AddGlobalNotification("Please enter a valid Telegram notification format.");
                    return;
                }
                FomoTelegramPlugin.NotificationFormat.Value = newFormat;
                ChatUtils.AddGlobalNotification($"Telegram notification format is now set to: {newFormat}.");
            }
        }
    }
}
