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

        public PlayerStateSnapshot(Vector3 position, Quaternion rotation, bool wasFocused, FocusType focusType)
        {
            Position = position;
            Rotation = rotation;
            WasFocused = wasFocused;
            FocusType = focusType;
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

            PlayerFocusController? focusController = tcm.MainFocusController;
            if (focusController != null && focusController.IsFocus)
            {
                isFocused = true;
                focusType = focusController.FocusType;
            }

            return new PlayerStateSnapshot(position, rotation, isFocused, focusType);
        }
    }
}
