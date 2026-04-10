using HarmonyLib;
using BepInEx.Logging;
using PurrNet;
using UnityEngine;

namespace Echo.Patches
{
    /// <summary>
    /// Teleports the local player by intercepting the next MovePlayer tick.
    /// Direct CharacterController.transform.position assignment is overwritten every frame,
    /// so we queue the warp here and apply it from inside MovePlayer where it sticks.
    /// </summary>
    [HarmonyPatch(typeof(PlayerMovementController))]
    public static class PlayerMovementControllerPatch
    {
        static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.PlayerMovementControllerPatch");

        /// <summary>Non-zero means a teleport is pending. Cleared after applied.</summary>
        public static Vector3 WarpPosition = Vector3.zero;

        [HarmonyPrefix]
        [HarmonyPatch("MovePlayer")]
        public static bool MovePlayer_Prefix(ref CharacterController ____characterController, PlayerMovementController __instance)
        {
            if (WarpPosition == Vector3.zero)
                return true;

            // Only apply to the local player's movement controller
            var tcm = NetworkSingleton<TextChannelManager>.I;
            if (tcm == null || __instance != tcm.MainMovementController)
                return true;

            ____characterController.transform.position = WarpPosition;
            Logger.LogInfo($"Teleported to {WarpPosition}");
            WarpPosition = Vector3.zero;
            return false; // skip original MovePlayer this tick
        }
    }
}
