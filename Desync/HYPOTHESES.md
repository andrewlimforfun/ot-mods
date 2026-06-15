# Desync - Lobby/Host Desync Detection and Fixes

## Problem

After very long sessions (e.g., 20+ hours), the host becomes unresolvable:
- `/alphaserverinfo` shows empty Host Name and Host Steam
- New players can no longer join the lobby from outside
- Existing players remain connected and functional

## Hypotheses

### H1: Steam Lobby Metadata Expired

**Cause:** Steam lobbies rely on periodic heartbeats. After prolonged uptime, if the host's
Steam client loses connectivity briefly or the lobby metadata times out, `GetLobbyOwner`
returns an invalid/zero CSteamID. The lobby becomes invisible to searchers.

**Detection:** `SteamMatchmaking.GetLobbyOwner(lobbyId)` returns `0` or mismatches the known host.

**Fix (LobbyRefresh):** Host periodically writes a timestamp to lobby metadata via
`SteamMatchmaking.SetLobbyData`, keeping the Steam lobby heartbeat alive.

---

### H2: PurrNet Host Identity Lost

**Cause:** PurrNet may internally re-establish its transport layer after a network hiccup.
If the host's `PlayerID` gets recreated, the `PlayerPanelController.PlayerIDs` list retains
the old one where `isServer` is now stale - no entries have `isServer == true`.

**Detection:** Zero `PlayerID` entries with `isServer == true` in the player list.

**Fix (HostIdentity):** When zero server players are detected, re-resolve the host by
matching the Steam lobby owner's SteamID against `PlayerSteamIDs` and marking that entry's
corresponding connection as the server.

---

### H3: Host Despawned from Player List

**Cause:** A momentary network hiccup causes the host's `NetworkTransform` to despawn and
respawn. `DespawnHandle` removes them from all lists. If the re-add RPC is lost or arrives
before the new transform is ready, the host is permanently missing from `PlayerPanelController`.

**Detection:** The Steam lobby owner's SteamID is not present in `PlayerSteamIDs` at all,
but the player count from PurrNet disagrees with the list length.

**Fix (PlayerListRepair):** No automatic fix (too dangerous to fabricate list entries).
Logs a warning and suggests the host rejoin. A future version could hook the spawn path
to ensure re-addition.

---

### H4: Steam Persona Cache Eviction

**Cause:** `SteamFriends.GetFriendPersonaName(csteamid)` only returns names for users
the local client has "seen" recently. After many hours, Steam's persona cache may evict
entries, returning empty string. This doesn't affect connectivity but breaks display.

**Detection:** `GetFriendPersonaName` returns empty for a SteamID that is still in the
player list.

**Fix (PersonaCache):** Periodically call `SteamFriends.RequestUserInformation` for all
players in the lobby to keep the persona cache warm.

---

### H5: Steam Lobby Delisted (No Fix)

**Cause:** Steam may delist lobbies open for extremely long periods. The lobby works for
current players but new joins fail entirely - both browser and code-join.

**Detection:** Lobby is valid locally but external join attempts fail. Cannot be detected
from inside the session reliably.

**Fix:** None from client side. The only solution is for the host to recreate the lobby.
The mod logs a warning if H1-H4 are all healthy but the lobby has been active 12+ hours.

## Configuration

Each fix is independently toggleable via BepInEx config:
- `Fix.LobbyRefresh` (default: true) - H1 mitigation
- `Fix.HostIdentity` (default: true) - H2 mitigation
- `Fix.PlayerListRepair` (default: true) - H3 detection/logging
- `Fix.PersonaCache` (default: true) - H4 mitigation
- `Monitor.IntervalSec` (default: 300) - Health check interval in seconds
