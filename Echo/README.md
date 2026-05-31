# Echo

A BepInEx mod for [On Together](https://store.steampowered.com/app/2688490/On_Together/) that adds teleportation, name copying, and outfit copying utilities.

> **Note:** Echo can cause chaos. Install with care.

## In-Game Commands

Type any command into the in-game chat. Commands start with `/` and are **not sent** to other players.

| Command | Short | Description |
|---|---|---|
| `/echoteleportperson <player> [x y z]` | `/etp` | Teleport to a player. Optional XYZ offset relative to their position. |
| `/echoteleportlocation <name>` | `/etl` | Teleport to a saved location by name, or save your current position under that name if it doesn't exist yet. |
| `/echoteleportlocation <x> <y> <z>` | `/etl` | Teleport directly to the given world coordinates. |
| `/echofollow <player> [x y z]` | `/ef` | Follow a player at a fixed world-space offset. Uses current distance if no offset given. Sit at a desk first to prevent falling. |
| `/echofollowrelative <player> [behind\|beside\|front\|x y z]` | `/efr` | Follow a player relative to their facing direction. Offset rotates with the target. Presets: `behind`, `beside`, `right`, `front`. |
| `/echounfollow` | `/eu` | Stop following the current target. Also stops on teleport or target disconnect. |
| `/echosyncrotation <player>` | `/esr` | Mirror another player's facing direction each frame. Works alongside `/ef`. |
| `/echounsyncrotation` | `/eur` | Stop syncing rotation. |
| `/echorotate [degrees\|pitch yaw roll\|cardinal\|player]` | `/er` | Set facing direction (one-shot). Accepts yaw degrees, full `pitch yaw roll`, cardinal (`n/s/e/w/ne/se/sw/nw`), or player name. No args prints current rotation. |
| `/echorotatelock [degrees\|pitch yaw roll\|cardinal\|player]` | `/erl` | Lock rotation every frame so movement can't override it. Same argument formats as `/er`. |
| `/echolookat <player>` | `/ela` | Continuously face toward a player each frame. Pairs with `/efr` for face-to-face following. |
| `/echorotateunlock` | `/eru` | Stop all rotation overrides (lock, look-at). |
| `/echocopyname <player>` | `/ecn` | Copy another player's display name (including TMP colour/style tags). Token-gated - see `Security > AccessToken` in config. |
| `/echocopynamestyle [-s] <player>` | `/ecns` | Copy another player's TMP tag styling onto your own name's characters. `-s` also copies non-ASCII characters as part of the style. Token-gated. |
| `/echosetname <new name>` | `/esn` | Change your display name text while preserving the current TMP tag styling. |
| `/echorevertname` | `/ern` | Revert your display name to what it was before any Echo rename. |
| `/echocopyoutfit <player>` | `/eco` | Copy another player's full outfit and appearance onto yourself. |
| `/echoremovestatuscommand <status_brakets>` | `/ers` | Remove status from copied name. |
| `/echotoggle` | `/et` | Toggle all Echo features on or off. |
| `/echohelp` | `/eh` | List all Echo commands. |

**Player query formats** - display name (partial match), Steam ID suffix (digits only), Steam persona name, or `_host`.

## Configuration

Located in `BepInEx/config/com.andrewlin.ontogether.echo.cfg`

| Section | Key | Default | Description |
|---|---|---|---|
| `General` | `EnableFeature` | `true` | Master switch for the mod |
| `Security` | `AccessToken` | *(placeholder)* | SHA-256-hashed token required to use `/echocopyname` and `/echocopynamestyle` |

Saved locations are stored separately in `BepInEx/config/Echo_locations.cfg` (plain-text, one `name=x|y|z` entry per line).

## Installation

Use r2modman or the Thunderstore app for the simplest install.

**Manual:**

| Step | Action |
|---|---|
| 1 | Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) into your On Together game folder |
| 2 | Copy `AndrewLin.Echo.dll` into `BepInEx/plugins/` |
| 3 | Launch the game - config files will be generated automatically |

**Dependencies:** [BepInExPack](https://thunderstore.io/c/on-together/p/BepInEx/BepInExPack/), [Alpha](https://thunderstore.io/c/on-together/p/AndrewLin/Alpha/)
