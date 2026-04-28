using Alpha.Core.Command;
using Alpha.Core.Util;
using Echo.Patches;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echounfollow (/eu) - Stop following the current target.
    /// </summary>
    public class EchoUnfollowCommand : IChatCommand
    {
        public string Name => "echounfollow";
        public string ShortName => "eu";
        public string Description => "Stop following the current target.";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (!PlayerMovementControllerPatch.IsFollowing)
            {
                ChatUtils.AddGlobalNotification("Not currently following anyone.");
                return;
            }

            PlayerMovementControllerPatch.StopFollowing();
            ChatUtils.AddGlobalNotification("Stopped following.");
        }
    }
}
