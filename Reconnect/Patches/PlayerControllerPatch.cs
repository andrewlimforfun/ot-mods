using BepInEx.Logging;
using HarmonyLib;
using Reconnect.Core;
using UnityEngine;

namespace Reconnect.Patches
{
    /// <summary>
    /// Restores the player's position (and logs focus state) after a successful reconnect spawn.
    /// Also schedules a focus area repair check in case the Init RPC was missed.
    /// </summary>
    [HarmonyPatch(typeof(PlayerController))]
    public static class PlayerControllerPatch
    {
        static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Reconnect.PlayerControllerPatch");

        [HarmonyPatch("OnSpawned")]
        [HarmonyPostfix]
        static void OnSpawned_Postfix(PlayerController __instance)
        {
            if (!__instance.isOwner)
                return;

            // Schedule focus area repair check (handles missed Init RPC)
            if (ReconnectPlugin.FixFocusArea?.Value == true)
                ReconnectPlugin.StartPluginCoroutine(FocusAreaFix.TryRepairAfterSpawn());

            // Only restore position if we just reconnected and have saved state
            PlayerStateSnapshot? state = ReconnectPlugin.ReconnectManager.SavedState;
            if (state == null)
                return;

            // Consume the snapshot so it only applies once
            ReconnectPlugin.ReconnectManager.SavedState = null;

            // Restore position and rotation
            __instance.transform.position = state.Position;
            __instance.transform.rotation = state.Rotation;
            _log.LogInfo($"Restored position: {state.Position}");

            if (state.WasFocused && ReconnectPlugin.RestoreFocus?.Value == true)
            {
                _log.LogInfo($"Player was in focus mode ({state.FocusType}) before disconnect. Attempting auto-restore...");
                ReconnectPlugin.StartPluginCoroutine(FocusRestorer.TryRestoreFocus(state));
            }
            else if (state.WasFocused)
            {
                Alpha.Core.Util.ChatUtils.AddGlobalNotification("Reconnected - position restored. Press F to re-enter focus.");
            }
            else
            {
                Alpha.Core.Util.ChatUtils.AddGlobalNotification("Reconnected - position restored.");
            }
        }
    }
}
