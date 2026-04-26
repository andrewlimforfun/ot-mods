using System;
using System.IO;
using System.Linq;
using BepInEx.Logging;

namespace Alpha.Core.Util
{
    /// <summary>
    /// Replaces the default BepInEx <see cref="DiskLogListener"/> with a timestamped
    /// version that prepends [HH:mm:ss] to each log line.
    /// </summary>
    internal static class TimestampLogListener
    {
        /// <summary>
        /// When true, the disk log drops the spammy PurrNet NetworkReflection
        /// NullReferenceException that originates from a PurrNet bug (not mod code).
        /// </summary>
        internal static bool SuppressPurrNetNullRef { get; set; }

        private const string PurrNetNullRefMarker = "PurrNet.NetworkReflection.ObserversRpc_Original_1";

        private static readonly string DiagPath =
            Path.Combine(BepInEx.Paths.BepInExRootPath, "TimestampLogListener.diag.log");

        private static void Diag(string msg)
        {
            try { File.AppendAllText(DiagPath, $"[{DateTime.Now:HH:mm:ss.fff}] {msg}{Environment.NewLine}"); }
            catch { /* last resort - don't throw during diagnostics */ }
        }

        /// <summary>
        /// Removes the default <see cref="DiskLogListener"/> from
        /// <see cref="BepInEx.Logging.Logger.Listeners"/> and replaces it with a
        /// timestamped wrapper. The console listener is left untouched.
        /// </summary>
        internal static void Install()
        {
            try
            {
                Diag("Install() begin");

                DiskLogListener? diskListener = null;

                ILogListener[] snapshot = BepInEx.Logging.Logger.Listeners.ToArray();
                Diag($"Listeners snapshot count: {snapshot.Length}");

                foreach (ILogListener listener in snapshot)
                {
                    Diag($"  Found listener: {listener.GetType().FullName}");
                    if (listener is DiskLogListener dl)
                        diskListener = dl;
                }

                if (diskListener != null)
                {
                    Diag($"DiskLogListener.LogWriter is null: {diskListener.LogWriter == null}");
                    // Snapshot the writer and level BEFORE removing, in case Remove() disposes it
                    var writer = diskListener.LogWriter;
                    var level = diskListener.DisplayedLogLevel;
                    Diag($"DisplayedLogLevel: {level}");

                    BepInEx.Logging.Logger.Listeners.Remove(diskListener);
                    Diag($"DiskLogListener removed. Writer still non-null: {writer != null}");

                    BepInEx.Logging.Logger.Listeners.Add(new TimestampedDiskLogListener(writer, level));
                    Diag("TimestampedDiskLogListener added OK");
                }
                else
                {
                    Diag("No DiskLogListener found - nothing to replace");
                }

                Diag("Install() complete");
            }
            catch (Exception ex)
            {
                Diag($"Install() FAILED: {ex.GetType().Name}: {ex.Message}");
                Diag(ex.StackTrace ?? "(no stack trace)");
            }
        }

        private static string FormatWithTimestamp(LogEventArgs eventArgs)
        {
            return $"[{DateTime.Now:HH:mm:ss}] {eventArgs}";
        }

        /// <summary>Writes timestamped log lines to disk, using the writer captured at install time.</summary>
        private sealed class TimestampedDiskLogListener : ILogListener
        {
            private readonly TextWriter? _writer;
            private readonly LogLevel _displayedLogLevel;

            internal TimestampedDiskLogListener(TextWriter? writer, LogLevel displayedLogLevel)
            {
                _writer = writer;
                _displayedLogLevel = displayedLogLevel;
            }

            public void LogEvent(object sender, LogEventArgs eventArgs)
            {
                try
                {
                    if (_writer == null)
                        return;

                    if ((eventArgs.Level & _displayedLogLevel) == 0)
                        return;

                    if (SuppressPurrNetNullRef &&
                        eventArgs.Data?.ToString()?.Contains(PurrNetNullRefMarker) == true)
                        return;

                    _writer.WriteLine(FormatWithTimestamp(eventArgs));
                }
                catch (ObjectDisposedException)
                {
                    // Writer was disposed under us - nothing we can do
                }
                catch (Exception ex)
                {
                    Diag($"LogEvent threw: {ex.GetType().Name}: {ex.Message}");
                }
            }

            public void Dispose()
            {
                // Do not dispose _writer - it belongs to the original DiskLogListener
            }
        }
    }
}
