# Fomo

A BepInEx mod for [On Together](https://store.steampowered.com/app/2688490/On_Together/) that enhances the in-game chat with configurable message limits and in-game utility commands.

## Features

- **In-game commands** :: Toggle and configure all features without leaving the game
- **Configurable chat history length** :: Increase how many messages are visible in the global and local chat windows
- **Message UI Redirection** :: Subscription to message UI allows for redirection to logs, websocket, telegram, etc.

> Looking for WebSocket support? Install the [FomoWebSocket](https://thunderstore.io/c/on-together/p/AndrewLin/FomoWebSocket/) add-on.
> Looking for chat file logging? Install the [FomoChatLog](https://thunderstore.io/c/on-together/p/AndrewLin/FomoChatLog/) add-on.
> Looking for Telegram support? Install the [FomoTelegram](https://thunderstore.io/c/on-together/p/AndrewLin/FomoTelegram/) add-on.

## In-Game Commands

Type any command into the in-game chat. Commands start with `/` and are **not sent** to other players.

- `/fomohelp` (`/fh`) :: Lists all available commands
- `/fomotoggle` (`/ft`) :: Toggle Fomo feature on/off
- `/fomoshowcommand` (`/fsc`) :: Toggle show/hide Fomo commands in chat
- `/fomomessagelimit [global|local] <number>` (`/fml`) :: Set the global or local chat message window size
- `/fomochatsinksetlocalrange <number>` (`/fcsslr`) :: Set local range for the chat sink
- `/fomochatsinkgetlocalrange` (`/fcsglr`) :: Get the local range for the chat sink
- `/fomochatsinkcleantags` (`/fcsct`) :: Toggle stripping TMP tags from all sink output
- `/fomoplayerid` (`/fpi`) :: Print your current player ID

### Examples

```
/fomosetmessagelimit global 100
/fsml local 50
```

## Configuration

Located in `BepInEx/config/com.andrewlin.ontogether.fomo.cfg`

- **General**
  - `EnableFeature` (default: `true`) :: Enable or disable all mod features
  - `ShowCommand` (default: `false`) :: Show commands in chat when used
  - `CleanChatSinkTags` (default: `false`) :: Strip TMP color/formatting tags from messages before dispatching to any sink
- **Chat**
  - `GlobalMessageLimitCount` (default: `50`) :: Max messages shown in global chat window
  - `LocalMessageLimitCount` (default: `25`) :: Max messages shown in local chat window
  - `ChatLogLocalRange` (default: `5`) :: Local range threshold for relaying local chat messages to sinks

## Installation

Use `r2modman` or the Thunderstore app for the simplest install.

**Manual:**
1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) into your On Together game folder
2. Copy `AndrewLin.Fomo.dll` into `BepInEx/plugins/`
3. Launch the game — a config file will be generated at `BepInEx/config/com.andrewlin.ontogether.fomo.cfg`
