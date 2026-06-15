using BepInEx.Logging;
using PurrNet;

namespace Desync.Core.Fixes
{
    /// <summary>
    /// H2 Fix: Local player is the lobby owner but NetworkManager.isHost is false,
    /// indicating a mismatch between Steam lobby ownership and PurrNet host state.
    /// </summary>
    public static class HostIdentityFix
    {
        static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Desync.HostIdentityFix");

        public static void Apply(DesyncDiagnostics diag)
        {
            _log.LogWarning("H2: Local player is lobby owner but NetworkManager.isHost is false.");
            _log.LogWarning($"H2: LobbyOwner={diag.LobbyOwnerSteamId}, LocalPlayer={diag.LocalSteamId}, IsHost={diag.IsHost}.");
            _log.LogWarning("H2: Host may need to soft-reconnect or restart the session.");

            Alpha.Core.Util.ChatUtils.AddGlobalNotification(
                "[Desync] Warning: You are the lobby owner but lost host authority. " +
                "New players may not be able to join. Try restarting the session.");
        }
    }
}
