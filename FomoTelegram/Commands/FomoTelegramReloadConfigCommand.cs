using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoTelegram.Commands
{
    public class FomoTelegramReloadConfigCommand : IChatCommand
    {
        public string Name => "fomotelegramreloadconfig";
        public string ShortName => "ftrc";
        public string Description => "Reload Telegram config from disk and show current API key and chat ID.";
        public string Namespace => "fomo";

        public void Execute(string[] args)
        {
            string summary = FomoTelegramPlugin.ReloadConfig();
            ChatUtils.AddGlobalNotification($"Config reloaded.\n{summary}");
        }
    }
}
