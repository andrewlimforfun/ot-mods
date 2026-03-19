using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoTelegram.Commands
{
    public class FomoTelegramMessageFormatCommand : IChatCommand
    {
        public const string CMD = "fomotelegrammessageformat";
        public string Name => CMD;
        public string ShortName => "ftmf";
        public string Description => "Set or get the Telegram message format.";
        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoTelegramPlugin.EnableFeature?.Value != true )
            {
                ChatUtils.AddGlobalNotification("Telegram feature is disabled.");
                return;
            }

            if (FomoTelegramPlugin.MessageFormat == null)
            {
                ChatUtils.AddGlobalNotification("Fomo Telegram Mod is not initialized yet.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"Telegram message format: {FomoTelegramPlugin.MessageFormat?.Value}");
                return;
            }

            if (args.Length > 0)
            {
                string newFormat = string.Join(" ", args);
                if (string.IsNullOrWhiteSpace(newFormat))
                {
                    ChatUtils.AddGlobalNotification("Please enter a valid Telegram message format.");
                    return;
                }
                FomoTelegramPlugin.MessageFormat.Value = newFormat;
                ChatUtils.AddGlobalNotification($"Telegram message format is now set to: {newFormat}.");
            }
        }
    }
}
