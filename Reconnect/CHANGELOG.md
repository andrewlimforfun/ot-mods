# Changelog

## 0.0.4

### Added

- Restore player position and rotation after successful reconnect
- Capture focus state at disconnect time and notify player to re-enter manually

## 0.0.3

### Fixed

- **`/reconnecttest`** - renamed to `/reconnectsimulate` (`/rcs`) to avoid shortname collision with `/reconnecttoggle` (`/rct`).

## 0.0.2

- Better default values
- Update configs in-game

## 0.0.1

- Initial release
- Auto-reconnect on unexpected disconnection (client-only)
- Configurable max attempts, interval, and cooldown
- Intentional leave detection via QuitSession/ButtonQuit patches
