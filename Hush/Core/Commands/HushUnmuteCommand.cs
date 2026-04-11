using System.Text;
using Alpha.Core.Command;
using Alpha.Core.Util;
using Hush;
using Hush.Core;
using PurrNet;
using UnityEngine;

namespace Hush.Core.Commands
{
    /// <summary>
    /// Unmutes a player. The host executes directly;
    /// whitelisted delegates and mod admins relay the request to the host via a sentinel message.
    /// </summary>
    public class HushUnmuteCommand : IChatCommand
    {
        public string Name => "hushunmute";
        public string ShortName => "hum";
        public string Description => "Unmute a player. Host executes directly; delegates relay to host. Usage: /hushunmute <player|steamid>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushunmute <player|steamid>");
                return;
            }

            string query = string.Join(" ", args);

            // Try to resolve to a current lobby player; fall back to raw query as Steam ID for offline unmutes
            PlayerDetail? player = PlayerUtils.FindPlayerByQuery(query);
            string targetId = player?.SteamID ?? query;
            string displayName = player?.UserNameClean ?? query;

            // Host: execute directly
            if (PlayerUtils.GetHost()?.SteamID == SteamUtils.GetPlayerSteamID())
            {
                PlayerMuteManager? mutes = HushPlugin.MuteManager;
                if (mutes == null) return;
                if (mutes.Unmute(targetId))
                {
                    HushPlugin.SaveMutes();
                    ChatUtils.AddGlobalNotification($"Hush: unmuted {displayName}.");
                }
                else
                {
                    ChatUtils.AddGlobalNotification($"Hush: {displayName} is not muted.");
                }
                return;
            }

            // Non-host: relay to host via sentinel
            var tcm = NetworkSingleton<TextChannelManager>.I;
            if (tcm == null)
            {
                ChatUtils.AddGlobalNotification("Hush: not connected - cannot relay unmute request.");
                return;
            }

            string sentinel = $"hush:unmute:{targetId}";
            Vector3 pos = tcm.MainPlayer != null ? tcm.MainPlayer.position : Vector3.zero;
            tcm.SendMessageAsync(
                Encoding.Unicode.GetBytes(sentinel),
                Encoding.Unicode.GetBytes(tcm.UserName ?? string.Empty),
                false,
                pos,
                SteamUtils.GetPlayerSteamID()
            );
            ChatUtils.AddGlobalNotification($"Hush: unmute request sent for {displayName}.");
        }
    }
}
