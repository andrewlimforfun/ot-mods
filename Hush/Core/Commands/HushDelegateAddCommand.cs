using Alpha.Core.Command;
using Alpha.Core.Util;
using Hush;
using Hush.Core;

namespace Hush.Core.Commands
{
    /// <summary>Host-only command to add a player to the timed-mute delegate whitelist.</summary>
    public class HushDelegateAddCommand : IChatCommand
    {
        public string Name => "hushdelegateadd";
        public string ShortName => "hdda";
        public string Description => "Add a player to the mute-delegate whitelist (host only). Usage: /hushdelegateadd <player>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (!HushMuteCommand.HostGuard()) return;
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushdelegateadd <player>");
                return;
            }

            string query = string.Join(" ", args);
            PlayerDetail? player = PlayerUtils.FindPlayerByQuery(query);
            if (player == null)
            {
                ChatUtils.AddGlobalNotification($"Hush: player not found: \"{query}\"");
                return;
            }

            PlayerMuteManager? mutes = HushPlugin.MuteManager;
            if (mutes == null) return;

            if (mutes.AddDelegate(player.SteamID))
            {
                HushPlugin.SaveMutes();
                ChatUtils.AddGlobalNotification($"Hush: {player.UserNameClean} added as mute delegate.");
            }
            else
            {
                ChatUtils.AddGlobalNotification($"Hush: {player.UserNameClean} is already a delegate.");
            }
        }
    }
}
