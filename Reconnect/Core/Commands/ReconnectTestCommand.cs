using Alpha.Core.Command;
using Alpha.Core.Util;
using PurrNet;

namespace Reconnect.Core.Commands
{
    public class ReconnectTestCommand : IChatCommand
    {
        public string Name => "reconnecttest";
        public string ShortName => "rctest";
        public string Description => "Simulate an unintentional disconnect to test the reconnect sequence.";
        public string Namespace => "reconnect";

        public void Execute(string[] args)
        {
            if (ReconnectPlugin.Enabled?.Value != true)
            {
                ChatUtils.AddGlobalNotification("Reconnect is disabled. Enable it first with /rct on");
                return;
            }

            if (ReconnectPlugin.IsReconnecting)
            {
                ChatUtils.AddGlobalNotification("Already reconnecting.");
                return;
            }

            if (NetworkManager.main.isHost)
            {
                ChatUtils.AddGlobalNotification("Cannot test reconnect as host.");
                return;
            }

            ChatUtils.AddGlobalNotification("Simulating unintentional disconnect...");
            ReconnectPlugin.IsIntentionalLeave = false;
            NetworkManager.main.StopClient();
        }
    }
}
