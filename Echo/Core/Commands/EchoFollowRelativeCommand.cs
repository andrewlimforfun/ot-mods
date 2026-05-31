using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using Echo.Patches;
using UnityEngine;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echofollowrelative (/efr) - Follow a player with an offset relative to their facing direction.
    /// The offset rotates with the target so you stay behind, beside, or in front regardless of their heading.
    /// Usage:
    ///   /efr alice              - follow behind at default distance
    ///   /efr alice behind       - same as above
    ///   /efr alice beside       - follow to their left
    ///   /efr alice front        - follow in front
    ///   /efr alice 2 0 -1       - custom offset in target's local space (X=right, Y=up, Z=forward)
    /// Stop with /echounfollow (/eu).
    /// </summary>
    public class EchoFollowRelativeCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.EchoFollowRelativeCommand");

        private const float DefaultDistance = 1.5f;

        public string Name => "echofollowrelative";
        public string ShortName => "efr";
        public string Description => "Follow a player relative to their facing. Usage: /efr <player> [behind|beside|front|x y z]";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /efr <player> [behind|beside|front|x y z]");
                return;
            }

            // Try to detect trailing preset or offset
            Vector3 offset;
            string query;

            // Check for 3-float custom offset at the end
            if (args.Length >= 4 &&
                float.TryParse(args[args.Length - 3], out float ox) &&
                float.TryParse(args[args.Length - 2], out float oy) &&
                float.TryParse(args[args.Length - 1], out float oz))
            {
                offset = new Vector3(ox, oy, oz);
                query = string.Join(" ", args, 0, args.Length - 3).Trim();
            }
            // Check for preset keyword at the end
            else if (args.Length >= 2 && TryParsePreset(args[args.Length - 1], out offset))
            {
                query = string.Join(" ", args, 0, args.Length - 1).Trim();
            }
            else
            {
                // Default: behind
                offset = new Vector3(0f, 0f, -DefaultDistance);
                query = string.Join(" ", args).Trim();
            }

            if (string.IsNullOrEmpty(query))
            {
                ChatUtils.AddGlobalNotification("Usage: /efr <player> [behind|beside|front|x y z]");
                return;
            }

            PlayerDetail? target = PlayerUtils.FindPlayerByQuery(query);
            if (target == null)
            {
                ChatUtils.AddGlobalNotification($"Player \"{query}\" not found.");
                return;
            }

            string cleanName = ChatUtils.CleanTMPTags(target.UserName).Trim();
            PlayerMovementControllerPatch.StartFollowing(target.PlayerTransform.transform, offset, relative: true);

            Logger.LogInfo($"Following {cleanName} (relative) at offset {offset}");
            ChatUtils.AddGlobalNotification($"Following {cleanName} (relative) at ({offset.x:F1}, {offset.y:F1}, {offset.z:F1}). Use /eu to stop.");
        }

        private static bool TryParsePreset(string s, out Vector3 offset)
        {
            switch (s.ToLowerInvariant())
            {
                case "behind":
                case "back":
                    offset = new Vector3(0f, 0f, -DefaultDistance);
                    return true;
                case "beside":
                case "left":
                    offset = new Vector3(-DefaultDistance, 0f, 0f);
                    return true;
                case "right":
                    offset = new Vector3(DefaultDistance, 0f, 0f);
                    return true;
                case "front":
                case "ahead":
                    offset = new Vector3(0f, 0f, DefaultDistance);
                    return true;
                default:
                    offset = Vector3.zero;
                    return false;
            }
        }
    }
}
