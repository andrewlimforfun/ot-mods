using Hush;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Hush.Core.Commands
{
    public class HushCensorCharCommand : IChatCommand
    {
        public const string CMD = "hushcensorchar";
        public string Name => CMD;
        public string ShortName => "hcc";
        public string Description => "Set Hush censor character: /hushcensorchar <char>.";

        public string Namespace => "hush";
        public void Execute(string[] args)
        {
            if (HushPlugin.CensorCharConfig == null)
            {
                return;
            }

            if (args.Length == 0 || args[0].Length != 1)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushcensorchar <char>. Supply exactly one character.");
                return;
            }

            HushPlugin.CensorCharConfig.Value = args[0][0].ToString();
            ChatUtils.AddGlobalNotification($"Hush censor character set to '{args[0][0]}'.");
        }
    }
}
