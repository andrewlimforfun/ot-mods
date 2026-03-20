using Hush;
using Hush.Core;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Hush.Core.Commands
{
    public class HushRemovePatternCommand : IChatCommand
    {
        public string Name => "hushremovepattern";
        public string ShortName => "hrp";
        public string Description => "Remove a raw regex pattern from the Hush filter. Usage: /hushremovepattern <pattern>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushremovepattern <pattern>");
                return;
            }

            string pattern = string.Join(" ", args);
            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null) return;

            if (filter.RemovePattern(pattern))
            {
                HushPlugin.SaveFilter();
                ChatUtils.AddGlobalNotification($"Hush: removed pattern \"{pattern}\".");
            }
            else
            {
                ChatUtils.AddGlobalNotification($"Hush: pattern \"{pattern}\" was not in the list.");
            }
        }
    }
}
