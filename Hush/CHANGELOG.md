# Changelog

All notable changes to this project will be documented in this file.

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
