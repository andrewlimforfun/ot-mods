using System.Collections.Generic;
using System.IO;
using Alpha.Core.Util;
using BepInEx;
using Echo.Patches;
using PurrNet;
using UnityEngine;
namespace Echo.Core
{
    public static class MoveUtil
    {
        /// <summary>
        /// Teleports the local player to <paramref name="targetPosition"/>.
        /// Queues the warp for the next PlayerMovementController.MovePlayer tick so the
        /// CharacterController doesn't fight the position change.
        /// </summary>
        public static void Teleport(Vector3 targetPosition)
        {
            var tcm = NetworkSingleton<TextChannelManager>.I;
            if (tcm == null)
            {
                ChatUtils.AddGlobalNotification("Teleport failed: not in a session.");
                return;
            }

            // Queue the position for the Harmony patch to apply on the next MovePlayer tick
            PlayerMovementControllerPatch.WarpPosition = targetPosition;

            // Also update the network transform so other players see the move immediately
            tcm.MainPlayer.position = targetPosition;
        }

        /// <summary>
        /// Sets the local player's rotation (one-shot). Applied on the next MovePlayer tick.
        /// </summary>
        public static void SetRotation(float pitch, float yaw, float roll)
        {
            PlayerMovementControllerPatch.WarpRotation = Quaternion.Euler(pitch, yaw, roll);
        }

        /// <summary>
        /// Locks the local player's rotation to the given Euler angles every frame.
        /// </summary>
        public static void LockRotation(float pitch, float yaw, float roll)
        {
            PlayerMovementControllerPatch.LockedRotation = Quaternion.Euler(pitch, yaw, roll);
        }

        /// <summary>Unlocks rotation, allowing the game to control it again.</summary>
        public static void UnlockRotation()
        {
            PlayerMovementControllerPatch.LockedRotation = null;
        }

        /// <summary>Whether rotation is currently locked.</summary>
        public static bool IsRotationLocked => PlayerMovementControllerPatch.LockedRotation.HasValue;

        /// <summary>
        /// Rotates the local player to face toward <paramref name="targetPosition"/> (one-shot, Y-axis only).
        /// </summary>
        public static void LookAt(Vector3 targetPosition)
        {
            Vector3 playerPos = PlayerMovementControllerPatch.GetLocalPlayerPosition();
            Vector3 dir = targetPosition - playerPos;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            PlayerMovementControllerPatch.WarpRotation = Quaternion.LookRotation(dir);
        }
    }
}
