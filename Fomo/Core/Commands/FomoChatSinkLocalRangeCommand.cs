using Fomo;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Fomo.Core.Commands
{
    public class FomoChatSinkLocalRangeCommand : IChatCommand
    {
        public const string CMD = "fomochatsinklocalrange";
        public string Name => CMD;
        public string ShortName => "fcslr";
        public string Description =>
            $"Get or set the local range for the chat sink. " +
            $"No args = get current value. Pass a number to set it. " +
            $"Current: {(FomoPlugin.ChatSinkLocalRange?.Value ?? FomoPlugin.DefaultChatSinkLocalRange)}";
        public string Namespace => "fomo";

        public void Execute(string[] args)
        {
            if (FomoPlugin.ChatSinkLocalRange == null)
            {
                ChatUtils.AddGlobalNotification("Chat sink local range config is not initialized yet.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"Chat sink local range is currently set to {FomoPlugin.ChatSinkLocalRange.Value}.");
                return;
            }

            if (!int.TryParse(args[0], out int newRange) || newRange <= 0)
            {
                ChatUtils.AddGlobalNotification("Please enter a valid positive integer.");
                return;
            }

            FomoPlugin.ChatSinkLocalRange.Value = newRange;
            ChatUtils.AddGlobalNotification($"Chat sink local range is now set to {newRange}.");
        }
    }
}
