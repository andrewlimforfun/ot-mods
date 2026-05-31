using Alpha.Core.Command;
using Alpha.Core.Util;

namespace QuickHost.Core.Commands
{
    public class QuickHostToggleCommand : IChatCommand
    {
        public string Name => "quickhosttoggle";
        public string ShortName => "qht";
        public string Description => "Toggle QuickHost auto-lobby on/off for next launch.";
        public string Namespace => "quickhost";

        public void Execute(string[] args)
        {
            if (QuickHostPlugin.Enabled == null) return;
            QuickHostPlugin.Enabled.Value = !QuickHostPlugin.Enabled.Value;
            ChatUtils.AddGlobalNotification($"QuickHost is now {(QuickHostPlugin.Enabled.Value ? "enabled" : "disabled")} (takes effect next launch).");
        }
    }
}
