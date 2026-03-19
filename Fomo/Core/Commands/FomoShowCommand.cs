using Fomo;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Fomo.Core.Commands
{
    public class ShowFomoCommand : IChatCommand
    {
        public const string CMD = "fomoshowcommand";
        public string Name => CMD;
        public string ShortName => "fsc";
        public string Description => "Toggle Fomo show/hide Fomo command in chat." +
            "If enabled, user command such as '/roll' will be shown in chat. Current: " + 
            (FomoPlugin.ShowCommand?.Value == true ? "shown" : "hidden");

        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoPlugin.ShowCommand == null)
            {
                return;
            }

            FomoPlugin.ShowCommand.Value = !FomoPlugin.ShowCommand.Value;
            ChatUtils.AddGlobalNotification($"Fomo user command is now {(FomoPlugin.ShowCommand.Value ? "shown" : "hidden")}.");
        }
    }
}
