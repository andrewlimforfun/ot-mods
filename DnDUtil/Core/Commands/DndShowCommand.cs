using Alpha.Core.Util;
using DnDUtil;
using Alpha.Core.Command;

namespace DnDUtil.Core.Commands
{
    public class DndShowCommand : IChatCommand
    {
        public const string CMD = "dndshowcommand";
        public string Name => CMD;
        public string ShortName => "dsc";
        public string Description => "Toggle show/hide DnD command in chat." +
            "If enabled, user command such as '/roll' will be shown in chat. Current: " + 
            (DndPlugin.ShowCommand?.Value == true ? "shown" : "hidden");

        public string Namespace => "dnd";
        public void Execute(string[] args)
        {
            if (DndPlugin.ShowCommand == null)
            {
                return;
            }

            DndPlugin.ShowCommand.Value = !DndPlugin.ShowCommand.Value;
            ChatUtils.AddGlobalNotification($"DnD user command is now {(DndPlugin.ShowCommand.Value ? "shown" : "hidden")}.");
        }
    }
}
