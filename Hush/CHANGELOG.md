# Changelog

All notable changes to this project will be documented in this file.

## [0.1.8] - 2026-04-11

### Added

- Relay commands (e.g. `hush:tmute:`) now accept a player name query in place of a raw Steam ID64. If the target field is not a valid Steam ID64 (`SteamUtils.IsSteamID`), it is resolved via `PlayerUtils.FindPlayerByQuery` before execution. Unresolvable queries are rejected with a warning.
- **`RelayExecutor`** — relay business logic extracted from `TextChannelManagerPatch` into a standalone injectable class. All game-API dependencies (mute manager, ban, name resolution, notifications) are injected via constructor, making the relay logic fully unit-testable.
- `/hushunmute` is no longer host-only: whitelisted delegates and mod admins can now relay unmute requests to the host via `hush:unmute:<target>` sentinel, matching the existing tmute/ban relay pattern.
- Relay sentinels now accept a player name query in place of a raw Steam ID64 for all relay commands. If the target is not a valid Steam ID64, it is resolved via `PlayerUtils.FindPlayerByQuery` before execution; unresolvable queries are rejected with a warning.

## [0.1.7] - 2026-04-11

### Added

- Improve relay and server side logging

## [0.1.6] - 2026-04-10

### Added

- `/hushlogverbose` (`/hlv`) - toggle verbose BepInEx logging for Hush at runtime. Off by default; when enabled, debug-level traces (mute checks, relay paths, save/load messages) are emitted to the log.
- `Hush.Tests` project with 81 unit tests covering `ChatFilterManager`, `PlayerMuteManager`, `RelayParser`, and `DurationFormatter`.

### Changed

- Removed client-side suppression of `hush:` sentinel messages. If a sentinel leaks to a client it will now display in chat as a visible indicator that the server-side intercept failed.
- Relay parsing and duration formatting extracted into standalone `RelayParser` / `DurationFormatter` classes (no behaviour change).

## [0.1.5] - 2026-04-05

### Added

- `BanManager` class: centralises all ban operations (add, add offline, remove, list) on top of the game's `DataManager.BanData`.
- `/hushban <player>` (`/hb`) - ban an online player. Host executes directly; whitelisted delegates relay the request to the host and also add the player to their own local ban list.
- `/hushbanoffline <steamid> <nickname>` (`/hbo`) - ban a player by Steam ID without them being online (host only).
- `/hushunban <player|steamid>` (`/hub`) - remove a ban; accepts player query or raw Steam ID for offline removal (host only).
- `/hushgetbans` (`/hgb`) - list all banned players (host only).
- Ban commands share the existing delegate whitelist: delegates may relay `/hushban` to the host in the same way as timed mutes.

## [0.1.4] - 2026-04-02

## Added

- Fixed Hush nullrefs

## [0.1.3] - 2026-03-31

## Added

- `/hushversion` (`/hv`) command

## Changed

- Attempt 1 at fixing the host-side of the `hush:tmute:` relay

## [0.1.2] - 2026-03-30

### Added

- Mute-delegate relay system: the host can whitelist trusted non-host players to use `/hushtmute`. Delegates send a hidden sentinel message; the host validates and executes the mute server-side - the sentinel is never visible to other clients.
- `/hushdelegateadd <player>` (`/hdda`) - add a player to the mute-delegate whitelist (host only).
- `/hushdelegateremove <player>` (`/hddr`) - remove a player from the whitelist; accepts a raw Steam ID for offline players (host only).
- `/hushdelegatelist` (`/hddl`) - list all current delegates (host only).
- Delegate list persisted to the mutes JSON file and restored on load.

### Changed

- `/hushtmute` is no longer host-only: whitelisted delegates may use it and their request is relayed to the host for execution.


## [0.1.1] - 2026-03-29

### Changed

- Add notification on timed mute expiry
- Add notification on blocked players

## [0.1.0] - 2026-03-29

### Added

- Player mute system (host only): suppress all messages from specific players server-side.
- `/hushmute <player>` - permanently mute a player by name or Steam ID suffix.
- `/hushtmute <player> <duration>` - timed mute with ISO 8601 / `hh:mm:ss` duration (e.g. `10m`, `1h30m`, `30s`).
- `/hushunmute <player>` - remove a permanent or timed mute. Accepts Steam ID for offline players.
- `/hushgetmutes` - list all currently muted players with their expiry time.
- Mute list persisted to `BepInEx/config/com.andrewlin.ontogether.hush.mutes.json`; timed mute expiry times are preserved across game restarts.

## [0.0.1] - 2026-03-20

### Added

- Server-side relay intercept: when installed on the host, the filter is enforced before the message is broadcast to all clients.
- Client-side display filter: censors or blocks messages at the UI layer as a second pass.
- `ChatFilterManager` with two modes: `Censor` (replace matched text with asterisks) and `Block` (suppress the entire message).
- Literal word filtering with automatic case-insensitive whole-word boundary matching.
- Raw regex pattern filtering with full inline flag support (`(?i)`, `\b`, etc.).
- In-game commands for managing the filter list without restarting the game.
- Filter list persisted to `BepInEx/config/AndrewLin.Hush.filter.json`; updated automatically on every mutation.
- BepInEx config entry for `Filter > Action` (`Censor` or `Block`), live-reload supported via ConfigurationManager.

| Command | Description |
|---|---|
| `/hushaddword <word>` | Add a literal word |
| `/hushremoveword <word>` | Remove a literal word |
| `/hushgetwords` | List all words |
| `/hushaddpattern <regex>` | Add a regex pattern |
| `/hushremovepattern <regex>` | Remove a regex pattern |
| `/hushgetpatterns` | List all patterns |
| `/hushtoggle` | Toggle the filter on or off |
| `/hushfilteraction <action>` | Set the filter action (`Censor` or `Block`) |
