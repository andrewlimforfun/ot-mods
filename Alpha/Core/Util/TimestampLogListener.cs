using System;
using System.Linq;
using BepInEx.Logging;

namespace Alpha.Core.Util
{
    /// <summary>
    /// Replaces the default BepInEx disk log listener with a timestamped version
    /// that prepends [HH:mm:ss] to each log line.
    /// </summary>
    internal static class TimestampLogListener
    {
        /// <summary>
        /// Removes the default <see cref="DiskLogListener"/> from
        /// <see cref="BepInEx.Logging.Logger.Listeners"/> and replaces it with a
        /// timestamped wrapper.
        /// </summary>
        internal static void Install()
        {
            DiskLogListener? diskListener = null;

            ILogListener[] snapshot = BepInEx.Logging.Logger.Listeners.ToArray();

            foreach (ILogListener listener in snapshot)
            {
                if (listener is DiskLogListener dl)
                {
                    diskListener = dl;
                    break;
                }
            }

            if (diskListener != null)
            {
                BepInEx.Logging.Logger.Listeners.Remove(diskListener);
                BepInEx.Logging.Logger.Listeners.Add(new TimestampedDiskLogListener(diskListener));
            }
        }

        private static string FormatWithTimestamp(LogEventArgs eventArgs)
        {
            return $"[{DateTime.Now:HH:mm:ss}] {eventArgs}";
        }

        /// <summary>Writes timestamped log lines to disk, reusing the original DiskLogListener's writer.</summary>
        private sealed class TimestampedDiskLogListener : ILogListener
        {
            private readonly DiskLogListener _inner;

            internal TimestampedDiskLogListener(DiskLogListener inner)
            {
                _inner = inner;
            }

            public void LogEvent(object sender, LogEventArgs eventArgs)
            {
                if (_inner.LogWriter == null)
                    return;

                if ((eventArgs.Level & _inner.DisplayedLogLevel) == 0)
                    return;

                _inner.LogWriter.WriteLine(FormatWithTimestamp(eventArgs));
            }

            public void Dispose()
            {
                _inner.Dispose();
            }
        }
    }
}
