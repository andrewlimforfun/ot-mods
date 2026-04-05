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
    /// Bans a currently-online player. The host executes directly;
    /// whitelisted delegates relay the request to the host and also add the player to their own local ban list.
    /// </summary>
    public class HushBanCommand : IChatCommand
    {
        public string Name => "hushban";
        public string ShortName => "hb";
        public string Description => "Ban an online player. Host executes directly; delegates relay to host. Usage: /hushban <player>";
        public string Namespace => "hush";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /hushban <player>");
                return;
            }

            string query = string.Join(" ", args);
            PlayerDetail? player = PlayerUtils.FindPlayerByQuery(query);
            if (player == null)
            {
                ChatUtils.AddGlobalNotification($"Hush: player not found: \"{query}\"");
                return;
            }

            string mySteamId = SteamUtils.GetPlayerSteamID();

            // Host: execute ban directly
            if (PlayerUtils.GetHost()?.SteamID == mySteamId)
            {
                BanManager? bans = HushPlugin.BanManager;
                if (bans == null) return;
                if (bans.Ban(player.SteamID, player.UserNameClean))
                    ChatUtils.AddGlobalNotification($"Hush: banned {player.UserNameClean}.");
                else
                    ChatUtils.AddGlobalNotification($"Hush: {player.UserNameClean} is already banned.");
                return;
            }

            // Non-host: must be a delegate
            PlayerMuteManager? mutes = HushPlugin.MuteManager;
            if (mutes == null) return;

            if (!mutes.IsDelegate(mySteamId))
            {
                ChatUtils.AddGlobalNotification("Hush: only the host or a designated delegate can ban players.");
                return;
            }

            // Add to own local ban list
            HushPlugin.BanManager?.BanOffline(player.SteamID, player.UserNameClean);

            // Relay to host via sentinel
            TextChannelManager? tcm = NetworkSingleton<TextChannelManager>.I;
            if (tcm == null)
            {
                ChatUtils.AddGlobalNotification("Hush: not connected - cannot relay ban request.");
                return;
            }

            string sentinel = $"hush:ban:{player.SteamID}";
            Vector3 pos = tcm.MainPlayer != null ? tcm.MainPlayer.position : Vector3.zero;
            tcm.SendMessageAsync(
                Encoding.Unicode.GetBytes(sentinel),
                Encoding.Unicode.GetBytes(tcm.UserName ?? string.Empty),
                false,
                pos,
                mySteamId
            );
            ChatUtils.AddGlobalNotification($"Hush: ban request relayed for {player.UserNameClean}.");
        }

    }
}
