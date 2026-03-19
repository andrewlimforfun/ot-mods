using System;
using System.Threading.Tasks;
using BepInEx.Logging;
using Fomo.Core;

namespace FomoWebSocket.Sinks
{
    /// <summary>
    /// Broadcasts each <see cref="ChatEntry"/> to all connected WebSocket clients.
    /// </summary>
    public class WebSocketChatSink : IChatSink
    {
        private readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{FomoWebSocketPlugin.ModName}.WSCS");
        private const int WarnLimit = 20;
        private int _warnCount = 0;

        public Task SendAsync(ChatEntry entry)
        {
            try
            {
                var wsManager = FomoWebSocketPlugin.WsManager;
                if (wsManager != null && wsManager.IsRunning)
                {
                    if (entry.IsNotification)
                    {
                        string formattedEntry = ChatEntryFormatter.Format(entry, FomoWebSocketPlugin.NotificationFormat?.Value);
                        wsManager.Broadcast(formattedEntry);
                    }
                    else
                    {
                        string formattedEntry = ChatEntryFormatter.Format(entry, FomoWebSocketPlugin.MessageFormat?.Value);
                        wsManager.Broadcast(formattedEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                _warnCount++;
                if (_warnCount <= WarnLimit)
                    _log.LogWarning($"Failed to broadcast WebSocket message: {ex.Message}");
                else if (_warnCount == WarnLimit + 1)
                    _log.LogWarning("Further warnings about WebSocket message broadcasting will be suppressed.");
            }

            return Task.CompletedTask;
        }
    }
}
