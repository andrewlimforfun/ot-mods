using Alpha.Core.Util;
using BepInEx.Logging;
using DnDUtil;
using Alpha.Core.Command;

namespace DnDUtil.Core.Commands
{
    public class DndSetAnnouncerNameCommand : IChatCommand
    {
        public const string CMD = "dndsetannouncername";
        public string Name => CMD;
        public string ShortName => "dsan";
        public string Description => "Set the name to use when sending messages to chat. Current: " + 
        (DndPlugin.AnnouncerChatName?.Value ?? DndPlugin.DefaultAnnouncerChatName);

        public string Namespace => "dnd";
        public void Execute(string[] args)
        {
            if (DndPlugin.AnnouncerChatName == null)
            {
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /dndsetannouncername [name] - Sets the name to use when sending messages to chat.");
                return;
            }

            DndPlugin.AnnouncerChatName.Value = args[0];
            ChatUtils.AddGlobalNotification($"Chat name is now set to {DndPlugin.AnnouncerChatName.Value}.");
        }
    }
}
