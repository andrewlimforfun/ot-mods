using Alpha.Core.Command;
using Alpha.Core.Util;
using Reconnect.Core;

namespace Reconnect.Core.Commands
{
    public class ReconnectFocusCommand : IChatCommand
    {
        public string Name => "reconnectfocus";
        public string ShortName => "rcf";
        public string Description => "Repair broken focus areas after reconnect.";
        public string Namespace => "reconnect";

        public void Execute(string[] args)
        {
            if (!FocusAreaFix.ShouldRepair())
            {
                ChatUtils.AddGlobalNotification("Focus areas are working normally (AreaIndexes is populated).");
                return;
            }

            bool success = FocusAreaFix.Apply();
            if (success)
                ChatUtils.AddGlobalNotification("Sent focus area re-sync request to server.");
            else
                ChatUtils.AddGlobalNotification("Failed to repair - FocusAreaManager or MainNetTransform not available.");
        }
    }
}
