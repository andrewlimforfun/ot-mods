using Hush;
using Hush.Core;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Hush.Core.Commands
{
    public class HushAddPatternCommand : IChatCommand
    {
        public string Name => "hushaddpattern";
        public string ShortName => "hap";
        public string Description => "Add a raw regex pattern to the Hush filter. Usage: /hushaddpattern <pattern>  (e.g. (?i)f+u+c+k)";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushaddpattern <pattern>  Example: /hushaddpattern (?i)f+u+c+k");
                return;
            }

            string pattern = string.Join(" ", args);
            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null) return;

            if (filter.AddPattern(pattern))
            {
                HushPlugin.SaveFilter();
                ChatUtils.AddGlobalNotification($"Hush: added pattern \"{pattern}\".");
            }
            else
            {
                ChatUtils.AddGlobalNotification($"Hush: pattern \"{pattern}\" is invalid or already in the list.");
            }
        }
    }
}
