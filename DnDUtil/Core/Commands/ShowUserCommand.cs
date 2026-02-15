using DnDUtil;

namespace DnDUtil.Core.Commands
{
    public class ShowUserCommand : IChatCommand
    {
        public const string CMD = "showusercommand";
        public string Name => CMD;
        public string ShortName => "suc";
        public string Description => "Toggle DnDUtil show/hide user command in chat." +
            "If enabled, user command such as '/roll' will be shown in chat. Current: " + 
            (Plugin.ShowCommand?.Value == true ? "shown" : "hidden");

        public void Execute(string[] args)
        {
            if (Plugin.ShowCommand == null)
            {
                return;
            }

            Plugin.ShowCommand.Value = !Plugin.ShowCommand.Value;
            ChatUtils.AddGlobalNotification($"DnD user command is now {(Plugin.ShowCommand.Value ? "shown" : "hidden")}.");
        }
    }
}
