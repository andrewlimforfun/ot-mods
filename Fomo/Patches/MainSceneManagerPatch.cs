using BepInEx.Logging;
using Fomo.Core;
using HarmonyLib;
using PurrNet;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fomo.Patches
{
    [HarmonyPatch(typeof(MainSceneManager))]
    internal class MainSceneManagerPatch
    {
        private static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{FomoPlugin.ModName}.MSMP");

        [HarmonyPatch("ConnectionLost")]
        [HarmonyPostfix]
        public static void ConnectionLostPostfix(NotificationStatus notificationStatus)
        {
            _log.LogWarning($"Disconnected: {notificationStatus} at {System.DateTime.Now}");

            ChatEntry entry = new ChatEntry(timestamp: System.DateTime.Now,
                                            message: $"Disconnected: {notificationStatus}",
                                            channel: null,
                                            userName: null);

            if (FomoPlugin.SinkManager == null) return;
            _ = FomoPlugin.SinkManager.BroadcastAsync(entry);
        }

        [HarmonyPatch(nameof(MainSceneManager.ReturnMenu), typeof(bool))]
        [HarmonyPostfix]
        public static void ReturnMenuPostfix(bool wishListCheck)
        {
            _log.LogInfo($"Returned to menu at {System.DateTime.Now}");

            ChatEntry entry = new ChatEntry(timestamp: System.DateTime.Now,
                                message: $"Returned to menu",
                                channel: null,
                                userName: null);

            if (FomoPlugin.SinkManager == null) return;
            _ = FomoPlugin.SinkManager.BroadcastAsync(entry);
        }
    }
}
