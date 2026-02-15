using DnDUtil;

namespace DnDUtil.Core.Commands
{
    public class ShowDndCommand : IChatCommand
    {
        public const string CMD = "showdndcommand";
        public string Name => CMD;
        public string ShortName => "sdc";
        public string Description => "Toggle DnDUtil show/hide DnD command in chat." +
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
