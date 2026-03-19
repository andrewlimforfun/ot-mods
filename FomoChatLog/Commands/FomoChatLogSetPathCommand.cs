using System;
using System.IO;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoChatLog.Commands
{
    public class FomoChatLogSetPathCommand : IChatCommand
    {
        public const string CMD = "fomochatlogsetpath";
        public string Name => CMD;
        public string ShortName => "fclsp";
        public string Description => "Set the chat log file path. Usage: /fomochatlogsetpath <path>. Example: /fomochatlogsetpath C:\\Logs\\chat.log";

        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ChatUtils.AddGlobalNotification("Usage: /fomochatlogsetpath <path>. Example: /fomochatlogsetpath C:\\Logs\\chat.log");
                return;
            }

            string newPath = args[0];
            if (string.IsNullOrWhiteSpace(newPath))
            {
                ChatUtils.AddGlobalNotification("Please enter a valid path.");
                return;
            }

            FomoChatLogPlugin.SetChatLogPath(newPath);

            try
            {
                string newLogDirPath = Path.GetDirectoryName(newPath) ?? string.Empty;
                if (!string.IsNullOrEmpty(newLogDirPath) && !Directory.Exists(newLogDirPath))
                    Directory.CreateDirectory(newLogDirPath);
            }
            catch (Exception ex)
            {
                ChatUtils.AddGlobalNotification($"Failed to create directory for the new chat log path: {ex.Message}");
                return;
            }

            ChatUtils.AddGlobalNotification($"Chat log path set to: {newPath}. Takes effect on next restart.");
        }
    }
}
