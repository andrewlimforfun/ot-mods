using System;
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
    /// Temporarily mutes a player for a given duration. The host executes locally;
    /// whitelisted delegates relay the request to the host via a hidden sentinel message.
    /// </summary>
    public class HushTempMuteCommand : IChatCommand
    {
        public string Name => "hushtmute";
        public string ShortName => "htm";
        public string Description => "Temporarily mute a player. Host executes immediately; delegates relay to host. Usage: /hushtmute <player> <duration>  e.g. /hushtmute bob 10m";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushtmute <player> <duration>  e.g. /hushtmute bob 10m");
                return;
            }

            // Last token is the duration; everything before is the player query
            string durationStr = args[args.Length - 1];
            string query = string.Join(" ", args, 0, args.Length - 1);

            PlayerDetail? player = PlayerUtils.FindPlayerByQuery(query);
            if (player == null)
            {
                ChatUtils.AddGlobalNotification($"Hush: player not found: \"{query}\"");
                return;
            }

            if (!TimeUtils.TryParseDuration(durationStr, out TimeSpan duration) || duration <= TimeSpan.Zero)
            {
                ChatUtils.AddGlobalNotification($"Hush: invalid duration \"{durationStr}\". Use e.g. 10m, 1h30m, 30s.");
                return;
            }

            PlayerMuteManager? mutes = HushPlugin.MuteManager;
            if (mutes == null) return;

            // Host: execute directly
            if (PlayerUtils.GetHost()?.SteamID == SteamUtils.GetPlayerSteamID())
            {
                mutes.MuteFor(player.SteamID, duration);
                HushPlugin.SaveMutes();
                ChatUtils.AddGlobalNotification($"Hush: muted {player.UserNameClean} for {FormatDuration(duration)}.");
                return;
            }

            // Non-host: send a hidden sentinel to the host which will validate the whitelist and act.
            var tcm = NetworkSingleton<TextChannelManager>.I;
            if (tcm == null)
            {
                ChatUtils.AddGlobalNotification("Hush: not connected - cannot relay mute request.");
                return;
            }

            string sentinel = $"\x01hush:tmute:{player.SteamID}:{(int)duration.TotalSeconds}";
            Vector3 pos = tcm.MainPlayer != null ? tcm.MainPlayer.position : Vector3.zero;
            tcm.SendMessageAsync(
                Encoding.Unicode.GetBytes(sentinel),
                Encoding.Unicode.GetBytes(tcm.UserName ?? string.Empty),
                false,
                pos,
                SteamUtils.GetPlayerSteamID()
            );
            ChatUtils.AddGlobalNotification($"Hush: mute request sent for {player.UserNameClean}.");
        }

        private static string FormatDuration(TimeSpan ts)
        {
            if (ts.TotalSeconds < 60) return $"{(int)ts.TotalSeconds}s";
            if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes}m";
            if (ts.TotalHours < 24)
            {
                string h = $"{ts.Hours}h";
                return ts.Minutes > 0 ? $"{h} {ts.Minutes}m" : h;
            }
            return $"{(int)ts.TotalDays}d";
        }
    }
}
