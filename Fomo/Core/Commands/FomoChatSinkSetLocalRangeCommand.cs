using System;
using System.IO;
using Fomo;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Fomo.Core.Commands
{
    public class FomoChatSinkSetLocalRangeCommand : IChatCommand
    {
        public const string CMD = "fomochatsinksetlocalrange";
        public string Name => CMD;
        public string ShortName => "fcsslr";
        public string Description => "Set local range for the chat sink. Current: " + (FomoPlugin.ChatSinkLocalRange?.Value ?? FomoPlugin.DefaultChatSinkLocalRange);
        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoPlugin.ChatSinkLocalRange == null)
            {
                ChatUtils.AddGlobalNotification("Chat sink local range config is not initialized yet.");
                return;
            }

            if (args.Length < 1)
            {
                FomoPlugin.ChatSinkLocalRange.Value = FomoPlugin.DefaultChatSinkLocalRange;
                ChatUtils.AddGlobalNotification($"Chat sink local range is now set to default: {FomoPlugin.DefaultChatSinkLocalRange}.");
                return;
            }

            string newRangeStr = args[0];
            if (!int.TryParse(newRangeStr, out int newRange) || newRange <= 0)
            {
                ChatUtils.AddGlobalNotification("Please enter a valid local range value.");
                return;
            }

            FomoPlugin.ChatSinkLocalRange.Value = newRange;
            ChatUtils.AddGlobalNotification($"Chat sink local range is now set to {newRange}.");
        }
    }
}
