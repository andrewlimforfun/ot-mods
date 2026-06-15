using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Desync.Core.Commands
{
    public class DesyncIntervalCommand : IChatCommand
    {
        public string Name => "desyncinterval";
        public string ShortName => "dsi";
        public string Description => "Set health check interval in seconds. Usage: /desyncinterval <seconds>";
        public string Namespace => "desync";

        public void Execute(string[] args)
        {
            if (DesyncPlugin.MonitorIntervalSec == null) return;

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"Desync interval: {DesyncPlugin.MonitorIntervalSec.Value}s (min 60).");
                return;
            }

            if (!float.TryParse(args[0], out float seconds))
            {
                ChatUtils.AddGlobalNotification("Invalid number. Usage: /desyncinterval <seconds>");
                return;
            }

            if (seconds < 60f)
                seconds = 60f;

            DesyncPlugin.MonitorIntervalSec.Value = seconds;
            ChatUtils.AddGlobalNotification($"Desync interval set to {seconds}s.");
        }
    }
}
