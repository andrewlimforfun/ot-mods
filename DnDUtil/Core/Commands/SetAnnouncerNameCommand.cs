using BepInEx.Logging;
using DnDUtil;

namespace DnDUtil.Core.Commands
{
    public class SetAnnouncerNameCommand : IChatCommand
    {
        public const string CMD = "setannouncername";
        public string Name => CMD;
        public string Description => "Set the name to use when sending messages to chat. Current: " + 
        (Plugin.AnnouncerChatName?.Value ?? Plugin.DefaultAnnouncerChatName);

        public void Execute(string[] args)
        {
            if (Plugin.AnnouncerChatName == null)
            {
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /setchatname [name] - Sets the name to use when sending messages to chat.");
                return;
            }

            Plugin.AnnouncerChatName.Value = args[0];
            ChatUtils.AddGlobalNotification($"Chat name is now set to {Plugin.AnnouncerChatName.Value}.");
        }
    }
}
