using Remind;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Remind.Core.Commands
{
    public class RemindToggleCommand : IChatCommand
    {
        public const string CMD = "remindtoggle";
        public string Name => CMD;
        public string ShortName => "rt";
        public string Description => "Toggle Remind feature on/off. ";

        public string Namespace => "remind";
        public void Execute(string[] args)
        {
            if (RemindPlugin.EnableFeature == null)
            {
                return;
            }

            RemindPlugin.EnableFeature.Value = !RemindPlugin.EnableFeature.Value;
            ChatUtils.AddGlobalNotification($"Remind feature is now {(RemindPlugin.EnableFeature.Value ? "enabled" : "disabled")}.");
        }
    }
}
