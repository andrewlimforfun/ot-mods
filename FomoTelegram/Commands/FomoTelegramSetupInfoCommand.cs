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
            ChatUtils.AddGlobalNotification("To set up Telegram integration you need:");
            ChatUtils.AddGlobalNotification("1. A Telegram Bot API key from @BotFather.");
            ChatUtils.AddGlobalNotification("2. Your Telegram chat ID from @userinfobot.");
            ChatUtils.AddGlobalNotification("3. Update the API key and chat ID in the mod configuration.");
            ChatUtils.AddGlobalNotification("Config path: " + FomoTelegramPlugin.ConfigPath);
            ChatUtils.AddGlobalNotification("4. Then restart the game.");
            ChatUtils.AddGlobalNotification("Inputting API key and chat ID via chat commands is not supported for security reasons. Please update the config file directly.");
        }
    }
}
