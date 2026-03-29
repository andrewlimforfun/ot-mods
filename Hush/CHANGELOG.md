# Changelog

All notable changes to this project will be documented in this file.

## [0.1.1] - 2026-03-29

### Changed

- Add notification on timed mute expiry
- Add notification on blocked players

## [0.1.0] - 2026-03-29

### Added

- Player mute system (host only): suppress all messages from specific players server-side.
- `/hushmute <player>` — permanently mute a player by name or Steam ID suffix.
- `/hushtmute <player> <duration>` — timed mute with ISO 8601 / `hh:mm:ss` duration (e.g. `10m`, `1h30m`, `30s`).
- `/hushunmute <player>` — remove a permanent or timed mute. Accepts Steam ID for offline players.
- `/hushgetmutes` — list all currently muted players with their expiry time.
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
