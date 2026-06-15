using System.Text;

namespace Desync.Core
{
    /// <summary>
    /// Analyzes a <see cref="DesyncDiagnostics"/> snapshot and determines which issues are present.
    /// </summary>
    public class LobbyHealthMonitor
    {
        public DesyncDiagnostics Diagnostics { get; }

        public bool HasLobbyMetadataIssue { get; }
        public bool HasHostIdentityIssue { get; }
        public bool HasPlayerListIssue { get; }
        public bool HasPersonaCacheIssue { get; }

        public bool IsHealthy => !HasLobbyMetadataIssue && !HasHostIdentityIssue
                                 && !HasPlayerListIssue && !HasPersonaCacheIssue;

        public string Summary { get; }

        public LobbyHealthMonitor(DesyncDiagnostics diag)
        {
            Diagnostics = diag;

            // H1: Lobby metadata / owner invalid
            HasLobbyMetadataIssue = !diag.LobbyValid || diag.IsLobbyOwnerInvalid;

            // H2: No player has isServer flag
            HasHostIdentityIssue = diag.IsServerIdentityLost;

            // H3: Host SteamID missing from player list
            HasPlayerListIssue = diag.IsHostMissingFromList;

            // H4: Persona cache stale
            HasPersonaCacheIssue = diag.IsPersonaCacheStale;

            var sb = new StringBuilder();
            if (HasLobbyMetadataIssue) sb.Append("[H1:LobbyMetadata] ");
            if (HasHostIdentityIssue) sb.Append("[H2:HostIdentity] ");
            if (HasPlayerListIssue) sb.Append("[H3:PlayerList] ");
            if (HasPersonaCacheIssue) sb.Append("[H4:PersonaCache] ");
            Summary = sb.ToString().TrimEnd();
        }
    }
}
