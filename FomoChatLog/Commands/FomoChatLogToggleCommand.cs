using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoChatLog.Commands
{
    public class FomoChatLogToggleCommand : IChatCommand
    {
        public const string CMD = "fomochatlogtoggle";
        public string Name => CMD;
        public string ShortName => "fclt";
        public string Description => "Toggle chat file logging on/off.";

        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoChatLogPlugin.EnableFeature == null)
                return;

            FomoChatLogPlugin.EnableFeature.Value = !FomoChatLogPlugin.EnableFeature.Value;
            ChatUtils.AddGlobalNotification($"Fomo chat log is now {(FomoChatLogPlugin.EnableFeature.Value ? "enabled" : "disabled")}.");
        }
    }
}
