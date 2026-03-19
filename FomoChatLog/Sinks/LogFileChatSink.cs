using System;
using System.IO;
using System.Threading.Tasks;
using BepInEx.Logging;
using Fomo.Core;

namespace FomoChatLog.Sinks
{
    /// <summary>
    /// Appends each <see cref="ChatEntry"/> to the configured chat log file.
    /// </summary>
    public class LogFileChatSink : IChatSink
    {
        private readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{FomoChatLogPlugin.ModName}.LFCS");

        private const int WarnLimit = 20;
        private int _warnCount = 0;

        public async Task SendAsync(ChatEntry entry)
        {
            if (FomoChatLogPlugin.EnableFeature?.Value != true)
                return;

            try
            {
                string logPath = FomoChatLogPlugin.GetChatLogPath();
                string? format = entry.IsNotification
                                ? FomoChatLogPlugin.NotificationFormat?.Value
                                : FomoChatLogPlugin.MessageFormat?.Value;
                string line = ChatEntryFormatter.Format(entry, format);
                await File.AppendAllTextAsync(logPath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _warnCount++;
                if (_warnCount <= WarnLimit)
                    _log.LogWarning($"Failed to write chat log: {ex.Message}");
                else if (_warnCount == WarnLimit + 1)
                    _log.LogWarning("Further warnings about chat log writing will be suppressed.");
            }
        }
    }
}
