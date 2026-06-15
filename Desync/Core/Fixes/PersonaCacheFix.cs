using System;
using BepInEx.Logging;
using PurrNet;
using Steamworks;

namespace Desync.Core.Fixes
{
    /// <summary>
    /// H4 Fix: Refreshes the Steam persona name cache for all players in the lobby.
    /// Calls RequestUserInformation for each tracked SteamID to force Steam to
    /// re-fetch their display names.
    /// </summary>
    public static class PersonaCacheFix
    {
        static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Desync.PersonaCacheFix");

        public static void Apply(DesyncDiagnostics diag)
        {
            int refreshed = 0;
            try
            {
                PlayerPanelController? controller = NetworkSingleton<PlayerPanelController>.I;
                if (controller == null) return;

                for (int i = 0; i < controller.PlayerSteamIDs.Count; i++)
                {
                    string steamIdStr = controller.PlayerSteamIDs[i];
                    if (string.IsNullOrEmpty(steamIdStr)) continue;

                    CSteamID csteamId = new CSteamID(ulong.Parse(steamIdStr));
                    // RequestUserInformation returns true if a callback will come (name not cached)
                    // false if already cached. Either way, it refreshes the internal cache.
                    SteamFriends.RequestUserInformation(csteamId, bRequireNameOnly: true);
                    refreshed++;
                }

                _log.LogInfo($"H4 Fix: Requested persona refresh for {refreshed} players.");
            }
            catch (Exception ex)
            {
                _log.LogError($"H4 Fix failed: {ex.Message}");
            }
        }
    }
}
