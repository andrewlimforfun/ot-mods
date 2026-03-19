# Chalky

A BepInEx mod for [On Together](https://store.steampowered.com/app/2688490/On_Together/) that enhances the chalkboard drawing experience with configurable brush sizes and in-game chat commands.


## Features

- **Configurable chalk brush size** - Paint a larger area on the chalkboard in a single stroke. The default game brush covers a 2×2 grid cell block; Chalky lets you scale this up to any integer size.
- **Chalkboard save & load** - Save any board's full drawing state to a local file and restore it later. Saved files are portable JSON and include an optional PNG preview.
- **Broadcast on load** - When the host loads a board, all currently connected players receive the restored drawing instantly. Late-joiners get it automatically through the game's existing sync flow — no mod required on their end.
- **In-game chat commands** - Control all mod behavior on the fly without leaving the game. Commands are typed into the chat box and are **never sent** to other players.
- **Toggle on/off at runtime** - Enable or disable the mod's drawing enhancements instantly without restarting.
- **Show/hide command echo** - Optionally suppress commands from appearing in your chat history.


## Warning

- Chalk size can only be enlarged. Smaller than the default 2×2 is not supported.
- Loading a board requires you to be the **session host**. Saving works for everyone.

## In-Game Commands

Type any command into the in-game chat. Commands start with `/` and are **not sent** to other players. Each command has a short alias for convenience.

- `/chalkyhelp` (`/ch`) - List all available Chalky commands
- `/chalkytoggle` (`/ct`) - Toggle the mod on or off
- `/chalkysetsize [size]` (`/css [size]`) - Set the chalk brush size. Omit to reset to default (2)
- `/chalkyshowcommand` (`/csc`) - Toggle whether your commands are echoed in chat
- `/chalkysaveboard [name] [index]` (`/csb [name] [index]`) - Save a board to disk. Omit `[index]` to use the active board
- `/chalkyloadboard [name] [index]` (`/clb [name] [index]`) - Load a board from disk and sync to all players (host only)

> **Note:** Size changes are not persistent and reset to the default of `2` on game restart.

### Examples

```
# Set brush to paint a 5x5 block per stroke
/css 5

# Reset brush size back to default (2x2)
/css

# Save the currently active board as "lobby_art"
/csb lobby_art

# Save board index 2 specifically as "backup"
/csb backup 2

# Load "lobby_art" onto the currently active board (host only)
/clb lobby_art

# Load "backup" onto board index 0 (host only)
/clb backup 0

# Turn off Chalky's drawing enhancements entirely
/ct

# Check which commands are available
/ch
```

## Configuration

Settings are saved to the BepInEx config file and can also be changed at runtime via chat commands.

**Location:** `BepInEx/config/com.andrewlin.ontogether.chalky.cfg`

- `General`
  - `EnableFeature` (default: `true`) - Enable or disable all mod features
  - `ShowCommand` (default: `false`) - Show commands in chat when typed
- `Boards`
  - `BoardSaveDirectory` (default: `~/on-together/chalkboard`) - Directory where board saves are stored. `~` expands to your home folder

Saved boards are stored as `<name>.chalkboard.json` (full drawing data) and `<name>.png` (preview image) in the configured directory.

## Installation

Use `r2modman` for simpler installation.

1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) into your On Together game folder
2. Copy `AndrewLin.Chalky.dll` into `BepInEx/plugins/`
3. Launch the game - the mod will load automatically and generate a config file at `BepInEx/config/com.andrewlin.ontogether.chalky.cfg`

## Building from Source

Requires .NET SDK and the game's managed DLLs referenced in the project.

```
dotnet build
```

Output DLL will be in `Chalky/bin/`.
