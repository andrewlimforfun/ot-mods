using System.Text;
using Alpha.Core.Command;
using Alpha.Core.Util;
using Hush;
using Hush.Core;

namespace Hush.Core.Commands
{
    /// <summary>Host-only command to list all players on the timed-mute delegate whitelist.</summary>
    public class HushDelegateListCommand : IChatCommand
    {
        public string Name => "hushdelegatelist";
        public string ShortName => "hdl";
        public string Description => "List all mute delegates (host only). Usage: /hushdelegatelist";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (!HushMuteCommand.HostGuard()) return;

            PlayerMuteManager? mutes = HushPlugin.MuteManager;
            if (mutes == null) return;

            var delegates = mutes.GetDelegates();
            if (delegates.Count == 0)
            {
                ChatUtils.AddGlobalNotification("Hush: no mute delegates configured.");
                return;
            }

            var sb = new StringBuilder($"Hush: {delegates.Count} mute delegate(s):");
            foreach (string id in delegates)
            {
                PlayerDetail? player = PlayerUtils.FindPlayerBySteamID(id);
                string name = player != null ? $"{player.UserNameClean} ({id})" : id;
                sb.Append($"\n  {name}");
            }
            ChatUtils.AddGlobalNotification(sb.ToString());
        }
    }
}
