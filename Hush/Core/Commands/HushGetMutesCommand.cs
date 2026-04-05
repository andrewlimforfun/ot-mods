using System;
using System.Text;
using Alpha.Core.Command;
using Alpha.Core.Util;
using Hush;
using Hush.Core;

namespace Hush.Core.Commands
{
    /// <summary>Host-only command to list all currently muted players.</summary>
    public class HushGetMutesCommand : IChatCommand
    {
        public string Name => "hushgetmutes";
        public string ShortName => "hgm";
        public string Description => "List all currently muted players (host only). Usage: /hushgetmutes";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (!HushMuteCommand.HostGuard()) return;

            PlayerMuteManager? mutes = HushPlugin.MuteManager;
            if (mutes == null) return;

            var list = mutes.GetMutes();
            if (list.Count == 0)
            {
                ChatUtils.AddGlobalNotification("Hush: no players are currently muted.");
                return;
            }

            var sb = new StringBuilder($"Hush: {list.Count} muted player(s):");
            foreach (var (steamId, expiry) in list)
            {
                PlayerDetail? player = PlayerUtils.FindPlayerBySteamID(steamId);
                string name = player?.UserNameClean ?? steamId;
                string expiryStr = expiry == null
                    ? "permanent"
                    : $"until {expiry.Value.ToLocalTime():HH:mm:ss}";
                sb.Append($"\n  {name} - {expiryStr}");
            }
            ChatUtils.AddGlobalNotification(sb.ToString());
        }
    }
}
