# Desync

Detect and fix lobby/host desync issues during long sessions (20+ hours).

Monitors lobby health periodically and applies configurable fixes to prevent
the host from becoming unresolvable and the lobby from becoming unjoinable.

## Commands

| Command | Short | Description |
|---------|-------|-------------|
| `/desynctoggle` | `/dst` | Toggle Desync monitoring on/off |
| `/desyncstatus` | `/dss` | Run health check and show diagnostics |
| `/desyncfix` | `/dsf` | Toggle individual fixes on/off |
| `/desynchelp` | `/dsh` | List all desync commands |

## Fixes

| Fix | Config | Description |
|-----|--------|-------------|
| LobbyRefresh | `Fix.LobbyRefresh` | Host refreshes lobby metadata to prevent Steam timeout |
| HostIdentity | `Fix.HostIdentity` | Detects when PurrNet host identity is lost |
| PlayerListRepair | `Fix.PlayerListRepair` | Detects host missing from player list |
| PersonaCache | `Fix.PersonaCache` | Refreshes Steam persona names for all players |

## Installation

Install via [Thunderstore](https://thunderstore.io/c/on-together/) or r2modman.

Requires: [Alpha](https://thunderstore.io/c/on-together/p/AndrewLin/Alpha/)
