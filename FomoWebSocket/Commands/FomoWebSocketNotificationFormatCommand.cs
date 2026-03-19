using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoWebSocket.Commands
{
    public class FomoWebSocketNotificationFormatCommand : IChatCommand
    {
        public const string CMD = "fomowebsocketnotificationformat";
        public string Name => CMD;
        public string ShortName => "fwsnf";
        public string Description => "Set or get the WebSocket notification message format.";
        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoWebSocketPlugin.EnableFeature?.Value != true )
            {
                ChatUtils.AddGlobalNotification("WebSocket feature is disabled.");
                return;
            }

            if (FomoWebSocketPlugin.NotificationFormat == null || FomoWebSocketPlugin.WsManager is null)
            {
                ChatUtils.AddGlobalNotification("Fomo WebSocket Mod is not initialized yet.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"WebSocket notification message format: {FomoWebSocketPlugin.NotificationFormat?.Value}");
                return;
            }

            if (args.Length > 0)
            {
                string newFormat = string.Join(" ", args);
                if (string.IsNullOrWhiteSpace(newFormat))
                {
                    ChatUtils.AddGlobalNotification("Please enter a valid WebSocket notification message format.");
                    return;
                }
                FomoWebSocketPlugin.NotificationFormat.Value = newFormat;
                ChatUtils.AddGlobalNotification($"WebSocket notification message format is now set to: {newFormat}.");
            }
        }
    }
}
