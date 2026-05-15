using BepInEx.Logging;
using HarmonyLib;
using PurrNet;
using PurrNet.Transports;
using Alpha.Core.Util;
using Reconnect.Core;

namespace Reconnect.Patches
{
    /// <summary>
    /// Intercepts the disconnect handler on <see cref="MainSceneManager"/> to suppress
    /// the automatic return-to-menu flow and attempt reconnection instead.
    /// </summary>
    [HarmonyPatch(typeof(MainSceneManager))]
    public static class MainSceneManagerPatch
    {
        static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Reconnect.MainSceneManagerPatch");

        /// <summary>
        /// Prefix on <c>ConnectionLost</c>: if the disconnect was unexpected
        /// and reconnect is enabled, suppress the menu return and start the reconnect sequence.
        /// </summary>
        [HarmonyPatch("ConnectionLost")]
        [HarmonyPrefix]
        static bool ConnectionLost_Prefix(NotificationStatus notificationStatus)
        {
            // Skip if mod is disabled
            if (ReconnectPlugin.Enabled?.Value != true)
                return true;

            // Allow deliberate disconnects through
            if (ReconnectPlugin.IsIntentionalLeave)
            {
                _log.LogInfo("Intentional leave - allowing normal disconnect flow.");
                ReconnectPlugin.IsIntentionalLeave = false;
                return true;
            }

            // Don't intercept if we're already trying to reconnect
            if (ReconnectPlugin.IsReconnecting)
            {
                _log.LogInfo("Already reconnecting - suppressing duplicate ConnectionLost.");
                return false;
            }

            // Hosts cannot reconnect to themselves - only clients
            if (NetworkManager.main.isHost)
            {
                _log.LogInfo("Host disconnected - cannot self-reconnect, allowing normal flow.");
                return true;
            }

            // Save lobby ID for potential full rejoin later
            try
            {
                MultiplayerManager? mm = MonoSingleton<MultiplayerManager>.I;
                if (mm != null && mm.LobbyStatus)
                    ReconnectPlugin.SavedLobbyId = mm.LobbyCode;
            }
            catch
            {
                // LobbyCode may throw if lobby is already invalid
            }

            _log.LogInfo($"Unexpected disconnect (status={notificationStatus}). Attempting reconnect...");
            ChatUtils.AddGlobalNotification("Connection lost - attempting to reconnect...");

            // Capture player state before it's lost
            ReconnectPlugin.ReconnectManager.SavedState = PlayerStateSnapshot.Capture();
            if (ReconnectPlugin.ReconnectManager.SavedState != null)
                _log.LogInfo($"Saved player state: pos={ReconnectPlugin.ReconnectManager.SavedState.Position}, focused={ReconnectPlugin.ReconnectManager.SavedState.WasFocused}");

            // Suppress ConnectionLost -> ReturnMenu flow
            ReconnectPlugin.StartReconnectCoroutine();
            return false;
        }
    }
}
