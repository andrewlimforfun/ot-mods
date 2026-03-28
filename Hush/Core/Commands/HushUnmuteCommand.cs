using Alpha.Core.Command;
using Alpha.Core.Util;
using Hush;
using Hush.Core;

namespace Hush.Core.Commands
{
    /// <summary>Host-only command to remove a mute from a player.</summary>
    public class HushUnmuteCommand : IChatCommand
    {
        public string Name => "hushunmute";
        public string ShortName => "humu";
        public string Description => "Unmute a player (host only). Usage: /hushunmute <player>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (!HushMuteCommand.HostGuard()) return;
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushunmute <player>");
                return;
            }

            string query = string.Join(" ", args);
            PlayerMuteManager? mutes = HushPlugin.MuteManager;
            if (mutes == null) return;

            // Try to resolve to a current lobby player for a friendly name;
            // fall back to the raw query as the Steam ID for offline-player unmutes
            PlayerDetail? player = PlayerUtils.FindPlayerByQuery(query);
            string steamId = player?.SteamID ?? query;
            string displayName = player?.UserNameClean ?? query;

            if (mutes.Unmute(steamId))
            {
                HushPlugin.SaveMutes();
                ChatUtils.AddGlobalNotification($"Hush: unmuted {displayName}.");
            }
            else
            {
                ChatUtils.AddGlobalNotification($"Hush: {displayName} is not muted.");
            }
        }
    }
}
