# Echo

A BepInEx mod for [On Together](https://store.steampowered.com/app/2688490/On_Together/) that adds teleportation, name copying, and outfit copying utilities.

> **Note:** Echo can cause chaos. Install with care.

## In-Game Commands

Type any command into the in-game chat. Commands start with `/` and are **not sent** to other players.

| Command | Short | Description |
|---|---|---|
| `/echotpto <player> [x y z]` | `/etp` | Teleport to a player. Optional XYZ offset relative to their position. |
| `/echocopyname <player>` | `/ecn` | Copy another player's display name (including TMP colour/style tags). Token-gated — see `Security > AccessToken` in config. |
| `/echorevertname` | `/ern` | Revert your display name to what it was before any Echo rename. |
| `/echocopyoutfit <player>` | `/eco` | Copy another player's full outfit and appearance onto yourself. |
| `/echotoggle` | `/ht` | Toggle all Echo features on or off. |
| `/echohelp` | `/eh` | List all Echo commands. |

**Player query formats** — display name (partial match), Steam ID suffix (digits only), Steam persona name, or `_host`.

## Configuration

Located in `BepInEx/config/com.andrewlin.ontogether.echo.cfg`

| Section | Key | Default | Description |
|---|---|---|---|
| `General` | `EnableFeature` | `true` | Master switch for the mod |
| `Security` | `AccessToken` | *(placeholder)* | SHA-256-hashed token required to use `/echocopyname` |

## Installation

Use r2modman or the Thunderstore app for the simplest install.

**Manual:**

| Step | Action |
|---|---|
| 1 | Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) into your On Together game folder |
| 2 | Copy `AndrewLin.Echo.dll` into `BepInEx/plugins/` |
| 3 | Launch the game — config files will be generated automatically |

**Dependencies:** [BepInExPack](https://thunderstore.io/c/on-together/p/BepInEx/BepInExPack/), [Alpha](https://thunderstore.io/c/on-together/p/AndrewLin/Alpha/)
