using System.Threading.Tasks;
using BepInEx.Logging;
using Fomo.Core;

namespace FomoTelegram
{
    /// <summary>
    /// An <see cref="IChatSink"/> that forwards each <see cref="ChatEntry"/> to a
    /// Telegram chat via <see cref="FomoTelegramManager"/>.
    /// </summary>
    public class TelegramChatSink : IChatSink
    {
        private static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{FomoTelegramPlugin.ModName}.TCS");

        private readonly FomoTelegramManager _manager;

        public TelegramChatSink(FomoTelegramManager manager)
        {
            _manager = manager;
        }

        /// <inheritdoc />
        public Task SendAsync(ChatEntry entry)
        {
            // -- Master switch -------------------------------------------------
            if (FomoTelegramPlugin.EnableFeature?.Value != true)
                return Task.CompletedTask;

            if (!_manager.IsReady)
                return Task.CompletedTask;

            // -- Per-channel filters -------------------------------------------
            bool isNotification = entry.Channel == null || entry.UserName == null;

            if (isNotification)
            {
                if (FomoTelegramPlugin.RelayNotifications?.Value != true)
                    return Task.CompletedTask;
            }
            else if (entry.Channel != null && entry.Channel.StartsWith("Local"))
            {
                if (FomoTelegramPlugin.RelayLocalChat?.Value != true)
                    return Task.CompletedTask;
            }
            else // Global
            {
                if (FomoTelegramPlugin.RelayGlobalChat?.Value != true)
                    return Task.CompletedTask;
            }

            // -- Format & enqueue ----------------------------------------------
            if (entry.IsNotification)
            {
                string formatted = ChatEntryFormatter.Format(entry, FomoTelegramPlugin.NotificationFormat?.Value);
                _manager.Enqueue(formatted);
            }
            else
            {
                string formatted = ChatEntryFormatter.Format(entry, FomoTelegramPlugin.MessageFormat?.Value);
                _manager.Enqueue(formatted);
            }

            return Task.CompletedTask;
        }
    }
}
