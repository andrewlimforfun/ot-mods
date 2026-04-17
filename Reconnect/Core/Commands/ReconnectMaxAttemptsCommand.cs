using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Reconnect.Core.Commands
{
    public class ReconnectMaxAttemptsCommand : IChatCommand
    {
        public string Name => "reconnectmaxattempts";
        public string ShortName => "rcma";
        public string Description => "Get or set max reconnect attempts (1-10). Usage: /rcma [value]";
        public string Namespace => "reconnect";

        public void Execute(string[] args)
        {
            if (ReconnectPlugin.MaxAttempts == null) return;

            if (args.Length >= 1)
            {
                if (!int.TryParse(args[0], out int value) || value < 1 || value > 100)
                {
                    ChatUtils.AddGlobalNotification("Usage: /rcma [1-100]");
                    return;
                }
                ReconnectPlugin.MaxAttempts.Value = value;
            }

            ChatUtils.AddGlobalNotification($"MaxAttempts = {ReconnectPlugin.MaxAttempts.Value}");
        }
    }
}
