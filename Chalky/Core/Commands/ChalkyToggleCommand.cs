using Chalky;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Chalky.Core.Commands
{
    public class ChalkyToggleCommand : IChatCommand
    {
        public const string CMD = "chalkytoggle";
        public string Name => CMD;
        public string ShortName => "ct";
        public string Description => "Toggle Chalky feature on/off. ";

        public string Namespace => "chalky";
        public void Execute(string[] args)
        {
            if (ChalkyPlugin.EnableFeature == null)
            {
                return;
            }

            ChalkyPlugin.EnableFeature.Value = !ChalkyPlugin.EnableFeature.Value;
            ChatUtils.AddGlobalNotification($"Chalky feature is now {(ChalkyPlugin.EnableFeature.Value ? "enabled" : "disabled")}.");
        }
    }
}
