using System;
using System.Text.RegularExpressions;

namespace Fomo.Core
{
    /// <summary>
    /// Formats a <see cref="ChatEntry"/> using a named-placeholder format string.
    /// <para>
    /// Supported placeholders (all accept an optional <c>:specifier</c> after the name):
    /// <list type="bullet">
    ///   <item><c>{timestamp}</c> - message time. Specifier is a standard <see cref="DateTime"/> format string, default <c>HH:mm:ss</c>.</item>
    ///   <item><c>{channel}</c>  - channel label (e.g. <c>G</c>, <c>L|12</c>). Use specifier <c>short</c> for just the first character (<c>G</c> / <c>L</c>).</item>
    ///   <item><c>{username}</c> - display name of the sender.</item>
    ///   <item><c>{message}</c>  - message body.</item>
    ///   <item><c>{distance}</c> - distance in metres for local messages, empty string otherwise.</item>
    /// </list>
    /// </para>
    /// <example>
    /// <code>
    /// "[{timestamp:HH:mm}] [{channel:short}] {username}: {message}"
    /// // => "[21:30] [G] acka: hello"
    /// </code>
    /// </example>
    /// </summary>
    public static class ChatEntryFormatter
    {
        /// <summary>Default format applied when no custom format is configured.</summary>
        public const string DefaultMessageFormat = "[{timestamp:HH:mm}] [{channel:short}{distance}] {username}: {message}";
        public const string DefaultNotificationFormat = "[{timestamp:HH:mm}] {message}";

        public const string PlaceholdersCsv = "{timestamp[:fmt]}, {channel[:short]}, {username}, {message}, {distance}, {source}, {playerid}";

        // Matches {tokenName} or {tokenName:specifier}
        private static readonly Regex TokenRegex =
            new Regex(@"\{(\w+)(?::([^}]*))?\}", RegexOptions.Compiled);

        /// <summary>
        /// Formats <paramref name="entry"/> using <paramref name="format"/>.
        /// If <paramref name="entry"/> is a system notification (no Channel / UserName),
        /// a simple <c>[HH:mm:ss] message</c> string is returned regardless of the format string.
        /// </summary>
        public static string Format(ChatEntry entry, string? format = null)
        {
            string defaultFormat = entry.IsNotification ? DefaultNotificationFormat : DefaultMessageFormat;
            string template = string.IsNullOrEmpty(format) ? defaultFormat : format;

            return TokenRegex.Replace(template, m =>
            {
                string token = m.Groups[1].Value;
                string specifier = m.Groups[2].Success ? m.Groups[2].Value : string.Empty;

                switch (token)
                {
                    case "timestamp":
                        return entry.Timestamp.ToString(
                            string.IsNullOrEmpty(specifier) ? "HH:mm" : specifier);

                    case "channel":
                        return FormatChannelLabel(entry, specifier);

                    case "username":
                        return entry.UserName ?? string.Empty;

                    case "source":
                        return entry.Source ?? string.Empty;

                    case "playerid":
                        return FormatPlayerID(entry, specifier);

                    case "message":
                        return entry.Message;

                    case "distance":
                        return FormatDistanceLabel(entry, specifier);

                    default:
                        // Leave unknown tokens as-is so they surface clearly in output
                        return m.Value;
                }
            });
        }

        private static string FormatPlayerID(ChatEntry entry, string specifier)
        {
            if (entry.PlayerID != null && entry.PlayerID.Length >= 3)
            {
                if (specifier == "")
                {
                    return entry.PlayerID;
                }
                else if (specifier == "short")
                {
                    return entry.PlayerID[^3..];
                }
                else
                {
                    // get the last n characters, where n is the specifier
                    if (int.TryParse(specifier, out int n) && n > 0 && n <= entry.PlayerID.Length)
                    {
                        return entry.PlayerID.Substring(entry.PlayerID.Length - n);
                    }
                    // If specifier is invalid, fall back to full PlayerID
                    return entry.PlayerID;
                }
            }
            return string.Empty;
        }

        public static string FormatDistanceLabel(ChatEntry entry, string specifier)
        {
            if (entry.Distance.HasValue && entry.Distance.Value > 0)
            {
                return $"{entry.Distance.Value}";
            }
            return string.Empty;
        }

        /// <summary>
        /// Builds the compact channel label used by <see cref="Format"/>:
        /// <c>G</c> / <c>L</c> with an optional <c>|distance</c> suffix.
        /// </summary>
        public static string FormatChannelLabel(ChatEntry entry, string specifier = "")
        {
            string channel = entry.Channel ?? "System";

            if (specifier == "short")
                channel = channel.Length > 0 ? channel.Substring(0, 1) : "?";

            return channel;
        }
    }
}
