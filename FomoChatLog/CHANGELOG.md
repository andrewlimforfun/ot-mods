# Changelog

All notable changes to FomoChatLog will be documented in this file.

## [1.2.4] - 2026-03-29

### Added
- **Rolling daily log files** — each day's chat is written to a separate file with the date embedded in the filename (e.g. `on_together_chat_log_2026-03-29.txt`); no more single file that wipes itself at midnight
- `MaxLogDays` config (default: `5`) — number of daily log files to keep; oldest files are pruned automatically on startup
- `ChatLogManager` internal class — extracted path resolution, directory creation, and log pruning out of the plugin entry point

### Changed
- `ChatLogPath` config is now a **base path**; the date (`yyyy-MM-dd`) is appended automatically before the file extension
- `/fomochatloggetpath` now shows today's full dated path
- `/fomochatlogsetpath` sets the base path (date suffix is still appended automatically)

### Removed
- Previous behavior of deleting the single log file when a new day began

## [0.2.2] - 2026-03-04

### Changed
- TMP tag stripping (via core Fomo `CleanChatSinkTags` config) now also removes `<align>` and `<rotate>` tags

## [0.2.1] - 2026-03-02

### Fixed
- Build configuration: corrected `Directory.Build.props` and `.csproj` to restore clean builds

## [0.1.0] - 2026-03-01

### Added
- Initial release — extracted from Fomo core mod
- `LogFileChatSink`: appends chat entries to a local text file using `ChatEntryFormatter`
- `/fomochatlogtoggle` (`/fclt`) — toggle logging on/off in-game
- `/fomochatloggetpath` (`/fclgp`) — print the current log file path
- `/fomochatlogsetpath` (`/fclsp`) — change the log file path at runtime
- `/fomochatlogmessageformat` (`/fclmf`) — get or set the chat message format string
- `/fomochatlognotificationformat` (`/fclnf`) — get or set the notification format string
- `EnableFeature` config to enable or disable writing to the log file
- `ChatLogPath` config for the output file path (supports `~` and environment variables)
- `MessageFormat` config with named placeholders (`{timestamp}`, `{channel}`, `{username}`, `{message}`, etc.)
- `NotificationFormat` config for system notifications
- TMP tag stripping delegated to core Fomo `CleanChatSinkTags` config
- Log file cleared automatically when a new day begins
