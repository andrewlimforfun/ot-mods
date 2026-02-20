# Changelog

All notable changes to DnDUtil will be documented in this file.

## [0.0.6] - 2026-02-20

### Changed
- Renamed `/usedndfeature` (`/udf`) to `/dndtoggle` (`/dt`)
- Renamed `/setannouncerarea` (`/saa`) to `/dndsetannouncerarea` (`/dsaa`)
- Renamed `/setannouncername` (`/san`) to `/dndsetannouncername` (`/dsan`)
- Internal refactor: extracted `DndRollCommand` base class; command files renamed with `Dnd` prefix

## [0.0.5] - 2026-02-16

### Fixed
- Help text for OfficerBalls mods are no longer 'eaten'

## [0.0.4] - 2026-02-15

### Added
- Short command aliases for all commands (e.g., `/r` for `/roll`, `/h` for `/help`)
- Short names displayed in help command output

### Changed
- Improved help command text and formatting
- Commands no longer interfere with or clean other game commands

### Fixed
- Help text display issues

## [0.0.3] - 2026-02-15

### Changed
- Improved documentation with comprehensive usage examples
- Documentation cleanup and restructuring

## [0.0.2] - 2026-02-15

### Added
- Announcer area configuration (self/local/global broadcast)
- Commands to set announcer area and name
- Message scoping by selected area

### Changed
- Announcements now properly identified by area scope

## [0.0.1] - 2026-02-15

### Added
- Initial release
- Basic dice rolling functionality (`/roll XdY`)
- Multiple dice rolling in single command
- BepInEx plugin integration
- Chat notification system
