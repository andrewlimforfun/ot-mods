using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Fomo;
using Fomo.Core;
using Alpha;
using FomoChatLog.Commands;
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
        public static ConfigEntry<string>? MessageFormat { get; set; }
        public static ConfigEntry<string>? NotificationFormat { get; set; }

        internal static ManualLogSource Log = null!;

        void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{ModName} v{ModVersion} loading…");

            InitConfig();
            InitializeChatLog();

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

        private void InitializeChatLog()
        {
            string chatLogPath = GetChatLogPath();
            string chatLogDirPath = Path.GetDirectoryName(chatLogPath) ?? string.Empty;
            Log.LogInfo("Creating chat log directory if it doesn't exist: " + chatLogDirPath);
            Directory.CreateDirectory(chatLogDirPath);

            // Clear the log file if it was created on a previous day
            if (EnableFeature?.Value == true && File.Exists(chatLogPath))
            {
                if (File.GetCreationTime(chatLogPath).Date < DateTime.Now.Date)
                {
                    Log.LogInfo("Clearing chat log file from previous day: " + chatLogPath);
                    File.Delete(chatLogPath);
                }
            }
        }

        // -- Public API --------------------------------------------------------

        public static string GetChatLogPath()
        {
            string raw = ChatLogPathRaw?.Value ?? DefaultChatLogPathRaw;
            raw = Environment.ExpandEnvironmentVariables(raw);
            if (raw.StartsWith("~"))
                raw = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), raw[1..]);
            return raw;
        }

        public static void SetChatLogPath(string newPath)
        {
            if (ChatLogPathRaw != null)
                ChatLogPathRaw.Value = newPath;
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
                "Path to the chat log file. Supports ~ and environment variables.");
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
