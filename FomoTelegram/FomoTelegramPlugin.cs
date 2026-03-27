using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Fomo;
using Fomo.Core;
using Alpha;
using FomoTelegram.Commands;

namespace FomoTelegram
{
    [BepInPlugin(FomoTelegramPlugin.ModGUID, FomoTelegramPlugin.ModName, FomoTelegramPlugin.ModVersion)]
    [BepInDependency(FomoPlugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class FomoTelegramPlugin : BaseUnityPlugin
    {
        public const string ModGUID    = "com.andrewlin.ontogether.fomotelegram";
        public const string ModName    = "FomoTelegram";
        public const string ModVersion = BuildInfo.Version;

        // -- Config entries ----------------------------------------------------
        public static ConfigEntry<bool>?    EnableFeature     { get; private set; }
        public static ConfigEntry<string>?  TelegramBotApiKey  { get; private set; }
        public static ConfigEntry<string>?  TelegramChatId     { get; private set; }
        public static ConfigEntry<bool>?    RelayGlobalChat    { get; private set; }
        public static ConfigEntry<bool>?    RelayLocalChat     { get; private set; }
        public static ConfigEntry<bool>?    RelayNotifications { get; private set; }
        public static ConfigEntry<string>?  MessageFormat      { get; private set; }
        public static ConfigEntry<string>?  NotificationFormat { get; private set; }
        // -- Singleton manager -------------------------------------------------
        public static FomoTelegramManager? TelegramManager { get; private set; }

        public static string ConfigPath { get; private set; } = $"BepInEx/config/{ModGUID}.cfg";

        internal static ManualLogSource Log = null!;

        void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{ModName} v{ModVersion} loading…");

            InitConfig();
            
            ConfigPath = Config.ConfigFilePath;
            Log.LogInfo($"Config file path: {ConfigPath}");
            
            AlphaPlugin.CommandManager?.Register(new FomoTelegramToggleCommand());
            AlphaPlugin.CommandManager?.Register(new FomoTelegramMessageFormatCommand());
            AlphaPlugin.CommandManager?.Register(new FomoTelegramNotificationFormatCommand());
            AlphaPlugin.CommandManager?.Register(new FomoTelegramSetupInfoCommand());

            string apiKey = TelegramBotApiKey?.Value ?? string.Empty;
            string chatId = TelegramChatId?.Value  ?? string.Empty;

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == FomoTelegramManager.PlaceholderApiKey)
            {
                Log.LogWarning($"TelegramBotApiKey is not set. Edit config and restart: {ConfigPath}");
            }
            else if (string.IsNullOrWhiteSpace(chatId) || chatId == FomoTelegramManager.PlaceholderChatId)
            {
                Log.LogWarning($"TelegramChatId is not set. Edit config and restart: {ConfigPath}");
            }
            else
            {
                TelegramManager = new FomoTelegramManager(apiKey, chatId);
                var sink = new TelegramChatSink(TelegramManager);
                FomoPlugin.SinkManager?.Register(sink);
                Log.LogInfo("TelegramChatSink registered with Fomo SinkManager.");
            }

            Log.LogInfo($"{ModName} loaded.");
        }

        void OnDestroy()
        {
            TelegramManager?.Dispose();
        }

        private void InitConfig()
        {
            EnableFeature = Config.Bind(
                "General", "EnableFeature",
                true,
                "Master switch - disable to stop all Telegram forwarding without removing the plugin.");

            TelegramBotApiKey = Config.Bind(
                "Telegram", "TelegramBotApiKey",
                FomoTelegramManager.PlaceholderApiKey,
                "Your Telegram Bot API token from @BotFather (e.g. 123456:ABC-DEF…).");

            TelegramChatId = Config.Bind(
                "Telegram", "TelegramChatId",
                FomoTelegramManager.PlaceholderChatId,
                "Telegram chat / group / channel ID to forward messages to. " +
                "Use @userinfobot or the Telegram API to find your chat ID. e.g. 1234567890");

            RelayGlobalChat = Config.Bind(
                "Filters", "RelayGlobalChat",
                true,
                "Forward global chat messages to Telegram.");

            RelayLocalChat = Config.Bind(
                "Filters", "RelayLocalChat",
                true,
                "Forward local chat messages to Telegram.");

            RelayNotifications = Config.Bind(
                "Filters", "RelayNotifications",
                true,
                "Forward system notifications (joins, leaves, etc.) to Telegram.");

            MessageFormat = Config.Bind(
                "Formatting", "MessageFormat",
                "[{channel:short}{distance}] {username}: {message}",
                "Format string for chat messages sent to Telegram.\n" +
                "Placeholders: " + ChatEntryFormatter.PlaceholdersCsv);

            NotificationFormat = Config.Bind(
                "Formatting", "NotificationFormat",
                "{message}",
                "Format string for system notifications sent to Telegram.\n" +
                "Placeholders: " + ChatEntryFormatter.PlaceholdersCsv);
        }
    }
}
