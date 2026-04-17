using System;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Sweep.Core.Commands
{
    public class SweepIntervalCommand : IChatCommand
    {
        public string Name => "sweepinterval";
        public string ShortName => "si";
        public string Description => "Get or set the sweep interval. Usage: /si [duration] (e.g. 30M, 1H, PT1H30M)";
        public string Namespace => "sweep";

        public void Execute(string[] args)
        {
            if (SweepPlugin.Interval == null) return;

            if (args.Length >= 1)
            {
                string input = args[0];
                if (!TimeUtils.TryParseDuration(input, out TimeSpan ts) || ts.TotalSeconds <= 0)
                {
                    ChatUtils.AddGlobalNotification("Invalid duration. Examples: 30M, 1H, PT1H30M");
                    return;
                }

                SweepPlugin.Interval.Value = input.ToUpperInvariant();
                SweepPlugin.Instance?.RestartCoroutine();
            }

            ChatUtils.AddGlobalNotification($"Sweep interval = {SweepPlugin.FormatInterval()}");
        }
    }
}
