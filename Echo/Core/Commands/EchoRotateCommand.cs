using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using Echo.Core;
using UnityEngine;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echorotate (/er) - One-shot rotation change.
    /// Usage: /er &lt;yaw&gt; | /er &lt;pitch&gt; &lt;yaw&gt; &lt;roll&gt; | /er &lt;cardinal&gt; | /er &lt;player&gt;
    /// </summary>
    public class EchoRotateCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.EchoRotateCommand");

        public string Name => "echorotate";
        public string ShortName => "er";
        public string Description => "Set facing direction. Usage: /er <degrees|pitch yaw roll|cardinal|player>";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Vector3 euler = PlayerUtils.GetPlayerDetail()?.PlayerTransform.transform.eulerAngles ?? Vector3.zero;
                ChatUtils.AddGlobalNotification($"Current rotation: ({euler.x:F1}, {euler.y:F1}, {euler.z:F1})");
                return;
            }

            // 3-float Euler: /er <pitch> <yaw> <roll>
            if (args.Length == 3 &&
                float.TryParse(args[0], out float pitch) &&
                float.TryParse(args[1], out float yaw) &&
                float.TryParse(args[2], out float roll))
            {
                MoveUtil.SetRotation(pitch, yaw, roll);
                ChatUtils.AddGlobalNotification($"Rotation set to ({pitch}, {yaw}, {roll}).");
                Logger.LogInfo($"One-shot rotation to ({pitch}, {yaw}, {roll}).");
                return;
            }

            string input = string.Join(" ", args).Trim();

            // Cardinal direction
            if (TryParseCardinal(input, out float cardinalYaw))
            {
                MoveUtil.SetRotation(0f, cardinalYaw, 0f);
                ChatUtils.AddGlobalNotification($"Facing {input} ({cardinalYaw} degrees).");
                Logger.LogInfo($"One-shot rotation to cardinal {input} ({cardinalYaw}).");
                return;
            }

            // Single float = yaw only
            if (float.TryParse(input, out float yawOnly))
            {
                MoveUtil.SetRotation(0f, yawOnly, 0f);
                ChatUtils.AddGlobalNotification($"Facing {yawOnly} degrees.");
                Logger.LogInfo($"One-shot rotation to yaw {yawOnly}.");
                return;
            }

            // Look at player
            PlayerDetail? target = PlayerUtils.FindPlayerByQuery(input);
            if (target != null)
            {
                MoveUtil.LookAt(target.PlayerTransform.transform.position);
                string cleanName = ChatUtils.CleanTMPTags(target.UserName).Trim();
                ChatUtils.AddGlobalNotification($"Facing toward {cleanName}.");
                Logger.LogInfo($"One-shot rotation toward player {cleanName}.");
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
