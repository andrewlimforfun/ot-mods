# FomoChatLog

A [Fomo](https://thunderstore.io/c/on-together/p/AndrewLin/Fomo/) add-on for [On Together](https://store.steampowered.com/app/2688490/On_Together/) that logs all in-game chat messages to a local text file with timestamps and channel labels.

## Requirements

- [Fomo](https://thunderstore.io/c/on-together/p/AndrewLin/Fomo/) (hard dependency)

## Features

- Appends every global and local chat message (and system notifications) to a text file
- **Rolling daily log files** - each day gets its own dated file (e.g. `on_together_chat_log_2026-03-29.txt`)
- Configurable number of days to retain (default: 5); older files are pruned automatically on startup
- Configurable base log file path
- Configurable format strings for chat messages and notifications
- Optional TMP tag stripping via the core Fomo `CleanChatSinkTags` config

## In-Game Commands

Type any command into the in-game chat. Commands start with `/` and are **not sent** to other players.

- `/fomochatlogtoggle` (`/fclt`) - Toggle chat file logging on/off
- `/fomochatloggetpath` (`/fclgp`) - Print the current log file path
- `/fomochatlogsetpath <path>` (`/fclsp`) - Set the log file path (supports `~` and environment variables)
- `/fomochatlogmessageformat [format]` (`/fclmf`) - Get or set the chat message format string
- `/fomochatlognotificationformat [format]` (`/fclnf`) - Get or set the notification format string

### Examples

```
/fomochatlogsetpath ~/on-together/fomo/chat.txt
/fomochatlogmessageformat [{timestamp:HH:mm}] [{channel:short}] {username}: {message}
```

## Configuration

Located in `BepInEx/config/com.andrewlin.ontogether.fomochatlog.cfg`

- **General**
  - `EnableFeature` (default: `true`) - Enable or disable writing to the log file
  - `ChatLogPath` (default: `~/on-together/fomo/on_together_chat_log.txt`) - Base path for log files. The current date (`yyyy-MM-dd`) is appended automatically before the file extension. Supports `~` and environment variables
  - `MaxLogDays` (default: `5`) - Number of daily log files to retain. Files older than the newest N days are deleted on startup
- **Formatting**
  - `MessageFormat` (default: `[{timestamp:yyyy-MM-dd HH:mm:ss}] [{channel}] {username}: {message}`) - Format string for chat messages written to the log file
  - `NotificationFormat` (default: `[{timestamp:yyyy-MM-dd HH:mm:ss}] {message}`) - Format string for system notifications written to the log file

> **Tag cleaning:** Use `CleanChatSinkTags` in the core **Fomo** config to strip TMP formatting tags from messages before they reach any sink, including this one.

## Format Placeholders

The `MessageFormat` and `NotificationFormat` settings accept the following placeholders:

- `{timestamp}` - message time; accepts a C# DateTime format specifier (e.g. `{timestamp:HH:mm}`)
- `{channel}` - full channel label use `{channel:short}` for just the first character (`G` / `L`)
- `{username}` - display name of the sender
- `{message}` - message body
- `{distance}` - distance in metres for local messages, empty for global/notifications
- `{source}` - internal message source identifier
- `{playerid}` - sender's player ID


## Log Format

Each day writes to a separate file named after the base path with the date inserted before the extension:

```
~/on-together/fomo/on_together_chat_log_2026-03-29.txt
~/on-together/fomo/on_together_chat_log_2026-03-28.txt
...
```

With the default `MessageFormat`:

```
[2026-03-29 21:30:15] [G] PlayerName: hello world
[2026-03-29 21:31:02] [L] OtherPlayer: hey nearby!
[2026-03-29 21:32:45] PlayerName joined the lobby.
```

## Installation

Use `r2modman` or the Thunderstore app for the simplest install. Fomo must be installed first.

**Manual:**
1. Install [Fomo](https://thunderstore.io/c/on-together/p/AndrewLin/Fomo/) first
2. Copy `AndrewLin.FomoChatLog.dll` into `BepInEx/plugins/`
3. Launch the game - a config file will be generated at `BepInEx/config/com.andrewlin.ontogether.fomochatlog.cfg`
