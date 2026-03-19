# FomoWebSocket

A [Fomo](https://thunderstore.io/c/on-together/p/AndrewLin/Fomo/) add-on for [On Together](https://store.steampowered.com/app/2688490/On_Together/) that exposes in-game chat as a WebSocket server, enabling real-time external integrations.

## Requirements

- [Fomo](https://thunderstore.io/c/on-together/p/AndrewLin/Fomo/) (hard dependency)

## Features

- **WebSocket chat server** — Forwards all in-game chat messages and notifications to connected WebSocket clients in real time
- **Bidirectional relay** — Messages sent from a WebSocket client are injected into the in-game chat input; Fomo commands are supported too
- **Configurable format** — Separate format strings for chat messages and notifications
- Server starts automatically on launch when `EnableFeature` is `true`

## In-Game Commands

Type any command into the in-game chat. Commands start with `/` and are **not sent** to other players.

- `/fomowebsockettoggle` (`/fwst`) — Toggle the WebSocket server on/off
- `/fomowebsocketport [port]` (`/fwsp`) — Get or set the WebSocket server port (default: `8765`)
- `/fomowebsocketmessageformat [format]` (`/fwsmf`) — Get or set the chat message format string
- `/fomowebsocketnotificationformat [format]` (`/fwsnf`) — Get or set the notification format string

### Examples

```
/fomowebsockettoggle
/fomowebsocketport 9000
/fomowebsocketmessageformat [{timestamp:HH:mm} {channel:short}] {username}: {message}
```

## Configuration

Located in `BepInEx/config/com.andrewlin.ontogether.fomo.websocket.cfg`

- **General**
  - `EnableFeature` (default: `true`) — Start the WebSocket server automatically when the game launches
- **WebSocket**
  - `WebSocketChatPort` (default: `8765`) — Port the WebSocket server listens on
- **Formatting**
  - `MessageFormat` (default: `[{timestamp:HH:mm} {channel:short}] {username}: {message}`) — Format string for chat messages sent to clients
  - `NotificationFormat` (default: `[{timestamp:HH:mm}] {message}`) — Format string for notifications sent to clients

## Format Placeholders

The `MessageFormat` and `NotificationFormat` settings accept the following placeholders:

- `{timestamp}` — message time; accepts a C# DateTime format specifier (e.g. `{timestamp:HH:mm}`)
- `{channel}` — full channel label use `{channel:short}` for just the first character (`G` / `L`)
- `{username}` — display name of the sender
- `{message}` — message body
- `{distance}` — distance in metres for local messages, empty for global/notifications
- `{source}` — internal message source identifier
- `{playerid}` — sender's player ID

## Usage

1. Install **Fomo** and **FomoWebSocket** via r2modman or Thunderstore
2. Launch the game — the server starts automatically if `EnableFeature` is `true`
3. Connect any WebSocket client to `ws://localhost:8765`
4. Use `/fomowebsockettoggle` in chat to start or stop the server at runtime

### Receiving messages

Each chat message arrives as a plain text string formatted according to `MessageFormat`. Notifications use `NotificationFormat`.

### Sending messages

Send any plain text string to the WebSocket server. It will be injected into the in-game chat input. Fomo commands (e.g. `/fomohelp`) are also supported.

## Installation

Use `r2modman` or the Thunderstore app for the simplest install. Fomo must be installed first.

**Manual:**
1. Install [Fomo](https://thunderstore.io/c/on-together/p/AndrewLin/Fomo/) first
2. Copy `AndrewLin.FomoWebSocket.dll` into `BepInEx/plugins/`
3. Launch the game — the server starts automatically
