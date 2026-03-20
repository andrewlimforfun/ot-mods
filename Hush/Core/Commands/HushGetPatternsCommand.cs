using System;
using Hush;
using Hush.Core;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Hush.Core.Commands
{
    public class HushGetPatternsCommand : IChatCommand
    {
        public string Name => "hushgetpatterns";
        public string ShortName => "hgp";
        public string Description => "List all raw regex patterns currently in the Hush filter.";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null) return;

            string[] patterns = filter.GetPatterns();
            if (patterns.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Hush: no patterns in the filter.");
                return;
            }

            Array.Sort(patterns, StringComparer.Ordinal);
            ChatUtils.AddGlobalNotification($"Hush patterns ({patterns.Length}): {string.Join(", ", patterns)}");
        }
    }
}
