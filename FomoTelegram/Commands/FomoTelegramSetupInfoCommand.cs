using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoTelegram.Commands
{
    public class FomoTelegramSetupInfoCommand : IChatCommand
    {
        public const string CMD = "fomotelegramsetupinfo";
        public string Name => CMD;
        public string ShortName => "ftsinfo";
        public string Description => "Instruction to set up the Telegram integration.";
        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            ChatUtils.AddGlobalNotification(
                "To set up Telegram integration you need:\n" +
                "1. A Telegram Bot API key from @BotFather.\n" +
                "2. Your Telegram chat ID from @userinfobot.\n" +
                "3. Update the API key and chat ID in the mod configuration.\n" +
                "Config path: " + FomoTelegramPlugin.ConfigPath + "\n" +
                "4. Reload config with /ftrc.\n" +
                "5. Restart telegram manager with /ftr.");
        }
    }
}
