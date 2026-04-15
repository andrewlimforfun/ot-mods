# Changelog

All notable changes to this project will be documented in this file.

## [1.1.10] - 2026-04-12

### Changed

- Dependency update to Alpha 0.0.11

## [1.1.9] - 2026-04-12

### Changed

- Depends on Alpha 0.0.10

## [1.1.8] - 2026-04-11

### Changed

- Depends on Alpha 0.0.8

## [1.0.0] - 2026-03-18

### Changed
- Depend on `AndrewLin-Alpha 1.0.0` mod
- Rename `BroadcastCreation` to `ChatConfirmation`
- Interpret a specific clock time e.g. `01:00` that was in the past for tomorrow (like alarm clock)

## [0.0.5] - 2026-03-14

### Changed
- HOTFIX: some names are too darn long, so unformat for now
- No longer Jaide ping compliant temporarily

## [0.0.4] - 2026-03-14

### Added
- `/remindbroadcastcreation` `/rbc` to set if initial creation broadcast needs to happen

### Changed
- Jaide ping compliant
- Fix remind at timezone


## [0.0.3] - 2026-03-13

### Changed
- Remove trailing fraction seconds

## [0.0.2] - 2026-03-13

### Changed
- Reminder chats now only use duration to be timezone invariant

## [0.0.1] - 2026-03-12

### Added
- `/remindmein` (`/rmi`) — show a private notification after a duration (`hh:mm:ss`)
- `/remindmeat` (`/rma`) — show a private notification at a local time (`HH:mm`)
- `/remindlocalin` (`/rli`) — send a local chat message after a duration
- `/remindlocalat` (`/rla`) — send a local chat message at a local time
- `/remindglobalin` (`/rgi`) — send a global chat message after a duration
- `/remindglobalat` (`/rga`) — send a global chat message at a local time
- `/remindhelp` (`/rh`) — list all available commands
- `/remindtoggle` (`/rt`) — toggle the mod on/off
- `/remindshowcommand` (`/rsc`) — toggle showing commands in chat
- `ScheduledTaskManager` — wall-clock based one-shot task scheduler with `ScheduleIn(TimeSpan)`, `ScheduleAt(DateTime)`, `TryScheduleIn(string, ...)`, and `TryScheduleAt(string, ...)` overloads
