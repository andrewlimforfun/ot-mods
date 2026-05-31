using Alpha.Core.Command;
using Alpha.Core.Util;
using Echo.Core;
using Echo.Patches;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echorotateunlock (/eru) - Stop all rotation overrides (lock, look-at).
    /// </summary>
    public class EchoRotateUnlockCommand : IChatCommand
    {
        public string Name => "echorotateunlock";
        public string ShortName => "eru";
        public string Description => "Stop all rotation overrides (lock, look-at).";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            bool hadLock = MoveUtil.IsRotationLocked;
            bool hadLookAt = PlayerMovementControllerPatch.IsLookingAt;

            if (!hadLock && !hadLookAt)
            {
                ChatUtils.AddGlobalNotification("No rotation override active.");
                return;
            }

            MoveUtil.UnlockRotation();
            PlayerMovementControllerPatch.StopLookingAt();
            ChatUtils.AddGlobalNotification("Rotation unlocked.");
        }
    }
}
