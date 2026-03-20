using System;
using Hush;
using Hush.Core;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Hush.Core.Commands
{
    public class HushAddWordCommand : IChatCommand
    {
        public string Name => "hushaddword";
        public string ShortName => "haw";
        public string Description => "Add a literal word to the Hush filter. Usage: /hushaddword <word>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushaddword <word>");
                return;
            }

            string word = string.Join(" ", args);
            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null) return;

            if (filter.Add(word))
            {
                HushPlugin.SaveFilter();
                ChatUtils.AddGlobalNotification($"Hush: added word \"{word}\".");
            }
            else
            {
                ChatUtils.AddGlobalNotification($"Hush: \"{word}\" is already in the list.");
            }
        }
    }
}
