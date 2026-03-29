using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Fomo;
using Fomo.Core;
using Alpha;
using FomoChatLog.Commands;
using FomoChatLog.Core;
using FomoChatLog.Sinks;

namespace FomoChatLog
{
    [BepInPlugin(FomoChatLogPlugin.ModGUID, FomoChatLogPlugin.ModName, FomoChatLogPlugin.ModVersion)]
    [BepInDependency(FomoPlugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class FomoChatLogPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.andrewlin.ontogether.fomochatlog";
        public const string ModName = "FomoChatLog";
        public const string ModVersion = BuildInfo.Version;

        private static readonly string DefaultChatLogPathRaw =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                         "on-together/fomo/on_together_chat_log.txt");

        // -- Config entries ----------------------------------------------------
        public static ConfigEntry<bool>? EnableFeature { get; private set; }
        private static ConfigEntry<string>? ChatLogPathRaw { get; set; }
        public static ConfigEntry<int>? MaxLogDays { get; private set; }
        public static ConfigEntry<string>? MessageFormat { get; private set; }
        public static ConfigEntry<string>? NotificationFormat { get; private set; }

        // -- Chat log manager --------------------------------------------------
        public static ChatLogManager ChatLog { get; private set; } = null!;

        internal static ManualLogSource Log = null!;

        void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{ModName} v{ModVersion} loading…");

            InitConfig();

            ChatLog = new ChatLogManager(Log, EnableFeature!, ChatLogPathRaw!, MaxLogDays!);
            ChatLog.Initialize();

            // Register the file sink with the core Fomo SinkManager
            FomoPlugin.SinkManager?.Register(new LogFileChatSink());
            Log.LogInfo("LogFileChatSink registered with Fomo SinkManager.");

            // Register commands with the core Fomo CommandManager
            AlphaPlugin.CommandManager?.Register(new FomoChatLogGetPathCommand());
            AlphaPlugin.CommandManager?.Register(new FomoChatLogSetPathCommand());
            AlphaPlugin.CommandManager?.Register(new FomoChatLogToggleCommand());
            AlphaPlugin.CommandManager?.Register(new FomoChatLogMessageFormatCommand());
            AlphaPlugin.CommandManager?.Register(new FomoChatLogNotificationFormatCommand());

            Log.LogInfo($"{ModName} loaded.");
        }

        // -- Config ------------------------------------------------------------

        private void InitConfig()
        {
            EnableFeature = Config.Bind(
                "General", "EnableFeature",
                true,
                "Enable or disable chat file logging.");
            ChatLogPathRaw = Config.Bind(
                "General", "ChatLogPath",
                DefaultChatLogPathRaw,
                "Base path for chat log files. Supports ~ and environment variables.\n" +
                "The date (yyyy-MM-dd) is appended automatically before the file extension.");
            MaxLogDays = Config.Bind(
                "General", "MaxLogDays",
                5,
                "Number of daily log files to retain. Older files are deleted on startup.");
            MessageFormat = Config.Bind(
                "Formatting", "MessageFormat",
                "[{timestamp:yyyy-MM-dd HH:mm:ss}] [{channel}] {username}: {message}",
                "Format string for chat messages sent to Telegram.\n" +
                "Placeholders: " + ChatEntryFormatter.PlaceholdersCsv);
            NotificationFormat = Config.Bind(
                "Formatting", "NotificationFormat",
                "[{timestamp:yyyy-MM-dd HH:mm:ss}] {message}",
                "Format string for system notifications sent to Telegram.\n" +
                "Placeholders: " + ChatEntryFormatter.PlaceholdersCsv);
        }
    }
}
