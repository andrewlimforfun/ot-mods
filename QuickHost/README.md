# QuickHost

Auto-create a lobby with configured name and social tags on game launch. Designed for server hosts who restart frequently.

## In-Game Commands

| Command | Short | Description |
|---------|-------|-------------|
| `/quickhosttoggle` | `/qht` | Toggle auto-lobby on/off for next launch |
| `/quickhoststatus` | `/qhs` | Show current QuickHost configuration |
| `/quickhostrestart` | `/qhr` | Launch restart script and quit the game |
| `/quickhosthelp` | `/qhh` | List all QuickHost commands |

## Configuration

Located in `BepInEx/config/com.andrewlin.ontogether.quickhost.cfg`

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| `General` | `Enabled` | `true` | Master switch for auto-host |
| `General` | `DelaySeconds` | `2` | Seconds to wait before creating lobby |
| `Lobby` | `LobbyName` | *(empty)* | Lobby name. **Must be set** - auto-host won't trigger if empty. |
| `Lobby` | `MaxPlayers` | `17` | Max players (1-17) |
| `Lobby` | `Visibility` | `Public` | `Public` or `Private` |
| `Lobby` | `RequestToJoin` | `false` | Require join requests |
| `SocialTags` | `Focus` | `false` | Focus tag |
| `SocialTags` | `Mature` | `false` | Mature tag |
| `SocialTags` | `Chill` | `true` | Chill tag |
| `SocialTags` | `Break` | `false` | Break tag |
| `SocialTags` | `Modded` | `true` | Modded tag |
| `Maintenance` | `RestartScriptPath` | *(empty)* | Path to restart `.ps1` script for `/qhr` |

## First Run

On first install, `LobbyName` is empty so auto-host will **not** trigger. Configure the lobby name in the config file, then re-launch.

## Restart Script

The `/qhr` command launches an external PowerShell script and quits the game. The script should wait briefly then re-launch through Steam:

```powershell
# restart-ontogether.ps1
param([int]$DelaySeconds = 3)
Start-Sleep -Seconds $DelaySeconds
Start-Process "steam://rungameid/3707400"
```

Set `RestartScriptPath` in the config to point at this script.

## Installation

Install via [Thunderstore](https://thunderstore.io/c/on-together/) or r2modman.

Requires: [Alpha](https://thunderstore.io/c/on-together/p/AndrewLin/Alpha/)
