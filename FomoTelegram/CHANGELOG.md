# Changelog

## [1.2.8] - 2026-04-16

### Changed

- Improved error logging in `FomoTelegramManager.ValidateAndStartReceiverAsync`:
  - `HttpRequestException` (network/TLS failure) is now caught separately and logged as a network error rather than misleadingly suggesting an invalid API key.
  - Full inner exception chain is logged so the root cause (DNS, TLS negotiation, missing certificates) is visible.
  - Telegram `error_code` and `description` fields are extracted from the JSON response and logged when the API itself rejects the request.
  - Added `LogDebug` of the raw `getMe` HTTP status and response body.
- Added `ServicePointManager.SecurityProtocol = Tls12` in the constructor to fix TLS negotiation failures on Mono/Unity.

---

## [1.2.7] - 2026-04-12

### Changed

- Dependency update to Alpha 0.0.11

## [0.2.3] - 2026-03-06

### Changed
- Fixed set message and notification format in-game

## [0.2.2] - 2026-03-04

### Changed
- Incoming Telegram messages are now routed via `ChatUtils.DispatchIncomingText`: `/fomo*` commands are processed directly by `ChatCommandManager`; other slash commands are injected via the UI input system; plain text is sent through the regular chat pipeline
- TMP tag stripping now also removes `<align>` and `<rotate>` tags

## [0.2.1] - 2026-03-02

### Fixed
- Build configuration: corrected `Directory.Build.props` and `.csproj` to restore clean builds

## [0.1.0] - 2026-03-01

### Added
- Initial release of FomoTelegram
- `FomoTelegramPlugin` — BepInEx entry point; reads config and registers the sink with Fomo's `SinkManager`
- `FomoTelegramManager` — owns an `HttpClient` pointed at the Telegram Bot HTTP API, validates credentials on startup, and runs a rate-limited background send queue (≥50 ms between messages); also runs a long-poll receiver loop to inject Telegram replies back into the game
- `TelegramChatSink` — `IChatSink` implementation; applies per-channel filters (global / local / notifications) and formats messages using configurable format strings
- `/fomotelegramtoggle` (`/ftt`) — toggle Telegram forwarding on/off
- `/fomotelegramsetupinfo` (`/ftsinfo`) — print setup instructions and current config status in-game
- `/fomotelegrammessageformat` (`/ftmf`) — get or set the chat message format string at runtime
- `/fomotelegramnotificationformat` (`/ftnf`) — get or set the notification format string at runtime
- Config entries: `EnableFeature`, `TelegramBotApiKey`, `TelegramChatId`, `RelayGlobalChat`, `RelayLocalChat`, `RelayNotifications`, `MessageFormat`, `NotificationFormat`
- Uses `HttpClient` + `Newtonsoft.Json` against the Telegram Bot HTTP API directly — no `Telegram.Bot` library dependency, ensuring full Mono/BepInEx compatibility
