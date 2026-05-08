# Changelog

All notable changes to this project will be documented in this file.

## [0.2.1] - 2026-05-08

### Added

- **`/echosyncrotation`** (`/esr`) - Mirror another player's facing direction each frame. Works standalone or combined with `/echofollow`.
- **`/echounsyncrotation`** (`/eur`) - Stop syncing rotation.

## [0.2.0] - 2026-05-08

### Fixed

- **`/echocopyoutfit`** — fixed for game v1.1.0. The game restructured `CustomizationData` from individual fields to dictionaries and added new outfit types (`Bed`, `Floater`); now compiling against the new DLLs and persisting `CurrentCustomizationDataIDs` using `CustomizationDataIDs3`.

## [0.1.9] - 2026-04-28

### Added

- **`/echofollow`** (`/ef`) - Follow a player at a fixed offset. Usage: `/ef <player> [x y z]`. If no offset is given, uses your current distance from the target. Player should be focused (sitting) to prevent falling.
- **`/echounfollow`** (`/eu`) - Stop following the current target. Following also stops automatically on teleport or if the target leaves.

## [0.1.8] - 2026-04-15

### Added

- **`/echolocationlist`** (`/ell`) — lists all saved locations and their coordinates, or reports "No saved locations." if none are saved.
- **`/echolocationremove`** (`/elr`) — removes a saved location by name (case-insensitive). Confirms removal or reports if the name was not found.

## [0.1.7] - 2026-04-12

### Changed

- Dependency update to Alpha 0.0.11

## [0.1.6] - 2026-04-12

### Changed

- Temporarily not using the PlayerList blacklist/admin feature from Alpha pending further review.

## [0.1.5] - 2026-04-12

### Changed

- Depends on Alpha 0.0.10

## [0.1.4] - 2026-04-11

### Added

- **`/echoteleportlocation`** (`/etl`) — save named locations and teleport to them by name, or teleport directly to world coordinates (`/etl <x> <y> <z>`). Locations are persisted to `BepInEx/config/Echo_locations.cfg` across sessions.

### Changed

- Depends on Alpha 0.0.8

## [0.1.2] - 2026-03-29

### Changed

- Fix copy outfit not being broadcasted by updating datamanager first

## [0.1.1] - 2026-03-29

### Added

- `/echoremovestatuscommand <status_brakets>` `/ers` remove status from copied name

## [0.1.0] - 2026-03-29

### Added

- **Teleportation** — `/echotpto <player> [x y z]` warps you to any connected player, with an optional XYZ offset. Implemented via a `PlayerMovementController.MovePlayer` Harmony prefix so the warp survives the movement controller's per-frame override.
- **Name copying** — `/echocopyname <player>` copies another player's full display name (including TMP rich-text tags). Token-gated via SHA-256 `AccessToken` config entry. Saves your original name for revert.
- **Name reverting** — `/echorevertname` restores your display name to what it was before any Echo rename.
- **Outfit copying** — `/echocopyoutfit <player>` copies another player's full appearance (all 18 customization fields) onto your own character.
- **Toggle** — `/echotoggle` enables or disables all Echo features at runtime.
- OfficerBalls compatibility: when the `officerballs.StatusManager` plugin is present, `/setname` is also sent via the UI so that the OfficerBalls player-list stays in sync.
