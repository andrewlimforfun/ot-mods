using System;

namespace Hush.Core
{
    public static class DurationFormatter
    {
        public static string Format(TimeSpan ts)
        {
            if (ts.TotalSeconds < 60) return $"{(int)ts.TotalSeconds}s";
            if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes}m";
            if (ts.TotalHours < 24)
            {
                string h = $"{ts.Hours}h";
                return ts.Minutes > 0 ? $"{h} {ts.Minutes}m" : h;
            }
            return $"{(int)ts.TotalDays}d";
        }
    }
}
