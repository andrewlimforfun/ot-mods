using Chalky;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Chalky.Core.Commands
{
    public class ShowChalkyCommand : IChatCommand
    {
        public const string CMD = "chalkyshowcommand";
        public string Name => CMD;
        public string ShortName => "cshc";
        public string Description => "Toggle Chalky show/hide Chalky command in chat." +
            "If enabled, user command such as '/chalkysetcolor' will be shown in chat. Current: " + 
            (ChalkyPlugin.ShowCommand?.Value == true ? "shown" : "hidden");

        public string Namespace => "chalky";
        public void Execute(string[] args)
        {
            if (ChalkyPlugin.ShowCommand == null)
            {
                return;
            }

            ChalkyPlugin.ShowCommand.Value = !ChalkyPlugin.ShowCommand.Value;
            ChatUtils.AddGlobalNotification($"Chalky user command is now {(ChalkyPlugin.ShowCommand.Value ? "shown" : "hidden")}.");
        }
    }
}
