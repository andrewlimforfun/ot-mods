using Chalky;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Chalky.Core.Commands
{
    public class ChalkyHostRelayCommand : IChatCommand
    {
        public const string CMD = "chalkyhostrelay";
        public string Name => CMD;
        public string ShortName => "chr";
        public string Description => "Toggle Chalky host relay on/off.";
        public string Namespace => "chalky";
        public void Execute(string[] args)
        {
            if (ChalkyPlugin.EnableHostRelay == null)
            {
                return;
            }

            ChalkyPlugin.EnableHostRelay.Value = !ChalkyPlugin.EnableHostRelay.Value;
            ChatUtils.AddGlobalNotification($"Chalky host relay is now {(ChalkyPlugin.EnableHostRelay.Value ? "enabled" : "disabled")}.");
        }
    }
}
