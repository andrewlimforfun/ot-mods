using BepInEx.Logging;
using PurrNet;

namespace Desync.Core.Fixes
{
    /// <summary>
    /// H3 Fix: Detects when the host's SteamID is missing from the PlayerPanelController lists.
    /// Cannot safely fabricate entries, so logs a warning and notifies the user.
    /// </summary>
    public static class PlayerListRepairFix
    {
        static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Desync.PlayerListRepairFix");

        public static void Apply(DesyncDiagnostics diag)
        {
            _log.LogWarning($"H3: Lobby owner SteamID ({diag.LobbyOwnerSteamId}) is not in PlayerSteamIDs list.");
            _log.LogWarning($"H3: Tracked players: {diag.TotalTrackedPlayers}, SteamIDs in list: {diag.PlayerSteamIds.Count}");

            _log.LogWarning("H3: A player (likely host) despawned and was not re-added to the panel list.");

            Alpha.Core.Util.ChatUtils.AddGlobalNotification(
                "[Desync] Warning: Host player missing from player list. " +
                "This may prevent new players from joining. Host should consider restarting the session.");
        }
    }
}
