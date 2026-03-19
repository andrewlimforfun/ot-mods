using System;
using BepInEx;
using BepInEx.Configuration;
using Fomo;
using Fomo.Core;
using FomoWebSocket.Commands;
using FomoWebSocket.Sinks;
using UnityEngine;

namespace FomoWebSocket
{
    [BepInPlugin(FomoWebSocketPlugin.ModGUID, FomoWebSocketPlugin.ModName, FomoWebSocketPlugin.ModVersion)]
    [BepInDependency(Fomo.FomoPlugin.ModGUID)]
    public class FomoWebSocketPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.andrewlin.ontogether.fomo.websocket";
        public const string ModName = "FomoWebSocket";
        public const string ModVersion = BuildInfo.Version;
        public const int DefaultWebSocketChatPort = 8765;
        public static ConfigEntry<bool>? EnableFeature { get; private set; }
        public static ConfigEntry<int>? WebSocketChatPort { get; private set; }
        public static ConfigEntry<string>? MessageFormat { get; private set; }
        public static ConfigEntry<string>? NotificationFormat { get; private set; }

        public static WebSocketManager? WsManager { get; private set; }

        void Awake()
        {
            Logger.LogInfo($"{ModName} v{ModVersion} loaded!");

            InitConfig();

            WsManager = new WebSocketManager();

            // Register sink and command into Fomo's shared managers
            FomoPlugin.SinkManager?.Register(new WebSocketChatSink());
            Alpha.AlphaPlugin.CommandManager?.Register(new FomoWebSocketCommand());
            Alpha.AlphaPlugin.CommandManager?.Register(new FomoWebSocketPortCommand());
            Alpha.AlphaPlugin.CommandManager?.Register(new FomoWebSocketMessageFormatCommand());
            Alpha.AlphaPlugin.CommandManager?.Register(new FomoWebSocketNotificationFormatCommand());

            // Start the WebSocket server if the feature is enabled in config
            if (EnableFeature?.Value == true)
            {
                WsManager.Start();
            }
        }

        private void InitConfig()
        {
            EnableFeature = Config.Bind(
                "General", "EnableFeature",
                true,
                "Enable or disable chat file logging.");
            WebSocketChatPort = Config.Bind(
                "WebSocket",
                "WebSocketChatPort",
                DefaultWebSocketChatPort,
                "Port for the WebSocket chat server.");
            MessageFormat = Config.Bind(
                "Formatting", "MessageFormat",
                "[{timestamp:HH:mm} {channel:short}] {username}: {message}",
                "Format string for chat messages sent to Telegram.\n" +
                "Placeholders: " + ChatEntryFormatter.PlaceholdersCsv);
            NotificationFormat = Config.Bind(
                "Formatting", "NotificationFormat",
                "[{timestamp:HH:mm}] {message}",
                "Format string for system notifications sent to Telegram.\n" +
                "Placeholders: " + ChatEntryFormatter.PlaceholdersCsv);
        }

        void OnDestroy()
        {
            WsManager?.Stop();
        }
    }
}
