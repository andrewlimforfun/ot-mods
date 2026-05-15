using BepInEx.Logging;
using HarmonyLib;
using Reconnect.Core;
using UnityEngine;

namespace Reconnect.Patches
{
    /// <summary>
    /// Restores the player's position (and logs focus state) after a successful reconnect spawn.
    /// </summary>
    [HarmonyPatch(typeof(PlayerController))]
    public static class PlayerControllerPatch
    {
        static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Reconnect.PlayerControllerPatch");

        [HarmonyPatch("OnSpawned")]
        [HarmonyPostfix]
        static void OnSpawned_Postfix(PlayerController __instance)
        {
            // Only act if we just reconnected and have saved state
            if (!__instance.isOwner)
                return;

            PlayerStateSnapshot? state = ReconnectPlugin.ReconnectManager.SavedState;
            if (state == null)
                return;

            // Consume the snapshot so it only applies once
            ReconnectPlugin.ReconnectManager.SavedState = null;

            // Restore position and rotation
            __instance.transform.position = state.Position;
            __instance.transform.rotation = state.Rotation;
            _log.LogInfo($"Restored position: {state.Position}");

            if (state.WasFocused)
            {
                _log.LogInfo($"Player was in focus mode ({state.FocusType}) before disconnect. Position restored to focus location - player can re-enter focus manually.");
                Alpha.Core.Util.ChatUtils.AddGlobalNotification("Position restored. You were in focus mode before - press F to re-enter.");
            }
            else
            {
                Alpha.Core.Util.ChatUtils.AddGlobalNotification("Reconnected - position restored.");
            }
        }
    }
}
