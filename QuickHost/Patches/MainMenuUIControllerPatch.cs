using HarmonyLib;
using BepInEx.Logging;
using QuickHost.Core;

namespace QuickHost.Patches
{
    [HarmonyPatch(typeof(MainMenuUIController))]
    public static class MainMenuUIControllerPatch
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("QuickHost.MainMenuPatch");

        /// <summary>
        /// After the main menu finishes Start(), trigger auto-host if configured.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void Start_Postfix(MainMenuUIController __instance)
        {
            Logger.LogDebug("Main menu Start() fired.");
            AutoHostService.TryAutoHost(__instance);
        }
    }
}
