using System;
using System.Collections.Concurrent;
using BepInEx.Logging;
using Fomo.Core;
using Fleck;
using Unity.VisualScripting;
using Fomo;
using Alpha.Core.Util;

namespace FomoWebSocket
{
    /// <summary>
    /// Manages an embedded WebSocket server for sending and receiving plain chat strings.
    /// Uses Fleck which works on Mono/Unity unlike HttpListener.AcceptWebSocketAsync.
    /// Start the server with <see cref="Start"/>, stop it with <see cref="Stop"/>.
    /// Subscribe to <see cref="OnMessageReceived"/> to handle incoming messages.
    /// Call <see cref="Broadcast"/> to push a message to all connected clients.
    /// </summary>
    public class WebSocketManager : IDisposable
    {
        private readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{FomoWebSocketPlugin.ModName}.WSM");
        private WebSocketServer? _server;
        private readonly ConcurrentDictionary<Guid, IWebSocketConnection> _clients = new ConcurrentDictionary<Guid, IWebSocketConnection>();

        public bool IsRunning => _server != null;

        public WebSocketManager() { }

        /// <summary>Starts the WebSocket server on the configured port.</summary>
        public void Start()
        {
            int port = FomoWebSocketPlugin.WebSocketChatPort?.Value ?? FomoWebSocketPlugin.DefaultWebSocketChatPort;
            if (IsRunning)
            {
                ChatUtils.AddGlobalNotification("WebSocket is already running.");
                return;
            }

            _server = new WebSocketServer($"ws://0.0.0.0:{port}");
            _server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    _log.LogInfo($"Client connected: {socket.ConnectionInfo.Id} from {socket.ConnectionInfo.ClientIpAddress}");
                    _clients[socket.ConnectionInfo.Id] = socket;

                    Broadcast($"[WSM] Client connected: {socket.ConnectionInfo.ClientIpAddress}");
                };

                socket.OnClose = () =>
                {
                    _log.LogInfo($"Client disconnected: {socket.ConnectionInfo.Id} from {socket.ConnectionInfo.ClientIpAddress}");
                    _clients.TryRemove(socket.ConnectionInfo.Id, out _);

                    Broadcast($"[WSM] Client disconnected: {socket.ConnectionInfo.ClientIpAddress}");
                };

                socket.OnMessage = text =>
                {
                    // execute commands or send chat
                    FomoPlugin.DispatchIncomingText(text);
                };

                socket.OnError = ex =>
                {
                    _log.LogWarning($"Client error {socket.ConnectionInfo.Id}: {ex.Message}");
                    _clients.TryRemove(socket.ConnectionInfo.Id, out _);

                    Broadcast($"[WSM] Client disconnected due to error: {socket.ConnectionInfo.ClientIpAddress}");
                };
            });

            _log.LogInfo($"WebSocketManager started on ws://0.0.0.0:{port}/");
            ChatUtils.AddGlobalNotification($"WebSocketManager started on ws://0.0.0.0:{port}/");
        }

        /// <summary>Stops the server and disconnects all clients.</summary>
        public void Stop()
        {
            ChatUtils.AddGlobalNotification("WebSocket stopping...");

            foreach (var pair in _clients)
            {
                try { pair.Value.Close(); } catch { }
            }
            _clients.Clear();

            try { _server?.Dispose(); } catch { }
            _server = null;
            _log.LogInfo("WebSocket server stopped.");
        }

        /// <summary>Sends a plain-text message to all currently connected clients.</summary>
        public void Broadcast(string message)
        {
            foreach (var pair in _clients)
            {
                try
                {
                    if (pair.Value.IsAvailable)
                        pair.Value.Send(message);
                }
                catch (Exception ex)
                {
                    _log.LogWarning($"Failed to send to client {pair.Key}: {ex.Message}");
                    _clients.TryRemove(pair.Key, out _);
                }
            }
        }

        public void Dispose() => Stop();
    }
}
