using Alpha.Core.Command;
using Alpha.Core.Util;

namespace FomoChatLog.Commands
{
    public class FomoChatLogMessageFormatCommand : IChatCommand
    {
        public const string CMD = "fomochatlogmessageformat";
        public string Name => CMD;
        public string ShortName => "fclmf";
        public string Description => "Set or get the chat log message format.";
        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoChatLogPlugin.EnableFeature?.Value != true )
            {
                ChatUtils.AddGlobalNotification("Chat log feature is disabled.");
                return;
            }

            if (FomoChatLogPlugin.MessageFormat == null)
            {
                ChatUtils.AddGlobalNotification("Fomo Chat Log Mod is not initialized yet.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"Chat log message format: {FomoChatLogPlugin.MessageFormat?.Value}");
                return;
            }

            if (args.Length > 0)
            {
                string newFormat = string.Join(" ", args);
                if (string.IsNullOrWhiteSpace(newFormat))
                {
                    ChatUtils.AddGlobalNotification("Please enter a valid chat log message format.");
                    return;
                }
                FomoChatLogPlugin.MessageFormat.Value = newFormat;
                ChatUtils.AddGlobalNotification($"Chat log message format is now set to: {newFormat}.");
            }
        }
    }
}
