using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoWebSocket.Commands
{
    public class FomoWebSocketPortCommand : IChatCommand
    {
        public const string CMD = "fomowebsocketport";
        public string Name => CMD;
        public string ShortName => "fwsp";
        public string Description => "Set or get the WebSocket chat port. Default (8765).";
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

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"Current WebSocket chat port: {FomoWebSocketPlugin.WebSocketChatPort.Value}");
                return;
            }

            if (args.Length > 0)
            {
                string newPortStr = args[0];
                if (!int.TryParse(newPortStr, out int newPort) || newPort <= 0)
                {
                    ChatUtils.AddGlobalNotification("Please enter a valid WebSocket port value.");
                    return;
                }
                FomoWebSocketPlugin.WebSocketChatPort.Value = newPort;
                ChatUtils.AddGlobalNotification($"WebSocket chat port is now set to {newPort}.");
            }
        }
    }
}
