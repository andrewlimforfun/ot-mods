using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Alpha.Core.Util;

namespace Remind.Core
{
    /// <summary>
    /// A handle returned when scheduling a task. Call <see cref="Cancel"/> before it fires to prevent execution.
    /// </summary>
    public class ScheduledTask
    {
        public DateTime FireAt { get; }
        public bool IsCancelled { get; private set; }
        private readonly Action _action;

        internal ScheduledTask(DateTime fireAt, Action action)
        {
            FireAt = fireAt;
            _action = action;
        }

        public void Cancel() => IsCancelled = true;

        internal void Execute() => _action();
    }

    /// <summary>
    /// Manages one-shot scheduled tasks. Call <see cref="Tick"/> every frame from <c>Plugin.Update()</c>.
    /// Uses wall-clock time (<see cref="DateTime.UtcNow"/>) so timers are accurate for long delays and survive scene reloads.
    /// </summary>
    public class ScheduledTaskManager
    {
        private readonly List<ScheduledTask> _tasks = new List<ScheduledTask>();
        private readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{RemindPlugin.ModName}.STM");

        /// <summary>Schedule <paramref name="action"/> to run after <paramref name="delay"/> from now.</summary>
        public ScheduledTask ScheduleIn(TimeSpan delay, Action action)
            => ScheduleAt(DateTime.UtcNow + delay, action);

        /// <summary>
        /// Tries to schedule <paramref name="action"/> to run after a delay parsed from <paramref name="delay"/>.
        /// Tries ISO 8601 duration first (e.g. "1h30m", "15s", "PT1H30M"), then falls back to <see cref="TimeSpan.TryParse"/> (e.g. "1:30:00").
        /// "PT" prefix is added automatically if missing; input is case-insensitive.
        /// Returns <c>false</c> and sets <paramref name="error"/> if the string cannot be parsed or is not positive.
        /// </summary>
        public bool TryScheduleIn(string delay, Action action, out ScheduledTask? task, out string error)
        {
            if (TimeUtils.TryParseDuration(delay, out TimeSpan ts) && ts > TimeSpan.Zero)
            {
                task = ScheduleIn(ts, action);
                error = "";
                return true;
            }
            task = null;
            error = $"Invalid duration: '{delay}'. Use ISO 8601 (1h30m, 15s, PT1H30M) or hh:mm:ss.";
            return false;
        }

        /// <summary>Schedule <paramref name="action"/> to run at a specific UTC instant.</summary>
        private ScheduledTask ScheduleAt(DateTime fireAt, Action action)
        {
            var task = new ScheduledTask(fireAt, action);
            _tasks.Add(task);
            return task;
        }

        /// <summary>
        /// Tries to schedule <paramref name="action"/> to run at a local time parsed from <paramref name="fireAt"/>.
        /// Accepts formats like "14:45", "14:45:00", "2026-03-12 14:45", etc.
        /// Returns <c>false</c> and sets <paramref name="error"/> if the string cannot be parsed or resolves to a past time.
        /// </summary>
        public bool TryScheduleAt(string fireAt, Action action, out ScheduledTask? task, out string error)
        {
            if (!DateTime.TryParse(fireAt, out DateTime dt))
            {
                task = null;
                error = $"Invalid time: '{fireAt}'. Use HH:mm, HH:mm:ss, or yyyy-MM-dd HH:mm.";
                return false;
            }
            DateTime utc = dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
            if (utc <= DateTime.UtcNow)
            {
                // If the time is in the past, 
                // try interpreting it as a time on the next day (e.g. "14:45" means the next 14:45, not today at 14:45)
                utc = utc.AddHours(24);

                // if still in the past thats
                if (utc <= DateTime.UtcNow)
                {
                    task = null;
                    error = $"Time '{fireAt}' is in the past.";
                    return false;
                }
            }
            task = ScheduleAt(utc, action);
            error = "";
            return true;
        }

        /// <summary>
        /// Fires all due (or cancelled) tasks. Must be called on the Unity main thread - typically from <c>Plugin.Update()</c>.
        /// </summary>
        public void Tick()
        {
            if (_tasks.Count == 0) return;

            DateTime now = DateTime.UtcNow;

            // Iterate backwards so RemoveAt doesn't shift unvisited indices
            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                ScheduledTask task = _tasks[i];
                if (task.IsCancelled || task.FireAt <= now)
                {
                    _tasks.RemoveAt(i);
                    if (!task.IsCancelled)
                    {
                        try { task.Execute(); }
                        catch (Exception ex) { _log.LogError($"[ScheduledTaskManager] Task threw: {ex}"); }
                    }
                }
            }
        }

        /// <summary>Number of pending (not yet fired, not cancelled) tasks.</summary>
        public int PendingCount => _tasks.Count;
    }
}
