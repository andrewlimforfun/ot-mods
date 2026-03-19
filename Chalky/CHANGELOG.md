# Changelog

All notable changes to this project will be documented in this file.

## [0.0.4] - 2026-02-22

### Removed
- `/help chalky` (`/h chalky`) - use `/chalkyhelp` (`/ch`) instead

## [0.0.3] - 2026-02-21

### Added
- Any players can now save chalkboard state as JSON to their local pc with a name.
- Only hosts are allowed to load chalkboard from a JSON.
- Commands
  - `/chalkygetboards` (`cgb`) to list all saved boards
  - `/chalkysaveboard` (`csb`) to save a specific/active board
  - `/chalkyloadboard` (`clb`) to load a board

## [0.0.2] - 2026-02-20

### Changed
- Changes are now non-local, observers can see it too

### Removed
- Disabled chalk size 1, smaller than 2x2 causes desync
- Disabled chalk colors, colors causes desync.

## [0.0.1] - 2026-02-20

### Added
- Configurable chalk brush size - paints a `size x size` grid block per stroke instead of the default 2×2
  - Size 1 paints a single grid cell
  - Size 2 (default) falls through to the original game logic unchanged
  - Sizes > 2 use the expanded custom painting path
- Erase mode always uses original game logic regardless of configured size
- In-game chat command system - commands starting with `/` are intercepted and never sent to other players
- `/chalkysetsize [size]` (`/css`) - set brush size at runtime; omit argument to reset to default
- `/chalkytoggle` (`/ct`) - toggle all mod features on/off at runtime
- `/chalkyshowcommand` (`/fsc`) - toggle whether typed commands are echoed in the chat history
- `/chalkyhelp` (`/fh`) - list all available commands in chat
- `/help chalky` (`/h chalky`) - same as above via the shared help command
- `EnableFeature` config entry (default: `true`) - persisted toggle for mod features
- `ShowCommand` config entry (default: `false`) - persisted toggle for command echo
