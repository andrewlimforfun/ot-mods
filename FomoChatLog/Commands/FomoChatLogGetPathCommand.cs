using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoChatLog.Commands
{
    public class FomoChatLogGetPathCommand : IChatCommand
    {
        public const string CMD = "fomochatloggetpath";
        public string Name => CMD;
        public string ShortName => "fclgp";
        public string Description => "Get the current chat log file path.";

        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            ChatUtils.AddGlobalNotification($"Chat log path: {FomoChatLogPlugin.GetChatLogPath()}");
        }
    }
}
