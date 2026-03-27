using Alpha.Core.Util;
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
    }
}
