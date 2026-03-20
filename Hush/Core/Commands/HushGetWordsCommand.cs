using System;
using Hush;
using Hush.Core;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Hush.Core.Commands
{
    public class HushGetWordsCommand : IChatCommand
    {
        public string Name => "hushgetwords";
        public string ShortName => "hgw";
        public string Description => "List all literal words currently in the Hush filter.";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null) return;

            string[] words = filter.GetWords();
            if (words.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Hush: no words in the filter.");
                return;
            }

            Array.Sort(words, StringComparer.OrdinalIgnoreCase);
            ChatUtils.AddGlobalNotification($"Hush words ({words.Length}): {string.Join(", ", words)}");
        }
    }
}
