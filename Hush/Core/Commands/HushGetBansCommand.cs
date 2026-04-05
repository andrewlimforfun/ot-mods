using System.Text;
using Alpha.Core.Command;
using Alpha.Core.Util;
using Hush;
using Hush.Core;

namespace Hush.Core.Commands
{
    /// <summary>Host-only command to list all currently banned players.</summary>
    public class HushGetBansCommand : IChatCommand
    {
        public string Name => "hushgetbans";
        public string ShortName => "hgb";
        public string Description => "List all banned players (host only). Usage: /hushgetbans";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (!HushMuteCommand.HostGuard()) return;

            BanManager? bans = HushPlugin.BanManager;
            if (bans == null) return;

            var list = bans.GetBans();
            if (list.Count == 0)
            {
                ChatUtils.AddGlobalNotification("Hush: no players are currently banned.");
                return;
            }

            var sb = new StringBuilder($"Hush: {list.Count} banned player(s):");
            foreach (var (steamId, nick) in list)
                sb.Append($"\n  {nick} ({steamId})");
            ChatUtils.AddGlobalNotification(sb.ToString());
        }
    }
}
