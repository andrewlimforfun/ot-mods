using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using UnityEngine;

namespace Alpha.Core.Commands
{
    /// <summary>
    /// /alphaunloadunusedassets - calls UnityEngine.Resources.UnloadUnusedAssets().
    /// </summary>
    public class AlphaUnloadUnusedAssetsCommand : IChatCommand
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Alpha.AUUA");
        public string Name => "alphaunloadunusedassets";
        public string ShortName => "auua";
        public string Description => "Unload unused Unity assets. Usage: /alphaunloadunusedassets";
        public string Namespace => "alpha";

        public void Execute(string[] args)
        {
            _log.LogInfo("Unloading unused assets via /alphaunloadunusedassets command.");
            ChatUtils.AddGlobalNotification("Unloading unused assets...");
            Resources.UnloadUnusedAssets();
        }
    }
}
