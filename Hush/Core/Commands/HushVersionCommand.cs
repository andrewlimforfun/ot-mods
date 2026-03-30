using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Hush.Core.Commands
{
    public class HushVersionCommand : IChatCommand
    {
        public string Name => "hushversion";
        public string ShortName => "hv";
        public string Description => "Print the installed Hush version.";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            ChatUtils.AddGlobalNotification($"Hush v{HushPlugin.ModVersion}");
        }
    }
}
