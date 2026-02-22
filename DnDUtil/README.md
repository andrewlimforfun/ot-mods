# DnDUtil

A mod for On-Together that adds Dungeons & Dragons utilities to enhance your tabletop gaming experience.

## Features

- **Dice Rolling**: Roll any combination of dice (d4, d6, d8, d10, d12, d20, d100, etc.)
- **Flexible Broadcasting**: Send results to yourself, local chat, or global chat
- **Customizable Announcer**: Configure the name displayed when broadcasting rolls
- **In-Game Commands**: Easy-to-use slash commands for all features
- **Toggleable Features**: Enable/disable mod features as needed

## Commands

> **Note:** Most commands have short aliases for faster typing (shown in parentheses).

### Core Commands

- **`/dndhelp`** (or `/dh`) - Display a list of all available DnD mod commands in-game

- **`/roll XdY [MdN ...]`** (or `/r XdY`) - Roll dice with specified sides
  - `X` = number of dice to roll
  - `Y` = number of sides on each die
  - You can roll multiple dice types in one command
  - **Examples:**
    - `/roll 1d20` - Roll one 20-sided die
    - `/roll 2d6` - Roll two 6-sided dice
    - `/roll 2d20 1d6 1d4` - Roll two d20s, one d6, and one d4
    - `/roll d20` - Roll a single d20 (X defaults to 1)

### Configuration Commands

- **`/dndsetannouncerarea [self|local|global]`** (or `/dsaa`) - Set where roll results are displayed
  - `self` - Only you see the results
  - `local` - Nearby players see the results (default)
  - `global` - All players see the results
  - **Example:** `/dndsetannouncerarea global` or `/dsaa global`

- **`/dndsetannouncername [name]`** (or `/dsan`) - Set the display name for roll announcements
  - Default: "DnDSystem"
  - **Example:** `/dndsetannouncername DungeonMaster` or `/dsan DM`

- **`/showdndcommand`** (or `/sdc`) - Toggle visibility of your commands in chat
  - When enabled, other players see your command (e.g., "/roll 2d20")
  - When disabled, only the results are shown
  - Default: hidden

- **`/dndtoggle`** (or `/dt`) - Enable or disable all DnD utility features

## Installation

### Automatic Installation (Recommended)

1. Install [r2modman](https://thunderstore.io/package/ebkr/r2modman/) or Thunderstore Mod Manager
2. Look for On-Together game
3. Search for "DnDUtil" in the mod manager
4. Click "Install"
5. Launch the game through the mod manager

### Manual Installation

1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) for On-Together:
   - Download the appropriate BepInEx version for your platform
   - Extract BepInEx to your game's installation directory
   - Run the game once to generate BepInEx configuration files

2. Download DnDUtil:
   - Get the latest release from GitHub or Thunderstore

3. Install DnDUtil:
   - Extract the DnDUtil archive
   - Copy the contents to your game's installation directory
   - Merge folders when prompted

## Configuration

Configuration files are automatically created in `BepInEx/config/` after first launch.

**`com.andrewlin.ontogether.dndmod.cfg`:**
- `EnableFeature` - Enable/disable the mod (default: true)
- `ShowCommand` - Show commands in chat (default: false)
- `AnnouncerChatName` - Display name for announcements (default: "DnDSystem")
- `AnnouncerArea` - Where to broadcast rolls: self|local|global (default: "self")

## Usage Examples

### Basic Rolling
```
/roll 1d20          -> Roll for initiative!
/r 2d6              -> Roll damage for a greatsword (short command)
/roll 4d6           -> Roll stats for character creation
```

### Multiple Dice
```
/roll 1d20 8d6      -> Roll attack with spell damage
/r 1d20 1d4         -> Roll with bardic inspiration (short command)
```

### Broadcasting to Others
```
/dsaa global    -> Set to global chat
/roll 1d20      -> Everyone sees your roll
/dsaa self      -> Back to private rolling
```

### Custom Announcer Name
```
/dsan DM     -> Set name to "DM"
/roll 1d20   -> "DM rolled 1d20: 15"
```

## Support

For bug reports, feature requests, or questions:
- Open an issue on GitHub
- Contact the mod author on the On-Together community Discord
