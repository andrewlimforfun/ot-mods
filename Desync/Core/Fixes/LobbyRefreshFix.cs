using BepInEx.Logging;
using PurrNet;
using Steamworks;

namespace Desync.Core.Fixes
{
    /// <summary>
    /// H1 Fix: Refreshes Steam lobby metadata to keep the lobby alive.
    /// Only effective when the local player is the host.
    /// Writes a heartbeat timestamp to lobby data, which keeps Steam from
    /// considering the lobby inactive/expired.
    /// </summary>
    public static class LobbyRefreshFix
    {
        static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Desync.LobbyRefreshFix");

        public static void Apply(DesyncDiagnostics diag)
        {
            if (!diag.IsHost)
            {
                _log.LogDebug("H1 Fix: Not host - cannot refresh lobby metadata.");
                return;
            }

            try
            {
                string lobbyCode = MonoSingleton<MultiplayerManager>.I.LobbyCode;
                CSteamID lobbyId = new CSteamID(ulong.Parse(lobbyCode));

                // Writing any data to the lobby triggers Steam's internal heartbeat
                string timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                bool success = SteamMatchmaking.SetLobbyData(lobbyId, "heartbeat", timestamp);

                if (success)
                    _log.LogInfo($"H1 Fix: Refreshed lobby heartbeat ({timestamp}).");
                else
                    _log.LogWarning("H1 Fix: SetLobbyData returned false - lobby may be invalid.");
            }
            catch (System.Exception ex)
            {
                _log.LogError($"H1 Fix failed: {ex.Message}");
            }
        }
    }
}
