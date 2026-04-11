using System;

namespace Hush.Core
{
    public enum RelayCommandType { TimedMute, Ban, Unmute, Unknown }

    public readonly struct ParsedRelayCommand
    {
        public readonly bool IsValid;
        public readonly RelayCommandType Type;
        public readonly string TargetSteamId;
        public readonly int DurationSeconds;
        public readonly string? Error;

        private ParsedRelayCommand(RelayCommandType type, string targetSteamId, int durationSeconds)
        {
            IsValid = true;
            Type = type;
            TargetSteamId = targetSteamId;
            DurationSeconds = durationSeconds;
            Error = null;
        }

        private ParsedRelayCommand(string error)
        {
            IsValid = false;
            Type = RelayCommandType.Unknown;
            TargetSteamId = string.Empty;
            DurationSeconds = 0;
            Error = error;
        }

        internal static ParsedRelayCommand Valid(RelayCommandType type, string targetId, int duration = 0)
            => new ParsedRelayCommand(type, targetId, duration);

        internal static ParsedRelayCommand Invalid(string error)
            => new ParsedRelayCommand(error);
    }

    public static class RelayParser
    {
        /// <summary>
        /// Parses a relay payload (the portion after the "hush:" prefix).
        /// Supported formats:
        /// <list type="bullet">
        /// <item><c>tmute:&lt;targetSteamId&gt;:&lt;seconds&gt;</c></item>
        /// <item><c>ban:&lt;targetSteamId&gt;</c></item>
        /// <item><c>unmute:&lt;targetSteamId&gt;</c></item>
        /// </list>
        /// </summary>
        public static ParsedRelayCommand Parse(string payload)
        {
            if (string.IsNullOrEmpty(payload))
                return ParsedRelayCommand.Invalid("Empty payload");

            int cmdEnd = payload.IndexOf(':');
            if (cmdEnd < 0)
                return ParsedRelayCommand.Invalid($"Malformed payload: {payload}");

            string cmd = payload.Substring(0, cmdEnd);
            string rest = payload.Substring(cmdEnd + 1);

            switch (cmd)
            {
                case "tmute":
                {
                    int lastColon = rest.LastIndexOf(':');
                    if (lastColon < 0)
                        return ParsedRelayCommand.Invalid($"Bad tmute args: {rest}");

                    string targetId = rest.Substring(0, lastColon);
                    if (string.IsNullOrEmpty(targetId))
                        return ParsedRelayCommand.Invalid($"Empty tmute target: {rest}");

                    if (!int.TryParse(rest.Substring(lastColon + 1), out int secs) || secs <= 0)
                        return ParsedRelayCommand.Invalid($"Bad duration: {rest}");

                    return ParsedRelayCommand.Valid(RelayCommandType.TimedMute, targetId, secs);
                }
                case "ban":
                {
                    if (string.IsNullOrEmpty(rest))
                        return ParsedRelayCommand.Invalid("Empty ban target");

                    return ParsedRelayCommand.Valid(RelayCommandType.Ban, rest);
                }
                case "unmute":
                {
                    if (string.IsNullOrEmpty(rest))
                        return ParsedRelayCommand.Invalid("Empty unmute target");

                    return ParsedRelayCommand.Valid(RelayCommandType.Unmute, rest);
                }
                default:
                    return ParsedRelayCommand.Invalid($"Unknown command: {cmd}");
            }
        }
    }
}
