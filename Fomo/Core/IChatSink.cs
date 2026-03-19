using System.Threading.Tasks;

namespace Fomo.Core
{
    /// <summary>
    /// Receives processed chat entries and forwards them to an output destination
    /// (e.g. a log file, WebSocket server, Telegram bot, etc.).
    /// </summary>
    public interface IChatSink
    {
        Task SendAsync(ChatEntry entry);
    }
}
