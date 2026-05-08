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

        // --- Rotation sync state ---
        private static Transform? _rotationSyncTarget;

        /// <summary>Whether we are currently following a target.</summary>
        public static bool IsFollowing => _followTarget != null;

        /// <summary>Whether we are currently syncing rotation to a target.</summary>
        public static bool IsSyncingRotation => _rotationSyncTarget != null;

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

        /// <summary>Begin mirroring <paramref name="target"/>'s Y rotation each frame.</summary>
        public static void StartSyncingRotation(Transform target)
        {
            _rotationSyncTarget = target;
        }

        /// <summary>Stop syncing rotation.</summary>
        public static void StopSyncingRotation()
        {
            _rotationSyncTarget = null;
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

            // When following, suppress CharacterController.Move() so it doesn't fight
            // the position that Update_Postfix will apply this same frame.
            if (!ReferenceEquals(_followTarget, null) && _followTarget != null)
                return false;

            return true;
        }

        /// <summary>
        /// Runs after every Update() tick regardless of focus/blocking state.
        /// This is the only place that actually moves the player during a follow session,
        /// because MovePlayer() is never called when the player is focused (sitting at desk).
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void Update_Postfix(PlayerMovementController __instance)
        {
            if (ReferenceEquals(_followTarget, null) && ReferenceEquals(_rotationSyncTarget, null))
                return;

            var tcm = NetworkSingleton<TextChannelManager>.I;
            if (tcm == null || __instance != tcm.MainMovementController)
                return;

            // --- Position follow ---
            if (!ReferenceEquals(_followTarget, null))
            {
                // Target destroyed or left the session
                if (_followTarget == null || _followTarget.gameObject == null
                    || !_followTarget.gameObject.activeInHierarchy)
                {
                    StopFollowing();
                    ChatUtils.AddGlobalNotification("Stopped following (target left).");
                }
                else
                {
                    Vector3 destination = _followTarget.position + _followOffset;
                    __instance.transform.position = destination;
                    tcm.MainPlayer.position = destination;
                }
            }

            // --- Rotation sync ---
            ApplyRotationSync(tcm.MainMovementController, __instance);
        }

        private static void ApplyRotationSync(PlayerMovementController mainCtrl, PlayerMovementController instance)
        {
            if (ReferenceEquals(_rotationSyncTarget, null))
                return;

            if (_rotationSyncTarget == null || _rotationSyncTarget.gameObject == null
                || !_rotationSyncTarget.gameObject.activeInHierarchy)
            {
                StopSyncingRotation();
                ChatUtils.AddGlobalNotification("Stopped syncing rotation (target left).");
                return;
            }

            if (instance != mainCtrl)
                return;

            float targetY = _rotationSyncTarget.eulerAngles.y;
            instance.transform.rotation = Quaternion.Euler(0f, targetY, 0f);
        }
    }
}
