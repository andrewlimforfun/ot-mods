using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using Echo.Patches;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echolookat (/ela) - Continuously face toward a target player each frame.
    /// Pairs well with /echofollowrelative to stay in front while facing toward the target.
    /// Stop with /echorotateunlock (/eru).
    /// Usage: /ela &lt;name|steamid&gt;
    /// </summary>
    public class EchoLookAtCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.EchoLookAtCommand");

        public string Name => "echolookat";
        public string ShortName => "ela";
        public string Description => "Continuously face toward a player. Usage: /ela <name|steamid>. Stop with /eru.";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /ela <name|steamid>. Stop with /eru.");
                return;
            }

            string query = string.Join(" ", args).Trim();
            PlayerDetail? target = PlayerUtils.FindPlayerByQuery(query);
            if (target == null)
            {
                ChatUtils.AddGlobalNotification($"Player \"{query}\" not found.");
                return;
            }

            string cleanName = ChatUtils.CleanTMPTags(target.UserName).Trim();
            PlayerMovementControllerPatch.StartLookingAt(target.PlayerTransform.transform);

            Logger.LogInfo($"Looking at {cleanName}.");
            ChatUtils.AddGlobalNotification($"Facing toward {cleanName}. Use /eru to stop.");
        }
    }
}
