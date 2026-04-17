using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Reconnect.Core.Commands
{
    public class ReconnectToggleCommand : IChatCommand
    {
        public string Name => "reconnecttoggle";
        public string ShortName => "rct";
        public string Description => "Get or set auto-reconnect on/off. Usage: /rct [on|off]";
        public string Namespace => "reconnect";

        public void Execute(string[] args)
        {
            if (ReconnectPlugin.Enabled == null) return;

            if (args.Length >= 1)
            {
                string arg = args[0].ToLowerInvariant();
                if (arg == "on" || arg == "true" || arg == "1")
                    ReconnectPlugin.Enabled.Value = true;
                else if (arg == "off" || arg == "false" || arg == "0")
                    ReconnectPlugin.Enabled.Value = false;
                else
                {
                    ChatUtils.AddGlobalNotification("Usage: /rct [on|off]");
                    return;
                }
            }
            else
            {
                ReconnectPlugin.Enabled.Value = !ReconnectPlugin.Enabled.Value;
            }

            ChatUtils.AddGlobalNotification($"Reconnect is now {(ReconnectPlugin.Enabled.Value ? "enabled" : "disabled")}.");
        }
    }
}
