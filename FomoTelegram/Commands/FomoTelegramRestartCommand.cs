using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoTelegram.Commands
{
    public class FomoTelegramRestartCommand : IChatCommand
    {
        public string Name => "fomotelegramrestart";
        public string ShortName => "ftr";
        public string Description => "Reload config and restart the Telegram connection.";
        public string Namespace => "fomo";

        public void Execute(string[] args)
        {
            string result = FomoTelegramPlugin.RestartTelegram();
            ChatUtils.AddGlobalNotification(result);
        }
    }
}
