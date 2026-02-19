using DnDUtil;

namespace DnDUtil.Core.Commands
{
    public class DndShowCommand : IChatCommand
    {
        public const string CMD = "dndshowcommand";
        public string Name => CMD;
        public string ShortName => "dsc";
        public string Description => "Toggle show/hide DnD command in chat." +
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
