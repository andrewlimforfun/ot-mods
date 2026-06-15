using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Desync.Core.Commands
{
    public class DesyncStatusCommand : IChatCommand
    {
        public string Name => "desyncstatus";
        public string ShortName => "dss";
        public string Description => "Run health check and show lobby/host diagnostics.";
        public string Namespace => "desync";

        public void Execute(string[] args)
        {
            if (DesyncPlugin.Enabled?.Value != true)
            {
                ChatUtils.AddGlobalNotification("Desync is disabled.");
                return;
            }

            DesyncDiagnostics diag = DesyncDiagnostics.Capture();
            LobbyHealthMonitor monitor = new LobbyHealthMonitor(diag);

            string fixStates =
                $"Fixes: Lobby={On(DesyncPlugin.FixLobbyRefresh)} " +
                $"Host={On(DesyncPlugin.FixHostIdentity)} " +
                $"List={On(DesyncPlugin.FixPlayerListRepair)} " +
                $"Persona={On(DesyncPlugin.FixPersonaCache)}";

            string status = monitor.IsHealthy
                ? "All OK"
                : $"Issues: {monitor.Summary}";

            string msg =
                $"Desync Health: {status}\n" +
                $"Lobby Valid: {diag.LobbyValid}\n" +
                $"Lobby Owner: {diag.LobbyOwnerSteamId}\n" +
                $"Local Player: {diag.LocalSteamId}\n" +
                $"IsHost: {diag.IsHost}\n" +
                $"Host in list: {!diag.IsHostMissingFromList}\n" +
                $"Host persona: \"{diag.HostPersonaName}\"\n" +
                $"Interval: {DesyncPlugin.MonitorIntervalSec?.Value}s\n" +
                fixStates;

            DesyncPlugin.Log.LogInfo(msg);
            ChatUtils.AddGlobalNotification(msg);
        }

        static string On(BepInEx.Configuration.ConfigEntry<bool>? entry) =>
            entry?.Value == true ? "ON" : "OFF";
    }
}
