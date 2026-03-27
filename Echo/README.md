# Echo

A BepInEx mod for [On Together](https://store.steampowered.com/app/2688490/On_Together/).
## In-Game Commands

Type any command into the in-game chat. Commands start with `/` and are **not sent** to other players.

| Command | Short | Description |
|---------|-------|-------------|
| `/echotoggle` | `/ht` | Toggle Echo on/off |
| `/echocopyoutfit <slot>` | `/eco <slot>` | Copy current outfit/appearance to another slot (1–3) |
| `/echohelp` | `/eh` | List all Echo commands |

## Configuration

Located in `BepInEx/config/com.andrewlin.ontogether.echo.cfg`

| Key | Default | Description |
|---|---|---|
| `General > EnableFeature` | `true` | Master switch for the mod |
| `General > ShowCommand` | `false` | Show typed commands in chat |

## Installation

Use r2modman or the Thunderstore app for the simplest install.

**Manual:**

| Step | Action |
|---|---|
| 1 | Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) into your On Together game folder |
| 2 | Copy `AndrewLin.Echo.dll` into `BepInEx/plugins/` |
| 3 | Launch the game -- config files will be generated automatically |

**Dependencies:** [BepInExPack](https://thunderstore.io/c/on-together/p/BepInEx/BepInExPack/), [Alpha](https://thunderstore.io/c/on-together/p/AndrewLin/Alpha/)

## Host-only vs. all-clients

Installing on the host is sufficient to enforce filtering for everyone in the session. Clients who also have the mod installed get an additional local filter pass that covers their own messages before they are sent.
