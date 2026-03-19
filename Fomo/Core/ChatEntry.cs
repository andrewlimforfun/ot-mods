using System;

namespace Fomo.Core
{
    /// <summary>
    /// Represents a single chat event to be dispatched to all registered <see cref="IChatSink"/>s.
    /// </summary>
    public class ChatEntry
    {
        public DateTime Timestamp { get; }
        /// <summary>Message text. Tags may or may not be cleaned depending on the patch.</summary>
        public string Message { get; }

        /// <summary>Channel name, e.g. "Global", "Local", or null for system notifications.</summary>
        public string? Channel { get; }
        /// <summary>Cleaned display name, or null for system notifications.</summary>
        public string? UserName { get; }
        /// <summary>
        /// Source of the chat entry, e.g. the patched method name. Useful for debugging and filtering, but may be null.
        /// </summary>
        public string? Source { get; }
        /// <summary>
        /// Player ID associated with the chat entry, or null for system notifications.
        /// </summary>
        public string? PlayerID { get; }
        /// <summary>
        /// Distance from the main player, or null for global messages or system notifications.
        /// </summary>
        public int? Distance { get; }
        public bool IsNotification => Channel == null || UserName == null;

        public ChatEntry(DateTime timestamp, string message, string? channel = null, string? userName = null, string? source = null, string? playerID = null, int? distance = null)
        {
            Timestamp = timestamp;
            Message = message;
            Channel = channel;
            UserName = userName;
            Source = source;
            PlayerID = playerID;
            Distance = distance;
        }
    }
}
