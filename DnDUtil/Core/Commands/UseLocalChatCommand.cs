using DnDUtil;

namespace DnDUtil.Core.Commands
{
    public class UseLocalChatCommand : IChatCommand
    {
        public const string CMD = "uselocalchat";
        public string Name => CMD;
        public string Description => "Toggles DnD chat local/global. Current: " + (Plugin.LocalChat?.Value == true ? "local" : "global");

        public void Execute(string[] args)
        {
            if (Plugin.LocalChat == null)
            {
                return;
            }

            Plugin.LocalChat.Value = !Plugin.LocalChat.Value;
            ChatUtils.AddGlobalNotification($"DnD chat is now {(Plugin.LocalChat.Value ? "local" : "global")}.");
        }
    }
}
