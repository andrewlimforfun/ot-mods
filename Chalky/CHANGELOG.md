# Changelog

All notable changes to this project will be documented in this file.

## [1.2.9] - 2026-06-15

### Fixed

- Adapt to game update 1.1.3 - replaced fragile Traverse reflection calls (PaintPixel/RenderBatch) in RebuildRenderTexture with the game's own GetQuadImage_Original_1 method
- BroadcastToAllPlayers now skips self to avoid duplicate local repaint

## [1.2.8] - 2026-04-27

### Changed

- Host relay notification and log now include the board's network ID

## [1.2.7] - 2026-04-12

### Changed

- Dependency update to Alpha 0.0.11

## [1.2.6] - 2026-04-12

### Changed

- Depends on Alpha 0.0.10

## [1.2.5] - 2026-04-11

### Changed

- Depends on Alpha 0.0.8

## [1.2.4] - 2026-04-01

### Added
- Somehow 1.2.3 got removed from thunderstore

## [1.2.3] - 2026-04-01

### Added
- `EnableHostRelay` config entry (default `true`) - controls whether the host relays board state to other players
- `/chalkyhostrelay` (`/chr`) - toggle host relay on/off at runtime

### Removed
- Spoofing host feature and `/chalkyspoofhost` (`/csh`) command

## [1.2.0] - 2026-03-28

### Added
- `/chalkyspoofhost` (`/csh`) - toggle spoof host on/off at runtime (hidden from help listing)

## [1.1.0] - 2026-03-27

### Added
- Spoofing host feature (experimental)

### Changed
- Fixed occassional host not found issue

## [1.0.2] - 2026-03-25

### Added
- Non-host clients can now load boards and broadcast to all players via host relay
  - Board state is sent to the host, which relays to all other connected players
  - Requires the host to also have Chalky installed

### Changed
- `/chalkyloadboard` no longer requires being the session host
- Updated user-facing messages for non-host board loading

## [1.0.0] - 2026-03-20

### Removed
- `/chalkyshowcommand` (`/cshc`) - use the `ShowCommand` config entry instead

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
