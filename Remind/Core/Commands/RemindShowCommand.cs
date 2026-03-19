using Remind;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Remind.Core.Commands
{
    public class ShowRemindCommand : IChatCommand
    {
        public const string CMD = "remindshowcommand";
        public string Name => CMD;
        public string ShortName => "rsc";
        public string Description => "Toggle Remind show/hide Remind command in chat." +
            "If enabled, user command such as '/roll' will be shown in chat. Current: " + 
            (RemindPlugin.ShowCommand?.Value == true ? "shown" : "hidden");

        public string Namespace => "remind";
        public void Execute(string[] args)
        {
            if (RemindPlugin.ShowCommand == null)
            {
                return;
            }

            RemindPlugin.ShowCommand.Value = !RemindPlugin.ShowCommand.Value;
            ChatUtils.AddGlobalNotification($"Remind user command is now {(RemindPlugin.ShowCommand.Value ? "shown" : "hidden")}.");
        }
    }
}
