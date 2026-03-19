using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoTelegram.Commands
{
    public class FomoTelegramToggleCommand : IChatCommand
    {
        public const string CMD = "fomotelegramtoggle";
        public string Name => CMD;
        public string ShortName => "ftt";
        public string Description => "Toggle Telegram chat feature on/off.";

        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoTelegramPlugin.EnableFeature == null)
                return;

            FomoTelegramPlugin.EnableFeature.Value = !FomoTelegramPlugin.EnableFeature.Value;
            ChatUtils.AddGlobalNotification($"Fomo Telegram is now {(FomoTelegramPlugin.EnableFeature.Value ? "enabled" : "disabled")}.");
        }
    }
}
