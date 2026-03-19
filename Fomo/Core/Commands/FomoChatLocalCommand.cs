using Fomo;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Fomo.Core.Commands
{
    public class FomoChatLocalCommand : IChatCommand
    {
        public const string CMD = "fomochatlocal";
        public string Name => CMD;
        public string ShortName => "fcl";
        public string Description => $"Send message in local chat. ";

        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ChatUtils.AddGlobalNotification($"Usage: /{CMD} [message]");
                return;
            }

            string message = string.Join(" ", args);
            if (string.IsNullOrWhiteSpace(message))
            {
                ChatUtils.AddGlobalNotification("Please enter a valid message.");
                return;
            }

            string username = PlayerUtils.GetUserName();
            ChatUtils.SendMessageAsync(username, message, true);
        }
    }
}
