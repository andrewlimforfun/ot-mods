using Hush;
using Alpha.Core.Util;
using Alpha.Core.Command;
using Hush.Core;
using System;

namespace Hush.Core.Commands
{
    public class HushFilterActionCommand : IChatCommand
    {
        public const string CMD = "hushfilteraction";
        public string Name => CMD;
        public string ShortName => "hfa";
        public string Description => "Set Hush filter action command: /hushfilteraction <action>. Valid actions: block, censor.";

        public string Namespace => "hush";
        public void Execute(string[] args)
        {
            if (HushPlugin.FilterActionConfig == null)
            {
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushfilteraction <action>. Valid actions: block, censor.");
                return;
            }

            if (Enum.TryParse(args[0], ignoreCase: true, out FilterAction action))
            {
                HushPlugin.FilterActionConfig.Value = action;
                ChatUtils.AddGlobalNotification($"Hush filter action set to {action}.");
            }
            else
            {
                ChatUtils.AddGlobalNotification("Invalid action. Valid actions: block, censor.");
            }

            // HushPlugin.FilterAction.Value = ;
            // ChatUtils.AddGlobalNotification($"Hush feature is now {(HushPlugin.EnableFeature.Value ? "enabled" : "disabled")}.");
        }
    }
}
