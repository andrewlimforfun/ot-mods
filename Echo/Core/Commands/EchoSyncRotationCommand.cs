using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using Echo.Patches;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echosyncrotation (/esr) - Mirror another player's facing direction each frame.
    /// Compatible with /echofollow: you can follow AND sync rotation simultaneously.
    /// Usage: /esr &lt;name|steamid&gt;
    /// Stop with /echounsyncrotation (/eur).
    /// </summary>
    public class EchoSyncRotationCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.EchoSyncRotationCommand");

        public string Name => "echosyncrotation";
        public string ShortName => "esr";
        public string Description => "Mirror another player's facing direction. Usage: /esr <name|steamid>. Stop with /eur.";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /esr <name|steamid>. Stop with /eur.");
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
            PlayerMovementControllerPatch.StartSyncingRotation(target.PlayerTransform.transform);

            Logger.LogInfo($"Syncing rotation to {cleanName}.");
            ChatUtils.AddGlobalNotification($"Syncing rotation to {cleanName}. Use /eur to stop.");
        }
    }
}
