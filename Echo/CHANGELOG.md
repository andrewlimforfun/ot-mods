# Changelog

All notable changes to this project will be documented in this file.

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
