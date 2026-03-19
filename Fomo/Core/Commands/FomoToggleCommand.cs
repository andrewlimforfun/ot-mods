using Fomo;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Fomo.Core.Commands
{
    public class FomoToggleCommand : IChatCommand
    {
        public const string CMD = "fomotoggle";
        public string Name => CMD;
        public string ShortName => "ft";
        public string Description => "Toggle Fomo feature on/off. ";

        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoPlugin.EnableFeature == null)
            {
                return;
            }

            FomoPlugin.EnableFeature.Value = !FomoPlugin.EnableFeature.Value;
            ChatUtils.AddGlobalNotification($"Fomo feature is now {(FomoPlugin.EnableFeature.Value ? "enabled" : "disabled")}.");
        }
    }
}
