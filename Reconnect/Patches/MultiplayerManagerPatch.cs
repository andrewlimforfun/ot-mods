using BepInEx.Logging;
using HarmonyLib;

namespace Reconnect.Patches
{
    /// <summary>
    /// Tracks deliberate leave actions so the reconnect logic does not fire
    /// when the player intentionally leaves a session.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerManager))]
    public static class MultiplayerManagerPatch
    {
        static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Reconnect.MultiplayerManagerPatch");

        [HarmonyPatch(nameof(MultiplayerManager.QuitSession))]
        [HarmonyPrefix]
        static void QuitSession_Prefix()
        {
            Logger.LogDebug("QuitSession called - marking as intentional leave.");
            ReconnectPlugin.IsIntentionalLeave = true;
        }
    }

    /// <summary>
    /// Tracks the ButtonQuit UI action as an intentional leave.
    /// </summary>
    [HarmonyPatch(typeof(MainSceneManager))]
    public static class MainSceneManagerQuitPatch
    {
        static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Reconnect.MainSceneManagerQuitPatch");

        [HarmonyPatch(nameof(MainSceneManager.ButtonQuit))]
        [HarmonyPrefix]
        static void ButtonQuit_Prefix()
        {
            Logger.LogInfo("ButtonQuit called - marking as intentional leave.");
            ReconnectPlugin.IsIntentionalLeave = true;
        }
    }
}
