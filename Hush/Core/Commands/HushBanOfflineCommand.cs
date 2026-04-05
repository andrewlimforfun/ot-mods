using System.Text.RegularExpressions;
using Alpha.Core.Command;
using Alpha.Core.Util;
using Hush;
using Hush.Core;

namespace Hush.Core.Commands
{
    /// <summary>
    /// Adds a player to the ban list by Steam ID and nickname without them needing to be online.
    /// Host-only; persists via the game's DataManager.SaveBanData().
    /// </summary>
    public class HushBanOfflineCommand : IChatCommand
    {
        public string Name => "hushbanoffline";
        public string ShortName => "hbo";
        public string Description => "Ban a player by Steam ID without them being online (host only). Usage: /hushbanoffline <steamid> <nickname>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (!HushMuteCommand.HostGuard()) return;

            if (args.Length < 2)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushbanoffline <steamid> <nickname>");
                return;
            }

            string steamId = args[0].Trim();
            string nickname = string.Join(" ", args, 1, args.Length - 1);

            if (!Regex.IsMatch(steamId, @"^\d{17}$"))
            {
                ChatUtils.AddGlobalNotification($"Hush: invalid Steam ID \"{steamId}\". Must be a 17-digit number.");
                return;
            }

            BanManager? bans = HushPlugin.BanManager;
            if (bans == null) return;

            if (bans.BanOffline(steamId, nickname))
                ChatUtils.AddGlobalNotification($"Hush: banned {nickname} ({steamId}).");
            else
                ChatUtils.AddGlobalNotification($"Hush: {steamId} is already banned.");
        }
    }
}
