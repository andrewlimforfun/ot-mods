using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using Echo.Patches;
using UnityEngine;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echofollow (/ef) - Follow a player at a fixed offset.
    /// Usage:
    ///   /ef alice           - follow alice at her current offset from you
    ///   /ef alice 2 0 -1    - follow alice at the specified offset
    /// Unfollows on /echounfollow, or on any teleport.
    /// The local player should be focused (sitting at desk) to prevent falling.
    /// </summary>
    public class EchoFollowCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.EchoFollowCommand");

        public string Name => "echofollow";
        public string ShortName => "ef";
        public string Description => "Follow a player at a fixed offset. Usage: /ef <name|steamid> [x y z]";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /ef <name|steamid> [x y z]");
                return;
            }

            // Parse optional offset - last 3 args are floats
            Vector3? explicitOffset = null;
            string query;

            if (args.Length >= 4 &&
                float.TryParse(args[args.Length - 3], out float ox) &&
                float.TryParse(args[args.Length - 2], out float oy) &&
                float.TryParse(args[args.Length - 1], out float oz))
            {
                explicitOffset = new Vector3(ox, oy, oz);
                query = string.Join(" ", args, 0, args.Length - 3);
            }
            else
            {
                query = string.Join(" ", args);
            }

            query = query.Trim();

            PlayerDetail? target = PlayerUtils.FindPlayerByQuery(query);
            if (target == null)
            {
                ChatUtils.AddGlobalNotification($"Player \"{query}\" not found.");
                return;
            }

            // Calculate offset: explicit or current distance from target
            Vector3 localPos = PlayerMovementControllerPatch.GetLocalPlayerPosition();
            Vector3 offset = explicitOffset ?? (localPos - target.Position);

            string cleanName = ChatUtils.CleanTMPTags(target.UserName).Trim();
            PlayerMovementControllerPatch.StartFollowing(target.PlayerTransform.transform, offset);

            Logger.LogInfo($"Following {cleanName} at offset {offset}");
            ChatUtils.AddGlobalNotification($"Following {cleanName} at offset ({offset.x:F1}, {offset.y:F1}, {offset.z:F1}). Use /eu to stop.");
        }
    }
}
