using Fomo;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Fomo.Core.Commands
{
    public class FomoMessageLimitCommand : IChatCommand
    {
        public const string CMD = "fomomessagelimit";
        public string Name => CMD;
        public string ShortName => "fml";
        public string Description => $"Set the global or local chat message window size. " +
            $"Usage: /{CMD} [global|local] [number]. " +
            $"Current: global={FomoPlugin.GlobalMessageLimitCount?.Value}, local={FomoPlugin.LocalMessageLimitCount?.Value}";

        public string Namespace => "fomo";
        public void Execute(string[] args)
        {
            if (FomoPlugin.GlobalMessageLimitCount == null || FomoPlugin.LocalMessageLimitCount == null)
            {
                return;
            }

            if (args.Length != 2)
            {
                ChatUtils.AddGlobalNotification($"Usage: /{CMD} [global|local] [number]");
                return;
            }

            string type = args[0].ToLower();
            if (!int.TryParse(args[1], out int value) || value <= 0)
            {
                ChatUtils.AddGlobalNotification("Invalid number. Please enter a positive integer.");
                return;
            }

            if (type == "global")
            {
                FomoPlugin.GlobalMessageLimitCount.Value = value;
                ChatUtils.AddGlobalNotification($"Global message limit set to {value}.");
            }
            else if (type == "local")
            {
                FomoPlugin.LocalMessageLimitCount.Value = value;
                ChatUtils.AddGlobalNotification($"Local message limit set to {value}.");
            }
            else
            {
                ChatUtils.AddGlobalNotification($"Unknown type '{type}'. Use 'global' or 'local'.");
            }
        }
    }
}
