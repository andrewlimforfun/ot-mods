using Alpha.Core.Command;
using Alpha.Core.Util;
using Hush;
using Hush.Core;

namespace Hush.Core.Commands
{
    /// <summary>Host-only command to permanently mute a player.</summary>
    public class HushMuteCommand : IChatCommand
    {
        public string Name => "hushmute";
        public string ShortName => "hmu";
        public string Description => "Permanently mute a player (host only). Usage: /hushmute <player>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (!HostGuard()) return;
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushmute <player>");
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

            if (mutes.Mute(player.SteamID))
            {
                HushPlugin.SaveMutes();
                ChatUtils.AddGlobalNotification($"Hush: permanently muted {player.UserNameClean}.");
            }
            else
            {
                ChatUtils.AddGlobalNotification($"Hush: {player.UserNameClean} is already permanently muted.");
            }
        }

        internal static bool HostGuard()
        {
            if (PlayerUtils.GetHost()?.SteamID == SteamUtils.GetPlayerSteamID()) return true;
            ChatUtils.AddGlobalNotification("Hush: only the host can mute players.");
            return false;
        }
    }
}
