using Alpha.Core.Command;
using Alpha.Core.Util;
using Hush;
using Hush.Core;

namespace Hush.Core.Commands
{
    /// <summary>Host-only command to remove a player from the ban list.</summary>
    public class HushUnbanCommand : IChatCommand
    {
        public string Name => "hushunban";
        public string ShortName => "hub";
        public string Description => "Unban a player (host only). Accepts player query or raw Steam ID. Usage: /hushunban <player|steamid>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (!HushMuteCommand.HostGuard()) return;

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushunban <player|steamid>");
                return;
            }

            string query = string.Join(" ", args);
            BanManager? bans = HushPlugin.BanManager;
            if (bans == null) return;

            // Try to resolve to a current lobby player for a friendly name;
            // fall back to the raw query as the Steam ID for offline unbans.
            PlayerDetail? player = PlayerUtils.FindPlayerByQuery(query);
            string steamId = player?.SteamID ?? query;
            string displayName = player?.UserNameClean ?? query;

            if (bans.Unban(steamId))
                ChatUtils.AddGlobalNotification($"Hush: unbanned {displayName}.");
            else
                ChatUtils.AddGlobalNotification($"Hush: {displayName} is not banned.");
        }
    }
}
