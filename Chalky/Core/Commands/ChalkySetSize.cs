using System;
using Chalky;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Chalky.Core.Commands
{
    public class ChalkySetSize : IChatCommand
    {
        public const string CMD = "chalkysetsize";
        public string Name => CMD;
        public string ShortName => "css";
        public string Description => "Set the size of the chalk. Not persistent, will reset to default on game restart. Game Default: 2. Usage: /chalkysetsize [size]";

        public string Namespace => "chalky";
        public void Execute(string[] args)
        {
            if (ChalkyPlugin.EnableFeature == null)
            {
                return;
            }

            if (args.Length == 0)
            {
                ChalkyPlugin.chalkySize = 2; // reset to default
                ChatUtils.AddGlobalNotification("Reset chalk size.");
                return;
            }

            if (int.TryParse(args[0], out int newSize))
            {
                if (newSize < 1)
                {
                    ChatUtils.AddGlobalNotification("Invalid size value. Please provide an integer value of 1 or greater.");
                    return;
                }
                if (newSize == 1)
                {
                    ChatUtils.AddGlobalNotification("Sorry! ): Size 1 is not possible atm. Please choose a size of 2 or greater. \nP.S. if you figure out how to do size 1 please let me know!");
                    return;
                }
                ChalkyPlugin.chalkySize = newSize;
                ChatUtils.AddGlobalNotification($"Chalk size is now set to {ChalkyPlugin.chalkySize}.");
            }
            else
            {
                ChatUtils.AddGlobalNotification("Invalid size value. Please provide a valid integer.");
            }
        }
    }
}
