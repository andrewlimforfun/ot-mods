using Alpha.Core.Util;
using DnDUtil;
using Alpha.Core.Command;

namespace DnDUtil.Core.Commands
{
    public class DndToggleCommand : IChatCommand
    {
        public const string CMD = "dndtoggle";
        public string Name => CMD;
        public string ShortName => "dt";
        public string Description => "Toggle DnDUtil feature on/off. ";

        public string Namespace => "dnd";
        public void Execute(string[] args)
        {
            if (DndPlugin.EnableFeature == null)
            {
                return;
            }

            DndPlugin.EnableFeature.Value = !DndPlugin.EnableFeature.Value;
            ChatUtils.AddGlobalNotification($"DnD feature is now {(DndPlugin.EnableFeature.Value ? "enabled" : "disabled")}.");
        }
    }
}
