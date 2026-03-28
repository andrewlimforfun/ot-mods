# Remind

A BepInEx mod for [On Together](https://store.steampowered.com/app/2688490/On_Together/) that adds scheduled reminder commands to in-game chat.

## In-Game Commands

Type any command into the in-game chat. Commands start with `/` and are **not sent** to other players.

| Command | Short | Description |
|---|---|---|
| `/remindhelp` | `/rh` | List all available commands |
| `/remindtoggle` | `/rt` | Toggle Remind on/off |
| `/remindchatconfirmation` | `/rcc` | Toggle sending confirmation in chat after scheduling reminder |
| `/remindmein` | `/rmi` | Show a private notification after a delay |
| `/remindmeat` | `/rma` | Show a private notification at a specific time |
| `/remindlocalin` | `/rli` | Send a local chat message after a delay |
| `/remindlocalat` | `/rla` | Send a local chat message at a specific time |
| `/remindglobalin` | `/rgi` | Send a global chat message after a delay |
| `/remindglobalat` | `/rga` | Send a global chat message at a specific time |

### Reminder Syntax

`in` commands take a duration (timespan `hh:mm:ss` or ISO 8601 duration `nhnmns`), `at` commands take a local time (`HH:mm[:ss]`):

```
/remindmein 1h30m Take a break
/remindmein 0:05:00 Take a break
/remindmeat 14:45 Stand-up meeting

/remindlocalin 1:00:00 One hour left!
/remindlocalin 45s Quiz ends!
/remindlocalat 20:00 Game night starts

/remindglobalin 0:30:00 Checkpoint in 30 minutes
/remindglobalat 15:00 Boss fight time
```

## Configuration

Located in `BepInEx/config/com.andrewlin.ontogether.remind.cfg`

- `EnableFeature` (default: `true`) — Enable or disable all mod features
- `ShowCommand` (default: `false`) — Show commands in chat when used

## Installation

Use `r2modman` or the Thunderstore app for the simplest install.

**Manual:**
1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) into your On Together game folder
2. Copy `AndrewLin.Remind.dll` into `BepInEx/plugins/`
3. Launch the game — a config file will be generated automatically
