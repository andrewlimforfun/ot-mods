using System.Globalization;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Reconnect.Core.Commands
{
    public class ReconnectCooldownCommand : IChatCommand
    {
        public string Name => "reconnectcooldown";
        public string ShortName => "rccd";
        public string Description => "Get or set cooldown between sequences in seconds (10-120). Usage: /rccd [value]";
        public string Namespace => "reconnect";

        public void Execute(string[] args)
        {
            if (ReconnectPlugin.CooldownSec == null) return;

            if (args.Length >= 1)
            {
                if (!float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
                    || value < 10f || value > 120f)
                {
                    ChatUtils.AddGlobalNotification("Usage: /rccd [10-120]");
                    return;
                }
                ReconnectPlugin.CooldownSec.Value = value;
            }

            ChatUtils.AddGlobalNotification($"CooldownSec = {ReconnectPlugin.CooldownSec.Value}");
        }
    }
}
