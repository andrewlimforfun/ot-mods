using Hush;
using Hush.Core;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Hush.Core.Commands
{
    public class HushRemoveWordCommand : IChatCommand
    {
        public string Name => "hushremoveword";
        public string ShortName => "hrw";
        public string Description => "Remove a literal word from the Hush filter. Usage: /hushremoveword <word>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushremoveword <word>");
                return;
            }

            string word = string.Join(" ", args);
            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null) return;

            if (filter.Remove(word))
            {
                HushPlugin.SaveFilter();
                ChatUtils.AddGlobalNotification($"Hush: removed word \"{word}\".");
            }
            else
            {
                ChatUtils.AddGlobalNotification($"Hush: \"{word}\" was not in the list.");
            }
        }
    }
}
