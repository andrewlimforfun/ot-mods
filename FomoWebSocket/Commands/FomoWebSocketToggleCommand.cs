using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoWebSocket.Commands
{
    public class FomoWebSocketCommand : IChatCommand
    {
        public const string CMD = "fomowebsockettoggle";
        public string Name => CMD;
        public string ShortName => "fwst";
        public string Description => "Toggle WebSocket chat feature on/off.";
        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoWebSocketPlugin.EnableFeature?.Value != true )
            {
                ChatUtils.AddGlobalNotification("WebSocket feature is disabled.");
                return;
            }

            if (FomoWebSocketPlugin.WebSocketChatPort == null)
            {
                ChatUtils.AddGlobalNotification("WebSocket config is not initialized yet.");
                return;
            }

            if (FomoWebSocketPlugin.WsManager is null)
            {
                ChatUtils.AddGlobalNotification("WebSocketManager is not initialized yet.");
                return;
            }
            
            var wsManager = FomoWebSocketPlugin.WsManager;

            // toggle feature on/off
            FomoWebSocketPlugin.EnableFeature.Value = !FomoWebSocketPlugin.EnableFeature.Value;
            ChatUtils.AddGlobalNotification($"Fomo WebSocket chat feature is now {(FomoWebSocketPlugin.EnableFeature.Value ? "enabled" : "disabled")}.");

            // start/stop WebSocket server based on new feature state
            if (FomoWebSocketPlugin.EnableFeature.Value && !wsManager.IsRunning)
            {
                wsManager.Start();
            }
            else if (!FomoWebSocketPlugin.EnableFeature.Value && wsManager.IsRunning)
            {
                wsManager.Stop();
            }
        }
    }
}
