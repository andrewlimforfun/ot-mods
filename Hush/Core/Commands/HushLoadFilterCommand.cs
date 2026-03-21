using Hush;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Hush.Core.Commands
{
    public class HushLoadFilterCommand : IChatCommand
    {
        public const string CMD = "hushloadfilter";
        public string Name => CMD;
        public string ShortName => "hlf";
        public string Description => "Reload the Hush filter word list from disk: /hushloadfilter.";

        public string Namespace => "hush";
        public void Execute(string[] args)
        {
            if (HushPlugin.FilterManager == null) return;

            HushPlugin.FilterManager.Load(HushPlugin.FilterConfigPath);
            ChatUtils.AddGlobalNotification($"Hush filter reloaded ({HushPlugin.FilterManager.Count} entries).");
        }
    }
}
