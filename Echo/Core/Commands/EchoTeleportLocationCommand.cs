using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using Echo.Core;
using UnityEngine;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echoteleportlocation <name> — teleport to a saved location, or save the current spot under that name.
    /// /echoteleportlocation <x> <y> <z> — teleport directly to the given world coordinates.
    /// </summary>
    public class EchoTeleportLocationCommand : IChatCommand
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Echo.ETL");

        public string Name => "echoteleportlocation";
        public string ShortName => "etl";
        public string Description => "Teleport to a saved location or save current spot. Usage: /etl <name> | /etl <x> <y> <z>";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /etl <name>  — teleport to or save a location\n       /etl <x> <y> <z>  — teleport to coordinates");
                return;
            }

            // Vector3 case: exactly 3 float args
            if (args.Length == 3 &&
                float.TryParse(args[0], out float x) &&
                float.TryParse(args[1], out float y) &&
                float.TryParse(args[2], out float z))
            {
                var dest = new Vector3(x, y, z);
                MoveUtil.Teleport(dest);
                ChatUtils.AddGlobalNotification($"Teleported to ({x}, {y}, {z}).");
                _log.LogInfo($"Teleported to coordinates ({x}, {y}, {z}).");
                return;
            }

            // Named location case
            string name = string.Join(" ", args).Trim();
            var locations = EchoPlugin.Locations;
            if (locations == null)
            {
                ChatUtils.AddGlobalNotification("Location manager not initialized.");
                return;
            }

            if (locations.TryGet(name, out Vector3 pos))
            {
                MoveUtil.Teleport(pos);
                ChatUtils.AddGlobalNotification($"Teleported to \"{name}\".");
                _log.LogInfo($"Teleported to saved location \"{name}\" at {pos}.");
            }
            else
            {
                Vector3 current = PlayerUtils.GetPlayerDetail()?.Position ?? Vector3.zero;
                locations.Save(name, current);
                ChatUtils.AddGlobalNotification($"Saved location \"{name}\" at {current}.");
                _log.LogInfo($"Saved location \"{name}\" at {current}.");
            }
        }
    }
}
