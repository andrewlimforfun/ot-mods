using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Hush.Core.Commands
{
    public class HushLogVerboseCommand : IChatCommand
    {
        public const string CMD = "hushlogverbose";
        public string Name => CMD;
        public string ShortName => "hlv";
        public string Description => "Toggle verbose BepInEx logging for Hush.";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            HushPlugin.VerboseLogging = !HushPlugin.VerboseLogging;
            ChatUtils.AddGlobalNotification($"Hush verbose logging is now {(HushPlugin.VerboseLogging ? "enabled" : "disabled")}.");
        }
    }
}
