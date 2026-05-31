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
        private static bool _followRelative;

        // --- Rotation sync state ---
        private static Transform? _rotationSyncTarget;

        // --- Look-at state ---
        private static Transform? _lookAtTarget;

        // --- Rotation warp/lock state ---
        /// <summary>Non-null means a one-shot rotation is pending. Cleared after applied.</summary>
        public static Quaternion? WarpRotation;

        /// <summary>Non-null means rotation is locked to this value every frame.</summary>
        public static Quaternion? LockedRotation;

        /// <summary>Whether we are currently following a target.</summary>
        public static bool IsFollowing => _followTarget != null;

        /// <summary>Whether we are currently syncing rotation to a target.</summary>
        public static bool IsSyncingRotation => _rotationSyncTarget != null;

        /// <summary>Whether we are continuously facing a target.</summary>
        public static bool IsLookingAt => _lookAtTarget != null;

        /// <summary>Begin following <paramref name="target"/> at <paramref name="offset"/>.</summary>
        /// <param name="relative">If true, offset is in the target's local space (rotates with them).</param>
        public static void StartFollowing(Transform target, Vector3 offset, bool relative = false)
        {
            _followTarget = target;
            _followOffset = offset;
            _followRelative = relative;
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

        /// <summary>Begin continuously facing toward <paramref name="target"/> each frame.</summary>
        public static void StartLookingAt(Transform target)
        {
            _lookAtTarget = target;
        }

        /// <summary>Stop looking at a target.</summary>
        public static void StopLookingAt()
        {
            _lookAtTarget = null;
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

            // --- One-shot rotation warp ---
            if (WarpRotation.HasValue)
            {
                __instance.transform.rotation = WarpRotation.Value;
                Logger.LogInfo($"Rotated to {WarpRotation.Value.eulerAngles}");
                WarpRotation = null;
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
            if (ReferenceEquals(_followTarget, null) && ReferenceEquals(_rotationSyncTarget, null)
                && ReferenceEquals(_lookAtTarget, null) && !LockedRotation.HasValue)
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
                    Vector3 worldOffset = _followRelative
                        ? _followTarget.rotation * _followOffset
                        : _followOffset;
                    Vector3 destination = _followTarget.position + worldOffset;
                    __instance.transform.position = destination;
                    tcm.MainPlayer.position = destination;
                }
            }

            // --- Rotation sync ---
            ApplyRotationSync(tcm.MainMovementController, __instance);

            // --- Look-at (overrides rotation sync) ---
            ApplyLookAt(tcm.MainMovementController, __instance);

            // --- Locked rotation (overrides everything) ---
            if (LockedRotation.HasValue && __instance == tcm.MainMovementController)
            {
                __instance.transform.rotation = LockedRotation.Value;
            }
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

        private static void ApplyLookAt(PlayerMovementController mainCtrl, PlayerMovementController instance)
        {
            if (ReferenceEquals(_lookAtTarget, null))
                return;

            if (_lookAtTarget == null || _lookAtTarget.gameObject == null
                || !_lookAtTarget.gameObject.activeInHierarchy)
            {
                StopLookingAt();
                ChatUtils.AddGlobalNotification("Stopped looking at target (target left).");
                return;
            }

            if (instance != mainCtrl)
                return;

            Vector3 dir = _lookAtTarget.position - instance.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f)
                return;

            instance.transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
