using HarmonyLib;
using PurrNet;
using UnityEngine;

namespace Reconnect.Core
{
    /// <summary>
    /// Captures the local player's position and focus state at disconnect time
    /// so it can be restored after a successful reconnection.
    /// </summary>
    public class PlayerStateSnapshot
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public bool WasFocused { get; }
        public FocusType FocusType { get; }

        /// <summary>Area controller ID the player was focused at, or -1 for free focus.</summary>
        public int FocusAreaId { get; }

        /// <summary>Position before entering focus (where to teleport on unfocus).</summary>
        public Vector3 PreFocusPosition { get; }

        /// <summary>Rotation before entering focus.</summary>
        public Quaternion PreFocusRotation { get; }

        public PlayerStateSnapshot(
            Vector3 position, Quaternion rotation,
            bool wasFocused, FocusType focusType,
            int focusAreaId, Vector3 preFocusPosition, Quaternion preFocusRotation)
        {
            Position = position;
            Rotation = rotation;
            WasFocused = wasFocused;
            FocusType = focusType;
            FocusAreaId = focusAreaId;
            PreFocusPosition = preFocusPosition;
            PreFocusRotation = preFocusRotation;
        }

        /// <summary>
        /// Captures the current local player state. Returns null if the local player is unavailable.
        /// </summary>
        public static PlayerStateSnapshot? Capture()
        {
            TextChannelManager? tcm = NetworkSingleton<TextChannelManager>.I;
            if (tcm == null || tcm.MainPlayer == null)
                return null;

            Transform player = tcm.MainPlayer;
            Vector3 position = player.position;
            Quaternion rotation = player.rotation;

            bool isFocused = false;
            FocusType focusType = default;
            int focusAreaId = -1;
            Vector3 preFocusPosition = position;
            Quaternion preFocusRotation = rotation;

            PlayerFocusController? focusController = tcm.MainFocusController;
            if (focusController != null && focusController.IsFocus)
            {
                isFocused = true;
                focusType = focusController.FocusType;

                FocusAreaController? area = focusController.CurrentFocusAreaController;
                if (area != null)
                    focusAreaId = area.ID;

                // Read private _preFocusPosition/_preFocusRotation via reflection
                var posField = AccessTools.Field(typeof(PlayerFocusController), "_preFocusPosition");
                var rotField = AccessTools.Field(typeof(PlayerFocusController), "_preFocusRotation");
                if (posField != null)
                    preFocusPosition = (Vector3)posField.GetValue(focusController);
                if (rotField != null)
                    preFocusRotation = (Quaternion)rotField.GetValue(focusController);
            }

            return new PlayerStateSnapshot(position, rotation, isFocused, focusType, focusAreaId, preFocusPosition, preFocusRotation);
        }
    }
}
