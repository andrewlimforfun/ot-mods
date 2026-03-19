using System;
using System.IO;
using Fomo;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Fomo.Core.Commands
{
    public class FomoChatSinkGetLocalRangeCommand : IChatCommand
    {
        public const string CMD = "fomochatsinkgetlocalrange";
        public string Name => CMD;
        public string ShortName => "fcsglr";
        public string Description => "Get the local range for the chat sink.";
        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoPlugin.ChatSinkLocalRange == null)
            {
                ChatUtils.AddGlobalNotification($"Chat sink local range is currently set to {FomoPlugin.DefaultChatSinkLocalRange}.");
                return;
            }

            ChatUtils.AddGlobalNotification($"Chat sink local range is currently set to {FomoPlugin.ChatSinkLocalRange.Value}.");
        }
    }
}
