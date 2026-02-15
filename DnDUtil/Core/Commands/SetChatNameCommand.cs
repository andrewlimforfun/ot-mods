using BepInEx.Logging;
using DnDUtil;

namespace DnDUtil.Core.Commands
{
    public class SetChatNameCommand : IChatCommand
    {
        public const string CMD = "setchatname";
        public string Name => CMD;
        public string Description => "Set the name to use when sending messages to chat. Current: " + (Plugin.ChatName?.Value ?? "default player name");

        public void Execute(string[] args)
        {
            if (Plugin.ChatName == null)
            {
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /setchatname [name] - Sets the name to use when sending messages to chat.");
                return;
            }

            Plugin.ChatName.Value = args[0];
            ChatUtils.AddGlobalNotification($"Chat name is now set to {Plugin.ChatName.Value}.");
        }
    }
}
