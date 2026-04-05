using System.Collections.Generic;
using BepInEx.Logging;
using PurrNet;

namespace Hush.Core
{
    /// <summary>
    /// Host-side player ban manager. Wraps the game's DataManager.BanData to provide a single
    /// place for ban operations: persistent list management and RPC kicking for online players.
    /// </summary>
    public class BanManager
    {
        private readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{HushPlugin.ModName}.BM");

        /// <summary>Returns true if the Steam ID is in the ban list.</summary>
        public bool IsBanned(string steamId) =>
            MonoSingleton<DataManager>.I.BanData.BanServerPlayers.Contains(steamId);

        /// <summary>
        /// Bans a player: adds to the persistent list, saves, and fires the BanRPC to kick if they are online.
        /// Returns false if already banned.
        /// </summary>
        public bool Ban(string steamId, string displayName)
        {
            BanData banData = MonoSingleton<DataManager>.I.BanData;
            if (banData.BanServerPlayers.Contains(steamId))
                return false;

            banData.BanServerPlayers.Add(steamId);
            banData.BanServerPlayerNicks.Add(displayName);
            MonoSingleton<DataManager>.I.SaveBanData();
            _log.LogInfo($"Banned: {displayName} ({steamId})");

            // Kick if online
            PlayerPanelController? ppc = NetworkSingleton<PlayerPanelController>.I;
            TextChannelManager? tcm = NetworkSingleton<TextChannelManager>.I;
            if (ppc != null && tcm != null)
            {
                int idx = ppc.PlayerSteamIDs.IndexOf(steamId);
                if (idx >= 0)
                    tcm.MainPlayerController.BanRPC(ppc.PlayerIDs[idx], isFirstBan: true);
            }

            return true;
        }

        /// <summary>
        /// Adds a player to the ban list without kicking (offline ban). Saves persistently.
        /// Returns false if already banned.
        /// </summary>
        public bool BanOffline(string steamId, string displayName)
        {
            BanData banData = MonoSingleton<DataManager>.I.BanData;
            if (banData.BanServerPlayers.Contains(steamId))
                return false;

            banData.BanServerPlayers.Add(steamId);
            banData.BanServerPlayerNicks.Add(displayName);
            MonoSingleton<DataManager>.I.SaveBanData();
            _log.LogInfo($"Offline banned: {displayName} ({steamId})");
            return true;
        }

        /// <summary>
        /// Removes a player from the ban list. Returns false if they were not banned.
        /// </summary>
        public bool Unban(string steamId)
        {
            BanData banData = MonoSingleton<DataManager>.I.BanData;
            int idx = banData.BanServerPlayers.IndexOf(steamId);
            if (idx < 0)
                return false;

            string nick = banData.BanServerPlayerNicks[idx];
            banData.BanServerPlayers.RemoveAt(idx);
            banData.BanServerPlayerNicks.RemoveAt(idx);
            MonoSingleton<DataManager>.I.SaveBanData();
            _log.LogInfo($"Unbanned: {nick} ({steamId})");
            return true;
        }

        /// <summary>Returns a snapshot of all banned entries as (SteamId, Nickname) pairs.</summary>
        public List<(string SteamId, string Nick)> GetBans()
        {
            BanData banData = MonoSingleton<DataManager>.I.BanData;
            var result = new List<(string, string)>(banData.BanServerPlayers.Count);
            for (int i = 0; i < banData.BanServerPlayers.Count; i++)
                result.Add((banData.BanServerPlayers[i], banData.BanServerPlayerNicks[i]));
            return result;
        }
    }
}
