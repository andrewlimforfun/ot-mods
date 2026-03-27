using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using UnityEngine;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echotpto [+x +y +z] -- teleport to a player by approximate name or Steam ID suffix, with optional offset.
    /// Usage:
    ///   /echotpto alice
    ///   /echotpto 12345
    ///   /echotpto alice 0 1 0
    ///   /echotpto 12345 5 0 -3
    /// </summary>
    public class EchoTpToCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.EchoTpToCommand");

        public string Name => "echotpto";
        public string ShortName => "etp";
        public string Description => "Teleport to a player by name or Steam ID suffix. Usage: /echotpto <name|steamid_suffix|persona> [x y z]";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /echotpto <name|steamid_suffix|persona> [x y z]");
                return;
            }

            // Parse optional offset — last 3 args are floats if we have 4 args total
            Vector3 offset = Vector3.zero;
            string query;

            if (args.Length >= 4 &&
                float.TryParse(args[args.Length - 3], out float ox) &&
                float.TryParse(args[args.Length - 2], out float oy) &&
                float.TryParse(args[args.Length - 1], out float oz))
            {
                offset = new Vector3(ox, oy, oz);
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

            Vector3 destination = target.Position + offset;
            string cleanName = ChatUtils.CleanTMPTags(target.UserName).Trim();
            Logger.LogInfo($"Teleporting to {cleanName} at {target.Position} + offset {offset} = {destination}");
            MoveUtil.Teleport(destination);

            string offsetStr = offset == Vector3.zero ? "" : $" (+{offset.x}, +{offset.y}, +{offset.z})";
            ChatUtils.AddGlobalNotification($"Teleporting to {cleanName}{offsetStr}.");
        }
    }
}
