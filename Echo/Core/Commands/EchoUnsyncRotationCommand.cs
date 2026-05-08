using Alpha.Core.Command;
using Alpha.Core.Util;
using Echo.Patches;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echounsyncrotation (/eur) - Stop mirroring another player's facing direction.
    /// </summary>
    public class EchoUnsyncRotationCommand : IChatCommand
    {
        public string Name => "echounsyncrotation";
        public string ShortName => "eur";
        public string Description => "Stop syncing rotation to the current target.";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (!PlayerMovementControllerPatch.IsSyncingRotation)
            {
                ChatUtils.AddGlobalNotification("Not currently syncing rotation.");
                return;
            }

            PlayerMovementControllerPatch.StopSyncingRotation();
            ChatUtils.AddGlobalNotification("Stopped syncing rotation.");
        }
    }
}
