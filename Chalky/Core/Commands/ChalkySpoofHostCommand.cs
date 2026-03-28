using Chalky;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Chalky.Core.Commands
{
    public class ChalkySpoofHostCommand : IChatCommand
    {
        public const string CMD = "chalkyspoofhost";
        public string Name => CMD;
        public string ShortName => "csh";
        public string Description => "Toggle Chalky spoof host on/off. ";
        public bool IsHidden => true; // this is a more technical setting that most users won't need to know about, so hide from help listing
        public string Namespace => "chalky";
        public void Execute(string[] args)
        {
            if (ChalkyPlugin.SpoofHost == null)
            {
                return;
            }

            ChalkyPlugin.SpoofHost.Value = !ChalkyPlugin.SpoofHost.Value;
            ChatUtils.AddGlobalNotification($"Chalky spoof host is now {(ChalkyPlugin.SpoofHost.Value ? "enabled" : "disabled")}.");
        }
    }
}
