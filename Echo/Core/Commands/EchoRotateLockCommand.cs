using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using Echo.Core;
using UnityEngine;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echorotatelock (/erl) - Lock rotation to a fixed value every frame.
    /// Usage: /erl &lt;yaw&gt; | /erl &lt;pitch&gt; &lt;yaw&gt; &lt;roll&gt; | /erl &lt;cardinal&gt; | /erl &lt;player&gt;
    /// Unlock with /echorotateunlock (/eru).
    /// </summary>
    public class EchoRotateLockCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.EchoRotateLockCommand");

        public string Name => "echorotatelock";
        public string ShortName => "erl";
        public string Description => "Lock rotation to a fixed value. Usage: /erl <degrees|pitch yaw roll|cardinal|player>. Unlock with /eru.";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /erl <degrees|pitch yaw roll|cardinal|player>. Unlock with /eru.");
                return;
            }

            // 3-float Euler: /erl <pitch> <yaw> <roll>
            if (args.Length == 3 &&
                float.TryParse(args[0], out float pitch) &&
                float.TryParse(args[1], out float yaw) &&
                float.TryParse(args[2], out float roll))
            {
                MoveUtil.LockRotation(pitch, yaw, roll);
                ChatUtils.AddGlobalNotification($"Rotation locked to ({pitch}, {yaw}, {roll}). Use /eru to unlock.");
                Logger.LogInfo($"Rotation locked to ({pitch}, {yaw}, {roll}).");
                return;
            }

            string input = string.Join(" ", args).Trim();

            // Cardinal direction
            if (TryParseCardinal(input, out float cardinalYaw))
            {
                MoveUtil.LockRotation(0f, cardinalYaw, 0f);
                ChatUtils.AddGlobalNotification($"Rotation locked to {input} ({cardinalYaw} degrees). Use /eru to unlock.");
                Logger.LogInfo($"Rotation locked to cardinal {input} ({cardinalYaw}).");
                return;
            }

            // Single float = yaw only
            if (float.TryParse(input, out float yawOnly))
            {
                MoveUtil.LockRotation(0f, yawOnly, 0f);
                ChatUtils.AddGlobalNotification($"Rotation locked to {yawOnly} degrees. Use /eru to unlock.");
                Logger.LogInfo($"Rotation locked to yaw {yawOnly}.");
                return;
            }

            // Look at player (lock facing their current position)
            PlayerDetail? target = PlayerUtils.FindPlayerByQuery(input);
            if (target != null)
            {
                Vector3 playerPos = Echo.Patches.PlayerMovementControllerPatch.GetLocalPlayerPosition();
                Vector3 dir = target.PlayerTransform.transform.position - playerPos;
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.01f)
                {
                    ChatUtils.AddGlobalNotification("Too close to target to determine direction.");
                    return;
                }
                Quaternion rot = Quaternion.LookRotation(dir);
                Vector3 euler = rot.eulerAngles;
                MoveUtil.LockRotation(euler.x, euler.y, euler.z);
                string cleanName = ChatUtils.CleanTMPTags(target.UserName).Trim();
                ChatUtils.AddGlobalNotification($"Rotation locked toward {cleanName}. Use /eru to unlock.");
                Logger.LogInfo($"Rotation locked toward player {cleanName}.");
                return;
            }

            ChatUtils.AddGlobalNotification($"Unknown target \"{input}\". Use degrees, cardinal, or player name.");
        }

        static bool TryParseCardinal(string s, out float angle)
        {
            angle = s.ToLowerInvariant() switch
            {
                "north" => 0f,
                "n" => 0f,
                "east" => 90f,
                "e" => 90f,
                "south" => 180f,
                "s" => 180f,
                "west" => 270f,
                "w" => 270f,
                "northeast" => 45f,
                "ne" => 45f,
                "southeast" => 135f,
                "se" => 135f,
                "southwest" => 225f,
                "sw" => 225f,
                "northwest" => 315f,
                "nw" => 315f,
                _ => -1f
            };
            return angle >= 0f;
        }
    }
}
