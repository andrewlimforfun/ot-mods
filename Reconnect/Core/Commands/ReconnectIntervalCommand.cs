using System.Globalization;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Reconnect.Core.Commands
{
    public class ReconnectIntervalCommand : IChatCommand
    {
        public string Name => "reconnectinterval";
        public string ShortName => "rciv";
        public string Description => "Get or set seconds between attempts (2-30). Usage: /rciv [value]";
        public string Namespace => "reconnect";

        public void Execute(string[] args)
        {
            if (ReconnectPlugin.AttemptIntervalSec == null) return;

            if (args.Length >= 1)
            {
                if (!float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
                    || value < 2f || value > 30f)
                {
                    ChatUtils.AddGlobalNotification("Usage: /rciv [2-30]");
                    return;
                }
                ReconnectPlugin.AttemptIntervalSec.Value = value;
            }

            ChatUtils.AddGlobalNotification($"AttemptIntervalSec = {ReconnectPlugin.AttemptIntervalSec.Value}");
        }
    }
}
