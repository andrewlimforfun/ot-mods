using Alpha.Core.Command;
using Alpha.Core.Util;
using Hush;
using Hush.Core;

namespace Hush.Core.Commands
{
    /// <summary>Host-only command to remove a player from the timed-mute delegate whitelist.</summary>
    public class HushDelegateRemoveCommand : IChatCommand
    {
        public string Name => "hushdelegateremove";
        public string ShortName => "hddr";
        public string Description => "Remove a player from the mute-delegate whitelist (host only). Usage: /hushdelegateremove <player>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (!HushMuteCommand.HostGuard()) return;
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushdelegateremove <player>");
                return;
            }

            string query = string.Join(" ", args);
            // Allow offline removal: if no current lobby player is found, treat query as raw Steam ID
            PlayerDetail? player = PlayerUtils.FindPlayerByQuery(query);
            string steamId = player?.SteamID ?? query;
            string displayName = player?.UserNameClean ?? query;

            PlayerMuteManager? mutes = HushPlugin.MuteManager;
            if (mutes == null) return;

            if (mutes.RemoveDelegate(steamId))
            {
                HushPlugin.SaveMutes();
                ChatUtils.AddGlobalNotification($"Hush: {displayName} removed from mute delegates.");
            }
            else
            {
                ChatUtils.AddGlobalNotification($"Hush: {displayName} is not a delegate.");
            }
        }
    }
}
