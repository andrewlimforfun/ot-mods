using Alpha.Core.Util;
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
    /// Also handles persistent follow-at-offset behavior.
    /// </summary>
    [HarmonyPatch(typeof(PlayerMovementController))]
    public static class PlayerMovementControllerPatch
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.PlayerMovementControllerPatch");

        /// <summary>Non-zero means a teleport is pending. Cleared after applied.</summary>
        public static Vector3 WarpPosition = Vector3.zero;

        // --- Follow state ---
        private static Transform? _followTarget;
        private static Vector3 _followOffset;

        /// <summary>Whether we are currently following a target.</summary>
        public static bool IsFollowing => _followTarget != null;

        /// <summary>Begin following <paramref name="target"/> at <paramref name="offset"/>.</summary>
        public static void StartFollowing(Transform target, Vector3 offset)
        {
            _followTarget = target;
            _followOffset = offset;
        }

        /// <summary>Stop following.</summary>
        public static void StopFollowing()
        {
            _followTarget = null;
            _followOffset = Vector3.zero;
        }

        /// <summary>Returns the local player's current world position.</summary>
        public static Vector3 GetLocalPlayerPosition()
        {
            var tcm = NetworkSingleton<TextChannelManager>.I;
            return tcm != null ? tcm.MainPlayer.position : Vector3.zero;
        }

        [HarmonyPrefix]
        [HarmonyPatch("MovePlayer")]
        public static bool MovePlayer_Prefix(ref CharacterController ____characterController, PlayerMovementController __instance)
        {
            var tcm = NetworkSingleton<TextChannelManager>.I;
            if (tcm == null || __instance != tcm.MainMovementController)
                return true;

            // --- One-shot teleport (highest priority, also cancels follow) ---
            if (WarpPosition != Vector3.zero)
            {
                if (IsFollowing)
                {
                    StopFollowing();
                    ChatUtils.AddGlobalNotification("Stopped following (teleported).");
                }

                ____characterController.transform.position = WarpPosition;
                Logger.LogInfo($"Teleported to {WarpPosition}");
                WarpPosition = Vector3.zero;
                return false;
            }

            // --- Persistent follow ---
            // ReferenceEquals checks the C# ref; Unity's == catches destroyed objects.
            if (!ReferenceEquals(_followTarget, null))
            {
                if (_followTarget == null || _followTarget.gameObject == null
                    || !_followTarget.gameObject.activeInHierarchy)
                {
                    StopFollowing();
                    ChatUtils.AddGlobalNotification("Stopped following (target left).");
                    return true;
                }

                Vector3 destination = _followTarget.position + _followOffset;
                ____characterController.transform.position = destination;
                tcm.MainPlayer.position = destination;
                return false; // skip normal MovePlayer every tick
            }

            return true;
        }
    }
}
