# Changelog

All notable changes to this project will be documented in this file.

## [1.2.9] - 2026-04-18

### Added

- **Host-only join notification** - when a player joins the server, a second notification is shown to the host only displaying their display name and Steam ID (`Name - 76561198XXXXXXXXX`).

## [1.2.7] - 2026-04-12

### Changed

- Dependency update to Alpha 0.0.11

## [1.2.6] - 2026-04-12

### Changed

- Depends on Alpha 0.0.10

## [1.2.5] - 2026-04-11

### Changed

- Depends on Alpha 0.0.8

## [1.1.0]

### Changed
- Merged `/fomochatsinkgetlocalrange` (`/fcsglr`) and `/fomochatsinksetlocalrange` (`/fcsslr`) into `/fomochatsinklocalrange` (`/fcslr`) - no args gets the current value; passing a number sets it
- `/fomoincomingmode` (`/fim`) - toggle whether messages dispatched from external sources (Telegram, WebSocket, etc.) are sent as global or local chat; persisted via config
- `DispatchIncomingLocal` config entry (`Chat` section, default `false`) - set the default channel for dispatched incoming messages

## [1.0.0]

### Changed
- Merged `/fomochatsinkgetlocalrange` (`/fcsglr`) and `/fomochatsinksetlocalrange` (`/fcsslr`) into `/fomochatsinklocalrange` (`/fcslr`) - no args gets the current value; passing a number sets it

## [0.2.4]

### Added
- `/fomochatlocal` (`/fcl`) - send chat locally
 
## [0.2.3] - 2026-03-06

### Changed
- Fixed set message and notification format in-game

## [0.2.2] - 2026-03-04

### Added
- Notifications when the local player disconnects or returns to the main menu (new `MainSceneManagerPatch`)
- `ChatUtils.DispatchIncomingText` - shared routing helper for text received from external sources (WebSocket, Telegram): `/fomo*` commands are processed directly by `ChatCommandManager`; other slash commands are injected via the UI input system; plain text is sent through the regular chat pipeline
- `ChatUtils` and `PlayerUtils` moved into `Fomo.Core.Util` namespace

### Changed
- TMP tag cleanup regex simplified; added `align` and `rotate` to the supported tag list
- `Plugin.cs` renamed to `FomoPlugin.cs`
- `CommandManager` renamed to `ChatCommandManager`; `CommandArgs` renamed to `ChatCommandArgs`
- Renamed in-game commands:
  - `/fomomessagelimit [global|local] <number>` (`/fml`) - set the global or local chat window size
  - `/fomochatsinksetlocalrange <number>` (`/fcsslr`) - set local range for chat sinks
  - `/fomochatsinkgetlocalrange` (`/fcsglr`) - get local range for chat sinks
  - `/fomochatsinkcleantags` (`/fcsct`) - toggle TMP tag stripping for all sinks
  - `/fomoplayerid` (`/fpi`) - print your current player ID

## [0.2.1] - 2026-03-02

### Fixed
- Build configuration: corrected `Directory.Build.props` and all `.csproj` files to restore clean builds across all sub-projects

## [0.1.0] - 2026-03-01

### Removed
- Removed Websocket functionalities and moved to separate mod
- Removed ChatLog functionalities and moved to separate mod

## [0.0.9] - 2026-02-25

### Changed
- WebSocket Chat: Trim messages so newlines are gone 
- Notification log is now cleaned from TMP tags
- Add back self in chat logs and web socket

## [0.0.8] - 2026-02-22

### Changed
- Fixed chat log write blocked
- Better chat log default path `(OS specific user home)/on-together/fomo/on_together_chat_log.txt`
- Notification chat is now cleaned from TMP tags

## [0.0.7] - 2026-02-22

### Added
- `/fomowebsocket` (`/fws`) WebSocket chat server feature. Allow sending chats from websocket client.

### Changed
- Chat Log deletion is now based on creation date not last modified date. This is for cases players cross over start of day.
- TMP tags regex cleanup now uses non-capturing group for efficiency.
- Notification is now logged and also sent to WebSocket.

### Removed
- `/help` use `/fomohelp` instead.

## [0.0.6] - 2026-02-21

### Changed
- Default chat log path is now `~/on-together/fomo/on_together_chat_log.txt`

## [0.0.5] - 2026-02-21

### Added
- Commands
  - `/fomosetchatloglocalrange`
  - `/fomogetchatloglocalrange`

### Changed
- ChatLogPath now accepts `~` and windows/unix style environment variables
- TMP cleaner also removes voffset and font

## [0.0.4] - 2026-02-20

### Changed
- Rename command
  - `/fomtoggle` to `/fomotoggle`

## [0.0.3] - 2026-02-20

### Changed
- Rename command
  - `/fomousefeature` (`/fuf`) to  `/fomotoggle` (`/ft`)

## [0.0.2] - 2026-02-18

### Changed
- Updated manifest description
- Updated short commands
  - `/fomousefeature` (`/fuf`) - toggle all features on/off
  - `/fomoshowcommand` (`/fsc`) - toggle command visibility in chat  

## [0.0.1] - 2026-02-18

### Added
- Chat logging to file with timestamps and channel labels (`[Global]` / `[Local]`)
- Configurable chat log file path via config and `/fomosetchatlogpath` command
- Option to strip TMP formatting tags from the chat log via `CleanChatLogTags` config and `/fomocleanchatlogtags` command
- Configurable global and local chat window message limits via `GlobalMessageLimitCount` / `LocalMessageLimitCount` config and `/fomosetmessagelimit` command
- Raised notification badge cap from hardcoded 99 to configurable value (default 300, max 999) via `NotificationLimit` config
- In-game command system with the following commands:
  - `/fomohelp` (`/fh`) - list all commands
  - `/fomousefeature` (`/uff`) - toggle all features on/off
  - `/fomoshowcommand` (`/sfc`) - toggle command visibility in chat
  - `/fomousechatlog` (`/fucl`) - toggle chat file logging
  - `/fomogetchatlogpath` (`/fgclp`) - print current log file path
  - `/fomosetchatlogpath` (`/fsclp`) - set log file path
  - `/fomocleanchatlogtags` (`/fcclt`) - toggle TMP tag stripping in log
  - `/fomosetmessagelimit` (`/fsml`) - set global/local chat window size
