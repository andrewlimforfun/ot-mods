using System;
using System.IO;
using Fomo;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Fomo.Core.Commands
{
    // TODO deprecate this for  ChatEntryFormatter that can clean tags if desired
    public class FomoChatSinkCleanTagsCommand : IChatCommand
    {
        public const string CMD = "fomochatsinkcleantags";
        public string Name => CMD;
        public string ShortName => "fcsct";
        public string Description => "Toggle cleaning TMP tags in the chat sink e.g. <noparse><#ff0000></noparse>. Currently: " + (FomoPlugin.CleanChatSinkTags?.Value == true ? "enabled" : "disabled") + ". Usage: /fomocleanchatsinktags";

        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoPlugin.CleanChatSinkTags == null)
            {
                return;
            }

            FomoPlugin.CleanChatSinkTags.Value = !FomoPlugin.CleanChatSinkTags.Value;
            ChatUtils.AddGlobalNotification($"Cleaning TMP tags in chat sink is now {(FomoPlugin.CleanChatSinkTags.Value ? "enabled" : "disabled")}.");
    
        }
    }
}
