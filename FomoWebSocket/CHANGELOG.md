# Changelog

All notable changes to FomoWebSocket will be documented in this file.

## [1.2.7] - 2026-04-12

### Changed

- Dependency update to Alpha 0.0.11

## [0.2.3] - 2026-03-06

### Changed
- Fixed set message and notification format in-game

## [0.2.2] - 2026-03-04

### Changed
- Incoming WebSocket messages are now routed via `ChatUtils.DispatchIncomingText`: `/fomo*` commands are processed directly by `ChatCommandManager`; other slash commands are injected via the UI input system; plain text is sent through the regular chat pipeline
- TMP tag stripping now also removes `<align>` and `<rotate>` tags

## [0.2.1] - 2026-03-02

### Fixed
- Build configuration: corrected `Directory.Build.props` and `.csproj` to restore clean builds

## [0.1.0] - 2026-03-01

### Added
- Initial release as a standalone mod split from Fomo
- WebSocket server via Fleck, forwarding all chat entries and notifications in real time
- `/fomowebsockettoggle` (`/fwst`) - toggle the WebSocket server on/off
- `/fomowebsocketport` (`/fwsp`) - get or set the WebSocket server port at runtime
- `/fomowebsocketmessageformat` (`/fwsmf`) - get or set the chat message format string
- `/fomowebsocketnotificationformat` (`/fwsnf`) - get or set the notification format string
- `EnableFeature` config to start the server automatically on launch
- `WebSocketChatPort` config (default: `8765`)
- `MessageFormat` config with named placeholders (`{timestamp}`, `{channel}`, `{username}`, `{message}`, etc.)
- `NotificationFormat` config for system notifications
- Bidirectional relay - incoming WebSocket messages are injected into the in-game chat on the Unity main thread
