using Alpha.Core.Command;
using Alpha.Core.Util;
using UnityEngine;

namespace Sweep.Core.Commands
{
    public class SweepNowCommand : IChatCommand
    {
        public string Name => "sweepnow";
        public string ShortName => "sn";
        public string Description => "Unload unused Unity assets immediately.";
        public string Namespace => "sweep";

        public void Execute(string[] args)
        {
            SweepPlugin.Log.LogInfo("Manual sweep triggered via /sweepnow.");
            ChatUtils.AddGlobalNotification("Sweeping unused assets...");
            Resources.UnloadUnusedAssets();
        }
    }
}
