# FomoTelegram

A [Fomo](https://thunderstore.io/c/on-together/p/AndrewLin/Fomo/) add-on for [On Together](https://store.steampowered.com/app/2688490/On_Together/) that relays in-game chat messages to a **Telegram** group or channel via a Telegram Bot.

## Requirements

- [Fomo](https://thunderstore.io/c/on-together/p/AndrewLin/Fomo/) (hard dependency)

## Features

- Forwards **global chat**, **local chat**, and **system notifications** to any Telegram chat or group
- Respects the Fomo `ChatSinkLocalRange` setting — local messages from players outside your range are filtered before reaching Telegram
- Rate-limited background send queue — won't hammer the Telegram API during busy chat sessions
- Bidirectional relay — messages sent from Telegram back to the bot are injected into the in-game chat
- Configurable format strings for chat messages and notifications
- Per-category toggles (global / local / notifications) and a master on/off switch

## Setup

### 1. Create a Telegram Bot

1. Open Telegram and message **@BotFather**
2. Send `/newbot` and follow the prompts
3. Copy the **API token** (looks like `123456789:ABCdefGHI…`) to somewhere safe
4. Say hello to your own bot so it gains the rights to talk to you.

### 2. Find your Chat ID

1. Say hello to user **@userinfobot**.
2. It will reply with your chat ID.
3. User chat IDs are positive e.g. `123456789`

### 3. Configure the mod

After the first run a config file is generated at:

```
BepInEx/config/com.andrewlin.ontogether.fomotelegram.cfg
```

Fill in `TelegramBotApiKey` and `TelegramChatId`, then restart the game.

```ini
[Telegram]
TelegramBotApiKey = 123456789:ABCdefGHI…
TelegramChatId    = 234567890

[Filters]
RelayGlobalChat    = true
RelayLocalChat     = true
RelayNotifications = true

[Formatting]
MessageFormat      = [{channel:short}] {username}: {message}
NotificationFormat = {message}
```

## In-Game Commands

Type any command into the in-game chat. Commands start with `/` and are **not sent** to other players.

- `/fomotelegramtoggle` (`/ftt`) — Toggle Telegram forwarding on/off
- `/fomotelegramsetupinfo` (`/ftsinfo`) — Print setup instructions and current config status
- `/fomotelegrammessageformat [format]` (`/ftmf`) — Get or set the chat message format string
- `/fomotelegramnotificationformat [format]` (`/ftnf`) — Get or set the notification format string

### Examples

```
/fomotelegramtoggle
/fomotelegrammessageformat [{timestamp:HH:mm}] [{channel:short}] {username}: {message}
```

## Configuration

Located in `BepInEx/config/com.andrewlin.ontogether.fomotelegram.cfg`

- **General**
  - `EnableFeature` (default: `true`) — Master switch; disable to pause all Telegram forwarding without removing the plugin
- **Telegram**
  - `TelegramBotApiKey` — Your bot token from @BotFather (e.g. `123456:ABC-DEF…`)
  - `TelegramChatId` — Target chat / group / channel ID (group IDs are negative)
- **Filters**
  - `RelayGlobalChat` (default: `true`) — Forward global chat messages to Telegram
  - `RelayLocalChat` (default: `true`) — Forward local chat messages to Telegram
  - `RelayNotifications` (default: `true`) — Forward system notifications (joins, leaves, etc.) to Telegram
- **Formatting**
  - `MessageFormat` (default: `[{channel:short}] {username}: {message}`) — Format string for chat messages sent to Telegram
  - `NotificationFormat` (default: `{message}`) — Format string for system notifications sent to Telegram

## Format Placeholders

The `MessageFormat` and `NotificationFormat` settings accept the following placeholders:

- `{timestamp}` — message time; accepts a C# DateTime format specifier (e.g. `{timestamp:HH:mm}`)
- `{channel}` — full channel label use `{channel:short}` for just the first character (`G` / `L`)
- `{username}` — display name of the sender
- `{message}` — message body
- `{distance}` — distance in metres for local messages, empty for global/notifications
- `{source}` — internal message source identifier
- `{playerid}` — sender's player ID

## Installation

Use `r2modman` or the Thunderstore app for the simplest install. Fomo must be installed first.

**Manual:**
1. Install [Fomo](https://thunderstore.io/c/on-together/p/AndrewLin/Fomo/) first
2. Copy `AndrewLin.FomoTelegram.dll` into `BepInEx/plugins/`
3. Launch the game — a config file will be generated at `BepInEx/config/com.andrewlin.ontogether.fomotelegram.cfg`
4. Fill in `TelegramBotApiKey` and `TelegramChatId` and restart

## Dependencies

- [Fomo](https://thunderstore.io/c/on-together/p/AndrewLin/Fomo/) — Core mod providing `IChatSink`, `ChatEntry`, `ChatSinkManager`
- [BepInEx 5.x](https://github.com/BepInEx/BepInEx) — Mod loader
- [Newtonsoft.Json](https://www.newtonsoft.com/json) — JSON parsing for Telegram HTTP API responses

> Telegram communication uses the standard .NET `HttpClient` against the Telegram Bot HTTP API directly — no third-party Telegram library, ensuring full Mono/BepInEx compatibility.

## License

MIT — same as the parent Fomo mod.
