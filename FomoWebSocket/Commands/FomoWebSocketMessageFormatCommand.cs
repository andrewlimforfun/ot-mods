using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoWebSocket.Commands
{
    public class FomoWebSocketMessageFormatCommand : IChatCommand
    {
        public const string CMD = "fomowebsocketmessageformat";
        public string Name => CMD;
        public string ShortName => "fwsmf";
        public string Description => "Set or get the WebSocket chat message format.";
        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoWebSocketPlugin.EnableFeature?.Value != true )
            {
                ChatUtils.AddGlobalNotification("WebSocket feature is disabled.");
                return;
            }

            if (FomoWebSocketPlugin.MessageFormat == null || FomoWebSocketPlugin.WsManager is null)
            {
                ChatUtils.AddGlobalNotification("Fomo WebSocket Mod is not initialized yet.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"WebSocket message format: {FomoWebSocketPlugin.MessageFormat?.Value}");
                return;
            }

            if (args.Length > 0)
            {
                string newFormat = string.Join(" ", args);
                if (string.IsNullOrWhiteSpace(newFormat))
                {
                    ChatUtils.AddGlobalNotification("Please enter a valid WebSocket message format.");
                    return;
                }
                FomoWebSocketPlugin.MessageFormat.Value = newFormat;
                ChatUtils.AddGlobalNotification($"WebSocket message format is now set to: {newFormat}.");
            }
        }
    }
}
