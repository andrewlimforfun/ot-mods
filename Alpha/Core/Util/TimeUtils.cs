using System;
using System.Xml;

namespace Alpha.Core.Util
{
    public static class TimeUtils
    {
        /// <summary>
        /// Tries to parse a duration string into a <see cref="TimeSpan"/>.
        /// Accepts ISO 8601 duration format (e.g. "1h30m", "15s", "PT1H30M") and
        /// standard <see cref="TimeSpan"/> format (e.g. "1:30:00").
        /// The "PT" prefix is added automatically if missing; input is case-insensitive.
        /// Returns <c>false</c> if the string cannot be parsed.
        /// </summary>
        public static bool TryParseDuration(string input, out TimeSpan result)
        {
            // Try ISO 8601: attach PT prefix if not already present (case-insensitive)
            string upper = input.ToUpperInvariant();
            string iso = upper.StartsWith("P") ? upper : "PT" + upper;
            try
            {
                result = XmlConvert.ToTimeSpan(iso);
                return true;
            }
            catch { }

            // Fall back to TimeSpan.TryParse (hh:mm:ss, mm:ss, etc.)
            return TimeSpan.TryParse(input, out result);
        }
    }
}
