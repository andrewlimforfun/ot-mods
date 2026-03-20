using Hush;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Hush.Core.Commands
{
    public class HushToggleCommand : IChatCommand
    {
        public const string CMD = "hushtoggle";
        public string Name => CMD;
        public string ShortName => "ht";
        public string Description => "Toggle Hush feature on/off. ";

        public string Namespace => "hush";
        public void Execute(string[] args)
        {
            if (HushPlugin.EnableFeature == null)
            {
                return;
            }

            HushPlugin.EnableFeature.Value = !HushPlugin.EnableFeature.Value;
            ChatUtils.AddGlobalNotification($"Hush feature is now {(HushPlugin.EnableFeature.Value ? "enabled" : "disabled")}.");
        }
    }
}
